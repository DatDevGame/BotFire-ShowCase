using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SeasonPreludeProgressSection : MonoBehaviour
{
    [SerializeField] SeasonProgressNode progressNode;
    [SerializeField] Transform progressNodeContainer;
    [SerializeField] TMP_Text progressTxt;

    int claimedNodeCount = 0;
    int maxNodeCount = 0;
    List<SeasonProgressNode> progressNodes = new();
    List<MissionData> preludeMissions => SeasonPassManager.Instance.missionSavedDataSO.data.PreludeMissions;

    public bool isCompleted => claimedNodeCount >= maxNodeCount;

    public void Init()
    {
        maxNodeCount = preludeMissions.Count;
        var claimedRewardMissions = preludeMissions.FindAll(x => x.isRewardClaimed);
        for (var i = 0; i < preludeMissions.Count; i++)
        {
            var isRewardClaimed = i < claimedRewardMissions.Count;
            var node = Instantiate(progressNode, progressNodeContainer);
            node.Init(isRewardClaimed);
            progressNodes.Add(node);
            if (isRewardClaimed)
            {
                claimedNodeCount++;
            }
        }
        progressTxt.text = $"{claimedNodeCount}/{maxNodeCount}";
    }

    public void IncreaseNode()
    {
        if (claimedNodeCount < maxNodeCount)
        {
            claimedNodeCount++;
            progressNodes[claimedNodeCount - 1].IncreaseFill();
            progressTxt.text = $"{claimedNodeCount}/{maxNodeCount}";
        }
    }
}
