using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ABTestBossData
{
    [Serializable]
    public class StatsModifier
    {
        public PBPartSO partSO;
        public int upgradeStep;
        public PBStatID statID = PBStatID.Attack;

        public void InjectData()
        {
            if (statID == PBStatID.Attack)
            {
                partSO.UpgradePath.attackUpgradeSteps[0] = upgradeStep;
            }
            else if (statID == PBStatID.Health)
            {
                partSO.UpgradePath.hpUpgradeSteps[0] = upgradeStep;
            }
        }
    }

    [ShowInInspector, PropertyOrder(-1)]
    public PBChassisSO bodySO => statsModifiers == null || statsModifiers.Count <= 0 ? null : statsModifiers.Find(item => item.partSO.PartType == PBPartType.Body)?.partSO?.Cast<PBChassisSO>();
    public List<StatsModifier> statsModifiers;

    public void InjectData()
    {
        for (int i = 0; i < statsModifiers.Count; i++)
        {
            statsModifiers[i].InjectData();
        }
    }
}
[Serializable]
public class ABTestBossDataGroup
{
    public List<ABTestBossData> bossesData;

    public void InjectData()
    {
        for (int i = 0; i < bossesData.Count; i++)
        {
            bossesData[i].InjectData();
        }
    }
}
[CreateAssetMenu(fileName = "ABTestBossSO", menuName = "PocketBots/ABTest/v2.1.0/ABTestBossSO")]
public class ABTestBossSO : GroupBasedABTestSO
{
    [SerializeField]
    private List<ABTestBossDataGroup> groups;

    public override void InjectData(int groupIndex)
    {
        groups[groupIndex].InjectData();
    }
}