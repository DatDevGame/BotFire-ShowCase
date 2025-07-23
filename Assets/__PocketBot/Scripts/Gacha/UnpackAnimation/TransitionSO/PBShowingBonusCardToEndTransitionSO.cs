using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Monetization;
using LatteGames.StateMachine;
using LatteGames.UnpackAnimation;
using UnityEngine;

[CreateAssetMenu(fileName = "PB_ShowingBonusCard - EndTransitionSO", menuName = "PocketBots/Gacha/UnpackAnimation/TransitionSO/PBShowingBonusCardToEndTransitionSO")]
public class PBShowingBonusCardToEndTransitionSO : ShowingCardToSummaryTransitionSO
{
    readonly ShowingBonusCardToEndEvent transitionEvent = new();
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

    protected class ShowingBonusCardToEndEvent : ShowingCardToSummaryEvent
    {
        internal PBOpenPackAnimationSM pbController;

        bool isBonusCard;
        bool isBonusCardRVClaimed;
        bool isSkipButtonClicked;

        public override void Enable()
        {
            base.Enable();
            isBonusCardRVClaimed = false;
            isSkipButtonClicked = false;
            isBonusCard = controller.CurrentGroupedCards[^1].isBonusCard && controller.RemainingItemAmount <= 0;
            if (isBonusCard)
            {
                pbController.BonusCardUI.SkipButton.onClick.AddListener(OnSkipButtonClicked);
                pbController.BonusCardUI.ClaimRVButton.OnRewardGranted += OnRewardGranted;
            }
        }

        public override void Disable()
        {
            base.Disable();
            if (isBonusCard)
            {
                pbController.BonusCardUI.SkipButton.onClick.RemoveListener(OnSkipButtonClicked);
                pbController.BonusCardUI.ClaimRVButton.OnRewardGranted -= OnRewardGranted;
            }
        }

        private void OnSkipButtonClicked()
        {
            isSkipButtonClicked = true;
            base.HandleMouseClicked();
        }

        private void OnRewardGranted(RVButtonBehavior.RewardGrantedEventData data)
        {
            isBonusCardRVClaimed = true;
        }

        protected override bool isMatchCondition => ((controller.CurrentGroupedCards[^1].isBonusCard && controller.RemainingItemAmount <= 0 && isBonusCardRVClaimed) || isSkipButtonClicked) && controller.IsLastSubPack;
    }
}