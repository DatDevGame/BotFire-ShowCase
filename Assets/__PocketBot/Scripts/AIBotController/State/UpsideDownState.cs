using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpsideDownState : AIBotState
{
    [Serializable]
    public struct Config
    {
        public int maxAutoFlipTimes;
        public RangeFloatValue randomDelayTimeInSeconds;
    }

    [ShowInInspector, ReadOnly]
    private int currentAutoFlipTimes;
    [ShowInInspector, ReadOnly]
    private float upsideDownDurationToFlip;
    [ShowInInspector, ReadOnly]
    private float lastTimeEnterState;

    protected override void OnStateEnable()
    {
        base.OnStateEnable();
        lastTimeEnterState = Time.time;
        upsideDownDurationToFlip = GetConfig().randomDelayTimeInSeconds.RandomRange();
    }

    protected override void OnStateUpdate()
    {
        base.OnStateUpdate();
        var currentTime = Time.time;
        if (currentTime - lastTimeEnterState >= upsideDownDurationToFlip && IsAbleToFlip())
        {
            currentAutoFlipTimes++;
            lastTimeEnterState = Time.time;
            BotController.CarPhysics.Flip();
        }
    }

    public bool IsAbleToFlip()
    {
        return currentAutoFlipTimes < GetConfig().maxAutoFlipTimes;
    }

    public Config GetConfig()
    {
        return BotController.AIProfile.UpsideDownConfig;
    }
}
public class AnyStateToUpsideDownTransition : AIBotStateTransition
{
    private UpsideDownState upsideDownState;

    protected override bool Decide()
    {
        if (botController.CarPhysics.IsUpsideDown() && upsideDownState.IsAbleToFlip())
        {
            Log($"{botController.CurrentState}->UpsideDownState");
            return true;
        }
        return false;
    }

    public override void InitializeTransition(AIBotState originState, AIBotController botController)
    {
        base.InitializeTransition(originState, botController);
        upsideDownState = GetTargetStateAsType<UpsideDownState>();
    }
}
public class UpsideDownToChasingState : AIBotStateTransition
{
    private UpsideDownState upsideDownState;

    protected override bool Decide()
    {
        if (!botController.CarPhysics.IsUpsideDown() || !upsideDownState.IsAbleToFlip())
        {
            Log($"UpsideDown->ChasingTarget");
            return true;
        }
        return false;
    }

    public override void InitializeTransition(AIBotState originState, AIBotController botController)
    {
        base.InitializeTransition(originState, botController);
        upsideDownState = GetOriginStateAsType<UpsideDownState>();
    }
}