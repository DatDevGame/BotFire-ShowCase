using System;
using System.Collections.Generic;
using DG.DemiLib;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionGeneratorConfigSO", menuName = "PocketBots/Mission/MissionGeneratorConfigSO")]
public class MissionGeneratorConfigSO : SerializedScriptableObject
{
    [SerializeField] private IntVariable _requireMedalToUnlockBattleRoyale;
    [SerializeField] private IntVariable _requireMedalToUnlockBattleBoss;
    [SerializeField] private HighestAchievedPPrefFloatTracker _highestAchievedMedal; 
    [SerializeField] private WinStreakManagerSO _winStreakManagerSO;
    [SerializeField] private PBPartManagerSO _frontPartManagerSO;
    [SerializeField] private PBPartManagerSO _upperPartManagerSO;
    [SerializeField, BoxGroup("Asset")] private Dictionary<string, Sprite> _missionIconByID = new();
    [SerializeField, BoxGroup("Scripted Round")] private Dictionary<MissionScope, Dictionary<int, List<MissionData>>> _scriptedMissions = new();
    [SerializeField, BoxGroup("Scripted Round")] private Dictionary<MissionScope, int> _maxScriptedRoundCounts = new();
    [SerializeField, BoxGroup("Priority Lists")] private int _dailyPrioritizedSpacing = new();
    [SerializeField, BoxGroup("Priority Lists")] private int _weeklyPrioritizedSpacing = new();
    [SerializeField, BoxGroup("Priority Lists")] private Dictionary<MissionScope, List<List<MissionTargetType>>> _prioritizedLists = new();
    [SerializeField, BoxGroup("Sheet Data")] private Dictionary<MissionScope, MissionScopeConfig> _scopeConfigs = new();
    [SerializeField, BoxGroup("Sheet Data")] private Dictionary<MissionTargetType, TargetTypeConfig> _targetTypeConfigs = new();
    [SerializeField, BoxGroup("Utility")] private Dictionary<MissionMainCategory, List<MissionSubCategory>> _mainToSubCategory = new();
    [SerializeField, BoxGroup("Utility")] private Dictionary<MissionSubCategory, List<MissionTargetType>> _subCategoryToTarget = new();

    public int dailyPrioritizedSpacing => _dailyPrioritizedSpacing;
    public int weeklyPrioritizedSpacing => _weeklyPrioritizedSpacing;

    private bool _isAtLeast1EpicOrHigherFront = false;
    private bool _isAtLeast1EpicOrHigherUpper = false;
    private bool _isAtLeast1LegendaryOrHigherFront = false;
    private bool _isAtLeast1LegendaryOrHigherUpper = false;

    private bool isBattleRoyaleUnlocked => _requireMedalToUnlockBattleRoyale.value <= _highestAchievedMedal.value;
    private bool isBattleBossUnlocked => _requireMedalToUnlockBattleBoss.value <= _highestAchievedMedal.value;
    private bool isWinStreakUnlocked => _winStreakManagerSO.ConditionDisplayedWinStreak;
    private bool isAtLeast1EpicOrHigherFront
    {
        get
        {
            if(!_isAtLeast1EpicOrHigherFront && !_isAtLeast1LegendaryOrHigherFront &&
                _frontPartManagerSO.Parts.Exists((partSO)=>partSO.IsUnlocked() && partSO.GetRarityType() >= RarityType.Epic))
            {
                _isAtLeast1EpicOrHigherFront = true;
            }
            return _isAtLeast1EpicOrHigherFront || _isAtLeast1LegendaryOrHigherFront;
        }
    }
    private bool isAtLeast1EpicOrHigherUpper
    {
        get
        {
            if(!_isAtLeast1EpicOrHigherUpper && ! _isAtLeast1LegendaryOrHigherUpper &&
                _upperPartManagerSO.Parts.Exists((partSO)=>partSO.IsUnlocked() && partSO.GetRarityType() >= RarityType.Epic))
            {
                _isAtLeast1EpicOrHigherUpper = true;
            }
            return _isAtLeast1EpicOrHigherUpper || _isAtLeast1LegendaryOrHigherUpper;
        }
    }
    private bool isAtLeast1LegendaryOrHigherFront
    {
        get
        {
            if(!_isAtLeast1LegendaryOrHigherFront &&
                _frontPartManagerSO.Parts.Exists((partSO)=>partSO.IsUnlocked() && partSO.GetRarityType() >= RarityType.Legendary))
            {
                _isAtLeast1EpicOrHigherFront = true;
                _isAtLeast1LegendaryOrHigherFront = true;
            }
            return _isAtLeast1LegendaryOrHigherFront;
        }
    }
    private bool isAtLeast1LegendaryOrHigherUpper
    {
        get
        {
            if(! _isAtLeast1LegendaryOrHigherUpper &&
                _upperPartManagerSO.Parts.Exists((partSO)=>partSO.IsUnlocked() && partSO.GetRarityType() >= RarityType.Legendary))
            {
                _isAtLeast1EpicOrHigherUpper = true;
                _isAtLeast1LegendaryOrHigherUpper = true;
            }
            return _isAtLeast1LegendaryOrHigherUpper;
        }
    }

    public MissionMainCategory TargetToMainCategory(MissionTargetType targetType)
    {
        return _targetTypeConfigs[targetType].mainCategory;
    }

    public MissionSubCategory TargetToSubCategory(MissionTargetType targetType)
    {
        return _targetTypeConfigs[targetType].subCategory;
    }

    public List<MissionSubCategory> MainToSubCategory(MissionMainCategory mainCategory)
    {
        return _mainToSubCategory[mainCategory];
    }

    public List<MissionTargetType> SubCategoryToTarget(MissionSubCategory subCategory)
    {
        return _subCategoryToTarget[subCategory];
    }
    
    public float GetAmountMultiplier(MissionScope scope)
    {
        return _scopeConfigs[scope].amountMultiplier;
    }

    public float GetTokenAmount(MissionScope scope, MissionDifficulty difficulty)
    {
        return _scopeConfigs[scope].GetTokenAmount(difficulty); 
    }

    public MissionCategoryRate<MissionMainCategory> GetMainCategoryRate(MissionScope scope, MissionMainCategory mainCategory)
    {
        return _scopeConfigs[scope].GetMainCategoryRate(mainCategory);
    }

    public MissionCategoryRate<MissionSubCategory> GetSubCategoryRate(MissionScope scope, MissionSubCategory subCategory)
    {
        return _scopeConfigs[scope].GetSubCategoryRate(subCategory);
    }

    public IntRange GetMainCategoryRangeInt(MissionScope scope, MissionMainCategory mainCategory)
    {
        return _scopeConfigs[scope].GetMainCategoryRangeInt(mainCategory);
    }

    public float GetAmountFactor(MissionTargetType targetType, MissionDifficulty difficulty)
    {
        return _targetTypeConfigs[targetType].amountFactor[difficulty];
    }

    public Sprite GetMissionIcon(MissionTargetType targetType)
    {
        string iconID = _targetTypeConfigs[targetType].iconID;
        if (string.IsNullOrEmpty(iconID) || _missionIconByID == null || !_missionIconByID.TryGetValue(iconID, out Sprite missionIcon))
            return null;
        return missionIcon;
    }

    public List<MissionDifficulty> GetEnabledDIfficulties(MissionScope scope, MissionTargetType targetType)
    {
        return _scopeConfigs[scope].GetEnabledDIfficulties(targetType);
    }

    public List<MissionData> GetScriptedMissions(MissionScope scope, int roundCount)
    {
        if (_scriptedMissions.TryGetValue(scope, out var missionRounds) &&
            missionRounds.TryGetValue(roundCount, out var missions))
            return missions;
        return null;
    }

    public int GetMaxScriptedRoundCount(MissionScope scope)
    {
        return _maxScriptedRoundCounts.TryGetValue(scope, out var max) ? max : 0;
    }

    public List<MissionTargetType> GetPrioritizedTargetList(MissionScope scope, int index)
    {
        if (_prioritizedLists.TryGetValue(scope, out var prioritizedList) && index >= 0 && index < prioritizedList.Count)
            return prioritizedList[index];
        return null;
    }

    public bool IsTargetTypeEnable(MissionScope scope, MissionTargetType targetType)
    {
        if (!_scopeConfigs[scope].IsTargetTypeEnable(targetType)) return false;
        switch(scope)
        {
            case MissionScope.Daily:
                return targetType switch
                {
                    MissionTargetType.Battle_Top_1 => isBattleRoyaleUnlocked,
                    MissionTargetType.MatchPlay_Count_BattleRoyale => isBattleRoyaleUnlocked,
                    MissionTargetType.KillMaster_Count_DoubleKill => isBattleRoyaleUnlocked,
                    MissionTargetType.KillMaster_Count_TripleKill => isBattleRoyaleUnlocked,
                    MissionTargetType.KillMaster_Count_UltraKill => isBattleRoyaleUnlocked,
                    MissionTargetType.KillMaster_Count_RampageKill => isBattleRoyaleUnlocked,
                    MissionTargetType.StreakMastery_Reach_Streak => isWinStreakUnlocked,
                    MissionTargetType.BossDefeats_Count => isBattleBossUnlocked,
                    MissionTargetType.BossDefeats_Specific => isBattleBossUnlocked,
                    MissionTargetType.BossClaim_Count => isBattleBossUnlocked,
                    MissionTargetType.BossClaim_Specific => isBattleBossUnlocked,
                    _ => true,
                };
            case MissionScope.Weekly:
                return targetType switch
                {
                    MissionTargetType.WeaponMastery_Count_Equip => isAtLeast1EpicOrHigherFront || isAtLeast1EpicOrHigherUpper,
                    MissionTargetType.WeaponMastery_Count_Win => isAtLeast1EpicOrHigherFront || isAtLeast1EpicOrHigherUpper,
                    MissionTargetType.WeaponMastery_Count_Kill => isAtLeast1EpicOrHigherFront || isAtLeast1EpicOrHigherUpper,
                    MissionTargetType.StreakMastery_Reach_Streak => isWinStreakUnlocked,
                    MissionTargetType.BossDefeats_Count => isBattleBossUnlocked,
                    MissionTargetType.BossDefeats_Specific => isBattleBossUnlocked,
                    MissionTargetType.BossClaim_Count => isBattleBossUnlocked,
                    MissionTargetType.BossClaim_Specific => isBattleBossUnlocked,
                    _ => true,
                };
            case MissionScope.Season:
                return targetType switch
                {
                    MissionTargetType.WeaponMastery_Count_Equip => isAtLeast1LegendaryOrHigherFront || isAtLeast1LegendaryOrHigherUpper,
                    MissionTargetType.WeaponMastery_Count_Win => isAtLeast1LegendaryOrHigherFront || isAtLeast1LegendaryOrHigherUpper,
                    MissionTargetType.WeaponMastery_Count_Kill => isAtLeast1LegendaryOrHigherFront || isAtLeast1LegendaryOrHigherUpper,
                    MissionTargetType.StreakMastery_Reach_Streak => isWinStreakUnlocked,
                    MissionTargetType.BossDefeats_Count => isBattleBossUnlocked,
                    MissionTargetType.BossDefeats_Specific => isBattleBossUnlocked,
                    MissionTargetType.BossClaim_Count => isBattleBossUnlocked,
                    MissionTargetType.BossClaim_Specific => isBattleBossUnlocked,
                    _ => true,
                };
        }
        return false;
    }
    
#if UNITY_EDITOR
    [SerializeField, FolderPath, BoxGroup("Editor")] private string _iconFolder;

    [Button]
    private void GetAllMissionIcon()
    {
        _missionIconByID.Clear();
        string[] files = System.IO.Directory.GetFiles(_iconFolder, "*.*", System.IO.SearchOption.TopDirectoryOnly);
        foreach(string file in files)
        {
            Sprite sprite;
            if (sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(file))
            {
                _missionIconByID[sprite.name] = sprite;
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
    }

    [Button]
    private void ValidateAllMissionIcon()
    {
        bool isSomethingWrong = false;
        foreach(MissionTargetType targetType in _targetTypeConfigs.Keys)
        {
            if (!GetMissionIcon(targetType))
            {
                Debug.LogError($"{targetType} has wrong icon configs");
                isSomethingWrong = true;
            }
        }
        if (!isSomethingWrong)
            Debug.LogError("All MissionTargetType have correct icon");
    }
#endif
}

[Serializable]
public class MissionScopeConfig
{
    [SerializeField] private float _amountMultiplier;
    [SerializeField] private Dictionary<MissionDifficulty, float> _tokenAmountsByDifficulty = new();
    [SerializeField] private Dictionary<MissionMainCategory, MissionCategoryRate<MissionMainCategory>> _mainCategoryRates = new();
    [SerializeField] private Dictionary<MissionMainCategory, IntRange> _mainCategoryRangeInts = new();
    [SerializeField] private Dictionary<MissionSubCategory, MissionCategoryRate<MissionSubCategory>> _subCategoryRates = new();
    [SerializeField] private Dictionary<MissionTargetType, bool> _isEnabled = new();
    [SerializeField] private Dictionary<MissionTargetType, Dictionary<MissionDifficulty, bool>> _difficultyEnableMap = new();

    public float amountMultiplier => _amountMultiplier;

    public float GetTokenAmount(MissionDifficulty difficulty)
    {
        return _tokenAmountsByDifficulty[difficulty];
    }

    public MissionCategoryRate<MissionMainCategory> GetMainCategoryRate(MissionMainCategory mainCategory)
    {
        return _mainCategoryRates[mainCategory];
    }

    public MissionCategoryRate<MissionSubCategory> GetSubCategoryRate(MissionSubCategory subCategory)
    {
        return _subCategoryRates[subCategory];
    }

    public IntRange GetMainCategoryRangeInt(MissionMainCategory mainCategory)
    {
        return _mainCategoryRangeInts[mainCategory];
    }

    public List<MissionDifficulty> GetEnabledDIfficulties(MissionTargetType targetType)
    {
        List<MissionDifficulty> difficulties = new();
        Dictionary<MissionDifficulty, bool> enabledDifficulties = _difficultyEnableMap[targetType];
        foreach(MissionDifficulty difficulty in enabledDifficulties.Keys)
        {
            if (enabledDifficulties[difficulty])
            {
                difficulties.Add(difficulty);
            }
        }
        return difficulties;
    }

    public bool IsTargetTypeEnable(MissionTargetType targetType)
    {
        if (!_isEnabled[targetType]) return false;
        foreach (MissionDifficulty difficulty in _difficultyEnableMap[targetType].Keys)
        {
            if (_difficultyEnableMap[targetType][difficulty]) return true;
        }
        return false;
    }
}

public struct MissionCategoryRate<T> : IRandomizable
{
    [Range(0f, 1f)] public float probability;
    public T value;
    public float Probability { get => probability; set => probability = value; }
}

public struct TargetTypeConfig
{
    public string iconID;
    public Dictionary<MissionDifficulty, float> amountFactor;
    public MissionTargetType targetType;
    public MissionMainCategory mainCategory;
    public MissionSubCategory subCategory;
}