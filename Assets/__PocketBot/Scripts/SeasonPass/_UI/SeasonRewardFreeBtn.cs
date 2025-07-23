using HyrphusQ.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonRewardFreeBtn : MonoBehaviour
{
    [SerializeField] protected Button btn;
    [SerializeField] protected Image icon;
    [SerializeField] protected RectTransform iconRect;
    [SerializeField] protected TMP_Text amountTxt;
    [SerializeField] protected GameObject tickImg;
    [SerializeField] protected GameObject shineFX;

    protected SeasonPassSO.Milestone milestone;

    public virtual void Init(SeasonPassSO.Milestone milestone, Vector2 iconSize)
    {
        this.milestone = milestone;
        var reward = milestone.RewardFree();
        if (reward.currencyItems != null && reward.currencyItems.Count > 0)
        {
            var currencyPair = reward.currencyItems.First();
            icon.sprite = CurrencyManager.Instance.GetCurrencySO(currencyPair.Key).icon;
            amountTxt.text = currencyPair.Value.value.ToRoundedText();
        }
        else
        {
            var generalPair = reward.generalItems.First();
            icon.sprite = generalPair.Key.GetThumbnailImage();
            amountTxt.text = generalPair.Value.value.ToRoundedText();
        }
        iconRect.sizeDelta = iconSize;

        milestone.OnClaimedFree += UpdateView;
        btn.onClick.AddListener(OnButtonClick);
        UpdateView();
    }

    protected virtual void OnDestroy()
    {
        if (milestone != null)
        {
            milestone.OnClaimedFree -= UpdateView;
        }
        btn.onClick.RemoveListener(OnButtonClick);
    }

    protected virtual void OnButtonClick()
    {
        milestone.TryClaim(false);

        #region Firebase Event
        try
        {
            List<MissionData> dailyMission = SeasonPassManager.Instance.missionSavedDataSO.data.DailyMissions;
            List<MissionData> weeklyMission = SeasonPassManager.Instance.missionSavedDataSO.data.WeeklyMissions;
            List<MissionData> seasonMission = SeasonPassManager.Instance.missionSavedDataSO.data.SeasonMissions;

            string seasonType = "season";
            int rewardID = SeasonPassManager.Instance.seasonPassSO.milestones.IndexOf(milestone) + 1;
            int totalRewards = SeasonPassManager.Instance.seasonPassSO.milestones.Count;
            int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();
            string typeOfReward = "free";
            int todayCompleted = dailyMission.Where(v => v.isCompleted && !v.isRewardClaimed).Count();
            int weeklyCompleted = weeklyMission.Where(v => v.isCompleted && !v.isRewardClaimed).Count();
            int seasonCompleted = seasonMission.Where(v => v.isCompleted && !v.isRewardClaimed).Count();
            int missionsCompleted = MissionManager.Instance.GetAllMissionCompletedInSeason();
            int todayAvailable = dailyMission.Count;
            int weeklyAvailable = weeklyMission.Count;
            int seasonAvailable = seasonMission.Count;
#if UNITY_EDITOR
            Debug.Log($"LogFirebaseEventCode.ClaimSeasonReward: \n" +
                $"seasonType:{seasonType}\n" +
                $"missionsCompleted:{missionsCompleted}\n" +
                $"rewardID:{rewardID}\n" +
                $"totalRewards:{totalRewards}\n" +
                $"seasonID:{seasonID}\n" +
                $"typeOfReward:{typeOfReward}\n" +
                $"todayCompleted:{todayCompleted}\n" +
                $"weeklyCompleted:{weeklyCompleted}\n" +
                $"seasonCompleted:{seasonCompleted}\n" +
                $"todayAvailable:{todayAvailable}\n" +
                $"weeklyAvailable:{weeklyAvailable}\n" +
                $"seasonAvailable:{seasonAvailable}");
#endif
            GameEventHandler.Invoke(LogFirebaseEventCode.ClaimSeasonReward, seasonType, missionsCompleted, rewardID, totalRewards, seasonID, typeOfReward, todayCompleted, weeklyCompleted, seasonCompleted, todayAvailable, weeklyAvailable, seasonAvailable);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    public virtual void UpdateView()
    {
        if (milestone.Unlocked)
        {
            btn.enabled = true;
            if (!milestone.ClaimedFree)
            {
                shineFX.SetActive(true);
                tickImg.SetActive(false);
                btn.enabled = true;
                btn.interactable = true;
            }
            else
            {
                shineFX.SetActive(false);
                tickImg.SetActive(true);
                btn.enabled = true;
                btn.interactable = false;
            }
        }
        else
        {
            shineFX.SetActive(false);
            tickImg.SetActive(false);
            btn.enabled = false;
            btn.interactable = true;
        }
    }
}
