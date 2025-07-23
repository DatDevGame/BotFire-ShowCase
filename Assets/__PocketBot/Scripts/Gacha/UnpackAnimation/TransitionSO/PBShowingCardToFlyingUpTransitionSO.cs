using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames.EditableStateMachine;
using LatteGames.StateMachine;
using LatteGames.UnpackAnimation;
using UnityEngine;

[CreateAssetMenu(fileName = "PB_ShowingCard - CardFlyingUpTransitionSO", menuName = "PocketBots/Gacha/UnpackAnimation/TransitionSO/PBShowingCardToFlyingUpTransitionSO")]
public class PBShowingCardToFlyingUpTransitionSO : ShowingCardToCardFlyingUpTransitionSO
{
    readonly PBShowingCardToFlyingUpEvent transitionEvent = new();

    public override StateMachine.State.Transition Transition
    {
        get
        {
            if (transition == null)
            {
                transition = new StateMachine.State.Transition(transitionEvent, targetState.State);
            }
            return base.Transition;
        }
    }

    public override void SetupTransition(object[] parameters)
    {
        if (parameters[0] is not OpenPackAnimationSM) return;
        transitionEvent.controller = (OpenPackAnimationSM)parameters[0];
        transitionEvent.pbController = (PBOpenPackAnimationSM)parameters[0];
        transitionEvent.cardController = (AbstractCardController)parameters[1];
        transitionEvent.minShowingCardTime = (float)Convert.ToDouble(parameters[2]);
    }

    protected class PBShowingCardToFlyingUpEvent : ShowingCardToFlyingUpEvent
    {
        internal PBOpenPackAnimationSM pbController;
        protected virtual int stopUntilRemainingItemAmount => controller.CurrentGroupedCards[^1].isBonusCard ? 1 : 0;
        protected override bool isMatchCondition => controller.RemainingItemAmount > stopUntilRemainingItemAmount && controller.CurrentSubPackInfo.cardPlace != CardPlace.NonPack;

        public override void Enable()
        {
            base.Enable();
            pbController.BonusCardUI.onSpendSucceeded += OnSpendSucceeded;
        }

        public override void Disable()
        {
            base.Disable();
            pbController.BonusCardUI.onSpendSucceeded -= OnSpendSucceeded;
        }

        private void OnSpendSucceeded()
        {
            Trigger();
            callback?.Invoke();
        }
    }
}