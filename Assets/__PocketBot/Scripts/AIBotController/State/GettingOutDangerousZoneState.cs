using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class GettingOutDangerousZoneState : AIBotState
{
    [Serializable]
    public struct Config
    {
        public GettingOutDangerousZoneToAttackingTargetTransition.Config attackingTargetTransitionConfig;
    }

    private const float VectorBehindAngles = 120f;

    [ShowInInspector, ReadOnly]
    private float lastTimeInDangerousZone;
    [ShowInInspector, ReadOnly]
    private Vector3 safePoint = default;

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (BotController == null)
            return;
        if (safePoint != default)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(safePoint, 0.5f);
        }
    }

    protected override void OnStateEnable()
    {
        base.OnStateEnable();
        lastTimeInDangerousZone = Time.time;
    }

    protected override void OnStateUpdate()
    {
        if (BotController.FrameCount % 2 != (ulong)((31 - BotController.Robot.RobotLayer) / 3))
            return;
        base.OnStateUpdate();
        var carPhysics = BotController.CarPhysics;
        var carRight = BotController.Robot.ChassisInstanceTransform.right;
        var carForward = BotController.Robot.ChassisInstanceTransform.forward;
        var currentPosition = BotController.Robot.GetTargetPoint();
        var currentRotation = BotController.Robot.ChassisInstanceTransform.rotation;
        var targetPosition = BotController.Target.GetTargetPoint();
        var isTargetUpsideDown = (BotController.Target as PBRobot).ChassisInstance.CarPhysics.IsUpsideDown();
        if (!isTargetUpsideDown
        && BotController.IsInDangerousZone(out List<Vector3> reachablePoints, VectorBehindAngles)
        && BotController.TryFindTheSafePoint(reachablePoints, out safePoint)
        && NavMeshHelper.CalcNumOfReachablePoints(currentPosition, currentRotation, 30f, filterPredicate: point => BotController.IsReachableNavMesh(currentPosition, point, false)) < NavMeshHelper.CalcNumOfReachablePoints(safePoint, currentRotation, 30f, filterPredicate: point => BotController.IsReachableNavMesh(safePoint, point, false)))
        {
            lastTimeInDangerousZone = Time.time;
            var movingDir = safePoint - currentPosition;
            var normalizedMovingDir = movingDir.normalized;
            var accelInputSign = Vector3.Dot(normalizedMovingDir, carForward) >= Mathf.Cos(90f * Mathf.Deg2Rad) ? 1f : -1f;
            carPhysics.AccelInput = GetMovingSpeed() * accelInputSign;
            carPhysics.RotationInput = Vector3.Dot(carRight, normalizedMovingDir * accelInputSign);
            carPhysics.SteeringSpeedMultiplier = GetSteeringSpeed();
            carPhysics.TurningMultiplier = GetSteeringSpeed();
            carPhysics.InputDir = movingDir * accelInputSign;
        }
    }

    private float GetMovingSpeed()
    {
        return Const.FloatValue.OneF;
    }

    private float GetSteeringSpeed()
    {
        return Const.FloatValue.OneF;
    }

    public float GetTotalTimeInSafeZone()
    {
        return Time.time - lastTimeInDangerousZone;
    }

    public Config GetConfig()
    {
        return BotController.AIProfile.GettingOutDangerousZoneConfig;
    }
}

[Serializable]
public class GettingOutDangerousZoneToAttackingTargetTransition : AIBotStateTransition
{
    [Serializable]
    public struct Config
    {
        public float maxTimeBackToAttack;
    }

    private GettingOutDangerousZoneState getOutDangerousZoneState;
    private AttackingTargetState attackingState;

    private Config GetConfig()
    {
        return getOutDangerousZoneState.GetConfig().attackingTargetTransitionConfig;
    }

    protected override bool Decide()
    {
        // I'm standing in safe area in 1 seconds
        if (getOutDangerousZoneState.GetTotalTimeInSafeZone() >= GetConfig().maxTimeBackToAttack)
        {
            Log($"GettingOutDangerousZone->AttackingTarget because I'm standing in safe zone in {getOutDangerousZoneState.GetTotalTimeInSafeZone()} seconds - {Time.time}");
            return true;
        }
        if (attackingState.IsTargetOutOfAttackRange() && Vector3.Distance(botController.Robot.GetTargetPoint(), botController.Target.GetTargetPoint()) >= 15f)
        {
            Log($"GettingOutDangerousZone->AttackingTarget because target is out of target range - {Time.time}");
            return true;
        }
        return false;
    }

    public override void InitializeTransition(AIBotState originState, AIBotController botController)
    {
        base.InitializeTransition(originState, botController);
        getOutDangerousZoneState = GetOriginStateAsType<GettingOutDangerousZoneState>();
        attackingState = GetTargetStateAsType<AttackingTargetState>();
    }
}

[Serializable]
public class GettingOutDangerousZoneToLookingForTargetTransition : AIBotStateTransition
{
    protected override bool Decide()
    {
        // Target died
        if (!botController.Target.IsAvailable())
        {
            Log($"GettingOutDangerousZone->LookingForTarget because target died");
            return true;
        }
        return false;
    }
}