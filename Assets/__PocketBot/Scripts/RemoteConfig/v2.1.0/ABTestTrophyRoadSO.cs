using LatteGames.PvP.TrophyRoad;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ABTestTrophyRoadSO", menuName = "PocketBots/ABTest/v2.1.0/ABTestTrophyRoadSO")]
public class ABTestTrophyRoadSO : GroupBasedABTestSO
{
    [Serializable]
    public class ABTestTrophyRoadData
    {
        public List<TrophyRoadSO.ArenaSection> arenaSections;
    }

    [SerializeField]
    private PBTrophyRoadSO trophyRoadSO;
    [SerializeField]
    private List<ABTestTrophyRoadData> groups;

    public override void InjectData(int groupIndex)
    {
        trophyRoadSO.SetArenaSections(groups[groupIndex].arenaSections);
    }

#if UNITY_EDITOR
    public virtual void RetrieveData(int groupIndex)
    {
        groups[groupIndex].arenaSections = new List<TrophyRoadSO.ArenaSection>(trophyRoadSO.ArenaSections);
    }
#endif
}
