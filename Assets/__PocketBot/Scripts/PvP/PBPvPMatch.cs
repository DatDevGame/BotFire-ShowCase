using System.Collections;
using System.Collections.Generic;
using LatteGames.PvP;
using UnityEngine;
using System.Linq;

public class PBPvPMatch : PvPMatch
{
    public class PBEndgameData : EndgameData
    {
        public PBEndgameData(PvPMatch match, PlayerInfo winner, bool isAnyContestantAFK, int remainingTime) : base(match, winner, isAnyContestantAFK)
        {
            this.remainingTime = remainingTime;
        }

        public virtual int remainingTime { get; set; }
        public virtual bool isTimesUp => remainingTime <= 0;
    }

    public virtual int rankOfMine => GetRank(GetLocalPlayerInfo());
    public virtual Mode mode { get; set; }
    public override bool isAbleToComplete
    {
        get
        {
            if (mode != Mode.Battle)
                return base.isAbleToComplete;
            return true;
        }
    }

    public override bool isVictory
    {
        get
        {
            List<PBRobot> allBots = PBRobot.allRobots;
            List<PBRobot> teamARobots = allBots
                .Where(v => v.TeamId == 1)
                .ToList();

            List<PBRobot> teamBRobots = allBots
                .Where(v => v.TeamId != 1)
                .ToList();

            int totalKillA = teamARobots.Sum(r => r.PlayerKDA.Kills);
            int totalKillB = teamBRobots.Sum(r => r.PlayerKDA.Kills);

            return totalKillA >= totalKillB;
        }
    }
    public virtual PBEndgameData pbEndgameData => endgameData as PBEndgameData;
    public virtual List<PlayerInfoVariable> Contestants { get; private set; } = new();
    public virtual List<PlayerInfo> survivingContestantOrders { get; private set; } = new List<PlayerInfo>();

    public virtual int GetRank(PlayerInfo playerInfoVariable)
    {
        return survivingContestantOrders.IndexOf(playerInfoVariable) + 1 + (Contestants.Count - survivingContestantOrders.Count);
    }

    public virtual void AddSurvivor(PlayerInfo playerInfoVariable)
    {
        if (!survivingContestantOrders.Contains(playerInfoVariable))
            survivingContestantOrders.Insert(0, playerInfoVariable);

        if (survivingContestantOrders.Count == Contestants.Count - 1)
        {
            foreach (var contestant in Contestants)
            {
                if (!survivingContestantOrders.Contains(contestant))
                {
                    // Add the last player
                    survivingContestantOrders.Insert(0, contestant);
                    break;
                }
            }
        }
    }

    public override void EndRound(EndgameData endgameData)
    {
        var levelController = ObjectFindCache<PBLevelController>.Get();
        base.EndRound(new PBEndgameData(endgameData.match, endgameData.winner, endgameData.isAnyContestantAFK, levelController.RemainingTime));
    }

    public static PBPvPMatch CreateMatch(PvPArenaSO arenaSO, Mode mode, params PlayerInfoVariable[] contestants)
    {
        return new PBPvPMatch()
        {
            mode = mode,
            status = Status.Waiting,
            arenaSO = arenaSO,
            Contestants = new List<PlayerInfoVariable>(contestants)
        };
    }

    public override void PrepareMatch()
    {
        if (Contestants.Count <= 0)
        {
            Debug.LogError("Bruhhh???");
            return;
        }

        NotifyEventMatchPrepared();
    }

    public override PlayerInfo GetLocalPlayerInfo()
    {
        foreach (var player in Contestants)
        {
            if (player.value.personalInfo.isLocal) return player.value;
        }
        return null;
    }

    public override PlayerInfo GetOpponentInfo()
    {
        foreach (var player in Contestants)
        {
            if (player.value.personalInfo.isLocal == false) return player.value;
        }
        return null;
    }

    public virtual bool CheckVictoryStatus(PlayerInfo playerInfo)
    {
        if (mode != Mode.Battle)
            return endgameData != null && endgameData.winner == playerInfo;
        return GetRank(playerInfo) <= 2;
    }
}