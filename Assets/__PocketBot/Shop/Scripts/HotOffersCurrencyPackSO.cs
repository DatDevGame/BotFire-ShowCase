using System;
using System.Collections;
using System.Collections.Generic;
using PackReward;
using Sirenix.OdinInspector;
using UnityEngine;

public class HotOffersCurrencyPackSO : PackRewardSO
{
    [SerializeField, BoxGroup("Data")] protected PBPvPTournamentSO m_PBPvPTournamentSO;
    [SerializeField, BoxGroup("Data")] protected CurrentHighestArenaVariable m_CurrentHighestArenaVariable;

    public virtual int GetReward()
    {
        return 99;
    }
    protected override void AcquireCurrency(CurrencySO currency, ResourceLocationProvider resourceLocationProvider)
    {
        if (m_CurrentHighestArenaVariable == null) return;
        if (resourceLocationProvider != null)
        {
            currency.Acquire(GetReward(), resourceLocationProvider.GetLocation(), resourceLocationProvider.GetItemId());
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
