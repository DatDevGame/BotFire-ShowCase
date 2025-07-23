using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DG.Tweening.DOTweenAnimation;

public class SeasonPreludeUI : MonoBehaviour
{
    public Action OnCompletedClaiming;

    public List<SeasonMissionCell> MissionCells => missionCells;

    public const int MAX_CELL_AMOUNT = 3;
    [SerializeField] SeasonMissionCell missionCell;
    [SerializeField] SeasonPreludeProgressSection preludeProgressSection;
    [SerializeField] Transform missionCellContainer;
    [SerializeField] GameObject CurrencyCanvas;
    [SerializeField] Button claimBtn;
    [SerializeField] VerticalLayoutGroup missionCellVerticalGroup;
    [SerializeField] EZAnimBase showClaimBtnAnim;
    [SerializeField] ResourceLocationProvider resourceLocationProvider;

    List<Vector3> cellPositions = new();
    [ShowInInspector] List<SeasonMissionCell> missionCells = new();
    Queue<MissionData> unclaimedMissions;
    bool isInitialized = false;
    bool isShowClaimButton = false;
    Coroutine claimCoroutine;

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

    public void Refresh()
    {
        bool atLeastOneCompleted = false;
        foreach (var cell in missionCells)
        {
            if (cell.gameObject.activeInHierarchy)
            {
                cell.UpdateProgress(AnimationDuration.SSHORT);
                atLeastOneCompleted |= cell.mission.isCompleted;

                #region Progression Event
                LogEventStartMission(cell);
                #endregion
            }
        }

        if (atLeastOneCompleted)
        {
            UnreadManager.Instance.AddUnreadTag(UnreadType.Dot, UnreadLocation.SeasonCompletedMissions, gameObject);
        }
        else
        {
            UnreadManager.Instance.RemoveUnreadTag(UnreadType.Dot, UnreadLocation.SeasonCompletedMissions, gameObject);
        }

        if (isShowClaimButton != atLeastOneCompleted)
        {
            isShowClaimButton = atLeastOneCompleted;
            if (isShowClaimButton)
            {
                showClaimBtnAnim.InversePlay(() =>
                {
                    claimBtn.interactable = true;
                });
            }
            else
            {
                showClaimBtnAnim.Play();
                claimBtn.interactable = false;
            }
        }
    }

    public void Init()
    {
        SeasonPassManager.Instance.isAllowShowPopup = true;
        CurrencyCanvas.SetActive(false);
        preludeProgressSection.Init();
        unclaimedMissions = new Queue<MissionData>(SeasonPassManager.Instance.missionSavedDataSO.data.PreludeMissions.FindAll(x => !x.isRewardClaimed));

        for (var i = 0; i < MAX_CELL_AMOUNT; i++)
        {
            var cell = Instantiate(missionCell, missionCellContainer);
            cell.gameObject.SetActive(false);
            missionCells.Add(cell);
        }

        claimBtn.interactable = false;
        claimBtn.onClick.AddListener(OnClickedClaimBtn);

        InitUnclaimedMissionCells();
        Refresh();
        isInitialized = true;

        #region Progression Event
        string status = "Start";
        string content = "Prelude";
        string keyEventProgression = $"key-Progression-{status}-{content}";
        if (!PlayerPrefs.HasKey(keyEventProgression))
        {
            PlayerPrefs.SetInt(keyEventProgression, 1);
            GameEventHandler.Invoke(ProgressionEvent.Progression, status, content);
        }
        #endregion
    }

    private void OnDestroy()
    {
        if (UnreadManager.Instance != null)
        {
            UnreadManager.Instance.RemoveUnreadTag(UnreadType.Dot, UnreadLocation.SeasonCompletedMissions, gameObject);
        }
    }

    public void InitUnclaimedMissionCells()
    {
        var listCount = unclaimedMissions.Count > MAX_CELL_AMOUNT ? MAX_CELL_AMOUNT : unclaimedMissions.Count;
        for (var i = 0; i < listCount; i++)
        {
            var cell = missionCells[i];
            cell.gameObject.SetActive(true);
            var missionData = unclaimedMissions.Dequeue();
            cell.Init(missionData, CurrencyType.Standard, false);
        }
    }

    void OnClickedClaimBtn()
    {
        if (claimCoroutine != null)
        {
            StopCoroutine(claimCoroutine);
        }
        claimCoroutine = StartCoroutine(CR_Claim());
    }

    IEnumerator CR_Claim()
    {
        if (missionCellVerticalGroup.enabled)
        {
            foreach (var cell in missionCells)
            {
                cellPositions.Add(cell.transform.position);
            }
            missionCellVerticalGroup.enabled = false;
        }

        isShowClaimButton = false;
        showClaimBtnAnim.Play();
        UnreadManager.Instance.RemoveUnreadTag(UnreadType.Dot, UnreadLocation.SeasonCompletedMissions, gameObject);
        claimBtn.interactable = false;

        var completedCells = missionCells.FindAll(x => x.gameObject.activeInHierarchy && x.mission.isCompletedUI && !x.mission.isRewardClaimed);
        var uncompletedCells = missionCells.FindAll(x => x.gameObject.activeInHierarchy && !x.mission.isCompletedUI);
        CurrencyCanvas.SetActive(true);
        if (completedCells.Count > 0)
        {
            //Set data immediately
            foreach (var cell in completedCells)
            {
                LogEventCompleteMission(cell);
                cell.mission.EarnReward();
                CurrencyManager.Instance.Acquire(CurrencyType.Standard, cell.mission.currencyRewardAmount, resourceLocationProvider.GetLocation(), resourceLocationProvider.GetItemId());

                #region Firebase Event
                try
                {
                    List<MissionData> preludeMissions = SeasonPassManager.Instance.missionSavedDataSO.data.PreludeMissions;

                    string keyNoteClaimPrelude = $"PreludeSeason-Key-ClaimMission-{cell.mission.targetType}";
                    int preludeMissionNumberValue = PlayerPrefs.GetInt(keyNoteClaimPrelude, 0);

                    string seasonType = "prelude";
                    int missionsCompleted = preludeMissions.Where(v => v.isCompleted).Count();
                    int totalMissions = preludeMissions.Count;
                    int missionNumber = preludeMissionNumberValue;
                    string missionName = cell.mission.description;
                    string missionType = "null";
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

            if (unclaimedMissions.Count <= 0 && uncompletedCells.Count <= 0)
            {
                SeasonPassManager.Instance.CheckToNewSeason();

                #region Progression Event
                string status = "Complete";
                string content = "Prelude";
                string keyEventProgression = $"key-Progression-{status}-{content}";
                if (!PlayerPrefs.HasKey(keyEventProgression))
                {
                    PlayerPrefs.SetInt(keyEventProgression, 1);
                    GameEventHandler.Invoke(ProgressionEvent.Progression, status, content);
                }
                #endregion
            }

            //Run animation
            foreach (var cell in completedCells)
            {
                CurrencyManager.Instance.PlayAcquireAnimation(CurrencyType.Standard, cell.mission.currencyRewardAmount, cell.RewardIconImg.transform.position, null, null, null);
                preludeProgressSection.IncreaseNode();
                cell.UpdateView();
                yield return new WaitForSeconds(AnimationDuration.TINY);
            }

            var completedMoveLeftAnimationCount = 0;
            for (var i = 0; i < completedCells.Count; i++)
            {
                var cell = completedCells[i];
                var originalPos = cell.transform.position;
                completedMoveLeftAnimationCount++;
                cell.transform.DOMove(originalPos + Vector3.left * 1000, AnimationDuration.TINY).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    //Sort the missionCells list
                    missionCells.Remove(cell);
                    missionCells.Add(cell);
                    completedMoveLeftAnimationCount--;
                    if (completedMoveLeftAnimationCount <= 0)
                    {
                        MoveUpCells(completedCells);
                        if (preludeProgressSection.isCompleted)
                        {
                            GameEventHandler.Invoke(SeasonPassEventCode.ShowSeasonUnlockScreen);
                        }
                    }
                });
            }

            void MoveUpCells(List<SeasonMissionCell> completedCells)
            {
                var completedMoveUpAnimationCount = 0;
                for (var i = 0; i < missionCells.Count; i++)
                {
                    var cell = missionCells[i];
                    MissionData missionData = null;
                    if (completedCells.Contains(cell))
                    {
                        missionData = unclaimedMissions.Count > 0 ? unclaimedMissions.Dequeue() : null;
                        if (missionData != null)
                        {
                            completedMoveUpAnimationCount++;
                            missionData.SetPaused(false);
                            missionData.ResetData();
                            cell.Init(missionData, CurrencyType.Standard, false);
                            cell.transform.position = cellPositions[0] + Vector3.down * 1000;
                            cell.transform.DOMove(cellPositions[missionCells.IndexOf(cell)], AnimationDuration.TINY).SetEase(Ease.OutQuad).OnComplete(() =>
                            {
                                completedMoveUpAnimationCount--;
                                if (completedMoveUpAnimationCount <= 0)
                                {
                                    Refresh();
                                }
                            });
                        }
                        else
                        {
                            cell.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        cell.transform.DOMove(cellPositions[missionCells.IndexOf(cell)], AnimationDuration.TINY).SetEase(Ease.OutQuad);
                    }
                }
            }
        }

        yield return new WaitForSeconds(AnimationDuration.MEDIUM);
        CurrencyCanvas.SetActive(false);
        OnCompletedClaiming?.Invoke();
    }

    #region Design Event
    private void LogEvent(SeasonMissionCell cell, string status)
    {
        if (cell == null || cell.mission == null)
        {
            Debug.LogError($"Cell or Mission Is Null");
            return;
        }

        MissionTargetType missionTargetType = cell.mission.targetType;
        MissionDifficulty missionDifficulty = MissionDifficulty.Easy;
        int preludeID = cell.mission.PreludeID;
        int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();
        string missionName = preludeID switch
        {
            1 => "CardUpgrade_1",
            2 => "MatchWin_1",
            3 => "OpenBoxes_1",
            4 => "EarnCurrency_1",
            5 => "CardUpgrade_2",
            6 => "WeaponMastery_1",
            7 => "BossDefeats_1",
            8 => "MatchWin_2",
            9 => "CardCollection_1",
            _ => "null",
        };

        string key = $"{preludeID}-{status}-{seasonID}-{cell.mission.targetType}";
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetInt(key, 1);
            GameEventHandler.Invoke(DesignEvent.PreludeMission, status, seasonID, missionName, missionDifficulty);
        }
    }

    private void LogEventStartMission(SeasonMissionCell cell)
    {
        LogEvent(cell, DesignEventStatus.Start);
    }

    private void LogEventCompleteMission(SeasonMissionCell cell)
    {
        LogEvent(cell, DesignEventStatus.Complete);
    }
    #endregion

}
