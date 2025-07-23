using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Helpers;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.StateMachine;
using Sirenix.Utilities;
using Sirenix.OdinInspector;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AIBotController : MonoBehaviour
{
    public event Action<INavigationPoint> onTargetReached;
    public event Action<INavigationPoint> onTargetChanged;

    private const float VectorBehindAngles = 150f;
    private const float SafePointCheckAngles = 45f;
    private const int MaxSafePointCount = 360 / (int)SafePointCheckAngles;
    private const int ValidPointCount = 3;
    private const int MaxInterpolation = 10;
    private const float RaycastMaxDistance = 20f;
    private static readonly Color[] GizmosColors = new Color[] { Color.red, Color.green, Color.blue };

    [SerializeField, Title("DEBUG")]
    private bool isEnableLog = true;
    [SerializeField, Title("DEBUG")]
    private string[] enableLogWithTags = new string[] { nameof(AIBotController), nameof(AIBotState), nameof(AIBotStateTransition) };

    [SerializeField]
    private float gapRaycastPointScale = 1f;
    [SerializeField]
    private PBRobot pbRobot;
    [SerializeField]
    private RadarDetector radarDetector;
    [SerializeReference]
    private List<AIBotState> states;

    private bool isRunning = false;
    private ulong frameCount;
    private int allRobotLayers;
    private int allLayersIgnoreMyself;
    private int allRobotLayersIgnoreMyself;
    private INavigationPoint target;
    private PB_AIProfile aiProfile;
    private StateMachine.Controller stateMachineController = new StateMachine.Controller();
    private Transform gapRaycastPoint;
    private Transform obstacleRaycastPoint;
    private NavMeshPath navMeshPath;
    private Coroutine castSkillCoroutine;
    private Collider[] overlapCheckBoxColliders = new Collider[1];

    private Transform GapRaycastPoint
    {
        get
        {
            if (gapRaycastPoint == null)
            {
                gapRaycastPoint = new GameObject("RaycastPoint-ReversingState_Gap").transform;
                gapRaycastPoint.SetParent(CarPhysics.transform);
                gapRaycastPoint.transform.SetLocalPositionAndRotation(CarPhysics.LocalBounds.size.z * gapRaycastPointScale * -Vector3.forward, Quaternion.identity);
            }
            return gapRaycastPoint;
        }
    }
    private Transform ObstacleRaycastPoint
    {
        get
        {
            if (obstacleRaycastPoint == null)
            {
                obstacleRaycastPoint = new GameObject("RaycastPoint-ReversingState_Obstacle").transform;
                obstacleRaycastPoint.SetParent(CarPhysics.transform);
                obstacleRaycastPoint.transform.SetLocalPositionAndRotation(CarPhysics.LocalBounds.extents.z * -Vector3.forward, Quaternion.identity);
            }
            return obstacleRaycastPoint;
        }
    }

    public ulong FrameCount => frameCount;
    public bool IsEnableLog => isEnableLog;
    public bool IsRunning => isRunning;
    public float TotalTimeNotFindPathToTarget { get; set; }
    public int AllRobotLayers => allRobotLayers;
    public int AllLayersIgnoreMyself => allLayersIgnoreMyself;
    public int AllRobotLayersIgnoreMyself => allRobotLayersIgnoreMyself;
    [ShowInInspector, ReadOnly]
    public bool IsLockCastSkill { get; set; }
    [ShowInInspector, ReadOnly]
    public float LastTimeCastSkill { get; set; }
    public RadarDetector RadarDetector => radarDetector;
    public PBRobot Robot => pbRobot;
    public PBChassis Chassis => pbRobot.ChassisInstance;
    public CarPhysics CarPhysics => pbRobot.ChassisInstance.CarPhysics;
    public PB_AIProfile AIProfile => aiProfile;
    public AIBotState CurrentState => stateMachineController.CurrentState as AIBotState;
    public INavigationPoint Target
    {
        get => target;
        set
        {
            var previousTarget = target;
            target = value;
            if (previousTarget != target)
            {
                TotalTimeNotFindPathToTarget = 0f;
                onTargetChanged?.Invoke(target);
            }
        }
    }
    public ActiveSkillCaster ActiveSkillCaster => Robot.ActiveSkillCaster;

#if UNITY_EDITOR
    [FoldoutGroup("State Info"), ShowInInspector]
    private float RemainDistance => Target == null ? float.PositiveInfinity : Vector3.Distance(Target.GetTargetPoint(), Robot.GetTargetPoint());
    [FoldoutGroup("State Info"), ShowInInspector]
    private string CurrentStateName => stateMachineController.CurrentState == null ? "Null" : stateMachineController.CurrentState.ToString();
    [FoldoutGroup("State Info"), ShowInInspector]
    private UnityEngine.Object CurrentTargetAsObject => Target as UnityEngine.Object;
    [FoldoutGroup("State Info"), ShowInInspector]
    private string CurrentTarget => Target == null ? "Null" : Target.ToString();
    [FoldoutGroup("State Info"), ShowInInspector]
    private float TimeNotFindPathToTarget => TotalTimeNotFindPathToTarget;
    [FoldoutGroup("State Info"), ShowInInspector]
    private float CurrentTime => Time.time;
    [FoldoutGroup("State Info"), ShowInInspector]
    private float LastTimeCauseDamage => Robot.LastTimeCauseDamage;
    [FoldoutGroup("State Info"), ShowInInspector]
    private float LastTimeReceiveDamage => Robot.LastTimeReceiveDamage;
    [FoldoutGroup("State Info"), ShowInInspector]
    private float AvgSpeedIn15Frames => !Application.isPlaying ? 0f : CarPhysics.SpeedBuffer.GetAvg();
    [FoldoutGroup("State Info"), ShowInInspector]
    private float DisplacementInOneSec => !Application.isPlaying ? 0f : Vector3.Distance(CarPhysics.PreviousOneSecPosition, Robot.GetTargetPoint());
    [FoldoutGroup("State Info"), ShowInInspector]
    private bool IsImmobilized => !Application.isPlaying ? false : CarPhysics.IsImmobilized;
    [FoldoutGroup("State Info"), ShowInInspector]
    private bool IsStandstill => !Application.isPlaying ? false : CarPhysics.IsStandstill();
    [FoldoutGroup("State Info"), ShowInInspector]
    private bool IsUpsideDown => !Application.isPlaying ? false : CarPhysics.IsUpsideDown();
    [FoldoutGroup("State Info"), ShowInInspector]
    private float DiagonalOfBounds => Chassis == null ? 0f : CarPhysics.CalcDiagonalOfBounds();
    [FoldoutGroup("State Info"), ShowInInspector]
    private float AnglesBetweenMyForwardAndPointToTarget => Chassis == null || Target == null ? 0f : Vector3.Angle(CarPhysics.transform.forward, (Target.GetTargetPoint() - Robot.GetTargetPoint()).normalized);
#endif

    private void Awake()
    {
        ObjectFindCache<AIBotController>.Add(this);
    }

    private void Start()
    {
        foreach (var robotLayerIndex in PBRobot.allFightingRobots.Select(robot => robot.RobotLayer))
        {
            allRobotLayers |= 1 << robotLayerIndex;
        }
        navMeshPath = new NavMeshPath();
        allLayersIgnoreMyself = Physics.DefaultRaycastLayers ^ (1 << Robot.RobotLayer);
        allRobotLayersIgnoreMyself = allRobotLayers ^ (1 << Robot.RobotLayer);
        aiProfile = pbRobot.PlayerInfoVariable != null && pbRobot.PlayerInfoVariable.value != null ? pbRobot.PlayerInfoVariable.value.Cast<PBBotInfo>()?.aiProfile : null;
        radarDetector.Initialize(this);
        Robot.OnHealthChanged += OnHealthChanged;
        InitializeStateMachine();
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, StartStateMachine);
    }

    private void OnHealthChanged(Competitor.HealthChangedEventData eventData)
    {
        if (Robot.IsDead)
        {
            StartCoroutine(SetTargetNull_CR());

            IEnumerator SetTargetNull_CR()
            {
                yield return CommonCoroutine.EndOfFrame;
                Target = null;
                StopStateMachine();
            }
        }
    }

    private void OnDestroy()
    {
        ObjectFindCache<AIBotController>.Remove(this);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, StartStateMachine);
    }

    private void Update()
    {
        if (!isRunning || Robot.IsDead)
            return;
        stateMachineController.Update();
        // Handle cast skill
        HandleCastSkill();
    }

    private void LateUpdate()
    {
        frameCount++;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        foreach (var state in states)
        {
            state.InvokeMethod("OnValidate");
        }
        radarDetector.InvokeMethod("OnValidate");
    }

    private void OnDrawGizmos()
    {
        if (Robot == null || Robot.ChassisInstanceTransform == null || !enabled)
            return;
        if (!RadarDetector.DrawGizmos)
            return;
        var currentPos = Robot.GetTargetPoint();
        var currentRot = Robot.ChassisInstanceTransform.rotation;
        var halfDiagonalOfBounds = CarPhysics.CalcDiagonalOfBounds() / 2f;
        var count = 360f / NavMeshHelper.ReachablePointPartitionAngles;
        for (int i = 0; i < count; i++)
        {
            var distance = halfDiagonalOfBounds + NavMeshHelper.ReachablePointMaxDistance;
            var direction = transform.rotation * Quaternion.Euler(new Vector3(0f, i * NavMeshHelper.ReachablePointPartitionAngles, 0f)) * Vector3.forward;
            var targetPos = currentPos + direction * distance;
            if (!NavMesh.Raycast(currentPos, targetPos, out NavMeshHit hit, NavMesh.AllAreas) && IsReachableNavMesh(currentPos, targetPos, false))
            {
                var numOfReachablePoints = NavMeshHelper.CalcNumOfReachablePoints(targetPos, currentRot, angles: SafePointCheckAngles, filterPredicate: point => IsReachableNavMesh(targetPos, point, false));
                Gizmos.color = Color.Lerp(Color.black, Color.green, (float)numOfReachablePoints / MaxSafePointCount);
                Gizmos.DrawWireSphere(targetPos, 0.5f);
                Gizmos.color = Color.green;
                Handles.color = Color.black;
                Handles.Label(targetPos, numOfReachablePoints.ToString());
            }
            else
            {
                Gizmos.color = Color.black;
                Gizmos.DrawWireSphere(hit.hit ? hit.position : targetPos, 0.5f);
            }
            Gizmos.DrawRay(currentPos, direction * distance);
        }
        // if (Target is PBRobot targetRobot && targetRobot.PersonalInfo.isLocal)
        // {
        //     Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        //     Gizmos.DrawSphere(Robot.ChassisInstanceTransform.position, 5f);
        // }
        foreach (var state in states)
        {
            state.InvokeMethod("OnDrawGizmos");
        }
        radarDetector.InvokeMethod("OnDrawGizmos");
    }

    private void OnDrawGizmosSelected()
    {
        if (!RadarDetector.DrawGizmos)
            return;
        foreach (var state in states)
        {
            state.InvokeMethod("OnDrawGizmosSelected");
        }
        radarDetector.InvokeMethod("OnDrawGizmosSelected");
    }
#endif

    private IEnumerator CastSkill_CR()
    {
        IsLockCastSkill = true;
        float delayTime = AIProfile.DelayTimeRangeBeforeCastSkill.RandomRange();
        Log($"Wait to cast skill: ({AIProfile.DelayTimeRangeBeforeCastSkill.minValue}, {AIProfile.DelayTimeRangeBeforeCastSkill.maxValue}) - {delayTime} - {Time.time}");
        yield return new WaitForSeconds(delayTime);
        yield return new WaitUntil(() => ActiveSkillCaster.IsAbleToPerformSkillForAI());
        Log($"Cast skill - {Time.time}");
        LastTimeCastSkill = Time.time;
        ActiveSkillCaster.PerformSkill();
        IsLockCastSkill = false;
        castSkillCoroutine = null;
    }

    private void HandleCastSkill()
    {
        if (IsLockCastSkill || ActiveSkillCaster == null)
            return;
        if (ActiveSkillCaster.IsAbleToPerformSkillForAI())
        {
            castSkillCoroutine = StartCoroutine(CastSkill_CR());
        }
    }

    [System.Diagnostics.Conditional(LGDebug.k_UnityEditorDefineSymbol)]
    public void Log(string message, string tag = nameof(AIBotController), UnityEngine.Object context = null)
    {
        if (!IsEnableLog)
            return;
        if (!enableLogWithTags.Contains(tag))
            return;
        LGDebug.Log(message, tag, context ?? Robot);
    }

    [System.Diagnostics.Conditional(LGDebug.k_UnityEditorDefineSymbol)]
    public void VisualizeOverlapBox(Vector3 position, Quaternion rotation, Vector3 halfExtends, bool isOverlap)
    {
        // if (!IsEnableLog)
        //     return;
        // var color = isOverlap ? Color.red : Color.green;
        // color.a = 0.5f;
        // DebugCube.transform.SetPositionAndRotation(position, rotation);
        // DebugCube.transform.localScale = halfExtends * 2f;
        // DebugCube.GetComponent<Renderer>().material.color = color;
    }

    public void InitializeStateMachine()
    {
        foreach (var state in states)
        {
            state.InitializeState(this);
        }
    }

    public void StartStateMachine()
    {
        if (states.Count <= 0)
            return;
        isRunning = true;
        stateMachineController.StateChanged(states[0]);
    }

    public void StopStateMachine()
    {
        isRunning = false;
        stateMachineController.Stop();
        StopAllCoroutines();
    }

    public AIBotState FindStateById(string stateId)
    {
        return FindStateById<AIBotState>(stateId);
    }

    public T FindStateById<T>(string stateId) where T : AIBotState
    {
        foreach (var state in states)
        {
            if (state.StateId == stateId)
                return state as T;
        }
        return null;
    }

    public bool IsAnyGapDetected()
    {
        var raycastPoint = GapRaycastPoint.position;
        raycastPoint.y = CarPhysics.transform.position.y;
        if (Physics.Raycast(raycastPoint, Vector3.down, out RaycastHit hitInfo, RaycastMaxDistance, CarPhysics.RaycastMask, QueryTriggerInteraction.Ignore))
        {
            Log($"Detect ground: {hitInfo.collider}");
            return false;
        }
        Log($"Not detect any ground -> so it's probably a gap");
        return true;
    }

    public bool IsAnyObstacleDetected()
    {
        var from = CarPhysics.transform.position;
        var to = ObstacleRaycastPoint.transform.position;
        var dir = to - from;
        var raycastLayers = AllLayersIgnoreMyself ^ CarPhysics.RaycastMask;
        if (Physics.Raycast(from, dir, out RaycastHit hitInfo, dir.magnitude, raycastLayers))
        {
            Log($"Detect obstacle or wall or other players: {hitInfo.collider}");
            return hitInfo.collider.GetComponent<CollectableItem>() == null;
        }
        Log($"Not detect any obstacle");
        return false;
    }

    public bool IsAbleToReach(INavigationPoint navPoint)
    {
        return NavMesh.CalculatePath(Robot.GetTargetPoint(), navPoint.GetTargetPoint(), NavMesh.AllAreas, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete;
    }

    public void NotifyEventTargetReached(INavigationPoint navPoint)
    {
        onTargetReached?.Invoke(navPoint);
    }

    [ShowInInspector]
    public bool IsReachablePhysics(Vector3 from, Vector3 to, bool isVisualize = true, bool isLog = true)
    {
        if (Mathf.Approximately((to - from).sqrMagnitude, 0f))
            return true;
        var carPhysics = CarPhysics;
        var allLayersIgnoreMyselfAndTarget = AllLayersIgnoreMyself ^ (Target.GetPointType() == PointType.OpponentPoint ? 1 << (Target as PBRobot).RobotLayer : 0);
        var allLayersIgnoreMyselfAndTargetAndGround = allLayersIgnoreMyselfAndTarget ^ carPhysics.RaycastMask;
        var boxCenter = Vector3.Lerp(from, to, 0.5f);
        var boxRotation = Quaternion.LookRotation((to - from).normalized);
        var boxHalfExtends = carPhysics.LocalBounds.extents;
        boxHalfExtends.y = Mathf.Max(boxHalfExtends.y * 2f, Mathf.Abs(from.y - to.y) / 2f);
        boxHalfExtends.z = Vector3.Distance(from, to) / 2f;
        // Checks overlap a big box against all colliders to find whether any obstacles is on this way
        if (Physics.OverlapBoxNonAlloc(boxCenter, boxHalfExtends, overlapCheckBoxColliders, boxRotation, allLayersIgnoreMyselfAndTargetAndGround, QueryTriggerInteraction.Ignore) > 0)
        {
            if (isVisualize)
                VisualizeOverlapBox(boxCenter, boxRotation, boxHalfExtends, true);
            if (isLog)
                Log($"Not reachable because it will collide with {overlapCheckBoxColliders[0]}", context: overlapCheckBoxColliders[0]);
            return false;
        }
        else
        {
            if (isVisualize)
                VisualizeOverlapBox(boxCenter, boxRotation, boxHalfExtends, false);
        }
        for (int i = 0; i < MaxInterpolation; i++)
        {
            var interpolatePoint = Vector3.Lerp(from, to, (i + 1) / (float)MaxInterpolation);
            if (isVisualize)
                Debug.DrawRay(interpolatePoint, Vector3.down * RaycastMaxDistance, GizmosColors[i % GizmosColors.Length]);
            if (Physics.Raycast(interpolatePoint, Vector3.down, out var hitInfo, RaycastMaxDistance, allLayersIgnoreMyselfAndTarget, QueryTriggerInteraction.Ignore))
            {
                if ((1 << hitInfo.collider.gameObject.layer) != carPhysics.RaycastMask)
                {
                    if (isLog)
                        Log($"Not reachable because it will collide with {hitInfo.collider}", context: hitInfo.collider);
                    return false;
                }
            }
            else
            {
                if (isLog)
                    Log($"Not reachable because it does not hit the Ground");
                return false;
            }
        }
        if (isLog)
            Log($"It's reachable");
        return true;
    }

    public bool IsReachableNavMesh(Vector3 from, Vector3 to, bool isIgnoreRobotTarget = true, bool isVisualize = false)
    {
        if (Mathf.Approximately((to - from).sqrMagnitude, 0f))
            return true;
        var carPhysics = CarPhysics;
        var layerMaskToCheckCollision = isIgnoreRobotTarget ? AllRobotLayersIgnoreMyself ^ (Target.GetPointType() == PointType.OpponentPoint ? 1 << (Target as PBRobot).RobotLayer : 0) : AllRobotLayersIgnoreMyself;
        var boxCenter = Vector3.Lerp(from, to, 0.5f);
        var boxRotation = Quaternion.LookRotation((to - from).normalized);
        var boxHalfExtends = carPhysics.LocalBounds.extents;
        boxHalfExtends.y = Mathf.Max(boxHalfExtends.y * 2f, Mathf.Abs(from.y - to.y) / 2f);
        boxHalfExtends.z = Vector3.Distance(from, to) / 2f;
        // Do not hit any robot and is unobstructed on the way.
        var isOverlapOrObstruct = !(!NavMesh.Raycast(from, to, out NavMeshHit navMeshHit, NavMesh.AllAreas) && Physics.OverlapBoxNonAlloc(boxCenter, boxHalfExtends, overlapCheckBoxColliders, boxRotation, layerMaskToCheckCollision, QueryTriggerInteraction.Ignore) == 0);
        if (isVisualize)
            VisualizeOverlapBox(boxCenter, boxRotation, boxHalfExtends, isOverlapOrObstruct);
        return !isOverlapOrObstruct;
    }

    public bool IsOverlapAnyThing(Vector3 from, Vector3 to, int layersToCheckOverlap, bool isVisualize = false)
    {
        if (Mathf.Approximately((to - from).sqrMagnitude, 0f))
            return true;
        var carPhysics = CarPhysics;
        var boxCenter = Vector3.Lerp(from, to, 0.5f);
        var boxRotation = Quaternion.LookRotation((to - from).normalized);
        var boxHalfExtends = carPhysics.LocalBounds.extents;
        boxHalfExtends.y = Mathf.Max(boxHalfExtends.y * 2f, Mathf.Abs(from.y - to.y) / 2f);
        boxHalfExtends.z = Vector3.Distance(from, to) / 2f;
        // Do not hit any robot and is unobstructed on the way.
        var isOverlap = !(Physics.OverlapBoxNonAlloc(boxCenter, boxHalfExtends, overlapCheckBoxColliders, boxRotation, layersToCheckOverlap, QueryTriggerInteraction.Ignore) == 0);
        if (isVisualize)
            VisualizeOverlapBox(boxCenter, boxRotation, boxHalfExtends, isOverlap);
        return isOverlap;
    }

    public bool IsInDangerousZone(out List<Vector3> reachablePoints, float vectorBehindAngles = VectorBehindAngles)
    {
        float angles = 30f;
        int validPointCount = Mathf.RoundToInt((180f - vectorBehindAngles) / angles) * 2 + 1;
        float halfDiagonalOfBounds = CarPhysics.CalcDiagonalOfBounds() / 2f;
        if (NavMeshHelper.TryGetReachablePointsFromSource(CarPhysics, out reachablePoints, angles))
        {
            var safePointsBehindMe = reachablePoints.Where(IsBehindMe).ToList();
            var safePointsBehindMeCount = safePointsBehindMe.Count;
            if (safePointsBehindMeCount < validPointCount)
            {
                return true;
            }
        }
        return false;

        bool IsBehindMe(Vector3 point)
        {
            var dir = point - Robot.GetTargetPoint();
            var forward = Robot.ChassisInstanceTransform.forward;
            var dp = Vector3.Dot(forward, dir.normalized);
            var cos = Mathf.Cos(vectorBehindAngles * Mathf.Deg2Rad);
            return dp < cos || Mathf.Approximately(dp, cos);
        }
    }

    public bool TryFindTheSafePoint(List<Vector3> reachablePoints, out Vector3 safePoint)
    {
        var currentPosition = Robot.GetTargetPoint();
        reachablePoints = reachablePoints.Where(IsSafePoint).ToList();
        reachablePoints.Sort(CompareTwoPoint);
        safePoint = reachablePoints.Count > 0 ? reachablePoints[0] : default;
        return reachablePoints.Count > 0;

        int CompareTwoPoint(Vector3 a, Vector3 b)
        {
            return NavMeshHelper.CalcNumOfReachablePoints(b, Quaternion.identity, angles: SafePointCheckAngles, filterPredicate: point => IsValidDirection(b, point))
            .CompareTo(NavMeshHelper.CalcNumOfReachablePoints(a, Quaternion.identity, angles: SafePointCheckAngles, filterPredicate: point => IsValidDirection(a, point)));
        }
        bool IsValidDirection(Vector3 from, Vector3 to)
        {
            var dir1 = (from - currentPosition).normalized;
            var dir2 = (to - from).normalized;
            if (Vector3.Dot(dir2, dir1) < 0)
                return false;
            return IsReachableNavMesh(from, to, false);
        }
        bool IsSafePoint(Vector3 point)
        {
            // Check whether I'm able to reach that point
            if (!IsReachableNavMesh(Robot.GetTargetPoint(), point, false))
                return false;
            return true;
        }
    }

    public bool IsUsingSkill()
    {
        return ActiveSkillCaster == null ? false : ActiveSkillCaster.skillState == ActiveSkillCaster.SkillState.Active || (Time.time - LastTimeCastSkill <= 3f);
    }
}

#region RadarDetector
[Serializable]
public class RadarDetector
{
    [SerializeField]
    private FloatVariable maxRadiusVar;
    [SerializeField]
    private FloatVariable maxAnglesVar;
    [SerializeField, BoxGroup("Debug")]
    private bool drawGizmos = true;
    [SerializeField, BoxGroup("Debug")]
    private Color detectRobotWithinAreaColor = Color.red;
    [SerializeField, BoxGroup("Debug")]
    private Color notDetectRobotWithinAreaColor = Color.green;

    private bool isEnabled = true;
    private float lastTimeFetchRobotsInDetectArea;
    private Coroutine disableRadarInDurationCoroutine;
    private AIBotController botController;
    private List<(PBRobot, float)> robotsInDetectArea = new List<(PBRobot, float)>();
    private Collider[] detectedColliders = new Collider[1];

    public float MaxRadius { get; set; }
    public float MaxAngle { get; set; }
    [ShowInInspector]
    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            isEnabled = value;
            if (isEnabled && disableRadarInDurationCoroutine != null)
            {
                StopCoroutine(disableRadarInDurationCoroutine);
                disableRadarInDurationCoroutine = null;
            }
        }
    }
    public AIBotController BotController => botController;

    public bool DrawGizmos => drawGizmos;

    private void OnValidate()
    {

    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;
        if (botController == null || !IsEnabled)
            return;
        Gizmos.matrix = botController.CarPhysics.transform.localToWorldMatrix;
        Gizmos.color = TryScanRobotInDetectArea(out _) ? detectRobotWithinAreaColor : notDetectRobotWithinAreaColor;
        Gizmos.DrawLine(Vector3.zero, Quaternion.Euler(-Vector3.right * MaxAngle / 2f) * Vector3.forward * MaxRadius);
        Gizmos.DrawLine(Vector3.zero, Quaternion.Euler(Vector3.right * MaxAngle / 2f) * Vector3.forward * MaxRadius);
        Gizmos.DrawLine(Vector3.zero, Quaternion.Euler(Vector3.up * MaxAngle / 2f) * Vector3.forward * MaxRadius);
        Gizmos.DrawLine(Vector3.zero, Quaternion.Euler(-Vector3.up * MaxAngle / 2f) * Vector3.forward * MaxRadius);
        Handles.color = Gizmos.color;
        Handles.DrawSolidArc(botController.Robot.GetTargetPoint(), Quaternion.AngleAxis(90, -botController.CarPhysics.transform.right) * botController.CarPhysics.transform.forward, Quaternion.AngleAxis(-MaxAngle / 2, botController.CarPhysics.transform.up) * botController.CarPhysics.transform.forward, MaxAngle, MaxRadius);
        Handles.DrawSolidArc(botController.Robot.GetTargetPoint(), botController.CarPhysics.transform.forward, Quaternion.AngleAxis(-MaxAngle / 2, botController.CarPhysics.transform.up) * botController.CarPhysics.transform.forward, 360, MaxRadius);
    }

    private void OnDrawGizmosSelected()
    {

    }
#endif

    private Coroutine StartCoroutine(IEnumerator routine)
    {
        return botController.StartCoroutine(routine);
    }

    private void StopCoroutine(Coroutine routine)
    {
        botController.StopCoroutine(routine);
    }

    public void Initialize(AIBotController botController)
    {
        Initialize(botController, maxRadiusVar.value, maxAnglesVar.value);
    }

    public void Initialize(AIBotController botController, float maxRadius, float maxAngle, bool drawGizmos = true)
    {
        if (botController == null)
            return;
        this.drawGizmos = drawGizmos;
        this.botController = botController;
        MaxRadius = maxRadius;
        MaxAngle = maxAngle;
    }

    public List<(PBRobot, float)> ScanAllRobotsInDetectArea(Vector3 center, Vector3 forward)
    {
        if (!IsEnabled)
            return null;
        var currentTime = Time.time;
        if (lastTimeFetchRobotsInDetectArea < currentTime)
        {
            lastTimeFetchRobotsInDetectArea = currentTime;
            robotsInDetectArea.Clear();
            foreach (var robot in PBRobot.allFightingRobots)
            {
                if (robot.IsDead || botController.Robot.GetHashCode() == robot.GetHashCode())
                    continue;
                int hitCount = Physics.OverlapSphereNonAlloc(center, MaxRadius, detectedColliders, 1 << robot.RobotLayer, QueryTriggerInteraction.Ignore);
                if (hitCount > 0 && Vector3.Angle(detectedColliders[0].transform.position - center, forward) <= MaxAngle / 2f)
                {
                    Vector3 closestPoint = detectedColliders[0].ClosestPoint(center);
                    robotsInDetectArea.Add((robot, Vector3.Distance(closestPoint, center)));
                }
            }
        }
        return robotsInDetectArea;
    }

    public List<(T, float)> ScanAllObjectsInDetectArea<T>(Vector3 center, Vector3 forward, int layers = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide, Predicate<T> predicate = null)
    {
        if (!IsEnabled)
            return null;
        List<(T, float)> results = new List<(T, float)>();
        Collider[] detectedColliders = Physics.OverlapSphere(center, MaxRadius, layers, queryTriggerInteraction);
        for (int i = 0; i < detectedColliders.Length; i++)
        {
            T component = default;
            bool isComponentFound = (detectedColliders[i].attachedRigidbody != null && detectedColliders[i].attachedRigidbody.TryGetComponent(out component)) || detectedColliders[i].TryGetComponent(out component);
            if (isComponentFound && Vector3.Angle(detectedColliders[i].transform.position - center, forward) <= MaxAngle / 2f && (predicate?.Invoke(component) ?? true))
            {
                Vector3 closestPoint = detectedColliders[i].ClosestPoint(center);
                results.Add((component, Vector3.Distance(closestPoint, center)));
            }
        }
        return results;
    }

    public List<(PBRobot, float)> ScanAllRobotsInDetectArea()
    {
        if (!IsEnabled)
            return robotsInDetectArea;
        return ScanAllRobotsInDetectArea(botController.CarPhysics.transform.position, botController.CarPhysics.transform.forward);
    }

    public List<(T, float)> ScanAllObjectsInDetectArea<T>(int layers = Physics.DefaultRaycastLayers, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Collide, Predicate<T> predicate = null)
    {
        return ScanAllObjectsInDetectArea(botController.CarPhysics.transform.position, botController.CarPhysics.transform.forward, layers, queryTriggerInteraction, predicate);
    }

    public bool TryScanRobotInDetectArea(Vector3 center, Vector3 forward, out PBRobot robot, params PBRobot[] filterRobots)
    {
        robot = ScanAllRobotsInDetectArea(center, forward)
            ?.Where(item => !filterRobots.Contains(item.Item1))
            .OrderBy(item => item.Item2)
            .FirstOrDefault().Item1;
        return robot != null;
    }

    public bool TryScanOpponentRobotInDetectArea(Vector3 center, Vector3 forward, out PBRobot robot, params PBRobot[] filterRobots)
    {
        var candidates = ScanAllRobotsInDetectArea(center, forward)
            ?.Where(item => item.Item1 != null)
            .Where(item => !filterRobots.Contains(item.Item1))
            .Where(item => item.Item1.TeamId != botController.Robot.TeamId)
            .OrderBy(item => item.Item2)
            .ToList();

        if (candidates != null && candidates.Count > 0)
        {
            robot = candidates[0].Item1;
            return true;
        }

        robot = null;
        return false;
    }

    public bool TryScanRobotInDetectArea(out PBRobot robot, params PBRobot[] filterRobots)
    {
        return TryScanRobotInDetectArea(botController.CarPhysics.transform.position, botController.CarPhysics.transform.forward, out robot, filterRobots);
    }
    public bool TryScanOpponentRobotInDetectArea(out PBRobot robot, params PBRobot[] filterRobots)
    {
        return TryScanOpponentRobotInDetectArea(botController.CarPhysics.transform.position, botController.CarPhysics.transform.forward, out robot, filterRobots);
    }
    public bool IsRobotInDetectArea(PBRobot robot)
    {
        return ScanAllRobotsInDetectArea()?.Exists(item => item.Item1 == robot) ?? false;
    }

    public void DisableInDuration(float seconds)
    {
        if (disableRadarInDurationCoroutine != null)
        {
            StopCoroutine(disableRadarInDurationCoroutine);
        }
        IsEnabled = false;
        disableRadarInDurationCoroutine = StartCoroutine(CommonCoroutine.Delay(seconds, false, () =>
        {
            IsEnabled = true;
        }));
    }
}
#endregion