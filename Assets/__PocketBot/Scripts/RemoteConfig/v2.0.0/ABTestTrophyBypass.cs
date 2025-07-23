using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ABTestTrophyBypass", menuName = "PocketBots/ABTest/v2.0.0/ABTestTrophyBypass")]
public class ABTestTrophyBypass : GroupBasedABTestSO
{
    [SerializeField]
    private List<bool> m_DataGroups;

    public override void InjectData(int groupIndex)
    {
        BossInfoNode.IS_CHALLENGEABLE = m_DataGroups[groupIndex];
    }
}