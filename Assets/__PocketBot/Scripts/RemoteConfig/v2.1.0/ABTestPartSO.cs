using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class ABTestPartData
{
    public ABTestPartData(PBPartSO partSO)
    {
        this.partSO = partSO;
        this.foundInArena = partSO.foundInArena;
        this.trophyThreshold = partSO.TrophyThreshold;
        this.attackUpgradeSteps = new List<int>(partSO.UpgradePath.attackUpgradeSteps);
        this.hpUpgradeSteps = new List<int>(partSO.UpgradePath.hpUpgradeSteps);
        this.requiredNumOfCardsLevels = new List<int>(partSO.GetModule<PBUpgradableItemModule>().GetUpgradeRequirementData().requiredNumOfCardsLevels);
    }

    public PBPartSO partSO;
    public int foundInArena;
    public int trophyThreshold;
    public List<int> attackUpgradeSteps;
    public List<int> hpUpgradeSteps; 
    public List<int> requiredNumOfCardsLevels;

    public void InjectData()
    {
        if (partSO == null)
            return;
        partSO.foundInArena = foundInArena;
        partSO.TrophyThreshold = trophyThreshold;
        partSO.UpgradePath.attackUpgradeSteps = new List<int>(attackUpgradeSteps);
        partSO.UpgradePath.hpUpgradeSteps = new List<int>(hpUpgradeSteps);
        if (partSO.TryGetModule(out PBUpgradableItemModule upgradableModule))
        {
            var upgradeRequirementData = upgradableModule.GetUpgradeRequirementData();
            upgradeRequirementData.requiredNumOfCardsLevels = new List<int>(requiredNumOfCardsLevels);
        }
    }
}
[Serializable]
public class ABTestPartDataGroup
{
    [TableList]
    public List<ABTestPartData> dataOfParts = new List<ABTestPartData>();
}
[CreateAssetMenu(fileName = "ABTestPartSO", menuName = "PocketBots/ABTest/v2.1.0/ABTestPartSO")]
public class ABTestPartSO : GroupBasedABTestSO
{
    [SerializeField]
    private List<ABTestPartDataGroup> partDataGroups = new List<ABTestPartDataGroup>(new ABTestPartDataGroup[4]);

    public override void InjectData(int groupIndex)
    {
        var partDataGroup = partDataGroups[groupIndex];
        partDataGroup.dataOfParts.ForEach(partData => partData.InjectData());
    }

#if UNITY_EDITOR
    // Editor only
    public void RetrieveData(int groupIndex, List<PBPartSO> partSOs)
    {
        var partDataGroup = partDataGroups[groupIndex];
        partDataGroup.dataOfParts.Clear();
        foreach (var partSO in partSOs)
        {
            partDataGroup.dataOfParts.Add(new ABTestPartData(partSO));
        }
    }
#endif
}