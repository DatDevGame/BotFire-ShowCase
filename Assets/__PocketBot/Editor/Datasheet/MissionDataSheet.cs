using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using DG.DemiLib;
using HyrphusQ.Helpers;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionDataSheet", menuName = "LatteGames/Editor/Datasheet/MissionDataSheet")]
public class MissionDataSheet : Datasheet
{
    private const string MISSION_LIST = "MissionList";
    private const string DAILY_MISSION = "DailyMission";
    private const string WEEKLY_MISSION = "WeeklyMission";
    private const string SEASON_MISSION = "SeasonMission";

    [SerializeField, BoxGroup("Sheet Id")] private string _missionListSheetId;
    [SerializeField, BoxGroup("Sheet Id")] private string _dailyMissionSheetId;
    [SerializeField, BoxGroup("Sheet Id")] private string _weeklyMissionSheetId;
    [SerializeField, BoxGroup("Sheet Id")] private string _seasonMissionSheetId;
    [SerializeField, BoxGroup("Data")] private MissionGeneratorConfigSO _missionGeneratorConfigSO;
    
    
    public override void ExportData(string directoryPath)
    {
        Debug.LogError("Not support!");
    }

    public override void ImportData()
    {
        var remoteSheetUrls = new List<string>()
        {
            remotePath.Replace("{sheetID}", _missionListSheetId),
            remotePath.Replace("{sheetID}", _dailyMissionSheetId),
            remotePath.Replace("{sheetID}", _weeklyMissionSheetId),
            remotePath.Replace("{sheetID}", _seasonMissionSheetId),
        };
        var localFilePaths = new List<string>()
        {
            localPath.Replace("{name}", MISSION_LIST),
            localPath.Replace("{name}", DAILY_MISSION),
            localPath.Replace("{name}", WEEKLY_MISSION),
            localPath.Replace("{name}", SEASON_MISSION),
        };

        RemoteDataSync.Sync(remoteSheetUrls.ToArray(), localFilePaths.ToArray(), false, OnSyncCompleted);

        void OnSyncCompleted(bool isSucceeded)
        {
            if (!isSucceeded)
            {
                EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.FailMessage, RemoteDataSync.OkMessage);
                return;
            }
            ReadAndInjectTargetTypeConfig();
            ReadAndInjectScopeConfig();
            AssetDatabase.SaveAssetIfDirty(_missionGeneratorConfigSO);
            EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.SuccessMessage, RemoteDataSync.OkMessage);
        }
    }

    private void ReadAndInjectTargetTypeConfig()
    {
        var filePath = localPath.Replace("{name}", MISSION_LIST);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var rowIndex = 6;
            List<TargetTypeConfig> rawTargetTypeConfigs = new();
            Skip(csv, 6);
            while (csv.Read())
            {
                rowIndex++;
                if (ReadLine(csv, rowIndex, out TargetTypeConfig newConfig))
                    rawTargetTypeConfigs.Add(newConfig);
            }
            Dictionary<MissionTargetType, TargetTypeConfig> targetTypeConfigs = new();
            Dictionary<MissionMainCategory, List<MissionSubCategory>> mainToSubCategory = new();
            Dictionary<MissionSubCategory, List<MissionTargetType>> subCategoryToTarget = new();
            foreach(TargetTypeConfig targetTypeConfig in rawTargetTypeConfigs)
            {
                targetTypeConfigs[targetTypeConfig.targetType] = targetTypeConfig;
                if (mainToSubCategory.TryGetValue(targetTypeConfig.mainCategory, out List<MissionSubCategory> subCategories))
                {
                    if (!subCategories.Contains(targetTypeConfig.subCategory))
                        subCategories.Add(targetTypeConfig.subCategory);
                }
                else
                    mainToSubCategory[targetTypeConfig.mainCategory] = new List<MissionSubCategory>() {targetTypeConfig.subCategory};

                if (subCategoryToTarget.TryGetValue(targetTypeConfig.subCategory, out List<MissionTargetType> targetTypes))
                {
                    if (!targetTypes.Contains(targetTypeConfig.targetType))
                        targetTypes.Add(targetTypeConfig.targetType);
                }
                else
                    subCategoryToTarget[targetTypeConfig.subCategory] = new List<MissionTargetType>() {targetTypeConfig.targetType};
            }
            _missionGeneratorConfigSO.SetFieldValue("_targetTypeConfigs", targetTypeConfigs);
            _missionGeneratorConfigSO.SetFieldValue("_mainToSubCategory", mainToSubCategory);
            _missionGeneratorConfigSO.SetFieldValue("_subCategoryToTarget", subCategoryToTarget);
            EditorUtility.SetDirty(_missionGeneratorConfigSO);
        }
    }

    private void ReadAndInjectScopeConfig()
    {
        Dictionary<MissionScope, MissionScopeConfig> scopeConfigs = new();
        scopeConfigs[MissionScope.Daily] = ReadScopeConfig(DAILY_MISSION);
        scopeConfigs[MissionScope.Weekly] = ReadScopeConfig(WEEKLY_MISSION);
        scopeConfigs[MissionScope.Season] = ReadScopeConfig(SEASON_MISSION);
        _missionGeneratorConfigSO.SetFieldValue("_scopeConfigs", scopeConfigs);
        EditorUtility.SetDirty(_missionGeneratorConfigSO);
    }

    private MissionScopeConfig ReadScopeConfig(string localFileName)
    {
        MissionScopeConfig result = new();

        var filePath = localPath.Replace("{name}", localFileName);
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            Skip(csv, 2);
            var rowIndex = 2;
            csv.Read();
            if (ReadLine(csv, rowIndex, out Dictionary<MissionDifficulty, float> tokenAmountsByDifficulty, out float amountMultiplier ))
            {
                result.SetFieldValue("_amountMultiplier", amountMultiplier);
                result.SetFieldValue("_tokenAmountsByDifficulty", tokenAmountsByDifficulty);
            }
            rowIndex++;

            Skip(csv, 4);
            rowIndex += 4;
            Dictionary<MissionMainCategory, MissionCategoryRate<MissionMainCategory>> mainCategoryRates = new();
            Dictionary<MissionMainCategory, IntRange> mainCategoryRangeInts = new();
            for ( int i = 0, length = Enum.GetValues(typeof(MissionMainCategory)).Length; i < length; i++)
            {
                csv.Read();
                if (ReadLine(csv, rowIndex, out MissionCategoryRate<MissionMainCategory> mainCategoryRate, out IntRange limit ))
                {
                    mainCategoryRates[mainCategoryRate.value] = mainCategoryRate;
                    mainCategoryRangeInts[mainCategoryRate.value] = limit;
                } 
                rowIndex++;
            }
            result.SetFieldValue("_mainCategoryRates", mainCategoryRates);
            result.SetFieldValue("_mainCategoryRangeInts", mainCategoryRangeInts);

            Skip(csv, 4);
            rowIndex += 4;
            Dictionary<MissionSubCategory, MissionCategoryRate<MissionSubCategory>> subCategoryRates = new();
            for ( int i = 0, length = Enum.GetValues(typeof(MissionSubCategory)).Length; i < length; i++)
            {
                csv.Read();
                if (ReadLine(csv, rowIndex, out MissionCategoryRate<MissionSubCategory> subCategoryRate ))
                {
                    subCategoryRates[subCategoryRate.value] = subCategoryRate;
                } 
                rowIndex++;
            }
            result.SetFieldValue("_subCategoryRates", subCategoryRates);

            Skip(csv, 4);
            rowIndex += 4;
            Dictionary<MissionTargetType, bool> isEnabled = new();
            Dictionary<MissionTargetType, Dictionary<MissionDifficulty, bool>> difficultyEnableMap = new();
            for ( int i = 0, length = Enum.GetValues(typeof(MissionTargetType)).Length; i < length; i++)
            {
                csv.Read();
                if (ReadLine(csv, rowIndex, out MissionTargetType targetType, out Dictionary<MissionDifficulty, bool> difficultyEnable, out bool isMissionEnabled ))
                {
                    isEnabled[targetType] = isMissionEnabled;
                    difficultyEnableMap[targetType] = difficultyEnable;
                } 
                rowIndex++;
            }
            result.SetFieldValue("_isEnabled", isEnabled);
            result.SetFieldValue("_difficultyEnableMap", difficultyEnableMap);
        }

        return result;
    }

    private void Skip(CsvReader csv, int lineSkip)
    {
        for (int i = 0; i < lineSkip; i++)
            csv.Read();
    }

    private bool ReadLine(CsvReader csv, int rowIndex, out TargetTypeConfig targetTypeConfig)
    {
        targetTypeConfig = new();
        try
        {
            targetTypeConfig.amountFactor = new();
            targetTypeConfig.mainCategory = Enum.Parse<MissionMainCategory>(csv.GetField(0));
            targetTypeConfig.subCategory = Enum.Parse<MissionSubCategory>(csv.GetField(1));
            targetTypeConfig.targetType = Enum.Parse<MissionTargetType>(csv.GetField(2));
            targetTypeConfig.iconID = csv.GetField(3);
            targetTypeConfig.amountFactor[MissionDifficulty.Easy] = float.Parse(csv.GetField(7));
            targetTypeConfig.amountFactor[MissionDifficulty.Medium] = float.Parse(csv.GetField(8));
            targetTypeConfig.amountFactor[MissionDifficulty.Hard] = float.Parse(csv.GetField(9));
            targetTypeConfig.amountFactor[MissionDifficulty.VeryHard] = float.Parse(csv.GetField(10));
            return true;
        }
        catch (Exception exc)
        {
            Debug.LogError($"Error in row {rowIndex + 1}");
            Debug.LogException(exc);
            return false;
        }
    }

    private bool ReadLine(CsvReader csv, int rowIndex, out Dictionary<MissionDifficulty, float> tokenAmountsByDifficulty, out float amountMultiplier)
    {
        tokenAmountsByDifficulty = new();
        amountMultiplier = 0;
        try
        {
            amountMultiplier = float.Parse(csv.GetField(0));
            tokenAmountsByDifficulty[MissionDifficulty.Easy] = float.Parse(csv.GetField(1));
            tokenAmountsByDifficulty[MissionDifficulty.Medium] = float.Parse(csv.GetField(2));
            tokenAmountsByDifficulty[MissionDifficulty.Hard] = float.Parse(csv.GetField(3));
            tokenAmountsByDifficulty[MissionDifficulty.VeryHard] = float.Parse(csv.GetField(4));
            return true;
        }
        catch (Exception exc)
        {
            Debug.LogError($"Error in row {rowIndex + 1}");
            Debug.LogException(exc);
            return false;
        }
    }

    private bool ReadLine(CsvReader csv, int rowIndex, out MissionCategoryRate<MissionMainCategory> mainCategoryRate, out IntRange limit)
    {
        mainCategoryRate = new();
        limit = new();
        try
        {
            mainCategoryRate.value = Enum.Parse<MissionMainCategory>(csv.GetField(0));
            string probabilityStr = csv.GetField(1);
            mainCategoryRate.probability = probabilityStr.Contains("%")?
                float.Parse(probabilityStr.Replace("%",string.Empty)) * 0.01f :
                float.Parse(probabilityStr);
            limit.min = int.Parse(csv.GetField(2));
            limit.max = int.Parse(csv.GetField(3));
            return true;
        }
        catch (Exception exc)
        {
            Debug.LogError($"Error in row {rowIndex + 1}");
            Debug.LogException(exc);
            return false;
        }
    }

    private bool ReadLine(CsvReader csv, int rowIndex, out MissionCategoryRate<MissionSubCategory> subCategoryRate)
    {
        subCategoryRate = new();
        try
        {
            subCategoryRate.value = Enum.Parse<MissionSubCategory>(csv.GetField(1));
            string probabilityStr = csv.GetField(2);
            subCategoryRate.probability = probabilityStr.Contains("%")?
                float.Parse(probabilityStr.Replace("%",string.Empty)) * 0.01f :
                float.Parse(probabilityStr);
            return true;
        }
        catch (Exception exc)
        {
            Debug.LogError($"Error in row {rowIndex + 1}");
            Debug.LogException(exc);
            return false;
        }
    }

    private bool ReadLine(CsvReader csv, int rowIndex, out MissionTargetType targetType, out Dictionary<MissionDifficulty, bool> difficultyEnable, out bool isMissionEnabled)
    {
        targetType = new();
        difficultyEnable = new();
        isMissionEnabled = new();
        try
        {
            targetType = Enum.Parse<MissionTargetType>(csv.GetField(2));
            isMissionEnabled = bool.Parse(csv.GetField(3));
            difficultyEnable[MissionDifficulty.Easy] = bool.Parse(csv.GetField(5));
            difficultyEnable[MissionDifficulty.Medium] = bool.Parse(csv.GetField(6));
            difficultyEnable[MissionDifficulty.Hard] = bool.Parse(csv.GetField(7));
            difficultyEnable[MissionDifficulty.VeryHard] = bool.Parse(csv.GetField(8));
            return true;
        }
        catch (Exception exc)
        {
            Debug.LogError($"Error in row {rowIndex + 1}");
            Debug.LogException(exc);
            return false;
        }
    }
}
