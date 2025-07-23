using System;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CsvHelper;
using CsvHelper.Configuration;
using HyrphusQ.Helpers;

[CreateAssetMenu(fileName = "PartDatasheet", menuName = "LatteGames/Editor/Datasheet/PartDatasheet")]
public class PartDatasheet : Datasheet
{
    // General
    public const string IndexHeader = "No.";
    public const string InternalNameHeader = "Internal Name";
    public const string DisplayNameHeader = "Display Name";
    public const string GearProfileHeader = "Gear Profile";
    public const string FoundInHeader = "Found In";
    public const string RarityHeader = "Rarity";
    public const string DescriptionHeader = "Description";
    public const string TrophyThresholdHeader = "Trophy Threshold";
    // Stats
    public const string PowerHeader = "Power";
    public const string ResistanceHeader = "Resistance";
    public const string TurningHeader = "Turning";
    public const string AtkUpgradeStepHeader = "ATK Score";
    public const string HpUpgradeStepHeader = "HP Score";
    // Upgrade requirement
    public const string RequiredAmountOfMoneyHeader = "Money Upgrade Required";
    public const string RequiredNumOfCardsHeader = "Cards Upgrade Required";

    [Serializable]
    public class Row
    {
        // General
        public string internalName { get; set; }
        public string displayName { get; set; }
        public int foundInArena { get; set; }
        public RarityType rarityType { get; set; }
        // Non-Upgradable Stats
        public float power { get; set; }
        public float resistance { get; set; }
        public float turning { get; set; }
        // Upgradable Stats
        public int attack_UpgradeStep { get; set; }
        public int hp_UpgradeStep { get; set; }
        // Upgrade requirement
        public int requiredAmountOfMoneyToUpgrade { get; set; }
        public int requiredNumOfCardsToUpgrade { get; set; }

        private int ReadFoundInArena(string foundInArena)
        {
            if (string.IsNullOrEmpty(foundInArena))
                return -1;
            return int.Parse(foundInArena[^1..]);
        }

        private RarityType ReadRarityType(string rarityString)
        {
            if (string.IsNullOrEmpty(rarityString))
                return RarityType.Common;
            rarityString = rarityString.Replace(" ", string.Empty);
            if (rarityString.Equals("Starter"))
                return RarityType.Common;
            return (RarityType)Array.IndexOf(Enum.GetNames(typeof(RarityType)), rarityString);
        }

        public void ReadData(CsvReader csv)
        {
            internalName = csv.GetField(InternalNameHeader);
            displayName = csv.GetField(DisplayNameHeader);
            foundInArena = ReadFoundInArena(csv.GetField(FoundInHeader));
            rarityType = ReadRarityType(csv.GetField(RarityHeader));

            if (csv.TryGetField(PowerHeader, out string powerAsString) && !string.IsNullOrEmpty(powerAsString))
                power = float.Parse(powerAsString);
            if (csv.TryGetField(ResistanceHeader, out string resistanceAsString) && !string.IsNullOrEmpty(resistanceAsString))
                resistance = float.Parse(resistanceAsString);
            if (csv.TryGetField(TurningHeader, out string turningAsString) && !string.IsNullOrEmpty(turningAsString))
                turning = float.Parse(turningAsString);

            requiredAmountOfMoneyToUpgrade = int.Parse(csv.GetField(RequiredAmountOfMoneyHeader));
            requiredNumOfCardsToUpgrade = int.Parse(csv.GetField(RequiredNumOfCardsHeader));

            attack_UpgradeStep = int.Parse(csv.GetField(AtkUpgradeStepHeader));
            hp_UpgradeStep = int.Parse(csv.GetField(HpUpgradeStepHeader));
        }

        public void Display(CsvReader csv)
        {
            Debug.Log(csv.Parser.RawRecord);
            Debug.Log(
                $"Power: {power}\n" +
                $"Resistance: {resistance}\n" +
                $"Turning: {turning}\n" +
                $"Attack: {attack_UpgradeStep}\n" +
                $"Hp: {hp_UpgradeStep}\n");
        }

        public static Row Read(CsvReader csv)
        {
            var row = new Row();
            try
            {
                row.ReadData(csv);
            }
            catch (Exception exc)
            {
                row.Display(csv);
                Debug.LogException(exc);
                return null;
            }
            return row;
        }
    }
    [Serializable]
    public class PartData
    {
        public class ColorModuleData
        {
            public bool isTintColor;
            public Color defaultColor;
            public List<Color> colors;
        }

        // General
        public int index { get; set; }
        public int foundInArena { get; set; }
        public string internalName { get; set; }
        public string displayName { get; set; }
        public RarityType rarityType { get; set; }
        public string description { get; set; }
        public int trophyThreshold { get; set; }
        // Non-Upgradable Stats
        public float power { get; set; }
        public float resistance { get; set; }
        public float turning { get; set; }
        // Upgradable Stats
        public List<int> attack_UpgradeStep { get; set; } = new List<int>();
        public List<int> hp_UpgradeStep { get; set; } = new List<int>();
        // Upgrade requirement
        public List<float> requiredAmountOfMoneyToUpgrade { get; set; } = new List<float>();
        public List<int> requiredNumOfCardsToUpgrade { get; set; } = new List<int>();
    }

    [Serializable]
    public class GearEditorData
    {
        [field: SerializeField]
        public string sheetID { get; set; }
    }

    [SerializeField]
    protected string m_GearDefinitionSheetID;
    [SerializeField]
    protected string m_GearListSheetID;
    [SerializeField]
    protected Dictionary<PBPartManagerSO, GearEditorData> m_GearEditorDataDictionary;

    protected void TransferData(PartData partData, List<Row> rows)
    {
        if (partData == null)
            return;
        partData.displayName = rows[0].displayName;
        partData.foundInArena = rows[0].foundInArena;
        partData.rarityType = rows[0].rarityType;
        partData.power = rows[0].power;
        partData.resistance = rows[0].resistance;
        partData.turning = rows[0].turning;

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            partData.attack_UpgradeStep.Add(row.attack_UpgradeStep);
            partData.hp_UpgradeStep.Add(row.hp_UpgradeStep);

            partData.requiredAmountOfMoneyToUpgrade.Add(row.requiredAmountOfMoneyToUpgrade);
            partData.requiredNumOfCardsToUpgrade.Add(row.requiredNumOfCardsToUpgrade);
        }

        Debug.Log($"{partData.internalName} - {rows[0].internalName} - {rows.Count} rows");
    }

    protected T AddOrGetModule<T>(ItemSO itemSO) where T : ItemModule, new()
    {
        if (itemSO.TryGetModule(out T module))
            return module;
        module = ItemModule.CreateModule<T>(itemSO);
        itemSO.AddModule(module);
        return module;
    }

    protected virtual void InjectData(PBPartSO partSO, PartData partData)
    {
        partSO.foundInArena = partData.foundInArena;
        partSO.TrophyThreshold = partData.trophyThreshold;
        partSO.Stats.Power = partData.power;
        partSO.Stats.Resistance = partData.resistance;
        partSO.Stats.Turning = partData.turning;

        partSO.UpgradePath.Clear();
        partSO.UpgradePath.hpUpgradeSteps.AddRange(partData.hp_UpgradeStep);
        partSO.UpgradePath.attackUpgradeSteps.AddRange(partData.attack_UpgradeStep);

        var upgradableModule = AddOrGetModule<PBUpgradableItemModule>(partSO);
        var gachaUpgradeRequirementData = upgradableModule.GetFieldValue<PBGachaUpgradeRequirementData>("m_UpgradeRequirementData");
        var requiredAmountOfMoneyLevels = gachaUpgradeRequirementData.GetFieldValue<List<float>>("m_RequiredAmountOfCurrencyLevels");
        var requiredNumOfCardsLevels = gachaUpgradeRequirementData.GetFieldValue<List<int>>("m_RequiredNumOfCardsLevels");
        requiredAmountOfMoneyLevels.Clear();
        requiredNumOfCardsLevels.Clear();
        requiredAmountOfMoneyLevels.AddRange(partData.requiredAmountOfMoneyToUpgrade);
        requiredNumOfCardsLevels.AddRange(partData.requiredNumOfCardsToUpgrade);

        var descriptionModule = AddOrGetModule<DescriptionItemModule>(partSO);
        descriptionModule.Description = partData.description;

        var nameModule = AddOrGetModule<NameItemModule>(partSO);
        nameModule.SetFieldValue("m_DisplayName", partData.displayName);

        var rarityModule = AddOrGetModule<RarityItemModule>(partSO);
        rarityModule.SetFieldValue("m_RarityType", partData.rarityType);

        EditorUtility.SetDirty(partSO);
        AssetDatabase.SaveAssetIfDirty(partSO);
        Debug.Log($"Import {partSO.name} {partData.foundInArena} successfully!!!");
    }

    public override void ImportData()
    {
        var partDataList = CreatePartDataList();
        var remoteSheetUrls = new List<string>()
        {
            remotePath.Replace("{sheetID}", m_GearDefinitionSheetID),
            remotePath.Replace("{sheetID}", m_GearListSheetID),

        };
        var localFilePaths = new List<string>()
        {
            localPath.Replace("{name}", "GearDefinition"),
            localPath.Replace("{name}", "GearList"),
        };
        foreach (var item in m_GearEditorDataDictionary)
        {
            remoteSheetUrls.Add(remotePath.Replace("{sheetID}", item.Value.sheetID));
            localFilePaths.Add(localPath.Replace("{name}", item.Key.name));
        }
        RemoteDataSync.Sync(remoteSheetUrls.ToArray(), localFilePaths.ToArray(), false, OnSyncCompleted);

        void OnSyncCompleted(bool isSucceeded)
        {
            if (!isSucceeded)
            {
                EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.FailMessage, RemoteDataSync.OkMessage);
                return;
            }
            ReadPart();
            ReadPartDefinition();
            ReadPartList();
            foreach (var item in m_GearEditorDataDictionary)
            {
                var partManagerSO = item.Key;
                foreach (var partSO in partManagerSO.genericItems)
                {
                    var partData = FindPartDataPartSO(partSO);
                    if (partData == null)
                        continue;
                    InjectData(partSO, partData);
                }
                foreach (var partSO in partManagerSO.genericItems)
                {
                    var partData = FindPartDataPartSO(partSO);
                    if (partData == null)
                        continue;
                    Debug.Log($"{partSO.name}: swap index {partManagerSO.genericItems.IndexOf(partSO)} -> {partData.index}");
                    partManagerSO.initialValue.Swap(partManagerSO.genericItems.IndexOf(partSO), partData.index);
                }
                EditorUtility.SetDirty(partManagerSO);
                AssetDatabase.SaveAssetIfDirty(partManagerSO);
            }
            EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.SuccessMessage, RemoteDataSync.OkMessage);
        }

        List<PartData> CreatePartDataList()
        {
            var partDataList = new List<PartData>();
            foreach (var item in m_GearEditorDataDictionary)
            {
                var partManagerSO = item.Key;
                foreach (var part in partManagerSO.genericItems)
                {
                    partDataList.Add(new PartData()
                    {
                        internalName = part.GetInternalName()
                    });
                }
            }
            return partDataList;
        }

        PartData FindPartDataPartSO(ItemSO partSO)
        {
            return FindPartDataByInternalName(partSO.GetInternalName());
        }

        PartData FindPartDataByInternalName(string internalName)
        {
            return partDataList.Find(item => item.internalName == internalName);
        }

        void ReadPart()
        {
            foreach (var item in m_GearEditorDataDictionary)
            {
                List<Row> rowArr = null;
                var filePath = localPath.Replace("{name}", item.Key.name);
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
                    do
                    {
                        if (csv.Parser.Record.All(item => string.IsNullOrEmpty(item)))
                            continue;
                        if (csv.Parser.Record.Any(item => item.Equals(InternalNameHeader)))
                        {
                            if (rowArr != null)
                            {
                                TransferData(FindPartDataByInternalName(rowArr[0].internalName), rowArr);
                            }
                            rowArr = new List<Row>();
                            continue;
                        }
                        var row = Row.Read(csv);
                        if (row != null)
                        {
                            rowArr.Add(row);
                        }
                    }
                    while (csv.Read());
                    TransferData(FindPartDataByInternalName(rowArr[0].internalName), rowArr);
                }
            }
        }

        void ReadPartDefinition()
        {
            var filePath = localFilePaths[0];
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
                csv.Read();
                while (csv.Read())
                {
                    if (csv.Parser.Record.All(item => string.IsNullOrEmpty(item)))
                        continue;
                    var internalName = csv.GetField(InternalNameHeader);
                    var partData = FindPartDataByInternalName(internalName);
                    if (partData == null)
                        continue;
                    partData.description = ReadDescription(csv);
                }
            }

            string ReadDescription(CsvReader csv)
            {
                return csv.GetField(DescriptionHeader);
            }
        }

        void ReadPartList()
        {
            var filePath = localFilePaths[1];
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                IgnoreBlankLines = true,
                MissingFieldFound = null,
            };
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, config))
            {
                while (csv.Read())
                {
                    if (csv.GetField(0).Equals("INITIAL"))
                        break;
                }
                csv.Read();
                csv.ReadHeader();
                while (csv.Read())
                {
                    if (csv.Parser.Record.All(item => string.IsNullOrEmpty(item)))
                        continue;
                    var internalName = csv.GetField(InternalNameHeader);
                    var partData = FindPartDataByInternalName(internalName);
                    if (partData == null)
                        continue;
                    partData.index = ReadIndex(csv);
                    partData.trophyThreshold = ReadTrophyThreshold(csv);
                }
            }

            int ReadIndex(CsvReader csv)
            {
                return int.Parse(csv.GetField(IndexHeader));
            }
            int ReadTrophyThreshold(CsvReader csv)
            {
                return int.Parse(csv.GetField(TrophyThresholdHeader));
            }
        }
    }

    public override void ExportData(string directoryPath)
    {
        Debug.LogError("Not support!");
    }
}