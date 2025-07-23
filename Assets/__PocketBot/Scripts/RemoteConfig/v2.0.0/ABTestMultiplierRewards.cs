using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ABTestMultiplierRewards", menuName = "PocketBots/ABTest/v2.0.0/ABTestMultiplierRewards")]
public class ABTestMultiplierRewards : GroupBasedABTestSO
{
    [SerializeField]
    private List<MultiplierRewardsDataSO.Config> m_DataGroups = new List<MultiplierRewardsDataSO.Config>();
    [SerializeField]
    private MultiplierRewardsDataSO m_MultiplierRewardsDataSO;

    public override void InjectData(int groupIndex)
    {
        m_MultiplierRewardsDataSO.config.DeepCopy(m_DataGroups[groupIndex]);
    }
}