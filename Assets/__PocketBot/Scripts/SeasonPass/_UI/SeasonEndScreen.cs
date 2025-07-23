using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.HDROutputUtils;

public class SeasonEndScreen : MonoBehaviour
{
    [SerializeField] CanvasGroupVisibility visibility;
    [SerializeField] SeasonRewardUI seasonRewardUI;
    [SerializeField] LocalizationParamsManager stepTxt;
    [SerializeField] Button okBtn;
    [SerializeField] Button claimAllBtn;
    [SerializeField] Button seasonPassBtn;

    bool isShowing;
    SeasonPassSO seasonPassSO => SeasonPassManager.Instance.seasonPassSO;
    Coroutine trackUltimatePackPopupCoroutine;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(SeasonPassEventCode.ShowEndSeasonPopup, Show);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.HideEndSeasonPopup, Hide);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.OnPurchaseSeasonPass, UpdateView);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        okBtn.onClick.AddListener(OnClickedOk);
        claimAllBtn.onClick.AddListener(OnClickedClaimAll);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.ShowEndSeasonPopup, Show);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.HideEndSeasonPopup, Hide);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.OnPurchaseSeasonPass, UpdateView);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
    }

    void Show()
    {
        visibility.Show();
        seasonRewardUI.InitOrRefresh();
        seasonRewardUI.FocusCurrentMilestone();
        UpdateView();
        isShowing = true;
        TrackUltimatePackPopup();
    }

    void Hide()
    {
        if (trackUltimatePackPopupCoroutine != null)
        {
            StopCoroutine(trackUltimatePackPopupCoroutine);
        }
        visibility.Hide();
        isShowing = false;
    }

    void TrackUltimatePackPopup()
    {
        if (trackUltimatePackPopupCoroutine != null)
        {
            StopCoroutine(trackUltimatePackPopupCoroutine);
        }
        trackUltimatePackPopupCoroutine = StartCoroutine(CR_TrackUltimatePackPopup());
    }

    IEnumerator CR_TrackUltimatePackPopup()
    {
        yield return new WaitUntil(() => PBUltimatePackPopup.Instance.IsShowing);
        visibility.HideImmediately();
        yield return new WaitUntil(() => !PBUltimatePackPopup.Instance.IsShowing);
        visibility.ShowImmediately();
    }

    void UpdateView()
    {
        var unlockedMilestone = SeasonPassManager.Instance.seasonPassSO.milestones.FindAll(x => x.Unlocked);
        var unclaimedMilestoneFree = unlockedMilestone.FindAll(x => !x.ClaimedFree);
        var unclaimedMilestonePremium = unlockedMilestone.FindAll(x => !x.ClaimedPremium);
        var isPurchasedSeasonPass = SeasonPassManager.Instance.seasonPassSO.isPurchased;
        stepTxt.SetParameterValue("Index", unlockedMilestone.Count.ToString());
        bool isShowOkBtn = unclaimedMilestoneFree.Count <= 0 && (!isPurchasedSeasonPass || unclaimedMilestonePremium.Count <= 0);
        okBtn.gameObject.SetActive(isShowOkBtn);
        claimAllBtn.gameObject.SetActive(!isShowOkBtn);
        seasonPassBtn.gameObject.SetActive(!isPurchasedSeasonPass);
    }

    void OnClickedOk()
    {
        SeasonPassManager.Instance.CheckToNewSeason();
        GameEventHandler.Invoke(SeasonPassEventCode.ResetSeasonUI);
        seasonRewardUI.Reset();
        GameEventHandler.Invoke(SeasonPassEventCode.HideEndSeasonPopup);

        #region Progression Event
        string status = "Complete";
        string content = "Season";
        GameEventHandler.Invoke(ProgressionEvent.Progression, status, content);
        #endregion
    }

    void OnClickedClaimAll()
    {
        var isPurchased = seasonPassSO.isPurchased;
        List<RewardGroupInfo> rewards = new List<RewardGroupInfo>();
        foreach (var milestone in seasonPassSO.milestones)
        {
            if (milestone.Unlocked)
            {
                if (!milestone.ClaimedFree)
                {
                    rewards.Add(milestone.RewardFree());
                    milestone.ClaimedFree = true;
                }

                if (isPurchased && !milestone.ClaimedPremium)
                {
                    rewards.Add(milestone.RewardPremium());
                    milestone.ClaimedPremium = true;
                }
            }
        }

        ((PBGachaCardGenerator)GachaCardGenerator.Instance).GenerateRewards(rewards, out List<GachaCard> gachaCards, out List<GachaPack> gachaPacks);
        foreach (var card in gachaCards)
        {
            if (card is GachaCard_Currency gachaCard_Currency)
            {
                //TODO: Fix ResourceLocation type (wait for design)
                gachaCard_Currency.ResourceLocationProvider = new ResourceLocationProvider(ResourceLocation.TrophyRoad, "Season Claim All");
            }

            if (card is GachaCard_Skin gachaCard_Skin)
            {
                #region Progression Event
                try
                {
                    if (gachaCard_Skin != null && gachaCard_Skin.SkinSO != null)
                    {
                        string input = $"{gachaCard_Skin.SkinSO.name}";
                        string[] parts = input.Split('_');
                        if (parts.Length > 1 && int.TryParse(parts[1], out int skinID))
                        {
                            string status = ProgressionEventStatus.Start;
                            string partname_SkinID = $"{gachaCard_Skin.SkinSO.partSO.GetDisplayName()}-{skinID}";
                            string currentcyType = "Season";
                            GameEventHandler.Invoke(ProgressionEvent.BuySkin, status, partname_SkinID, currentcyType);
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
        }

        #region Log GA
        try
        {
            if (gachaPacks != null)
            {
                gachaPacks.ForEach(pack =>
                {
                    if (pack != null)
                    {
                        #region DesignEvent
                        string openStatus = "NoTimer";
                        string location = "Season";
                        GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
                        #endregion

                        #region Firebase Event
                        GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, pack, "free");
                        #endregion

                        Debug.Log($"Open Box - {location}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion

        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, gachaCards, gachaPacks, null);
    }

    void OnUnpackStart()
    {
        if (isShowing)
            visibility.HideImmediately();
    }

    void OnUnpackDone()
    {
        if (isShowing)
        {
            visibility.ShowImmediately();
            UpdateView();
        }
    }
}
