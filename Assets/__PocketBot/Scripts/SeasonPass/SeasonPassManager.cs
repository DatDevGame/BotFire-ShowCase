using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public class SeasonPassManager : Singleton<SeasonPassManager>
{
    public static int PRESEASON_DAY_AMOUNT = 2;
    public static int TODAY_MISSION_AMOUNT = 3;
    public static int WEEK_MISSION_AMOUNT = 1;
    public static int HALF_SEASON_MISSION_AMOUNT = 6;

    [SerializeField] float unlockAtTrophies = 20;
    [SerializeField] HighestAchievedPPrefFloatTracker highestTrophy;
    [SerializeField] SeasonPassSO _seasonPassSO;
    [SerializeField] MissionSavedDataSO _missionSavedDataSO;
    [SerializeField] List<MissionTargetType> removedPreludeMissionTargetTypesForOldUsers;
    [SerializeField, BoxGroup("Test")] float _offsetHoursForTesting = 0;
    [BoxGroup("Test"), ShowInInspector] string nowTime => now.ToString();

    [HideInInspector] public bool isAllowShowPopup = false;
    [HideInInspector] public bool isAddingMissions = false;

    public bool isUnlockSeasonPass => highestTrophy.value >= unlockAtTrophies;
    public string preSeasonRemainingTime => now.ToReadableTimeSpan(seasonPassSO.data.FirstDayOfSeason);
    public string endSeasonRemainingTime => now.ToReadableTimeSpan(seasonPassSO.GetLastDay());
    public string endDayRemainingTime => now.ToReadableTimeSpan(seasonPassSO.GetNextMissionDay(), 2);
    public string endWeekRemainingTime => now.ToReadableTimeSpan(seasonPassSO.GetNextMissionWeek(), 2);
    public string endHalfSeasonRemainingTime => now.ToReadableTimeSpan(seasonPassSO.GetNextMissionHalfSeason(), 2);
    public DateTime now => DateTime.Now.AddHours(offsetHoursForTesting);
    public float offsetHoursForTesting
    {
        get
        {
#if UNITY_EDITOR
            return _offsetHoursForTesting;
#else 
            return GameDataSO.Instance.isDevMode ? _offsetHoursForTesting : 0;
#endif
        }

        set
        {
            _offsetHoursForTesting = value;
        }
    }
    public SeasonPassSO seasonPassSO => _seasonPassSO;
    public MissionSavedDataSO missionSavedDataSO => _missionSavedDataSO;
    public SeasonPassState state
    {
        get
        {
            return _seasonPassSO.data.state;
        }
        set
        {
            if (_seasonPassSO.data.state != value)
            {
                _seasonPassSO.data.state = value;
                if (value == SeasonPassState.StartSeason)
                {
                    WaitToShowSeasonPopup();
                }
                else if (value == SeasonPassState.EndSeason)
                {
                    WaitToShowSeasonPopup();
                }
            }
        }
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("DETECT_NEW_USER"))
        {
            PlayerPrefs.SetInt("DETECT_NEW_USER", 1);
            seasonPassSO.data.isNewUser = !isUnlockSeasonPass;
        }
        if (state == SeasonPassState.StartSeason)
        {
            WaitToShowSeasonPopup();
        }
        else if (state == SeasonPassState.EndSeason)
        {
            WaitToShowSeasonPopup();
        }
    }

    private void Update()
    {
        if (state == SeasonPassState.None)
        {
            if (isUnlockSeasonPass)
            {
                isAddingMissions = true;
                var preludeMissions = _missionSavedDataSO.preludeMissions;
                if (!seasonPassSO.data.isNewUser)
                {
                    preludeMissions = preludeMissions.FindAll(x => !removedPreludeMissionTargetTypesForOldUsers.Contains(x.targetType));
                }
                for (var i = 0; i < preludeMissions.Count; i++)
                {
                    var mission = preludeMissions[i];
                    var missionData = MissionManager.Instance.GetClonedPreludeMission(mission);
                    if (i >= SeasonPreludeUI.MAX_CELL_AMOUNT)
                    {
                        missionData.SetPaused(true);
                    }
                    _missionSavedDataSO.AddMission(missionData);
                }
                state = SeasonPassState.PreludeSeason;
                isAddingMissions = false;
            }
        }
        else if (state == SeasonPassState.PreSeason)
        {
            if (now > seasonPassSO.data.FirstDayOfSeason || seasonPassSO.isPurchased)
            {
                state = SeasonPassState.StartSeason;
            }
            else if (now > seasonPassSO.GetLastDay())
            {
                CheckToNewSeason();
            }
            else if (now < seasonPassSO.GetStartPreSeasonDay())
            {
                CheckToNewSeason();
            }
        }
        else if (state == SeasonPassState.InSeason)
        {
            if (now > seasonPassSO.GetLastDay())
            {
                state = SeasonPassState.EndSeason;
            }
            else if (now < seasonPassSO.GetStartPreSeasonDay())
            {
                state = SeasonPassState.EndSeason;
            }
            else
            {
                if (now > seasonPassSO.GetNextMissionDay())
                {
                    seasonPassSO.data.passedDay = new DateTime(now.Year, now.Month, now.Day);
                    isAddingMissions = true;
                    UpdateMissions(MissionScope.Daily);
                    isAddingMissions = false;
                }
                if (now > seasonPassSO.GetNextMissionWeek())
                {
                    var nextMissionWeek = seasonPassSO.GetNextMissionWeek();
                    while (nextMissionWeek < now)
                    {
                        seasonPassSO.data.passedWeek = new DateTime(nextMissionWeek.Year, nextMissionWeek.Month, nextMissionWeek.Day);
                        nextMissionWeek = seasonPassSO.GetNextMissionWeek();
                    }
                    isAddingMissions = true;
                    UpdateMissions(MissionScope.Weekly);
                    isAddingMissions = false;
                }
                if (now > seasonPassSO.GetNextMissionHalfSeason())
                {
                    var nextMissionHalfSeason = seasonPassSO.GetNextMissionHalfSeason();
                    seasonPassSO.data.passedHalfSeason = new DateTime(nextMissionHalfSeason.Year, nextMissionHalfSeason.Month, nextMissionHalfSeason.Day);
                    isAddingMissions = true;
                    UpdateMissions(MissionScope.Season);
                    isAddingMissions = false;
                }
            }
        }
    }

    void UpdateMissions(MissionScope missionScope)
    {
        missionSavedDataSO.ClearAllMissionsByScope(missionScope);
        var missionAmount = missionScope switch
        {
            MissionScope.Daily => TODAY_MISSION_AMOUNT,
            MissionScope.Weekly => WEEK_MISSION_AMOUNT,
            MissionScope.Season => HALF_SEASON_MISSION_AMOUNT,
            _ => 1,
        };

        if (seasonPassSO.data.isNewUser)
        {
            List<MissionData> scriptedMissions = MissionManager.Instance.GetScriptedMissions(missionScope);
            for (var i = 0; i < missionAmount; i++)
            {
                if (scriptedMissions != null && scriptedMissions.Count > i)
                    MissionManager.Instance.AddMission(scriptedMissions[i]);
                else
                    MissionManager.Instance.AddMission(MissionManager.Instance.GetRandomMission(missionScope, i));
            }
        }
        else
        {
            for (var i = 0; i < missionAmount; i++)
            {
                MissionManager.Instance.AddMission(MissionManager.Instance.GetRandomMission(missionScope, i));
            }
        }

        if (missionScope == MissionScope.Daily)
        {
            GameEventHandler.Invoke(SeasonPassEventCode.OnUpdateNewDailyMissions);
        }
        else if (missionScope == MissionScope.Weekly)
        {
            GameEventHandler.Invoke(SeasonPassEventCode.OnUpdateNewWeeklyMissions);
        }
        else
        {
            GameEventHandler.Invoke(SeasonPassEventCode.OnUpdateNewHalfSeasonMissions);
        }
    }

    public void CheckToNewSeason()
    {
        CheckTime();
    }

    Coroutine waitToShowSeasonPopupCoroutine;
    void WaitToShowSeasonPopup()
    {
        if (waitToShowSeasonPopupCoroutine != null)
        {
            StopCoroutine(waitToShowSeasonPopupCoroutine);
        }
        waitToShowSeasonPopupCoroutine = StartCoroutine(CR_WaitToShowSeasonPopup());
    }

    IEnumerator CR_WaitToShowSeasonPopup()
    {
        yield return new WaitUntil(() => LoadingScreenUI.IS_LOADING_COMPLETE);
        var dockController = FindObjectOfType<PBDockController>();
        yield return new WaitUntil(() => !isAllowShowPopup && (dockController.CurrentSelectedButtonType == ButtonType.Main || dockController.CurrentSelectedButtonType == ButtonType.BattlePass));
        if (state == SeasonPassState.StartSeason)
        {
            state = SeasonPassState.InSeason;
            UpdateInSeasonMissions();
            GameEventHandler.Invoke(SeasonPassEventCode.ShowStartSeasonPopup);
            yield return new WaitForSeconds(AnimationDuration.TINY);
            GameEventHandler.Invoke(SeasonPassEventCode.UpdateSeasonUI);
        }
        else if (state == SeasonPassState.EndSeason)
        {
            GameEventHandler.Invoke(SeasonPassEventCode.ShowEndSeasonPopup);
        }
    }

    void StartSeason()
    {
        seasonPassSO.seasonCurrency.value = 0;
        seasonPassSO.data.seasonTokenUI = seasonPassSO.seasonCurrency.value;
        seasonPassSO.data.isPurchasedPass = default;
        seasonPassSO.data.firstTimeEarnReward = false;
        seasonPassSO.InitRewardTree();
        MissionManager.Instance.ResetMissionCount();
        if (now > seasonPassSO.GetNextMissionDay())
        {
            seasonPassSO.data.passedDay = new DateTime(now.Year, now.Month, now.Day);
        }
        if (now > seasonPassSO.GetNextMissionWeek())
        {
            var nextMissionWeek = seasonPassSO.GetNextMissionWeek();
            while (nextMissionWeek < now)
            {
                seasonPassSO.data.passedWeek = new DateTime(nextMissionWeek.Year, nextMissionWeek.Month, nextMissionWeek.Day);
                nextMissionWeek = seasonPassSO.GetNextMissionWeek();
            }
        }
        if (now > seasonPassSO.GetNextMissionHalfSeason())
        {
            var nextMissionHalfSeason = seasonPassSO.GetNextMissionHalfSeason();
            seasonPassSO.data.passedHalfSeason = new DateTime(nextMissionHalfSeason.Year, nextMissionHalfSeason.Month, nextMissionHalfSeason.Day);
        }
    }

    void UpdateInSeasonMissions()
    {
        isAddingMissions = true;
        UpdateMissions(MissionScope.Daily);
        UpdateMissions(MissionScope.Weekly);
        UpdateMissions(MissionScope.Season);
        isAddingMissions = false;
    }

#if UNITY_EDITOR
    [Button]
    void DeleteData()
    {
        _seasonPassSO.seasonCurrency.value = 0;
        _seasonPassSO.Delete();
        _missionSavedDataSO.Delete();
        PlayerPrefs.DeleteKey("DETECT_NEW_USER");
    }

    [Button]
#endif
    void CheckTime()
    {
        var now = this.now;
        DateTime firstDayOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
        DateTime firstDayOfNextMonth = firstDayOfCurrentMonth.AddMonths(1);
        DateTime nextMonthPreSeasonStart = firstDayOfNextMonth.AddDays(-PRESEASON_DAY_AMOUNT);
        if (now >= nextMonthPreSeasonStart && now < firstDayOfNextMonth)
        {
            _seasonPassSO.data.FirstDayOfSeason = firstDayOfNextMonth;
            state = SeasonPassState.PreSeason;
            StartSeason();
        }
        else if (now >= firstDayOfCurrentMonth && now < nextMonthPreSeasonStart)
        {
            _seasonPassSO.data.FirstDayOfSeason = firstDayOfCurrentMonth;
            state = SeasonPassState.StartSeason;
            StartSeason();
        }
    }
}
