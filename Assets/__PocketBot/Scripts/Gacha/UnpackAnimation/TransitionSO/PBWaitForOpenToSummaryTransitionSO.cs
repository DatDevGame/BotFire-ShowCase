using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames.UnpackAnimation;
using LatteGames.StateMachine;
using System;

[CreateAssetMenu(fileName = "PB_WaitForOpen - SummaryTransitionSO", menuName = "PocketBots/Gacha/UnpackAnimation/TransitionSO/PBWaitForOpenToSummaryTransitionSO")]
public class PBWaitForOpenToSummaryTransitionSO : WaitForOpenToSummaryTransitionSO
{
    readonly OpenPackAnimationSM.SkipEvent skipEvent = new();

    public override StateMachine.State.Transition Transition
    {
        get
        {
            if (transition == null)
            {
                transition = new StateMachine.State.Transition(skipEvent, targetState.State);
            }
            return base.Transition;
        }
    }

    public override void SetupTransition(object[] parameters)
    {
        if (parameters[0] is not OpenPackAnimationSM openPackController) return;
        skipEvent.controller = openPackController;
        skipEvent.callback += OnSkipButtonClicked;
    }

    private void OnSkipButtonClicked()
    {
        if (skipEvent == null || skipEvent.controller == null)
            return;
        skipEvent.controller.RemainingItemAmount = skipEvent.controller.CurrentGroupedCards[^1].isBonusCard ? 1 : 0;
    }
}