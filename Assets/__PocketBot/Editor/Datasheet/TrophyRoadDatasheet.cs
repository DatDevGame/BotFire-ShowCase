using CsvHelper.Configuration;
using CsvHelper;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using System;
using DG.DemiEditor;
using LatteGames.PvP.TrophyRoad;
using LatteGames.PvP;
using Sirenix.OdinInspector;
using GachaSystem.Core;
using HyrphusQ.SerializedDataStructure;
using HyrphusQ.Helpers;
using System.Linq;

[Serializable]
public class PBScriptedTrophyRoadBoxesGroup
{
    public List<GachaPacksList> scriptedPacks;
}
[CreateAssetMenu(fileName = "TrophyRoadDatasheet", menuName = "LatteGames/Editor/Datasheet/TrophyRoadDatasheet")]
public class TrophyRoadDatasheet : Datasheet
{
    public const string ARENA_HEADER = "ARENA";
    public const string FOUND_IN_HEADER = "FOUND IN";
    public const string MILE_STONES_HEADER = "MILESTONES";
    public const string REWARDS_HEADER = "REWARDS";
    public const string LOCAL_FILE_NAME = "TrophyRoad";

    [SerializeField, BoxGroup("Trophy Road")]
    protected TrophyRoadSO _trophyRoadSO;
    [SerializeField, BoxGroup("Trophy Road")]
    protected List<PBPartManagerSO> _partManagerLs;
    [SerializeField, BoxGroup("Trophy Road")]
    protected PBGachaPackManagerSO gachaPackManagerSO;
    [SerializeField, BoxGroup("Trophy Road")]
    protected PBPvPTournamentSO _tournamentSO;
    [SerializeField, BoxGroup("Trophy Road")]
    protected string _trophyRoadSheetId;

    //#region SCRIPTED BOXES    
    //[SerializeField, BoxGroup("Scripted Boxes")]
    //PBGachaPacksList gachaPacksList;
    //[SerializeField, BoxGroup("Scripted Boxes")]
    //PBPvPTournamentSO tournamentSO;
    //[SerializeField, BoxGroup("Scripted Boxes")]
    //PBPartManagerSO chassisManagerSOList;
    //[SerializeField, BoxGroup("Scripted Boxes")]
    //PBPartManagerSO frontManagerSOList;
    //[SerializeField, BoxGroup("Scripted Boxes")]
    //PBPartManagerSO upperManagerSOList;
    //[SerializeField, BoxGroup("Scripted Boxes")]
    //PBPartManagerSO wheelManagerSOList;
    //[SerializeField, BoxGroup("Scripted Boxes")]
    //PBPartManagerSO specialManagerSOList;
    //[SerializeField, BoxGroup("Scripted Boxes")]
    //GachaCardTemplates cardTemplates;
    //[SerializeField, BoxGroup("Scripted Boxes")]
    //List<SheetLocation> scriptedBoxesSheetLocations;
    //[SerializeField, BoxGroup("Scripted Boxes")]
    //List<PBScriptedTrophyRoadBoxesGroup> scriptedTrophyRoadBoxesGroups;
    //[SerializeField, BoxGroup("Scripted Boxes")]
    //SerializedDictionary<string, AbstractPack> gachaBoxModelDict;

    //[Serializable]
    //public class ScriptedBoxRow
    //{
    //    public int rowIndex;
    //    public int index;
    //    public string boxType;
    //    public int unlockedDuration;
    //    public int totalCards;
    //    public int amountOfMoney;
    //    public int amountOfGem;
    //    public string partName;
    //    public int numOfCards;

    //    public void ReadData(CsvReader csv)
    //    {
    //        csv.TryGetField("INDEX", out index);
    //        boxType = csv.GetField("BOX TYPE");
    //        csv.TryGetField("OPEN TIMER\n(seconds)", out unlockedDuration);
    //        csv.TryGetField("TOTAL CARDS", out totalCards);
    //        csv.TryGetField("MONEY", out amountOfMoney);
    //        csv.TryGetField("GEM", out amountOfGem);
    //        partName = csv.GetField("NAME");
    //        numOfCards = csv.GetField<int>("QUANTITY");
    //    }

    //    public static ScriptedBoxRow Read(CsvReader csv, int rowIndex)
    //    {
    //        var row = new ScriptedBoxRow();
    //        try
    //        {
    //            var boxType = csv.GetField("BOX TYPE");
    //            var partName = csv.GetField("NAME");
    //            if ((boxType.IsNullOrEmpty() || boxType.TrimNonASCII().Equals("-")) && partName.IsNullOrEmpty())
    //                return null;
    //            row.ReadData(csv);
    //            row.rowIndex = rowIndex;
    //        }
    //        catch (Exception exc)
    //        {
    //            Debug.LogException(exc);
    //            return null;
    //        }
    //        return row;
    //    }
    //}

    //private T FindPartByName<T>(string partInternalName, PBPartManagerSO partManagerSO, int rowIndex) where T : PBPartSO
    //{
    //    if (partInternalName.IsNullOrEmpty())
    //        return null;
    //    var partSO = partManagerSO.Find(item => item.GetInternalName() == partInternalName).Cast<T>();
    //    if (partSO == null)
    //        throw new Exception($"Row {rowIndex + 1} - {partInternalName} is not founded in {partManagerSO} - Name Length: {partInternalName.Length}");
    //    return partSO;
    //}

    //private T FindPartByName<T>(string partInternalName, int rowIndex) where T : PBPartSO
    //{
    //    if (partInternalName.IsNullOrEmpty())
    //        return null;
    //    T partSO = null;
    //    List<PBPartManagerSO> partManagers = new List<PBPartManagerSO>() { chassisManagerSOList, frontManagerSOList, upperManagerSOList, wheelManagerSOList, specialManagerSOList };
    //    foreach (var partManager in partManagers)
    //    {
    //        partSO = partManager.Find(item => item.GetInternalName() == partInternalName).Cast<T>();
    //        if (partSO != null)
    //            break;
    //    }
    //    if (partSO == null)
    //        throw new Exception($"Row {rowIndex + 1} - {partInternalName} is not founded - Name Length: {partInternalName.Length}");
    //    return partSO;
    //}

    //private void ReadAndInjectScriptedBoxesData(int groupIndex)
    //{
    //    var filePath = scriptedBoxesSheetLocations[groupIndex].localPath;
    //    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    //    {
    //        IgnoreBlankLines = true,
    //        MissingFieldFound = null,
    //    };
    //    using (var reader = new StreamReader(filePath))
    //    using (var csv = new CsvReader(reader, config))
    //    {
    //        foreach (var scriptedPack in scriptedTrophyRoadBoxesGroups[groupIndex].scriptedPacks)
    //        {
    //            scriptedPack.InvokeMethod("ClearAllProducts");
    //            scriptedPack.InvokeMethod("CreateFolderIfNotFound");
    //        }
    //        AssetDatabase.Refresh();
    //        var rowIndex = 1;
    //        var currentArenaIndex = 0;
    //        var scriptedBoxesRowsOfEachArena = new List<List<ScriptedBoxRow>>() { new List<ScriptedBoxRow>() };
    //        csv.Read();
    //        csv.Read();
    //        csv.ReadHeader();
    //        while (csv.Read())
    //        {
    //            rowIndex++;
    //            if (csv.Parser.Record.All(item => string.IsNullOrEmpty(item)))
    //                continue;
    //            if (csv.GetField(0).Contains("GACHA BOX - ARENA"))
    //            {
    //                currentArenaIndex++;
    //                scriptedBoxesRowsOfEachArena.Add(new List<ScriptedBoxRow>());
    //                continue;
    //            }
    //            if (csv.GetField(0).Contains("INDEX"))
    //                continue;
    //            var row = ScriptedBoxRow.Read(csv, rowIndex);
    //            if (row == null)
    //                continue;
    //            scriptedBoxesRowsOfEachArena[currentArenaIndex].Add(row);
    //        }
    //        InjectData();

    //        void InjectData()
    //        {
    //            for (int i = 0; i < scriptedBoxesRowsOfEachArena.Count; i++)
    //            {
    //                var currentArena = tournamentSO.arenas[i].Cast<PBPvPArenaSO>();
    //                var rows = scriptedBoxesRowsOfEachArena[i];
    //                var j = 0;
    //                var k = 0;
    //                var totalBoxes = 0;
    //                while (j < rows.Count)
    //                {
    //                    int idx = j;
    //                    int totalCards = rows[j].totalCards - (rows[j].amountOfMoney != 0 ? 1 : 0) - (rows[j].amountOfGem != 0 ? 1 : 0);
    //                    int totalNumOfCards = 0;
    //                    while (totalNumOfCards < totalCards)
    //                    {
    //                        totalNumOfCards += rows[idx].numOfCards;
    //                        idx++;
    //                    }
    //                    int fromIdx = j;
    //                    int toIdx = idx;
    //                    j = idx;
    //                    totalBoxes++;
    //                    CreateGachaBox(rows.GetRange(fromIdx, toIdx - fromIdx), currentArena, k++);
    //                }
    //                var scriptedGachaBoxes = scriptedTrophyRoadBoxesGroups[groupIndex].scriptedPacks[i];
    //                if (scriptedGachaBoxes != null)
    //                {
    //                    scriptedGachaBoxes.InvokeMethod("GetProducts");
    //                    AssetDatabase.SaveAssetIfDirty(scriptedGachaBoxes);
    //                }
    //                Debug.Log($"Group {groupIndex + 1} - Scripted boxes of Arena {currentArena.index + 1}: {totalBoxes} - {scriptedBoxesRowsOfEachArena[i].Count}");
    //            }
    //        }
    //        void CreateGachaBox(List<ScriptedBoxRow> rows, PBPvPArenaSO arenaSO, int currentBoxIndex)
    //        {
    //            var filePath = scriptedTrophyRoadBoxesGroups[groupIndex].scriptedPacks[arenaSO.index].productsFolderPath;
    //            var shopProduct = CreateInstance<ShopProductSO>();
    //            shopProduct.name = $"ShopProductSO_TrophyRoad_ScriptedGachaPack {currentBoxIndex + 1}";
    //            shopProduct.currencyItems = new Dictionary<CurrencyType, ShopProductSO.DiscountableValue>();
    //            if (rows[0].amountOfMoney != 0)
    //                shopProduct.currencyItems.Add(CurrencyType.Standard, new ShopProductSO.DiscountableValue() { value = rows[0].amountOfMoney });
    //            if (rows[0].amountOfGem != 0)
    //                shopProduct.currencyItems.Add(CurrencyType.Premium, new ShopProductSO.DiscountableValue() { value = rows[0].amountOfGem });
    //            shopProduct.generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>();
    //            for (int i = 0; i < rows.Count; i++)
    //            {
    //                var partSO = FindPartByName<PBPartSO>(rows[i].partName, rows[i].rowIndex);
    //                shopProduct.generalItems.Add(partSO, new ShopProductSO.DiscountableValue() { value = rows[i].numOfCards });
    //            }
    //            AssetDatabase.CreateAsset(shopProduct, $"{filePath}/{shopProduct.name}.asset");

    //            var manualGachaPack = CreateInstance<PBManualGachaPack>();
    //            var simulationFromGachaPack = arenaSO.gachaPackCollection.PackRngInfos.Select(item => item.pack).FirstOrDefault(item => item.name.ToLower().Contains(rows[0].boxType.ToLower()));
    //            manualGachaPack.name = $"TrophyRoad_ScriptedGachaPack {currentBoxIndex + 1}";
    //            manualGachaPack.SetFieldValue("cardTemplates", cardTemplates);
    //            manualGachaPack.SetFieldValue("packPrefab", gachaBoxModelDict[rows[0].boxType]);
    //            manualGachaPack.SetFieldValue("unlockedDuration", -1);
    //            manualGachaPack.SetFieldValue("isSpeedUpPack", false);
    //            manualGachaPack.SetFieldValue("totalCardsCount", rows[0].totalCards);
    //            manualGachaPack.SetFieldValue("m_ShopProductSO", shopProduct);
    //            manualGachaPack.SetFieldValue("simulationFromGachaPack", simulationFromGachaPack);
    //            var nameModule = ItemModule.CreateModule<NameItemModule>(manualGachaPack);
    //            nameModule.SetFieldValue("m_DisplayName", $"{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(rows[0].boxType.ToLower())} Box - Arena {arenaSO.index + 1}");
    //            manualGachaPack.AddModule(nameModule);
    //            AssetDatabase.CreateAsset(manualGachaPack, $"{filePath}/{manualGachaPack.name}.asset");
    //            manualGachaPack.InvokeMethod("OnValidate");
    //            EditorUtility.SetDirty(manualGachaPack);
    //        }
    //    }
    //}
    //protected virtual void ReadAndInjectScriptedBoxesData()
    //{
    //    for (int i = 0; i < scriptedBoxesSheetLocations.Count; i++)
    //    {
    //        ReadAndInjectScriptedBoxesData(i);
    //    }
    //    gachaPacksList.InvokeMethod("GetProducts");
    //}
    //#endregion

    protected PBPartSO FindPartSO(string partId)
    {
        PBPartSO partSO = null;
        foreach (var partManager in _partManagerLs)
        {
            partSO = partManager.Parts.Find(_ => _.name == partId);
            if (partSO != null) return partSO;
        }
        return null;
    }

    protected PBGachaPack FindGachaBox(string boxName)
    {
        boxName = boxName.TrimNonASCII();
        Debug.Log(boxName);
        var splitStrings = boxName.Split("_");
        var arenaIndex = int.Parse(splitStrings[1].Replace("Arena", "")) - 1;
        var boxType = (GachaPackRarity)Array.IndexOf(Enum.GetNames(typeof(GachaPackRarity)), splitStrings[2]);
        return gachaPackManagerSO.GetGachaPackByArenaIndex(boxType, arenaIndex);
    }

    protected virtual ItemSO FindRewardItemSO(string itemName)
    {
        var partSO = FindPartSO(itemName);
        if (partSO != null)
            return partSO;
        return FindGachaBox(itemName);
    }

    protected virtual void ReadAndInjectTrophyRoadData()
    {
        var filePath = localPath.Replace("{name}", LOCAL_FILE_NAME);
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
                    AssetDatabase.CreateAsset(productSO, $"Assets/__PocketBot/TrophyRoad/ScriptableObjs/Rewards/{productSO.name}.asset");
                    AssetDatabase.SaveAssetIfDirty(productSO);
                }

                _trophyRoadSO.ArenaSections.Add(arenaSection);
            }
        }
        EditorUtility.SetDirty(_trophyRoadSO);
        AssetDatabase.SaveAssetIfDirty(_trophyRoadSO);
    }

    public override void ExportData(string directoryPath)
    {
        Debug.LogError("Not support!");
    }

    public override void ImportData()
    {
        var remoteSheetUrls = new List<string>()
        {
            remotePath.Replace("{sheetID}", _trophyRoadSheetId)
        };
        var localFilePaths = new List<string>()
        {
            localPath.Replace("{name}", LOCAL_FILE_NAME)
        };
        RemoteDataSync.Sync(remoteSheetUrls.ToArray(), localFilePaths.ToArray(), false, OnSyncCompleted);

        void OnSyncCompleted(bool isSucceeded)
        {
            if (!isSucceeded)
            {
                EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.FailMessage, RemoteDataSync.OkMessage);
                return;
            }

            //ReadAndInjectScriptedBoxesData();
            ReadAndInjectTrophyRoadData();

            EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.SuccessMessage, RemoteDataSync.OkMessage);
        }
    }

    [Serializable]
    public class ArenaData
    {
        public int ArenaIndex;
        public List<string> FoundInLs = new();
        public List<Reward> RewardLs = new();

        public void Log()
        {
            Debug.Log("---------------");
            Debug.Log($"Arena {ArenaIndex}");
            foreach (var foundIn in FoundInLs)
                Debug.Log($"Part id {foundIn}");

            foreach (var reward in RewardLs)
                reward.Log();
        }
        public class Reward
        {
            public List<(string, int)> ItemLs = new();
            public int Gem;
            public int Money;
            public int RequiredTrophy;

            public void Log()
            {
                Debug.Log($"Gem {Gem} - Money {Money} - MilesStones {RequiredTrophy}");
                foreach (var item in ItemLs)
                    Debug.Log($"Item {item.Item1} - Quantity {item.Item2}");
            }
        }

    }
}
