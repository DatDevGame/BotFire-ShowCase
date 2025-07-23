using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DrawCardProcedure;

[CreateAssetMenu(fileName = "DrawCardProcedureDataSO", menuName = "PocketBots/DrawCardProcedure/DrawCardProcedureDataSO")]
public class DrawCardProcedureDataSO : ScriptableObject
{
    [Serializable]
    public class Config
    {
        [SerializeField]
        private List<Group> m_DefaultGroupsRngInfo;

        public List<Group> CloneDefaultGroupsRngInfo(List<PBPartSO> filteredNewAvaiableGroup, List<PBPartSO> filteredDuplicateGroup, List<PBPartSO> filteredInUsedGroup)
        {
            var groupsRngInfo = m_DefaultGroupsRngInfo.Select(group =>
            {
                var clonedGroup = group.DeepClone();
                switch (group.GroupType)
                {
                    case GroupType.NewAvailable:
                        clonedGroup.PartSOs = filteredNewAvaiableGroup;
                        break;
                    case GroupType.Duplicate:
                        clonedGroup.PartSOs = filteredDuplicateGroup;
                        break;
                    case GroupType.InUsed:
                        clonedGroup.PartSOs = filteredInUsedGroup;
                        break;
                    default:
                        break;
                }
                return clonedGroup;
            }).ToList();
            // Remove all groups that are empty
            groupsRngInfo.RemoveAll(group => group.PartSOs == null || group.PartSOs.Count <= 0);
            return groupsRngInfo;
        }
    }

    [SerializeField]
    private Config m_Config;
    [SerializeField]
    private CurrentHighestArenaVariable m_CurrentHighestArenaVar;
    [SerializeField]
    private List<PBPartManagerSO> m_PartManagerSOs;

    public Config config => m_Config;
    public PBGachaPack currentUltraGachaBox => m_CurrentHighestArenaVar.value.gachaPackCollection.PackRngInfos.Find(pack => pack.pack.name.Contains("Ultra")).pack.Cast<PBGachaPack>();
    public List<PBPartManagerSO> partManagerSOs => m_PartManagerSOs;
}