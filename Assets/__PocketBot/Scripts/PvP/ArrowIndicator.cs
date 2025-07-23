using System.Collections;
using DG.Tweening;
using HyrphusQ.Events;
using UnityEngine;

public class ArrowIndicator : MonoBehaviour
{
    [SerializeField] SpriteRenderer arrow;
    public Transform followObject;
    public Transform target;
    public bool hasInit;


    [SerializeField] private float _distanceShowArrow = 30;
    private CarPhysics _carPhysicsTarget;
    private bool _onFadeArrow = false;
    private IEnumerator FadeArrowCR;
    private Camera _mainCamera;
    private void Awake()
    {
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotVisibleOnScreen, HandleOnRobotVisibleOnScreen);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnRoundCompleted, HandleOnRoundCompleted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnAnyPlayerDied, HandleOnEnemyDied);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnAnyPlayerDied, HandleOnPlayerDied);

    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotVisibleOnScreen, HandleOnRobotVisibleOnScreen);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnRoundCompleted, HandleOnRoundCompleted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnAnyPlayerDied, HandleOnEnemyDied);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnAnyPlayerDied, HandleOnPlayerDied);
    }

    private void Start()
    {
        arrow.DOFade(1, 2f);

        if (FadeArrowCR != null)
            StopCoroutine(FadeArrowCR);
        FadeArrowCR = HandleFadeArrowCR();
        StartCoroutine(FadeArrowCR);
    }
    public void SetCamera(Camera camera) => _mainCamera = camera;
    private void LateUpdate()
    {
        if(followObject != null)
        {
            FollowObjec();
        }
        if(target != null)
        {
            RotateToObject();
        }
        if(hasInit)
        {
            if(target == null)
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator HandleFadeArrowCR()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);
        while (true)
        {
            HandleFadeArrow();
            yield return waitForSeconds;
        }
    }
    private void HandleFadeArrow()
    {
        if (_mainCamera == null) return;

        if (target != null)
        {
            _carPhysicsTarget = target.GetComponentInParent<CarPhysics>();
            if (_carPhysicsTarget == null) return;

            Vector3 objectPosition = _carPhysicsTarget.transform.position;
            Vector3 viewportPoint = _mainCamera.WorldToViewportPoint(objectPosition);

            if (viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1)
            {
                arrow.DOFade(0, 1);
                _onFadeArrow = true;
            }

            if(viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1)
            {
                arrow.DOFade(1, 1);
                _onFadeArrow = false;
            }

            bool targetIsDead = !_carPhysicsTarget.isActiveAndEnabled;
            if (targetIsDead)
            {
                arrow.enabled = false;
                if (FadeArrowCR != null)
                    StopCoroutine(FadeArrowCR);
            }

        }
    }
    void FollowObjec()
    {
        Vector3 followPosition = new Vector3(followObject.position.x, followObject.position.y, followObject.position.z);
        transform.position = Vector3.MoveTowards(transform.position, followPosition, 3);
    }
    
    void RotateToObject()
    {
        var lookPos = target.position - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
    }

    void HandleOnRobotVisibleOnScreen(params object[] parameters)
    {
        //bool isOnScreen = (bool)parameters[0];
        //int targetObjectID = (int)parameters[1];
        //if (target == null) return;
        //var carPhysics = target.GetComponentInParent<CarPhysics>();
        //if (carPhysics == null) return;
        //bool targetIsDead = !carPhysics.isActiveAndEnabled;
        //if (target.gameObject.GetInstanceID().Equals(targetObjectID))
        //{
        //    arrow.DOFade(!isOnScreen && targetIsDead || isOnScreen && !targetIsDead ? 0 : 1 , 0.25f);
        //}
    }

    void HandleOnRoundCompleted(params object[] parameters)
    {
        arrow.DOFade(0, 0.25f).OnComplete(()=> arrow.enabled = false);

        if (FadeArrowCR != null)
            StopCoroutine(FadeArrowCR);
    }

    void HandleOnEnemyDied(params object[] parameters)
    {
        PBRobot pbRobot = (PBRobot)parameters[0];
        NotifyOnVisible notifyOnVisible = pbRobot.GetComponentInChildren<NotifyOnVisible>();

        if (notifyOnVisible == null) return;
        if (target.gameObject.GetInstanceID().Equals(notifyOnVisible.gameObject.GetInstanceID()))
        {
            arrow.DOFade(0, 0.25f).OnComplete(() => arrow.enabled = false);
        }
    }

    void HandleOnPlayerDied(params object[] parameters)
    {
        PBRobot pbRobot = (PBRobot)parameters[0];
        if(pbRobot.PersonalInfo.isLocal)
        {
            arrow.DOFade(0, 0.25f).OnComplete(() => arrow.enabled = false);
        }
    }
}
