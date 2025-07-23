using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonMissionUI : MonoBehaviour
{
    public SeasonMissionSection DailySection => dailySection;
    public SeasonMissionSection WeeklySection => weeklySection;
    public SeasonMissionSection SeasonSection => seasonSection;

    [SerializeField] TMP_Text dateTxtToday;
    [SerializeField] SeasonMissionSection dailySection;
    [SerializeField] SeasonMissionSection weeklySection;
    [SerializeField] SeasonMissionSection seasonSection;
    [HideInInspector] public SeasonInSeasonHeaderUI header;

    bool isInitialized = false;

    public void InitOrRefresh()
    {
        if (!isInitialized)
        {
            Init();
        }
        else
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        dailySection.UpdateView();
        weeklySection.UpdateView();
        seasonSection.UpdateView();
    }

    public void Init()
    {
        isInitialized = true;
        GameEventHandler.AddActionEvent(SeasonPassEventCode.OnUpdateNewDailyMissions, dailySection.UpdateNewMission);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.OnUpdateNewWeeklyMissions, weeklySection.UpdateNewMission);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.OnUpdateNewHalfSeasonMissions, seasonSection.UpdateNewMission);

        dailySection.Init(MissionScope.Daily, header, true);
        weeklySection.Init(MissionScope.Weekly, header);
        seasonSection.Init(MissionScope.Season, header);

        Refresh();
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.OnUpdateNewDailyMissions, dailySection.UpdateNewMission);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.OnUpdateNewWeeklyMissions, weeklySection.UpdateNewMission);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.OnUpdateNewHalfSeasonMissions, seasonSection.UpdateNewMission);
    }

    private void Update()
    {
        if (isInitialized)
        {
            dailySection.cooldownTxt.text = SeasonPassManager.Instance.endDayRemainingTime;
            weeklySection.cooldownTxt.text = SeasonPassManager.Instance.endWeekRemainingTime;
            seasonSection.cooldownTxt.text = SeasonPassManager.Instance.endHalfSeasonRemainingTime;
            dateTxtToday.text = SeasonPassManager.Instance.now.GetFormattedDate(LocalizationManager.CurrentCulture);
        }
    }
}
