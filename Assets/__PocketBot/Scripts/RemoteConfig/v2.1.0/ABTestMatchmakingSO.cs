using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class OverallScoreClampGroup
{
    [TableList]
    public List<OverallScoreClamp> overallScoreClamps;
}
[CreateAssetMenu(fileName = "ABTestMatchmakingSO", menuName = "PocketBots/ABTest/ABTestMatchmakingSO")]
public class ABTestMatchmakingSO : GroupBasedABTestSO
{
    [SerializeField]
    private PBPvPMatchMakingSO matchmakingSO;
    [SerializeField]
    private List<OverallScoreClampGroup> overallScoreClampGroups = new List<OverallScoreClampGroup>(new OverallScoreClampGroup[4]);

    public override void InjectData(int groupIndex)
    {
        matchmakingSO.OverallScoreClamps = overallScoreClampGroups[groupIndex].overallScoreClamps;
    }

    public void SetOverallScoreClampOfGroup(int groupIndex, List<OverallScoreClamp> overallScoreClamps)
    {
        overallScoreClampGroups[groupIndex].overallScoreClamps = overallScoreClamps;
    }
}