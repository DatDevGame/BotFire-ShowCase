using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames;
using LatteGames.Monetization;
using LatteGames.Template;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SeasonMissionSection : MonoBehaviour
{
    [SerializeField] Transform cellContainer;
    [SerializeField] EZAnimSequence showButtonGroupAnim;
    [SerializeField] Button requestBonusBtn;
    [SerializeField] Button claimBtn;
    [SerializeField] RVButtonBehavior doubleClaimBtn;
    [SerializeField] GameObject roundGameObject;
    [SerializeField] List<LocalizationParamsManager> roundTxtList;
    [SerializeField] ResourceLocationProvider resourceLocationProvider;
    [SerializeField] CanvasGroup canvasGroup;
    public TMP_Text cooldownTxt;

    [ReadOnly] public List<SeasonMissionCell> cells;

    bool isShowClaimButton;
    bool isShowRound;
    MissionScope missionScope;
    SeasonInSeasonHeaderUI headerUI;
    List<MissionData> missions
    {
        get
        {
            return missionScope switch
            {
                MissionScope.Daily => MissionManager.Instance.dailyMissions,
                MissionScope.Weekly => MissionManager.Instance.weeklyMissions,
                MissionScope.Season => MissionManager.Instance.seasonMissions,
                _ => null,
            };
        }
    }

    private void OnDestroy()
    {
        if (UnreadManager.Instance != null)
        {
            UnreadManager.Instance.RemoveUnreadTag(UnreadType.Dot, UnreadLocation.SeasonCompletedMissions, gameObject);
        }
    }

    void UpdateRoundTxt()
    {
        foreach (var txt in roundTxtList)
        {
            txt.SetParameterValue("Index", SeasonPassManager.Instance.seasonPassSO.data.todayRefreshRound.ToString());
        }
    }

    public void Init(MissionScope missionScope, SeasonInSeasonHeaderUI headerUI, bool isShowRound = false)
    {
        this.headerUI = headerUI;
        this.missionScope = missionScope;
        this.isShowRound = isShowRound;
        cells = new List<SeasonMissionCell>(cellContainer.GetComponentsInChildren<SeasonMissionCell>());
        roundGameObject.SetActive(isShowRound);
        var dockController = FindObjectOfType<PBDockController>();
        foreach (var cell in cells)
        {
            cell.isShowing = () => canvasGroup.alpha == 1 && dockController.CurrentSelectedButtonType == ButtonType.BattlePass;
            cell.OnInverseUpdated += UpdateView;
            cell.OnSkipAction += LogEventSkipMission;
        }
        requestBonusBtn.onClick.AddListener(OnClickedRequestBtn);
        claimBtn.onClick.AddListener(OnClickedClaimBtn);
        doubleClaimBtn.OnRewardGranted += OnRewardGrantedDoubleReward;

        UpdateNewMission();
    }

    public void UpdateNewMission()
    {
        if (isShowClaimButton)
        {
            isShowClaimButton = false;
            showButtonGroupAnim.InversePlay(() =>
            {
                roundGameObject.SetActive(isShowRound);
            });
            claimBtn.interactable = false;
            doubleClaimBtn.interactable = false;
            requestBonusBtn.interactable = false;
        }
        for (var i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            int index = i;
            cell.Init(missions[i], CurrencyType.SeasonToken, true, Replace);

            void Replace(SeasonMissionCell cell)
            {
                var randomMission = MissionManager.Instance.GetRandomMission(missionScope, index);
                MissionManager.Instance.ReplaceMission(cell.mission, randomMission);
                cell.Init(randomMission, CurrencyType.SeasonToken, true, Replace);
            }
        }
        UpdateRoundTxt();
    }

    public void UpdateView()
    {
        bool atLeastOneCompleted = false;
        bool allRewardIsClaimed = true;
        foreach (var cell in cells)
        {
            if (cell.gameObject.activeInHierarchy)
            {
                cell.UpdateProgress(AnimationDuration.SSHORT);
                atLeastOneCompleted |= cell.mission.isCompleted && !cell.mission.isRewardClaimed;
                allRewardIsClaimed &= cell.mission.isRewardClaimed && isShowRound;
            }

            #region Design Event
            try
            {
                LogEventStartMission(cell);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion

        }

        if (atLeastOneCompleted)
        {
            UnreadManager.Instance.AddUnreadTag(UnreadType.Dot, UnreadLocation.SeasonCompletedMissions, gameObject);
        }
        else
        {
            UnreadManager.Instance.RemoveUnreadTag(UnreadType.Dot, UnreadLocation.SeasonCompletedMissions, gameObject);
        }

        if (isShowClaimButton != atLeastOneCompleted || allRewardIsClaimed)
        {
            isShowClaimButton = atLeastOneCompleted || allRewardIsClaimed;

            if (isShowClaimButton)
            {
                requestBonusBtn.gameObject.SetActive(allRewardIsClaimed && isShowRound);
                claimBtn.gameObject.SetActive(!(allRewardIsClaimed && isShowRound));
                doubleClaimBtn.gameObject.SetActive(!(allRewardIsClaimed && isShowRound));
                roundGameObject.SetActive(false);
                showButtonGroupAnim.Play(() =>
                {
                    claimBtn.interactable = true;
                    doubleClaimBtn.interactable = true;
                    requestBonusBtn.interactable = true;
                });

                #region Firebase Event
                try
                {
                    if (requestBonusBtn.gameObject.activeSelf && canvasGroup.alpha == 1)
                    {
                        string keyFirebaseEvent = $"LogFirebaseEventCode-RequestBonusMissionsShown-{SeasonPassManager.Instance.seasonPassSO.data.todayRefreshRound}-{GetDayOfYearKey()}-{DateTime.Now.Year}";
                        if (!PlayerPrefs.HasKey(keyFirebaseEvent))
                        {
                            PlayerPrefs.SetInt(keyFirebaseEvent, 1);
                            int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();
                            GameEventHandler.Invoke(LogFirebaseEventCode.RequestBonusMissionsShown, seasonID);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
                }
                #endregion
            }
            else
            {
                showButtonGroupAnim.InversePlay(() =>
                {
                    roundGameObject.SetActive(isShowRound);
                });
                claimBtn.interactable = false;
                doubleClaimBtn.interactable = false;
                requestBonusBtn.interactable = false;
            }
        }
    }

    void OnClickedRequestBtn()
    {
        SeasonPassManager.Instance.seasonPassSO.data.todayRefreshRound++;
        isShowClaimButton = false;
        showButtonGroupAnim.InversePlay(() =>
        {
            roundGameObject.SetActive(isShowRound);
        });
        requestBonusBtn.interactable = false;
        RefreshMissions();
        LogMissionRound();

        #region Firebase Event
        try
        {
            int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();
            GameEventHandler.Invoke(LogFirebaseEventCode.RequestBonusMissionsClicked, seasonID);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    Coroutine claimCoroutine;
    void OnClickedClaimBtn()
    {
        if (claimCoroutine != null)
        {
            StopCoroutine(claimCoroutine);
        }
        claimCoroutine = StartCoroutine(CR_Claim(false));
    }

    void OnRewardGrantedDoubleReward(RVButtonBehavior.RewardGrantedEventData data)
    {
        if (claimCoroutine != null)
        {
            StopCoroutine(claimCoroutine);
        }
        claimCoroutine = StartCoroutine(CR_Claim(true));
    }

    IEnumerator CR_Claim(bool isDoubleClaim)
    {
        isShowClaimButton = false;
        showButtonGroupAnim.InversePlay(() =>
        {
            roundGameObject.SetActive(isShowRound);
        });
        claimBtn.interactable = false;
        doubleClaimBtn.interactable = false;

        var completedCells = cells.FindAll(x => x.gameObject.activeInHierarchy && x.mission.isCompletedUI && !x.mission.isRewardClaimed);
        if (completedCells.Count > 0)
        {
            var currencyMultiplier = isDoubleClaim ? 2 : 1;

            //Set data immediately
            foreach (var cell in completedCells)
            {
                #region Design Event
                try
                {
                    LogEventCompleteMission(cell);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
                }
                #endregion

                cell.mission.EarnReward();
                CurrencyManager.Instance.Acquire(CurrencyType.SeasonToken, currencyMultiplier * cell.mission.currencyRewardAmount, resourceLocationProvider.GetLocation(), resourceLocationProvider.GetItemId());

                #region Firebase Event
                try
                {
                    List<MissionData> dailyMission = SeasonPassManager.Instance.missionSavedDataSO.data.DailyMissions;
                    List<MissionData> weeklyMission = SeasonPassManager.Instance.missionSavedDataSO.data.WeeklyMissions;
                    List<MissionData> seasonMission = SeasonPassManager.Instance.missionSavedDataSO.data.SeasonMissions;
                    int totalMissionCompleted = MissionManager.Instance.GetAllMissionCompletedInSeason();

                    string seasonType = "season";
                    int missionsCompleted = totalMissionCompleted;
                    int totalMissions = dailyMission.Count + weeklyMission.Count + seasonMission.Count;
                    int missionNumber = 0;
                    string missionName = cell.mission.description;
                    string missionType = missionScope switch
                    {
                        MissionScope.Daily => "today",
                        MissionScope.Weekly => "weekly",
                        MissionScope.Season => "season",
                        _ => "null"
                    };
                    int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();
#if UNITY_EDITOR
                    Debug.Log($"LogFirebaseEventCode.ClaimMissionReward: seasonType:{seasonType} | missionsCompleted:{missionsCompleted} | totalMissions: {totalMissions} | missionNumber:{missionNumber} | missionName:{missionName} | missionType:{missionType} | seasonID:{seasonID}");
#endif
                    GameEventHandler.Invoke(LogFirebaseEventCode.ClaimMissionReward, seasonType, missionsCompleted, totalMissions, missionNumber, missionName, missionType, seasonID);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
                }
                #endregion
            }

            //Run animation
            foreach (var cell in completedCells)
            {
                CurrencyManager.Instance.PlayAcquireAnimation(CurrencyType.SeasonToken, currencyMultiplier * cell.mission.currencyRewardAmount, cell.RewardIconImg.transform.position, headerUI.tokenIcon.position, null, null, x =>
                {
                    headerUI.tokenIconAnim.Play();
                    SoundManager.Instance.PlaySFX(GeneralSFX.UIFillUpPremiumCurrency);
                    HapticManager.Instance.PlayFlashHaptic(HapticTypes.HeavyImpact);
                });
                yield return new WaitForSeconds(AnimationDuration.TINY);
                if (isDoubleClaim)
                {
                    cell.ShowDoubleRewardText(() =>
                    {
                        cell.HideRewardAndShowTick(() =>
                        {
                            cell.UpdateView();
                        });
                    });
                }
                else
                {
                    cell.HideRewardAndShowTick(() => cell.UpdateView());
                }
            }
        }

        #region MonetizationEventCode
        try
        {
            string missionType = missionScope switch
            {
                MissionScope.Daily => "Today",
                MissionScope.Weekly => "Weekly",
                MissionScope.Season => "Season",
                _ => "Null"
            };
            int accumulate = completedCells.Count;
            GameEventHandler.Invoke(MonetizationEventCode.ClaimDoubleMission, missionType, accumulate);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion

        yield return new WaitForSeconds(AnimationDuration.SHORT);
        UpdateView();
    }

    #region Design Event
    private void LogEvent(SeasonMissionCell cell, string status, DesignEvent designEvent)
    {
        MissionTargetType missionTargetType = cell.mission.targetType;
        MissionDifficulty missionDifficulty = cell.mission.difficulty;
        MissionScope missionScope = cell.mission.scope;
        int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();
        int missionIndex = cells.IndexOf(cell);
        int roundId = SeasonPassManager.Instance.seasonPassSO.data.todayRefreshRound;
        string missionID = missionTargetType.ToString();

        string baseKey = $"{status}-{missionTargetType}-{missionDifficulty}-{missionScope}-{seasonID}-{missionIndex}-{missionID}";
        string key = missionScope switch
        {
            MissionScope.Daily => baseKey + roundId + GetDayOfYearKey(),
            MissionScope.Weekly => baseKey + GetWeekOfYearKey(),
            MissionScope.Season => baseKey,
            _ => baseKey
        };

        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetInt(key, 1);
            GameEventHandler.Invoke(designEvent, status, seasonID, missionID, missionDifficulty);

            Debug.Log($"designEvent:{designEvent}\n" +
                $"status:{status}\n" +
                $"seasonID:{seasonID}\n" +
                $"missionID:{missionID}\n" +
                $"missionDifficulty:{missionDifficulty}");
        }
    }
    private void LogEventStartMission(SeasonMissionCell cell)
    {
        if (missionScope == MissionScope.Daily)
            LogEvent(cell, DesignEventStatus.Start, DesignEvent.TodayMission);
        else if (missionScope == MissionScope.Weekly)
            LogEvent(cell, DesignEventStatus.Start, DesignEvent.WeeklyMission);
        else if (missionScope == MissionScope.Season)
            LogEvent(cell, DesignEventStatus.Start, DesignEvent.SeasonMission);
    }

    private void LogEventCompleteMission(SeasonMissionCell cell)
    {
        if (missionScope == MissionScope.Daily)
            LogEvent(cell, DesignEventStatus.Complete, DesignEvent.TodayMission);
        else if (missionScope == MissionScope.Weekly)
            LogEvent(cell, DesignEventStatus.Complete, DesignEvent.WeeklyMission);
        else if (missionScope == MissionScope.Season)
            LogEvent(cell, DesignEventStatus.Complete, DesignEvent.SeasonMission);
    }

    private void LogEventSkipMission(SeasonMissionCell cell)
    {
        #region Design Event
        try
        {
            if (missionScope == MissionScope.Daily)
                LogEvent(cell, DesignEventStatus.Skip, DesignEvent.TodayMission);
            else if (missionScope == MissionScope.Weekly)
                LogEvent(cell, DesignEventStatus.Skip, DesignEvent.WeeklyMission);
            else if (missionScope == MissionScope.Season)
                LogEvent(cell, DesignEventStatus.Skip, DesignEvent.SeasonMission);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    private string GetDayOfYearKey()
    {
        DateTime today = DateTime.Now;
        int dayOfYear = today.DayOfYear;
        string dateTimeKey = $"Day-{dayOfYear}-Yeak-{today.Year}";
        return dateTimeKey;
    }
    private string GetWeekOfYearKey()
    {
        DateTime today = DateTime.Now;
        int weekOfYear = GetWeekOfYear(today);
        string dateTimeKey = $"Week-{weekOfYear}-Yeak-{today.Year}";
        return dateTimeKey;

        int GetWeekOfYear(DateTime time)
        {
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                time,
                CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek
            );
        }
    }

    #endregion

    #region Progression Event
    private void LogMissionRound()
    {
        // Get the reversed list of milestones
        List<SeasonPassSO.Milestone> milestones =
            new List<SeasonPassSO.Milestone>(SeasonPassManager.Instance.seasonPassSO.milestones);
        milestones.Reverse();

        // Find the highest unlocked milestone
        SeasonPassSO.Milestone highestMilestone = milestones.FirstOrDefault(milestone => milestone.Unlocked);

        // Determine the index of the highest milestone
        int indexMilestone = highestMilestone != null
            ? SeasonPassManager.Instance.seasonPassSO.milestones.IndexOf(highestMilestone) + 1
            : 0;

        // Log the mission round
        string status = ProgressionEventStatus.Start;
        int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();
        int milestoneID = indexMilestone;

        GameEventHandler.Invoke(ProgressionEvent.MissionRound, status, seasonID, milestoneID);
    }
    #endregion


#if UNITY_EDITOR
    [Button]
    void CompleteFirstMission()
    {
        var uncompletedCell = cells.FindAll(x => !x.mission.isCompleted);
        var cell = uncompletedCell.First();
        var mission = cell.mission;
        mission.SetProgress(mission.targetValue);
        UpdateView();
    }
#endif
    [Button]
    void RefreshMissions()
    {
        foreach (var cell in cells)
            MissionManager.Instance.RemoveMission(cell.mission);
        if (SeasonPassManager.Instance.seasonPassSO.data.isNewUser)
        {
            List<MissionData> scriptedMissions = MissionManager.Instance.GetScriptedMissions(missionScope);
            for (int i = 0; i < cells.Count; i++)
            {
                if (scriptedMissions != null && scriptedMissions.Count > i)
                    MissionManager.Instance.AddMission(scriptedMissions[i]);
                else
                    MissionManager.Instance.AddMission(MissionManager.Instance.GetRandomMission(missionScope, i));
            }
        }
        else
            for (int i = 0; i < cells.Count; i++)
                MissionManager.Instance.AddMission(MissionManager.Instance.GetRandomMission(missionScope, i));
        UpdateNewMission();
        UpdateView();
    }
}
