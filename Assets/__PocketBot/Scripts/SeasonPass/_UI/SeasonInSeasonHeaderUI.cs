using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.Tab;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonInSeasonHeaderUI : MonoBehaviour
{
    [SerializeField] TMP_Text seasonCooldownTxt;
    [SerializeField, BoxGroup("Token Progress")] CurrencySO tokenSO;
    [SerializeField, BoxGroup("Token Progress")] SeasonPassSO seasonPassSO;
    [BoxGroup("Token Progress")] public EZAnimVector3 milestoneAnim;
    [BoxGroup("Token Progress")] public TMP_Text milestoneTxt;
    [BoxGroup("Token Progress")] public Transform tokenIcon;
    [BoxGroup("Token Progress")] public EZAnimVector3 tokenIconAnim;
    [BoxGroup("Token Progress")] public TMP_Text tokenProgressTxt;
    [BoxGroup("Token Progress")] public Slider slider;
    [BoxGroup("Token Progress")] public AnimationCurve increaseTokenCurve;
    [BoxGroup("Token Progress")] public float increaseTokenDuration = 1.5f;
    [BoxGroup("RewardTab")] public TMP_Text claimableMilestoneAmountTxt;
    [BoxGroup("RewardTab")] public GameObject claimableMilestoneAmountObj;
    [BoxGroup("RewardTab")] public CanvasGroupVisibility checkYourProgressVisibility;
    [BoxGroup("TabSystem")] public SeasonTabButton rewardTabButton;
    [BoxGroup("TabSystem")] public GameObject missionTabActiveGroup;
    [BoxGroup("TabSystem")] public GameObject rewardTabActiveGroup;
    [BoxGroup("TabSystem")] public TabSystem tabSystem;
    [SerializeField, BoxGroup("Log Event")] private PPrefDatetimeVariable m_TimeCompleteAMilestone;

    int currentMilestoneIndex = 0;
    float currentToken = 0;

    public void UpdateView()
    {
        seasonCooldownTxt.text = SeasonPassManager.Instance.endSeasonRemainingTime;
    }

    public void SwitchToDefaultTab()
    {
        tabSystem.ActiveDefaultTab();
    }

    public void Init()
    {
        UpdateTokenProgress(tokenSO.value, false);
        UpdateRewardTab();
        tokenSO.onValueChanged += OnTokenChanged;
        SeasonPassSO.Milestone.OnClaimedAny += OnClaimedAnyMilestone;
        SeasonPassSO.Milestone.OnUnlockedAny += OnUnlockedAny;
        SeasonTabButton.OnSetStateActive += OnSeasonTabButtonSetActive;
        GameEventHandler.AddActionEvent(SeasonPassEventCode.OnPurchaseSeasonPass, OnPurchaseSeasonPass);

        #region ProgressionEvent Event
        int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();
        string keyTheFirstTime = $"TheFirstTime-SeasonRewardUI-{seasonID}";
        if (!PlayerPrefs.HasKey(keyTheFirstTime))
        {
            PlayerPrefs.SetInt(keyTheFirstTime, 1);
            m_TimeCompleteAMilestone.value = DateTime.Now;
        }
        #endregion 
    }

    private void OnDestroy()
    {
        tokenSO.onValueChanged -= OnTokenChanged;
        SeasonPassSO.Milestone.OnClaimedAny -= OnClaimedAnyMilestone;
        SeasonPassSO.Milestone.OnUnlockedAny -= OnUnlockedAny;
        SeasonTabButton.OnSetStateActive -= OnSeasonTabButtonSetActive;
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.OnPurchaseSeasonPass, OnPurchaseSeasonPass);
    }

    void OnPurchaseSeasonPass()
    {
        UpdateRewardTab();
    }

    void OnSeasonTabButtonSetActive(SeasonTabButton seasonTabButton)
    {
        if (seasonTabButton == rewardTabButton)
        {
            checkYourProgressVisibility.HideImmediately();
        }
        else
        {
            checkYourProgressVisibility.ShowImmediately();
        }
        missionTabActiveGroup.SetActive(seasonTabButton != rewardTabButton);
        rewardTabActiveGroup.SetActive(seasonTabButton == rewardTabButton);
    }

    void OnClaimedAnyMilestone(SeasonPassSO.Milestone milestone)
    {
        SeasonPassManager.Instance.seasonPassSO.data.firstTimeEarnReward = true;
        UpdateRewardTab();
    }

    void OnUnlockedAny(SeasonPassSO.Milestone milestone)
    {
        UpdateRewardTab();
    }

    void OnTokenChanged(ValueDataChanged<float> data)
    {
        RunTokenProgressAnim();
    }

    Coroutine runTokenProgressAnimCoroutine;
    void RunTokenProgressAnim()
    {
        if (runTokenProgressAnimCoroutine != null)
        {
            StopCoroutine(runTokenProgressAnimCoroutine);
        }
        runTokenProgressAnimCoroutine = StartCoroutine(CR_RunTokenProgressAnim());
    }

    IEnumerator CR_RunTokenProgressAnim()
    {
        yield return new WaitForSeconds(AnimationDuration.SSHORT);
        float t = 0;
        var startToken = currentToken;
        while (t < 1)
        {
            t += Time.deltaTime / increaseTokenDuration;
            UpdateTokenProgress(Mathf.Lerp(startToken, tokenSO.value, increaseTokenCurve.Evaluate(t)), true);
            yield return null;
        }
    }

    void UpdateRewardTab()
    {
        var isPurchased = SeasonPassManager.Instance.seasonPassSO.isPurchased;
        var claimableMilestones = SeasonPassManager.Instance.seasonPassSO.milestones.FindAll(x => x.Unlocked && (!x.ClaimedFree || (isPurchased && !x.ClaimedPremium)));
        if (claimableMilestones.Count > 0)
        {
            claimableMilestoneAmountTxt.text = claimableMilestones.Count.ToString();
            claimableMilestoneAmountObj.gameObject.SetActive(true);
            if (!SeasonPassManager.Instance.seasonPassSO.data.firstTimeEarnReward)
            {
                checkYourProgressVisibility.gameObject.SetActive(true);
            }
            else
            {
                checkYourProgressVisibility.gameObject.SetActive(false);
            }
            UnreadManager.Instance.AddUnreadTag(UnreadType.Dot, UnreadLocation.SeasonUnlockedMilestones, gameObject);
        }
        else
        {
            claimableMilestoneAmountObj.gameObject.SetActive(false);
            checkYourProgressVisibility.gameObject.SetActive(false);
            UnreadManager.Instance.RemoveUnreadTag(UnreadType.Dot, UnreadLocation.SeasonUnlockedMilestones, gameObject);
        }
    }

    void UpdateTokenProgress(float tokenAmount, bool isPlayAnimation)
    {
        currentToken = tokenAmount;
        var remainderProgress = 0f;
        var remainderTokenAmount = 0f;
        var remainderTokenTotalAmount = 0f;
        bool isUpMilestone = false;
        bool isOutMilestone = true;
        for (var i = 0; i < seasonPassSO.milestones.Count; i++)
        {
            var mileStone = seasonPassSO.milestones[i];
            mileStone.TryUnlock(currentToken);

            #region ProgressionEvent Event
            try
            {
                if (mileStone.Unlocked)
                    LogEventSeasonReward(i);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion

            #region Firebase Event
            try
            {
                if (mileStone.Unlocked)
                    LogFirebaseEventSeasonReward(mileStone, i);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion

            if (tokenAmount < mileStone.requiredAmount)
            {
                isUpMilestone = i > currentMilestoneIndex;
                isOutMilestone = false;
                currentMilestoneIndex = i;
                var previousSeasonCurrency = i > 0 ? seasonPassSO.milestones[i - 1].requiredAmount : 0;
                remainderTokenAmount = tokenAmount - previousSeasonCurrency;
                remainderTokenTotalAmount = mileStone.requiredAmount - previousSeasonCurrency;
                remainderProgress = remainderTokenAmount / remainderTokenTotalAmount;
                break;
            }
            else if (i < seasonPassSO.milestones.Count - 1 && tokenAmount == mileStone.requiredAmount)
            {
                isUpMilestone = i + 1 > currentMilestoneIndex;
                isOutMilestone = false;
                currentMilestoneIndex = i + 1;
                var nextSeasonCurrency = i < seasonPassSO.milestones.Count - 1 ? seasonPassSO.milestones[i + 1].requiredAmount : seasonPassSO.milestones[i].requiredAmount;
                remainderTokenAmount = 0;
                remainderTokenTotalAmount = nextSeasonCurrency - mileStone.requiredAmount;
                remainderProgress = 0;
                break;
            }
        }

        if (isOutMilestone)
        {
            isUpMilestone = seasonPassSO.milestones.Count > currentMilestoneIndex;
            currentMilestoneIndex = seasonPassSO.milestones.Count - 1;
            remainderProgress = 1;
        }

        slider.value = remainderProgress;
        tokenProgressTxt.text = isOutMilestone ? I2LHelper.TranslateTerm(I2LTerm.Text_Max) : $"{Mathf.RoundToInt(remainderTokenAmount)}/{Mathf.RoundToInt(remainderTokenTotalAmount)}";
        milestoneTxt.text = (currentMilestoneIndex + 1).ToString();
        if (isUpMilestone && isPlayAnimation)
        {
            milestoneAnim.Play();
        }
    }


    private void LogEventSeasonReward(int rewardIndex)
    {
        string status = ProgressionEventStatus.Start;
        int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();
        int mileStoneID = rewardIndex + 1;
        int PlayedTimeToCompleteAMilestone = GetTotalTimeCompleteMilestone();
        string keyEvent = $"SeasonReward-{seasonID}-{mileStoneID}-{GetYear()}";

        if (!PlayerPrefs.HasKey(keyEvent))
        {
            PlayerPrefs.SetInt(keyEvent, 1);
            GameEventHandler.Invoke(ProgressionEvent.Season, status, seasonID, mileStoneID, PlayedTimeToCompleteAMilestone);
            m_TimeCompleteAMilestone.value = DateTime.Now;
        }

        int GetTotalTimeCompleteMilestone()
        {
            TimeSpan timeSpan = DateTime.Now - m_TimeCompleteAMilestone.value;
            int totalSeconds = (int)timeSpan.TotalSeconds;
            return totalSeconds;
        }
        int GetYear() => DateTime.Now.Year;
    }

    private void LogFirebaseEventSeasonReward(SeasonPassSO.Milestone milestone, int index)
    {
        List<MissionData> dailyMission = SeasonPassManager.Instance.missionSavedDataSO.data.DailyMissions;
        List<MissionData> weeklyMission = SeasonPassManager.Instance.missionSavedDataSO.data.WeeklyMissions;
        List<MissionData> seasonMission = SeasonPassManager.Instance.missionSavedDataSO.data.SeasonMissions;

        string seasonType = "season";
        int missionsCompleted = MissionManager.Instance.GetAllMissionCompletedInSeason();
        int rewardID = index + 1;
        int totalRewards = SeasonPassManager.Instance.seasonPassSO.milestones.Count;
        int seasonID = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex();
        string typeOfReward = milestone.ClaimedPremium ? "premium" : "free";
        int todayCompleted = dailyMission.Where(v => v.isCompleted && !v.isRewardClaimed).Count();
        int weeklyCompleted = weeklyMission.Where(v => v.isCompleted && !v.isRewardClaimed).Count();
        int seasonCompleted = seasonMission.Where(v => v.isCompleted && !v.isRewardClaimed).Count();
        int todayAvailable = dailyMission.Count;
        int weeklyAvailable = weeklyMission.Count;
        int seasonAvailable = seasonMission.Count;

        string keyEvent = $"AvailableSeasonReward-{typeOfReward}-{seasonID}-{rewardID}-{GetYear()}";
        if (!PlayerPrefs.HasKey(keyEvent))
        {
#if UNITY_EDITOR
            Debug.Log($"LogFirebaseEventCode.AvailableSeasonReward:\n" +
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

            PlayerPrefs.SetInt(keyEvent, 1);
            GameEventHandler.Invoke(LogFirebaseEventCode.AvailableSeasonReward, seasonType, missionsCompleted, rewardID, totalRewards, seasonID, typeOfReward, todayCompleted, weeklyCompleted, seasonCompleted, todayAvailable, weeklyAvailable, seasonAvailable);
        }
        if (SeasonPassManager.Instance.seasonPassSO.isPurchased)
        {
            typeOfReward = "premium";
            keyEvent = $"AvailableSeasonReward-{typeOfReward}-{seasonID}-{rewardID}-{GetYear()}";
            if (!PlayerPrefs.HasKey(keyEvent))
            {
#if UNITY_EDITOR
                Debug.Log($"LogFirebaseEventCode.AvailableSeasonReward:\n" +
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

                PlayerPrefs.SetInt(keyEvent, 1);
                GameEventHandler.Invoke(LogFirebaseEventCode.AvailableSeasonReward, seasonType, missionsCompleted, rewardID, totalRewards, seasonID, typeOfReward, todayCompleted, weeklyCompleted, seasonCompleted, todayAvailable, weeklyAvailable, seasonAvailable);
            }
        }

        int GetYear() => DateTime.Now.Year;
    }
}
