using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;
using Sirenix.OdinInspector;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames.Template;

public class PBStageObserver : MonoBehaviour
{
    [SerializeField] PvPArenaVariable currentArenaVar;
    [SerializeField] CinemachineVirtualCamera virtualCam, horizontalVirtualCam;
    [SerializeField] Transform lookTarget;
    [OnValueChanged("UpdateCamPos"), SerializeField] float sphereRadius;
    [OnValueChanged("UpdateCamPos"), SerializeField] float azimuth;
    [OnValueChanged("UpdateCamPos"), SerializeField] float elevation;
    [SerializeField] AnimationCurve moveCurve;
    [SerializeField] float animationDuration;
    [SerializeField] float moveMultiplier = 0.1f;
    [SerializeField] float topDownMoveSpeed = 0.5f;
    [SerializeField] Vector2 fovRange;
    [SerializeField] bool isPlayAnimInStart;

    private Animator animator;
    private CinemachineBrain cinemachineBrain;
    private float originalBlendTime;
    private bool _isRunningViewStage = false;

    [SerializeField] private Variable<Mode> m_CurrentMode;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnEndSearchingArena, EndSearching);
        animator = GetComponent<Animator>();
        cinemachineBrain = ObjectFindCache<CinemachineBrain>.Get();
    }
    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnEndSearchingArena, EndSearching);
    }

    private void Start()
    {
        // if (isPlayAnimInStart)
        // {
        //     EndSearching();
        // }
        EndSearching();
    }

    void UpdateCamPos()
    {
        _isRunningViewStage = true;
        var sphereCenter = transform.position;
        float theta = Mathf.Deg2Rad * azimuth;
        float phi = Mathf.Deg2Rad * elevation;
        Vector3 direction = new Vector3(
            sphereRadius * Mathf.Sin(theta) * Mathf.Cos(phi),
            sphereRadius * Mathf.Sin(phi),
            sphereRadius * Mathf.Cos(theta) * Mathf.Cos(phi)
        );

        // Position object at calculated point
        virtualCam.transform.position = sphereCenter + direction;

        virtualCam.transform.LookAt(lookTarget);
    }

    private void EndSearching()
    {
        // SoundManager.Instance.PlaySFX(PBSFX.UIFindOpponent);
        UpdateCamPos();
        animator.SetInteger("Arena", currentArenaVar.value.index + 1);
        animator.SetTrigger("Play");
        // StartCoroutine(CR_PlayCamAnim());
    }

    IEnumerator CR_PlayCamAnim()
    {
        var t = 0f;
        var orginalPos = lookTarget.position;
        while (t <= 1)
        {
            t += Time.deltaTime / animationDuration;
            // float value = Mathf.PingPong(t * moveMultiplier + 0.5f, 1.0f);
            // lookTarget.position = orginalPos + Vector3.forward * moveCurve.Evaluate(value);
            // virtualCam.transform.LookAt(lookTarget);
            virtualCam.m_Lens.FieldOfView = Mathf.Lerp(fovRange.x, fovRange.y, t);
            virtualCam.transform.position += virtualCam.transform.forward * topDownMoveSpeed * Time.deltaTime;
            yield return null;
        }
        _isRunningViewStage = false;
        //GameEventHandler.Invoke(PBPvPEventCode.OnCompleteCamRotation);
    }

    public void EndAnimation()
    {
        if(m_CurrentMode == null)
        {
            GameEventHandler.Invoke(PBPvPEventCode.OnCompleteCamRotation);
            return;
        }
        GameEventHandler.Invoke(CharacterRoomEvent.OnStartMatch);
        GameEventHandler.Invoke(PBPvPEventCode.OnCompleteCamRotation);
    }

    public void SetBlendTimeToZero()
    {
        originalBlendTime = cinemachineBrain.m_DefaultBlend.m_Time;
        cinemachineBrain.m_DefaultBlend.m_Time = 0;
    }

    public void RevertBlendTime()
    {
        cinemachineBrain.m_DefaultBlend.m_Time = originalBlendTime;
    }

    private void OnApplicationPause(bool pause)
    {
        if (_isRunningViewStage)
        {
            _isRunningViewStage = false;

            string name = "";
            GameObject pbFightingStage = PBFightingStage.Instance.gameObject;
            if (pbFightingStage != null)
            {
                name = pbFightingStage.name.Replace("(Clone)", "");
            }

            string stageName = name;
            string mainPhase = "Matchmaking";
            string subPhase = "ReviewStage";
            GameEventHandler.Invoke(DesignEvent.QuitCheck, stageName, mainPhase, subPhase);
        }
    }
}
