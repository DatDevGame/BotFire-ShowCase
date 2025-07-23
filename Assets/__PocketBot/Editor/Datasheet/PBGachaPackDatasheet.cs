using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using DG.DemiEditor;
using GachaSystem.Core;
using HyrphusQ.Helpers;
using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[Serializable]
public class PBScriptedGachaPacksGroup
{
    public List<PBScriptedGachaPacks> scriptedPacks;
}
public class PBGachaPackDatasheet : GachaPackDatasheet
{
    [SerializeField]
    List<SheetLocation> gachaBoxesSheetLocations;
    [SerializeField]
    ABTestGachaSO abTestGachaSO;
    [SerializeField, BoxGroup("Scripted Boxes")]
    PBGachaPacksList gachaPacksList;
    [SerializeField, BoxGroup("Scripted Boxes")]
    PBPvPTournamentSO tournamentSO;
    [SerializeField, BoxGroup("Scripted Boxes")]
    PBPartManagerSO chassisManagerSOList;
    [SerializeField, BoxGroup("Scripted Boxes")]
    PBPartManagerSO frontManagerSOList;
    [SerializeField, BoxGroup("Scripted Boxes")]
    PBPartManagerSO upperManagerSOList;
    [SerializeField, BoxGroup("Scripted Boxes")]
    PBPartManagerSO wheelManagerSOList;
    [SerializeField, BoxGroup("Scripted Boxes")]
    PBPartManagerSO specialManagerSOList;
    [SerializeField, BoxGroup("Scripted Boxes")]
    GachaCardTemplates cardTemplates;
    [SerializeField, BoxGroup("Scripted Boxes")]
    List<int> unlockedDurations;
    [SerializeField, BoxGroup("Scripted Boxes")]
    List<SheetLocation> scriptedBoxesSheetLocations;
    [SerializeField, BoxGroup("Scripted Boxes")]
    List<PBScriptedGachaPacksGroup> scriptedGachaBoxesGroups;
    [SerializeField, BoxGroup("Scripted Boxes")]
    SerializedDictionary<string, AbstractPack> gachaBoxModelDict;

    [Serializable]
    public class ScriptedBoxRow
    {
        public int rowIndex;
        public int index;
        public string boxType;
        public int unlockedDuration;
        public int totalCards;
        public int amountOfMoney;
        public int amountOfGem;
        public string partName;
        public int numOfCards;

        public void ReadData(CsvReader csv)
        {
            csv.TryGetField("INDEX", out index);
            boxType = csv.GetField("BOX TYPE");
            csv.TryGetField("OPEN TIMER\n(seconds)", out unlockedDuration);
            csv.TryGetField("TOTAL CARDS", out totalCards);
            csv.TryGetField("MONEY", out amountOfMoney);
            csv.TryGetField("GEM", out amountOfGem);
            partName = csv.GetField("NAME");
            numOfCards = csv.GetField<int>("QUANTITY");
        }

        public static ScriptedBoxRow Read(CsvReader csv, int rowIndex)
        {
            var row = new ScriptedBoxRow();
            try
            {
                var boxType = csv.GetField("BOX TYPE");
                var partName = csv.GetField("NAME");
                if ((boxType.IsNullOrEmpty() || boxType.TrimNonASCII().Equals("-")) && partName.IsNullOrEmpty())
                    return null;
                row.ReadData(csv);
                row.rowIndex = rowIndex;
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
                return null;
            }
            return row;
        }
    }

    private T FindPartByName<T>(string partInternalName, PBPartManagerSO partManagerSO, int rowIndex) where T : PBPartSO
    {
        if (partInternalName.IsNullOrEmpty())
            return null;
        var partSO = partManagerSO.Find(item => item.GetInternalName() == partInternalName).Cast<T>();
        if (partSO == null)
            throw new Exception($"Row {rowIndex + 1} - {partInternalName} is not founded in {partManagerSO} - Name Length: {partInternalName.Length}");
        return partSO;
    }

    private T FindPartByName<T>(string partInternalName, int rowIndex) where T : PBPartSO
    {
        if (partInternalName.IsNullOrEmpty())
            return null;
        T partSO = null;
        List<PBPartManagerSO> partManagers = new List<PBPartManagerSO>() { chassisManagerSOList, frontManagerSOList, upperManagerSOList, wheelManagerSOList, specialManagerSOList };
        foreach (var partManager in partManagers)
        {
            partSO = partManager.Find(item => item.GetInternalName() == partInternalName).Cast<T>();
            if (partSO != null)
                break;
        }
        if (partSO == null)
            throw new Exception($"Row {rowIndex + 1} - {partInternalName} is not founded - Name Length: {partInternalName.Length}");
        return partSO;
    }

    private void ReadAndInjectScriptedBoxesData(int groupIndex)
    {
        var filePath = scriptedBoxesSheetLocations[groupIndex].localPath;
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            foreach (var scriptedPack in scriptedGachaBoxesGroups[groupIndex].scriptedPacks)
            {
                scriptedPack.InvokeMethod("ClearAllProducts");
                scriptedPack.InvokeMethod("CreateFolderIfNotFound");
            }
            AssetDatabase.Refresh();
            var rowIndex = 1;
            var currentArenaIndex = 0;
            var scriptedBoxesRowsOfEachArena = new List<List<ScriptedBoxRow>>() { new List<ScriptedBoxRow>() };
            csv.Read();
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                rowIndex++;
                if (csv.Parser.Record.All(item => string.IsNullOrEmpty(item)))
                    continue;
                if (csv.GetField(0).Contains("GACHA BOX - ARENA"))
                {
                    currentArenaIndex++;
                    scriptedBoxesRowsOfEachArena.Add(new List<ScriptedBoxRow>());
                    continue;
                }
                if (csv.GetField(0).Contains("INDEX"))
                    continue;
                var row = ScriptedBoxRow.Read(csv, rowIndex);
                if (row == null)
                    continue;
                scriptedBoxesRowsOfEachArena[currentArenaIndex].Add(row);
            }
            InjectData();

            void InjectData()
            {
                for (int i = 0; i < scriptedBoxesRowsOfEachArena.Count; i++)
                {
                    var currentArena = tournamentSO.arenas[i].Cast<PBPvPArenaSO>();
                    var rows = scriptedBoxesRowsOfEachArena[i];
                    var j = 0;
                    var k = 0;
                    var totalBoxes = 0;
                    while (j < rows.Count)
                    {
                        int idx = j;
                        int totalCards = rows[j].totalCards - (rows[j].amountOfMoney != 0 ? 1 : 0) - (rows[j].amountOfGem != 0 ? 1 : 0);
                        int totalNumOfCards = 0;
                        while (totalNumOfCards < totalCards)
                        {
                            totalNumOfCards += rows[idx].numOfCards;
                            idx++;
                        }
                        int fromIdx = j;
                        int toIdx = idx;
                        j = idx;
                        totalBoxes++;
                        CreateGachaBox(rows.GetRange(fromIdx, toIdx - fromIdx), currentArena, k++);
                    }
                    var scriptedGachaBoxes = scriptedGachaBoxesGroups[groupIndex].scriptedPacks[i];
                    if (scriptedGachaBoxes != null)
                    {
                        scriptedGachaBoxes.InvokeMethod("GetProducts");
                        AssetDatabase.SaveAssetIfDirty(scriptedGachaBoxes);
                    }
                    Debug.Log($"Group {groupIndex + 1} - Scripted boxes of Arena {currentArena.index + 1}: {totalBoxes} - {scriptedBoxesRowsOfEachArena[i].Count}");
                }
            }
            void CreateGachaBox(List<ScriptedBoxRow> rows, PBPvPArenaSO arenaSO, int currentBoxIndex)
            {
                var filePath = scriptedGachaBoxesGroups[groupIndex].scriptedPacks[arenaSO.index].productsFolderPath;
                var shopProduct = CreateInstance<ShopProductSO>();
                shopProduct.name = $"ShopProductSO_ScriptedGachaPack {currentBoxIndex + 1}";
                shopProduct.currencyItems = new Dictionary<CurrencyType, ShopProductSO.DiscountableValue>();
                if (rows[0].amountOfMoney != 0)
                    shopProduct.currencyItems.Add(CurrencyType.Standard, new ShopProductSO.DiscountableValue() { value = rows[0].amountOfMoney });
                if (rows[0].amountOfGem != 0)
                    shopProduct.currencyItems.Add(CurrencyType.Premium, new ShopProductSO.DiscountableValue() { value = rows[0].amountOfGem });
                shopProduct.generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>();
                for (int i = 0; i < rows.Count; i++)
                {
                    var partSO = FindPartByName<PBPartSO>(rows[i].partName, rows[i].rowIndex);
                    shopProduct.generalItems.Add(partSO, new ShopProductSO.DiscountableValue() { value = rows[i].numOfCards });
                }
                AssetDatabase.CreateAsset(shopProduct, $"{filePath}/{shopProduct.name}.asset");

                var manualGachaPack = CreateInstance<PBManualGachaPack>();
                var simulationFromGachaPack = arenaSO.gachaPackCollection.PackRngInfos.Select(item => item.pack).FirstOrDefault(item => item.name.ToLower().Contains(rows[0].boxType.ToLower()));
                manualGachaPack.name = $"ScriptedGachaPack {currentBoxIndex + 1}";
                manualGachaPack.SetFieldValue("cardTemplates", cardTemplates);
                manualGachaPack.SetFieldValue("packPrefab", gachaBoxModelDict[rows[0].boxType]);
                manualGachaPack.SetFieldValue("unlockedDuration", (currentBoxIndex < unlockedDurations.Count && arenaSO.index <= 0) ? unlockedDurations[currentBoxIndex] : -1);
                manualGachaPack.SetFieldValue("isSpeedUpPack", currentBoxIndex < unlockedDurations.Count && arenaSO.index <= 0);
                manualGachaPack.SetFieldValue("totalCardsCount", rows[0].totalCards);
                manualGachaPack.SetFieldValue("m_ShopProductSO", shopProduct);
                manualGachaPack.SetFieldValue("simulationFromGachaPack", simulationFromGachaPack);
                var nameModule = ItemModule.CreateModule<NameItemModule>(manualGachaPack);
                nameModule.SetFieldValue("m_DisplayName", $"{CultureInfo.InvariantCulture.TextInfo.ToTitleCase(rows[0].boxType.ToLower())} Box - Arena {arenaSO.index + 1}");
                manualGachaPack.AddModule(nameModule);
                AssetDatabase.CreateAsset(manualGachaPack, $"{filePath}/{manualGachaPack.name}.asset");
                manualGachaPack.InvokeMethod("OnValidate");
                EditorUtility.SetDirty(manualGachaPack);
            }
        }
    }

    public override void ImportData()
    {
        var remoteSheetUrls = scriptedBoxesSheetLocations.Select(item => item.remotePath).ToList();
        var localFilePaths = scriptedBoxesSheetLocations.Select(item => item.localPath).ToList();
        foreach (var item in gachaPackEditorDataDictionary)
        {
            foreach (var sheetLocation in gachaBoxesSheetLocations)
            {
                remoteSheetUrls.Add(sheetLocation.remotePath.Replace("{sheetID}", item.Value.sheetID));
                localFilePaths.Add(sheetLocation.localPath.Replace("{name}", item.Key.ToString()));
            }
        }
        RemoteDataSync.Sync(remoteSheetUrls.ToArray(), localFilePaths.ToArray(), false, OnSyncCompleted);

        void OnSyncCompleted(bool isSucceeded)
        {
            if (!isSucceeded)
            {
                EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.FailMessage, RemoteDataSync.OkMessage);
                return;
            }

            // Scripted gacha boxes
            for (int i = 0; i < scriptedBoxesSheetLocations.Count; i++)
            {
                ReadAndInjectScriptedBoxesData(i);
            }
            gachaPacksList.InvokeMethod("GetProducts");

            // Gacha boxes
            for (int i = gachaBoxesSheetLocations.Count - 1; i >= 0; i--)
            {
                m_SheetLocation.SetFieldValue("m_LocalPath", gachaBoxesSheetLocations[i].localPath);
                var packInfoRows = new List<PackInfoRow>();
                var dropInfoRows = new List<DropInfoRow>();
                gearDropRateTables.Clear();
                characterDropRateTables.Clear();
                ReadDropRateData();
                ReadPackData();
                foreach (var collection in gachaPacksCollections)
                {
                    collection.PackRngInfos.Clear();
                }
                foreach (var row in packInfoRows)
                {
                    BaseGachaPack pack = GetGachaPackByRow(row);
                    InjectData(pack, row);
                    GachaPacksCollection collection = GetGachaPackCollectionByRow(row);
                    if (row.packDropRate <= 0f)
                        continue;
                    collection.PackRngInfos.Add(new GachaPacksCollection.PackRngInfo()
                    {
                        pack = pack,
                        probability = row.packDropRate
                    });
                    EditorUtility.SetDirty(collection);
                }
                EditorUtility.SetDirty(abTestGachaSO);
                abTestGachaSO.RetrieveData(i, gachaPacksCollections, gachaPacks.Cast<PBGachaPack>().ToList());

                void ReadPackData()
                {
                    var filePath = localPath.Replace("{name}", DataType.PackData.ToString());
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        IgnoreBlankLines = true,
                        MissingFieldFound = null,
                    };
                    string arenaDefine = null;
                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, config))
                    {
                        csv.Read(); //Skip first row

                        csv.Read();
                        float.TryParse(csv.GetField(PackInfoRow.GearDropRate), out gearDropRate);
                        // float.TryParse(csv.GetField(PackInfoRow.CharacterDropRate), out characterDropRate);

                        csv.Read();
                        float.TryParse(csv.GetField(PackInfoRow.NoBooster), out noBoosterProb);

                        csv.Read();
                        for (int i = PackInfoRow.SkillDropRate + 1; i <= PackInfoRow.SkillDropRate + PackInfoRow.SkillAmountRateCount; i++)
                        {
                            skillCardAmounts.Add(int.Parse(csv.GetField(i)));
                        }

                        while (csv.Read())
                        {
                            if (csv.Parser.Record.All(item => string.IsNullOrEmpty(item))) continue;
                            if (int.TryParse(csv.GetField(PackInfoRow.TotalCards), out int _) == false) continue;
                            if (string.IsNullOrEmpty(csv.GetField(PackInfoRow.ArenaLocation)) == false) arenaDefine = csv.GetField(PackInfoRow.ArenaLocation);
                            if (!int.TryParse(arenaDefine.Substring(arenaDefine.Length - 1), out int foundInArena) || foundInArena - 1 >= gachaPacksCollections.Count)
                                continue;
                            var row = PackInfoRow.Read(csv);
                            row.foundInArena = foundInArena;
                            row.define = arenaDefine;
                            row.characterData = GetCharacterDataByArena(row.foundInArena);
                            row.gearData = GetGearDataByArena(row.foundInArena);
                            if (row != null)
                            {
                                packInfoRows.Add(row);
                            }
                        }
                    }
                }
                void ReadDropRateData()
                {
                    var filePath = localPath.Replace("{name}", DataType.DropRateData.ToString());
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        IgnoreBlankLines = true,
                        MissingFieldFound = null,
                    };
                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, config))
                    {
                        csv.Read();
                        csv.ReadHeader();

                        string currentItemType = null;

                        while (csv.Read())
                        {
                            if (string.IsNullOrEmpty(csv.GetField(DropInfoRow.ItemType)) == false) currentItemType = csv.GetField(DropInfoRow.ItemType);
                            if (csv.Parser.Record.All(item => string.IsNullOrEmpty(item))) continue;
                            ReadDropData(csv);
                        }

                        void ReadDropData(CsvReader csv)
                        {
                            if (string.IsNullOrEmpty(currentItemType)) return;
                            if (currentItemType.Equals(m_CharacterLabel))
                            {
                                for (int i = 1; i <= gachaPacksCollections.Count; i++)
                                {
                                    string characterName = csv.GetField(DropInfoRow.GetArenaHeader(i));
                                    if (string.IsNullOrEmpty(characterName)) continue;
                                    var characterSO = GetCharacterSOByName(characterName);
                                    if (characterSO == null) continue;
                                    if (characterDropRateTables.Count < i) characterDropRateTables.Add(new Dictionary<GachaItemSO, float>());
                                    var prob = PercentTextToProb(csv.GetField(csv.GetFieldIndex(DropInfoRow.GetArenaHeader(i)) + 1));
                                    if (characterDropRateTables[i - 1].ContainsKey(characterSO))
                                    {
                                        characterDropRateTables[i - 1][characterSO] = prob;
                                    }
                                    else
                                    {
                                        characterDropRateTables[i - 1].Add(characterSO, prob);
                                    }
                                }
                            }
                            else if (currentItemType.Equals(m_GearLabel))
                            {
                                for (int i = 1; i <= gachaPacksCollections.Count; i++)
                                {
                                    string gearName = csv.GetField(DropInfoRow.GetArenaHeader(i));
                                    if (string.IsNullOrEmpty(gearName)) continue;
                                    var gearInfo = GetGearInfoByName(gearName);
                                    if (gearInfo == null) continue;
                                    if (gearDropRateTables.Count < i) gearDropRateTables.Add(new Dictionary<BaseGachaPack.GearInfo, float>());
                                    var prob = PercentTextToProb(csv.GetField(csv.GetFieldIndex(DropInfoRow.GetArenaHeader(i)) + 1));
                                    if (gearDropRateTables[i - 1].ContainsKey(gearInfo))
                                    {
                                        gearDropRateTables[i - 1][gearInfo] = prob;
                                    }
                                    else
                                    {
                                        gearDropRateTables[i - 1].Add(gearInfo, prob);
                                    }
                                }
                            }

                            float PercentTextToProb(string text)
                            {
                                text = text.Replace("%", string.Empty);
                                return float.Parse(text);
                            }
                        }
                    }
                }

                GachaItemSO GetCharacterSOByName(string name)
                {
                    return characterManagerSO.items.Find(character => character.GetInternalName().ToLower().Contains(name.ToLower())).Cast<GachaItemSO>();
                }

                BaseGachaPack.GearInfo GetGearInfoByName(string name)
                {
                    try
                    {
                        var gearInfo = new BaseGachaPack.GearInfo();
                        gearInfo.gearManagerSO = gearManagerSOs.
                            Find(manager => manager.items.
                            Any(gear => gear.GetInternalName().ToLower().Contains(name.ToLower())));
                        gearInfo.gearSO = gearInfo.gearManagerSO.items.
                            Find(gear => gear.GetInternalName().ToLower().Contains(name.ToLower())).Cast<GachaItemSO>();
                        return gearInfo;
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }

                BaseGachaPack GetGachaPackByRow(PackInfoRow row)
                {
                    var packs = gachaPacks.
                        FindAll(pack => pack.GetInternalName().ToLower().
                        Contains(row.define.Replace(" ", string.Empty).ToLower()));
                    return packs.Find(pack => pack.GetInternalName().ToLower().Contains(row.type.Replace(" ", string.Empty).ToLower()));
                }

                GachaPacksCollection GetGachaPackCollectionByRow(PackInfoRow row)
                {
                    var packCollection = gachaPacksCollections.Find(collection => collection.name.ToLower().Contains(row.define.Replace(" ", string.Empty).ToLower()));
                    return packCollection;
                }

                Dictionary<GachaItemSO, float> GetCharacterDataByArena(int arena)
                {
                    if (!characterDropRateTables.IsValidIndex(arena - 1))
                        return null;
                    return characterDropRateTables[arena - 1];
                }

                Dictionary<BaseGachaPack.GearInfo, float> GetGearDataByArena(int arena)
                {
                    return gearDropRateTables[arena - 1];
                }
            }
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.SuccessMessage, RemoteDataSync.OkMessage);
        }
    }

    public override void OpenRemoteURL()
    {
        foreach (var location in gachaBoxesSheetLocations)
        {
            Application.OpenURL(RemoveAfterExport(location.remotePath));
        }
        foreach (var location in scriptedBoxesSheetLocations)
        {
            Application.OpenURL(location.remotePath.Replace("export?format=csv&", "edit?"));
        }
    }

    public List<PBScriptedGachaPacksGroup> GetScriptedGachaBoxesGroups()
    {
        return scriptedGachaBoxesGroups;
    }
}