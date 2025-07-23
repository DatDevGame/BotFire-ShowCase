using CsvHelper.Configuration;
using CsvHelper;
using LatteGames.PvP.TrophyRoad;
using LatteGames.PvP;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using DG.DemiEditor;

public class TrophyRoadDatasheet_ABTest : TrophyRoadDatasheet
{
    [SerializeField, BoxGroup("AB Test")]
    protected List<string> trophyRoadSheetIds;
    [SerializeField, BoxGroup("AB Test")]
    protected ABTestTrophyRoadSO abTestTrophyRoadSO;

    private void ReadAndInjectTrophyRoadData(int groupIndex)
    {
        if (!AssetDatabase.IsValidFolder($"Assets/__PocketBot/TrophyRoad/ScriptableObjs/Rewards/Group {groupIndex + 1}"))
        {
            AssetDatabase.CreateFolder("Assets/__PocketBot/TrophyRoad/ScriptableObjs/Rewards", $"Group {groupIndex + 1}");
        }
        var filePath = localPath.Replace("{name}", $"{LOCAL_FILE_NAME}_Group {groupIndex + 1}");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        List<ArenaData> arenaDataLs = new();
        ArenaData arenaData = null;
        ArenaData.Reward currReward = null;
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Read();
            csv.Read();
            csv.ReadHeader();
            csv.Read();
            csv.Read();

            while (csv.Read())
            {
                if (csv.TryGetField<int>(0, out var arenaIndex))
                {
                    arenaData?.Log();
                    arenaData = new()
                    {
                        ArenaIndex = arenaIndex
                    };
                    arenaDataLs.Add(arenaData);
                }
                if (csv.TryGetField<int>(2, out var requiredTrophy) && csv.TryGetField<int>(3, out var money) && csv.TryGetField<int>(4, out var gem))
                {
                    currReward = new()
                    {
                        RequiredTrophy = requiredTrophy,

                        Money = money,
                        Gem = gem
                    };
                    arenaData.RewardLs.Add(currReward);
                }
                if (csv.TryGetField<string>(5, out var partId))
                {
                    if (partId.IsNullOrEmpty()) continue;
                    (string, int) rewardItem;
                    rewardItem.Item1 = partId;
                    rewardItem.Item2 = csv.GetField<int>(6);
                    currReward.ItemLs.Add(rewardItem);
                }
            }

            _trophyRoadSO.ArenaSections.Clear();
            for (int arenaIndex = 0; arenaIndex < arenaDataLs.Count; arenaIndex++)
            {
                var currArenaData = arenaDataLs[arenaIndex];
                if (currArenaData.RewardLs.Count <= 0)
                    continue;
                var arenaSection = new TrophyRoadSO.ArenaSection();
                if (arenaIndex < _tournamentSO.arenas.Count)
                    arenaSection.arenaSO = _tournamentSO.arenas[arenaIndex];

                int rewardIndex = 1;
                foreach (var reward in currArenaData.RewardLs)
                {
                    var milesStone = new TrophyRoadSO.Milestone();
                    var productSO = new ShopProductSO();
                    productSO.name = $"TrophyRoadRewardArena{currArenaData.ArenaIndex}_{rewardIndex}";
                    rewardIndex++;
                    milesStone.requiredAmount = reward.RequiredTrophy;
                    milesStone.reward = productSO;
                    milesStone.reward.currencyItems = new();

                    if (reward.Money != 0)
                    {
                        var currencyItem = new ShopProductSO.DiscountableValue()
                        {
                            value = reward.Money,
                            originalValue = reward.Money
                        };
                        milesStone.reward.currencyItems[CurrencyType.Standard] = currencyItem;
                    }
                    if (reward.Gem != 0)
                    {
                        var currencyItem = new ShopProductSO.DiscountableValue()
                        {
                            value = reward.Gem,
                            originalValue = reward.Gem
                        };
                        milesStone.reward.currencyItems[CurrencyType.Premium] = currencyItem;
                    }
                    milesStone.reward.generalItems = new();
                    foreach (var item in reward.ItemLs)
                    {
                        var itemId = item.Item1;
                        var itemAmount = item.Item2;
                        milesStone.reward.generalItems.Add(FindRewardItemSO(itemId), new ShopProductSO.DiscountableValue()
                        {
                            value = itemAmount,
                        });
                    }

                    arenaSection.milestones.Add(milesStone);

                    EditorUtility.SetDirty(productSO);
                    AssetDatabase.CreateAsset(productSO, $"Assets/__PocketBot/TrophyRoad/ScriptableObjs/Rewards/Group {groupIndex + 1}/{productSO.name}.asset");
                    AssetDatabase.SaveAssetIfDirty(productSO);
                }

                _trophyRoadSO.ArenaSections.Add(arenaSection);
            }
        }
    }

    public override void ImportData()
    {
        var remoteSheetUrls = new List<string>();
        var localFilePaths = new List<string>();
        for (int i = 0; i < trophyRoadSheetIds.Count; i++)
        {
            remoteSheetUrls.Add(remotePath.Replace("{sheetID}", trophyRoadSheetIds[i]));
            localFilePaths.Add(localPath.Replace("{name}", $"{LOCAL_FILE_NAME}_Group {i + 1}"));
        }
        RemoteDataSync.Sync(remoteSheetUrls.ToArray(), localFilePaths.ToArray(), false, OnSyncCompleted);

        void OnSyncCompleted(bool isSucceeded)
        {
            if (!isSucceeded)
            {
                EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.FailMessage, RemoteDataSync.OkMessage);
                return;
            }

            for (int i = trophyRoadSheetIds.Count - 1; i >= 0; i--)
            {
                ReadAndInjectTrophyRoadData(i);
                abTestTrophyRoadSO.RetrieveData(i);
            }

            EditorUtility.SetDirty(_trophyRoadSO);
            AssetDatabase.SaveAssetIfDirty(_trophyRoadSO);
            EditorUtility.SetDirty(abTestTrophyRoadSO);
            AssetDatabase.SaveAssetIfDirty(abTestTrophyRoadSO);
            EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.SuccessMessage, RemoteDataSync.OkMessage);
        }
    }

    public override void OpenRemoteURL()
    {
        for (int i = 0; i < trophyRoadSheetIds.Count; i++)
        {
            Application.OpenURL(m_SheetLocation.remotePath.Replace("export?format=csv&", "edit?").Replace("{sheetID}", trophyRoadSheetIds[i]));
        }
    }
}