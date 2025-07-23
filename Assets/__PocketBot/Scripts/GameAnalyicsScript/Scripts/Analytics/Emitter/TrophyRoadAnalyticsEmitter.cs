using HightLightDebug;
using HyrphusQ.Events;
using LatteGames.Analytics;
using LatteGames.PvP.TrophyRoad;
using PBAnalyticsEvents;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrophyRoadAnalyticsEmitter : MonoBehaviour
{
    [SerializeField, BoxGroup("Data")] private TrophyRoadSO m_TrophyRoadSO;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable _pprefTotalTimeUnlockMistone;

    private void Start()
    {
        string keyCallTheFirstReachMilesStone = $"ReachMilestone-TheFirstTime@@";
        if (!PlayerPrefs.HasKey(keyCallTheFirstReachMilesStone))
        {
            PlayerPrefs.SetInt(keyCallTheFirstReachMilesStone, 1);
            PBAnalyticsManager.Instance.ReachMilestone("Start", 1, _pprefTotalTimeUnlockMistone.value);
        }

        OnTrophyRoadInitialized();
    }

    private void OnTrophyRoadInitialized()
    {
        if (m_TrophyRoadSO == null || m_TrophyRoadSO.ArenaSections == null)
            return;
        foreach (var arenaSection in m_TrophyRoadSO.ArenaSections)
        {
            foreach (var milestone in arenaSection.milestones)
            {
                milestone.OnUnlocked += OnMilestoneUnlocked;

                void OnMilestoneUnlocked()
                {
                    milestone.OnUnlocked -= OnMilestoneUnlocked;
                    PBAnalyticsManager.Instance.ReachMilestone("Complete", (m_TrophyRoadSO as PBTrophyRoadSO).GetCurrentMilestoneIndex(), _pprefTotalTimeUnlockMistone.value);
                    PBAnalyticsManager.Instance.ReachMilestone("Start", (m_TrophyRoadSO as PBTrophyRoadSO).GetCurrentMilestoneIndex() + 1, _pprefTotalTimeUnlockMistone.value);
                    _pprefTotalTimeUnlockMistone.value = 0;
                }
            }
        }
    }
}