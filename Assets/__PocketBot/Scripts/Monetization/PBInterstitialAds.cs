using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using UnityEngine;

public class PBInterstitialAds : Singleton<PBInterstitialAds>
{
    [SerializeField] private AdsConfigSO adsConfigSO;
    [SerializeField] private HighestAchievedPPrefFloatTracker highestAchievedMedal;
    [SerializeField, BoxGroup("AfterExitingGameOverUI")] SceneName pvpSceneName;
    [SerializeField, BoxGroup("AfterExitingGameOverUI")] SceneName pvpBossSceneName;
    [SerializeField, BoxGroup("AfterExitingGameOverUI")] ModeVariable modeVariable;

    private bool isBackFromPvP;
    private bool matchIsVictory;

    bool isEnoughMedalToShow => highestAchievedMedal.value >= adsConfigSO.FirstShowTrophyThreshold;

    protected override void Awake()
    {
        base.Awake();
        GameEventHandler.AddActionEvent(SceneManagementEventCode.OnLoadSceneCompleted, HandleLoadSceneCompleted);
        PBOpenTrophyRoadButton.OnSetupCompleted += OnOpenTrophyRoadButtonSetupCompleted;
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.AddActionEvent(PBTrophyRoadEventCode.OnClaimReward, OnClaimTrophyRoadReward);
        GameEventHandler.AddActionEvent(BossFightEventCode.OnClaimReward, OnClaimBossReward);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnExitPreviousButton, OnExitPreviousButton);
        GameEventHandler.AddActionEvent(WinStreakPopup.OnAfterLoseItAnimation, OnAfterWinStreakLoseItAnimation);
        WinStreakCellUI.OnClaimNormalReward += OnClaimWinStreakNormalReward;
        UnlockBossScene.OnSkipClicked += OnUnlockBossSceneSkipClicked;
        PBShowingCardStateSO.OnSkipClicked += OnBonusCardSceneSkipClicked;
        PBSummaryStateSO.OnStateDisabledWithPack += OnOpenPackSummaryStateExited;
    }

    public void ShowAds(AdsLocation location, Action onCompleted = null, params string[] parameters)
    {
        if (!adsConfigSO.IsEnable)
            return;
        if (isEnoughMedalToShow && adsConfigSO.IsLocationEnabled(location))
        {
            if (AdsManager.Instance.IsReadyInterstitial)
            {
                NoAdsOffersPopup.Instance.ShowBeforeAds(() =>
                {
                    AdsManager.Instance.ShowInterstitialAd(location, onCompleted, parameters);
                });
            }
        }
    }

    private void OnClaimTrophyRoadReward()
    {
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);

        void OnUnpackDone()
        {
            GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
            ShowAds(AdsLocation.IS_AfterClaimingTrophyMilestones);
        }
    }

    private void OnClaimBossReward()
    {
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);

        void OnUnpackDone()
        {
            GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
            ShowAds(AdsLocation.IS_AfterClaimingBossRewards);
        }
    }

    private void OnClaimWinStreakNormalReward()
    {
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);

        void OnUnpackDone()
        {
            GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
            ShowAds(AdsLocation.IS_AfterClaimingWinStreakRewards);
        }
    }

    private void OnUnlockBossSceneSkipClicked()
    {
        ShowAds(AdsLocation.IS_AfterClickingClaimLaterBossUI);
    }

    private void OnBonusCardSceneSkipClicked()
    {
        ShowAds(AdsLocation.IS_AfterClickingLoseItBonusCard);
    }

    private void OnOpenPackSummaryStateExited()
    {
        ShowAds(AdsLocation.IS_AfterClickingTapToContinueOpenBox);
    }

    private void OnAfterWinStreakLoseItAnimation()
    {
        ShowAds(AdsLocation.IS_AfterClickingLoseItKeepWinStreakUI);
    }

    private void OnExitPreviousButton(params object[] objs)
    {
        var previousButtonType = (ButtonType)objs[0];
        if (previousButtonType == ButtonType.Character)
        {
            ShowAds(AdsLocation.IS_AfterExitingBuildUI);
        }
    }

    private void OnOpenTrophyRoadButtonSetupCompleted()
    {
        if (isBackFromPvP && matchIsVictory)
        {
            var trophyRoadUI = FindAnyObjectByType<PBTrophyRoadUI>();
            if (!trophyRoadUI.IsVisible)
            {
                if (modeVariable.value == Mode.Normal)
                {
                    ShowAds(AdsLocation.IS_AfterExitingDualGameOverUI);
                }
                else if (modeVariable.value == Mode.Boss)
                {
                    ShowAds(AdsLocation.IS_AfterExitingBossGameOverUI);
                }
                else if (modeVariable.value == Mode.Battle)
                {
                    ShowAds(AdsLocation.IS_AfterExitingBattleGameOverUI);
                }
            }
        }
    }

    private void HandleLoadSceneCompleted(params object[] objs)
    {
        if (objs[1] is not string originSceneName) return;
        isBackFromPvP = originSceneName == pvpSceneName.ToString() || originSceneName == pvpBossSceneName.ToString();
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        if (!matchOfPlayer.isAbleToComplete)
            return;
        matchIsVictory = matchOfPlayer.isVictory;
    }
}
