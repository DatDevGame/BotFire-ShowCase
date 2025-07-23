using Cinemachine;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.GameManagement;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PSFTUEController : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] private float m_WidthLine;

    [SerializeField, BoxGroup("Ref")] private PBRobot m_PlayerRobot;
    [SerializeField, BoxGroup("Ref")] private PBRobot m_EnemyRobot;
    [SerializeField, BoxGroup("Ref")] private Transform m_CameraPath_1;
    [SerializeField, BoxGroup("Ref")] private Transform m_CameraPath_2;
    [SerializeField, BoxGroup("Ref")] private Transform m_StepPoint_1;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_PlayerControllerCanvasGroup;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_BubbleDragToMoveCanvasGroup;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_BubbleCombat1CanvasGroup;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_BubbleCombat2CanvasGroup;
    [SerializeField, BoxGroup("Ref")] private CinemachineVirtualCamera m_CameraPath;
    [SerializeField, BoxGroup("Ref")] private CinemachineVirtualCamera m_WinCamera;

    [SerializeField, BoxGroup("Resource")] private ParticleSystem m_EntranceLightCircleVFX;
    [SerializeField, BoxGroup("Resource")] private ParticleSystem m_WaitAppearVFX;
    [SerializeField, BoxGroup("Resource")] private ParticleSystem m_AppearBurstVF;

    [SerializeField, BoxGroup("Data")] private Material m_LineMaterialPrefab;
    [SerializeField, BoxGroup("Data")] private PSFTUESO m_PSFTUESO;

    private bool m_Step_1 = false;
    private bool m_Step_2 = false;

    private LineRenderer m_LineRenderer;
    private Transform m_LineObject;
    private Material m_LineMaterial;
    private Tween m_ScrollTween;

    private List<CircleLineRenderer> m_CircleLineRenderer;
    private void Start()
    {
        m_PSFTUESO.ActiveDefautWeapon();
        m_PlayerControllerCanvasGroup.Hide();
        m_CameraPath.transform.position = m_CameraPath_1.position;
        m_CameraPath.transform.eulerAngles = m_CameraPath_1.eulerAngles;
        m_CameraPath.transform
            .DOMove(m_CameraPath_2.position, 1f).SetEase(Ease.InCirc)
            .SetDelay(1f)
            .OnComplete(() =>
            {
                StartCoroutine(Setup());
                m_CameraPath.enabled = false;
            });
    }

    private void OnDestroy()
    {
        m_ScrollTween?.Kill();
    }

    private void Update()
    {
        HandleStep1();
        HandleStep2();
    }

    private void HandleStep1()
    {
        if (!m_Step_1 || m_Step_2 || m_LineRenderer == null) return;

        m_LineRenderer.SetPosition(0, m_LineObject.position);
        m_LineRenderer.SetPosition(1, m_StepPoint_1.position);

        float distanceToStep1 = Vector3.Distance(m_LineObject.position, m_StepPoint_1.position);
        if (distanceToStep1 > 1f) return;
        GameEventHandler.Invoke(PSLogFTUEEventCode.EndMovement);
        GameEventHandler.Invoke(PSLogFTUEEventCode.StartCombat);

        m_BubbleDragToMoveCanvasGroup.Hide();
        m_PlayerControllerCanvasGroup.Hide();

        m_Step_1 = false;
        m_EntranceLightCircleVFX.gameObject.SetActive(false);
        m_WaitAppearVFX.Play();
        m_PlayerRobot.ChassisInstance.CarPhysics.CanMove = false;
        m_CircleLineRenderer.ForEach(v => v.gameObject.SetActive(true));
        StartCoroutine(CommonCoroutine.Delay(1f, false, (System.Action)(() =>
        {
            m_PlayerControllerCanvasGroup.Show();

            m_BubbleCombat1CanvasGroup.Show();
            m_Step_2 = true;
            m_AppearBurstVF.Play();
            m_EnemyRobot.BuildRobot();

            var botController = m_EnemyRobot.GetComponent<BotController>();
            if (botController != null)
                botController.enabled = false;

            var aimController = m_EnemyRobot.GetComponentInChildren<AimController>();
            if (aimController != null)
                aimController.IsActive = false;

            m_EnemyRobot.MaxHealth = m_PSFTUESO.EnemyHealth;
            m_EnemyRobot.Health = m_PSFTUESO.EnemyHealth;
            m_EnemyRobot.OnHealthChanged += EnemyOnHealthChange;
            m_EnemyRobot.TeamId = 0;
            m_EnemyRobot.gameObject.SetActive(true);
            m_LineMaterial.color = Color.red;
            m_PlayerRobot.ChassisInstance.CarPhysics.CanMove = true;
        })));
    }

    private void HandleStep2()
    {
        if (!m_Step_2 || m_LineRenderer == null ||
            m_PlayerRobot?.ChassisInstance == null ||
            m_EnemyRobot?.ChassisInstance == null)
            return;

        Transform playerPos = m_LineObject;
        Transform enemyPos = m_EnemyRobot.ChassisInstance.CarPhysics.transform;

        m_LineRenderer.SetPosition(0, playerPos.position);
        m_LineRenderer.SetPosition(1, enemyPos.position);

        float distance = Vector3.Distance(playerPos.position, enemyPos.position);
        float width = distance > 15f ? m_WidthLine : 0f;
        m_LineRenderer.startWidth = width;
        m_LineRenderer.endWidth = width;
    }


    private IEnumerator Setup()
    {
        yield return null;
        m_PlayerRobot.BuildRobot();
        m_PlayerRobot.TeamId = 1;


        AimController aimController = m_PlayerRobot.GetComponentInChildren<AimController>();
        if (aimController != null)
        {
            m_WinCamera.transform.SetParent(aimController.transform);
            m_WinCamera.transform.localPosition = new Vector3(0, 4.5f, 7.5f);
            m_WinCamera.transform.localEulerAngles = new Vector3(38, 180, 0);

            if (m_CircleLineRenderer == null)
            {
                m_CircleLineRenderer = aimController.CircleLineRenderers;
                m_CircleLineRenderer.ForEach(v => v.gameObject.SetActive(false));
            }
        }

        GameEventHandler.Invoke(PSLogFTUEEventCode.StartMovement);

        m_PlayerControllerCanvasGroup.Hide();
        m_EntranceLightCircleVFX.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        m_BubbleDragToMoveCanvasGroup.Show();
        m_PlayerRobot.ChassisInstance.CarPhysics.CanMove = true;

        SetUpLineFTUE();
        yield return new WaitForSeconds(1);
        m_PlayerControllerCanvasGroup.Show();
    }

    private void SetUpLineFTUE()
    {
        GameObject lineObject = new GameObject();
        lineObject.transform.SetParent(m_PlayerRobot.ChassisInstance.CarPhysics.transform);
        lineObject.transform.localPosition = new Vector3(0, 0.1f, 0);
        lineObject.transform.eulerAngles = new Vector3(90, 0, 0);
        m_LineObject = lineObject.transform;
        m_LineRenderer = m_LineObject.AddComponent<LineRenderer>();

        m_LineMaterial = new Material(m_LineMaterialPrefab);
        Vector2 currentOffset = m_LineMaterial.mainTextureOffset;

        m_ScrollTween = DOTween.To(
            () => m_LineMaterial.mainTextureOffset.x,
            x => m_LineMaterial.mainTextureOffset = new Vector2(x, currentOffset.y), -999f, 300f).SetEase(Ease.Linear)
         .SetLoops(-1, LoopType.Incremental);


        if (m_LineRenderer != null)
        {
            m_LineRenderer.positionCount = 2;
            m_LineRenderer.startWidth = m_WidthLine;
            m_LineRenderer.endWidth = m_WidthLine;
            m_LineRenderer.material = m_LineMaterial;
            m_LineRenderer.alignment = LineAlignment.TransformZ;
            m_LineRenderer.textureMode = LineTextureMode.Static;
            m_Step_1 = true;
        }
    }

    private void EnemyOnHealthChange(Competitor.HealthChangedEventData data)
    {
        if (data.CurrentHealth <= 0)
        {
            GameEventHandler.Invoke(PSLogFTUEEventCode.EndCombat);

            m_BubbleCombat1CanvasGroup.Hide();
            m_BubbleCombat2CanvasGroup.Show();
            m_PlayerRobot.ChassisInstance.CarPhysics.CanMove = false;
            m_LineRenderer.enabled = false;
            m_PlayerControllerCanvasGroup.Hide();

            m_PSFTUESO.FTUEFightPPref.value = true;
            StartCoroutine(CommonCoroutine.Delay(4f, false, (System.Action)(() =>
            {
                IAsyncTask asyncTask = SceneManager.LoadSceneAsync(SceneName.MainScene);
                LoadingScreenUI.Load(asyncTask);
            })));
        }
    }
}
