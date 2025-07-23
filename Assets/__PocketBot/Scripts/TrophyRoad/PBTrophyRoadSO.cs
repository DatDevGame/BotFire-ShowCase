using System.Collections.Generic;
using UnityEngine;
using LatteGames.PvP.TrophyRoad;
using System.Linq;

public class PBTrophyRoadSO : TrophyRoadSO
{
    public void SetArenaSections(List<ArenaSection> arenaSections)
    {
        this.arenaSections = arenaSections;
    }

    public Milestone GetNextMilestoneCurrent()
    {
        var nextMilestone = arenaSections
            .SelectMany(section => section.milestones)
            .FirstOrDefault(milestone => HighestAchievedMedals < milestone.requiredAmount);

        return nextMilestone;
    }

    public Milestone GetHighestAchievedMilestone()
    {
        var achievedMilestone = arenaSections
            .SelectMany(section => section.milestones)
            .Where(milestone => HighestAchievedMedals >= milestone.requiredAmount)
            .OrderByDescending(milestone => milestone.requiredAmount)
            .FirstOrDefault();
        return achievedMilestone;
    }

    public int GetHighestAchievedMilestoneNumber()
    {
        var allMilestones = arenaSections
            .SelectMany(section => section.milestones)
            .ToList();

        int index = allMilestones
            .FindLastIndex(milestone => HighestAchievedMedals >= milestone.requiredAmount);

        return index + 1;
    }


    public int GetCurrentMilestoneIndex()
    {
        var milestoneIndex = 0;
        foreach (var section in ArenaSections)
        {
            for (int i = 0; i < section.milestones.Count; i++)
            {
                if (section.milestones[i].Unlocked)
                    milestoneIndex++;
            }
        }
        return milestoneIndex;
    }
}