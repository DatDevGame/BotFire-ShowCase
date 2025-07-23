using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using LatteGames.Tab;
using UnityEngine;

public class SeasonUIController : MonoBehaviour
{
    [SerializeField] SeasonPreludeUI seasonPreludeUI;
    [SerializeField] SeasonInSeasonUI inSeasonUIPrefab;
    [SerializeField] SeasonPreSeasonUI preSeasonUI;
    [SerializeField] Transform underBottomTabs;
    [SerializeField] GameObject inSeasonUIOriginal;

    SeasonInSeasonUI inSeasonUIInstance;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonBattlePass, UpdateView);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.UpdateSeasonUI, UpdateView);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.ResetSeasonUI, ResetSeasonUI);

        #region Progression Event
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonBattlePass, LogEventEnterSeasonTab);
        #endregion

        #region Firebase Event
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonBattlePass, FirebaseEventEnterPreludeSeasonTab);
        GameEventHandler.AddActionEvent(SeasonTabEvent.TransitionTab, FirebaseEventEnterSeasonTab);
        #endregion
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonBattlePass, UpdateView);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.UpdateSeasonUI, UpdateView);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.ResetSeasonUI, ResetSeasonUI);

        #region Progression Event
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonBattlePass, LogEventEnterSeasonTab);
        #endregion

        #region Firebase Event
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonBattlePass, FirebaseEventEnterPreludeSeasonTab);
        GameEventHandler.AddActionEvent(SeasonTabEvent.TransitionTab, FirebaseEventEnterSeasonTab);
        #endregion
    }

    private void Start()
    {
        // Destroy(inSeasonUIOriginal);
        // CreateInstance();
        // UpdateView();
    }

    void UpdateView()
    {
        if (SeasonPassManager.Instance.seasonPassSO.data.state == SeasonPassState.PreludeSeason)
        {
            EnableOnlyOneUI(seasonPreludeUI.gameObject);
            seasonPreludeUI.InitOrRefresh();
        }
        else if (SeasonPassManager.Instance.seasonPassSO.data.state == SeasonPassState.PreSeason)
        {
            EnableOnlyOneUI(preSeasonUI.gameObject);
            preSeasonUI.InitOrRefresh();
        }
        else if (SeasonPassManager.Instance.seasonPassSO.data.state == SeasonPassState.InSeason)
        {
            EnableOnlyOneUI(inSeasonUIInstance.gameObject);
            inSeasonUIInstance.InitOrRefresh();
        }
    }

    void EnableOnlyOneUI(GameObject UI)
    {
        seasonPreludeUI.gameObject.SetActive(seasonPreludeUI.gameObject == UI);
        inSeasonUIInstance.gameObject.SetActive(inSeasonUIInstance.gameObject == UI);
        preSeasonUI.gameObject.SetActive(preSeasonUI.gameObject == UI);
    }

    void ResetSeasonUI()
    {
        CreateInstance();
        UpdateView();
    }

    void CreateInstance()
    {
        if (inSeasonUIInstance != null)
        {
            Destroy(inSeasonUIInstance.gameObject);
        }
        inSeasonUIInstance = Instantiate(inSeasonUIPrefab, underBottomTabs);
    }

    #region Progression Event
    private void LogEventEnterSeasonTab()
    {
        string status = ProgressionEventStatus.Start;
        string missionStatus = "MissionIncomplete";

        // Helper method to check mission completion status
        bool HasUnclaimedCompletedMissions(IEnumerable<SeasonMissionCell> cells) =>
            cells.Any(v => v.mission.isCompleted && !v.mission.isRewardClaimed);

        var seasonData = SeasonPassManager.Instance.seasonPassSO.data;
        if (seasonData.state == SeasonPassState.PreludeSeason)
        {
            // Check prelude season mission cells
            var activeMissionCells = seasonPreludeUI.MissionCells.Where(v => v.gameObject.activeSelf);
            if (!activeMissionCells.Any()) return;
            if (activeMissionCells.Any(v => v.mission.isCompleted))
                missionStatus = "MissionComplete";
        }
        else if (seasonData.state == SeasonPassState.InSeason)
        {
            // Check in-season mission sections for completed and unclaimed missions
            var seasonMissionUI = inSeasonUIInstance.SeasonMissionUI;

            if (HasUnclaimedCompletedMissions(seasonMissionUI.DailySection.cells) ||
                HasUnclaimedCompletedMissions(seasonMissionUI.WeeklySection.cells) ||
                HasUnclaimedCompletedMissions(seasonMissionUI.SeasonSection.cells))
            {
                missionStatus = "MissionComplete";
            }
        }
        // Trigger the progression event
        GameEventHandler.Invoke(ProgressionEvent.EnterSeasonTab, status, missionStatus);
    }
    #endregion

    #region Firebase Event
    private void FirebaseEventEnterPreludeSeasonTab()
    {
        var seasonData = SeasonPassManager.Instance.seasonPassSO.data;
        if (seasonData.state == SeasonPassState.PreludeSeason)
            FirebaseEventEnterSeasonTab(0);
    }
    private void FirebaseEventEnterSeasonTab(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null) return;
        int index = (int)parrams[0];

        if (inSeasonUIInstance == null)
            return;

        var seasonMissionUI = inSeasonUIInstance.SeasonMissionUI;
        var seasonData = SeasonPassManager.Instance.seasonPassSO.data;

        string seasonType = seasonData.state switch
        {
            SeasonPassState.PreludeSeason => "prelude",
            SeasonPassState.InSeason => "season",
            SeasonPassState.PreSeason => "pre-season",
            _ => "season"
        };
        string tabOpened = index == 0 ? "missions" : "rewards";
        int missionsCompleted = 0;
        int totalMissions = 0;
        int todayCompleted = 0;
        int todayAvailable = 0;
        int weeklyCompleted = 0;
        int weeklyAvailable = 0;
        int seasonCompleted = 0;
        int seasonAvailable = 0;
        MissionSavedDataSO.Data missionSavedDataSO = SeasonPassManager.Instance.missionSavedDataSO.data;
        if (seasonData.state == SeasonPassState.PreludeSeason)
        {
            List<MissionData> preludeMissions = missionSavedDataSO.PreludeMissions;
            missionsCompleted = preludeMissions.Where(v => v.isCompleted).Count();
            totalMissions = preludeMissions.Count();
            tabOpened = "null";
        }
        else if (seasonData.state == SeasonPassState.InSeason)
        {
            List<MissionData> dailyMission = missionSavedDataSO.DailyMissions;
            List<MissionData> weeklyMission = missionSavedDataSO.WeeklyMissions;
            List<MissionData> seasonMission = missionSavedDataSO.SeasonMissions;
            MissionSavedDataSO missionSavedData = MissionManager.Instance.MissionSavedDataSO;

            todayAvailable = dailyMission.Count();
            todayCompleted = dailyMission.Where(v => v.isCompleted && !v.isRewardClaimed).Count();
            weeklyAvailable = weeklyMission.Count();
            weeklyCompleted = weeklyMission.Where(v => v.isCompleted && !v.isRewardClaimed).Count();
            seasonAvailable = seasonMission.Count();
            seasonCompleted = seasonMission.Where(v => v.isCompleted && !v.isRewardClaimed).Count();

            totalMissions = todayAvailable + weeklyAvailable + seasonAvailable;
            missionsCompleted = MissionManager.Instance.GetAllMissionCompletedInSeason();
        }
        else
        {
            tabOpened = "null";
        }
        int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();
#if UNITY_EDITOR
        Debug.Log($"LogFirebaseEventCode.SeasonMenuReached | seasonType:{seasonType} | todayCompleted:{todayCompleted} | weeklyCompleted:{weeklyCompleted} | seasonCompleted:{seasonCompleted} | todayAvailable:{todayAvailable} | weeklyAvailable:{weeklyAvailable} | seasonAvailable:{seasonAvailable} | missionsCompleted:{missionsCompleted} | totalMissions:{totalMissions} | tabOpened:{tabOpened} | seasonID:{seasonID}");
#endif
        GameEventHandler.Invoke(LogFirebaseEventCode.SeasonMenuReached, seasonType, todayCompleted, weeklyCompleted, seasonCompleted, todayAvailable, weeklyAvailable, seasonAvailable, missionsCompleted, totalMissions, tabOpened, seasonID);
    }
    #endregion
}
