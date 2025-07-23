using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LatteGames.PvP.TrophyRoad;
using Sirenix.OdinInspector;
using UnityEngine;

public class GachaPoolManager : Singleton<GachaPoolManager>
{
    [Serializable]
    public class DebugPart
    {
        public DebugPart(PBPartSO partSO)
        {
            this.partSO = partSO;
            var milestone = GetFirstMilestoneOfPart(partSO);
            if (milestone != null)
            {
                this.status = $"Unlock at ({Instance.trophyRoadSO.HighestAchievedMedals}/{milestone.requiredAmount}) {(milestone.Unlocked ? "Unlocked" : "NOT Unlocked")}";
            }
            else
            {
                this.status = $"Unlock at ({Instance.trophyRoadSO.HighestAchievedMedals}/{partSO.TrophyThreshold})";
            }
        }

        [HorizontalGroup, LabelText("")]
        public PBPartSO partSO;
        [HorizontalGroup, LabelText("")]
        public string status;

        public int SortOrder
        {
            get
            {
                var milestone = GetFirstMilestoneOfPart(partSO);
                if (milestone != null)
                {
                    return Mathf.RoundToInt(milestone.requiredAmount);
                }
                else
                {
                    return partSO.TrophyThreshold;
                }
            }
        }
    }

    [SerializeField]
    private GachaPoolDataStorageSO dataStorageSO;
    [SerializeField]
    private TrophyRoadSO trophyRoadSO;
    [SerializeField]
    private CurrentHighestArenaVariable currentArenaVariable;
    [SerializeField]
    private PBPartManagerSO[] partManagerSOs;

#if UNITY_EDITOR
    [ShowInInspector, FoldoutGroup("Infos")]
    public static List<DebugPart> AvailableParts
    {
        get
        {
            if (Instance == null)
                return null;
            var availableParts = new List<DebugPart>();
            foreach (var partManagerSO in Instance.partManagerSOs)
            {
                foreach (PBPartSO partSO in partManagerSO)
                {
                    if (partSO.IsAvailable())
                        availableParts.Add(new DebugPart(partSO));
                }
            }
            availableParts.Sort((x, y) => x.SortOrder.CompareTo(y.SortOrder));
            return availableParts;
        }
    }
    [ShowInInspector, FoldoutGroup("Infos")]
    public static List<DebugPart> NotAvailableParts
    {
        get
        {
            if (Instance == null)
                return null;
            var notAvailableParts = new List<DebugPart>();
            foreach (var partManagerSO in Instance.partManagerSOs)
            {
                foreach (PBPartSO partSO in partManagerSO)
                {
                    if (!partSO.IsAvailable())
                        notAvailableParts.Add(new DebugPart(partSO));
                }
            }
            notAvailableParts.Sort((x, y) => x.SortOrder.CompareTo(y.SortOrder));
            return notAvailableParts;
        }
    }
#endif

    public static bool IsAvailable(PBPartSO partSO)
    {
        if (partSO.IsBossPart)
            return false;
        // var milestone = GetFirstMilestoneOfPart(partSO);
        // if (milestone != null)
        // {
        //     return milestone.Unlocked;
        // }
        // else
        // {
        //     return Instance.trophyRoadSO.HighestAchievedMedals >= partSO.TrophyThreshold;
        // }
        return Instance.trophyRoadSO.HighestAchievedMedals >= partSO.TrophyThreshold;
    }

    public static bool IsInTrophyRoad(PBPartSO partSO)
    {
        return GetFirstMilestoneOfPart(partSO) != null;
    }

    public static TrophyRoadSO.Milestone GetFirstMilestoneOfPart(PBPartSO partSO)
    {
        var arenaSections = Instance.trophyRoadSO.ArenaSections;
        foreach (var section in arenaSections)
        {
            foreach (var milestone in section.milestones)
            {
                if (milestone.reward.generalItems.ContainsKey(partSO))
                    return milestone;
            }
        }
        return null;
    }

    // Gacha Pool
    public static List<PBPartSO> GetAllAvailableParts(Predicate<PBPartSO> predicate = null)
    {
        var availableParts = new List<PBPartSO>();
        foreach (var partManagerSO in Instance.partManagerSOs)
        {
            foreach (PBPartSO partSO in partManagerSO)
            {
                if (partSO.IsAvailable() && !partSO.IsBossPart && (predicate?.Invoke(partSO) ?? true))
                    availableParts.Add(partSO);
            }
        }
        return availableParts;
    }

    public static PBPartSO GetRandomDuplicatePart(Predicate<PBPartSO> predicate = null)
    {
        var availableDuplicateParts = GetAllAvailableParts(partSO => partSO.IsUnlocked() && (predicate?.Invoke(partSO) ?? true));
        if (availableDuplicateParts.Count <= 0)
            return null;
        var currentArenaSO = Instance.currentArenaVariable.value.Cast<PBPvPArenaSO>();
        var currentClassicGachaPackSO = currentArenaSO.gachaPackCollection.PackRngInfos[0].pack as PBGachaPack;
        var partDropRates = currentClassicGachaPackSO.GetGearDropRates().Where(gearDropRate => availableDuplicateParts.Contains(gearDropRate.PartSO)).ToList();
        return partDropRates.GetRandomRedistribute().PartSO;
    }

    public static PBPartSO GetRandomDuplicatePartByRarity(RarityType rarityType)
    {
        return GetRandomDuplicatePart(partSO => partSO.GetRarityType() == rarityType);
    }

    // Waiting & Queue Pool
    public static void EnqueuePriorityPart(PBPartSO partSO)
    {
        Instance.dataStorageSO.EnqueuePriorityPart(partSO);
    }

    public static PBPartSO DequeueAvailablePriorityPart()
    {
        return Instance.dataStorageSO.DequeueAvailablePriorityPart();
    }

    public static bool TryDequeueAvailablePriorityPart(out PBPartSO availablePriorityPartSO)
    {
        availablePriorityPartSO = DequeueAvailablePriorityPart();
        return availablePriorityPartSO != null;
    }

    public static List<PBPartSO> GetAvailablePriorityPartQueue()
    {
        return Instance.dataStorageSO.GetAvailablePriorityPartQueue();
    }
}