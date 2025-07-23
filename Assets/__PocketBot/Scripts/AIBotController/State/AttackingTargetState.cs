using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Helpers;
using LatteGames;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[Serializable]
public class AttackingTargetState : AIBotState
{
    [Serializable]
    public struct Config
    {
        public AnimationCurve fallOffAccelCurve;
        public AttackingTargetToChasingTargetTransition.Config chasingTargetTransitionConfig;
        public AttackingTargetToGettingOutDangerousZoneTransition.Config gettingOutDangerousZoneTransitionConfig;
        public AttackingTargetToReversingTransition.Config reversingTransitionConfig;
    }

    private const float SphereCastMaxDistance = 100f;
    private const float DefaultUpperHeight = 0.3423699f;
    private readonly Color Pink = new Vector4(255f, 23f, 212f, 255f) / 255f;

    [ShowInInspector, ReadOnly]
    private float maxDistance;
    [ShowInInspector, ReadOnly]
    private Vector3 adjustedTargetPoint = Vector3.negativeInfinity;

    [ShowInInspector, ReadOnly]
    public float TotalAttackTargetTime { get; set; }
    [ShowInInspector, ReadOnly]
    public float LastTimeCauseAnyDamage { get; set; }
    [ShowInInspector, ReadOnly]
    public float TotalTimeMyHpPercentageLowerThanOpponent { get; set; }
    [ShowInInspector, ReadOnly]
    public float LastTimeStayInSafeZone { get; set; }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (BotController == null)
            return;
        Gizmos.color = Pink;
        Gizmos.DrawRay(BotController.Robot.GetTargetPoint(), BotController.Robot.ChassisInstanceTransform.forward * maxDistance);
        Gizmos.DrawWireSphere(BotController.Robot.GetTargetPoint() + BotController.Robot.ChassisInstanceTransform.forward * maxDistance, 0.5f);
        if (adjustedTargetPoint != Vector3.negativeInfinity)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(adjustedTargetPoint, 0.5f);
        }
    }

    protected override void OnStateEnable()
    {
        base.OnStateEnable();
        TotalTimeMyHpPercentageLowerThanOpponent = Const.FloatValue.ZeroF;
        LastTimeCauseAnyDamage = BotController.Robot.LastTimeCauseDamage;
        LastTimeStayInSafeZone = Time.time;
    }

    protected override void OnStateUpdate()
    {
        base.OnStateUpdate();
        if (BotController.Robot.LastTimeCauseDamage > LastTimeCauseAnyDamage)
            LastTimeCauseAnyDamage = BotController.Robot.LastTimeCauseDamage;
        LastTimeCauseAnyDamage += Time.deltaTime;
        TotalAttackTargetTime += Time.deltaTime;

        var targetRobot = BotController.Target as PBRobot;
        var myHpPercentage = BotController.Robot.HealthPercentage;
        var targetHpPercentage = targetRobot.HealthPercentage;
        if (myHpPercentage < targetHpPercentage)
        {
            TotalTimeMyHpPercentageLowerThanOpponent += Time.deltaTime;
        }
        else
        {
            TotalTimeMyHpPercentageLowerThanOpponent = Const.FloatValue.ZeroF;
        }

        // Move to target
        var carPhysics = BotController.CarPhysics;
        var carRight = BotController.Robot.ChassisInstanceTransform.right;
        var currentPosition = BotController.Robot.GetTargetPoint();
        var targetPosition = BotController.Target.GetTargetPoint();
        var movingDir = targetPosition - currentPosition;
        carPhysics.AccelInput = GetMovingSpeed() * CalcAccelInputFromDistance(movingDir);
        carPhysics.RotationInput = Vector3.Dot(carRight, movingDir.normalized);
        carPhysics.SteeringSpeedMultiplier = GetSteeringSpeed();
        carPhysics.TurningMultiplier = GetSteeringSpeed();
        carPhysics.InputDir = movingDir;
    }

    private float GetMovingSpeed()
    {
        return BotController.AIProfile?.MovingSpeedWhenAttack ?? Const.FloatValue.OneF;
    }

    private float GetSteeringSpeed()
    {
        return BotController.AIProfile?.SteeringSpeedWhenAttack ?? Const.FloatValue.OneF;
    }

    private float CalcAccelInputFromDistance(Vector3 movingDir)
    {
        var distance = movingDir.magnitude;
        // If angles between my forward vector and moving dir vector bigger than 45°.
        if (Vector3.Dot(BotController.Robot.ChassisInstanceTransform.forward, movingDir.normalized) <= Mathf.Cos(45f * Mathf.Deg2Rad))
            return Mathf.Clamp01(GetConfig().fallOffAccelCurve.Evaluate(distance));
        var targetAsRobot = BotController.Target as PBRobot;
        if (Physics.SphereCast(BotController.Robot.GetTargetPoint(), 0.25f, movingDir, out var hitInfo, SphereCastMaxDistance, 1 << targetAsRobot.RobotLayer, QueryTriggerInteraction.Ignore))
        {
            distance = hitInfo.distance;
            adjustedTargetPoint = hitInfo.point;
        }
        else
        {
            adjustedTargetPoint = Vector3.negativeInfinity;
        }
        var remainedDistance = distance - maxDistance;
        if (remainedDistance > 0f || BotController.IsAnyGapDetected() || BotController.IsAnyObstacleDetected())
        {
            return Mathf.Clamp01(GetConfig().fallOffAccelCurve.Evaluate(remainedDistance));
        }
        else
        {
            return -Mathf.Clamp01(GetConfig().fallOffAccelCurve.Evaluate(Mathf.Abs(remainedDistance)));
        }
    }

    private void OnTargetChanged(INavigationPoint newTarget)
    {
        TotalAttackTargetTime = Const.FloatValue.ZeroF;
    }

    // Find part with minimum attack range
    private BotPartSlot FindOptimalSlot()
    {
        var currentChassisInUse = BotController.Robot.ChassisInstance.PartSO as PBChassisSO;
        var optimalSlot = currentChassisInUse.AllPartSlots
        .Where(slot => slot.PartVariableSO.value?.GetModule<AttackRangeModule>() != null)
        .OrderBy(slot => slot.PartVariableSO.value.GetModule<AttackRangeModule>().AttackRange)
        //.OrderByDescending(slot => slot.PartVariableSO.value.Cast<PBPartSO>().CalCurrentAttack())
        .FirstOrDefault();
        return optimalSlot;
    }

    private float CalcMaxDistanceAttackRangeOfPart(BotPartSlot partSlot)
    {
        var distance = Const.FloatValue.ZeroF;
        if (partSlot.PartVariableSO != null)
        {
            var chassisTransform = BotController.Robot.ChassisInstanceTransform;
            var partContainerTransform = BotController.Robot.ChassisInstance.PartContainers.FirstOrDefault(slotContainer => slotContainer.PartSlotType == partSlot.PartSlotType).Containers[0];
            var partContainerLocalPos = chassisTransform.InverseTransformPoint(partContainerTransform.transform.position);
            if (partSlot.PartVariableSO.value.TryGetModule(out AttackRangeModule attackRangeModule))
            {
                distance = partContainerLocalPos.z + partSlot.PartVariableSO.value.GetModule<AttackRangeModule>().AttackRange;
                if (attackRangeModule.IsInfinityAttackRange)
                {
                    distance += Mathf.Tan(attackRangeModule.AttackAngles * Mathf.Deg2Rad) * (partContainerLocalPos.y - DefaultUpperHeight);
                    Log($"Robot {BotController.Robot} - Part {partSlot.PartVariableSO.value} - LocalPos {partContainerLocalPos} - HeightDiff {partContainerLocalPos.y - DefaultUpperHeight} - Tan {Mathf.Tan(attackRangeModule.AttackAngles * Mathf.Deg2Rad)}");
                }
            }
        }
        return distance;
    }

    public override void InitializeState(AIBotController botController)
    {
        base.InitializeState(botController);
        maxDistance = CalcMaxDistanceAttackRangeOfPart(FindOptimalSlot());
        BotController.onTargetChanged += OnTargetChanged;
        BotController.RadarDetector.MaxRadius = Mathf.Max(maxDistance + Const.FloatValue.OneF, BotController.RadarDetector.MaxRadius);
    }

    public Config GetConfig()
    {
        return BotController.AIProfile.AttackingTargetConfig;
    }

    public bool IsTargetOutOfAttackRange()
    {
        var isRobotReachedTarget = BotController.Target.IsRobotReached(BotController.Robot);
        var layers = Physics.DefaultRaycastLayers
        ^ (1 << BotController.Robot.RobotLayer)
        ^ (1 << (BotController.Target as PBRobot).RobotLayer)
        ^ BotController.CarPhysics.RaycastMask;
        return !isRobotReachedTarget || BotController.IsOverlapAnyThing(BotController.Robot.GetTargetPoint(), BotController.Target.GetTargetPoint(), layers, isVisualize: true);
    }
}
[Serializable]
public class AttackingTargetToLookingForTargetTransition : AIBotStateTransition
{
    // [SerializeField]
    // private float disableRadarDuration = 5f;
    // [SerializeField]
    // private float waitingTimeToFindNewTarget = 10f;

    private AttackingTargetState attackingTargetState;

    protected override bool Decide()
    {
        // Robot target is null or died
        if (botController.Target == null || !botController.Target.IsAvailable())
        {
            Log($"AttackingTarget->LookingForTarget because robot target died");
            return true;
        }
        // Wait for X seconds -> then pick a new target & disasble radar
        // if (attackingTargetState.GetTotalAttackTargetTime() > waitingTimeToFindNewTarget)
        // {
        //     attackingTargetState.SetTotalAttackTargetTime(Const.FloatValue.ZeroF);
        //     botController.RadarDetector.DisableInDuration(disableRadarDuration);
        //     return true;
        // }
        return false;
    }

    public override void InitializeTransition(AIBotState originState, AIBotController botController)
    {
        base.InitializeTransition(originState, botController);
        attackingTargetState = GetOriginStateAsType<AttackingTargetState>();
    }
}
[Serializable]
public class AttackingTargetToGettingOutDangerousZoneTransition : AIBotStateTransition
{
    [Serializable]
    public struct Config
    {
        public float maxTimeStayingInDangerousZone;
        [Range(0f, 1f)]
        public float getOutOfDangerousZoneProbability;
    }

    private AttackingTargetState attackingTargetState;

    private float GetMaxTimeStayingInDangerousZone()
    {
        return GetConfig().maxTimeStayingInDangerousZone;
    }

    private float GetOutOfDangerousZoneProbability()
    {
        return GetConfig().getOutOfDangerousZoneProbability;
    }

    private Config GetConfig()
    {
        return attackingTargetState.GetConfig().gettingOutDangerousZoneTransitionConfig;
    }

    protected override bool Decide()
    {
        if (botController.FrameCount % 2 != (ulong)((31 - botController.Robot.RobotLayer) / 3))
            return false;
        if (GetOutOfDangerousZoneProbability() <= 0f)
            return false;
        var currentPosition = botController.Robot.GetTargetPoint();
        var currentRotation = botController.Robot.ChassisInstanceTransform.rotation;
        var isTargetUpsideDown = (botController.Target as PBRobot).ChassisInstance.CarPhysics.IsUpsideDown();
        // I'm standing in dangerous zone
        if (!isTargetUpsideDown
        && !botController.IsUsingSkill()
        && botController.IsInDangerousZone(out List<Vector3> reachablePoints)
        && botController.TryFindTheSafePoint(reachablePoints, out Vector3 safePoint)
        && NavMeshHelper.CalcNumOfReachablePoints(currentPosition, currentRotation, 30f, filterPredicate: point => botController.IsReachableNavMesh(currentPosition, point, false)) < NavMeshHelper.CalcNumOfReachablePoints(safePoint, currentRotation, 30f, filterPredicate: point => botController.IsReachableNavMesh(safePoint, point, false)))
        {
            // Log($"{botController.FrameCount} I'm standing in dangerous zone - {attackingTargetState.LastTimeStayInSafeZone} - {Time.time}");
            if (Time.time - attackingTargetState.LastTimeStayInSafeZone >= GetMaxTimeStayingInDangerousZone())
            {
                if (Random.value < GetOutOfDangerousZoneProbability())
                {
                    Log($"AttackingTarget->GettingOutDangerousZone because I'm standing in dangerous zone for {Time.time - attackingTargetState.LastTimeStayInSafeZone} seconds - {Time.time}");
                    return true;
                }
                else
                {
                    attackingTargetState.LastTimeStayInSafeZone = Time.time;
                }
            }
        }
        else
        {
            // Log($"{botController.FrameCount} I'm standing in safe zone");
            attackingTargetState.LastTimeStayInSafeZone = Time.time;
        }
        return false;
    }

    public override void InitializeTransition(AIBotState originState, AIBotController botController)
    {
        base.InitializeTransition(originState, botController);
        attackingTargetState = GetOriginStateAsType<AttackingTargetState>();
    }
}
[Serializable]
public class AttackingTargetToChasingTargetTransition : AIBotStateTransition
{
    [Serializable]
    public struct Config
    {
        public float disableRadarDuration;
        public float timeToCollectBoosterOrRunAwayThreshold;
        [Range(0f, 1f)]
        public float collectBoosterWhenLowHpProbability;
    }

    private AttackingTargetState attackingTargetState;

    private Config GetConfig()
    {
        return attackingTargetState.GetConfig().chasingTargetTransitionConfig;
    }

    protected override bool Decide()
    {
        var robotTarget = botController.Target as PBRobot;
        // Opponent is out of attack range
        if (attackingTargetState.IsTargetOutOfAttackRange())
        {
            Log($"AttackingTarget->ChasingTarget because opponent is out of attack range");
            return true;
        }
        var isOpponentUpsideDown = robotTarget.ChassisInstance.CarPhysics.IsUpsideDown();
        // Total time that my HP percentage is lower than opponent's.
        // 1. I need to collect at least one booster to reverse the situation. In order (Hp -> Atk -> Speed)
        // 2. I need to run away to random point.
        if (!isOpponentUpsideDown && !botController.IsUsingSkill() && attackingTargetState.TotalTimeMyHpPercentageLowerThanOpponent > GetConfig().timeToCollectBoosterOrRunAwayThreshold)
        {
            if (Random.value <= GetConfig().collectBoosterWhenLowHpProbability)
            {
                var boosters = PBFightingStage.Instance.GetAllNavigationPoints().Get(PointType.CollectablePoint)?.Where(item => item is Booster && item.IsAvailable()).Select(item => item as Booster).ToList();
                if (boosters != null && boosters.Count > 0)
                {
                    var sortedBoosters = boosters.OrderBy(x => BoosterTypeToWeight(x.GetBoosterType())).ThenBy(item => Vector3.Distance(item.GetTargetPoint(), botController.Robot.GetTargetPoint()));
                    var chosenBooster = sortedBoosters.FirstOrDefault(booster => botController.IsAbleToReach(booster));
                    if (chosenBooster != null)
                    {
                        botController.RadarDetector.DisableInDuration(GetConfig().disableRadarDuration);
                        botController.Target = chosenBooster;
                        botController.onTargetReached += OnTargetReached;
                        Log($"AttackingTarget->ChasingTarget because my HP percentage is lower than opponent's for {GetConfig().timeToCollectBoosterOrRunAwayThreshold} secs");
                        return true;

                        void OnTargetReached(INavigationPoint navPoint)
                        {
                            botController.onTargetReached -= OnTargetReached;
                            if (navPoint == chosenBooster as INavigationPoint)
                            {
                                botController.RadarDetector.IsEnabled = true;
                            }
                        }
                    }
                }

                int BoosterTypeToWeight(PvPBoosterType boosterType)
                {
                    switch (boosterType)
                    {
                        case PvPBoosterType.Hp:
                            return 0;
                        case PvPBoosterType.Atk:
                            return 1;
                        case PvPBoosterType.Speed:
                            return 2;
                        default:
                            return -1;
                    }
                }
            }
            else
            {
                attackingTargetState.TotalTimeMyHpPercentageLowerThanOpponent = 0f;
            }
        }
        return false;
    }

    public override void InitializeTransition(AIBotState originState, AIBotController botController)
    {
        base.InitializeTransition(originState, botController);
        attackingTargetState = GetOriginStateAsType<AttackingTargetState>();
    }
}
[Serializable]
public class AttackingTargetToReversingTransition : AIBotStateTransition
{
    [Serializable]
    public struct Config
    {
        public float notCauseDamageDurationThreshold;
        public float disableRadarDuration;
        [Range(0f, 180f)]
        public float anglesThresholdToChangeAttackDir;
    }

    private AttackingTargetState attackingTargetState;

    private Config GetConfig()
    {
        return attackingTargetState.GetConfig().reversingTransitionConfig;
    }

    protected override bool Decide()
    {
        // I can't deal any damage in [X] secs
        if (attackingTargetState.LastTimeCauseAnyDamage - botController.Robot.LastTimeCauseDamage >= GetConfig().notCauseDamageDurationThreshold
        && !botController.IsAnyGapDetected()
        && !botController.IsAnyObstacleDetected())
        {
            Log($"AttackingTarget->ReversingTarget because I can't deal any damage in {GetConfig().notCauseDamageDurationThreshold} secs");
            return true;
        }
        var robotTarget = botController.Target as PBRobot;
        var carPhysicsTarget = robotTarget.ChassisInstance.CarPhysics;
        var isOpponentUpsideDown = botController.CarPhysics.IsUpsideDown();
        // Opponent is upside down -> then try to prevent to attack in front of robot
        if (isOpponentUpsideDown && Vector3.Dot(robotTarget.ChassisInstanceTransform.forward, (botController.Robot.GetTargetPoint() - robotTarget.GetTargetPoint()).normalized) > Mathf.Cos(GetConfig().anglesThresholdToChangeAttackDir * Mathf.Deg2Rad))
        {
            var halfDiagonalOfBounds = carPhysicsTarget.CalcDiagonalOfBounds() / 2f;
            if (NavMeshHelper.TryGetRandomReachablePointFromSource(robotTarget.ChassisInstanceTransform, out Vector3 point, maxDistance: NavMeshHelper.ReachablePointMaxDistance + halfDiagonalOfBounds, filterPredicate: FilterPointPredicate))
            {
                var normalPoint = PBFightingStage.Instance.GetNormalPointPool().Get();
                normalPoint.transform.position = point;
                botController.RadarDetector.DisableInDuration(GetConfig().disableRadarDuration);
                botController.Target = normalPoint;
                botController.onTargetReached += OnTargetReached;
                Log($"AttackingTarget->ChasingTarget because opponent is upside down, then try to prevent to attack in front of robot");
                return true;

                void OnTargetReached(INavigationPoint navPoint)
                {
                    botController.onTargetReached -= OnTargetReached;
                    PBFightingStage.Instance.GetNormalPointPool().Release(normalPoint);
                }
            }
        }
        return false;
    }

    private bool FilterPointPredicate(Vector3 point)
    {
        var robotTarget = botController.Target as PBRobot;
        var robotTransform = robotTarget.ChassisInstanceTransform;
        var isOppositeDirWithEnemy = Vector3.Dot((point - robotTransform.position).normalized, robotTransform.forward) <= Mathf.Cos(GetConfig().anglesThresholdToChangeAttackDir * Mathf.Deg2Rad);
        var isSameSideWithMe = Vector3.Dot((botController.Robot.GetTargetPoint() - robotTransform.position).normalized, robotTransform.right)
        * Vector3.Dot((point - robotTransform.position).normalized, robotTransform.right) >= 0;
        return isOppositeDirWithEnemy && isSameSideWithMe;
    }

    public override void InitializeTransition(AIBotState originState, AIBotController botController)
    {
        base.InitializeTransition(originState, botController);
        attackingTargetState = GetOriginStateAsType<AttackingTargetState>();
    }
}