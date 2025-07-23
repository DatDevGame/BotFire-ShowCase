using System;
using System.Collections.Generic;
using DG.DemiLib;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using I2.Loc;
using LatteGames.GameManagement;
using LatteGames.PvP;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeagueDivision
{
    public class RankBasedRewardLevel
    {
        [SerializeField]
        private RangeIntValue m_RankRange = new RangeIntValue();
        [SerializeField]
        private RewardGroupInfo m_RewardInfo = new RewardGroupInfo();

        public RangeIntValue rankRange => m_RankRange;
        public RewardGroupInfo rewardInfo => m_RewardInfo;
    }

    [SerializeField]
    private LocalizedString m_LocalizedDisplayName;
    [SerializeField]
    private Sprite m_Icon;
    [SerializeField]
    private int m_PlayerPoolCount;
    [SerializeField]
    private int m_PlayerGetPromotedCount;
    [SerializeField]
    private RankBasedRewardLevel[] m_RankBasedRewardLevels = new RankBasedRewardLevel[] { };
    [SerializeField]
    private Dictionary<LeagueDataSO.BotDifficulty, int> m_BotDifficultyDistribution = new();
    [SerializeField]
    private IntRange m_BotOnlineLimit = new();

    public string displayName => m_LocalizedDisplayName.ToString();
    public Sprite icon => m_Icon;
    public int playerPoolCount => m_PlayerPoolCount;
    public int playerGetPromotedCount => m_PlayerGetPromotedCount;
    public RewardGroupInfo bestRewardInfo => GetRewardInfoByRank(Const.IntValue.One);
    public Dictionary<LeagueDataSO.BotDifficulty, int> botDifficultyDistribution => m_BotDifficultyDistribution;
    public IntRange botOnlineLimit => m_BotOnlineLimit;

    public bool IsGetPromotedByRank(int rank)
    {
        return rank <= playerGetPromotedCount;
    }

    public RewardGroupInfo GetRewardInfoByRank(int rank)
    {
        for (int i = 0; i < m_RankBasedRewardLevels.Length; i++)
        {
            if (m_RankBasedRewardLevels[i].rankRange.IsOutOfRange(rank))
                continue;
            return m_RankBasedRewardLevels[i].rewardInfo;
        }
        return null;
    }
}
[Serializable]
public class LeaguePlayer
{
    [ShowInInspector]
    public string name => isLocalPlayer ? LeagueManager.leagueDataSO?.playerDatabaseSO.localPlayerPersonalInfo.name : LeagueManager.leagueDataSO?.playerDatabaseSO.botInfos[playerIndex].name;
    [ShowInInspector]
    public Sprite avatarThumbnail => isLocalPlayer ? LeagueManager.leagueDataSO?.playerDatabaseSO.localPlayerPersonalInfo.avatar : LeagueManager.leagueDataSO?.playerDatabaseSO.botInfos[playerIndex].avatar;
    [ShowInInspector]
    public Sprite nationalFlag => isLocalPlayer ? LeagueManager.leagueDataSO?.playerDatabaseSO.localPlayerPersonalInfo.nationalFlag : LeagueManager.leagueDataSO?.playerDatabaseSO.botInfos[playerIndex].nationalFlag;

    [field: SerializeField]
    public int playerIndex { get; set; }
    [field: SerializeField]
    public float numOfCrowns { get; set; }
    [field: SerializeField]
    public LeagueDataSO.BotDifficulty botDifficulty { get; set; }
    [field: SerializeField]
    public float botTotalCrowns { get; set; }
    [field: SerializeField]
    public bool isOnline { get; set; } = false;
    public bool isLocalPlayer => playerIndex == -1;
}

[Serializable]
public class LeagueBotProfile
{
    [Serializable]
    public class ModeRate
    {
        [field: SerializeField, Range(0, 1)]
        public float playRate { get; set; }
        [field: SerializeField, Range(0, 1)]
        public float minWinRate { get; set; }
        [field: SerializeField, Range(0, 1)]
        public float maxWinRate { get; set; }
    }

    [field: SerializeField]
    public int minDailyPlayMatch { get; set; }
    [field: SerializeField]
    public int maxDailyPlayMatch { get; set; }
    [field: SerializeField]
    public Dictionary<Mode, ModeRate> playModeRates { get; set; } = new();

    public float GetRandomTotalCrows(PvPArenaSO arenaSO)
    {
        int dailyPlayMatches = UnityEngine.Random.Range(minDailyPlayMatch, maxDailyPlayMatch + 1);
        int duelModeMatches = Mathf.RoundToInt(dailyPlayMatches * playModeRates[Mode.Normal].playRate);
        int battleModeMatches = dailyPlayMatches - duelModeMatches;
        int duelWinMatches = Mathf.RoundToInt(duelModeMatches * UnityEngine.Random.Range(playModeRates[Mode.Normal].minWinRate, playModeRates[Mode.Normal].maxWinRate));
        int battleTop1Matches = Mathf.RoundToInt(battleModeMatches * UnityEngine.Random.Range(playModeRates[Mode.Battle].minWinRate, playModeRates[Mode.Battle].maxWinRate));
        return arenaSO.TryGetReward(out CurrencyRewardModule medalReward, item => item.CurrencyType == CurrencyType.Medal) ?
            duelWinMatches * medalReward.Amount * PBPvPGameOverUI.GetRewardMultiplier(Mode.Normal, CurrencyType.Medal, 1) +
            battleTop1Matches * medalReward.Amount * PBPvPGameOverUI.GetRewardMultiplier(Mode.Battle, CurrencyType.Medal, 1) : 0f;
    }
}

[Serializable]
public class LeaguePlayedTimeToReachRank
{
    public PPrefDatetimeVariable PlayedTimeToReachRank;
    public PPrefBoolVariable StartPlayTime;
}

[CreateAssetMenu(fileName = "LeagueDataSO", menuName = "PocketBots/League/LeagueDataSO")]
public class LeagueDataSO : SerializedScriptableObject
{
    public enum Status
    {
        Passed,
        Present,
        Upcoming
    }

    public enum BotDifficulty
    {
        Offline,
        Easy,
        Medium,
        Hard,
        Veryhard,
        Insane,
        Brutal,
        Impossible
    }

    [Serializable]
    public class Config
    {
        [SerializeField]
        private bool m_HasBonusCard = true;
        [SerializeField]
        private int m_UnlockTrophyThreshold;
        [SerializeField]
        private LeagueEndUI m_LeagueEndUIPrefab;
        [SerializeField]
        private LeagueStartUI m_LeagueStartUIPrefab;
        [SerializeField]
        private LeagueInfoUI m_LeagueInfoUIPrefab;
        [SerializeField]
        private LeaguePromotionUI m_LeaguePromotionUIPrefab;
        [SerializeField, Range(0, 1)]
        private float m_BotProgressPerHour;
        [SerializeField]
        private int m_OnlineBotUpdateTimeMin;
        [SerializeField]
        private int m_OnlineBotUpdateTimeMax;
        [SerializeField, Range(0, 1)]
        private float m_OnlineBotUpdateProgressMin;
        [SerializeField, Range(0, 1)]
        private float m_OnlineBotUpdateProgressMax;

        public bool hasBonusCard => m_HasBonusCard;
        public int unlockTrophyThreshold => m_UnlockTrophyThreshold;
        public LeagueEndUI leagueEndUIPrefab => m_LeagueEndUIPrefab;
        public LeagueStartUI leagueStartUIPrefab => m_LeagueStartUIPrefab;
        public LeagueInfoUI leagueInfoUIPrefab => m_LeagueInfoUIPrefab;
        public LeaguePromotionUI leaguePromotionUIPrefab => m_LeaguePromotionUIPrefab;
        public float botProgressPerHour => m_BotProgressPerHour;
        public float onlineBotUpdateTimeMin => m_OnlineBotUpdateTimeMin;
        public float olineBotUpdateTimeMax => m_OnlineBotUpdateTimeMax;
        public float onlineBotUpdateProgressMin => m_OnlineBotUpdateProgressMin;
        public float onlineBotUpdateProgressMax => m_OnlineBotUpdateProgressMax;
    }

    public event Action onPlayerDataChanged = delegate { };
    public event Action onUnlocked = delegate { };

    [SerializeField]
    private Config m_ConfigLeague;
    [SerializeField]
    private LeagueLeaderboardUI.Config m_ConfigLeaderboardUI;
    [SerializeField]
    private LeagueDataStorageSO m_DataStorageSO;
    [SerializeField]
    private PPrefFloatVariable m_HighestNumOfTrophiesVar;
    [SerializeField]
    private PlayerDatabaseSO m_PlayerDatabaseSO;
    [SerializeField]
    private NationalFlagManagerSO m_NationalFlagManagerSO;
    [SerializeField]
    private DayBasedRewardSO m_TimeLeftUntilDivisionEnds;
    [SerializeField]
    private PPrefDatetimeVariable m_TimeLeftUntilLeagueEnds;
    [SerializeField]
    private TimeBasedRewardSO m_TimeLeftUntilOnlineBotCrownUpdate;
    [SerializeField]
    private TimeBasedRewardSO m_TimeLeftUntilBotOnlineRefresh;
    [SerializeField]
    private TimeBasedRewardSO m_TimeLeftUntilAllBotCrownUpdate;
    [SerializeField]
    private PBGachaPackManagerSO m_GachaPackManagerSO;
    [SerializeField]
    private LeagueDivision[] m_Divisions;
    [SerializeField]
    private Dictionary<BotDifficulty, LeagueBotProfile> m_LeagueBotProfiles;

    [NonSerialized]
    private LeaguePlayer m_LocalPlayer;
    [NonSerialized]
    private List<LeaguePlayer> m_SortedPlayers;
    [NonSerialized]
    private List<string> m_CrownAndRankChangedDataKeys = new List<string>();
    [NonSerialized]
    private Dictionary<string, ValueDataChanged<int>> m_RankChangedDataDictionary = new Dictionary<string, ValueDataChanged<int>>();
    [NonSerialized]
    private Dictionary<string, ValueDataChanged<float>> m_CrownChangedDataDictionary = new Dictionary<string, ValueDataChanged<float>>();

    [SerializeField, BoxGroup("Log Event")] private Dictionary<int, LeaguePlayedTimeToReachRank> m_LeaguePlayedTimeToReachRank;
    [SerializeField, BoxGroup("Log Event")] private PPrefDatetimeVariable m_LeagueStartDatetime;

    public bool sortFlag { get; set; }
    public Config configLeague => m_ConfigLeague;
    public LeagueLeaderboardUI.Config configLeaderboardUI => m_ConfigLeaderboardUI;
    public LeagueDataStorageSO dataStorageSO => m_DataStorageSO;
    public PlayerDatabaseSO playerDatabaseSO => m_PlayerDatabaseSO;
    public NationalFlagManagerSO nationalFlagManagerSO => m_NationalFlagManagerSO;
    public DayBasedRewardSO timeLeftUntilDivisionEnds => m_TimeLeftUntilDivisionEnds;
    public PPrefDatetimeVariable timeLeftUntilLeagueEnds => m_TimeLeftUntilLeagueEnds;
    public TimeBasedRewardSO timeLeftUntilOnlineBotCrownUpdate => m_TimeLeftUntilOnlineBotCrownUpdate;
    public TimeBasedRewardSO timeLeftUntilBotOnlineRefresh => m_TimeLeftUntilBotOnlineRefresh;
    public TimeBasedRewardSO timeLeftUntilAllBotCrownUpdate => m_TimeLeftUntilAllBotCrownUpdate;
    public PBGachaPackManagerSO gachaPackManagerSO => m_GachaPackManagerSO;
    public LeaguePlayer localPlayer
    {
        get
        {
            if (m_LocalPlayer == null)
            {
                m_LocalPlayer = new LeaguePlayer()
                {
                    playerIndex = -1,
                    numOfCrowns = CurrencyManager.Instance.GetCurrencySO(CurrencyType.Crown),
                    isOnline = true,
                };
            }
            return m_LocalPlayer;
        }
    }
    public LeagueDivision[] divisions => m_Divisions;
    public Dictionary<BotDifficulty, LeagueBotProfile> leagueBotProfiles => m_LeagueBotProfiles;
    public Dictionary<int, LeaguePlayedTimeToReachRank> LeaguePlayedTimeToReachRank => m_LeaguePlayedTimeToReachRank;
    public PPrefDatetimeVariable LeagueStartDatetime => m_LeagueStartDatetime;

    public int GetPlayerRank(LeaguePlayer player)
    {
        List<LeaguePlayer> sortedPlayers = GetSortedPlayersByCrown();
        return BinarySearch() + 1;

        int BinarySearch()
        {
            int left = 0;
            int right = sortedPlayers.Count - 1;
            while (left <= right)
            {
                int middle = (left + right) / 2;
                if (sortedPlayers[middle].numOfCrowns < player.numOfCrowns)
                {
                    right = middle - 1;
                }
                else if (sortedPlayers[middle].numOfCrowns > player.numOfCrowns)
                {
                    left = middle + 1;
                }
                else
                {
                    if (sortedPlayers[middle] == player)
                        return middle;
                    // Search nearby to the right
                    for (int i = middle + 1; i < sortedPlayers.Count && sortedPlayers[i].numOfCrowns == player.numOfCrowns; i++)
                    {
                        if (sortedPlayers[i] == player)
                            return i;
                    }
                    // Search nearby to the left
                    for (int i = middle - 1; i >= 0 && sortedPlayers[i].numOfCrowns == player.numOfCrowns; i--)
                    {
                        if (sortedPlayers[i] == player)
                            return i;
                    }
                    return -1;
                }
            }
            return -1;
        }
    }

    [Button]
    public int GetLocalPlayerRank(bool updateLocalPlayerData = false)
    {
        if (updateLocalPlayerData)
            UpdateLocalPlayerData();
        return GetPlayerRank(localPlayer);
    }

    [Button]
    public List<LeaguePlayer> GetSortedPlayersByCrown()
    {
        if (m_SortedPlayers == null || m_SortedPlayers.Count <= 0)
        {
            sortFlag = true;
            m_SortedPlayers = new List<LeaguePlayer>(dataStorageSO.data.botPlayers)
            {
                localPlayer
            };
        }
        if (sortFlag)
        {
            sortFlag = false;
            m_SortedPlayers.Sort((playerA, playerB) =>
            {
                int value = playerB.numOfCrowns.CompareTo(playerA.numOfCrowns);
                if (value == 0)
                {
                    return playerA.name.CompareTo(playerB.name);
                }
                return value;
            });
        }
        return m_SortedPlayers;
    }

    public void UpdateLocalPlayerData(bool isNotify = true)
    {
        var localPlayer = this.localPlayer;
        localPlayer.numOfCrowns = CurrencyManager.Instance.GetCurrencySO(CurrencyType.Crown);
        localPlayer.isOnline = true;
        UpdateSortFlag(true, isNotify);
    }

    public bool IsAbleToUnlock()
    {
        return m_HighestNumOfTrophiesVar.value >= configLeague.unlockTrophyThreshold;
    }

    public bool IsUnlocked()
    {
        return m_DataStorageSO.data.isUnlocked;
    }

    public void Unlock()
    {
        if (IsUnlocked())
            return;
        m_DataStorageSO.data.isUnlocked = true;
        onUnlocked.Invoke();
    }

    public bool IsLeagueOver()
    {
        return GetCurrentDivisionIndex() >= divisions.Length;
    }

    public bool IsAbleToStartNewLeague()
    {
        return GetCurrentTime() >= timeLeftUntilLeagueEnds.value;
    }

    public bool IsDivisionOver()
    {
        return timeLeftUntilDivisionEnds.canGetReward;
    }

    public bool IsReachFinalDivision()
    {
        return GetCurrentDivision() == GetFinalDivision();
    }

    public bool IsLocalPlayerGetPromoted()
    {
        return GetCurrentDivision().IsGetPromotedByRank(GetLocalPlayerRank());
    }

    public LeagueDivision GetFinalDivision()
    {
        return divisions[divisions.Length - 1];
    }

    public LeagueDivision GetCurrentDivision(bool isClamped = false)
    {
        int currentDivisionIndex = GetCurrentDivisionIndex();
        if (!divisions.IsValidIndex(currentDivisionIndex) && !isClamped)
            return null;
        return divisions[Mathf.Clamp(currentDivisionIndex, 0, divisions.Length - 1)];
    }

    public int GetCurrentDivisionIndex()
    {
        return dataStorageSO.data.currentDivisionIndex;
    }

    [Button]
    public void PromoteToNextDivision()
    {
        if (GetCurrentDivisionIndex() >= divisions.Length - 1)
        {
            ResetCrownAndRankChangedData();
            dataStorageSO.data.currentDivisionIndex = divisions.Length;
            return;
        }
        m_SortedPlayers = null;
        int promotedCount = GetCurrentDivision().playerGetPromotedCount;
        dataStorageSO.data.currentDivisionIndex++;
        timeLeftUntilDivisionEnds.GetReward();
        LeagueDivision division = GetCurrentDivision();
        dataStorageSO.data.botPlayers = dataStorageSO.GenerateRandomPlayers(division, new RangeInt(0, promotedCount - 1));
        RefreshBotOnlineStatus(true);
        ResetCrownAndRankChangedData();
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Crown).value = 0f;
    }

    [Button]
    public void RemainInCurrentDivision(bool resetForCheater = false)
    {
        m_SortedPlayers = null;
        timeLeftUntilDivisionEnds.GetReward();
        if (resetForCheater)
        {
            timeLeftUntilAllBotCrownUpdate.GetReward();
            timeLeftUntilBotOnlineRefresh.GetReward();
            timeLeftUntilOnlineBotCrownUpdate.GetReward();
            timeLeftUntilLeagueEnds.value = GetToday().AddDays(1).GetDayOfNextWeek(DayOfWeek.Monday);
        }
        LeagueDivision division = GetCurrentDivision();
        dataStorageSO.data.botPlayers = dataStorageSO.GenerateRandomPlayers(division, new RangeInt(division.playerGetPromotedCount, division.playerPoolCount - division.playerGetPromotedCount - 1));
        RefreshBotOnlineStatus(true);
        ResetCrownAndRankChangedData();
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Crown).value = 0f;
    }

    [Button]
    public void ResetLeague()
    {
        m_SortedPlayers = null;
        dataStorageSO.data.currentDivisionIndex = 0;
        timeLeftUntilDivisionEnds.GetReward();
        timeLeftUntilAllBotCrownUpdate.GetReward();
        timeLeftUntilLeagueEnds.value = GetToday().AddDays(1).GetDayOfNextWeek(DayOfWeek.Monday);
        dataStorageSO.data.botPlayers = dataStorageSO.GenerateRandomPlayers(GetCurrentDivision());
        RefreshBotOnlineStatus(true);
        ResetCrownAndRankChangedData();
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Crown).value = 0f;
    }

    public void UpdateSortFlag(bool sortFlag, bool isNotify = true)
    {
        this.sortFlag = sortFlag;
        if (isNotify)
            NotifyPlayerDataChanged();
    }

    public void NotifyPlayerDataChanged()
    {
        onPlayerDataChanged.Invoke();
    }

    public void RefreshBotOnlineStatus(bool isForced = false, bool isNotify = true)
    {
        if (!isForced && !m_TimeLeftUntilBotOnlineRefresh.canGetReward)
            return;
        m_TimeLeftUntilBotOnlineRefresh.GetReward();
        m_DataStorageSO.RefreshBotOnlineStatus(GetCurrentDivision());
        UpdateSortFlag(true, isNotify);
    }

    public void UpdateOnlineBotCrowns(bool isNotify = true)
    {
        if (!m_TimeLeftUntilOnlineBotCrownUpdate.canGetReward)
            return;
        m_TimeLeftUntilOnlineBotCrownUpdate.lastRewardTime =
        (DateTime.Now + TimeSpan.FromSeconds(UnityEngine.Random.Range(configLeague.onlineBotUpdateTimeMin, configLeague.olineBotUpdateTimeMax))).Ticks.ToString();
        m_DataStorageSO.UpdateBotCrowns(
            UnityEngine.Random.Range(configLeague.onlineBotUpdateProgressMin, configLeague.onlineBotUpdateProgressMax),
            player => player.isOnline
        );
        UpdateSortFlag(true, isNotify);
    }

    public void UpdateAllBotCrowns(bool isNotify = true)
    {
        if (!m_TimeLeftUntilAllBotCrownUpdate.canGetReward)
            return;
        m_DataStorageSO.UpdateBotCrowns(m_TimeLeftUntilAllBotCrownUpdate.GetRewardByAmount() * configLeague.botProgressPerHour);
        UpdateSortFlag(true, isNotify);
    }

    public void UpdateFinalBotCrows(bool isNotify = true)
    {
        m_DataStorageSO.UpdateBotCrowns(1);
        UpdateSortFlag(true, isNotify);
    }

    public void ResetCrownAndRankChangedData()
    {
        foreach (var key in m_CrownAndRankChangedDataKeys)
        {
            SetRankChangedData(key, default);
            SetCrownChangedData(key, default);
        }
    }

    public void UpdateCrownAndRankChangedData(ValueDataChanged<float> crownChangedData)
    {
        int oldRank = GetLocalPlayerRank();
        int newRank = GetLocalPlayerRank(true);
        foreach (var key in m_CrownAndRankChangedDataKeys)
        {
            ValueDataChanged<int> rankChangedData = GetRankChangedData(key);
            rankChangedData.oldValue = rankChangedData.oldValue == 0 ? oldRank : rankChangedData.oldValue;
            rankChangedData.newValue = newRank;
            SetRankChangedData(key, rankChangedData);
            SetCrownChangedData(key, crownChangedData);
        }
    }

    public ValueDataChanged<int> GetRankChangedData(string key)
    {
        return m_RankChangedDataDictionary.Get(key);
    }

    public void SetRankChangedData(string key, ValueDataChanged<int> rankChangedData)
    {
        m_RankChangedDataDictionary.Set(key, rankChangedData);
    }

    public ValueDataChanged<float> GetCrownChangedData(string key)
    {
        return m_CrownChangedDataDictionary.Get(key);
    }

    public void SetCrownChangedData(string key, ValueDataChanged<float> crownChangedData)
    {
        m_CrownChangedDataDictionary.Set(key, crownChangedData);
    }

    public void AddCrownAndRankChangedDataKey(string key)
    {
        if (!m_CrownAndRankChangedDataKeys.Contains(key))
            m_CrownAndRankChangedDataKeys.Add(key);
    }

    public static bool IsOnMainScreen()
    {
        Camera mainCam = MainCameraFindCache.Get();
        if (mainCam == null)
            return false;
        if (SceneManager.GetActiveScene().name != SceneName.MainScene.ToString())
            return false;
        if (!LoadingScreenUI.IS_LOADING_COMPLETE)
            return false;
        if (!MainScreenUI.IsShowing)
            return false;
        // FIXME: Dirty fix overlap with ultimate pack popup
        if (PBUltimatePackPopup.Instance != null && PBUltimatePackPopup.Instance.IsShowing)
            return false;
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = new Vector2(mainCam.pixelWidth / 2f, mainCam.pixelHeight / 2f);
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        List<BaseRaycaster> modules = RaycasterManager.GetRaycasters();
        int modulesCount = modules.Count;
        for (int i = 0; i < modulesCount; ++i)
        {
            var module = modules[i];
            if (module == null || !module.IsActive())
                continue;

            module.Raycast(pointerEventData, raycastResults);
            if (raycastResults.Count > 0)
                return false;
        }
        return true;
    }

    public static DateTime GetCurrentTime()
    {
        return DateTime.Now;
    }

    public static DateTime GetToday()
    {
        return DateTime.Today;
    }

    public static Status GetStatus(int value, int currentValue)
    {
        if (value < currentValue)
            return Status.Passed;
        if (value == currentValue)
            return Status.Present;
        return Status.Upcoming;
    }
}