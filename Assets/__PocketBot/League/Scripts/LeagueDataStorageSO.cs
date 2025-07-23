using System;
using System.Collections.Generic;
using System.Linq;
using LatteGames.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "LeagueDataStorageSO", menuName = "PocketBots/League/LeagueDataStorageSO")]
public class LeagueDataStorageSO : SavedDataSO<LeagueDataStorageSO.Data>
{
    [Serializable]
    public class Data : SavedData
    {
        [SerializeField]
        private bool m_IsUnlocked;
        [SerializeField]
        private int m_CurrentDivisionIndex;
        [SerializeField]
        private List<LeaguePlayer> m_BotPlayers = new List<LeaguePlayer>();

        public bool isUnlocked
        {
            get => m_IsUnlocked;
            set => m_IsUnlocked = value;
        }
        public int currentDivisionIndex
        {
            get => m_CurrentDivisionIndex;
            set => m_CurrentDivisionIndex = value;
        }
        public List<LeaguePlayer> botPlayers
        {
            get => m_BotPlayers;
            set => m_BotPlayers = value;
        }
    }

    [SerializeField]
    private LeagueDataSO m_LeagueDataSO;

    [SerializeField]
    private CurrentHighestArenaVariable m_HighestArenaVar;
    [SerializeField]
    private DayBasedRewardSO m_TimeLeftUntilDivisionEnds;

    public override Data defaultData
    {
        get
        {
            return new Data()
            {
                isUnlocked = false,
                currentDivisionIndex = 0,
                botPlayers = new List<LeaguePlayer>(),
            };
        }
    }

    // Editor only
    [ShowInInspector]
    private Data dataInspector => data;

    public List<LeaguePlayer> GenerateRandomPlayers(LeagueDivision division, RangeInt keepRange = default)
    {
        TimeSpan interval = DateTime.Now - m_TimeLeftUntilDivisionEnds.LastRewardTime;
        var remainingSeconds = m_TimeLeftUntilDivisionEnds.CoolDownInterval - interval.TotalSeconds;
        interval = TimeSpan.FromSeconds(remainingSeconds);
        float percentCrown = (int)(24 - interval.TotalHours) * m_LeagueDataSO.configLeague.botProgressPerHour;
        float totalCrowns;
        List<int> avatarIndices = new List<int>();
        for (int i = 0; i < m_LeagueDataSO.playerDatabaseSO.botInfos.Count; i++)
        {
            avatarIndices.Add(i);
        }
        List<LeaguePlayer> botPlayers = new List<LeaguePlayer>(division.playerPoolCount);
        Dictionary<LeagueDataSO.BotDifficulty, int> botDifficultyDistribution = division.botDifficultyDistribution;
        Dictionary<LeagueDataSO.BotDifficulty, int> botDifficultyCount = new();
        Array botDifficulties = Enum.GetValues(typeof(LeagueDataSO.BotDifficulty));
        foreach (var botDifficulty in botDifficulties)
        {
            botDifficultyCount.Set((LeagueDataSO.BotDifficulty)botDifficulty, 0);
        }
        if (keepRange.length > 0)
        {
            data.botPlayers.Sort((playerA, playerB) =>
            {
                int value = playerB.botTotalCrowns.CompareTo(playerA.botTotalCrowns);
                if (value == 0)
                {
                    return playerA.name.CompareTo(playerB.name);
                }
                return value;
            });
            //keeping old bots
            int difficultyCount = Enum.GetValues(typeof(LeagueDataSO.BotDifficulty)).Length;
            for (int i = keepRange.start; i < keepRange.start + keepRange.length; ++i)
            {
                LeaguePlayer bot = data.botPlayers[i];
                LeagueDataSO.BotDifficulty botDifficulty = bot.botDifficulty;
                botPlayers.Add(bot);
                while (botDifficultyCount[botDifficulty] >= botDifficultyDistribution[botDifficulty])
                {
                    botDifficulty = botDifficulty == 0 ? botDifficulty + difficultyCount - 1 : botDifficulty - 1;
                    if (botDifficulty == bot.botDifficulty) //safe guard. this should not happen
                        break;
                }
                totalCrowns = m_LeagueDataSO.leagueBotProfiles[botDifficulty].GetRandomTotalCrows(m_HighestArenaVar.value);
                bot.botDifficulty = botDifficulty;
                bot.botTotalCrowns = totalCrowns;
                bot.numOfCrowns = Mathf.Clamp(Mathf.Round(totalCrowns * percentCrown), 0, totalCrowns);
                botDifficultyCount[botDifficulty]++;
                avatarIndices.Remove(bot.playerIndex);
            }
        }
        //creating new bot
        for (int i = Mathf.Max(0, keepRange.length); i < division.playerPoolCount - 1; i++)
        {
            LeagueDataSO.BotDifficulty botDifficulty = (LeagueDataSO.BotDifficulty) botDifficulties.GetValue(botDifficulties.Length - 1);
            while (botDifficultyCount[botDifficulty] >= botDifficultyDistribution[botDifficulty])
            {
                --botDifficulty;
                if (botDifficulty < 0) //safe guard. this should not happen
                    break;
            }
            totalCrowns = m_LeagueDataSO.leagueBotProfiles[botDifficulty].GetRandomTotalCrows(m_HighestArenaVar.value);
            LeaguePlayer player = new LeaguePlayer()
            {
                playerIndex = avatarIndices.GetRandom(),
                botDifficulty = botDifficulty,
                botTotalCrowns = totalCrowns,
                numOfCrowns = Mathf.Clamp(Mathf.Round(totalCrowns * percentCrown), 0, totalCrowns),
            };
            botPlayers.Add(player);
            avatarIndices.Remove(player.playerIndex);
            botDifficultyCount[botDifficulty]++;
        }
        return botPlayers;
    }

    public void RefreshBotOnlineStatus(LeagueDivision division)
    {
        Dictionary<LeagueDataSO.BotDifficulty, List<LeaguePlayer>> filteredPlayersByDifficulty = new();
        List<LeaguePlayer> players;
        foreach (LeaguePlayer player in data.botPlayers)
        {
            if (!filteredPlayersByDifficulty.TryGetValue(player.botDifficulty, out players))
            {
                players = new();
                filteredPlayersByDifficulty[player.botDifficulty] = players;
            }
            players.Add(player);
            player.isOnline = false;
        }
        foreach (LeagueDataSO.BotDifficulty difficulty in filteredPlayersByDifficulty.Keys)
        {
            filteredPlayersByDifficulty[difficulty].Shuffle();
        }
        List<LeagueDataSO.BotDifficulty> sortedDifficulty = filteredPlayersByDifficulty.Keys.ToList();
        sortedDifficulty.Remove(LeagueDataSO.BotDifficulty.Offline);
        sortedDifficulty.Sort((a, b) => b - a);
        int onlineCount = UnityEngine.Random.Range(division.botOnlineLimit.min, division.botOnlineLimit.max + 1);
        int difficultyIdx = 0;
        while (onlineCount > 0)
        {
            players = filteredPlayersByDifficulty[sortedDifficulty[difficultyIdx]];
            difficultyIdx = (difficultyIdx + 1) % sortedDifficulty.Count;
            if (players.Count <= 0)
                continue;
            players[players.Count - 1].isOnline = true;
            players.RemoveAt(players.Count - 1);
            --onlineCount;
        }
    }

    public void UpdateBotCrowns(float updatePercent, Predicate<LeaguePlayer> predicate = null)
    {
        foreach (LeaguePlayer player in data.botPlayers)
        {
            if (predicate != null && !predicate(player))
                continue;
            player.numOfCrowns = Mathf.Clamp(player.numOfCrowns + Mathf.Ceil(updatePercent * player.botTotalCrowns), 0, player.botTotalCrowns);
        }
    }

    public override void Delete()
    {
        base.Delete();
        data = defaultData;
        m_LeagueDataSO.timeLeftUntilDivisionEnds.Clear();
        m_LeagueDataSO.timeLeftUntilLeagueEnds.Clear();
        m_LeagueDataSO.timeLeftUntilBotOnlineRefresh.Clear();
    }
}