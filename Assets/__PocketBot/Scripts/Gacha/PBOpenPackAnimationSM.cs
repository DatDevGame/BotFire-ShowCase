using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using LatteGames.UnpackAnimation;
using UnityEngine;
using HyrphusQ.Events;
using System;
using System.Linq;

public class PBOpenPackAnimationSM : OpenPackAnimationSM
{
    [SerializeField]
    protected BonusCardUI bonusCardUI;
    [SerializeField]
    protected BonusCardDataSO bonusCardConfigSO;

    protected bool isShowBonusCard;

    public BonusCardUI BonusCardUI => bonusCardUI;

    private DrawCardProcedure.PartCard m_PartCard;

    protected override void HandleGachaPackList(List<GachaPack> gachaPacks, CardPlace cardPlace)
    {
        foreach (var gachaPack in gachaPacks)
        {
            var cards = gachaPack.GenerateCards();
            HandleGachaCardList(cards, cardPlace, gachaPack);
        }
    }

    protected override void HandleGachaCardList(List<GachaCard> gachaCards, CardPlace cardPlace, GachaPack gachaPack = null)
    {
        if (gachaCards.Count <= 0)
        {
            return;
        }

        Dictionary<GachaCard_ActiveSkill, int> skillCardDictionary = new Dictionary<GachaCard_ActiveSkill, int>();
        foreach (var card in gachaCards)
        {
            #region Resource Events
            if (gachaPack != null)
            {
                string fullName = gachaPack.GetDisplayName();
                string[] parts = fullName.Split(' ');
                string foundInArean = parts[parts.Length - 1];
                name = parts[0];
                if (card is GachaCard_Currency gachaCard_Currency)
                    gachaCard_Currency.ResourceLocationProvider = new ResourceLocationProvider(ResourceLocation.Box, $"Box_{name}_A{foundInArean}");
            }
            #endregion
            card.GrantReward();

            #region Log Source Skill Card
            try
            {
                if (card is GachaCard_ActiveSkill activeSkill)
                {
                    if (!skillCardDictionary.ContainsKey(activeSkill))
                    {
                        skillCardDictionary.Add(activeSkill, 1);
                    }
                    else
                    {
                        skillCardDictionary[activeSkill]++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            
            #endregion
        }
        #region Log Source Skill Card
        try
        {
            ResourceLocationProvider resourceLocationProvider = null;
            float skillCardCount = 0;
            string skillName = "";
            if (gachaPack != null)
            {
                //Box ID
                string fullName = gachaPack.GetDisplayName();
                string[] parts = fullName.Split(' ');
                string foundInArean = parts[parts.Length - 1];
                name = parts[0];


                resourceLocationProvider = new ResourceLocationProvider(ResourceLocation.Box, $"Box_{name}_A{foundInArean}");

                for (int i = 0; i < skillCardDictionary.Count; i++)
                {
                    skillCardCount += skillCardDictionary.ElementAt(i).Value;

                    if(skillName == "")
                        skillName = skillCardDictionary.ElementAt(i).Key.GachaItemSO.Cast<ActiveSkillSO>().GetDisplayName();
                }
            }

            if (skillName != "" && skillCardCount > 0)
                GameEventHandler.Invoke(LogSinkSource.SkillCard, skillCardCount, resourceLocationProvider);

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion

        var duplicateGachaCardsGroups = gachaCards.GroupDuplicate();
        if (hasBonusCard && isShowBonusCard && cardPlace == CardPlace.NormalPack)
        {
            if (bonusCardConfigSO.IsAbleToShowBonusCard())
            {
                var refGachaPack = (gachaPack is PBManualGachaPack manualGachPack) ? manualGachPack.SimulationFromGachaPack : gachaPack.Cast<PBGachaPack>();
                var bonusCard = DrawCardProcedure.Instance.GenerateBonusCard(refGachaPack);
                if (bonusCard != null)
                {
                    m_PartCard = bonusCard;
                    var gachaCardsGroup = duplicateGachaCardsGroups.Find(item => item.representativeCard is GachaCard_Part);
                    if (gachaCardsGroup == null)
                    {
                        throw new System.Exception("Fails to find any gacha cards group contains card of type GachaCard_Part???");
                    }
                    var gachaCard = gachaCardsGroup.representativeCard.Clone(bonusCard.partSO);
                    duplicateGachaCardsGroups.Add(new DuplicateGachaCardsGroup()
                    {
                        isBonusCard = true,
                        cardsAmount = bonusCard.numOfCards,
                        representativeCard = gachaCard
                    });

                    #region Design Events
                    string cardGroup = m_PartCard.groupType switch
                    {
                        DrawCardProcedure.GroupType.InUsed => "InUsedGroup",
                        DrawCardProcedure.GroupType.Duplicate => "DuplicateGroup",
                        DrawCardProcedure.GroupType.NewAvailable => "AvailableGroup",
                        _ => "AvailableGroup"
                    };
                    string rvName = $"BonusCard_{cardGroup}";
                    string location = "OpenBoxUI";
                    GameEventHandler.Invoke(DesignEvent.RVShow);
                    #endregion
                }
            }
            else
            {
                bonusCardConfigSO.numOfOpenedBoxesVar.value++;
            }
        }
        subPackInfos.Add(new SubPackInfo()
        {
            duplicateGachaCardsGroups = duplicateGachaCardsGroups,
            cardPlace = cardPlace,
            gachaPack = gachaPack
        });
    }

    public override void UpdateRemainCardText()
    {
        remainingCardAmountText.text = CurrentGroupedCards[^1].isBonusCard ? Mathf.Max(RemainingItemAmount - 1, 0).ToString() : RemainingItemAmount.ToString();
    }

    public override void Unpack(object[] parameters)
    {
        bonusCardUI.ClaimRVButton.OnRewardGranted += ClaimRVButton_OnRewardGranted;
        isShowBonusCard = parameters.Length >= 4 ? (bool)parameters[3] : false;
        base.Unpack(parameters);
    }

    public override void StopStateMachine()
    {
        bonusCardUI.ClaimRVButton.OnRewardGranted -= ClaimRVButton_OnRewardGranted;
        base.StopStateMachine();
    }

    private void ClaimRVButton_OnRewardGranted(LatteGames.Monetization.RVButtonBehavior.RewardGrantedEventData obj)
    {
        #region MonetizationEventCode
        if (m_PartCard == null) return;
        string location = "OpenBoxUI";
        GameEventHandler.Invoke(MonetizationEventCode.BonusCard_OpenBoxUI, location, m_PartCard.groupType);
        #endregion
    }
}
