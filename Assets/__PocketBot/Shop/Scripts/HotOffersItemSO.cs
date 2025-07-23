using System;
using System.Collections;
using System.Collections.Generic;
using HightLightDebug;
using PackReward;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using HyrphusQ.Events;
using GachaSystem.Core;

[CreateAssetMenu(fileName = "HotOffersItemSO", menuName = "PocketBots/PackReward/HotOffersItemSO")]
public class HotOffersItemSO : PackRewardSO
{
    public bool IsActivePack => PlayerPrefs.HasKey(m_ActiveKey);
    public bool IsClaimed => PlayerPrefs.HasKey(m_KeyClaimed);

    protected string m_KeyClaimed => $"Claimed-{m_Key}";
    protected string m_ActiveKey => $"Active-{m_Key}";

    public float BonusRateCardProcedure => m_BonusRateCardProcedure;
    public float NewCardRateCardProcedure => m_NewCardRateCardProcedure;
    [SerializeField, BoxGroup("Config")] protected ResourceLocationProvider m_SinkResource;
    [SerializeField, BoxGroup("Config DrawCardProcedure")] protected float m_BonusRateCardProcedure, m_NewCardRateCardProcedure;

    public override void ActivePack()
    {
        if (!PlayerPrefs.HasKey(m_ActiveKey))
        {
            PlayerPrefs.SetInt(m_ActiveKey, 1);
            ResetReward();
            ResetNow();
        }
    }

    public void ResetPack()
    {
        PlayerPrefs.DeleteKey(m_KeyClaimed);
    }

    public virtual void Claim(ResourceLocationProvider resourceLocationProvider, DrawCardProcedure.PartCard partCard)
    {
        if (!IsClaim())
        {
            DebugPro.RedBold("You Can Not Claim!");
            return;
        }

        SpendResource(resourceLocationProvider);
        AcquireResource(resourceLocationProvider, partCard);
        ResetReward();
        NotifyMissionEvent();
        if (!PlayerPrefs.HasKey(m_KeyClaimed))
            PlayerPrefs.SetInt(m_KeyClaimed, 1);
    }

    public virtual void Claim(ResourceLocationProvider resourceLocationProvider, ResourceLocationProvider sinkLocationProvider, DrawCardProcedure.PartCard partCard)
    {
        if (!IsClaim())
        {
            DebugPro.RedBold("You Can Not Claim!");
            return;
        }
        SpendResource(sinkLocationProvider);
        AcquireResource(resourceLocationProvider, partCard);
        ResetReward();
        NotifyMissionEvent();
        if (!PlayerPrefs.HasKey(m_KeyClaimed))
            PlayerPrefs.SetInt(m_KeyClaimed, 1);
    }

    private void NotifyMissionEvent()
    {
        GameEventHandler.Invoke(MissionEventCode.OnPlayerPickedUpCollectable);
    }

    protected virtual void AcquireResource(ResourceLocationProvider resourceLocationProvider, DrawCardProcedure.PartCard partCard)
    {
        switch (PackType)
        {
            case PackType.Currency:
                HandleCurrencyAcquisition(resourceLocationProvider);
                break;

            case PackType.Item:
                ProcessRewardGroup(partCard, resourceLocationProvider);
                break;

            default:
                DebugPro.RedBold("Invalid PackType!");
                break;
        }
    }

    public void ProcessRewardGroup(DrawCardProcedure.PartCard partCard, IResourceLocationProvider resourceLocationProvider)
    {
        var rewardGroupInfo = CreateRewardGroupInfo(partCard);
        List<GachaCard> cards = GenerateGachaCards(rewardGroupInfo);

        if (cards == null || cards.Count == 0) return;

        AssignResourceProvider(cards, resourceLocationProvider);

        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, cards, null, null);
    }

    private RewardGroupInfo CreateRewardGroupInfo(DrawCardProcedure.PartCard partCard)
    {
        var rewardGroupInfo = new RewardGroupInfo();
        var discountableValue = new ShopProductSO.DiscountableValue
        {
            value = partCard.numOfCards
        };

        rewardGroupInfo.generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>
        {
            { partCard.partSO, discountableValue }
        };
        return rewardGroupInfo;
    }

    private List<GachaCard> GenerateGachaCards(RewardGroupInfo rewardGroupInfo)
    {
        return (GachaCardGenerator.Instance as PBGachaCardGenerator)?.Generate(rewardGroupInfo);
    }

    private void AssignResourceProvider(List<GachaCard> cards, IResourceLocationProvider resourceLocationProvider)
    {
        foreach (var card in cards)
        {
            if (card is GachaCard_Currency gachaCardCurrency)
            {
                gachaCardCurrency.ResourceLocationProvider = resourceLocationProvider;
            }
        }
    }

    /// <summary>
    /// EX: 15MIN 20S - 15(timeValueFontSize), MIN(labelFontSize)
    /// </summary>
    /// <param name="timeValueFontSize"></param>
    /// <param name="labelFontSize"></param>
    /// <returns></returns>
    public virtual string GetRemainingTimeHandle(float timeValueFontSize, float labelFontSize)
    {
        TimeSpan interval = DateTime.Now - LastRewardTime;
        double remainingSeconds = TimeWaitingForReward - interval.TotalSeconds;
        interval = TimeSpan.FromSeconds(remainingSeconds);

        string minuteAndSeconds = string.Format(
            "<size={0}>{1:00}<size={2}>M <size={0}>{3:00}<size={2}>S",
            timeValueFontSize, interval.Minutes, labelFontSize, interval.Seconds
        );

        string hourAndMinutes = string.Format(
            "<size={0}>{1:00}<size={2}>H <size={0}>{3:00}<size={2}>M",
            timeValueFontSize, interval.Hours, labelFontSize, interval.Minutes
        );

        string handleTextTimeSpanAds = interval.TotalMinutes >= 60 ? hourAndMinutes : minuteAndSeconds;

        return handleTextTimeSpanAds;
    }

    [Button("Reset Claim Key", ButtonSizes.Large), GUIColor(0.5f, 0.9f, 0), HorizontalGroup("Action Button 2")]
    public virtual void ResetClaimKey()
    {
        PlayerPrefs.DeleteKey(m_KeyClaimed);
    }
}
