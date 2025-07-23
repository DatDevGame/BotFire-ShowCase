using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ReversingState : AIBotState
{
    [Serializable]
    public struct Config
    {
        public ReversingToChasingTargetTransition.Config chasingTargetTransitionConfig;
        public ReversingToAttackingTargetTransition.Config attackingTagetTransitionConfig;
    }

    private float enterStateTime;

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        if (BotController == null)
            return;
    }

    protected override void OnStateEnable()
    {
        base.OnStateEnable();
        enterStateTime = Time.time;
    }

    protected override void OnStateUpdate()
    {
        base.OnStateUpdate();
        var carPhysics = BotController.CarPhysics;
        var currentPosition = BotController.Robot.GetTargetPoint();
        var targetPosition = BotController.Target.GetTargetPoint();
        var movingDir = targetPosition - currentPosition;
        carPhysics.AccelInput = -GetMovingSpeed();
        carPhysics.RotationInput = 0f;
        carPhysics.InputDir = movingDir;
    }

    private float GetMovingSpeed()
    {
        return BotController.AIProfile?.MovingSpeed ?? 1f;
    }

    private float GetSteeringSpeed()
    {
        return BotController.AIProfile?.SteeringSpeed ?? 1f;
    }

    public float GetEnterStateTime()
    {
        return enterStateTime;
    }

    public Config GetConfig()
    {
        return BotController.AIProfile.ReversingConfig;
    }
}
[Serializable]
public class ReversingToChasingTargetTransition : AIBotStateTransition
{
    [Serializable]
    public struct Config
    {
        public float reversingDuration;
    }

    private ReversingState reversingState;

    private Config GetConfig()
    {
        return reversingState.GetConfig().chasingTargetTransitionConfig;
    }

    protected override bool Decide()
    {
        if (Time.time - reversingState.GetEnterStateTime() > GetConfig().reversingDuration && botController.Target.GetPointType() != PointType.OpponentPoint)
        {
            Log("ReversingState->ChasingTargetState because timeout");
            return true;
        }
        if (botController.IsAnyGapDetected() || botController.IsAnyObstacleDetected() && botController.Target.GetPointType() != PointType.OpponentPoint)
        {
            var carPhysics = botController.CarPhysics;
            if (Mathf.Abs(carPhysics.AccelInput) != 0f)
            {
                carPhysics.AccelInput = 0f;
                carPhysics.RotationInput = 0f;
                carPhysics.Brake();
            }
            Log("ReversingState->ChasingTargetState because a gap or a obstacle has been detected");
            return true;
        }
        return false;
    }

    public override void InitializeTransition(AIBotState originState, AIBotController botController)
    {
        base.InitializeTransition(originState, botController);
        reversingState = GetOriginStateAsType<ReversingState>();
    }
}
[Serializable]
public class ReversingToAttackingTargetTransition : AIBotStateTransition
{
    [Serializable]
    public struct Config
    {
        public float reversingDuration;
    }

    private ReversingState reversingState;

    private Config GetConfig()
    {
        return reversingState.GetConfig().attackingTagetTransitionConfig;
    }

    protected override bool Decide()
    {
        if (Time.time - reversingState.GetEnterStateTime() > GetConfig().reversingDuration && botController.Target.GetPointType() == PointType.OpponentPoint)
        {
            Log("ReversingState->AttackingTargetState because timeout");
            return true;
        }
        if ((botController.IsAnyGapDetected() || botController.IsAnyObstacleDetected()) && botController.Target.GetPointType() == PointType.OpponentPoint)
        {
            var carPhysics = botController.CarPhysics;
            if (Mathf.Abs(carPhysics.AccelInput) != 0f)
            {
                carPhysics.AccelInput = 0f;
                carPhysics.RotationInput = 0f;
                carPhysics.Brake();
            }
            Log("ReversingState->AttackingTargetState because a gap or a obstacle has been detected");
            return true;
        }
        return false;
    }

    public override void InitializeTransition(AIBotState originState, AIBotController botController)
    {
        base.InitializeTransition(originState, botController);
        reversingState = GetOriginStateAsType<ReversingState>();
    }
}