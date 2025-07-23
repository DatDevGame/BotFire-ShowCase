using System.Collections;
using System.Collections.Generic;
using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class SeasonRewardCell : MonoBehaviour
{
    [SerializeField] SerializedDictionary<RewardType, float> sizeDic;
    [SerializeField] float defaultSize;

    public SeasonRewardPremiumBtn seasonRewardPremiumBtn;
    public SeasonRewardFreeBtn seasonRewardFreeBtn;

    bool isUnlocked;
    SeasonPassSO.Milestone milestone;
    SeasonRewardMilestoneImg milestoneImg;

    public SeasonRewardMilestoneImg MilestoneImg { get => milestoneImg; }

    private void Awake()
    {
        milestoneImg = GetComponentInChildren<SeasonRewardMilestoneImg>();
    }

    public virtual void Init(SeasonPassSO.Milestone milestone, int index)
    {
        this.milestone = milestone;
        seasonRewardPremiumBtn.Init(milestone, GetSize(milestone, false));
        seasonRewardFreeBtn.Init(milestone, GetSize(milestone, true));
        milestoneImg.Init(index);
        isUnlocked = false;
        TryToUnlock();
    }

    public Vector2 GetSize(SeasonPassSO.Milestone milestone, bool isFree)
    {
        var rewardType = isFree ? milestone.freeReward.type : milestone.premiumReward.type;
        if (sizeDic.TryGetValue(rewardType, out float size))
        {
            return new Vector2(size, size);
        }
        else
        {
            return new Vector2(defaultSize, defaultSize);
        }
    }

    public void TryToUnlock()
    {
        if (this.isUnlocked != milestone.Unlocked)
        {
            this.isUnlocked = milestone.Unlocked;
            seasonRewardPremiumBtn.UpdateView();
            seasonRewardFreeBtn.UpdateView();
        }
    }
}
