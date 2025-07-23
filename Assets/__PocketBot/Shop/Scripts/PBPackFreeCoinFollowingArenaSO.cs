using System;
using System.Collections;
using System.Collections.Generic;
using HightLightDebug;
using PackReward;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "PBPackFreeCoinFollowingArenaSO", menuName = "PocketBots/PackReward/PBPackFreeCoinFollowingArenaSO")]
public class PBPackFreeCoinFollowingArenaSO : PackRewardSO
{
    [SerializeField, BoxGroup("Config")] protected List<int> m_MultipWinArenas;
    [SerializeField, BoxGroup("Data")] protected PBPvPTournamentSO m_PBPvPTournamentSO;
    [SerializeField, BoxGroup("Data")] protected CurrentHighestArenaVariable m_CurrentHighestArenaVariable;
    public virtual int GetRewardArena()
    {
        if (m_PBPvPTournamentSO != null)
        {
            if (m_MultipWinArenas.Count == m_PBPvPTournamentSO.arenas.Count)
            {
                return (m_CurrentHighestArenaVariable.value as PBPvPArenaSO).WonNumOfCoins * m_MultipWinArenas[m_CurrentHighestArenaVariable.value.index];
            }
        }
        return 1;
    }
    protected override void AcquireCurrency(CurrencySO currency, ResourceLocationProvider resourceLocationProvider)
    {
        if (m_CurrentHighestArenaVariable == null) return;
        if (resourceLocationProvider != null)
        {
            currency.Acquire(GetRewardArena(), resourceLocationProvider.GetLocation(), resourceLocationProvider.GetItemId());
        }
        else
        {
            currency.AcquireWithoutLogEvent(CurrencyPack.Value);
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
            "<size={0}>{1:00}<size={2}>MIN <size={0}>{3:00}<size={2}>S",
            timeValueFontSize, interval.Minutes, labelFontSize, interval.Seconds
        );

        string hourAndMinutes = string.Format(
            "<size={0}>{1:00}<size={2}>H <size={0}>{3:00}<size={2}>MIN",
            timeValueFontSize, interval.Hours, labelFontSize, interval.Minutes
        );

        string handleTextTimeSpanAds = interval.TotalMinutes >= 60 ? hourAndMinutes : minuteAndSeconds;

        return handleTextTimeSpanAds;
    }


#if UNITY_EDITOR
[ShowInInspector, ReadOnly, BoxGroup("Config"), LabelText("Arena Coins Won")] 
protected string m_CoinWinArena => $"Arena {m_CurrentHighestArenaVariable.value.index + 1}: {(m_CurrentHighestArenaVariable.value as PBPvPArenaSO).WonNumOfCoins}";

[ShowInInspector, ReadOnly, BoxGroup("Config"), LabelText("Total Free Coin Reward Following Arena")]


protected int m_TotalRewardFreeCoinFollowingArena => GetRewardArena();

[OnInspectorGUI]
protected void LoadMultipArenas()
    {
        if (m_PBPvPTournamentSO == null) return;
        if (m_MultipWinArenas == null)
            m_MultipWinArenas = new List<int>();

        for (int i = 0; i < m_PBPvPTournamentSO.arenas.Count; i++)
        {
            if (m_MultipWinArenas.Count <= m_PBPvPTournamentSO.arenas.Count)
            {
                m_MultipWinArenas.Add(0);
            }
        }
        if (m_MultipWinArenas.Count > m_PBPvPTournamentSO.arenas.Count)
            m_MultipWinArenas.RemoveAt(m_MultipWinArenas.Count - 1);
    }

#endif
}
