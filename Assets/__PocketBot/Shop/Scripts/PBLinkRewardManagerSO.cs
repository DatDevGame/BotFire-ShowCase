using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using HightLightDebug;
using HyrphusQ.SerializedDataStructure;
using PackReward;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum RewardSequenceType
{
    Nomal,
    Promoted
}

[CreateAssetMenu(fileName = "PBLinkRewardManagerSO", menuName = "PocketBots/PackReward/PBData/PBLinkRewardManagerSO")]
public class PBLinkRewardManagerSO : SerializedScriptableObject
{
    public bool m_IsDisplayed => m_HighestAchievedPPrefFloatTracker.value >= m_TrophyDisplayed;
    public bool IsActivePack => m_isActivePack;
    public RewardSequenceType RewardSequenceType => m_RewardSequenceType;
    public int TrophyDisplayed => m_TrophyDisplayed;

    [ReadOnly]
    public string m_Key;

    [SerializeField, BoxGroup("Config")] protected int m_TimeResetReward;
    [SerializeField, BoxGroup("Config")] protected int m_PromotedLostRewards;
    [SerializeField, BoxGroup("Config")] protected int m_TrophyDisplayed;
    [SerializeField, BoxGroup("Config")] protected Dictionary<RewardSequenceType, List<RateStandArena>>  m_RateStandArena;
    [SerializeField, BoxGroup("Config")] protected Dictionary<RewardSequenceType, List<RatePremiumArena>> m_RatePremiumArena;
    [SerializeField, BoxGroup("Resource")] protected Dictionary<RewardSequenceType, List<PBLinkRewardCellSO>> m_RewardSequence;
    [SerializeField, BoxGroup("Data")] protected PPrefIntVariable m_PromotedPPref;
    [SerializeField, BoxGroup("Data")] protected PBPvPTournamentSO m_PBPvPTournamentSO;
    [SerializeField, BoxGroup("Data")] protected CurrentHighestArenaVariable m_CurrentHighestArenaVariable;
    [SerializeField, BoxGroup("Data")] protected PBGachaPackManagerSO m_PBGachaPackManagerSO;
    [SerializeField, BoxGroup("Data")] protected HighestAchievedPPrefFloatTracker m_HighestAchievedPPrefFloatTracker;

    protected string m_ActiveKey => $"Active-{m_Key}";
    protected bool m_isActivePack => PlayerPrefs.HasKey(m_ActiveKey);
    protected RewardSequenceType m_RewardSequenceType => m_PromotedPPref.value >= m_PromotedLostRewards ? RewardSequenceType.Promoted : RewardSequenceType.Nomal;
    protected string GetReducedKey(PBLinkRewardCellSO pbLinkRewardCellSO) => $"ReducedValue-{pbLinkRewardCellSO.m_Key}";
    protected string GetRewardKey(PBLinkRewardCellSO pbLinkRewardCellSO) => $"RewardValue-{pbLinkRewardCellSO.m_Key}";
    protected int GetReducedValue(string key) => PlayerPrefs.GetInt($"ReducedValue-{key}");
    protected int GetRewardValue(string key) => PlayerPrefs.GetInt($"RewardValue-{key}");

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
        AddQueue();
        LastRewardTime = DateTime.Now;
    }

    [Button("Set Time Reward Complete", ButtonSizes.Large), GUIColor(1, 1, 0.7f), HorizontalGroup("Action Button")]
    public virtual void ResetNow()
    {
        LastRewardTime = DateTime.Now - TimeSpan.FromSeconds(m_TimeResetReward);
    }
    #endregion

    public virtual List<PBLinkRewardCellSO> GetQueue()
    {
        if (IsResetReward)
        {
            Reset();
        }

        RewardSequenceType rewardSequenceType = m_RewardSequenceType;
        List<PBLinkRewardCellSO> rewardCells = new List<PBLinkRewardCellSO>();
        CreateRewardCells(rewardCells, rewardSequenceType);

        return rewardCells;
    }

    #region Handle GetQueue
    protected void CreateRewardCells(List<PBLinkRewardCellSO> rewardCells, RewardSequenceType rewardSequenceType)
    {
        for (int i = 0; i < m_RewardSequence[rewardSequenceType].Count; i++)
        {
            PBLinkRewardCellSO cellSO = Instantiate(m_RewardSequence[rewardSequenceType][i]);
            ApplyReducedAndRewards(cellSO, rewardSequenceType, i);
            cellSO.UpdateRewardGroupInfo();
            rewardCells.Add(cellSO);
        }
    }

    protected void ApplyReducedAndRewards(PBLinkRewardCellSO cellSO, RewardSequenceType rewardSequenceType, int index)
    {
        string key = m_RewardSequence[rewardSequenceType][index].m_Key;

        if (cellSO.PackType == PackType.Currency)
        {
            cellSO.ReducedValue.Value = GetReducedValue(key);
            cellSO.CurrencyPack.Value = GetRewardValue(key);
        }
        else
        {
            cellSO.ReducedValue.Value = GetReducedValue(key);
            cellSO.ItemPack.Value = GetRewardValue(key);
            cellSO.ItemPack.ItemSO = m_PBGachaPackManagerSO.GetGachaPackCurrentArena(GetGachaPackRarity(rewardSequenceType, index));
        }
    }

    protected virtual GachaPackRarity GetGachaPackRarity(RewardSequenceType rewardSequenceType, int index)
    {
        return rewardSequenceType switch
        {
            RewardSequenceType.Nomal => index switch
            {
                1 => GachaPackRarity.Classic,
                3 => GachaPackRarity.Great,
                _ => GachaPackRarity.Classic
            },
            RewardSequenceType.Promoted => index switch
            {
                1 => GachaPackRarity.Great,
                3 => GachaPackRarity.Ultra,
                _ => GachaPackRarity.Classic
            },
            _ => GachaPackRarity.Classic
        };
    }

    protected virtual void ResetAllSequence()
    {
        for (int i = 0; i < m_RewardSequence.Count; i++)
            m_RewardSequence.ElementAt(i).Value.ForEach(v => v.ResetPack());
    }
    #endregion


    [Button("Add Queue", ButtonSizes.Large), GUIColor(1, 0.7f, 0.1f), HorizontalGroup("Action Button")]
    protected virtual void AddQueue()
    {
        SetRewardValues(m_RewardSequenceType);
        PlayerPrefs.Save();
    }

    #region Handle AddQueue
    protected void SetRewardValues(RewardSequenceType rewardType)
    {
        for (int i = 0; i < m_RewardSequence[rewardType].Count; i++)
        {
            int reducedValue = GetReducedValue(i);
            int rewardValue = GetRewardValue(i, rewardType);

            PlayerPrefs.SetInt(GetReducedKey(m_RewardSequence[rewardType][i]), reducedValue);
            PlayerPrefs.SetInt(GetRewardKey(m_RewardSequence[rewardType][i]), rewardValue);
        }
    }

    protected int GetReducedValue(int index)
    {
        return index switch
        {
            0 => 1,
            1 => 1,
            2 => 1,
            3 => 1,
            _ => 1
        };
    }

    protected int GetRewardValue(int index, RewardSequenceType rewardSequenceType)
    {
        RateStandArena rateStandArena = m_RateStandArena[rewardSequenceType][m_CurrentHighestArenaVariable.value.index];
        RatePremiumArena ratePremiumArena = m_RatePremiumArena[rewardSequenceType][m_CurrentHighestArenaVariable.value.index];

        int standMultip = UnityEngine.Random.Range(rateStandArena.MinValue, rateStandArena.MaxValue + 1) * (m_CurrentHighestArenaVariable.value as PBPvPArenaSO).WonNumOfCoins * rateStandArena.Multiplier;
        int premiumMultip = UnityEngine.Random.Range(ratePremiumArena.MinValue, ratePremiumArena.MaxValue + 1) * ratePremiumArena.Multiplier;

        return index switch
        {
            0 => standMultip, //Stand
            1 => 1, //Box
            2 => premiumMultip, //Premium
            3 => 1, //Box
            _ => 1
        };
    }
    #endregion

    public virtual void ActivePack()
    {
        if (!m_isActivePack)
        {
            PlayerPrefs.SetInt(m_ActiveKey, 1);
            Reset();
            ResetNow();
        }
    }

    public virtual void Reset()
    {
        ResetAllSequence();
        ResetReward();
    }


#if UNITY_EDITOR
    [Button("Clear Active Key", ButtonSizes.Large), GUIColor(0.1f, 0.9f, 0.5f), HorizontalGroup("Action Button 2")]
    public virtual void ClearActiveKey()
    {
        PlayerPrefs.DeleteKey(m_ActiveKey);
    }

    [OnInspectorGUI]
    protected virtual void LoadEditor()
    {
        GenerateSaveKey();
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

public class RateStandArena : RateArenaBase { }
public class RatePremiumArena : RateArenaBase { }
public abstract class RateArenaBase
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
    public int Multiplier;
}

