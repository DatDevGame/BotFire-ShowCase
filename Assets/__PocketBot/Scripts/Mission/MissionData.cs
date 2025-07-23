using System;
using I2.Loc;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class MissionData
{
    private static readonly string TARGET_PLACEHOLDER = "{[target]}";
    private static readonly string PROGRESS_PLACEHOLDER = "{[progress]}";
    private static readonly string CONDITION_PLACEHOLDER1 = "{[condition1]}";
    private static readonly string CONDITION_PLACEHOLDER2 = "{[condition2]}";
    private static readonly string MISSION_I2L_EUNM_PREFIX = "Mission_";
    private static readonly string MISSION_I2L_EUNM_PREFIX_SINGULAR = "Mission_Singular_";
    private static readonly string MISSION_I2L_ERROR = "Missing text enum: ";

    public event Action onMissionCompleted;
    public event Action onMissionProgressUpdated;
    public static event Action<MissionData, Vector2> OnMissionCompleted_Static;

    [SerializeField] private bool _isRewardClaimed;
    [SerializeField] private bool _isPreludeMission;
    [SerializeField] private bool _isFullyScripted; //Can later be changed to enum flag in case scripted-level design change. Its only has 2 levels of scripted now.
    [SerializeField] private int _preludeID; //Prelude mission need an ID to check if it's given or not
    [SerializeField] private float _progress;
    [SerializeField] private float _progressUI;
    [SerializeField] private float _targetValue;
    [SerializeField] private MissionTargetType _targetType;
    [SerializeField] private MissionScope _scope;
    [SerializeField] private float _currencyRewardAmount;
    [SerializeField] private MissionDifficulty _difficulty;
    [SerializeField] private object[] _conditionParamaters;

    [SerializeReference] private MissionProgressTracker _progressTracker;
    [ShowInInspector] private string _parsedDescription;

    [SerializeField] bool isPaused = false;

    public MissionProgressTracker progressTracker => _progressTracker;
    public bool isPreludeMission => _isPreludeMission;
    public int PreludeID => _preludeID;
    public float progress => _progress;
    public float progressUI
    {
        get => _progressUI;
        set
        {
            if (_progressUI != value)
            {
                _progressUI = value;
            }
        }
    }
    public float targetValue => _targetValue;
    public MissionTargetType targetType => _targetType;
    public MissionScope scope => _scope;
    public float currencyRewardAmount => _currencyRewardAmount;
    public MissionDifficulty difficulty => _difficulty;
    public object[] conditionParamaters => _conditionParamaters != null ? _conditionParamaters : _progressTracker != null ? _progressTracker.Serialize() : null;
    public string description
    {
        get
        {
            if (string.IsNullOrEmpty(_parsedDescription))
            {
                OnLocalizeEvent();
            }
            return _parsedDescription;
        }
    }
    public bool isCompleted => progress >= targetValue;
    public bool isCompletedUI => progressUI >= targetValue;
    public bool isRewardClaimed => _isRewardClaimed;
    public bool isFullyScripted => _isFullyScripted;

    //For Odin serializer only. Odin noob
    public MissionData() { }

    public MissionData(
        float targetValue,
        MissionTargetType targetType,
        MissionScope scope,
        float currencyRewardAmount,
        MissionDifficulty difficulty,
        MissionProgressTracker progressTracker,
        bool isPreludeMission = false,
        int preludeID = -1)
    {
        _isPreludeMission = false;
        _preludeID = -1;
        _isRewardClaimed = false;
        _progress = 0;
        _progressUI = 0;
        _targetValue = targetValue;
        _targetType = targetType;
        _scope = scope;
        _currencyRewardAmount = currencyRewardAmount;
        _difficulty = difficulty;

        _progressTracker = progressTracker;
        LocalizationManager.OnLocalizeEvent += OnLocalizeEvent;
        _isPreludeMission = isPreludeMission;
        _preludeID = preludeID;
        OnLocalizeEvent();
    }

    public void InitData(MissionProgressTracker progressTracker)
    {
        if (_progressTracker == null)
        {
            _progressTracker = progressTracker;
        }
        _conditionParamaters = _progressTracker.Serialize();
        InitTracker();
        OnLocalizeEvent();
    }

    public void ClearData()
    {
        ClearTracker();
        LocalizationManager.OnLocalizeEvent -= OnLocalizeEvent;
    }

    private void InitTracker()
    {
        if (isCompleted) return;
        _progressTracker.onMissionCompleted += OnMissionTrackerCompleted;
        _progressTracker.onMissionProgressChanged += OnMissionTrackerProgressChanged;
        _progressTracker.InitTracker();
    }

    private void ClearTracker()
    {
        _progressTracker.onMissionCompleted -= OnMissionTrackerCompleted;
        _progressTracker.onMissionProgressChanged -= OnMissionTrackerProgressChanged;
        _progressTracker.ClearTracker();
    }

    private void OnMissionTrackerCompleted()
    {
        if (isPaused) return;
        if (isCompleted) return;
        var oldProgress = _progress;
        _progress = _targetValue;
        NotifyMissionCompleted(oldProgress);
    }

    private void OnMissionTrackerProgressChanged(float changedAmount)
    {
        if (isPaused) return;
        if (isCompleted) return;
        var oldProgress = _progress;
        if (_targetType == MissionTargetType.StreakMastery_Reach_Streak) _progress = changedAmount;
        else _progress = Mathf.Min(_targetValue, _progress + changedAmount);
        if (oldProgress != _progress)
        {
            onMissionProgressUpdated?.Invoke();
        }
        if (isCompleted)
        {
            NotifyMissionCompleted(oldProgress);
        }
    }

    private void NotifyMissionCompleted(float oldProgress = 0)
    {
        onMissionCompleted?.Invoke();
        OnMissionCompleted_Static?.Invoke(this, new Vector2(oldProgress, _progress));
        ClearTracker();
    }

    private void OnLocalizeEvent()
    {
        string prefix = _targetValue == 1 ? MISSION_I2L_EUNM_PREFIX_SINGULAR : MISSION_I2L_EUNM_PREFIX;
        if (Enum.TryParse(prefix + _targetType, out I2LTerm descEnum))
        {
            _parsedDescription = I2LHelper.TranslateTerm(descEnum);
            if (!string.IsNullOrEmpty(_parsedDescription))
                _parsedDescription = _parsedDescription
                .Replace(TARGET_PLACEHOLDER, _targetValue.ToRoundedText())
                .Replace(PROGRESS_PLACEHOLDER, _progress.ToRoundedText())
                .Replace(CONDITION_PLACEHOLDER1, _progressTracker.condition1)
                .Replace(CONDITION_PLACEHOLDER2, _progressTracker.condition2);
        }
        else
        {
            _parsedDescription = MISSION_I2L_ERROR + prefix + _targetType;
        }
    }

    public void SetPaused(bool isPaused)
    {
        this.isPaused = isPaused;
    }

    public void ResetData()
    {
        ClearData();
        InitData(_progressTracker);
    }

    public void EarnReward()
    {
        _isRewardClaimed = true;
    }

    public void ResetReward()
    {
        _isRewardClaimed = false;
    }

    public void SetProgress(float changedAmount)
    {
        if (isPaused) return;
        var oldProgress = _progress;
        _progress = changedAmount;
        _progress = Mathf.Clamp(_progress, 0, _targetValue);
        if (oldProgress != _progress)
        {
            onMissionProgressUpdated?.Invoke();
        }
        if (isCompleted)
        {
            NotifyMissionCompleted(oldProgress);
        }
    }
}

public enum MissionTargetType
{
    MatchWins_Count_Any,
    MatchWins_Count_Duel,
    Battle_Top_1,
    WeaponMastery_Count_Equip,
    WeaponMastery_Count_Win,
    WeaponMastery_Count_Kill,
    MatchPlay_Count_Any,
    MatchPlay_Count_Duel,
    MatchPlay_Count_BattleRoyale,
    KillMaster_Count_Any,
    KillMaster_Count_DoubleKill,
    KillMaster_Count_TripleKill,
    KillMaster_Count_UltraKill,
    KillMaster_Count_RampageKill,
    EarnCurrency_Count_Trophies,
    EarnCurrency_Count_Coins,
    EarnCurrency_Count_Gems,
    StreakMastery_Reach_Streak,
    BossDefeats_Count,
    BossDefeats_Specific,
    OpenBoxes_CountAny,
    CardCollection_Any,
    CardCollection_Specific,
    ArenaUnlocks_Specific,
    BossClaim_Count,
    BossClaim_Specific,
    SpendCurrency_Count_Coins,
    SpendCurrency_Count_Gems,
    CardUpgrades_UptoLevel_Specific,
    PowerupsCollection_Count_Any,
    OfferClaims_Count_HotOffers,
    Purchases_Count_Any,
    WatchRVs_Count_Any,
}

public enum MissionScope
{
    Daily,
    Weekly,
    Season
}

public enum MissionDifficulty
{
    Easy,
    Medium,
    Hard,
    VeryHard,
}

public enum MissionMainCategory
{
    Main,
    Secondary,
    Side,
}

public enum MissionSubCategory
{
    MatchWins,
    BattleTop,
    WeaponMastery,
    MatchPlay,
    KillMaster,
    EarnCurrency,
    StreakMastery,
    BossDefeats,
    OpenBoxes,
    CardCollection,
    ArenaUnlocks,
    BossClaim,
    SpendCurrency,
    CardUpgrades,
    PowerupsCollection,
    OfferClaims,
    Purchases,
    WatchRVs,
}