using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using PackReward;
using GachaSystem.Core;
using HyrphusQ.SerializedDataStructure;
using System.Linq;
using Unity.VisualScripting;

public enum ScriptWinStreak
{
    Coin,
    Gem,
    BoxClassic,
    BoxGreat,
    BoxUltra,
    BoxFortune,
    BoxValor,
    BoxPrime,
    SkillCard
}

[CreateAssetMenu(fileName = "WinStreakManagerSO", menuName = "PocketBots/PackReward/WinStreakManagerSO")]
public class WinStreakManagerSO : SerializedScriptableObject
{
    [ReadOnly]
    public string m_Key;

    [ReadOnly]
    public int m_IdScript => PlayerPrefs.GetInt(GetScriptIDKey(), 1);
    public bool ConditionDisplayedWinStreak
    {
        get
        {
            if (m_HighestTrophy != null)
            {
                return m_HighestTrophy.value >= m_TrophyDisplayed;
            }
            return false;
        }
    }
    public PPrefBoolVariable PprefTheFirstTime => m_PprefTheFirstTime;
    public List<WinStreakCellSO> WinStreakCellSOs => m_WinStreakCellSOs;
    public List<WinStreakCellSO> WinStreakCellSOPremiums => m_WinStreakCellSOPremiums;

    [SerializeField, BoxGroup("Config")] protected int m_TimeResetReward;
    [SerializeField, BoxGroup("Config")] protected int m_TrophyDisplayed;
    [SerializeField, BoxGroup("Config")] protected Dictionary<int, ScriptStreak> m_ScriptStreaks;
    [SerializeField, BoxGroup("Config")] protected Dictionary<ScriptWinStreak, Sprite> m_Avatars;
    [SerializeField, BoxGroup("Config")] protected List<WinStreakStandConfig> m_WinStreakStandArena;
    [SerializeField, BoxGroup("Config")] protected List<WinStreakPremiumConfig> m_WinStreakPremiumArena;
    [SerializeField, BoxGroup("Config")] protected List<SkillCardConfig> m_SkillCardArena;

    [SerializeField, BoxGroup("Data")] protected PBGachaPackManagerSO m_PBGachaPackManagerSO;
    [SerializeField, BoxGroup("Data")] private HighestAchivedWinStreak m_HighestAchivedWinStreakPPref;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_WinStreakPPref;
    [SerializeField, BoxGroup("Data")] protected CurrentHighestArenaVariable m_CurrentHighestArenaVariable;
    [SerializeField, BoxGroup("Data")] private PBTrophyRoadSO m_PBTrophyRoadSO;
    [SerializeField, BoxGroup("Data")] protected HighestAchievedPPrefFloatTracker m_HighestTrophy;
    [SerializeField, BoxGroup("Data")] protected PPrefBoolVariable m_PprefTheFirstTime;
    [SerializeField, BoxGroup("Data")] protected GachaCard_RandomActiveSkill m_GachaCardRandomActiveSkill;
    [SerializeField, BoxGroup("Data")] protected IntVariable m_RequiredTrophiesToUnlockActiveSkill;
    [SerializeField, BoxGroup("Data")] protected List<WinStreakCellSO> m_WinStreakCellSOs;
    [SerializeField, BoxGroup("Data")] protected List<WinStreakCellSO> m_WinStreakCellSOPremiums;

    protected string GetReducedKey(WinStreakCellSO WinStreakCellSO) => $"ReducedValue-{WinStreakCellSO.m_Key}";
    protected string GetRewardKey(WinStreakCellSO WinStreakCellSO) => $"RewardValue-{WinStreakCellSO.m_Key}";
    protected int GetReducedValue(string key) => PlayerPrefs.GetInt($"ReducedValue-{key}", 1);
    protected int GetRewardValue(string key) => PlayerPrefs.GetInt($"RewardValue-{key}", 1);
    protected string GetScriptIDKey() => $"ScriptID-{m_Key}";
    protected string GetTheFirstTime() => $"TheFirstTime-{m_Key}";
    protected string GetTheFirstTimeActiveSkill() => $"TheFirstTimeActiveSkill-{m_Key}";

    [Button]
    public virtual List<WinStreakCellSO> GetQueue(bool isPremium = false)
    {
        List<WinStreakCellSO> rewardCells = new List<WinStreakCellSO>();
        CreateRewardCells(rewardCells, isPremium);
        return rewardCells;
    }

    [Button]
    public void ResetData()
    {
        m_WinStreakCellSOs.ForEach((v) =>
        {
            PlayerPrefs.DeleteKey(GetRewardKey(v));
            v.ResetClaim();
        });

        m_WinStreakCellSOPremiums.ForEach((v) =>
        {
            PlayerPrefs.DeleteKey(GetRewardKey(v));
            v.ResetClaim();
        });

        ResetReward();
        m_HighestAchivedWinStreakPPref.value = 0;
        m_WinStreakPPref.value = 0;
        AddQueue();
    }

    private void AddQueue()
    {
        List<int> validScriptIds = new List<int>();
        bool isActiveSkillUnlocked = m_HighestTrophy.value >= m_RequiredTrophiesToUnlockActiveSkill.value;

        foreach (var scriptPair in m_ScriptStreaks)
        {
            var streak = scriptPair.Value;
            bool hasSkillCardInStand = streak.ScriptStreakStandSet.Contains(ScriptWinStreak.SkillCard);
            bool hasSkillCardInPremium = streak.ScriptStreakPremiumSet.Contains(ScriptWinStreak.SkillCard);

            bool shouldAdd =
                isActiveSkillUnlocked ||
                (!hasSkillCardInStand && !hasSkillCardInPremium);

            if (shouldAdd)
                validScriptIds.Add(scriptPair.Key);
        }

        if (!PlayerPrefs.HasKey(GetTheFirstTimeActiveSkill()) && isActiveSkillUnlocked)
        {
            validScriptIds = m_ScriptStreaks
                .Where(pair =>
                    pair.Value.ScriptStreakStandSet.Contains(ScriptWinStreak.SkillCard) &&
                    pair.Value.ScriptStreakPremiumSet.Contains(ScriptWinStreak.SkillCard))
                .Select(pair => pair.Key)
                .ToList();

            PlayerPrefs.SetInt(GetTheFirstTimeActiveSkill(), 1);
        }

        if (validScriptIds.Count > 0)
        {
            int selectedId = validScriptIds.GetRandom();
            PlayerPrefs.SetInt(GetScriptIDKey(), selectedId);
        }
    }

    #region Handle GetQueue
    protected void CreateRewardCells(List<WinStreakCellSO> rewardCells, bool isPremium = false)
    {
        List<ScriptWinStreak> StandscriptWinStreaks = m_ScriptStreaks[m_IdScript].ScriptStreakStandSet;
        List<ScriptWinStreak> PremiumscriptWinStreaks = m_ScriptStreaks[m_IdScript].ScriptStreakPremiumSet;
        for (int i = 0; i < StandscriptWinStreaks.Count; i++)
        {
            WinStreakCellSO cellSO = Instantiate(isPremium ? m_WinStreakCellSOPremiums[i] : m_WinStreakCellSOs[i]);
            ApplyReducedAndRewards(cellSO, isPremium ? PremiumscriptWinStreaks[i] : StandscriptWinStreaks[i], i + 1);
            cellSO.UpdateRewardGroupInfo();
            rewardCells.Add(cellSO);
        }
    }

    protected void ApplyReducedAndRewards(WinStreakCellSO cellSO, ScriptWinStreak scriptWinStreak, int index)
    {
        string key = cellSO.m_Key;
        cellSO.SetScripWinStreak(scriptWinStreak);

        if(m_Avatars.ContainsKey(scriptWinStreak))
            cellSO.icon = m_Avatars[scriptWinStreak];

        int milestoneIndex = m_PBTrophyRoadSO.GetCurrentMilestoneIndex();

        if (scriptWinStreak == ScriptWinStreak.Coin || scriptWinStreak == ScriptWinStreak.Gem)
        {
            if (!PlayerPrefs.HasKey(GetRewardKey(cellSO)))
            {
                if (scriptWinStreak == ScriptWinStreak.Coin)
                {
                    WinStreakStandArena winStreakStandArena = cellSO.IsPremium
                        ? m_WinStreakStandArena[m_CurrentHighestArenaVariable.value.index].PremiumSetConfig
                        : m_WinStreakStandArena[m_CurrentHighestArenaVariable.value.index].StandSetConfig;

                    int randomCoefficientStand = UnityEngine.Random.Range(winStreakStandArena.MinValue, winStreakStandArena.MaxValue + 1);
                    int wonCoinAreana = (m_CurrentHighestArenaVariable.value as PBPvPArenaSO).WonNumOfCoins;
                    float coinCalc = randomCoefficientStand * wonCoinAreana * Mathf.Pow(1.4f, index);

                    PlayerPrefs.SetInt(GetRewardKey(cellSO), Mathf.RoundToInt(coinCalc));
                }
                else
                {
                    WinStreakPremiumArena winStreakPremiumArena = cellSO.IsPremium
                        ? m_WinStreakPremiumArena[m_CurrentHighestArenaVariable.value.index].PremiumSetConfig
                        : m_WinStreakPremiumArena[m_CurrentHighestArenaVariable.value.index].StandSetConfig;

                    int randomCoefficientPremium = UnityEngine.Random.Range(winStreakPremiumArena.MinValue, winStreakPremiumArena.MaxValue + 1);
                    float gemMultiplierOfTheArena = winStreakPremiumArena.Multiplier;
                    float gemCalc = randomCoefficientPremium * gemMultiplierOfTheArena * Mathf.Pow(1.2f, index);

                    PlayerPrefs.SetInt(GetRewardKey(cellSO), Mathf.RoundToInt(gemCalc));
                }
            }
            cellSO.PackType = PackType.Currency;
            cellSO.CurrencyPack.CurrencyType = scriptWinStreak == ScriptWinStreak.Coin ? CurrencyType.Standard : CurrencyType.Premium;
            cellSO.CurrencyPack.Value = GetRewardValue(key);
        }
        else if (scriptWinStreak == ScriptWinStreak.BoxClassic || scriptWinStreak == ScriptWinStreak.BoxGreat || scriptWinStreak == ScriptWinStreak.BoxUltra)
        {
            GachaPackRarity gachaPackRarity = scriptWinStreak switch
            {
                ScriptWinStreak.BoxClassic => GachaPackRarity.Classic,
                ScriptWinStreak.BoxGreat => GachaPackRarity.Great,
                ScriptWinStreak.BoxUltra => GachaPackRarity.Ultra,
                _ => GachaPackRarity.Classic
            };

            if (!PlayerPrefs.HasKey(cellSO.GachaKey))
                cellSO.SetGachaIndex(m_CurrentHighestArenaVariable.value.index);

            cellSO.PackType = PackType.Item;
            cellSO.ItemPack.Value = 1;
            cellSO.ItemPack.ItemSO = m_PBGachaPackManagerSO.GetGachaPackByArenaIndex(gachaPackRarity, cellSO.GachaIndex);
        }
        else if (scriptWinStreak == ScriptWinStreak.SkillCard)
        {
            SkillCardArena SkillCardArena = cellSO.IsPremium
                        ? m_SkillCardArena[m_CurrentHighestArenaVariable.value.index].PremiumSetConfig
                        : m_SkillCardArena[m_CurrentHighestArenaVariable.value.index].StandSetConfig;

            int randomCoefficientStand = UnityEngine.Random.Range(SkillCardArena.MinValue, SkillCardArena.MaxValue + 1);
            float cardCalc = randomCoefficientStand * SkillCardArena.Multiplier * index;
            PlayerPrefs.SetInt(GetRewardKey(cellSO), Mathf.CeilToInt(cardCalc));

            cellSO.PackType = PackType.Item;
            cellSO.ItemPack.Value = Mathf.CeilToInt(cardCalc);
            cellSO.ItemPack.ItemSO = m_GachaCardRandomActiveSkill;
            cellSO.icon = m_GachaCardRandomActiveSkill.GetThumbnailImage();
        }
    }

    public void LoadRewardFollowingArena(List<WinStreakCellSO> winStreakCellSOs)
    {
        for(int i = 0; i < winStreakCellSOs.Count; i++)
        {
            if (!winStreakCellSOs[i].IsClaimable && !winStreakCellSOs[i].IsClaimed)
            {
                PlayerPrefs.DeleteKey(GetRewardKey(winStreakCellSOs[i]));
                PlayerPrefs.DeleteKey(winStreakCellSOs[i].GachaKey);
                ApplyReducedAndRewards(winStreakCellSOs[i], winStreakCellSOs[i].ScriptWinStreak, i + 1);
            }
        }
    }
    #endregion

    #region TimeReward
    protected virtual string m_LastRewardTime
    {
        get
        {
            return PlayerPrefs.GetString($"Time-{m_Key}", "0");
        }
        set
        {
            PlayerPrefs.SetString($"Time-{m_Key}", value);
            value = PlayerPrefs.GetString($"Time-{m_Key}", "0");
        }
    }

    public virtual DateTime LastRewardTime
    {
        get
        {
            long time = long.Parse(m_LastRewardTime);
            return new DateTime(time);
        }
        private set => m_LastRewardTime = (value.Ticks.ToString());
    }

    public virtual bool IsResetReward
    {
        get
        {
            TimeSpan interval = DateTime.Now - LastRewardTime;
            return interval.TotalSeconds > m_TimeResetReward;
        }
    }

    /// <summary>
    /// EX: 15MIN 20S - 15(timeValueFontSize), MIN(labelFontSize)
    /// </summary>
    /// <param name="timeValueFontSize"></param>
    /// <param name="labelFontSize"></param>
    /// <returns></returns>
    public virtual string GetRemainingTimeHandle(float timeValueFontSize, float labelFontSize)
    {
        TimeSpan interval = DateTime.Now - LastRewardTime;
        double remainingSeconds = m_TimeResetReward - interval.TotalSeconds;
        interval = TimeSpan.FromSeconds(remainingSeconds);

        string hourAndMinutes = string.Format(
            "<size={0}>{2:00}<size={1}>H <size={0}>{3:00}<size={1}>M <size={0}>{4:00}<size={1}>S",
            timeValueFontSize, labelFontSize, interval.Hours, interval.Minutes, interval.Seconds
        );

        return hourAndMinutes;
    }

    [Button("Reset Time", ButtonSizes.Large), GUIColor(1, 1, 0), HorizontalGroup("Action Button")]
    public virtual void ResetReward()
    {
        LastRewardTime = DateTime.Now;
    }

    [Button("Set Time Reward Complete", ButtonSizes.Large), GUIColor(1, 1, 0.7f), HorizontalGroup("Action Button")]
    public virtual void ResetNow()
    {
        LastRewardTime = DateTime.Now - TimeSpan.FromSeconds(m_TimeResetReward);
    }
    #endregion

#if UNITY_EDITOR

    [Button]
    public void ClearKeyTheFirstTime()
    {
        PlayerPrefs.DeleteKey(GetTheFirstTime());
    }

    [OnInspectorGUI]
    protected virtual void LoadEditor()
    {
        GenerateSaveKey();
        //
    }

    protected virtual void GenerateSaveKey()
    {
            if (string.IsNullOrEmpty(m_Key) && !string.IsNullOrEmpty(name) && m_Key != name)
            {
                var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
                var guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
                m_Key = $"{name}_{guid}";
                UnityEditor.EditorUtility.SetDirty(this);
            }
    }
#endif
}

[Serializable]
public class ScriptStreak
{
    public List<ScriptWinStreak> ScriptStreakStandSet;
    public List<ScriptWinStreak> ScriptStreakPremiumSet;
}
[Serializable] public class WinStreakStandArena : WinStreakRewardArenaBase { }
[Serializable] public class WinStreakPremiumArena : WinStreakRewardArenaBase { }
[Serializable] public class SkillCardArena : WinStreakRewardArenaBase { }
public abstract class WinStreakRewardArenaBase
{
    [HorizontalGroup("Settings", LabelWidth = 100)]
    [BoxGroup("Settings/Value")]
    [LabelText("Minimum Value")]
    public int MinValue;

    [HorizontalGroup("Settings")]
    [BoxGroup("Settings/Value")]
    [LabelText("Maximum Value")]
    public int MaxValue;

    [HorizontalGroup("Settings")]
    [BoxGroup("Settings/Multiplier")]
    public float Multiplier;
}


[Serializable]
public class WinStreakStandConfig
{
    public WinStreakStandArena StandSetConfig;
    public WinStreakStandArena PremiumSetConfig;
}

[Serializable]
public class WinStreakPremiumConfig
{
    public WinStreakPremiumArena StandSetConfig;
    public WinStreakPremiumArena PremiumSetConfig;
}

[Serializable]
public class SkillCardConfig
{
    public SkillCardArena StandSetConfig;
    public SkillCardArena PremiumSetConfig;
}

