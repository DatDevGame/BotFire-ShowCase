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
using Sirenix.Utilities;

public class PartDatasheet_ABTest : PartDatasheet
{
    [SerializeField]
    private ABTestPartSO abTestPartSO;
    [SerializeField]
    private List<SheetLocation> partSheetLocations;

    protected override void InjectData(PBPartSO partSO, PartData partData)
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
        Debug.Log($"Import {partSO.name} {partData.foundInArena} successfully!!!");
    }

    public override void ImportData()
    {
        var partDataList = CreatePartDataList();
        var remoteSheetUrls = new List<string>();
        var localFilePaths = new List<string>();
        for (int i = 0; i < partSheetLocations.Count; i++)
        {
            remoteSheetUrls.Add(partSheetLocations[i].remotePath.Replace("{sheetID}", m_GearDefinitionSheetID));
            remoteSheetUrls.Add(partSheetLocations[i].remotePath.Replace("{sheetID}", m_GearListSheetID));
            localFilePaths.Add(partSheetLocations[i].localPath.Replace("{name}", "GearDefinition"));
            localFilePaths.Add(partSheetLocations[i].localPath.Replace("{name}", "GearList"));
            foreach (var item in m_GearEditorDataDictionary)
            {
                remoteSheetUrls.Add(partSheetLocations[i].remotePath.Replace("{sheetID}", item.Value.sheetID));
                localFilePaths.Add(partSheetLocations[i].localPath.Replace("{name}", item.Key.name));
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
            var partSOs = new List<PBPartSO>();
            m_GearEditorDataDictionary.Keys.ForEach(partManagerSO => partSOs.AddRange(partManagerSO.Parts));
            for (int i = partSheetLocations.Count - 1; i >= 0; i--)
            {
                partDataList = CreatePartDataList();
                m_SheetLocation.SetFieldValue("m_RemotePath", partSheetLocations[i].remotePath);
                m_SheetLocation.SetFieldValue("m_LocalPath", partSheetLocations[i].localPath);
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
                    if (i == 0)
                    {
                        foreach (var partSO in partManagerSO.genericItems)
                        {
                            var partData = FindPartDataPartSO(partSO);
                            if (partData == null)
                                continue;
                            Debug.Log($"{partSO.name}: swap index {partManagerSO.genericItems.IndexOf(partSO)} -> {partData.index}");
                            partManagerSO.initialValue.Swap(partManagerSO.genericItems.IndexOf(partSO), partData.index);
                        }
                        EditorUtility.SetDirty(partManagerSO);
                    }
                }
                abTestPartSO.RetrieveData(i, partSOs);
                EditorUtility.SetDirty(abTestPartSO);
            }
            AssetDatabase.SaveAssets();
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
            var filePath = localPath.Replace("{name}", "GearDefinition");
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
            var filePath = localPath.Replace("{name}", "GearList");
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

    public override void OpenRemoteURL()
    {
        foreach (var sheetLocation in partSheetLocations)
        {
            Application.OpenURL(RemoveAfterExport(sheetLocation.remotePath));
        }
    }
}