using System;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using LatteGames.PvP;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionSavedDataSO", menuName = "PocketBots/Mission/MissionSavedDataSO")]
public class MissionSavedDataSO : SavedDataSO<MissionSavedDataSO.Data>
{
    [Serializable]
    public class Data : SavedData
    {
        public List<MissionData> DailyMissions = new();
        public List<MissionData> WeeklyMissions = new();
        public List<MissionData> SeasonMissions = new();
        public List<MissionData> PreludeMissions = new();
        public HashSet<int> GivenPreludeMissions = new();
        public HashSet<int> DonePreludeMissions = new();
        public Dictionary<MissionScope, int> ScriptedRoundCount = new();
        public int weeklyMissionsCreatedInSeason = 0;
        public int seasonMissionsCreatedInSeason = 0;
    }
#if UNITY_EDITOR
    [NonSerialized, ShowInInspector, BoxGroup("Debug")] List<MissionData> DebugDailyMissions;
    [NonSerialized, ShowInInspector, BoxGroup("Debug")] List<MissionData> DebugWeeklyMissions;
    [NonSerialized, ShowInInspector, BoxGroup("Debug")] List<MissionData> DebugSeasonMissions;
    [NonSerialized, ShowInInspector, BoxGroup("Debug")] List<MissionData> DebugPreludeMissions;
#endif
    [SerializeField, BoxGroup("Prelude")] private List<MissionData> _preludeMissions = new();
    [SerializeField, BoxGroup("Prelude")] private List<MissionData> _preludeMissions_initialWeapons = new();
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable _isCompletedAllPrelude;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable _currentWinStreak;
    [SerializeField, BoxGroup("Data")] private PBPvPTournamentSO _tournamentSO;
    [SerializeField, BoxGroup("Data")] private CurrentHighestArenaVariable _highestArenaSO;
    [SerializeField, BoxGroup("Data")] private PBGachaPackManagerSO _gachaPackManagerSO;
    [SerializeField, BoxGroup("Data")] private PBPartManagerSO _frontPartManagerSO;
    [SerializeField, BoxGroup("Data")] private PBPartManagerSO _upperPartManagerSO;
    [SerializeField, BoxGroup("Data")] private PBPartManagerSO _bodyPartManagerSO;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_TotalTodayMissionCompletedInSeason;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_TotalWeeklyMissionCompletedInSeason;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_TotalSeasonMissionCompletedInSeason;
    private Dictionary<string, PBPartSO> _idToPartDictionary = new();
    private Dictionary<MissionTargetType, MissionTargetCalculator> _missionTargetCalculators = new();

    public bool isPreludeCompleted => _isCompletedAllPrelude.value;
    public List<MissionData> preludeMissions => InitialWeaponsABTest.isActive ? _preludeMissions_initialWeapons : _preludeMissions;
    public PPrefIntVariable TotalTodayMissionCompletedInSeason => m_TotalTodayMissionCompletedInSeason;
    public PPrefIntVariable TotalWeeklyMissionCompletedInSeason => m_TotalWeeklyMissionCompletedInSeason;
    public PPrefIntVariable TotalSeasonMissionCompletedInSeason => m_TotalSeasonMissionCompletedInSeason;

    protected override void NotifyEventDataLoaded()
    {
        _idToPartDictionary.Clear();
        foreach (var partSO in _frontPartManagerSO.Parts)
        {
            if (!_idToPartDictionary.ContainsKey(partSO.guid))
                _idToPartDictionary.Add(partSO.guid, partSO);
        }
        foreach (var partSO in _upperPartManagerSO.Parts)
        {
            if (!_idToPartDictionary.ContainsKey(partSO.guid))
                _idToPartDictionary.Add(partSO.guid, partSO);
        }
        foreach (var partSO in _bodyPartManagerSO.Parts)
        {
            if (!_idToPartDictionary.ContainsKey(partSO.guid))
                _idToPartDictionary.Add(partSO.guid, partSO);
        }
        foreach (MissionData missionData in data.DailyMissions)
            missionData.InitData(CreateMissionProgressTracker(true, missionData.targetType, missionData.conditionParamaters));
        foreach (MissionData missionData in data.WeeklyMissions)
            missionData.InitData(CreateMissionProgressTracker(true, missionData.targetType, missionData.conditionParamaters));
        foreach (MissionData missionData in data.SeasonMissions)
            missionData.InitData(CreateMissionProgressTracker(true, missionData.targetType, missionData.conditionParamaters));
        foreach (MissionData missionData in data.PreludeMissions)
            missionData.InitData(CreateMissionProgressTracker(true, missionData.targetType, missionData.conditionParamaters));
#if UNITY_EDITOR
        DebugDailyMissions = data.DailyMissions;
        DebugWeeklyMissions = data.WeeklyMissions;
        DebugSeasonMissions = data.SeasonMissions;
        DebugPreludeMissions = data.PreludeMissions;

        HashSet<int> preludeID = new();
        MissionData mission;
        for (int i = 0, length = preludeMissions.Count; i < length; i++)
        {
            mission = preludeMissions[i];
            if (!preludeID.Add(mission.PreludeID) || !mission.isPreludeMission)
                Debug.LogError($"Prelude mission with id {mission.PreludeID}, index {i} has duplicated ID or not being marked as prelude mission");
        }
#endif
        base.NotifyEventDataLoaded();
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        DebugDailyMissions = null;
        DebugWeeklyMissions = null;
        DebugSeasonMissions = null;
        DebugPreludeMissions = null;
#endif
    }

    private MissionProgressTracker CreateMissionProgressTracker(bool isDeserialize, MissionTargetType missionTargetType, params object[] objects)
    {
        switch (missionTargetType)
        {
            case MissionTargetType.MatchWins_Count_Any:
                return new MatchWins_Count_Any_ProgressTracker();
            case MissionTargetType.MatchWins_Count_Duel:
                return new MatchWins_Count_Duel_ProgressTracker();
            case MissionTargetType.Battle_Top_1:
                return new Battle_Top_1_ProgressTracker();
            case MissionTargetType.WeaponMastery_Count_Equip:
                return isDeserialize ? new WeaponMastery_Count_Equip_ProgressTracker(_idToPartDictionary[objects[0] as string]) :
                    new WeaponMastery_Count_Equip_ProgressTracker(objects[0] as PBPartSO);
            case MissionTargetType.WeaponMastery_Count_Win:
                return isDeserialize ? new WeaponMastery_Count_Win_ProgressTracker(_idToPartDictionary[objects[0] as string]) :
                    new WeaponMastery_Count_Win_ProgressTracker(objects[0] as PBPartSO);
            case MissionTargetType.WeaponMastery_Count_Kill:
                return isDeserialize ? new WeaponMastery_Count_Kill_ProgressTracker(_idToPartDictionary[objects[0] as string]) :
                    new WeaponMastery_Count_Kill_ProgressTracker(objects[0] as PBPartSO);
            case MissionTargetType.MatchPlay_Count_Any:
                return new MatchPlay_Count_Any_ProgressTracker();
            case MissionTargetType.MatchPlay_Count_Duel:
                return new MatchPlay_Count_Duel_ProgressTracker();
            case MissionTargetType.MatchPlay_Count_BattleRoyale:
                return new MatchPlay_Count_BattleRoyale_ProgressTracker();
            case MissionTargetType.KillMaster_Count_Any:
                return new KillMaster_Count_Any_ProgressTracker();
            case MissionTargetType.KillMaster_Count_DoubleKill:
                return new KillMaster_Count_DoubleKill_ProgressTracker();
            case MissionTargetType.KillMaster_Count_TripleKill:
                return new KillMaster_Count_TripleKill_ProgressTracker();
            case MissionTargetType.KillMaster_Count_UltraKill:
                return new KillMaster_Count_UltraKill_ProgressTracker();
            case MissionTargetType.KillMaster_Count_RampageKill:
                return new KillMaster_Count_RampageKill_ProgressTracker();
            case MissionTargetType.EarnCurrency_Count_Trophies:
                return new EarnCurrency_Count_Trophies_ProgressTracker();
            case MissionTargetType.EarnCurrency_Count_Coins:
                return new EarnCurrency_Count_Coins_ProgressTracker();
            case MissionTargetType.EarnCurrency_Count_Gems:
                return new EarnCurrency_Count_Gems_ProgressTracker();
            case MissionTargetType.StreakMastery_Reach_Streak:
                return new StreakMastery_Reach_Streak_ProgressTracker(_currentWinStreak);
            case MissionTargetType.BossDefeats_Count:
                return new BossDefeats_Count_ProgressTracker();
            case MissionTargetType.BossDefeats_Specific:
                return isDeserialize ? new BossDefeats_Specific_ProgressTracker(BossFightManager.Instance.GetBossSOByKey(objects[0] as string)) :
                    new BossDefeats_Specific_ProgressTracker(objects[0] as BossSO);
            case MissionTargetType.OpenBoxes_CountAny:
                return new OpenBoxes_CountAny_ProgressTracker();
            case MissionTargetType.CardCollection_Any:
                return new CardCollection_Any_ProgressTracker();
            case MissionTargetType.CardCollection_Specific:
                return isDeserialize ? new CardCollection_Specific_ProgressTracker(_idToPartDictionary[objects[0] as string]) :
                    new CardCollection_Specific_ProgressTracker(objects[0] as PBPartSO);
            case MissionTargetType.ArenaUnlocks_Specific:
                return isDeserialize ? new ArenaUnlocks_Specific_ProgressTracker(_tournamentSO.arenas[(int)objects[0]] as PvPArenaSO) :
                    new ArenaUnlocks_Specific_ProgressTracker(objects[0] as PvPArenaSO);
            case MissionTargetType.BossClaim_Count:
                return new BossClaim_Count_ProgressTracker();
            case MissionTargetType.BossClaim_Specific:
                return isDeserialize ? new BossClaim_Specific_ProgressTracker(BossFightManager.Instance.GetBossSOByKey(objects[0] as string)) :
                new BossClaim_Specific_ProgressTracker(objects[0] as BossSO);
            case MissionTargetType.SpendCurrency_Count_Coins:
                return new SpendCurrency_Count_Coins_ProgressTracker();
            case MissionTargetType.SpendCurrency_Count_Gems:
                return new SpendCurrency_Count_Gems_ProgressTracker();
            case MissionTargetType.CardUpgrades_UptoLevel_Specific:
                return isDeserialize ? new CardUpgrades_UptoLevel_Specific_ProgressTracker(_idToPartDictionary[objects[0] as string], (int)objects[1]) :
                    new CardUpgrades_UptoLevel_Specific_ProgressTracker(objects[0] as PBPartSO, (int)objects[1]);
            case MissionTargetType.PowerupsCollection_Count_Any:
                return new PowerupsCollection_Count_Any_ProgressTracker();
            case MissionTargetType.OfferClaims_Count_HotOffers:
                return new OfferClaims_Count_HotOffers_ProgressTracker();
            case MissionTargetType.Purchases_Count_Any:
                return new Purchases_Count_Any_ProgressTracker();
            case MissionTargetType.WatchRVs_Count_Any:
                return new WatchRVs_Count_Any_ProgressTracker();
            default:
                Debug.LogError($"{missionTargetType}_Tracker is not implemented yet");
                return null;
        }
    }

    public MissionTargetCalculator GetMissionTargetCalculator(MissionTargetType missionTargetType)
    {
        if (_missionTargetCalculators.TryGetValue(missionTargetType, out MissionTargetCalculator targetCalculator))
            return targetCalculator;
        switch (missionTargetType)
        {
            case MissionTargetType.WeaponMastery_Count_Equip:
            case MissionTargetType.WeaponMastery_Count_Win:
            case MissionTargetType.WeaponMastery_Count_Kill:
                targetCalculator = new WeaponMastery_TargetCalculator(_frontPartManagerSO, _upperPartManagerSO);
                _missionTargetCalculators[MissionTargetType.WeaponMastery_Count_Equip] =
                _missionTargetCalculators[MissionTargetType.WeaponMastery_Count_Win] =
                _missionTargetCalculators[MissionTargetType.WeaponMastery_Count_Kill] = targetCalculator;
                break;
            case MissionTargetType.EarnCurrency_Count_Trophies:
                targetCalculator = new EarnCurrency_Count_Trophies_TargetCalculator(_highestArenaSO);
                _missionTargetCalculators[MissionTargetType.EarnCurrency_Count_Trophies] = targetCalculator;
                break;
            case MissionTargetType.EarnCurrency_Count_Coins:
                targetCalculator = new EarnCurrency_Count_Coins_TargetCalculator(_highestArenaSO);
                _missionTargetCalculators[MissionTargetType.EarnCurrency_Count_Coins] = targetCalculator;
                break;
            case MissionTargetType.EarnCurrency_Count_Gems:
                targetCalculator = new EarnCurrency_Count_Gems_TargetCalculator(_highestArenaSO);
                _missionTargetCalculators[MissionTargetType.EarnCurrency_Count_Gems] = targetCalculator;
                break;
            case MissionTargetType.StreakMastery_Reach_Streak:
                targetCalculator = new StreakMastery_Reach_Streak_TargetCalculator();
                _missionTargetCalculators[MissionTargetType.StreakMastery_Reach_Streak] = targetCalculator;
                break;
            case MissionTargetType.BossDefeats_Count:
                targetCalculator = new BossDefeats_Count_TargetCalculator();
                _missionTargetCalculators[MissionTargetType.BossDefeats_Count] = targetCalculator;
                break;
            case MissionTargetType.BossDefeats_Specific:
                targetCalculator = new BossDefeats_Specific_TargetCalculator();
                _missionTargetCalculators[MissionTargetType.BossDefeats_Specific] = targetCalculator;
                break;
            case MissionTargetType.CardCollection_Any:
                targetCalculator = new CardCollection_Any_TargetCalculator(_gachaPackManagerSO);
                _missionTargetCalculators[MissionTargetType.CardCollection_Any] = targetCalculator;
                break;
            case MissionTargetType.CardCollection_Specific:
                targetCalculator = new CardCollection_Specific_TargetCalculator(_gachaPackManagerSO);
                _missionTargetCalculators[MissionTargetType.CardCollection_Specific] = targetCalculator;
                break;
            case MissionTargetType.ArenaUnlocks_Specific:
                targetCalculator = new ArenaUnlocks_Specific_TargetCalculator(_highestArenaSO, _tournamentSO);
                _missionTargetCalculators[MissionTargetType.ArenaUnlocks_Specific] = targetCalculator;
                break;
            case MissionTargetType.SpendCurrency_Count_Coins:
            case MissionTargetType.SpendCurrency_Count_Gems:
                targetCalculator = new SpendCurrency_TargetCalculator();
                _missionTargetCalculators[MissionTargetType.SpendCurrency_Count_Coins] =
                _missionTargetCalculators[MissionTargetType.SpendCurrency_Count_Gems] = targetCalculator;
                break;
            case MissionTargetType.BossClaim_Count:
                targetCalculator = new BossClaim_Count_TargetCalculator();
                _missionTargetCalculators[MissionTargetType.BossClaim_Count] = targetCalculator;
                break;
            case MissionTargetType.BossClaim_Specific:
                targetCalculator = new BossClaim_Specific_TargetCalculator();
                _missionTargetCalculators[MissionTargetType.BossClaim_Specific] = targetCalculator;
                break;
            case MissionTargetType.CardUpgrades_UptoLevel_Specific:
                targetCalculator = new CardUpgrades_UptoLevel_Specific_TargetCalculator(_frontPartManagerSO, _upperPartManagerSO, _bodyPartManagerSO);
                _missionTargetCalculators[MissionTargetType.CardUpgrades_UptoLevel_Specific] = targetCalculator;
                break;
            default:
                targetCalculator = new MissionTargetCalculator();
                _missionTargetCalculators[MissionTargetType.MatchWins_Count_Any] =
                _missionTargetCalculators[MissionTargetType.MatchWins_Count_Duel] =
                _missionTargetCalculators[MissionTargetType.Battle_Top_1] =
                _missionTargetCalculators[MissionTargetType.MatchPlay_Count_Any] =
                _missionTargetCalculators[MissionTargetType.MatchPlay_Count_Duel] =
                _missionTargetCalculators[MissionTargetType.MatchPlay_Count_BattleRoyale] =
                _missionTargetCalculators[MissionTargetType.KillMaster_Count_Any] =
                _missionTargetCalculators[MissionTargetType.KillMaster_Count_DoubleKill] =
                _missionTargetCalculators[MissionTargetType.KillMaster_Count_TripleKill] =
                _missionTargetCalculators[MissionTargetType.KillMaster_Count_UltraKill] =
                _missionTargetCalculators[MissionTargetType.KillMaster_Count_RampageKill] =
                _missionTargetCalculators[MissionTargetType.OpenBoxes_CountAny] =
                _missionTargetCalculators[MissionTargetType.PowerupsCollection_Count_Any] =
                _missionTargetCalculators[MissionTargetType.OfferClaims_Count_HotOffers] =
                _missionTargetCalculators[MissionTargetType.Purchases_Count_Any] =
                _missionTargetCalculators[MissionTargetType.WatchRVs_Count_Any] = targetCalculator;
                break;
        }
        return targetCalculator;
    }

    public MissionData CreateMission(MissionScope scope, MissionTargetType targetType, MissionDifficulty difficulty, float amountFactor, float amountMultiplier, float tokenAmount)
    {
        //Assumption: you already call MissionTargetCalculator.IsHaveValidTarget in the mission target filtering process
        //OR, the mission don't need to validate it's target at all, like these default sub category
        //(MissionSubCategory.MatchWins, MissionSubCategory.BattleTop, MissionSubCategory.KillMaster, MissionSubCategory.EarnCurrency)
        MissionTargetCalculator targetCalculator = GetMissionTargetCalculator(targetType);
        float targetValue = targetCalculator.CalculateTarget(amountFactor, amountMultiplier);
        targetValue = Mathf.CeilToInt(targetValue);
        MissionData missionData = new(
            targetValue,
            targetType,
            scope,
            tokenAmount,
            difficulty,
            CreateMissionProgressTracker(false, targetType, targetCalculator.additionalDatas)
        );
        return missionData;
    }

    public List<MissionData> GetMissionListByScope(MissionScope scope)
    {
        return scope switch
        {
            MissionScope.Daily => data.DailyMissions,
            MissionScope.Weekly => data.WeeklyMissions,
            MissionScope.Season => data.SeasonMissions,
            _ => null,
        };
    }

    /// <summary>
    /// Add when got a new mission to save it's progress.
    /// </summary>
    /// <param name="missionData"></param>
    public void AddMission(MissionData missionData)
    {
        missionData.InitData(null);
        if (missionData.isPreludeMission)
        {
            if (data.GivenPreludeMissions.Add(missionData.PreludeID)) data.PreludeMissions.Add(missionData);
            return;
        }
        var missionDatas = GetMissionListByScope(missionData.scope);
        if (missionDatas == null || missionDatas.Contains(missionData))
            return;
        missionDatas.Add(missionData);
    }

    /// <summary>
    /// Remove when a mission is done or swapped.
    /// </summary>
    /// <param name="missionData"></param>
    public void RemoveMission(MissionData missionData)
    {
        missionData.ClearData();
        if (missionData.isPreludeMission)
        {
            data.PreludeMissions.Remove(missionData);
            data.DonePreludeMissions.Add(missionData.PreludeID);
            if (data.DonePreludeMissions.Count == preludeMissions.Count) _isCompletedAllPrelude.value = true;
            return;
        }
        GetMissionListByScope(missionData.scope)?.Remove(missionData);
    }

    /// <summary>
    /// Remove old mission and add a new one to the same posision in data list
    /// </summary>
    /// <param name="oldMission"></param>
    /// <param name="newMission"></param>
    public void ReplaceMission(MissionData oldMission, MissionData newMission)
    {
        if (oldMission.isPreludeMission || newMission.isPreludeMission || oldMission.scope != newMission.scope)
            return;
        var missionDatas = GetMissionListByScope(oldMission.scope);
        oldMission.ClearData();
        newMission.InitData(null);
        missionDatas[missionDatas.IndexOf(oldMission)] = newMission;
    }

    /// <summary>
    /// Use in case you wanna clear all missions (end of seasons, end of prelude,...)
    /// </summary>
    public void ClearAllMissions()
    {
        foreach (MissionData missionData in data.DailyMissions) missionData.ClearData();
        foreach (MissionData missionData in data.WeeklyMissions) missionData.ClearData();
        foreach (MissionData missionData in data.SeasonMissions) missionData.ClearData();
        foreach (MissionData missionData in data.PreludeMissions) missionData.ClearData();
        data = defaultData;
#if UNITY_EDITOR
        DebugDailyMissions = data.DailyMissions;
        DebugWeeklyMissions = data.WeeklyMissions;
        DebugSeasonMissions = data.SeasonMissions;
        DebugPreludeMissions = data.PreludeMissions;
#endif
    }

    /// <summary>
    /// Use in case you wanna clear all missions in specific scope
    /// </summary>
    public void ClearAllMissionsByScope(MissionScope missionScope)
    {
        var missionList = missionScope switch
        {
            MissionScope.Daily => data.DailyMissions,
            MissionScope.Weekly => data.WeeklyMissions,
            MissionScope.Season => data.SeasonMissions,
            _ => null,
        };
        foreach (MissionData missionData in missionList) missionData.ClearData();
        missionList.Clear();
    }

    public MissionData GetClonedPreludeMission(MissionData missionData)
    {
        if (missionData != null)
        {
            MissionData cloned = new MissionData(
                missionData.targetValue,
                missionData.targetType,
                missionData.scope,
                missionData.currencyRewardAmount,
                missionData.difficulty,
                CreateMissionProgressTracker(true, missionData.targetType, missionData.conditionParamaters),
                true,
                missionData.PreludeID
                );
            return cloned;
        }
        return null;
    }

    public MissionData GetClonedScriptedMission(MissionData missionData)
    {
        if (missionData != null)
        {
            MissionData cloned = new MissionData(
                missionData.targetValue,
                missionData.targetType,
                missionData.scope,
                missionData.currencyRewardAmount,
                missionData.difficulty,
                CreateMissionProgressTracker(true, missionData.targetType, missionData.conditionParamaters)
                );
            return cloned;
        }
        return null;
    }

#if UNITY_EDITOR
    [Button("Delete Data")]
    public override void Delete()
    {
        foreach (var mission in preludeMissions)
        {
            mission.progressUI = 0;
            mission.ResetReward();
            mission.SetProgress(0);
        }
        base.Delete();
    }

    [Button]
    void CompleteFirstMission()
    {
        var uncompletedMission = data.PreludeMissions.FindAll(x => !x.isCompleted);
        var mission = uncompletedMission.First();
        mission.SetProgress(mission.targetValue);
        GameEventHandler.Invoke(SeasonPassEventCode.UpdateSeasonUI);
    }
#endif
}
