using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using PackReward;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "WinStreakCellSO", menuName = "PocketBots/PackReward/WinStreakCellSO")]
public class WinStreakCellSO : PackRewardSO
{
    [BoxGroup("Config")] public bool IsPremium;

    public string GachaKey => m_KeyGachaIndex;
    public string SkillKey => m_KeySkillName;

    public bool IsClaimed => PlayerPrefs.HasKey(m_KeyClaimed);
    public bool IsClaimable => PlayerPrefs.HasKey(m_KeyClaimable);
    public int GachaIndex => PlayerPrefs.GetInt(m_KeyGachaIndex, 0);
    public string SkillName => PlayerPrefs.GetString(m_KeySkillName, "");
    public ScriptWinStreak ScriptWinStreak => m_ScriptWinStreak;
    public RewardGroupInfo RewardGroupInfo => m_RewardGroupInfo;

    protected string m_KeyClaimed => $"Claimed-{m_Key}";
    protected string m_KeyClaimable=> $"Claimable-{m_Key}";
    protected string m_KeyGachaIndex=> $"GachaIndex-{m_Key}";
    protected string m_KeySkillName => $"SkillName-{m_Key}";

    protected ScriptWinStreak m_ScriptWinStreak;

    [TitleGroup("RewardGroupInfo"), SerializeField]
    private RewardGroupInfo m_RewardGroupInfo;

    public virtual void Claimable()
    {
        PlayerPrefs.SetInt(m_KeyClaimable, 1);
    }

    public virtual void Claim()
    {
        PlayerPrefs.SetInt(m_KeyClaimed, 1);
    }

    public virtual void SetGachaIndex(int index)
    {
        PlayerPrefs.SetInt(m_KeyGachaIndex, index);
    }
    public virtual void SetSkillName(string name)
    {
        PlayerPrefs.SetString(m_KeySkillName, name);
    }

    public virtual void ResetClaim()
    {
        PlayerPrefs.DeleteKey(m_KeyClaimed);
        PlayerPrefs.DeleteKey(m_KeyClaimable);
        PlayerPrefs.DeleteKey(m_KeyGachaIndex);
        PlayerPrefs.DeleteKey(m_KeySkillName);
    }

    public virtual void SetScripWinStreak(ScriptWinStreak scriptWinStreak) => m_ScriptWinStreak = scriptWinStreak;

    public void UpdateRewardGroupInfo()
    {
        if (m_RewardGroupInfo == null)
            m_RewardGroupInfo = new RewardGroupInfo();

        if (m_RewardGroupInfo.currencyItems == null)
            m_RewardGroupInfo.currencyItems = new Dictionary<CurrencyType, ShopProductSO.DiscountableValue>();

        if (m_RewardGroupInfo.generalItems == null)
            m_RewardGroupInfo.generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>();

        ShopProductSO.DiscountableValue discountableValue = new ShopProductSO.DiscountableValue();
        m_RewardGroupInfo.currencyItems.Clear();
        m_RewardGroupInfo.generalItems.Clear();
        if (m_ScriptWinStreak == ScriptWinStreak.Coin || m_ScriptWinStreak == ScriptWinStreak.Gem)
        {
            discountableValue.value = CurrencyPack.Value;
            m_RewardGroupInfo.currencyItems.Add(ScriptWinStreak == ScriptWinStreak.Coin ? CurrencyType.Standard : CurrencyType.Premium, discountableValue);
        }
        else if (m_ScriptWinStreak == ScriptWinStreak.BoxClassic || m_ScriptWinStreak == ScriptWinStreak.BoxGreat || m_ScriptWinStreak == ScriptWinStreak.BoxUltra)
        {
            discountableValue.value = ItemPack.Value;
            m_RewardGroupInfo.generalItems.Add(ItemPack.ItemSO, discountableValue);
        }
        else if (m_ScriptWinStreak == ScriptWinStreak.SkillCard)
        {
            discountableValue.value = ItemPack.Value;
            m_RewardGroupInfo.generalItems.Add(ItemPack.ItemSO, discountableValue);
        }
    }
}
