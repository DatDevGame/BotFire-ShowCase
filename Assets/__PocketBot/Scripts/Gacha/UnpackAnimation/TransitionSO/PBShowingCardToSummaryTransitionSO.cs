using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames.StateMachine;
using LatteGames.UnpackAnimation;
using UnityEngine;

[CreateAssetMenu(fileName = "PB_ShowingCard - SummaryTransitionSO", menuName = "PocketBots/Gacha/UnpackAnimation/TransitionSO/PBShowingCardToSummaryTransitionSO")]
public class PBShowingCardToSummaryTransitionSO : ShowingCardToSummaryTransitionSO
{
    readonly PBShowingCardToSummaryEvent showingCardEvent = new();

    public override StateMachine.State.Transition Transition
    {
        get
        {
            if (transition == null)
            {
                transition = new StateMachine.State.Transition(showingCardEvent, targetState.State);
            }
            return base.Transition;
        }
    }

    public override void SetupTransition(object[] parameters)
    {
        if (parameters[0] is not OpenPackAnimationSM) return;
        showingCardEvent.controller = (OpenPackAnimationSM)parameters[0];
        showingCardEvent.cardController = (AbstractCardController)parameters[1];
        showingCardEvent.minShowingCardTime = (float)Convert.ToDouble(parameters[2]);
    }

    protected class PBShowingCardToSummaryEvent : ShowingCardToSummaryEvent
    {
        protected virtual int stopUntilRemainingItemAmount => controller.CurrentGroupedCards[^1].isBonusCard ? 1 : 0;
        protected override bool isMatchCondition => controller.RemainingItemAmount == stopUntilRemainingItemAmount;
    }
}