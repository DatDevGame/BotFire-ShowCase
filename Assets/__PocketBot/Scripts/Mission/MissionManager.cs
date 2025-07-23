using System.Collections.Generic;
using DG.DemiLib;
using Sirenix.OdinInspector;
using UnityEngine;

public class MissionManager : Singleton<MissionManager>
{
    [SerializeField, BoxGroup("Data")] private MissionSavedDataSO _missionSavedData;
    [SerializeField, BoxGroup("Data")] private MissionGeneratorConfigSO _missionGeneratorConfig;

    [ShowInInspector] public MissionSavedDataSO MissionSavedDataSO => _missionSavedData;
    [ShowInInspector] public List<MissionData> dailyMissions => _missionSavedData.data.DailyMissions;
    [ShowInInspector] public List<MissionData> weeklyMissions => _missionSavedData.data.WeeklyMissions;
    [ShowInInspector] public List<MissionData> seasonMissions => _missionSavedData.data.SeasonMissions;
    [ShowInInspector] public List<MissionData> preludeMissions => _missionSavedData.data.PreludeMissions;
    [ShowInInspector] public bool isPreludeCompleted => _missionSavedData.isPreludeCompleted;

    public int GetAllMissionCompletedInSeason()
    {
        return _missionSavedData.TotalTodayMissionCompletedInSeason.value + _missionSavedData.TotalWeeklyMissionCompletedInSeason.value + _missionSavedData.TotalSeasonMissionCompletedInSeason.value;
    }

    //TODO: care priority mission list
    public MissionData GetRandomMission(MissionScope scope, int index)
    {
        Dictionary<MissionMainCategory, int> mainCategoryCount = new()
        {
            {MissionMainCategory.Main, 0},
            {MissionMainCategory.Secondary, 0},
            {MissionMainCategory.Side, 0},
        };
        HashSet<MissionSubCategory> existSubCategory = new();
        HashSet<MissionTargetType> existTargetType = new();
        List<MissionData> missions = _missionSavedData.GetMissionListByScope(scope);
        foreach (MissionData mission in missions)
        {
            MissionMainCategory mainCategory = _missionGeneratorConfig.TargetToMainCategory(mission.targetType);
            MissionSubCategory subCategory = _missionGeneratorConfig.TargetToSubCategory(mission.targetType);
            mainCategoryCount[mainCategory] = mainCategoryCount[mainCategory] + 1;
            existSubCategory.Add(subCategory);
            existTargetType.Add(mission.targetType);
        }
        
        List<MissionMainCategory> prioritizedMainCategory = new();
        List<MissionMainCategory> normalMainCategory = new();
        foreach (MissionMainCategory mainCategory in mainCategoryCount.Keys)
        {
            IntRange limit = _missionGeneratorConfig.GetMainCategoryRangeInt(scope, mainCategory);
            int count = mainCategoryCount[mainCategory];
            if (count < limit.min)
                prioritizedMainCategory.Add(mainCategory);
            if (count < limit.max)
                normalMainCategory.Add(mainCategory);
        }

        bool isPrioritizedRound = false;        
        switch (scope)
        {
            case MissionScope.Daily:
                int dailyRound = SeasonPassManager.Instance.seasonPassSO.data.todayRefreshRound;
                if (dailyRound == 1 || (dailyRound - 1) % (_missionGeneratorConfig.dailyPrioritizedSpacing + 1) == 0)
                    isPrioritizedRound = true;
                break;
            case MissionScope.Weekly:
                int weeklyCreated = _missionSavedData.data.weeklyMissionsCreatedInSeason = _missionSavedData.data.weeklyMissionsCreatedInSeason + 1;
                if (weeklyCreated == 1 || (weeklyCreated - 1) % (_missionGeneratorConfig.weeklyPrioritizedSpacing + 1) == 0)
                    isPrioritizedRound = true;
                break;
            case MissionScope.Season:
                int seasonCreated = _missionSavedData.data.seasonMissionsCreatedInSeason = _missionSavedData.data.seasonMissionsCreatedInSeason + 1;
                isPrioritizedRound = index == seasonCreated -1;
                break;
        }
        List<MissionTargetType> prioritizedTargets = null;
        Dictionary<MissionMainCategory, Dictionary<MissionSubCategory, List<MissionTargetType>>> validMainCategories = new();
        Dictionary<MissionSubCategory, List<MissionTargetType>> validSubCategories;
        if (isPrioritizedRound && index >= 0 && (prioritizedTargets = _missionGeneratorConfig.GetPrioritizedTargetList(scope, index)) != null)
        {
            for (int i = 0; i < normalMainCategory.Count; ++i)
            {
                if (ValidateMainCategoryBySubCategory(normalMainCategory[i], existSubCategory, scope, existTargetType, out validSubCategories))
                {
                    validMainCategories[normalMainCategory[i]] = validSubCategories;
                }
            }
            for (int i = 0; i < prioritizedTargets.Count; ++i)
            {
                MissionTargetType targetType = prioritizedTargets[i];
                MissionMainCategory mainCategory = _missionGeneratorConfig.TargetToMainCategory(targetType);
                MissionSubCategory subCategory = _missionGeneratorConfig.TargetToSubCategory(targetType);
                if (!validMainCategories.TryGetValue(mainCategory, out validSubCategories) ||
                    !validSubCategories.TryGetValue(subCategory, out List<MissionTargetType> validTargetTypes) ||
                    !validTargetTypes.Contains(targetType))
                {
                    prioritizedTargets.RemoveAt(i);
                    --i;
                }
            }
            if (prioritizedTargets.Count > 0)
                return CreateRandomPrioritizedMission(scope, prioritizedTargets);
        }

        for (int i = 0; i < prioritizedMainCategory.Count; ++i)
        {
            if (ValidateMainCategoryBySubCategory(prioritizedMainCategory[i], existSubCategory, scope, existTargetType, out validSubCategories))
            {
                return CreateRandomMission(scope, validSubCategories);
            }
        }
        
        for (int i = 0; i < normalMainCategory.Count; ++i)
        {
            if (ValidateMainCategoryBySubCategory(normalMainCategory[i], existSubCategory, scope, existTargetType, out validSubCategories))
            {
                validMainCategories[normalMainCategory[i]] = validSubCategories;
            }
        }

        if (validMainCategories.Keys.Count <= 0)
        {
            Debug.LogError("Can't find valid mission. use MatchWins or BattleTop or KillMaster or EarnCurrency");
            return CreateRandomMission(scope, new()
            {
                { MissionSubCategory.MatchWins, _missionGeneratorConfig.SubCategoryToTarget(MissionSubCategory.MatchWins) },
                { MissionSubCategory.BattleTop, _missionGeneratorConfig.SubCategoryToTarget(MissionSubCategory.BattleTop) },
                { MissionSubCategory.KillMaster, _missionGeneratorConfig.SubCategoryToTarget(MissionSubCategory.KillMaster) },
                { MissionSubCategory.EarnCurrency, _missionGeneratorConfig.SubCategoryToTarget(MissionSubCategory.EarnCurrency) },
            });
        }

        List<MissionCategoryRate<MissionMainCategory>> mainCategoryRates = new();
        foreach (MissionMainCategory mainCategory in validMainCategories.Keys)
        {
            mainCategoryRates.Add(_missionGeneratorConfig.GetMainCategoryRate(scope, mainCategory));
        }
        return CreateRandomMission(scope, validMainCategories[mainCategoryRates.GetRandomRedistribute().value]);


        bool ValidateMainCategoryBySubCategory(MissionMainCategory mainCategory, HashSet<MissionSubCategory> existSubCategory, MissionScope scope,
            HashSet<MissionTargetType> existTargetType, out Dictionary<MissionSubCategory, List<MissionTargetType>> validSubCategories)
        {
            validSubCategories = new();
            HashSet<MissionSubCategory> forbiddenCombo = new(){MissionSubCategory.MatchPlay, MissionSubCategory.MatchWins, MissionSubCategory.KillMaster};
            HashSet<MissionSubCategory> forbiddenComboCount = new();
            foreach(MissionSubCategory subCategory in forbiddenCombo)
                if (existSubCategory.Contains(subCategory))
                    forbiddenComboCount.Add(subCategory);
            
            foreach (MissionSubCategory subCategory in _missionGeneratorConfig.MainToSubCategory(mainCategory))
            {
                if (existSubCategory.Contains(subCategory))
                    continue;
                if (forbiddenCombo.Contains(subCategory) &&
                    !forbiddenComboCount.Contains(subCategory) &&
                    forbiddenComboCount.Count == 2)
                    continue;
                if (ValidateSubCategoryByMission(subCategory, scope, existTargetType, out List<MissionTargetType> validTargetTypes))
                    validSubCategories[subCategory] = validTargetTypes;
            }
            return validSubCategories.Count > 0;
        }

        bool ValidateSubCategoryByMission(MissionSubCategory subCategory, MissionScope scope,
            HashSet<MissionTargetType> existTargetType, out List<MissionTargetType> validTargetTypes)
        {
            validTargetTypes = new List<MissionTargetType>();
            foreach (MissionTargetType targetType in _missionGeneratorConfig.SubCategoryToTarget(subCategory))
            {
                if (existTargetType.Contains(targetType))
                    continue;
                if (!_missionGeneratorConfig.IsTargetTypeEnable(scope, targetType))
                    continue;
                if (_missionSavedData.GetMissionTargetCalculator(targetType).IsHaveValidTarget())
                    validTargetTypes.Add(targetType);
            }
            return validTargetTypes.Count > 0;
        }

        MissionData CreateRandomMission(MissionScope scope, Dictionary<MissionSubCategory, List<MissionTargetType>> subCategories)
        {
            List<MissionCategoryRate<MissionSubCategory>> subcategoryRates = new();
            foreach (MissionSubCategory subCategory in subCategories.Keys)
            {
                subcategoryRates.Add(_missionGeneratorConfig.GetSubCategoryRate(scope, subCategory));
            }
            MissionTargetType targetType = subCategories[subcategoryRates.GetRandomRedistribute().value].GetRandom();
            MissionDifficulty difficulty = _missionGeneratorConfig.GetEnabledDIfficulties(scope, targetType).GetRandom();
            return _missionSavedData.CreateMission(
                scope,
                targetType,
                difficulty,
                _missionGeneratorConfig.GetAmountFactor(targetType, difficulty),
                _missionGeneratorConfig.GetAmountMultiplier(scope),
                _missionGeneratorConfig.GetTokenAmount(scope, difficulty)
                );
        }

        MissionData CreateRandomPrioritizedMission(MissionScope scope, List<MissionTargetType> missionTargets)
        {
            MissionTargetType targetType = missionTargets.GetRandom();
            MissionDifficulty difficulty = _missionGeneratorConfig.GetEnabledDIfficulties(scope, targetType).GetRandom();
            return _missionSavedData.CreateMission(
                scope,
                targetType,
                difficulty,
                _missionGeneratorConfig.GetAmountFactor(targetType, difficulty),
                _missionGeneratorConfig.GetAmountMultiplier(scope),
                _missionGeneratorConfig.GetTokenAmount(scope, difficulty)
                ); 
        }
    }

    public List<MissionData> GetScriptedMissions(MissionScope scope)
    {
        if (!_missionSavedData.data.ScriptedRoundCount.TryGetValue(scope, out int scriptedRoundCount))
            scriptedRoundCount = 0;
        ++scriptedRoundCount;
        if (scriptedRoundCount > _missionGeneratorConfig.GetMaxScriptedRoundCount(scope))
            return null;
        _missionSavedData.data.ScriptedRoundCount[scope] = scriptedRoundCount;
        List<MissionData> configMissions = _missionGeneratorConfig.GetScriptedMissions(scope, scriptedRoundCount);
        if (configMissions == null || configMissions.Count == 0)
            return null;        
        List<MissionData> scriptedMissions = new();
        foreach(MissionData mission in configMissions)        
            if (mission.isFullyScripted)
                scriptedMissions.Add(_missionSavedData.GetClonedScriptedMission(mission));
            else if (_missionSavedData.GetMissionTargetCalculator(mission.targetType).IsHaveValidTarget())
                scriptedMissions.Add(_missionSavedData.CreateMission(
                    mission.scope,
                    mission.targetType,
                    mission.difficulty,
                    _missionGeneratorConfig.GetAmountFactor(mission.targetType, mission.difficulty),
                    _missionGeneratorConfig.GetAmountMultiplier(scope),
                    _missionGeneratorConfig.GetTokenAmount(scope, mission.difficulty)
                ));
        return scriptedMissions;
    }

    public void ResetMissionCount()
    {
        _missionSavedData.data.weeklyMissionsCreatedInSeason = 0;
        _missionSavedData.data.seasonMissionsCreatedInSeason = 0;
        _missionSavedData.TotalTodayMissionCompletedInSeason.value = 0;
        _missionSavedData.TotalWeeklyMissionCompletedInSeason.value = 0;
        _missionSavedData.TotalSeasonMissionCompletedInSeason.value = 0;
    }

    /// <summary>
    /// Add when got a new mission to track it's progress and save it's progress.
    /// </summary>
    /// <param name="missionData"></param>
    public void AddMission(MissionData mission)
    {
        if (mission == null) return;
        _missionSavedData.AddMission(mission);
    }

    /// <summary>
    /// Remove when a mission is done or swapped.
    /// </summary>
    /// <param name="missionData"></param>
    public void RemoveMission(MissionData mission)
    {
        if (mission == null) return;
        _missionSavedData.RemoveMission(mission);
    }

    public void ReplaceMission(MissionData oldMission, MissionData newMission)
    {
        if (oldMission == null || newMission == null) return;
        _missionSavedData.ReplaceMission(oldMission, newMission);
    }

    public Sprite GetMissionIcon(MissionTargetType targetType)
    {
        return _missionGeneratorConfig.GetMissionIcon(targetType);
    }

    public MissionData GetClonedPreludeMission(MissionData missionData)
    {
        return _missionSavedData.GetClonedPreludeMission(missionData);
    }

    public void ClearAllMissions()
    {
        _missionSavedData.ClearAllMissions();
    }

#if UNITY_EDITOR
    [ContextMenu("TestRefreshDailyMissions")]
    private void TestRefreshDailyMissions()
    {
        var scriptedMissions = GetScriptedMissions(MissionScope.Daily);
        SeasonPassManager.Instance.seasonPassSO.data.todayRefreshRound++;
        if (scriptedMissions == null)
        {
            for (int i = 0; i < dailyMissions.Count; ++i)
            {
                MissionData newMission = GetRandomMission(MissionScope.Daily, i);
                _missionSavedData.ReplaceMission(dailyMissions[i], newMission);
            }
        }
        else
        {
            _missionSavedData.ClearAllMissionsByScope(MissionScope.Daily);
            foreach(MissionData mission in scriptedMissions)
                AddMission(mission);
        }
    }

    [ContextMenu("TestRefreshWeeklyMissions")]
    private void TestRefreshWeeklyMissions()
    {
        var scriptedMissions = GetScriptedMissions(MissionScope.Weekly);
        if (scriptedMissions == null)
        {
            for (int i = 0; i < weeklyMissions.Count; ++i)
            {
                MissionData newMission = GetRandomMission(MissionScope.Weekly, i);
                _missionSavedData.ReplaceMission(weeklyMissions[i], newMission);
            }
        }
        else
        {
            _missionSavedData.ClearAllMissionsByScope(MissionScope.Weekly);
            foreach(MissionData mission in scriptedMissions)
                AddMission(mission);
        }
    }

    [ContextMenu("TestRefreshSeasonMissions")]
    private void TestRefreshSeasonMissions()
    {
        var scriptedMissions = GetScriptedMissions(MissionScope.Season);
        if (scriptedMissions == null)
        {
            for (int i = 0; i < seasonMissions.Count; ++i)
            {
                MissionData newMission = GetRandomMission(MissionScope.Season, i);
                _missionSavedData.ReplaceMission(seasonMissions[i], newMission);
            }
        }
        else
        {
            _missionSavedData.ClearAllMissionsByScope(MissionScope.Season);
            foreach(MissionData mission in scriptedMissions)
                AddMission(mission);
        }
    }

    [ContextMenu("TestClearAllMission")]
    private void TestClearAllMission()
    {
        _missionSavedData.ClearAllMissions();
    }
#endif
}

public enum MissionEventCode
{
    OnPlayerPickedUpCollectable,
    OnHotOfferClaimed,
}
