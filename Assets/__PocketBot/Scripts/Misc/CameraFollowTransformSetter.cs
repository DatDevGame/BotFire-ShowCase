using System.Collections.Generic;
using Cinemachine;
using HyrphusQ.Events;
using UnityEngine;
using DG.Tweening;
using System.Collections;
using Sirenix.OdinInspector;
using System.Linq;
using System;
using UnityEngine.UI;
using Unity.Burst.Intrinsics;
using LatteGames;

public enum CameraViewEvent
{
    Switch
}

public class CameraFollowTransformSetter : MonoBehaviour
{
    [SerializeField, TitleGroup("Property")] float minZoom = -12;
    [SerializeField, TitleGroup("Property")] float maxZoom = -20;
    [SerializeField, TitleGroup("Property")] float zoomLitmiter = 50;
    [SerializeField, TitleGroup("Property")] float lerpSpeed = 1;
    [SerializeField, TitleGroup("Property")] private float _offsetViewCameraTopDown = 5;

    [SerializeField, TitleGroup("Cinemachine Virtual Camera")] private Transform _cameraMain;
    [SerializeField, TitleGroup("Cinemachine Virtual Camera")] private CinemachineVirtualCamera mainVCam;
    [SerializeField, TitleGroup("Cinemachine Virtual Camera")] private CinemachineVirtualCamera _topDownCinemachine;
    [SerializeField, TitleGroup("Cinemachine Virtual Camera")] private CinemachineVirtualCamera _firstPersonCinemachine;
    [SerializeField, TitleGroup("Cinemachine Virtual Camera")] private CinemachineVirtualCamera _rotateCinemachine;
    [SerializeField, TitleGroup("Cinemachine Virtual Camera")] private PPrefIntVariable _pprefModeCamera;
    [SerializeField, TitleGroup("Cinemachine Virtual Camera")] CinemachineVirtualCamera[] subVCams;

    #region FOV Effect
    [Space]
    [SerializeField, TitleGroup("Handle View Camera", alignment: TitleAlignments.Centered)]
    private bool _isUsingEffectFOV;
    [SerializeField, TitleGroup("Handle View Camera", alignment: TitleAlignments.Centered)]
    private float _distanceFovEffect = 15;
    [SerializeField, TitleGroup("Handle View Camera", alignment: TitleAlignments.Centered)]
    private AnimationCurve _animationCurveCameraView;
    [SerializeField, TitleGroup("Handle View Camera", alignment: TitleAlignments.Centered)]
    private AnimationCurve _animationCurveCameraTopDownView;
    [SerializeField, TitleGroup("Handle View Camera", alignment: TitleAlignments.Centered)]
    private AnimationCurve _animationCurvePointView;
    [SerializeField, TitleGroup("Handle View Camera", alignment: TitleAlignments.Centered), ReadOnly]
    private List<Transform> _opponents;
    [SerializeField, TitleGroup("Handle View Camera", alignment: TitleAlignments.Centered), ReadOnly]
    private Transform _player;
    [SerializeField, TitleGroup("Handle View Camera", alignment: TitleAlignments.Centered), ReadOnly]
    private Transform _opponentNearest;
    private Transform _opponentNearestOld;
    #endregion

    [SerializeField, TitleGroup("Other")] private AudioListener _audioListenerPlayer;
    [SerializeField, TitleGroup("Other")] private ModeVariable currentMode;


    [SerializeField, TitleGroup("Rotate Third Person")] private LayerMask _wallMask;
    [SerializeField, TitleGroup("Rotate Third Person")] private float _timeDelayExitRotateThirdPerson = 5;
    [SerializeField, TitleGroup("Rotate Third Person")] private float _distanceCheckWall = 3;
    [SerializeField, TitleGroup("Rotate Third Person")] private float _minDistanceRotate = 15;
    [SerializeField, TitleGroup("Rotate Third Person")] private float _coneAngle = 30;
    [SerializeField, TitleGroup("Rotate Third Person")] private Vector2 _rangeTimeConvertRotateView;

    private float _timeRotateView = 7;
    private bool _isRotateView = false;
    private Tweener _rightTweenRotate;
    private Tweener _leftTweenRotate;
    private Sequence _mySequenceRotateCam;
    private PBRobot _pBRobotPlayer;
    private List<PBRobot> followTargets = new();
    private CinemachineOrbitalTransposer _thirdCinemachineOrbitalTransposer;
    private CinemachineVirtualCamera _mainVcamViewUsing;
    [ShowInInspector] private Transform _pointTopDownView;
    private Transform _pointLootAtFirstPerson;
    private Transform _pointFollowFirstPerson;
    private Transform _pointRotateThirdPerson;
    private IEnumerator _checkDistanceNearestOpponentCR;
    private IEnumerator _fovHandleCameraUsingCR;
    private IEnumerator _runRightRotateThirdPersonCR;
    private IEnumerator _runLeftRotateThirdPersonCR;

    public bool IsAbleToModifyFoV { get; set; } = true;

    void Awake()
    {
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, HandleBotModelSpawned);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelDespawned, HandleBotModelDespawned);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.AddActionEvent(CompetitorStatusEventCode.OnCompetitorBeforeDied, HandleCompetitorBeforeDied);
        _pprefModeCamera.onValueChanged += PPrefModeCameraOnValueChanged;
    }

    void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, HandleBotModelSpawned);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelDespawned, HandleBotModelDespawned);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.RemoveActionEvent(CompetitorStatusEventCode.OnCompetitorBeforeDied, HandleCompetitorBeforeDied);
        _pprefModeCamera.onValueChanged -= PPrefModeCameraOnValueChanged;

        StopCoroutineHelper(_checkDistanceNearestOpponentCR);
        StopCoroutineHelper(_fovHandleCameraUsingCR);

        KillTweenRotateThirdPerson();
    }

    private void PPrefModeCameraOnValueChanged(ValueDataChanged<int> data)
    {
        if (_pBRobotPlayer == null) return;
        _mainVcamViewUsing = SetViewCamera(data.newValue);
    }
    private void OnUnpackStart(params object[] parameters)
    {
        _cameraMain.gameObject.GetOrAddComponent<AudioListener>();
    }
    private void HandleCompetitorBeforeDied(params object[] parameters)
    {
        if (parameters[0] is PBRobot robot)
        {
            if (robot.PersonalInfo.isLocal)
            {
                Vector3 robotPosition = robot.transform.position;
                _pointTopDownView = CreatePointView(robot.transform, Vector3.up * 30f / robot.transform.localScale.x, "PointTopDown").transform;
                _topDownCinemachine.LookAt = null;
                _topDownCinemachine.Follow = null;

                StartCoroutine(CommonCoroutine.Delay(1.5f, false, () =>
                {
                    _topDownCinemachine.LookAt = _pointTopDownView;
                    _topDownCinemachine.Follow = _pointTopDownView;
                    _pointTopDownView.DOMove(new Vector3(robotPosition.x, _pointTopDownView.position.y, robotPosition.z), 1f);
                }));
            }
        }
    }


    private GameObject CreatePointView(Transform bodyView, Vector3 localPosition, string name)
    {
        GameObject objectView = new GameObject();
        objectView.transform.SetParent(bodyView.transform);
        objectView.name = name;
        objectView.transform.localPosition = localPosition;

        return objectView;
    }

    private void HandleOffsetWolrdSpaceInBodyCinemachine()
    {
        //Offset In Local
        Vector3 followOffset = new Vector3(0, 5, -22);

        //Get EulerAngles Y mainVcame
        float rotateYHandle = mainVCam.transform.eulerAngles.y;

        //Change EulerAngles X
        _topDownCinemachine.transform.eulerAngles = new Vector3(55, mainVCam.transform.eulerAngles.y, mainVCam.transform.eulerAngles.z);

        // Get the Virtual Camera component
        var virtualCamera = _topDownCinemachine.GetComponent<CinemachineVirtualCamera>();

        // Get the Transposer extension of the Virtual Camera
        var transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        Vector3 rotatedOffset = Quaternion.Euler(0f, rotateYHandle, 0f) * followOffset;

        // Set the transposer's offset to match the desired followOffset
        transposer.m_BindingMode = CinemachineTransposer.BindingMode.WorldSpace;
        transposer.m_FollowOffset = rotatedOffset;
    }
    private void TopDownView(PBRobot pbBot)
    {
        _pBRobotPlayer = pbBot;
        Transform bodyView = pbBot.ChassisInstance.CarPhysics.transform;
        if (bodyView == null) return;

        GameObject objectView = CreatePointView(bodyView, Vector3.up * 30 / pbBot.transform.localScale.x/*Vector3.forward * _offsetViewCameraTopDown*/, "PointTopDown");
        if (_pointTopDownView != null)
            Destroy(_pointTopDownView.gameObject);
        _pointTopDownView = objectView.transform;
        _topDownCinemachine.LookAt = _pointTopDownView;
        _topDownCinemachine.Follow = _pointTopDownView;
    }
    private void FirstPersonView(PBRobot pbBot)
    {
        _pBRobotPlayer = pbBot;
        Transform bodyView = pbBot.ChassisInstance.CarPhysics.transform;
        Vector3 offsetLookAt = new Vector3(0, 1, 2);
        Vector3 offsetFollow = new Vector3(0, 4, -3f);
        GameObject lookAtPoint = CreatePointView(bodyView, offsetLookAt, "PointLookAtFirstPerson");
        GameObject followPoint = CreatePointView(bodyView, offsetFollow, "PointFollowFirstPerson");

        if (bodyView == null) return;

        _pointLootAtFirstPerson = lookAtPoint.transform;
        _pointFollowFirstPerson = followPoint.transform;

        _firstPersonCinemachine.LookAt = _pointLootAtFirstPerson;
        _firstPersonCinemachine.Follow = _pointFollowFirstPerson;
    }
    private void ThirdPersonView(PBRobot pbBot)
    {
        Transform bodyView = pbBot.ChassisInstance.CarPhysics.transform;
        mainVCam.LookAt = bodyView;
        mainVCam.Follow = bodyView;
    }
    private void RotateView(PBRobot pbBot)
    {
        Transform bodyView = pbBot.ChassisInstance.CarPhysics.transform;
        _pointRotateThirdPerson = CreatePointView(bodyView, Vector3.zero, "PointRotateThirdPerson").transform;
        _pointRotateThirdPerson.transform.localEulerAngles = Vector3.zero;
        _rotateCinemachine.LookAt = _pointRotateThirdPerson;
        _rotateCinemachine.Follow = _pointRotateThirdPerson;

        var OrbitalTransposer = _rotateCinemachine.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        _thirdCinemachineOrbitalTransposer = OrbitalTransposer;
    }
    private void Update()
    {
        //UpdateRotateViewThirdPerson();
    }
    private IEnumerator DelaySetUpTopDownCamera()
    {
        HandleOffsetWolrdSpaceInBodyCinemachine();

        StartCoroutine(DisableAim(_topDownCinemachine));
        if (_isUsingEffectFOV)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutineHelper(_checkDistanceNearestOpponentCR, CheckDistanceNearestOpponent());
            StartCoroutineHelper(_fovHandleCameraUsingCR, FovHandleCameraUsing());
        }
    }
    private IEnumerator DisableAim(CinemachineVirtualCamera cinemachineVirtualCamera)
    {
        yield return new WaitForSeconds(0.2f);
        CinemachineComponentBase aimComposer = cinemachineVirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim);
        if (aimComposer != null)
            aimComposer.enabled = false;
    }
    private void StartCoroutineHelper(IEnumerator enumerator, IEnumerator enumeratorFunc)
    {
        if (enumerator != null)
            StopCoroutine(enumerator);
        enumerator = enumeratorFunc;
        StartCoroutine(enumerator);
    }
    private void StopCoroutineHelper(IEnumerator enumerator)
    {
        if (enumerator != null)
            StopCoroutine(enumerator);
    }

    private IEnumerator FovHandleCameraUsing()
    {
        yield return new WaitForSeconds(1);
        //while (true)
        //{
        //    if (IsAbleToModifyFoV)
        //    {
        //        if (_opponentNearest != null)
        //        {
        //            if (mainVCam != null)
        //            {
        //                float distance = Vector3.Distance(_player.position, _opponentNearest.position);

        //                float timeAnimationCurveCamera = Mathf.Clamp(distance, 0, _distanceFovEffect);
        //                mainVCam.m_Lens.FieldOfView = _animationCurveCameraView.Evaluate(timeAnimationCurveCamera);
        //                _topDownCinemachine.m_Lens.FieldOfView = _animationCurveCameraTopDownView.Evaluate(timeAnimationCurveCamera);
        //                _pointTopDownView.DOLocalMove(Vector3.forward * _animationCurvePointView.Evaluate(timeAnimationCurveCamera), 0);
        //            }
        //        }
        //    }
        //    yield return null;
        //}
    }

    private IEnumerator CheckDistanceNearestOpponent()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.3f);
        while (_player != null && _opponents.Count > 0)
        {
            _opponentNearest = _opponents
                    .Where(v => v != null && v.GetComponent<CarPhysics>().enabled)
                    .OrderBy(v => Vector3.Distance(v.position, _player.position)).FirstOrDefault();
            yield return waitForSeconds;
        }
    }
    private Vector3 GetMiddlePoint(Vector3 pointA, Vector3 pointB)
    {
        return (pointA + pointB) / 2f;
    }
    void HandleBotModelSpawned(params object[] parameters)
    {
        if (!gameObject.activeInHierarchy)
            return;
        var isInit = (bool)parameters[2];
        if (_opponents == null)
            _opponents = new List<Transform>();

        if (parameters[0] is PBRobot pbBot)
        {
            if (pbBot.PersonalInfo.isLocal == true)
            {
                _pBRobotPlayer = pbBot;
                StopCoroutineHelper(_checkDistanceNearestOpponentCR);
                StopCoroutineHelper(_fovHandleCameraUsingCR);

                Destroy(_audioListenerPlayer);
                _player = pbBot.ChassisInstanceTransform;
                _audioListenerPlayer = _player.gameObject.AddComponent<AudioListener>();

                //Set up Camera
                ThirdPersonView(_pBRobotPlayer);
                FirstPersonView(_pBRobotPlayer);
                TopDownView(_pBRobotPlayer);
                //RotateView(_pBRobotPlayer);

                if (isInit)
                {
                    StartCoroutine(WaitConvertCamera());
                }

                foreach (var vCam in subVCams)
                {
                    vCam.LookAt = pbBot.ChassisInstanceTransform;
                    vCam.Follow = pbBot.ChassisInstanceTransform;
                }
            }
            else
                _opponents.Add(pbBot.ChassisInstanceTransform);

            if (followTargets.Contains(pbBot) == false)
            {
                followTargets.Add(pbBot);
            }

            StartCoroutineHelper(_checkDistanceNearestOpponentCR, CheckDistanceNearestOpponent());
            StartCoroutineHelper(_fovHandleCameraUsingCR, FovHandleCameraUsing());
        }
    }
    private void KillTweenRotateThirdPerson()
    {
        if (_rightTweenRotate != null)
            _rightTweenRotate.Kill();
        if (_leftTweenRotate != null)
            _leftTweenRotate.Kill();
        if (_mySequenceRotateCam != null)
            _mySequenceRotateCam.Kill();
    }
    private IEnumerator RunRotateRight(bool isSwitch)
    {
        KillTweenRotateThirdPerson();
        RandomTimeRotateView();
        StopCoroutineHelper(_runLeftRotateThirdPersonCR);
        _rotateCinemachine.enabled = true;

        if (isSwitch)
        {
            _thirdCinemachineOrbitalTransposer.m_XAxis.Value = -90;
            yield break;
        }

        _rightTweenRotate = DOTween.To(() => _thirdCinemachineOrbitalTransposer.m_XAxis.Value, x => _thirdCinemachineOrbitalTransposer.m_XAxis.Value = x, -90, 3);
    }
    private IEnumerator RunRotateLeft(bool isSwitch)
    {
        KillTweenRotateThirdPerson();
        RandomTimeRotateView();
        StopCoroutineHelper(_runRightRotateThirdPersonCR);
        _rotateCinemachine.enabled = true;

        if (isSwitch)
        {
            _thirdCinemachineOrbitalTransposer.m_XAxis.Value = 90;
            yield break;
        }

        _leftTweenRotate = DOTween.To(() => _thirdCinemachineOrbitalTransposer.m_XAxis.Value, x => _thirdCinemachineOrbitalTransposer.m_XAxis.Value = x, 90, 3);
    }
    private void ResetRotateViewDefault()
    {
        KillTweenRotateThirdPerson();
        RandomTimeRotateView();
        StopCoroutineHelper(_runRightRotateThirdPersonCR);
        StopCoroutineHelper(_runLeftRotateThirdPersonCR);

        if (_isRotateView)
        {
            if (_player == null) return;
            _rotateCinemachine.LookAt = _player;
            _rotateCinemachine.Follow = _player;

            _mySequenceRotateCam = DOTween
            .Sequence()
            .Append(DOTween.To(() => _thirdCinemachineOrbitalTransposer.m_XAxis.Value, x => _thirdCinemachineOrbitalTransposer.m_XAxis.Value = x, 0, 1))
            .AppendInterval(_timeDelayExitRotateThirdPerson)
            .OnComplete(() =>
            {
                _isRotateView = false;
                _rotateCinemachine.enabled = false;
            });
        }
    }

    private CinemachineVirtualCamera SetViewCamera(int indexCamera)
    {
        CinemachineVirtualCamera cinemachineVirtualCamera = null;
        if (indexCamera == 0)
        {
            //third Person
            mainVCam.enabled = true;
            _topDownCinemachine.enabled = false;
            _firstPersonCinemachine.enabled = false;

            cinemachineVirtualCamera = mainVCam;

            //if (_isRotateView)
            //{
            //    mainVCam.enabled = false;
            //    _rotateCinemachine.enabled = true;
            //    _thirdCinemachineOrbitalTransposer.m_XAxis.Value = 0;

            //    if (PlayerValidAngleWall(true))
            //        StartCoroutineHelper(_runRightRotateThirdPersonCR, RunRotateRight(false));
            //    else if (PlayerValidAngleWall(false))
            //        StartCoroutineHelper(_runLeftRotateThirdPersonCR, RunRotateLeft(false));
            //    else
            //        ResetRotateViewDefault();
            //}
        }
        else if (indexCamera == 1)
        {
            //Top Down
            _topDownCinemachine.enabled = true;
            mainVCam.enabled = false;
            _firstPersonCinemachine.enabled = false;

            cinemachineVirtualCamera = _topDownCinemachine;
        }
        else
        {
            //First Person
            _firstPersonCinemachine.enabled = true;
            mainVCam.enabled = false;
            _topDownCinemachine.enabled = false;

            cinemachineVirtualCamera = _firstPersonCinemachine;
        }

        return cinemachineVirtualCamera;
    }

    private IEnumerator WaitConvertCamera()
    {
        SetViewCamera(0);
        var transposer = mainVCam.GetCinemachineComponent<CinemachineTransposer>();
        SetDampinginemachine(transposer, Vector3.zero, 0);
        yield return new WaitForSeconds(1);
        SetDampinginemachine(transposer, Vector3.one, 5);
        StartCoroutine(DelaySetUpTopDownCamera());

        //Convert Camera follwing ppref
        _mainVcamViewUsing = SetViewCamera(_pprefModeCamera.value);

    }
    private void SetDampinginemachine(CinemachineTransposer transposer, Vector3 damping, float yawDamping)
    {
        transposer.m_XDamping = damping.x;
        transposer.m_YDamping = damping.y;
        transposer.m_ZDamping = damping.z;
        transposer.m_YawDamping = yawDamping;
    }
    void HandleBotModelDespawned(params object[] parameters)
    {
        if (parameters[0] is PBRobot pbBot)
        {
            if (pbBot.PersonalInfo.isLocal == true)
            {
                mainVCam.Follow = null;
            }
            if (followTargets.Contains(pbBot) == true)
            {
                followTargets.Remove(pbBot);
            }
        }

        if (_pointFollowFirstPerson != null)
            _pointFollowFirstPerson = null;
        //if (_pointTopDownView != null)
        //    _pointTopDownView = null;
        if (_pointLootAtFirstPerson != null)
            _pointLootAtFirstPerson = null;

        _isRotateView = false;
        _rotateCinemachine.enabled = false;
    }

    private void LateUpdate()
    {
        if (followTargets.Count == 0) return;
        ZoomGroupTarget();

        void ZoomGroupTarget()
        {
            float newZoom = Mathf.Lerp(minZoom, maxZoom, GetGreatestDistance() / zoomLitmiter);
            var transposer = mainVCam.GetCinemachineComponent<CinemachineTransposer>();
            transposer.m_FollowOffset.z = Mathf.Lerp(transposer.m_FollowOffset.z, newZoom, Time.deltaTime * lerpSpeed);
        }
    }

    float GetGreatestDistance()
    {
        var bounds = GetBounds();
        return bounds.size.x;
    }

    Bounds GetBounds()
    {
        if (followTargets[0] == null) return new Bounds();
        var bounds = new Bounds(followTargets[0].ChassisInstanceTransform.position, Vector3.zero);

        for (int i = 0; i < followTargets.Count; i++)
        {
            bounds.Encapsulate(followTargets[i].ChassisInstanceTransform.position);
        }
        return bounds;
    }
    private void UpdateRotateViewThirdPerson()
    {
        if (!mainVCam.enabled) return;

        if (_player == null)
            return;

        RandomRotateViewThirdPerson();
        if (_opponentNearest == null)
        {
            _pointRotateThirdPerson.position = _player.position;
            return;
        }
        _pointRotateThirdPerson.position = GetMiddlePoint(_player.position, _opponentNearest.position);

        Vector3 directionAB = _opponentNearest.position - _player.position;
        float distance = directionAB.magnitude;

        if (distance <= _minDistanceRotate)
        {
            if (!_isRotateView)
            {
                _isRotateView = true;
                _rotateCinemachine.enabled = false;
                _thirdCinemachineOrbitalTransposer.m_XAxis.Value = 0;
                _rotateCinemachine.LookAt = _pointRotateThirdPerson;
                _rotateCinemachine.Follow = _pointRotateThirdPerson;

                if (PlayerValidAngleWall(true))
                    StartCoroutineHelper(_runRightRotateThirdPersonCR, RunRotateRight(false));
                else if (PlayerValidAngleWall(false))
                    StartCoroutineHelper(_runLeftRotateThirdPersonCR, RunRotateLeft(false));
                else
                    DOTween.To(() => _thirdCinemachineOrbitalTransposer.m_XAxis.Value, x => _thirdCinemachineOrbitalTransposer.m_XAxis.Value = x, 0, 1);
            }
        }
        else
        {
            ResetRotateViewDefault();
        }
    }
    private void RandomRotateViewThirdPerson()
    {
        if (!_isRotateView) return;

        _timeRotateView -= Time.deltaTime;
        if (_timeRotateView <= 0)
        {
            //Random Rotate View
            float randomRotateView = UnityEngine.Random.Range(1, 4);
            if (randomRotateView == 1 && PlayerValidAngleWall(true))
                StartCoroutineHelper(_runRightRotateThirdPersonCR, RunRotateRight(false));
            else if (randomRotateView == 2 && PlayerValidAngleWall(false))
                StartCoroutineHelper(_runLeftRotateThirdPersonCR, RunRotateLeft(false));
            else
            {
                RandomTimeRotateView();
                DOTween.To(() => _thirdCinemachineOrbitalTransposer.m_XAxis.Value, x => _thirdCinemachineOrbitalTransposer.m_XAxis.Value = x, 0, 1);
            }
        }
    }
    private void RandomTimeRotateView()
    {
        //Random Time
        float randomTime = UnityEngine.Random.Range(_rangeTimeConvertRotateView.x, _rangeTimeConvertRotateView.y);
        _timeRotateView = randomTime;
    }

    private bool PlayerValidAngleWall(bool isRight)
    {
        if (_pBRobotPlayer == null) return false;
        var player = _pBRobotPlayer.ChassisInstance.CarPhysics;
        Ray ray = new Ray(player.transform.position, isRight ? player.transform.right : -player.transform.right);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _distanceCheckWall, _wallMask))
            return false;
        else
            return true;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_pBRobotPlayer == null) return;
        var player = _pBRobotPlayer.ChassisInstance.CarPhysics;
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(player.transform.position, player.transform.right * _distanceCheckWall);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(player.transform.position, -player.transform.right * _distanceCheckWall);
    }
#endif
}
