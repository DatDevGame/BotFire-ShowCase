using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompoundRobotStats : IPartStats
{
    #region Constructor
    public CompoundRobotStats(Dictionary<PBPartSlot, IPartStats> partStatDictionary)
    {
        if (partStatDictionary == null)
        {
            Debug.LogError("Bruh ???");
            return;
        }
        this.partStatArr = GetPartStatsValues(partStatDictionary);
    }
    #endregion

    protected virtual IPartStats[] partStatArr { get; set; } = new IPartStats[0];

    protected virtual IEnumerable<IPartStats> iterator => partStatArr;

    public IStat<PBStatID, float> GetAttack()
    {
        var totalValue = Const.FloatValue.ZeroF;
        foreach (var robotStats in iterator)
        {
            if (Equals(robotStats, null))
                continue;
            totalValue += robotStats.GetAttack().value;
        }
        return new PBStat<float>(PBStatID.Attack, totalValue);
    }

    public IStat<PBStatID, float> GetHealth()
    {
        var totalValue = Const.FloatValue.ZeroF;
        foreach (var robotStats in iterator)
        {
            if (Equals(robotStats, null))
                continue;
            totalValue += robotStats.GetHealth().value;
        }
        return new PBStat<float>(PBStatID.Health, totalValue);
    }

    public IStat<PBStatID, float> GetPower()
    {
        var totalValue = Const.FloatValue.ZeroF;
        foreach (var robotStats in iterator)
        {
            if (Equals(robotStats, null))
                continue;
            totalValue += robotStats.GetPower().value;
        }
        return new PBStat<float>(PBStatID.Power, totalValue);
    }

    public IStat<PBStatID, float> GetResistance()
    {
        var totalValue = Const.FloatValue.ZeroF;
        foreach (var robotStats in iterator)
        {
            if (Equals(robotStats, null))
                continue;
            totalValue += robotStats.GetResistance().value;
        }
        return new PBStat<float>(PBStatID.Resistance, totalValue);
    }

    public IStat<PBStatID, float> GetStatsScore()
    {
        var totalValue = RobotStatsCalculator.CalStatsScore(GetHealth().value, GetAttack().value);
        return new PBStat<float>(PBStatID.StatsScore, totalValue);
    }

    public IStat<PBStatID, float> GetTurning()
    {
        var totalValue = Const.FloatValue.ZeroF;
        foreach (var robotStats in iterator)
        {
            if (Equals(robotStats, null))
                continue;
            totalValue += robotStats.GetTurning().value;
        }
        return new PBStat<float>(PBStatID.Turning, totalValue);
    }

    IPartStats[] GetPartStatsValues(Dictionary<PBPartSlot, IPartStats> statsOfRobot)
    {
        List<IPartStats> partStatValues = new();
        foreach (var stat in statsOfRobot)
        {
            partStatValues.Add(stat.Value);
        }
        return partStatValues.ToArray();
    }
}
