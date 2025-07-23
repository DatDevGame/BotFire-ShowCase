using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeagueStartUI : ComposeCanvasElementVisibilityController
{
    [SerializeField]
    private Button m_LetsGoButton;
    [SerializeField]
    private TextMeshProUGUI m_TimeLeftText;
    [SerializeField]
    private LeagueRewardUI m_RewardUI;

    private LeagueDataSO leagueDataSO => LeagueManager.leagueDataSO;

    private void Awake()
    {
        m_LetsGoButton.onClick.AddListener(() => 
        {
            leagueDataSO.LeagueStartDatetime.value = DateTime.Now;
            leagueDataSO.LeaguePlayedTimeToReachRank.ForEach(v => v.Value.PlayedTimeToReachRank.value = DateTime.Now);
            leagueDataSO.LeaguePlayedTimeToReachRank.ForEach(v => v.Value.StartPlayTime.value = false);
            HideImmediately();

            #region Progression Event
            #region LeaguePromotion Event
            try
            {
                //Start New
                LogEventLeaguePromotion(ProgressionEventStatus.Start);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion
            #region Progression
            string status = "Start";
            string content = "League";
            GameEventHandler.Invoke(ProgressionEvent.Progression, status, content);
            #endregion
            #endregion

            #region Firebase Event
            try
            {
                float balanceTrophy = CurrencyManager.Instance.GetCurrencySO(CurrencyType.Medal).value;
                //Start New
                GameEventHandler.Invoke(LogFirebaseEventCode.LeagueStarted, (int)balanceTrophy);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion
        });
        UpdateView();
    }

    private void Update()
    {
        UpdateTimeLeft();
    }

    private void UpdateTimeLeft()
    {
        m_TimeLeftText.SetText(leagueDataSO.timeLeftUntilLeagueEnds.ToRemainingTime(2));
    }

    #region Progression Event
    private void LogEventLeaguePromotion(string status)
    {
        int weeklyID = GetWeekOfYear();
        int divisionID = 1;

        GameEventHandler.Invoke(ProgressionEvent.LeaguePromotion, status, weeklyID, divisionID);
        int GetWeekOfYear()
        {
            DateTime today = DateTime.Now;
            int weekOfYear = CalcWeekOfYear(today);
            return weekOfYear;

            int CalcWeekOfYear(DateTime time)
            {
                return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    time,
                    CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                    CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek
                );
            }
        }
    }
    #endregion

    private void UpdateView()
    {
        UpdateTimeLeft();
        m_RewardUI.UpdateViewWithBestReward(leagueDataSO.GetFinalDivision().bestRewardInfo);
    }
}