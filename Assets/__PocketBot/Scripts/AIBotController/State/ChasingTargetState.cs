using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

[Serializable]
public class ChasingTargetState : AIBotState
{
    [Serializable]
    public struct Config
    {
        public float stopDistance;
        public float predictObstacleDistance;
        public AnimationCurve fallOffAccelCurve;
        public ChasingTargetToAttackingTargetTransition.Config attackingTargetTransitionConfig;
        public ChasingTargetToLookingForTargetTransition.Config lookingForTargetTransitionConfig;
    }

    private const float MaxDistance = 50f;
    private const float ThresholdToReverse = -0.86602529158f; // 30°
    private static int StaticGroundLayer;
    private static int DynamicGroundLayer;
    private static readonly Color[] GizmosColors = new Color[] { Color.red, Color.green, Color.blue };

    private bool hasAnyDynamicGround;
    private Vector3 targetPosition;
    private Vector3 lastTargetPosition = Vector3.negativeInfinity;
    private NavMeshPath navMeshPath;

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (BotController == null || BotController.Target == null || navMeshPath == null)
            return;
        var botPosition = BotController.Robot.GetTargetPoint();
        if (navMeshPath.corners.Length > 0)
        {
            for (int i = 0; i < navMeshPath.corners.Length - 1; i++)
            {
                var from = navMeshPath.corners[i];
                var to = navMeshPath.corners[i + 1];
                Gizmos.color = GizmosColors[i % GizmosColors.Length];
                Gizmos.DrawLine(from, to);
            }
        }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(botPosition, targetPosition);
    }

    protected override void OnStateEnable()
    {
        base.OnStateEnable();
        lastTargetPosition = Vector3.negativeInfinity;
        targetPosition = BotController.Robot.GetTargetPoint();
    }

    protected override void OnStateDisable()
    {
        base.OnStateDisable();
        BotController.TotalTimeNotFindPathToTarget = 0f;
    }

    protected override void OnStateUpdate()
    {
        base.OnStateUpdate();
        if (BotController.Target == null || BotController.Target.IsRobotReached(BotController.Robot))
            return;
        NavMeshHit hit = default;
        var robot = BotController.Robot;
        var carPhysics = BotController.CarPhysics;
        var botTransform = robot.ChassisInstanceTransform;
        var botPosition = botTransform.position;
        var movingDir = targetPosition - botPosition;
        var exceptionAccelInput = 0f;
        var isLastPositionOnNavMesh = false;
        var isPathFound = TryCalculatePath();
        if (isPathFound && navMeshPath.corners.Length > 1)
        {
            // Path is found
            var index = CalcCornerIndex();
            if (index == 1)
                AdjustPath(ref navMeshPath);
            targetPosition = navMeshPath.corners[index];
            lastTargetPosition = targetPosition;
            movingDir = targetPosition - botPosition;
            isLastPositionOnNavMesh = targetPosition == navMeshPath.corners[^1] && Vector3.Distance(botPosition, navMeshPath.corners[^1]) <= GetConfig().stopDistance;
            BotController.TotalTimeNotFindPathToTarget = 0f;
            //Log($"Path is found - {index} - {navMeshPath.corners.Length} - {movingDir.magnitude} - {Vector3.Distance(botPosition, navMeshPath.corners[^1])}");
        }
        else
        {
            // Path is not found
            if (!lastTargetPosition.Equals(Vector3.negativeInfinity) && BotController.IsReachablePhysics(botPosition, lastTargetPosition, false))
            {
                // Path is not found but last target position is still reachable then try to reach it
                targetPosition = lastTargetPosition;
                movingDir = targetPosition - botPosition;
                //Log($"Path is not found -> go to last target pos");
            }
            else if (NavMesh.SamplePosition(botPosition, out hit, 5f, NavMesh.AllAreas))
            {
                // Path is not found & last target position is not reachable -> Find closest point on NavMesh from me and go to there
                exceptionAccelInput = 1f;
                targetPosition = hit.position;
                movingDir = targetPosition - botPosition;
                //Log($"Path is not found -> go to closest point on NavMesh");
            }
            else
            {
                targetPosition = botPosition;
                movingDir = Vector3.zero;
            }
            isLastPositionOnNavMesh = true;
            BotController.TotalTimeNotFindPathToTarget += Time.deltaTime;
        }
        // Never reverse car, never turn your back on the enemy
        if (BotController.Target.GetPointType() == PointType.OpponentPoint)
        {
            carPhysics.AccelInput = GetMovingSpeed() * Mathf.Clamp01(GetConfig().fallOffAccelCurve.Evaluate(movingDir.magnitude) + exceptionAccelInput);
            carPhysics.RotationInput = Vector3.Dot(botTransform.right, movingDir.normalized);
            carPhysics.SteeringSpeedMultiplier = GetSteeringSpeed();
            carPhysics.TurningMultiplier = GetSteeringSpeed();
            carPhysics.InputDir = movingDir;
        }
        else
        {
            var accelInputSign = Vector3.Dot(carPhysics.transform.forward, movingDir.normalized) >= ThresholdToReverse ? 1f : -1f;
            carPhysics.AccelInput = GetMovingSpeed() * Mathf.Clamp01(GetConfig().fallOffAccelCurve.Evaluate(movingDir.magnitude) + exceptionAccelInput) * accelInputSign;
            carPhysics.RotationInput = Vector3.Dot(botTransform.right, accelInputSign > 0 ? movingDir.normalized : -movingDir.normalized);
            carPhysics.SteeringSpeedMultiplier = GetSteeringSpeed();
            carPhysics.TurningMultiplier = GetSteeringSpeed();
            carPhysics.InputDir = movingDir;
        }
        if (Vector3.Scale(movingDir, new Vector3(1f, 0f, 1f)).magnitude <= GetConfig().stopDistance && isLastPositionOnNavMesh)
        {
            carPhysics.Brake();
            if (targetPosition == lastTargetPosition)
            {
                lastTargetPosition = Vector3.negativeInfinity;
            }
        }
        Debug.DrawRay(botPosition, movingDir, Color.black);
        //Log($"{carPhysics.AccelInput} - {carPhysics.RotationInput} - {carPhysics.InputDir}");

        void AdjustPath(ref NavMeshPath path)
        {
            var targetPoint = path.corners[1];
            var currentPoint = botPosition;
            var dir = targetPoint - currentPoint;
            // Casts a big box against all colliders to find whether any obstacles is on this way to avoid other robot
            if (!BotController.IsReachableNavMesh(currentPoint, targetPoint, isVisualize: true))
            {
                List<Vector3> points = null;
                CarPhysics source = BotController.CarPhysics;
                if (NavMeshHelper.TryGetReachablePointsFromSource(source, out points, filterPredicate: FilterPoint))
                {
                    var point = points[0];
                    // FIXME: Dirty fix problem, it's probably cause GC because allocate new memory space over time.
                    var tempPath = new NavMeshPath();
                    if (NavMesh.CalculatePath(botPosition, point, NavMesh.AllAreas, tempPath) && tempPath.corners.Length > 1)
                    {
                        path = tempPath;
                    }
                }
                else if (NavMeshHelper.TryGetReachablePointsFromSource(source, out points, maxDistanceMultiplier: 2f, filterPredicate: FilterPoint))
                {
                    var point = points[0];
                    // FIXME: Dirty fix problem, it's probably cause GC because allocate new memory space over time.
                    var tempPath = new NavMeshPath();
                    if (NavMesh.CalculatePath(botPosition, point, NavMesh.AllAreas, tempPath) && tempPath.corners.Length > 1)
                    {
                        path = tempPath;
                    }
                }
            }

            bool FilterPoint(Vector3 point)
            {
                return BotController.IsReachableNavMesh(currentPoint, point) && BotController.IsReachableNavMesh(point, targetPoint);
            }
        }
        bool TryCalculatePath()
        {
            if (NavMesh.CalculatePath(botPosition, BotController.Target.GetTargetPoint(), NavMesh.AllAreas, navMeshPath))
                return true;
            if (NavMesh.SamplePosition(BotController.Target.GetTargetPoint(), out hit, 5f, NavMesh.AllAreas) && NavMesh.CalculatePath(botPosition, hit.position, NavMesh.AllAreas, navMeshPath))
                return true;
            if (NavMesh.FindClosestEdge(BotController.Target.GetTargetPoint(), out hit, NavMesh.AllAreas) && NavMesh.CalculatePath(botPosition, hit.position, NavMesh.AllAreas, navMeshPath))
                return true;
            return false;
        }
        int CalcCornerIndex()
        {
            if (navMeshPath.corners.Length > 2 && (IsOnDynamicGround(navMeshPath.corners[2]) || (IsOnDynamicGround(botPosition) && IsOnStaticGround(navMeshPath.corners[2]))) && BotController.IsReachablePhysics(botPosition, navMeshPath.corners[2]))
                return 2;
            return 1;
        }
    }

    private float GetMovingSpeed()
    {
        return BotController.AIProfile?.MovingSpeed ?? Const.FloatValue.OneF;
    }

    private float GetSteeringSpeed()
    {
        return BotController.AIProfile?.SteeringSpeed ?? Const.FloatValue.OneF;
    }

    private bool HasAnyDynamicGround()
    {
        var surfaces = NavMeshSurface.activeSurfaces;
        return surfaces.Any(surface => surface != null && surface.GetComponentInParent<IDynamicGround>() != null);
    }

    public override void InitializeState(AIBotController botController)
    {
        base.InitializeState(botController);
        navMeshPath = new NavMeshPath();
        hasAnyDynamicGround = HasAnyDynamicGround();
        StaticGroundLayer = 1 << NavMesh.GetAreaFromName("Walkable");
        DynamicGroundLayer = 1 << NavMesh.GetAreaFromName("DynamicGround");
    }

    public bool IsReachTarget()
    {
        var isRobotReachedTarget = BotController.Target.IsRobotReached(BotController.Robot);
        if (BotController.Target.GetPointType() != PointType.OpponentPoint)
            return isRobotReachedTarget;
        var layers = Physics.DefaultRaycastLayers
        ^ (1 << BotController.Robot.RobotLayer)
        ^ (1 << (BotController.Target as PBRobot).RobotLayer)
        ^ BotController.CarPhysics.RaycastMask;
        return isRobotReachedTarget && !BotController.IsOverlapAnyThing(BotController.Robot.GetTargetPoint(), BotController.Target.GetTargetPoint(), layers, isVisualize: true);
    }

    public bool IsOnDynamicGround(Vector3 wayPoint)
    {
        if (!hasAnyDynamicGround)
            return false;
        if (!NavMesh.SamplePosition(wayPoint, out NavMeshHit dynamicGroundHit, 5f, DynamicGroundLayer))
            return false;
        if (!NavMesh.SamplePosition(wayPoint, out NavMeshHit staticGroundHit, 5f, StaticGroundLayer))
            return true;
        return dynamicGroundHit.distance < staticGroundHit.distance;
    }

    public bool IsOnStaticGround(Vector3 wayPoint)
    {
        var allLayersIgnoreMyselfAndTarget = BotController.AllLayersIgnoreMyself ^ (BotController.Target.GetPointType() == PointType.OpponentPoint ? 1 << (BotController.Target as PBRobot).RobotLayer : 0);
        if (Physics.Raycast(wayPoint, Vector3.down, out RaycastHit hitInfo, MaxDistance, allLayersIgnoreMyselfAndTarget, QueryTriggerInteraction.Ignore))
        {
            if ((1 << hitInfo.collider.gameObject.layer) != BotController.CarPhysics.RaycastMask)
                return false;
            if (hitInfo.collider.GetComponentInParent<IDynamicGround>() != null)
                return false;
            return true;
        }
        return false;
    }

    public Config GetConfig()
    {
        return BotController.AIProfile.ChasingTargetConfig;
    }
}
[Serializable]
public class ChasingTargetToAttackingTargetTransition : AIBotStateTransition
{
    [Serializable]
    public struct Config
    {
        public float maxTimeDealDamage;
        public float maxTimeNotCauseOrDealDamage;
    }

    private ChasingTargetState chasingTargetState;

    private Config GetConfig()
    {
        return chasingTargetState.GetConfig().attackingTargetTransitionConfig;
    }

    protected override bool Decide()
    {
        // It has reached the chasing target
        if (chasingTargetState.IsReachTarget() && botController.Target.GetPointType() == PointType.OpponentPoint)
        {
            botController.NotifyEventTargetReached(botController.Target);
            Log($"ChasingTarget->AttackingTarget because bot has reached the target");
            return true;
        }
        // Any robot deals damage to me while I'm chasing other
        if (botController.RadarDetector.IsEnabled
        && !botController.Robot.IsDead
        && botController.Robot.LastRobotCauseDamageToMe != null
        && botController.Robot.LastRobotCauseDamageToMe as INavigationPoint != botController.Target
        && Time.time - botController.Robot.LastTimeReceiveDamage < GetConfig().maxTimeDealDamage
        && botController.Robot.TeamId != botController.Robot.LastRobotCauseDamageToMe.TeamId)
        {
            Log($"ChasingTarget->AttackingTarget because {botController.Robot.LastRobotCauseDamageToMe} attacked me at {botController.Robot.LastTimeReceiveDamage} - {Time.time - botController.Robot.LastTimeReceiveDamage}");
            botController.Target = botController.Robot.LastRobotCauseDamageToMe;
            return true;
        }
        // Any robot inside radar (AI sight) & robot doesn't take any damage or cause damage for a long time (X seconds) -> change to attack this target
        if (botController.RadarDetector.TryScanRobotInDetectArea(out PBRobot detectedRobot)
        && Time.time - botController.Robot.LastTimeReceiveDamage >= GetConfig().maxTimeNotCauseOrDealDamage
        && Time.time - botController.Robot.LastTimeCauseDamage >= GetConfig().maxTimeNotCauseOrDealDamage
        && botController.Target != detectedRobot as INavigationPoint
        && botController.Robot.TeamId != detectedRobot.TeamId)
        {
            Log($"ChasingTarget->AttackingTarget because detect {detectedRobot} in area and robot doesn't take any damage or cause damage for a long time");
            botController.Target = detectedRobot;
            return true;
        }
        return false;
    }

    public override void InitializeTransition(AIBotState originState, AIBotController botController)
    {
        base.InitializeTransition(originState, botController);
        chasingTargetState = GetOriginStateAsType<ChasingTargetState>();
    }
}
[Serializable]
public class ChasingTargetToLookingForTargetTransition : AIBotStateTransition
{
    [Serializable]
    public struct Config
    {
        public float maxUnreachableTimeToFindNewTarget;
    }

    private ChasingTargetState chasingTargetState;

    private Config GetConfig()
    {
        return chasingTargetState.GetConfig().lookingForTargetTransitionConfig;
    }

    protected override bool Decide()
    {
        // Target is not available anymore (disappeared or died)
        if (botController.Target == null || !botController.Target.IsAvailable())
        {
            Log($"ChasingTarget->LookinForTarget because the target is not available anymore (disappeared or died)");
            return true;
        }
        // It has reached the chasing target
        if (chasingTargetState.IsReachTarget() && botController.Target.GetPointType() != PointType.OpponentPoint)
        {
            botController.NotifyEventTargetReached(botController.Target);
            Log($"ChasingTarget->LookinForTarget because bot has reached the target");
            return true;
        }
        // Target is unreachable for X secs
        if (botController.TotalTimeNotFindPathToTarget >= GetConfig().maxUnreachableTimeToFindNewTarget)
        {
            Log($"ChasingTarget->LookinForTarget because bot can not find any path to target in {botController.TotalTimeNotFindPathToTarget} secs");
            return true;
        }
        return false;
    }

    public override void InitializeTransition(AIBotState originState, AIBotController botController)
    {
        base.InitializeTransition(originState, botController);
        chasingTargetState = GetOriginStateAsType<ChasingTargetState>();
    }
}
[Serializable]
public class ChasingTargetToReversingTransition : AIBotStateTransition
{
    private ChasingTargetState chasingTargetState;

    private bool IsCarStandstill()
    {
        return botController.CarPhysics.IsStandstill()
        && Vector3.Distance(botController.CarPhysics.PreviousOneSecPosition, botController.Robot.GetTargetPoint()) < 0.2f
        && !chasingTargetState.IsOnDynamicGround(botController.Robot.GetTargetPoint());
    }

    protected override bool Decide()
    {
        // Robot is standstill
        if (IsCarStandstill() && !botController.IsAnyGapDetected() && !botController.IsAnyObstacleDetected())
        {
            Log($"ChasingTarget->Reversing because bot is standstill");
            return true;
        }
        return false;
    }

    public override void InitializeTransition(AIBotState originState, AIBotController botController)
    {
        base.InitializeTransition(originState, botController);
        chasingTargetState = GetOriginStateAsType<ChasingTargetState>();
    }
}