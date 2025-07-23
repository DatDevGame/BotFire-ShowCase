using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using I2.Loc;
using Sirenix.OdinInspector;
using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper;

public class PBSeasonDatasheet : Datasheet
{
    public const int ColumnIndex_ID = 0;
    public const int ColumnIndex_RequiredToken = 2;
    public const int ColumnIndex_SeasonTreeTile = 3;
    public const int ColumnIndex_RewardType_Free = 3;
    public const int ColumnIndex_RewardAmount_Free = 4;
    public const int ColumnIndex_RewardType_Premium = 5;
    public const int ColumnIndex_RewardAmount_Premium = 6;
    public const int Spacing_SeasonTree = 4;

    [SerializeField] SeasonPassSO seasonPassSO;

    public override void ExportData(string directoryPath)
    {
        // Do nothing
        Debug.LogError("Not support!");
    }

    public override void ImportData()
    {
        RemoteDataSync.Sync(remotePath, localPath, false, callback: OnSyncCompleted);

        void OnSyncCompleted(bool isSucceeded)
        {
            if (!isSucceeded)
            {
                EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.FailMessage, RemoteDataSync.OkMessage);
                return;
            }

            try
            {
                ReadRewardTreeData();
                void ReadRewardTreeData()
                {
                    var filePath = localPath;
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        IgnoreBlankLines = true,
                        MissingFieldFound = null,
                    };
                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, config))
                    {
                        bool isStartRecordingMileStoneData = false;
                        bool hasCountedTree = false;
                        while (csv.Read())
                        {
                            //Counts the number of season trees in the CSV data and initializes the reward tree input data list.
                            if (!hasCountedTree)
                            {
                                seasonPassSO.rewardTreeInputDataList.Clear();
                                int treeCount = 0;
                                while (csv.GetField(ColumnIndex_SeasonTreeTile + treeCount * Spacing_SeasonTree) != null && csv.GetField(ColumnIndex_SeasonTreeTile + treeCount * Spacing_SeasonTree).Contains("Season Tree"))
                                {
                                    seasonPassSO.rewardTreeInputDataList.Add(new SeasonPassSO.RewardTreeInputData());
                                    treeCount++;
                                }
                                hasCountedTree = true;
                            }

                            if (int.TryParse(csv.GetField(ColumnIndex_ID), out int number))
                            {
                                for (var i = 0; i < seasonPassSO.rewardTreeInputDataList.Count; i++)
                                {
                                    var rewardTreeInputData = seasonPassSO.rewardTreeInputDataList[i];
                                    SeasonPassSO.MilestoneInputData milestone = new()
                                    {
                                        requiredToken = float.Parse(csv.GetField(ColumnIndex_RequiredToken)),
                                        rewardFree = new Reward
                                        {
                                            type = (RewardType)Enum.Parse(typeof(RewardType), csv.GetField(ColumnIndex_RewardType_Free + i * Spacing_SeasonTree).Replace(" ", "_").Replace("-", ""), true),
                                            amount = float.Parse(csv.GetField(ColumnIndex_RewardAmount_Free + i * Spacing_SeasonTree))
                                        },
                                        rewardPremium = new Reward
                                        {
                                            type = (RewardType)Enum.Parse(typeof(RewardType), csv.GetField(ColumnIndex_RewardType_Premium + i * Spacing_SeasonTree).Replace(" ", "_").Replace("-", ""), true),
                                            amount = float.Parse(csv.GetField(ColumnIndex_RewardAmount_Premium + i * Spacing_SeasonTree))
                                        }
                                    };
                                    rewardTreeInputData.milestoneInputDataList.Add(milestone);
                                }
                                if (!isStartRecordingMileStoneData)
                                {
                                    isStartRecordingMileStoneData = true;
                                }
                            }
                            else
                            {
                                if (isStartRecordingMileStoneData)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                EditorUtility.SetDirty(seasonPassSO);
                AssetDatabase.SaveAssetIfDirty(seasonPassSO);
                isSucceeded = true;
            }
            catch (Exception exc)
            {
                isSucceeded = false;
                Debug.LogException(exc);
            }

            if (isSucceeded)
            {
                EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.SuccessMessage, RemoteDataSync.OkMessage);
            }
            else
            {
                EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.FailMessage, RemoteDataSync.OkMessage);
            }
        }
    }
}