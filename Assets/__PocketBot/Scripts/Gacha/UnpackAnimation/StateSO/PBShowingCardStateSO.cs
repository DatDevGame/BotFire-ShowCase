using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using LatteGames.Monetization;
using LatteGames.UnpackAnimation;
using UnityEngine;

[CreateAssetMenu(fileName = "PBShowingCardStateSO", menuName = "PocketBots/Gacha/UnpackAnimation/StateSO/PBShowingCardStateSO")]
public class PBShowingCardStateSO : ShowingCardStateSO
{
    const int MAX_REROLL_TIMES = 1;
    public static Action OnSkipClicked;

    [SerializeField] protected BonusCardDataSO bonusCardDataSO;

    protected PBGachaPack refGachaPack;
    protected DrawCardProcedure.PartCard nextBonusCard;
    protected List<int> remainingRerollTimes = new List<int>();

    protected int RemainingRerollTime
    {
        get
        {
            if (remainingRerollTimes.Count <= 0)
            {
                for (int i = 0; i < controller.subPackInfos.Count; i++)
                {
                    remainingRerollTimes.Add(MAX_REROLL_TIMES);
                }
            }
            return remainingRerollTimes[controller.CurrentSubPackIndex];
        }
        set
        {
            if (remainingRerollTimes.IsValidIndex(controller.CurrentSubPackIndex))
                remainingRerollTimes[controller.CurrentSubPackIndex] = value;
        }
    }

    public override void SetupState(object[] parameters = null)
    {
        base.SetupState(parameters);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
    }

    protected override void StateEnable()
    {
        base.StateEnable();
        var controller = this.controller as PBOpenPackAnimationSM;
        if (controller == null || currentCard == null)
            return;
        if (currentCard.isBonusCard)
        {
            bonusCardDataSO.AddLastBonusPart(currentCard.representativeCard.Cast<GachaCard_Part>().PartSO);
            refGachaPack = (controller.CurrentGachaPack is PBManualGachaPack manualGachPack) ? manualGachPack.SimulationFromGachaPack : controller.CurrentGachaPack.Cast<PBGachaPack>();
            nextBonusCard = DrawCardProcedure.Instance.GenerateBonusCard(refGachaPack);
            tapCTAText.gameObject.SetActive(false);
            controller.BonusCardUI.Show(nextBonusCard != null && RemainingRerollTime-- > 0, bonusCardDataSO.config.delayTimeBeforeShowingButton);
            controller.BonusCardUI.RerollButton.onClick.AddListener(OnRerollButtonClicked);
            controller.BonusCardUI.SkipButton.onClick.AddListener(OnSkipButtonClicked);
            controller.BonusCardUI.ClaimRVButton.OnRewardGranted += OnRewardGranted;
            controller.BonusCardUI.ClaimRVButton.OnFailedWatchAds += OnFailedWatchAds;
        }
    }

    protected override void StateDisable()
    {
        base.StateDisable();
        var controller = this.controller as PBOpenPackAnimationSM;
        if (controller == null)
            return;
        if (currentCard.isBonusCard)
        {
            refGachaPack = null;
            nextBonusCard = null;
            controller.BonusCardUI.Hide();
            controller.BonusCardUI.RerollButton.onClick.RemoveListener(OnRerollButtonClicked);
            controller.BonusCardUI.SkipButton.onClick.RemoveListener(OnSkipButtonClicked);
            controller.BonusCardUI.ClaimRVButton.OnRewardGranted -= OnRewardGranted;
            controller.BonusCardUI.ClaimRVButton.OnFailedWatchAds -= OnFailedWatchAds;
        }
    }

    protected override void ShowCTA()
    {
        if (currentCard.isBonusCard)
            return;
        base.ShowCTA();
    }

    private void OnRerollButtonClicked()
    {
        var controller = this.controller as PBOpenPackAnimationSM;
        // Check is affordable
        if (!controller.BonusCardUI.IsAffordable())
        {
            // Show popup gem IAP
            IAPGemPackPopup.Instance?.Show();
        }
        else
        {
            // Reroll
            controller.BonusCardUI.SetActiveButtonsGroup(RemainingRerollTime > 0, false);
            controller.RemainingItemAmount = 1;
            var gachaCardsGroup = controller.CurrentGroupedCards[^1];
            gachaCardsGroup.cardsAmount = nextBonusCard.numOfCards;
            (gachaCardsGroup.representativeCard as GachaCard_Part).GachaItemSO = nextBonusCard.partSO;
            controller.BonusCardUI.Spend();
        }
    }

    private void OnSkipButtonClicked()
    {
        // Skip to next stage
        OnSkipClicked?.Invoke();
    }

    private void OnFailedWatchAds()
    {
        // Do nothing
    }

    private void OnRewardGranted(RVButtonBehavior.RewardGrantedEventData data)
    {
        var cardController = cardControllerInstance as PBCardController;
        cardController.GrantBonusCardReward(data);
        var controller = this.controller as PBOpenPackAnimationSM;
        controller.BonusCardUI.SetActiveButtonsGroup(false, false);
        controller.BonusCardUI.SkipButton.gameObject.SetActive(false);
        controller.BonusCardUI.TapToContinueText.gameObject.SetActive(true);
        bonusCardDataSO.numOfClaimedBonusCardVar.value++;
    }

    private void OnUnpackStart(object[] parameters)
    {
        remainingRerollTimes.Clear();
    }
}