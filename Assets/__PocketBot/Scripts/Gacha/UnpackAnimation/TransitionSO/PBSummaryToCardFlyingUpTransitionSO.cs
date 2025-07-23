using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames.EditableStateMachine;
using LatteGames.StateMachine;
using LatteGames.UnpackAnimation;
using UnityEngine;

[CreateAssetMenu(fileName = "PB_Summary - CardFlyingUpTransitionSO", menuName = "PocketBots/Gacha/UnpackAnimation/TransitionSO/PBSummaryToCardFlyingUpTransitionSO")]
public class PBSummaryToCardFlyingUpTransitionSO : TransitionSO
{
    readonly SummaryToCardFlyingUpEvent summaryEvent = new();
    public override StateMachine.State.Transition Transition
    {
        get
        {
            if (transition == null)
            {
                transition = new StateMachine.State.Transition(summaryEvent, targetState.State);
            }
            return base.Transition;
        }
    }

    public override void SetupTransition(object[] parameters)
    {
        if (parameters[0] is not OpenPackAnimationSM) return;
        summaryEvent.controller = (OpenPackAnimationSM)parameters[0];
        summaryEvent.summaryUI = (SummaryUI)parameters[1];
        summaryEvent.minShowingCardTime = (float)Convert.ToDouble(parameters[2]);
    }

    protected class SummaryToCardFlyingUpEvent : OpenPackAnimationSM.MouseClickEvent
    {
        public SummaryUI summaryUI;
        public float minShowingCardTime = 0.5f;

        protected float elapsedTime = 0f;

        protected virtual bool summaryShown => summaryUI != null && summaryUI.gameObject.activeInHierarchy;
        protected virtual bool matchCondition => controller != null && controller.CurrentGroupedCards[^1].isBonusCard && controller.RemainingItemAmount > 0;

        public override void Enable()
        {
            elapsedTime = 0;
            base.Enable();
        }

        public override void Update()
        {
            if (summaryShown == true)
            {
                elapsedTime += Time.deltaTime;
            }
        }

        protected override void HandleMouseClicked()
        {
            if (elapsedTime < minShowingCardTime) return;
            if (matchCondition == false) return;
            base.HandleMouseClicked();
        }
    }
}