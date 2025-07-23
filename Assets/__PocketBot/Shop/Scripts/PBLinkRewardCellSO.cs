using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HightLightDebug;
using HyrphusQ.Events;
using PackReward;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "PBLinkRewardCellSO", menuName = "PocketBots/PackReward/PBData/PBLinkRewardCellSO")]
public class PBLinkRewardCellSO : PackRewardSO
{
    public string m_KeyClaimed => $"Claimed-{m_Key}";
    public bool IsActivePack => PlayerPrefs.HasKey(m_Key);
    public bool IsClaimed => PlayerPrefs.HasKey(m_KeyClaimed);

    [TitleGroup("RewardGroupInfo"), SerializeField]
    public RewardGroupInfo m_RewardGroupInfo;

    public override void ActivePack()
    {
        if (!PlayerPrefs.HasKey(m_Key))
        {
            PlayerPrefs.SetInt(m_Key, 1);
            ResetReward();
        }
    }

    public void ResetPack()
    {
        PlayerPrefs.DeleteKey(m_Key);
        PlayerPrefs.DeleteKey(m_KeyClaimed);
    }

    public override void Claim(ResourceLocationProvider resourceLocationProvider)
    {
        if (!IsClaim())
        {
            return;
        }

        SpendResource(resourceLocationProvider);
        AcquireResource(resourceLocationProvider);
        ResetReward();

        if (!PlayerPrefs.HasKey(m_KeyClaimed))
            PlayerPrefs.SetInt(m_KeyClaimed, 1);
    }

    public void UpdateRewardGroupInfo()
    {
        if (m_RewardGroupInfo == null)
            m_RewardGroupInfo = new RewardGroupInfo();

        if (m_RewardGroupInfo.currencyItems == null)
            m_RewardGroupInfo.currencyItems = new Dictionary<CurrencyType, ShopProductSO.DiscountableValue>();

        if (m_RewardGroupInfo.generalItems == null)
            m_RewardGroupInfo.generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>();

        ShopProductSO.DiscountableValue discountableValue = new ShopProductSO.DiscountableValue();
        if (PackType == PackType.Currency)
        {
            discountableValue.value = CurrencyPack.Value;
            if (!m_RewardGroupInfo.currencyItems.ContainsKey(CurrencyPack.CurrencyType) || !m_RewardGroupInfo.currencyItems.ContainsValue(discountableValue))
            {
                m_RewardGroupInfo.generalItems.Clear();
                m_RewardGroupInfo.currencyItems.Clear();
                m_RewardGroupInfo.currencyItems.Add(CurrencyPack.CurrencyType, discountableValue);
            }
        }
        else if (PackType == PackType.Item)
        {
            if (ItemPack.ItemSO == null) return;

            if (ItemPack.ItemSO is GachaPack)
            {
                m_RewardGroupInfo.generalItems.Clear();
                m_RewardGroupInfo.currencyItems.Clear();
                return;
            }

            discountableValue.value = ItemPack.Value;
            if (!m_RewardGroupInfo.generalItems.ContainsKey(ItemPack.ItemSO) || !m_RewardGroupInfo.generalItems.ContainsValue(discountableValue))
            {
                m_RewardGroupInfo.currencyItems.Clear();
                m_RewardGroupInfo.generalItems.Clear();
                m_RewardGroupInfo.generalItems.Add(ItemPack.ItemSO, discountableValue);
            }
        }
    }

    protected override void AcquireResource(ResourceLocationProvider resourceLocationProvider)
    {
        switch (PackType)
        {
            case PackType.Currency:
                HandleCurrencyAcquisition(resourceLocationProvider);
                break;

            case PackType.Item:
                HandleItemAcquisition(resourceLocationProvider);
                break;

            default:
                DebugPro.RedBold("Invalid PackType!");
                break;
        }
    }

    private void HandleItemAcquisition(ResourceLocationProvider resourceLocationProvider)
    {
        if (ItemPack.ItemSO is GachaPack gachaPack)
        {
            var gachaPacks = new List<GachaPack>();

            for (int i = 0; i < ItemPack.Value; i++)
                gachaPacks.Add(gachaPack as PBGachaPack);


            for (int i = 0; i < gachaPacks.Count; i++)
            {
                #region DesignEvent
                string openStatus = "NoTimer";
                string location = "LinkRewards";
                GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
                #endregion

                #region Firebase Event
                if (gachaPacks[i] != null)
                {
                    string openType = "RV";
                    GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, gachaPacks[i], openType);
                }
                #endregion
            }


            GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, null, gachaPacks, null, true);
        }
        else
        {
            var gachaCards = (GachaCardGenerator.Instance as PBGachaCardGenerator)?.Generate(m_RewardGroupInfo) ?? new List<GachaCard>();

            foreach (var card in gachaCards)
            {
                if (card is GachaCard_Currency currencyCard)
                    currencyCard.ResourceLocationProvider = resourceLocationProvider;
            }
            GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, gachaCards, null, null);
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
}
