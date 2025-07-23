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
using DG.DemiEditor;
using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "ArenaDatasheet", menuName = "LatteGames/Editor/Datasheet/ArenaDatasheet")]
public class ArenaDatasheet : Datasheet
{
    [SerializeField, BoxGroup("Scripted Bots")]
    SheetLocation scriptedBotSheetLocation;
    [SerializeField, BoxGroup("Fighting Stages")]
    SheetLocation fightingStagesSheetLocation;
    [SerializeField, BoxGroup("Arena Info")]
    string arenaInfoGid;
    [SerializeField, BoxGroup("Arena Info")]
    string aiProfileConfigGid;
    [SerializeField, BoxGroup("Arena Info")]
    string singleMatchmakingProfileGid;
    [SerializeField, BoxGroup("Arena Info")]
    string battleMatchmakingProfileGid;
    [SerializeField, BoxGroup("Arena Info")]
    string singleDDA;
    [SerializeField, BoxGroup("Arena Info")]
    string battleDDA;
    [SerializeField, BoxGroup("Arena Info")]
    string overallScoreClampGid;
    [SerializeField, BoxGroup("Fighting Stages")]
    string fightingStagesGid;
    [SerializeField, BoxGroup("Fighting Stages")]
    string matchmakingPartsGid;
    [SerializeField, BoxGroup("Scripted Bots")]
    string singleScriptedBotGid;
    [SerializeField, BoxGroup("Scripted Bots")]
    string battleScriptedBotGid;
    [SerializeField, BoxGroup("References")]
    PBEmptyPvPArenaSO lastArenaSO;
    [SerializeField, BoxGroup("References")]
    PBPvPTournamentSO tournamentSO;
    [SerializeField, BoxGroup("References")]
    PBPartManagerSO chassisManagerSOList;
    [SerializeField, BoxGroup("References")]
    PBPartManagerSO frontManagerSOList;
    [SerializeField, BoxGroup("References")]
    PBPartManagerSO upperManagerSOList;
    [SerializeField, BoxGroup("References")]
    PBPartManagerSO wheelManagerSOList;
    [SerializeField, BoxGroup("References")]
    PBPartManagerSO specialManagerSOList;
    [SerializeField, BoxGroup("References")]
    PBPvPMatchMakingSO matchmakingSO;
    [SerializeField, BoxGroup("References"), PropertyOrder(1), InfoBox("AI Profiles have to be in ascending order of difficulty", InfoMessageType.Info), LabelText("AI Profiles without FTUE")]
    List<PB_AIProfile> aiProfiles;
    [SerializeField, BoxGroup("References"), PropertyOrder(1), InfoBox("AI Profiles have to be in ascending order of difficulty", InfoMessageType.Info)]
    List<PB_AIProfile> aiProfilesIncludeFTUE;

    [Serializable]
    public class ScriptedBotRow
    {
        private const int BodyColumnIndex = 2;
        //private const int MaxNumOfWheels = 2;
        //private const int WheelsColumnIndex = BodyColumnIndex + 1;
        private const int MaxNumOfUppers = 2;
        private const int UpperColumnIndex = BodyColumnIndex + 1;
        private const int MaxNumOfFronts = 1;
        private const int FrontColumnIndex = UpperColumnIndex + MaxNumOfUppers;
        private const int AIProfileColumnIndex = FrontColumnIndex + MaxNumOfFronts;

        public int rowIndex;
        public string body;
        //public List<string> wheelsNames = new List<string>();
        public List<string> upperNames = new List<string>();
        public List<string> frontNames = new List<string>();
        public int numOfAffectedBots;
        public string difficulty;
        public float scoreDiffMin;
        public float scoreDiffMax;

        private PB_AIProfile GetAIProfile(ArenaDatasheet arenaDatasheet)
        {
            difficulty = difficulty.Replace(" ", "");
            var difficultyType = difficulty.ToEnumOfType<DifficultyType>();
            return arenaDatasheet.aiProfiles.Find(item => item.BossType == difficultyType);
        }

        private RangeFloatValue GetScoreDiffRange()
        {
            var range = new RangeFloatValue(0f, 0f);
            range.SetFieldValue("m_MinValue", scoreDiffMin);
            range.SetFieldValue("m_MaxValue", scoreDiffMax);
            return range;
        }

        public PvPBattleTier ToPvPBattleTier(ArenaDatasheet arenaDatasheet)
        {
            var pvpTier = new PvPTier
            {
                AIProfile = GetAIProfile(arenaDatasheet),
                ScoreDiffRange = GetScoreDiffRange()
            };
            return new PvPBattleTier()
            {
                affectedBotAmount = numOfAffectedBots,
                PvPTier = pvpTier
            };
        }

        public PvPTier ToPvPTier(ArenaDatasheet arenaDatasheet)
        {
            var pvpTier = new PvPTier
            {
                AIProfile = GetAIProfile(arenaDatasheet),
                ScoreDiffRange = GetScoreDiffRange()
            };
            return pvpTier;
        }

        public ScriptedBot ToScriptedBot(ArenaDatasheet arenaDatasheet)
        {
            var scriptedBot = new ScriptedBot
            {
                chassisSO = Find<PBChassisSO>(body, arenaDatasheet.chassisManagerSOList),
                //wheel_1 = Find<PBPartSO>(GetName(wheelsNames, 0), arenaDatasheet.wheelManagerSOList),
                //wheel_2 = Find<PBPartSO>(GetName(wheelsNames, 1), arenaDatasheet.wheelManagerSOList),
                upper_1 = Find<PBPartSO>(GetName(upperNames, 0), arenaDatasheet.upperManagerSOList),
                upper_2 = Find<PBPartSO>(GetName(upperNames, 1), arenaDatasheet.upperManagerSOList),
                front = Find<PBPartSO>(GetName(frontNames, 0), arenaDatasheet.frontManagerSOList)
            };
            return scriptedBot;

            string GetName(List<string> names, int index)
            {
                if (names.IsValidIndex(index))
                    return names[index];
                return string.Empty;
            }

            T Find<T>(string partInternalName, PBPartManagerSO partManagerSO) where T : PBPartSO
            {
                if (partInternalName.IsNullOrEmpty())
                    return null;
                var partSO = partManagerSO.Find(item => item.GetInternalName() == partInternalName).Cast<T>();
                if (partSO == null)
                    throw new Exception($"Row {rowIndex} - {partInternalName} is not founded in {partManagerSO} - Name Length: {partInternalName.Length}");
                return partSO;
            }
        }

        public static ScriptedBotRow Read(CsvReader csv, ArenaDatasheet arenaDatasheet, int rowIndex, global::Mode mode)
        {
            var row = new ScriptedBotRow();
            try
            {
                if (!csv.GetField(BodyColumnIndex - 1).TrimNonASCII().ToLower().Contains("bot "))
                    return null;
                row.rowIndex = rowIndex;
                row.body = csv.GetField(BodyColumnIndex).TrimNonASCII();
                //for (int i = 0; i < MaxNumOfWheels; i++)
                //{
                //    row.wheelsNames.Add(csv.GetField(WheelsColumnIndex + i).TrimNonASCII());
                //}
                for (int i = 0; i < MaxNumOfUppers; i++)
                {
                    row.upperNames.Add(csv.GetField(UpperColumnIndex + i).TrimNonASCII());
                }
                for (int i = 0; i < MaxNumOfFronts; i++)
                {
                    row.frontNames.Add(csv.GetField(FrontColumnIndex + i).TrimNonASCII());
                }
                if (!csv.GetField(AIProfileColumnIndex).IsNullOrEmpty())
                {
                    if (mode == global::Mode.Normal)
                    {
                        row.numOfAffectedBots = 1;
                        row.difficulty = csv.GetField<string>(AIProfileColumnIndex).TrimNonASCII();
                        row.scoreDiffMin = csv.GetField<float>(AIProfileColumnIndex + 1);
                        row.scoreDiffMax = csv.GetField<float>(AIProfileColumnIndex + 2);
                    }
                    else
                    {
                        row.numOfAffectedBots = csv.GetField<int>(AIProfileColumnIndex);
                        row.difficulty = csv.GetField<string>(AIProfileColumnIndex + 1).TrimNonASCII();
                        row.scoreDiffMin = csv.GetField<float>(AIProfileColumnIndex + 2);
                        row.scoreDiffMax = csv.GetField<float>(AIProfileColumnIndex + 3);
                    }
                }
                return row;
            }
            catch (Exception exc)
            {
                Debug.LogError($"Error in row {rowIndex + 1}");
                Debug.LogException(exc);
                return null;
            }
        }
    }

    [Serializable]
    public class MatchmakingProfileRow
    {
        private const int ArenaColumnIndex = 0;
        private const int MaxScoreDiffRange = 7;
        private const int ScoreDiffRangeColumnIndex = ArenaColumnIndex + 1;
        private const int AIProfileColumnIndex = ScoreDiffRangeColumnIndex + MaxScoreDiffRange * 3;

        public int rowIndex;
        public int arenaIndex;
        public PBPvPArenaSO.MatchmakingProfileTable profileTable;

        public static MatchmakingProfileRow Read(CsvReader csv, ArenaDatasheet arenaDatasheet, int rowIndex)
        {
            var row = new MatchmakingProfileRow();
            try
            {
                row.rowIndex = rowIndex;
                if (!int.TryParse(csv.GetField(ArenaColumnIndex), out var arena))
                    return null;
                var statsScoreDiffRngInfos = new List<PBPvPArenaSO.StatsScoreDiffRngInfo>();
                var aiProfileRngInfos = new List<PBPvPArenaSO.AIProfileRngInfo>();
                row.arenaIndex = arena - 1;
                row.profileTable = new PBPvPArenaSO.MatchmakingProfileTable();
                row.profileTable.SetFieldValue("m_StatsScoreDiffRngInfos", statsScoreDiffRngInfos);
                row.profileTable.SetFieldValue("m_AIProfileRngInfos", aiProfileRngInfos);
                for (int i = ScoreDiffRangeColumnIndex; i < MaxScoreDiffRange * 3; i += 3)
                {
                    var probability = float.Parse(csv.GetField(i).TrimNonASCII().Replace("%", "").Replace(" ", "")) / 100f;
                    var min = csv.GetField<float>(i + 1);
                    var max = csv.GetField<float>(i + 2);
                    var randomRange = new RangeFloatValue(0f, 0f);
                    randomRange.SetFieldValue("m_MinValue", min);
                    randomRange.SetFieldValue("m_MaxValue", max);
                    var scoreDiffRngInfo = new PBPvPArenaSO.StatsScoreDiffRngInfo();
                    scoreDiffRngInfo.SetFieldValue("m_Probability", probability);
                    scoreDiffRngInfo.SetFieldValue("m_RandomRange", randomRange);
                    statsScoreDiffRngInfos.Add(scoreDiffRngInfo);
                }
                for (int i = AIProfileColumnIndex; i < AIProfileColumnIndex + arenaDatasheet.aiProfiles.Count; i++)
                {
                    var probability = float.Parse(csv.GetField(i).TrimNonASCII().Replace("%", "").Replace(" ", "")) / 100f;
                    var aiProfileRngInfo = new PBPvPArenaSO.AIProfileRngInfo();
                    aiProfileRngInfo.SetFieldValue("m_Probability", probability);
                    aiProfileRngInfo.SetFieldValue("m_AIProfile", arenaDatasheet.aiProfiles[i - AIProfileColumnIndex]);
                    aiProfileRngInfos.Add(aiProfileRngInfo);
                }
                return row;
            }
            catch (Exception exc)
            {
                Debug.LogError($"Error in row {rowIndex + 1}");
                Debug.LogException(exc);
                return null;
            }
        }
    }

    [Serializable]
    public class BuffInfoRow
    {
        private const int ArenaColumnIndex = 0;
        private const int BotNumberRangeColumnIndex = ArenaColumnIndex + 1;
        private const int BuffRatioColumnIndex = BotNumberRangeColumnIndex + 2;

        public int rowIndex;
        public int arenaIndex;
        public BattleBuffInfo buffInfo;

        public static BuffInfoRow Read(CsvReader csv, ArenaDatasheet arenaDatasheet, int rowIndex)
        {
            var row = new BuffInfoRow();
            try
            {
                row.rowIndex = rowIndex;
                if (!int.TryParse(csv.GetField(ArenaColumnIndex), out var arena))
                    return null;
                row.arenaIndex = arena - 1;
                var botNumbersRange = new RangeIntValue(0, 0);
                botNumbersRange.SetFieldValue("m_MinValue", csv.GetField<int>(BotNumberRangeColumnIndex));
                botNumbersRange.SetFieldValue("m_MaxValue", csv.GetField<int>(BotNumberRangeColumnIndex + 1));
                var buffRatioRange = new RangeFloatValue(0f, 0f);
                buffRatioRange.SetFieldValue("m_MinValue", csv.GetField<float>(BuffRatioColumnIndex));
                buffRatioRange.SetFieldValue("m_MaxValue", csv.GetField<float>(BuffRatioColumnIndex + 1));
                row.buffInfo = new BattleBuffInfo()
                {
                    BotNumbers = botNumbersRange,
                    BuffRatio = buffRatioRange,
                };
                return row;
            }
            catch (Exception exc)
            {
                Debug.LogError($"Error in row {rowIndex + 1}");
                Debug.LogException(exc);
                return null;
            }
        }
    }

    [Serializable]
    public class ArenaInfoRow
    {
        public const string ArenaHeader = "ARENA";
        public const string FormatHeader = "FORMAT";
        public const string PrizeHeader = "PRIZE (MONEY)";
        public const string UnlockMedalsHeader = "UNLOCK CONDITION (MEDALS)";
        public const string MedalVictoryHeader = "MEDAL VICTORY";
        public const string MedalLossHeader = "MEDAL LOSS";

        public int rowIndex;
        public int arenaIndex;
        public int numOfRounds;
        public int wonMoney;
        public float requiredMedalsToUnlock;
        public int wonMedals;
        public int lostMedals;

        public static ArenaInfoRow Read(CsvReader csv, ArenaDatasheet arenaDatasheet, int rowIndex)
        {
            var row = new ArenaInfoRow();
            try
            {
                row.rowIndex = rowIndex;
                row.arenaIndex = int.Parse(csv.GetField(ArenaHeader).Replace("ARENA ", "")) - 1;
                row.numOfRounds = int.Parse(csv.GetField(FormatHeader).Replace("BO ", ""));
                row.wonMoney = csv.GetField<int>(PrizeHeader);
                row.requiredMedalsToUnlock = csv.GetField<int>(UnlockMedalsHeader);
                row.wonMedals = csv.GetField<int>(MedalVictoryHeader);
                row.lostMedals = csv.GetField<int>(MedalLossHeader);
                return row;
            }
            catch (Exception exc)
            {
                Debug.LogError($"Error in row {rowIndex}");
                Debug.LogException(exc);
                return null;
            }
        }
    }

    [Serializable]
    public class MatchmakingPartRow
    {
        public string body;
        public string upper;
        public string front;
        public string wheel;
        public string specialBot;

        public void ReadData(CsvReader csv)
        {
            body = csv.GetField("Body");
            upper = csv.GetField("Upper");
            front = csv.GetField("Front");
            wheel = csv.GetField("Wheel");
            specialBot = csv.GetField("SPECIAL ROBOTS");
        }

        public static MatchmakingPartRow Read(CsvReader csv)
        {
            var row = new MatchmakingPartRow();
            try
            {
                row.ReadData(csv);
            }
            catch (Exception exc)
            {
                Debug.LogException(exc);
                return null;
            }
            return row;
        }
    }

    public override void ImportData()
    {
        var remoteSheetUrls = new List<string>()
        {
            remotePath.Replace("{sheetID}", arenaInfoGid),
            remotePath.Replace("{sheetID}", singleMatchmakingProfileGid),
            remotePath.Replace("{sheetID}", battleMatchmakingProfileGid),
            remotePath.Replace("{sheetID}", aiProfileConfigGid),
            remotePath.Replace("{sheetID}", singleDDA),
            remotePath.Replace("{sheetID}", battleDDA),
            remotePath.Replace("{sheetID}", overallScoreClampGid),
            scriptedBotSheetLocation.remotePath.Replace("{sheetID}", singleScriptedBotGid),
            scriptedBotSheetLocation.remotePath.Replace("{sheetID}", battleScriptedBotGid),
            fightingStagesSheetLocation.remotePath.Replace("{sheetID}", fightingStagesGid),
            fightingStagesSheetLocation.remotePath.Replace("{sheetID}", matchmakingPartsGid),
        };
        var localFilePaths = new List<string>()
        {
            localPath.Replace("{name}", "ArenaInfo"),
            localPath.Replace("{name}", "SingleMatchmakingProfile"),
            localPath.Replace("{name}", "BattleMatchmakingProfile"),
            localPath.Replace("{name}", "AIProfileConfig"),
            localPath.Replace("{name}", "SingleDDA"),
            localPath.Replace("{name}", "BattleDDA"),
            localPath.Replace("{name}", "OverallScore Clamp"),
            scriptedBotSheetLocation.localPath.Replace("{name}", "ScriptedBot_Single"),
            scriptedBotSheetLocation.localPath.Replace("{name}", "ScriptedBot_Battle"),
            fightingStagesSheetLocation.localPath.Replace("{name}", "FightingStages"),
            fightingStagesSheetLocation.localPath.Replace("{name}", "MatchmakingParts"),
        };
        RemoteDataSync.Sync(remoteSheetUrls.ToArray(), localFilePaths.ToArray(), false, OnSyncCompleted);

        void OnSyncCompleted(bool isSucceeded)
        {
            if (!isSucceeded)
            {
                EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.FailMessage, RemoteDataSync.OkMessage);
                return;
            }
            ReadAndInjectArenaInfo();
            ReadAndInjectSingleScriptedBotsData();
            ReadAndInjectBattleScriptedBotsData();
            // ReadAndInjectBattleMatchmakingProfile();
            // ReadAndInjectSingleMatchmakingProfile();
            ReadAndInjectFightingStages();
            ReadAndInjectMatchmakingParts();
            ReadAndInjectAIProfileConfigs();
            ReadAndInjectSingleDDA();
            ReadAndInjectBattleDDA();
            ReadAndInjectOverallScoreClamp();
            // Set dirty all arenas and save
            for (int i = 0; i < tournamentSO.arenas.Count; i++)
            {
                AssetDatabase.SaveAssetIfDirty(tournamentSO.arenas[i]);
            }
            AssetDatabase.SaveAssetIfDirty(lastArenaSO);
            EditorUtility.DisplayDialog(RemoteDataSync.Title, RemoteDataSync.SuccessMessage, RemoteDataSync.OkMessage);
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

    private void ReadAndInjectArenaInfo()
    {
        var filePath = localPath.Replace("{name}", "ArenaInfo");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var rowIndex = 0;
            var arenaInfoRows = new List<ArenaInfoRow>();
            csv.Read();
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                rowIndex++;
                var row = ArenaInfoRow.Read(csv, this, rowIndex);
                if (row == null)
                    continue;
                arenaInfoRows.Add(row);
            }
            InjectDataToArena();

            void InjectDataToArena()
            {
                foreach (var arenaInfoRow in arenaInfoRows)
                {
                    if (arenaInfoRow.arenaIndex > tournamentSO.arenas.Count)
                        continue;
                    var arenaSO = tournamentSO.arenas.Find(item => item.index == arenaInfoRow.arenaIndex) ?? lastArenaSO;
                    arenaSO.SetFieldValue("m_NumOfRounds", arenaInfoRow.numOfRounds);
                    if (arenaInfoRow.requiredMedalsToUnlock > 0)
                    {
                        var module = GetOrAddModule<UnlockableItemModule>(arenaSO);
                        var currencyRequirement = module.GetUnlockRequirements().Find(req => req is Requirement_Currency requirementCurrency && requirementCurrency.currencyType == CurrencyType.Medal) as Requirement_Currency;
                        currencyRequirement.SetFieldValue("m_RequiredAmountOfCurrency", arenaInfoRow.requiredMedalsToUnlock);
                    }
                    if (arenaInfoRow.wonMoney > 0 && arenaSO.TryGetReward(out CurrencyRewardModule moneyReward, reward => reward.CurrencyType == CurrencyType.Standard))
                    {
                        moneyReward.Amount = arenaInfoRow.wonMoney;
                    }
                    if (arenaInfoRow.wonMedals > 0 && arenaSO.TryGetReward(out CurrencyRewardModule medalsreward, reward => reward.CurrencyType == CurrencyType.Medal))
                    {
                        medalsreward.Amount = arenaInfoRow.wonMedals;
                    }
                    if (arenaInfoRow.lostMedals > 0 && arenaSO.TryGetPunishment(out CurrencyPunishmentModule medalsPunishment, punishment => punishment.currencyType == CurrencyType.Medal))
                    {
                        medalsPunishment.SetFieldValue("m_Amount", arenaInfoRow.lostMedals);
                    }
                    EditorUtility.SetDirty(arenaSO);
                    Debug.Log($"{arenaSO} - Update Arena Info");
                }

                T GetOrAddModule<T>(ItemSO itemSO) where T : ItemModule, new()
                {
                    if (itemSO.TryGetModule(out T module))
                    {
                        return module;
                    }
                    module = ItemModule.CreateModule<T>(itemSO);
                    itemSO.AddModule<T>(module);
                    return module;
                }
            }
        }
    }

    private void ReadAndInjectSingleScriptedBotsData()
    {
        var filePath = scriptedBotSheetLocation.localPath.Replace("{name}", "ScriptedBot_Single");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var rowIndex = -1;
            var currentArenaIndex = -1;
            var scriptedBotRowsOfEachArena = new List<List<ScriptedBotRow>>();
            while (csv.Read())
            {
                rowIndex++;
                if (csv.GetField(0).ToLower().Contains("arena"))
                {
                    currentArenaIndex++;
                    scriptedBotRowsOfEachArena.Add(new List<ScriptedBotRow>());
                }
                var row = ScriptedBotRow.Read(csv, this, rowIndex, global::Mode.Normal);
                if (row == null)
                    continue;
                scriptedBotRowsOfEachArena[currentArenaIndex].Add(row);
            }
            for (int i = 0; i < scriptedBotRowsOfEachArena.Count; i++)
            {
                var scriptedBotRows = scriptedBotRowsOfEachArena[i];
                var arenaSO = tournamentSO.arenas[i].Cast<PBPvPArenaSO>();
                InjectDataToArena(scriptedBotRows, arenaSO);
                Debug.Log($"SingleScriptedBot: Arena {i + 1} rows: {scriptedBotRows.Count}");
            }
            for (int i = scriptedBotRowsOfEachArena.Count; i < tournamentSO.arenas.Count; i++)
            {
                var arenaSO = tournamentSO.arenas[i].Cast<PBPvPArenaSO>();
                arenaSO.ScriptedBots.Clear();
                arenaSO.HardCodedInput.Tiers.Clear();
                arenaSO.SetFieldValue("numOfHardCodedMatches", 0);
                EditorUtility.SetDirty(arenaSO);
            }

            void InjectDataToArena(List<ScriptedBotRow> scriptedBotRows, PBPvPArenaSO arenaSO)
            {
                arenaSO.ScriptedBots.Clear();
                arenaSO.HardCodedInput.Tiers.Clear();
                arenaSO.SetFieldValue("numOfHardCodedMatches", scriptedBotRows.Count);
                for (int i = 0; i < scriptedBotRows.Count; i++)
                {
                    var row = scriptedBotRows[i];
                    InjectScriptedBots(row);
                    InjectAIProfile(row);
                }
                EditorUtility.SetDirty(arenaSO);

                void InjectScriptedBots(ScriptedBotRow scriptedBotRow)
                {
                    // Add to ScriptedBots only if chassis is not null or empty
                    var scriptedBot = scriptedBotRow.ToScriptedBot(this);
                    if (scriptedBot.chassisSO != null)
                        arenaSO.ScriptedBots.Add(scriptedBot);
                }
                void InjectAIProfile(ScriptedBotRow scriptedBotRow)
                {
                    if (scriptedBotRow.numOfAffectedBots != 0)
                        arenaSO.HardCodedInput.Tiers.Add(scriptedBotRow.ToPvPTier(this));
                }
            }
        }
    }

    private void ReadAndInjectBattleScriptedBotsData()
    {
        var filePath = scriptedBotSheetLocation.localPath.Replace("{name}", "ScriptedBot_Battle");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var rowIndex = -1;
            var currentArenaIndex = -1;
            var scriptedBotRowsOfEachArena = new List<List<ScriptedBotRow>>();
            while (csv.Read())
            {
                rowIndex++;
                if (csv.GetField(0).ToLower().Contains("arena"))
                {
                    currentArenaIndex++;
                    scriptedBotRowsOfEachArena.Add(new List<ScriptedBotRow>());
                }
                var row = ScriptedBotRow.Read(csv, this, rowIndex, global::Mode.Battle);
                if (row == null)
                    continue;
                scriptedBotRowsOfEachArena[currentArenaIndex].Add(row);
            }
            for (int i = 0; i < scriptedBotRowsOfEachArena.Count; i++)
            {
                var scriptedBotRows = scriptedBotRowsOfEachArena[i];
                var arenaSO = tournamentSO.arenas[i].Cast<PBPvPArenaSO>();
                InjectDataToArena(scriptedBotRows, arenaSO);
                Debug.Log($"BattleScriptedBot: Arena {i + 1} rows: {scriptedBotRows.Count}");
            }
            for (int i = scriptedBotRowsOfEachArena.Count; i < tournamentSO.arenas.Count; i++)
            {
                var arenaSO = tournamentSO.arenas[i].Cast<PBPvPArenaSO>();
                arenaSO.BattleScriptedBots.Clear();
                arenaSO.HardCodedBattleInput.Clear();
                arenaSO.SetFieldValue("numOfHardCodedMatchesBattle", 0);
                EditorUtility.SetDirty(arenaSO);
            }

            void InjectDataToArena(List<ScriptedBotRow> scriptedBotRows, PBPvPArenaSO arenaSO)
            {
                var numOfContestants = arenaSO.GetFieldValue<SerializedDictionary<global::Mode, MatchConfig>>("m_ModeConfigs").Get(global::Mode.Battle).numOfContestant;
                arenaSO.BattleScriptedBots.Clear();
                arenaSO.HardCodedBattleInput.Clear();
                arenaSO.SetFieldValue("numOfHardCodedMatchesBattle", scriptedBotRows.Count / (numOfContestants - 1));
                for (int i = 0; i < scriptedBotRows.Count; i += numOfContestants - 1)
                {
                    var fiveRows = scriptedBotRows.GetRange(i, numOfContestants - 1);
                    InjectScriptedBots(fiveRows);
                    InjectAIProfile(fiveRows);
                }
                EditorUtility.SetDirty(arenaSO);

                void InjectScriptedBots(List<ScriptedBotRow> scriptedBotRows)
                {
                    var battleScriptedBot = new BattleScriptedBot();
                    var bots = new List<ScriptedBot>();
                    foreach (var row in scriptedBotRows)
                    {
                        // Add to BattleScriptedBot only if chassis is not null or empty
                        var scriptedBot = row.ToScriptedBot(this);
                        if (scriptedBot.chassisSO != null)
                            bots.Add(scriptedBot);
                    }
                    battleScriptedBot.bots = bots.ToArray();
                    if (battleScriptedBot.bots.Length > 0)
                        arenaSO.BattleScriptedBots.Add(battleScriptedBot);
                }
                void InjectAIProfile(List<ScriptedBotRow> scriptedBotRows)
                {
                    var filterRows = scriptedBotRows.Where(row => row.numOfAffectedBots != 0).ToList();
                    var tierInput = new PvPArenaBattleTierInput()
                    {
                        Tiers = new List<PvPBattleTier>()
                    };
                    arenaSO.HardCodedBattleInput.Add(tierInput);
                    foreach (var row in filterRows)
                    {
                        tierInput.Tiers.Add(row.ToPvPBattleTier(this));
                    }
                }
            }
        }
    }

    private void ReadAndInjectBattleMatchmakingProfile()
    {
        var filePath = localPath.Replace("{name}", "BattleMatchmakingProfile");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var rowIndex = -1;
            var isBuffInfo = false;
            var profileRowIndex = -1;
            var buffInfoRows = new List<BuffInfoRow>();
            var matchmakingProfileRowArr2D = new List<List<MatchmakingProfileRow>>();
            while (csv.Read())
            {
                rowIndex++;
                if (!isBuffInfo && csv.GetField(0).TrimNonASCII().ToLower().Contains("buff info"))
                {
                    isBuffInfo = true;
                    continue;
                }
                if (!isBuffInfo)
                {
                    if (csv.GetField(0).TrimNonASCII().Contains("PvP MATCHMAKING PROFILE"))
                    {
                        profileRowIndex++;
                        matchmakingProfileRowArr2D.Add(new List<MatchmakingProfileRow>());
                        continue;
                    }
                    var row = MatchmakingProfileRow.Read(csv, this, rowIndex);
                    if (row == null)
                        continue;
                    matchmakingProfileRowArr2D[profileRowIndex].Add(row);
                }
                else
                {
                    var row = BuffInfoRow.Read(csv, this, rowIndex);
                    if (row == null)
                        continue;
                    buffInfoRows.Add(row);
                }
            }
            InjectDataToArena();

            void InjectDataToArena()
            {
                // Matchmaking profile table for battle
                for (int i = 0; i < tournamentSO.arenas.Count; i++)
                {
                    var arenaSO = tournamentSO.arenas[i].Cast<PBPvPArenaSO>();
                    var normalProfileTable = matchmakingProfileRowArr2D[0].Find(profileRow => profileRow.arenaIndex == arenaSO.index).profileTable;
                    var playerBiasProfileTable = matchmakingProfileRowArr2D[1].Find(profileRow => profileRow.arenaIndex == arenaSO.index).profileTable;
                    var opponentBiasProfileTable = matchmakingProfileRowArr2D[2].Find(profileRow => profileRow.arenaIndex == arenaSO.index).profileTable;
                    arenaSO.SetFieldValue("m_ProfileTable_Normal_Battle", normalProfileTable);
                    arenaSO.SetFieldValue("m_ProfileTable_PlayerBias_Battle", playerBiasProfileTable);
                    arenaSO.SetFieldValue("m_ProfileTable_OpponentBias_Battle", opponentBiasProfileTable);
                    EditorUtility.SetDirty(arenaSO);
                    Debug.Log($"Arena {arenaSO.index + 1} - Update PvP Matchmaking Profile Battle");
                }
                for (int i = 0; i < tournamentSO.arenas.Count; i++)
                {
                    var arenaSO = tournamentSO.arenas[i].Cast<PBPvPArenaSO>();
                    var buffInfoRow = buffInfoRows.Find(buffInfoRow => buffInfoRow.arenaIndex == arenaSO.index);
                    arenaSO.SetFieldValue("battleBuffInfo", buffInfoRow.buffInfo);
                    EditorUtility.SetDirty(arenaSO);
                    Debug.Log($"Arena {arenaSO.index + 1} - Update BuffInfo");
                }
            }
        }
    }

    private void ReadAndInjectSingleMatchmakingProfile()
    {
        var filePath = localPath.Replace("{name}", "SingleMatchmakingProfile");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var rowIndex = -1;
            var profileRowIndex = -1;
            var matchmakingProfileRowArr2D = new List<List<MatchmakingProfileRow>>();
            while (csv.Read())
            {
                rowIndex++;
                if (csv.GetField(0).TrimNonASCII().Contains("PvP MATCHMAKING PROFILE"))
                {
                    profileRowIndex++;
                    matchmakingProfileRowArr2D.Add(new List<MatchmakingProfileRow>());
                    continue;
                }
                var row = MatchmakingProfileRow.Read(csv, this, rowIndex);
                if (row == null)
                    continue;
                matchmakingProfileRowArr2D[profileRowIndex].Add(row);
            }
            InjectDataToArena();

            void InjectDataToArena()
            {
                // Matchmaking profile table for single
                for (int i = 0; i < tournamentSO.arenas.Count; i++)
                {
                    var arenaSO = tournamentSO.arenas[i].Cast<PBPvPArenaSO>();
                    var normalProfileTable = matchmakingProfileRowArr2D[0].Find(profileRow => profileRow.arenaIndex == arenaSO.index).profileTable;
                    var playerBiasProfileTable = matchmakingProfileRowArr2D[1].Find(profileRow => profileRow.arenaIndex == arenaSO.index).profileTable;
                    var opponentBiasProfileTable = matchmakingProfileRowArr2D[2].Find(profileRow => profileRow.arenaIndex == arenaSO.index).profileTable;
                    arenaSO.SetFieldValue("m_ProfileTable_Normal", normalProfileTable);
                    arenaSO.SetFieldValue("m_ProfileTable_PlayerBias", playerBiasProfileTable);
                    arenaSO.SetFieldValue("m_ProfileTable_OpponentBias", opponentBiasProfileTable);
                    EditorUtility.SetDirty(arenaSO);
                    Debug.Log($"Arena {arenaSO.index + 1} - Update PvP Matchmaking Profile Single");
                }
            }
        }
    }

    private void ReadAndInjectFightingStages()
    {
        const int StartColumnIndex = 1;
        const string DuelStagePath = "Assets/__PocketBot/PvP/Prefabs/_Stages/_DuelMode/";
        const string BattleStagePath = "Assets/__PocketBot/PvP/Prefabs/_Stages/_BattleMode/";

        var filePath = fightingStagesSheetLocation.localPath.Replace("{name}", "FightingStages");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var rowIndex = 2;
            var duelFightingStages = EditorUtils.FindAssetsOfType<GameObject>(DuelStagePath);
            var battleFightingStages = EditorUtils.FindAssetsOfType<GameObject>(BattleStagePath);
            var fightingStages = new List<GameObject>(duelFightingStages.Count + battleFightingStages.Count);
            fightingStages.AddRange(duelFightingStages);
            fightingStages.AddRange(battleFightingStages);
            foreach (PBPvPArenaSO arenaSO in tournamentSO.arenas.Cast<PBPvPArenaSO>())
            {
                foreach (var fightingStage in arenaSO.Stages)
                {
                    var scriptedStageList = fightingStage.GetFieldValue<List<PBPvPStage>>("scriptedStageList");
                    var randomStageList = fightingStage.GetFieldValue<List<PBPvPStage>>("randomStageList");
                    scriptedStageList.Clear();
                    randomStageList.Clear();
                    EditorUtility.SetDirty(arenaSO);
                }
            }
            csv.Read();
            csv.Read();
            csv.Read();
            while (csv.Read())
            {
                rowIndex++;
                try
                {
                    for (int i = StartColumnIndex; i < tournamentSO.arenas.Count * 4; i += 4)
                    {
                        var arenaSO = tournamentSO.arenas[(i - StartColumnIndex) / 4].Cast<PBPvPArenaSO>();
                        var randomStageGO = FindStageByName(csv.GetField(i));
                        var scriptedStageGO = FindStageByName(csv.GetField(i + 1));
                        var battleRandomStageGO = FindStageByName(csv.GetField(i + 2));
                        var battleScriptedStageGO = FindStageByName(csv.GetField(i + 3));
                        InjectDataToArena(scriptedStageGO, randomStageGO, arenaSO.Stages.Find(stageContainer => stageContainer.GetMode() == global::Mode.Normal));
                        InjectDataToArena(battleScriptedStageGO, battleRandomStageGO, arenaSO.Stages.Find(stageContainer => stageContainer.GetMode() == global::Mode.Battle));
                    }

                    void InjectDataToArena(GameObject scriptedStageGO, GameObject randomStageGO, StageContainer stageContainer)
                    {
                        if (scriptedStageGO != null)
                        {
                            var stage = new PBPvPStage();
                            stage.SetFieldValue("m_StagePrefab", randomStageGO);
                            stageContainer.GetFieldValue<List<PBPvPStage>>("scriptedStageList").Add(stage);
                        }
                        if (randomStageGO != null)
                        {
                            var stage = new PBPvPStage();
                            stage.SetFieldValue("m_StagePrefab", randomStageGO);
                            stageContainer.GetFieldValue<List<PBPvPStage>>("randomStageList").Add(stage);
                        }
                    }

                    GameObject FindStageByName(string name)
                    {
                        name = name.TrimNonASCII();
                        if (name.IsNullOrEmpty())
                            return null;
                        var fightingStageGO = fightingStages.Find(fightingStage => fightingStage.name == name)?.gameObject;
                        if (fightingStageGO == null)
                        {
                            throw new Exception($"Row {rowIndex + 1} - {name} is not found");
                        }
                        return fightingStageGO;
                    }
                }
                catch (Exception exc)
                {
                    Debug.LogException(exc);
                }
            }
        }
    }

    private void ReadAndInjectMatchmakingParts()
    {
        var filePath = fightingStagesSheetLocation.localPath.Replace("{name}", "MatchmakingParts");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            foreach (PBPvPArenaSO arenaSO in tournamentSO.arenas.Cast<PBPvPArenaSO>())
            {
                arenaSO.ChassisParts.Clear();
                arenaSO.FrontParts.Clear();
                arenaSO.UpperParts.Clear();
                arenaSO.WheelParts.Clear();
                arenaSO.SpecialBots.Clear();
                EditorUtility.SetDirty(arenaSO);
            }
            var rowIndex = 0;
            var currentArenaIndex = -1;
            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                rowIndex++;
                if (csv.Parser.Record.All(item => string.IsNullOrEmpty(item)))
                    continue;
                if (csv.GetField(0).Contains("ARENA "))
                {
                    currentArenaIndex++;
                }
                var row = MatchmakingPartRow.Read(csv);
                if (row == null)
                    continue;
                var currentArenaSO = tournamentSO.arenas[currentArenaIndex].Cast<PBPvPArenaSO>();
                AddPart(currentArenaSO.ChassisParts, chassisManagerSOList, row.body);
                AddPart(currentArenaSO.FrontParts, frontManagerSOList, row.front);
                AddPart(currentArenaSO.UpperParts, upperManagerSOList, row.upper);
                AddPart(currentArenaSO.WheelParts, wheelManagerSOList, row.wheel);
                AddPart(currentArenaSO.SpecialBots, specialManagerSOList, row.specialBot);
            }

            void AddPart<T>(List<T> parts, PBPartManagerSO managerSO, string name) where T : PBPartSO
            {
                if (!name.IsNullOrEmpty())
                {
                    var part = (T)managerSO.value.Find(item => item.name == name);
                    if (part != null)
                    {
                        parts.Add(part);
                    }
                    else
                    {
                        throw new Exception($"Row {rowIndex + 1} - {name} is not found in {managerSO}");
                    }
                }
            }
        }
    }

    private void ReadAndInjectAIProfileConfigs()
    {
        var filePath = localPath.Replace("{name}", "AIProfileConfig");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var rowIndex = 1;
            csv.Read();
            csv.Read();
            while (csv.Read())
            {
                rowIndex++;
                if (csv.Parser.Record.All(item => string.IsNullOrEmpty(item)))
                    continue;
                var fieldName = GetFieldName();
                var startColumnIndex = 4;
                for (int i = startColumnIndex; i < startColumnIndex + aiProfilesIncludeFTUE.Count; i++)
                {
                    aiProfilesIncludeFTUE[i - startColumnIndex].SetFieldValue(fieldName, csv.GetField<float>(i));
                }
            }
            foreach (var aiProfile in aiProfilesIncludeFTUE)
            {
                EditorUtility.SetDirty(aiProfile);
                AssetDatabase.SaveAssetIfDirty(aiProfile);
            }

            string GetFieldName()
            {
                switch (csv.GetField(2).TrimNonASCII())
                {
                    case "[Normal] Moving Speed":
                        return "movingSpeed";
                    case "[Normal] Steering Speed":
                        return "steeringSpeed";
                    case "[Attack] Moving Speed":
                        return "movingSpeedWhenAttack";
                    case "[Attack] Steering Speed":
                        return "steeringSpeedWhenAttack";
                    case "Initial_Normal":
                        return "collectBoosterProbability";
                    case "Initial_Lower":
                        return "lowerOverallscoreCollectBoosterProbability";
                    case "Low HP":
                        return "collectBoosterWhenLowHpProbability";
                    default:
                        return "movingSpeed";
                }

            }
        }
    }

    void ReadAndInjectSingleDDA()
    {
        var filePath = localPath.Replace("{name}", "SingleDDA");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var arenaIndex = -1;
            while (csv.Read())
            {
                // Ignore blank row
                if (csv.Parser.Record.All(item => string.IsNullOrEmpty(item)))
                    continue;
                if (csv.GetField(0).TrimNonASCII().Contains("ARENA"))
                {
                    arenaIndex++;
                    csv.Read();
                    csv.Read();
                    csv.Read();
                    csv.Read();
                    csv.Read();
                    // Default
                    tournamentSO.arenas[arenaIndex].Cast<PBPvPArenaSO>().MatchmakingInputArr2D_Normal.emptyConditionList = ToMatchmakingRawInput();
                    csv.Read();
                    // Lose streak
                    tournamentSO.arenas[arenaIndex].Cast<PBPvPArenaSO>().SetFieldValue("thresholdNumOfLostMatchesInColumn_Normal", csv.GetField<int>(2));
                    tournamentSO.arenas[arenaIndex].Cast<PBPvPArenaSO>().MatchmakingInputArr2D_Normal.lostConditionList = ToMatchmakingRawInput();
                    csv.Read();
                    // Part Upgraded
                    tournamentSO.arenas[arenaIndex].Cast<PBPvPArenaSO>().MatchmakingInputArr2D_Normal.upgradeConditionList = ToMatchmakingRawInput();
                    csv.Read();
                    // Win streak
                    tournamentSO.arenas[arenaIndex].Cast<PBPvPArenaSO>().SetFieldValue("thresholdNumOfWonMatchesInColumn_Normal", csv.GetField<int>(2));
                    tournamentSO.arenas[arenaIndex].Cast<PBPvPArenaSO>().MatchmakingInputArr2D_Normal.winConditionList = ToMatchmakingRawInput();

                    EditorUtility.SetDirty(tournamentSO.arenas[arenaIndex]);
                }
            }

            MatchmakingRawInput[] ToMatchmakingRawInput()
            {
                var max = 5;
                var startColumnIndex = 3;
                var rawInput = new List<MatchmakingRawInput>();
                for (int i = startColumnIndex; i < startColumnIndex + 2 * max; i += 2)
                {
                    if (csv.GetField(i).Equals("N/A"))
                        continue;
                    var scoreSourceString = csv.GetField(i).TrimNonASCII();
                    var profileSourceString = csv.GetField(i + 1).TrimNonASCII();
                    var scoreSource = scoreSourceString.Equals("Max") ? ScoreSource.MaxScore : ScoreSource.AvailableScore;
                    var profileSource = profileSourceString.Equals("Normal") ? ProfileSource.Normal : (profileSourceString.Equals("User") ? ProfileSource.UserBias : ProfileSource.OpponentBias);
                    rawInput.Add(new MatchmakingRawInput(scoreSource, profileSource));
                }
                return rawInput.ToArray();
            }
        }
    }

    void ReadAndInjectBattleDDA()
    {
        var filePath = localPath.Replace("{name}", "BattleDDA");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var arenaIndex = -1;
            while (csv.Read())
            {
                // Ignore blank row
                if (csv.Parser.Record.All(item => string.IsNullOrEmpty(item)))
                    continue;
                if (csv.GetField(0).TrimNonASCII().Contains("ARENA"))
                {
                    arenaIndex++;
                    csv.Read();
                    csv.Read();
                    csv.Read();
                    csv.Read();
                    csv.Read();
                    // Default
                    tournamentSO.arenas[arenaIndex].Cast<PBPvPArenaSO>().MatchmakingInputArr2D_Battle.emptyConditionList = ToMatchmakingRawInput();
                    csv.Read();
                    // Lose streak
                    tournamentSO.arenas[arenaIndex].Cast<PBPvPArenaSO>().SetFieldValue("thresholdNumOfLostMatchesInColumn_Battle", csv.GetField<int>(2));
                    tournamentSO.arenas[arenaIndex].Cast<PBPvPArenaSO>().MatchmakingInputArr2D_Battle.lostConditionList = ToMatchmakingRawInput();
                    csv.Read();
                    // Part Upgraded
                    tournamentSO.arenas[arenaIndex].Cast<PBPvPArenaSO>().MatchmakingInputArr2D_Battle.upgradeConditionList = ToMatchmakingRawInput();
                    csv.Read();
                    // Win streak
                    tournamentSO.arenas[arenaIndex].Cast<PBPvPArenaSO>().SetFieldValue("thresholdNumOfWonMatchesInColumn_Battle", csv.GetField<int>(2));
                    tournamentSO.arenas[arenaIndex].Cast<PBPvPArenaSO>().MatchmakingInputArr2D_Battle.winConditionList = ToMatchmakingRawInput();

                    EditorUtility.SetDirty(tournamentSO.arenas[arenaIndex]);
                }
            }

            MatchmakingRawInput[] ToMatchmakingRawInput()
            {
                var max = 5;
                var startColumnIndex = 3;
                var rawInput = new List<MatchmakingRawInput>();
                for (int i = startColumnIndex; i < startColumnIndex + 2 * max; i += 2)
                {
                    if (csv.GetField(i).Equals("N/A"))
                        continue;
                    var scoreSourceString = csv.GetField(i).TrimNonASCII();
                    var profileSourceString = csv.GetField(i + 1).TrimNonASCII();
                    var scoreSource = scoreSourceString.Equals("Max") ? ScoreSource.MaxScore : ScoreSource.AvailableScore;
                    var profileSource = profileSourceString.Equals("Normal") ? ProfileSource.Normal : (profileSourceString.Equals("User") ? ProfileSource.UserBias : ProfileSource.OpponentBias);
                    rawInput.Add(new MatchmakingRawInput(scoreSource, profileSource));
                }
                return rawInput.ToArray();
            }
        }
    }

    void ReadAndInjectOverallScoreClamp()
    {
        var filePath = localPath.Replace("{name}", "OverallScore Clamp");
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            IgnoreBlankLines = true,
            MissingFieldFound = null,
        };
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var overallScoreClamps = new List<OverallScoreClamp>();
            csv.Read();
            while (csv.Read())
            {
                // Ignore blank row
                if (csv.Parser.Record.All(item => string.IsNullOrEmpty(item)))
                    continue;
                if (csv.TryGetField(0, out int num))
                {
                    var overallScoreClamp = new OverallScoreClamp
                    {
                        triggeredTrophy = csv.GetField<int>(1),
                        scoreRange = new Vector2(csv.GetField<int>(2), csv.GetField<int>(3))
                    };
                    overallScoreClamps.Add(overallScoreClamp);
                }
            }

            matchmakingSO.OverallScoreClamps = overallScoreClamps;
            EditorUtility.SetDirty(matchmakingSO);
            AssetDatabase.SaveAssetIfDirty(matchmakingSO);
        }
    }

    public override void ExportData(string directoryPath)
    {
        Debug.LogError("Not support!");
    }

    public override void OpenRemoteURL()
    {
        base.OpenRemoteURL();
        Application.OpenURL(RemoveAfterExport(scriptedBotSheetLocation.remotePath));
        Application.OpenURL(RemoveAfterExport(fightingStagesSheetLocation.remotePath));
    }
}