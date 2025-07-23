using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;

public class MissionNotification : Singleton<MissionNotification>
{
    [SerializeField] EZAnimBase gradientAnim;
    [SerializeField] EZAnimBase showMissionCellAnim;
    [SerializeField] SeasonMissionCell seasonMissionCell;
    [SerializeField] AnimationCurve updateCellProgressCurve;
    [SerializeField] float displayDuration = 3;
    [SerializeField] float queueSpacingDuration = 1;
    [SerializeField] float updateCellProgressDuration = 1;

    Dictionary<MissionData, Vector2> updatedMissionProgressRanges = new();
    Coroutine showMissionNotificationCoroutine;
    Coroutine updateCellProgressCoroutine;
    int disableCount = 0;
    bool isDisable => disableCount > 0;

    protected override void Awake()
    {
        //TODO: Hide IAP & Popup
        //base.Awake();
        //MissionData.OnMissionCompleted_Static += OnMissionProgressUpdated;
        //GameEventHandler.AddActionEvent(SceneManagementEventCode.OnLoadSceneCompleted, OnLoadSceneCompleted);
        //GameEventHandler.AddActionEvent(PBPvPEventCode.OnShowGameOverUI, OnEnableMissionNotification);
        //GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnDisableMissionNotification);
        //GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnEnableMissionNotification);
    }

    private void OnDestroy()
    {
        //TODO: Hide IAP & Popup
        //MissionData.OnMissionCompleted_Static -= OnMissionProgressUpdated;
        //GameEventHandler.RemoveActionEvent(SceneManagementEventCode.OnLoadSceneCompleted, OnLoadSceneCompleted);
        //GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnShowGameOverUI, OnEnableMissionNotification);
        //GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnDisableMissionNotification);
        //GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnEnableMissionNotification);
    }

    void OnLoadSceneCompleted(object[] args)
    {
        var destinationSceneName = (string)args[0];
        if (destinationSceneName == SceneName.PvP.ToString() || destinationSceneName == SceneName.PvP_BossFight.ToString())
        {
            OnDisableMissionNotification();
        }
    }

    void OnDisableMissionNotification()
    {
        disableCount++;
    }

    void OnEnableMissionNotification()
    {
        disableCount--;
    }

    void OnMissionProgressUpdated(MissionData missionData, Vector2 progressRange)
    {
        if (!updatedMissionProgressRanges.ContainsKey(missionData))
        {
            updatedMissionProgressRanges.Add(missionData, progressRange);
        }
        else
        {
            updatedMissionProgressRanges[missionData] = new Vector2(updatedMissionProgressRanges[missionData].x, progressRange.y);
        }
        if (showMissionNotificationCoroutine == null)
        {
            showMissionNotificationCoroutine = StartCoroutine(CR_ShowMissionNotification());
        }


        if (missionData.scope == MissionScope.Daily)
        {
            MissionManager.Instance.MissionSavedDataSO.TotalTodayMissionCompletedInSeason.value++;
        }
        else if (missionData.scope == MissionScope.Weekly)
        {
            MissionManager.Instance.MissionSavedDataSO.TotalWeeklyMissionCompletedInSeason.value++;
        }
        else
        {
            MissionManager.Instance.MissionSavedDataSO.TotalSeasonMissionCompletedInSeason.value++;
        }

        StartCoroutine(CR_LogEvent(missionData, progressRange));
    }

    #region Firebase Event
    IEnumerator CR_LogEvent(MissionData missionData, Vector2 progressRange)
    {
        if (SeasonPassManager.Instance.isAddingMissions)
        {
            yield return new WaitUntil(() => !SeasonPassManager.Instance.isAddingMissions);
        }
        List<MissionData> preludeMissions = SeasonPassManager.Instance.missionSavedDataSO.data.PreludeMissions;
        List<MissionData> dailyMission = SeasonPassManager.Instance.missionSavedDataSO.data.DailyMissions;
        List<MissionData> weeklyMission = SeasonPassManager.Instance.missionSavedDataSO.data.WeeklyMissions;
        List<MissionData> seasonMission = SeasonPassManager.Instance.missionSavedDataSO.data.SeasonMissions;

        int totalMissionCompleted = 0;
        int totalMissions = 0;
        var seasonData = SeasonPassManager.Instance.seasonPassSO.data;
        if (seasonData.state == SeasonPassState.PreludeSeason)
        {
            totalMissionCompleted = preludeMissions.Where(v => v.isCompleted).Count();
            totalMissions = preludeMissions.Count();
        }
        else
        {
            totalMissionCompleted = MissionManager.Instance.GetAllMissionCompletedInSeason();
            totalMissions = dailyMission.Count + weeklyMission.Count + seasonMission.Count;
        }

        string keyNotePreludeSeason = "PreludeSeason-Key-Mission";
        string keyNoteClaimPrelude = $"PreludeSeason-Key-ClaimMission-{missionData.targetType}";
        int preludeMissionNumberValue = PlayerPrefs.GetInt(keyNotePreludeSeason, 1);
        PlayerPrefs.SetInt(keyNoteClaimPrelude, preludeMissionNumberValue);
        int missionsCompleted = totalMissionCompleted;
        int missionNumber = seasonData.state == SeasonPassState.PreludeSeason ? PlayerPrefs.GetInt(keyNotePreludeSeason, preludeMissionNumberValue) : 0;
        if (seasonData.state == SeasonPassState.None)
            missionNumber = PlayerPrefs.GetInt(keyNotePreludeSeason, preludeMissionNumberValue);
        PlayerPrefs.SetInt(keyNotePreludeSeason, preludeMissionNumberValue + 1);
        string missionName = missionData.description;

        string seasonType = seasonData.state switch
        {
            SeasonPassState.PreludeSeason => "prelude",
            SeasonPassState.InSeason => "season",
            SeasonPassState.PreSeason => "pre-season",
            _ => "prelude",
        };


        string missionType = missionData.scope switch
        {
            MissionScope.Daily => "today",
            MissionScope.Weekly => "weekly",
            MissionScope.Season => "season",
            _ => "null"
        };
        if (seasonData.state == SeasonPassState.PreludeSeason || seasonData.state == SeasonPassState.None)
            missionType = "null";

        int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();

#if UNITY_EDITOR
        Debug.Log($"LogFirebaseEventCode.AvailableMissionReward: \n" +
            $"seasonType:{seasonType}\n" +
            $"missionsCompleted:{missionsCompleted}\n" +
            $"totalMissions:{totalMissions}\n" +
            $"missionNumber:{missionNumber}\n" +
            $"missionName:{missionName}\n" +
            $"missionType:{missionType}\n" +
            $"seasonID:{seasonID}");
#endif
        GameEventHandler.Invoke(LogFirebaseEventCode.AvailableMissionReward, seasonType, missionsCompleted, totalMissions, missionNumber, missionName, missionType, seasonID);
    }
    #endregion

    IEnumerator CR_ShowMissionNotification()
    {
        bool isShowGradient = false;
        if (!isDisable)
        {
            gradientAnim.Play();
            isShowGradient = true;
        }
        while (updatedMissionProgressRanges.Count > 0)
        {
            if (isDisable)
            {
                if (isShowGradient)
                {
                    gradientAnim.InversePlay();
                    isShowGradient = false;
                }
                yield return new WaitUntil(() => !isDisable);
                gradientAnim.Play();
                isShowGradient = true;
            }
            var updatedMissionProgressRange = updatedMissionProgressRanges.First();
            var missionData = updatedMissionProgressRange.Key;
            var progressRange = updatedMissionProgressRange.Value;
            updatedMissionProgressRanges.Remove(missionData);
            var currencyType = missionData.isPreludeMission ? CurrencyType.Standard : CurrencyType.SeasonToken;
            seasonMissionCell.Init(missionData, currencyType, false, null, false);
            seasonMissionCell.UpdateViewManually(progressRange.x);
            showMissionCellAnim.Play(() =>
            {
                if (updateCellProgressCoroutine != null)
                {
                    StopCoroutine(updateCellProgressCoroutine);
                }
                updateCellProgressCoroutine = StartCoroutine(CommonCoroutine.LerpAnimation(updateCellProgressDuration, updateCellProgressCurve, (t) =>
                {
                    seasonMissionCell.UpdateViewManually(Mathf.Lerp(progressRange.x, progressRange.y, t));
                }));
            });
            yield return new WaitForSeconds(displayDuration);
            showMissionCellAnim.InversePlay();
            yield return new WaitForSeconds(queueSpacingDuration);
        }
        gradientAnim.InversePlay();
        isShowGradient = false;
        showMissionNotificationCoroutine = null;
    }
}
