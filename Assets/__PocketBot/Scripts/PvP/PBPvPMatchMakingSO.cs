using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using UnityEngine;
using LatteGames.PvP;
using LatteGames.Utils;
using Sirenix.OdinInspector;
using HyrphusQ.Events;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class ManualPartStats : IPartStats
{
    #region Constructors
    public ManualPartStats(float hp, float atk, float resistance, PBPartSO partSO, int upgradeLevel, ManualPartStats sourceCopySO = null)
    {
        this.hp = hp;
        this.atk = atk;
        this.turning = partSO.GetTurning().value;
        this.resistance = resistance;
        this.partSO = ClonePartSO(partSO);
        this.partSO.RemoveModule(this.partSO.GetModule<UpgradableItemModule>());
        this.partSO.AddModule(new ManualUpgradableItemModule(upgradeLevel));
        if (sourceCopySO == null)
        {
            var skinModule = partSO.GetModule<SkinItemModule>();
            if (skinModule != null)
            {
                var randomSkin = skinModule.skins.GetRandomRedistribute();
                var manualSkinModule = new ManualSkinItemModule(randomSkin);
                manualSkinModule.currentSkinIndex = skinModule.skins.IndexOf(randomSkin);
                this.partSO.RemoveModule(this.partSO.GetModule<SkinItemModule>());
                this.partSO.AddModule(manualSkinModule);
            }
        }
        else
        {
            var skinModule = partSO.GetModule<SkinItemModule>();
            if (skinModule != null)
            {
                var sourceSkinModule = sourceCopySO.partSO.GetModule<ManualSkinItemModule>();
                var manualSkinModule = new ManualSkinItemModule(skinModule.skins[sourceSkinModule.currentSkinIndex]);

                manualSkinModule.currentSkinIndex = sourceSkinModule.currentSkinIndex;
                this.partSO.RemoveModule(this.partSO.GetModule<SkinItemModule>());
                this.partSO.AddModule(manualSkinModule);
            }
        }
    }
    #endregion

    public float hp { get; set; }
    public float atk { get; set; }
    public float turning { get; set; }
    public float resistance { get; set; }
    public PBPartSO partSO { get; set; }

    public virtual void Dispose()
    {
        UnityEngine.Object.Destroy(partSO);
        partSO = null;
    }

    protected virtual PBPartSO ClonePartSO(PBPartSO partSO)
    {
        return UnityEngine.Object.Instantiate(partSO);
    }

    #region ICharacterStats Methods
    public IStat<PBStatID, float> GetHealth()
    {
        return new PBStat<float>(PBStatID.Health, hp);
    }

    public IStat<PBStatID, float> GetAttack()
    {
        return new PBStat<float>(PBStatID.Attack, atk);
    }

    public IStat<PBStatID, float> GetStatsScore()
    {
        return new PBStat<float>(PBStatID.StatsScore, RobotStatsCalculator.CalStatsScore(hp, atk));
    }

    public IStat<PBStatID, float> GetPower()
    {
        return new PBStat<float>(PBStatID.Power, 0);
    }

    public IStat<PBStatID, float> GetResistance()
    {
        return new PBStat<float>(PBStatID.Resistance, resistance);
    }

    public IStat<PBStatID, float> GetTurning()
    {
        return new PBStat<float>(PBStatID.Turning, turning);
    }
    #endregion
}

public class ManualChassisStats : ManualPartStats
{
    #region Constructor
    public ManualChassisStats(float hp, float atk, float resistance, PBPartSO partSO, int upgradeLevel) :
        base(hp, atk, resistance, partSO, upgradeLevel)
    {

    }
    #endregion

    protected override PBPartSO ClonePartSO(PBPartSO partSO)
    {
        return partSO.Cast<PBChassisSO>().CloneChassisSO();
    }
}

[SerializeField]
public enum ScoreSource
{
    MaxScore,
    AvailableScore
}

[Serializable]
public class MatchmakingRawInputData
{
    public MatchmakingRawInput[] emptyConditionList;
    public MatchmakingRawInput[] emptyConditionListTeammate;
    public MatchmakingRawInput[] lostConditionList;
    public MatchmakingRawInput[] lostConditionListTeammate;
    public MatchmakingRawInput[] upgradeConditionList;
    public MatchmakingRawInput[] upgradeConditionListTeammate;
    public MatchmakingRawInput[] winConditionList;
    public MatchmakingRawInput[] winConditionListTeammate;
}

public enum ProfileSource
{
    Normal,
    UserBias,
    OpponentBias
}

[Serializable]
public class MatchmakingInput
{
    public MatchmakingInput(ScoreSource scoreSource, PBPvPArenaSO.MatchmakingProfileTable profileTable)
    {
        this.scoreSource = scoreSource;
        this.profileTable = profileTable;
    }

    [field: SerializeField]
    public ScoreSource scoreSource { get; set; }
    [field: SerializeField]
    public PBPvPArenaSO.MatchmakingProfileTable profileTable { get; set; }
}

[Serializable]
public class MatchmakingRawInput
{
    public MatchmakingRawInput(ScoreSource scoreSource, ProfileSource profileSource)
    {
        this.scoreSource = scoreSource;
        this.profileSource = profileSource;
    }

    [field: SerializeField]
    public ScoreSource scoreSource { get; set; }
    [field: SerializeField]
    public ProfileSource profileSource { get; set; }
}

[Serializable]
public class PvPTier
{
    public PB_AIProfile AIProfile;
    public RangeFloatValue ScoreDiffRange;
}

[Serializable]
public class PvPBattleTier
{
    public PvPTier PvPTier;
    public int affectedBotAmount;
}

[Serializable]
public class PvPArenaTierInput
{
    public List<PvPTier> Tiers;
}

[Serializable]
public class PvPArenaBattleTierInput
{
    public List<PvPBattleTier> Tiers;
    public PvPBattleTier GetPvPBattleTier(int battleBotIndex)
    {
        int cumulativeBotIndex = 0;
        foreach (var tier in Tiers)
        {
            if (battleBotIndex < cumulativeBotIndex + tier.affectedBotAmount)
            {
                return tier;
            }
            cumulativeBotIndex += tier.affectedBotAmount;
        }
        return Tiers.Last();
    }
}

[Serializable]
public struct OverallScoreClamp
{
    public float triggeredTrophy;
    public Vector2 scoreRange;
}

[Serializable]
public class ManualCardItemModule : CardItemModule
{
    public ManualCardItemModule(int numOfCards)
    {
        m_NumOfCards = numOfCards;
    }

    [SerializeField]
    private int m_NumOfCards;

    public override int numOfCards { get => m_NumOfCards; protected set => m_NumOfCards = value; }
}

[CreateAssetMenu(fileName = "PBPvPMatchmakingSO", menuName = "PocketBots/PvP/MatchmakingSO")]
public class PBPvPMatchMakingSO : PvPMatchmakingSO<PBPlayerInfo>
{
    private const string kDebugTag = nameof(PBPvPMatchMakingSO);

    protected const float HPIncreasement = 3f;
    protected const int MaxAttempt = 100;
    protected static readonly RangeValue<float> DecentStatsScoreRange = new RangeFloatValue(0.9f, 1.1f);

    [SerializeField, BoxGroup("Twist")]
    protected float m_ChooseBossForOpponentPercent = 0.1f;
    [SerializeField, BoxGroup("Twist")]
    protected float m_ChooseTransformBotForOpponentPercent = 1f;
    [SerializeField, BoxGroup("Twist")]
    protected int m_MeetTransformBotNumberOfMatch = 0;
    [SerializeField, BoxGroup("Twist")]
    protected float m_ChooseChassisIrregularlyForOpponentPercent = 0.1f;
    [SerializeField, BoxGroup("Twist")]
    protected float m_ChooseBossScoreOffsetPercent = 0.3f;
    [SerializeField, BoxGroup("Configs")]
    protected float m_SearchingDurationBattleMode;
    [SerializeField, BoxGroup("Configs")]
    protected int decreaseSkillUsageRateThreshold = 3;
    [SerializeField, BoxGroup("Configs")]
    protected float decreaseSkillUsageRateAmount = 0f;
    [SerializeField, BoxGroup("Configs")]
    protected int increaseSkillUsageRateThreshold = 5;
    [SerializeField, BoxGroup("Configs")]
    protected float increaseSkillUsageRateAmount = 1.5f;

    [SerializeField]
    protected PPrefIntVariable m_MatchConditionStarterPack;
    [SerializeField]
    protected PPrefIntVariable m_WinTheFirstConditionStarterPack;
    [SerializeField]
    protected PPrefIntVariable m_MeetTransformBotAtMatch;
    [SerializeField]
    protected IntVariable m_TotalNumOfMatches;
    [SerializeField]
    protected ModeVariable m_CurrentChosenModeVariable;
    [SerializeField]
    protected PBRobotStatsSO m_LocalPlayerRobotStatsSO;
    [SerializeField]
    protected PlayerDatabaseSO m_PlayerDatabaseSO;
    [SerializeField]
    protected IntVariable m_MatchmakingDecodingNumber;
    [SerializeField]
    protected IntVariable m_MatchmakingDecodingNumber_Battle;
    [SerializeField]
    protected PBPartManagerSO chassisManagerSO;
    [SerializeField]
    protected PBPartManagerSO upperManagerSO;
    [SerializeField]
    protected PBPartManagerSO frontManagerSO;
    [SerializeField]
    protected PBPartManagerSO wheelManagerSO;
    [SerializeField]
    protected PBPartManagerSO specialBotManagerSO;
    [SerializeField]
    protected List<OverallScoreClamp> overallScoreClamps;
    [SerializeField]
    protected int numOfHardCodedMatches;
    [SerializeField]
    protected IntVariable requiredTrophiesToUnlockActiveSkillVar;
    [SerializeField]
    protected HighestAchievedPPrefFloatTracker highestAchievedTrophyVar;
    [SerializeField]
    protected ActiveSkillManagerSO activeSkillManagerSO;
    [SerializeField]
    protected PPrefIntVariable skillUseStreakCount;
    [SerializeField]
    protected PPrefIntVariable noSkillUseStreakCount;

    [NonSerialized]
    protected PBChassisSO bestPlayerChassis;
    // [NonSerialized]
    // protected float statsScore_Player_Battle;
    [NonSerialized, HideInInspector]
    public int battleBotIndex;
    [NonSerialized]
    int buffedBotCount;
    [NonSerialized]
    List<float> battleBotScoreList = new List<float>();
    [NonSerialized]
    protected ManualPartStats m_ManualRobotChassisStatsOfOpponent;

    public List<OverallScoreClamp> OverallScoreClamps
    {
        get => overallScoreClamps;
        set => overallScoreClamps = value;
    }

    public void SetupForBattleMode(PBPvPArenaSO arenaSO, int numOfContestant)
    {
        battleBotIndex = 0;
        // statsScore_Player_Battle = FindPlayerBestChassisScore(arenaSO, true);
        battleBotScoreList.Clear();
        if (!IsInHardCodedMatch(arenaSO, out PvPTier tier))
        {
            for (var i = 0; i < numOfContestant; i++)
            {
                battleBotScoreList.Add(GetScoreDiff(arenaSO, i >= 3));
            }
            var currentMatchmakingInput = GetCurrentMatchmakingRawInput(arenaSO, true);
            if (currentMatchmakingInput.profileSource == ProfileSource.OpponentBias)
            {
                // Save first three elements
                var firstThreeElements = new List<float> {
                    battleBotScoreList[0],
                    battleBotScoreList[1],
                    battleBotScoreList[2]
                };

                // Create sublist of elements starting from index 3
                var remainingElements = battleBotScoreList.GetRange(3, battleBotScoreList.Count - 3);

                // Sort, buff and shuffle only the remaining elements
                remainingElements.Sort((p1, p2) => p2.CompareTo(p1));
                buffedBotCount = Random.Range(arenaSO.BattleBuffInfo.BotNumbers.minValue, arenaSO.BattleBuffInfo.BotNumbers.maxValue + 1);
                for (var i = 0; i < Math.Min(buffedBotCount, remainingElements.Count); i++)
                {
                    remainingElements[i] = remainingElements[i] * arenaSO.BattleBuffInfo.BuffRatio.RandomRange();
                }
                remainingElements.Shuffle();

                // Rebuild the complete list
                battleBotScoreList.Clear();
                battleBotScoreList.AddRange(firstThreeElements);
                battleBotScoreList.AddRange(remainingElements);
            }
        }
    }

#if UNITY_EDITOR
    [SerializeField, BoxGroup("Editor Only")] PBPvPArenaSO testArenaSO;
    [Button, BoxGroup("Editor Only")]
    public PBRobotStatsSO TestFindOpponent()
    {
        var attempt = 0;
        do
        {
            var robotStats = FindRobotForOpponent(testArenaSO, attempt, out float scoreDiff, true);
            attempt++;
            if (robotStats != null)
            {
                return robotStats;
            }
        }
        while (attempt < MaxAttempt);
        return default;
    }
    [Button, BoxGroup("Editor Only")]
    private void TestRandomActiveSkills(int loopCount = 1000)
    {
        Dictionary<int, int> dict = new Dictionary<int, int>();
        for (int i = 0; i < loopCount; i++)
        {
            PBRobotStatsSO robotStatsSO = TestFindOpponent();
            if (robotStatsSO != null)
            {
                dict.Set(robotStatsSO.skillInUse.value.GetNumOfCards(), dict.Get(robotStatsSO.skillInUse.value.GetNumOfCards()) + 1);
            }
        }
        var rawInput = GetCurrentMatchmakingRawInput(testArenaSO, true);
        var input = GetCurrentMatchmakingInput(testArenaSO, true);
        LGDebug.Log($"Score source: {rawInput.scoreSource} - Profile Source: {rawInput.profileSource} - SkillUsageRate: {input.profileTable.skillUsageRate * 100f}%");
        foreach (var kvp in dict)
        {
            LGDebug.Log($"{kvp.Key}: {(float)kvp.Value / loopCount * 100f}%");
        }
    }
    [OnInspectorGUI]
    protected virtual void OnInspectorGUI()
    {
        GUI.enabled = false;
        EditorGUILayout.LabelField("=============== LOG_BEGIN ===============");
        EditorGUILayout.LabelField("=============== CHASSIS ===============");
        EditorGUILayout.LabelField("=============== CHASSIS_POTENTIAL ===============");
        EditorGUILayout.LabelField("=============== PARTS ===============");
        EditorGUILayout.LabelField("=============== PARTS_POTENTIAL ===============");

        if (bestPlayerChassis != null)
        {
            EditorGUILayout.LabelField("=============== BEST PLAYER ===============");
            EditorGUILayout.LabelField("Best Player Stats");
            EditorGUILayout.ObjectField("Chassis", bestPlayerChassis, typeof(PBChassisSO), false);
        }
        if (m_ManualRobotChassisStatsOfOpponent != null)
        {
            EditorGUILayout.LabelField("=============== OPPONENT ===============");
            EditorGUILayout.LabelField("Opponent Stats");
            EditorGUILayout.FloatField("Hp", m_ManualRobotChassisStatsOfOpponent.hp);
            EditorGUILayout.FloatField("StatsScore", m_ManualRobotChassisStatsOfOpponent.GetStatsScore().value);
            EditorGUILayout.ObjectField("Chassis", m_ManualRobotChassisStatsOfOpponent.partSO, typeof(PBChassisSO), false);
            var rawInput = GetCurrentMatchmakingRawInput(testArenaSO, true);
            var input = GetCurrentMatchmakingInput(testArenaSO, true);
            EditorGUILayout.LabelField($"Score source: {rawInput.scoreSource} - Profile Source: {rawInput.profileSource} - SkillUsageRate: {input.profileTable.skillUsageRate * 100f}%");
        }
        EditorGUILayout.LabelField("=============== LOG_END ===============");
        GUI.enabled = true;

        void LogInspector_1(PBPartSO partSO)
        {
            EditorGUILayout.LabelField($"Upgrade Level [{partSO}]: [{partSO.GetCurrentUpgradeLevel()}] - [{partSO.CalMaxReachableUpgradeLevel()}]");
            EditorGUILayout.LabelField($"Stats Score [{partSO}]: [{partSO.CalCurrentStatsScore()}] - [{partSO.CalStatsScoreByLevel(partSO.CalMaxReachableUpgradeLevel())}]");
        }
        void LogInspector_2(PBChassisSO chassisSO, PBPartSO[] partSOs)
        {
            foreach (var gearSO in partSOs)
            {
                EditorGUILayout.LabelField($"----- {gearSO} -----");
                EditorGUILayout.LabelField($"Upgrade Level [{gearSO}]: [{gearSO.GetCurrentUpgradeLevel()}] - [{gearSO.CalMaxReachableUpgradeLevel()}]");
                EditorGUILayout.LabelField($"Stats Score [{gearSO}]: [{RobotStatsCalculator.CalCombinationStatsScore(false, chassisSO, gearSO)}] - [{RobotStatsCalculator.CalCombinationStatsScore(true, chassisSO, gearSO)}]");
            }
        }
    }
#endif

    public virtual PersonalInfo GetRandomPersonalInfo()//(PBPvPArenaSO arenaSO)
    {
        return m_PlayerDatabaseSO.GetRandomBotInfo();
    }

    protected virtual PBPvPArenaSO.MatchmakingProfileTable FindProfileTableFromSource(ProfileSource profileSource, PBPvPArenaSO arenaSO)
    {
        if (m_CurrentChosenModeVariable.value == Mode.Battle)
        {
            return profileSource switch
            {
                ProfileSource.Normal => arenaSO.profileTableNormal_Battle,
                ProfileSource.UserBias => arenaSO.profileTablePlayerBias_Battle,
                ProfileSource.OpponentBias => arenaSO.profileTableOpponentBias_Battle,
                _ => null,
            };
        }

        return profileSource switch
        {
            ProfileSource.Normal => arenaSO.profileTableNormal,
            ProfileSource.UserBias => arenaSO.profileTablePlayerBias,
            ProfileSource.OpponentBias => arenaSO.profileTableOpponentBias,
            _ => null,
        };
    }

    protected virtual MatchmakingRawInput GetCurrentMatchmakingRawInput(PvPArenaSO arenaSO, bool isOpponent)
    {
        var PBArenaSO = arenaSO as PBPvPArenaSO;
        var MatchmakingInputArr2D = m_CurrentChosenModeVariable.value == Mode.Normal ? PBArenaSO.MatchmakingInputArr2D_Normal : PBArenaSO.MatchmakingInputArr2D_Battle;
        MatchmakingRawInput[][] m_MatchmakingInputArr2D = new MatchmakingRawInput[][] {
            isOpponent ? MatchmakingInputArr2D.lostConditionList : MatchmakingInputArr2D.lostConditionListTeammate,
            isOpponent ? MatchmakingInputArr2D.upgradeConditionList : MatchmakingInputArr2D.upgradeConditionListTeammate,
            isOpponent ? MatchmakingInputArr2D.winConditionList : MatchmakingInputArr2D.winConditionListTeammate,
            isOpponent ? MatchmakingInputArr2D.emptyConditionList : MatchmakingInputArr2D.emptyConditionListTeammate
        };

        var decodingNumber = m_CurrentChosenModeVariable.value == Mode.Battle ? m_MatchmakingDecodingNumber_Battle.value - 10 : m_MatchmakingDecodingNumber.value - 10;
        var x = decodingNumber % 10;
        var y = decodingNumber / 10;
        if (decodingNumber < 0 || y >= m_MatchmakingInputArr2D.Length || x >= m_MatchmakingInputArr2D[y].Length)
        {
            x = 0;
            y = 3;
        }
        //#if UNITY_EDITOR
        //            var rawInput = m_MatchmakingInputArr2D[y][x];
        //            Debug.Log($"Current matchmaking index: [{y},{x}]");
        //            Debug.Log($"Current matchmaking raw input: [{rawInput.scoreSource},{rawInput.profileSource}]");
        //#endif
        return m_MatchmakingInputArr2D[y][x];
    }

    protected virtual MatchmakingInput GetCurrentMatchmakingInput(PBPvPArenaSO arenaSO, bool isOpponent)
    {
        //if (m_TotalNumOfMatches < NumOfHardCodedMatches)
        //{
        //    return new MatchmakingInput(
        //        m_HardCodedInput[m_TotalNumOfMatches.value].scoreSource,
        //        m_HardCodedInput[m_TotalNumOfMatches.value].profileTable);
        //}
        var rawInput = GetCurrentMatchmakingRawInput(arenaSO, isOpponent);
        return new MatchmakingInput(
            m_TotalNumOfMatches.value < numOfHardCodedMatches ? ScoreSource.AvailableScore : rawInput.scoreSource,
            FindProfileTableFromSource(rawInput.profileSource, arenaSO));
    }

    protected virtual float GetScoreDiff(PBPvPArenaSO arenaSO, bool isOpponent)
    {
        if (IsInHardCodedMatch(arenaSO, out PvPTier tier))
        {
            return GetScoreDiffByTier(tier.ScoreDiffRange);
        }

        var currentMatchmakingInput = GetCurrentMatchmakingInput(arenaSO, isOpponent);
        return currentMatchmakingInput.profileTable.GetRandomScoreDiff();

        float GetScoreDiffByTier(RangeFloatValue scoreDiffRange)
        {
            return GetRandomScoreDiff(scoreDiffRange);
        }

        float GetRandomScoreDiff(RangeValue<float> scoreDiffRange)
        {
            return Random.Range(scoreDiffRange.minValue, scoreDiffRange.maxValue);
        }
    }

    // protected virtual float GetStatScore(PBPvPArenaSO arenaSO)
    // {
    //     var currentMatchmakingInput = GetCurrentMatchmakingInput(arenaSO);
    //     return FindStatsScoreFromSource(currentMatchmakingInput.scoreSource);
    // }

    protected virtual float FindPlayerBestChassisScore(PBPvPArenaSO arenaSO, bool isOpponent)
    {
        var currentMatchmakingInput = GetCurrentMatchmakingInput(arenaSO, isOpponent);
        var scoreSource = currentMatchmakingInput.scoreSource;

        var chassisParts = chassisManagerSO.Parts.FindAll(item => item.IsUnlocked()).Cast<PBChassisSO>();
        var frontParts = frontManagerSO.Parts.FindAll(item => item.IsUnlocked());
        var upperParts = upperManagerSO.Parts.FindAll(item => item.IsUnlocked());
        var wheelParts = wheelManagerSO.Parts.FindAll(item => item.IsUnlocked());
        var specialBots = specialBotManagerSO.Parts.FindAll(item => item.IsUnlocked()).Cast<PBChassisSO>();

        //Determine a set of upper and front
        List<PBPartSO> FindOptimalUpperAndFrontSet(List<PBPartSO> upperParts, List<PBPartSO> frontParts, int upperAmount, int frontAmount, float maxPower)
        {
            List<PBPartSO> currentSet = new List<PBPartSO>();
            List<PBPartSO> optimalSet = new List<PBPartSO>();
            float maxATK = 0;
            Backtrack(upperParts, frontParts, upperAmount, frontAmount, maxPower, 0, 0, currentSet, ref optimalSet, ref maxATK);
            return optimalSet;
        }

        void Backtrack(List<PBPartSO> upperParts, List<PBPartSO> frontParts, int upperAmount, int frontAmount, float maxPower, float currentPower, int index, List<PBPartSO> currentSet, ref List<PBPartSO> optimalSet, ref float maxATK)
        {
            if ((upperAmount == 0 || index >= upperParts.Count) && frontAmount == 0 || (index - upperParts.Count) >= frontParts.Count)
            {
                // backTrackCount_1++;
                if (currentPower <= maxPower)
                {
                    float totalATK = currentSet.Sum(part => scoreSource == ScoreSource.AvailableScore ? part.CalCurrentAttack() : part.CalAttackByLevel(part.CalMaxReachableUpgradeLevel()));

                    // backTrackCount++;
                    if (totalATK > maxATK)
                    {
                        maxATK = totalATK;
                        optimalSet = new List<PBPartSO>(currentSet);
                    }
                }
                return;
            }

            for (int i = index; i < upperParts.Count + frontParts.Count; i++)
            {
                PBPartSO part = null;
                bool isUpper = i < upperParts.Count;
                if (isUpper)
                {
                    if (upperAmount > 0)
                    {
                        part = upperParts[i];
                    }
                }
                else
                {
                    if (frontAmount > 0)
                    {
                        int frontIndex = i - upperParts.Count;
                        part = frontParts[frontIndex];
                    }
                }
                if (part != null)
                {
                    var nextPower = currentPower + part.GetPower().value;
                    if (nextPower > maxPower)
                    {
                        float totalATK = currentSet.Sum(part => scoreSource == ScoreSource.AvailableScore ? part.CalCurrentAttack() : part.CalAttackByLevel(part.CalMaxReachableUpgradeLevel()));

                        // backTrackCount++;
                        if (totalATK > maxATK)
                        {
                            maxATK = totalATK;
                            optimalSet = new List<PBPartSO>(currentSet);
                        }
                    }
                    else
                    {
                        currentSet.Add(part);
                        if (isUpper)
                        {
                            Backtrack(upperParts, frontParts, upperAmount - 1, frontAmount, maxPower, nextPower, i + 1, currentSet, ref optimalSet, ref maxATK);
                        }
                        else
                        {
                            Backtrack(upperParts, frontParts, upperAmount, frontAmount - 1, maxPower, nextPower, i + 1, currentSet, ref optimalSet, ref maxATK);
                        }
                        currentSet.RemoveAt(currentSet.Count - 1);
                    }
                }

            }
        }


        float FindHighestOverallScore()
        {
            var highestOverallScore = 0f;
            List<PBPartSO> parts = new();
            List<BotPartSlot> upperSlots = new();
            List<BotPartSlot> frontSlots = new();
            List<PBPartSO> optimalUpperAndFrontSet = new();
            List<PBPartSO> upperSet = new();
            List<PBPartSO> frontSet = new();
            var sortedChassisParts = chassisParts.OrderByDescending(x => x.GetPower().value)
            .ThenByDescending(x => scoreSource == ScoreSource.AvailableScore ? x.CalCurrentHp() : x.CalHpByLevel(x.CalMaxReachableUpgradeLevel()))
            .ThenByDescending(x => x.GetUpperAndFrontSlotAmount());
            var checkedChassisParts = new List<PBChassisSO>();
            bool IsWorseThanPreviousChassisParts(PBChassisSO currentChassisPart)
            {
                foreach (var item in checkedChassisParts)
                {
                    if (currentChassisPart.GetPower().value <= item.GetPower().value && currentChassisPart.GetUpperAndFrontSlotAmount() <= item.GetUpperAndFrontSlotAmount() &&
                        (scoreSource == ScoreSource.AvailableScore ? currentChassisPart.CalCurrentHp() : currentChassisPart.CalHpByLevel(currentChassisPart.CalMaxReachableUpgradeLevel())) <=
                        (scoreSource == ScoreSource.AvailableScore ? item.CalCurrentHp() : item.CalHpByLevel(item.CalMaxReachableUpgradeLevel())))
                    {
                        return true;
                    }
                }
                return false;
            }
            foreach (var chassisSO in sortedChassisParts)
            {
                parts.Clear();
                upperSlots.Clear();
                frontSlots.Clear();
                optimalUpperAndFrontSet.Clear();
                upperSet.Clear();
                frontSet.Clear();

                if (IsWorseThanPreviousChassisParts(chassisSO))
                {
                    continue;
                }
                else
                {
                    checkedChassisParts.Add(chassisSO);
                }
                upperSlots = chassisSO.AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Upper));
                frontSlots = chassisSO.AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Front));

                optimalUpperAndFrontSet = FindOptimalUpperAndFrontSet(upperParts, frontParts, upperSlots.Count, frontSlots.Count, chassisSO.GetPower().value);
                upperSet = optimalUpperAndFrontSet.Where(part => part.PartType == PBPartType.Upper).ToList();
                frontSet = optimalUpperAndFrontSet.Where(part => part.PartType == PBPartType.Front).ToList();
                for (var i = 0; i < (upperSlots.Count < upperSet.Count ? upperSlots.Count : upperSet.Count); i++)
                {
                    parts.Add(upperSet[i]);
                }

                for (var i = 0; i < (frontSlots.Count < frontSet.Count ? frontSlots.Count : frontSet.Count); i++)
                {
                    parts.Add(frontSet[i]);
                }
                var overallScore = RobotStatsCalculator.CalCombinationStatsScore(scoreSource != ScoreSource.AvailableScore, chassisSO, parts.ToArray());
                if (overallScore > highestOverallScore)
                {
                    bestPlayerChassis = chassisSO;
                    highestOverallScore = overallScore;
                }
            }

            return highestOverallScore;
        }

        var highestOverallScore = FindHighestOverallScore();

        //Use the special bot when the it's score is higher than the current score
        foreach (var specialBot in specialBots)
        {
            if (specialBot.GetTotalStatsScore() > highestOverallScore)
            {
                highestOverallScore = specialBot.GetTotalStatsScore();
                bestPlayerChassis = specialBot;
            }
        }
        return highestOverallScore;
    }

    protected virtual PBRobotStatsSO FindRobotForOpponent(PBPvPArenaSO arenaSO, int attempt, out float scoreDiff, bool isOpponent)
    {
        //Prepare for opponent finding
        PBChassisSO chassisSO_Result = null;

        List<PBPartSO> wheelSOResults = new();
        List<BotPartSlot> wheelSlots = new();
        List<PBPartSO> upperSOResults = new();
        List<BotPartSlot> upperSlots = new();
        List<PBPartSO> frontSOResults = new();
        List<BotPartSlot> frontSlots = new();

        bool hasWeaponPart = false;
        bool opponentHasPicked = false;
        bool opponentIsSpecialBot = false;

        //Determine Opponent Stats scores
        if (m_CurrentChosenModeVariable.value == Mode.Battle && !IsInHardCodedMatch(arenaSO, out PvPTier tier))
        {
            scoreDiff = battleBotScoreList[battleBotIndex];
        }
        else
        {
            scoreDiff = GetScoreDiff(arenaSO, isOpponent);
        }
        float statsScore_Player = FindPlayerBestChassisScore(arenaSO, isOpponent);
        var statsScore_Opponent = statsScore_Player * scoreDiff;
        var trophyAmount = CurrencyManager.Instance.GetCurrencySO(CurrencyType.Medal).value;
        for (int i = overallScoreClamps.Count - 1; i >= 0; i--)
        {
            if (trophyAmount >= overallScoreClamps[i].triggeredTrophy)
            {
                var overallScoreClamp = overallScoreClamps[i];
                statsScore_Opponent = Math.Clamp(statsScore_Opponent, overallScoreClamp.scoreRange.x, overallScoreClamp.scoreRange.y);
                break;
            }
        }

        void SetupScriptedBot(ScriptedBot scriptedBot)
        {
            chassisSO_Result = scriptedBot.chassisSO;
            wheelSlots = chassisSO_Result.Cast<PBChassisSO>().AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Wheels));
            upperSlots = chassisSO_Result.Cast<PBChassisSO>().AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Upper));
            frontSlots = chassisSO_Result.Cast<PBChassisSO>().AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Front));
            foreach (var slot in wheelSlots)
            {
                var attachedWheel = chassisSO_Result.AttachedWheels[Mathf.Min(chassisSO_Result.AttachedWheels.Count - 1, wheelSlots.IndexOf(slot))];
                wheelSOResults.Add(attachedWheel);
            }
            if (upperSlots.Count > 0)
            {
                var part = scriptedBot.upper_1;
                if (part != null)
                {
                    upperSOResults.Add(part);
                    if (part.GetAttack().value > 0)
                    {
                        hasWeaponPart = true;
                    }
                }
            }
            if (upperSlots.Count > 1)
            {
                var part = scriptedBot.upper_2;
                if (part != null)
                {
                    upperSOResults.Add(part);
                    if (part.GetAttack().value > 0)
                    {
                        hasWeaponPart = true;
                    }
                }
            }
            if (frontSlots.Count > 0)
            {
                var part = scriptedBot.front;
                if (part != null)
                {
                    frontSOResults.Add(part);
                    if (part.GetAttack().value > 0)
                    {
                        hasWeaponPart = true;
                    }
                }
            }
        }
        //Determine opponent from scripted parts
        if (m_CurrentChosenModeVariable.value == Mode.Battle)
        {
            if (!opponentHasPicked && arenaSO.BattleScriptedBotsIndex < arenaSO.BattleScriptedBots.Count)
            {
                var scriptedBot = arenaSO.BattleScriptedBots[arenaSO.BattleScriptedBotsIndex].bots[battleBotIndex];
                if (scriptedBot.chassisSO != null)
                {
                    SetupScriptedBot(scriptedBot);
                    opponentHasPicked = true;
                }
            }
        }
        else if (m_CurrentChosenModeVariable.value == Mode.Normal)
        {
            if (!opponentHasPicked && arenaSO.ScriptedBotsIndex < arenaSO.ScriptedBots.Count)
            {
                var scriptedBot = arenaSO.ScriptedBots[arenaSO.ScriptedBotsIndex];
                if (scriptedBot.chassisSO != null)
                {
                    SetupScriptedBot(scriptedBot);
                    opponentHasPicked = true;
                }
            }
        }

        //Determine opponent from the TransformBots:
        if (!opponentHasPicked && IsMeetTransformBotCondition(arenaSO, out PBChassisSO transformBotSO))
        {
            chassisSO_Result = transformBotSO;
            wheelSlots = chassisSO_Result.Cast<PBChassisSO>().AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Wheels));
            upperSlots = chassisSO_Result.Cast<PBChassisSO>().AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Upper));
            frontSlots = chassisSO_Result.Cast<PBChassisSO>().AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Front));
            foreach (var slot in wheelSlots)
            {
                wheelSOResults.Add((PBPartSO)slot.PartVariableSO.value);
            }
            foreach (var slot in upperSlots)
            {
                upperSOResults.Add((PBPartSO)slot.PartVariableSO.value);
                if (((PBPartSO)slot.PartVariableSO.value).GetAttack().value > 0)
                {
                    hasWeaponPart = true;
                }
            }
            foreach (var slot in frontSlots)
            {
                frontSOResults.Add((PBPartSO)slot.PartVariableSO.value);
                if (((PBPartSO)slot.PartVariableSO.value).GetAttack().value > 0)
                {
                    hasWeaponPart = true;
                }
            }
            opponentHasPicked = true;
            opponentIsSpecialBot = true;
            return CreateOpponentRobotStats(0, true, chassisSO_Result, wheelSOResults, wheelSlots, upperSOResults, upperSlots, frontSOResults, frontSlots, RandomActiveSkillForOpponent(arenaSO), false);
        }

        //Determine opponent from the special bots:
        if (!opponentHasPicked && Random.Range(0f, 1f) <= m_ChooseBossForOpponentPercent)
        {
            var specialBots = arenaSO.SpecialBots.FindAll(item => item.IsUnlocked());
            var validSpecialBots = specialBots.FindAll(bot => bot.GetTotalStatsScore() > statsScore_Opponent * (1f - m_ChooseBossScoreOffsetPercent) && bot.GetTotalStatsScore() < statsScore_Opponent * (1f + m_ChooseBossScoreOffsetPercent));
            if (validSpecialBots.Count > 0)
            {
                chassisSO_Result = validSpecialBots.GetRandom();
                wheelSlots = chassisSO_Result.Cast<PBChassisSO>().AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Wheels));
                upperSlots = chassisSO_Result.Cast<PBChassisSO>().AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Upper));
                frontSlots = chassisSO_Result.Cast<PBChassisSO>().AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Front));
                foreach (var slot in wheelSlots)
                {
                    wheelSOResults.Add((PBPartSO)slot.PartVariableSO.value);
                }
                foreach (var slot in upperSlots)
                {
                    upperSOResults.Add((PBPartSO)slot.PartVariableSO.value);
                    if (((PBPartSO)slot.PartVariableSO.value).GetAttack().value > 0)
                    {
                        hasWeaponPart = true;
                    }
                }
                foreach (var slot in frontSlots)
                {
                    frontSOResults.Add((PBPartSO)slot.PartVariableSO.value);
                    if (((PBPartSO)slot.PartVariableSO.value).GetAttack().value > 0)
                    {
                        hasWeaponPart = true;
                    }
                }
                opponentHasPicked = true;
                opponentIsSpecialBot = true;
            }
        }

        //Determine opponent from the normal bots:
        if (!opponentHasPicked)
        {
            //Determine chassis
            var upperAndUpperSlotAmount = bestPlayerChassis.GetUpperAndFrontSlotAmount();
            List<PBChassisSO> randomchassisPool = new();
            List<PBChassisSO> filteredChassisPool = new();

            if (Random.Range(0f, 1f) > m_ChooseChassisIrregularlyForOpponentPercent)
            {
                bool IsChassisMatchCondition(PBChassisSO chassisSO, int from, int to) //Check if robot has upper/front slot amount in range 'from' - 'to'
                {
                    var amount = chassisSO.GetUpperAndFrontSlotAmount();
                    if (amount >= from && amount <= to) return true;
                    return false;
                }

                switch (upperAndUpperSlotAmount)
                {
                    case 1:
                        randomchassisPool = arenaSO.ChassisParts.FindAll(chassis => IsChassisMatchCondition(chassis.Cast<PBChassisSO>(), 1, 2));
                        break;
                    case 2:
                        randomchassisPool = arenaSO.ChassisParts.FindAll(chassis => IsChassisMatchCondition(chassis.Cast<PBChassisSO>(), 2, 3));
                        break;
                    case 3:
                        randomchassisPool = arenaSO.ChassisParts.FindAll(chassis => IsChassisMatchCondition(chassis.Cast<PBChassisSO>(), 2, 3));
                        break;
                    default:
                        break;
                }

                if (arenaSO.index == 0 && arenaSO.totalNumOfPlayedMatches < 5)
                {
                    filteredChassisPool = randomchassisPool.FindAll(chassis => ((PBChassisSO)chassis).GetUpperAndFrontSlotAmount() == 1);
                }
                else if (upperAndUpperSlotAmount > 1)
                {
                    if (Random.Range(0f, 1f) <= 0.9f)
                    {
                        filteredChassisPool = randomchassisPool.FindAll(chassis => ((PBChassisSO)chassis).GetUpperAndFrontSlotAmount() == 2);
                    }
                    else
                    {
                        filteredChassisPool = randomchassisPool.FindAll(chassis => ((PBChassisSO)chassis).GetUpperAndFrontSlotAmount() == 3);
                    }
                }
            }
            else
            {
                randomchassisPool = arenaSO.ChassisParts;
            }

            if (filteredChassisPool.Count > 0)
            {
                randomchassisPool = filteredChassisPool;
            }
            chassisSO_Result = randomchassisPool.GetRandom();
            wheelSlots = chassisSO_Result.Cast<PBChassisSO>().AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Wheels));
            upperSlots = chassisSO_Result.Cast<PBChassisSO>().AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Upper));
            frontSlots = chassisSO_Result.Cast<PBChassisSO>().AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Front));

            //Determine parts of all slots
            var remainChassisPower = chassisSO_Result.GetPower().value;
            //==== Determine Wheel parts ====
            if (wheelSlots.Count > 0)
            {
                foreach (var slot in wheelSlots)
                {
                    var randomizedPart = chassisSO_Result.AttachedWheels[Mathf.Min(chassisSO_Result.AttachedWheels.Count - 1, wheelSlots.IndexOf(slot))];
                    wheelSOResults.Add(randomizedPart);
                }
            }

            //==== Determine Upper parts ====
            if (upperSlots.Count > 0)
            {
                List<PBPartSO> pickedParts = new();
                var validPartPool = arenaSO.UpperParts.FindAll(part => !part.IsBossPart || (part.IsBossPart && part.IsUnlocked()));
                foreach (var slot in upperSlots)
                {
                    List<PBPartSO> randomUpperPool = validPartPool.FindAll(part => part.GetPower().value <= remainChassisPower && !pickedParts.Contains(part));
                    if (!hasWeaponPart)
                    {
                        randomUpperPool = randomUpperPool.FindAll(part => part.GetAttack().value > 0);
                    }
                    if (randomUpperPool.Count > 0)
                    {
                        var randomizedPart = randomUpperPool.GetRandom();
                        remainChassisPower -= randomizedPart.GetPower().value;
                        upperSOResults.Add(randomizedPart);
                        pickedParts.Add(randomizedPart);
                        if (randomizedPart.GetAttack().value > 0)
                        {
                            hasWeaponPart = true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            //==== Determine Front parts ====
            if (frontSlots.Count > 0)
            {
                List<PBPartSO> pickedParts = new();
                var validPartPool = arenaSO.FrontParts.FindAll(part => !part.IsBossPart || (part.IsBossPart && part.IsUnlocked()));
                foreach (var slot in frontSlots)
                {
                    List<PBPartSO> randomFrontPool = validPartPool.FindAll(part => part.GetPower().value <= remainChassisPower && !pickedParts.Contains(part));
                    if (!hasWeaponPart)
                    {
                        randomFrontPool = randomFrontPool.FindAll(part => part.GetAttack().value > 0);
                    }
                    if (randomFrontPool.Count > 0)
                    {
                        var randomizedPart = randomFrontPool.GetRandom();
                        remainChassisPower -= randomizedPart.GetPower().value;
                        frontSOResults.Add(randomizedPart);
                        pickedParts.Add(randomizedPart);
                        if (randomizedPart.GetAttack().value > 0)
                        {
                            hasWeaponPart = true;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return CreateOpponentRobotStats(statsScore_Opponent, opponentIsSpecialBot, chassisSO_Result, wheelSOResults, wheelSlots, upperSOResults, upperSlots, frontSOResults, frontSlots, RandomActiveSkillForOpponent(arenaSO));
    }

    protected bool IsMeetTransformBotCondition(PBPvPArenaSO arenaSO, out PBChassisSO transformBotSO)
    {
        if (m_TotalNumOfMatches.value - m_MeetTransformBotAtMatch.value >= m_MeetTransformBotNumberOfMatch && arenaSO.TransformBots.Count > 0 && Random.Range(0f, 1f) <= m_ChooseTransformBotForOpponentPercent)
        {
            m_MeetTransformBotAtMatch.value = m_TotalNumOfMatches.value;
            transformBotSO = arenaSO.TransformBots.GetRandom();
            return true;
        }
        transformBotSO = null;
        return false;
    }

    protected PBRobotStatsSO CreateOpponentRobotStats(float statsScore_Opponent,
        bool opponentIsSpecialBot,
        PBChassisSO chassisSO_Result,
        List<PBPartSO> wheelSOResults,
        List<BotPartSlot> wheelSlots,
        List<PBPartSO> upperSOResults,
        List<BotPartSlot> upperSlots,
        List<PBPartSO> frontSOResults,
        List<BotPartSlot> frontSlots,
        ActiveSkillSO activeSkillSO,
        bool isFakeStats = true)
    {
        int weaponPart = 0;
        // foreach (var item in wheelSOResults)
        // {
        //     if (item.GetAttack().value > 0)
        //     {
        //         weaponPart++;
        //     }
        // }
        foreach (var item in upperSOResults)
        {
            if (item.GetAttack().value > 0)
            {
                weaponPart++;
            }
        }
        foreach (var item in frontSOResults)
        {
            if (item.GetAttack().value > 0)
            {
                weaponPart++;
            }
        }

        //Calculate Opponent's Properties:
        float attackSecond = Random.Range(6f, 12f);
        float fake_Atk = MathF.Sqrt(statsScore_Opponent / (attackSecond * Const.UpgradeValue.StatsFactor));
        float total_Atk = fake_Atk - Const.UpgradeValue.Virtual_Atk;
        float Hp = statsScore_Opponent / (fake_Atk * Const.UpgradeValue.StatsFactor);
        float AtkEachPart = total_Atk / weaponPart;

        //Result:
        // m_ManualRobotChassisStatsOfOpponent?.Dispose();
        if (!isFakeStats)
        {
            Hp = chassisSO_Result.GetHealth().value;
            total_Atk = chassisSO_Result.GetAttack().value;
            foreach (var item in upperSOResults)
            {
                Hp += item.GetHealth().value;
                total_Atk += item.GetAttack().value;
            }
            foreach (var item in frontSOResults)
            {
                Hp += item.GetHealth().value;
                total_Atk += item.GetAttack().value;
            }
            // foreach (var item in wheelSOResults)
            // {
            //     Hp += item.GetHealth().value;
            // }
        }
        m_ManualRobotChassisStatsOfOpponent = new ManualChassisStats(Hp, total_Atk, 0, chassisSO_Result, 0);
        Dictionary<PBPartSlot, IPartStats> manualParts = new();
        var chassisSOInstance = m_ManualRobotChassisStatsOfOpponent.partSO.Cast<PBChassisSO>();

        //Setup parts and random color.
        for (int i = 0; i < upperSlots.Count; i++)
        {
            if (i >= upperSOResults.Count)
            {
                var emptyPartVariable = chassisSOInstance.AllPartSlots.Find(slot => slot.PartSlotType.Equals(upperSlots[i].PartSlotType)).PartVariableSO;
                emptyPartVariable.value = null;
                break;
            }
            var upperSO = upperSOResults[i];
            var attack = !isFakeStats ? upperSO.GetAttack().value : (upperSO.GetAttack().value > 0 ? AtkEachPart : 0);
            var manualPart = new ManualPartStats(0, attack, upperSO.GetResistance().value, upperSO, 0);
            var partVariable = chassisSOInstance.AllPartSlots.Find(slot => slot.PartSlotType.Equals(upperSlots[i].PartSlotType)).PartVariableSO;
            partVariable.value = manualPart.partSO;
            manualParts.Add(upperSlots[i].PartSlotType, manualPart);
        }

        for (int i = 0; i < frontSlots.Count; i++)
        {
            if (i >= frontSOResults.Count)
            {
                var emptyPartVariable = chassisSOInstance.AllPartSlots.Find(slot => slot.PartSlotType.Equals(frontSlots[i].PartSlotType)).PartVariableSO;
                emptyPartVariable.value = null;
                break;
            }
            var frontSO = frontSOResults[i];
            var attack = !isFakeStats ? frontSO.GetAttack().value : (frontSO.GetAttack().value > 0 ? AtkEachPart : 0);
            var manualPart = new ManualPartStats(0, attack, frontSO.GetResistance().value, frontSO, 0);
            var partVariable = chassisSOInstance.AllPartSlots.Find(slot => slot.PartSlotType.Equals(frontSlots[i].PartSlotType)).PartVariableSO;
            partVariable.value = manualPart.partSO;
            manualParts.Add(frontSlots[i].PartSlotType, manualPart);
        }

        for (int i = 0; i < wheelSlots.Count; i++)
        {
            if (i >= wheelSOResults.Count)
            {
                var emptyPartVariable = chassisSOInstance.AllPartSlots.Find(slot => slot.PartSlotType.Equals(wheelSlots[i].PartSlotType)).PartVariableSO;
                emptyPartVariable.value = null;
                break;
            }
            var wheelSO = wheelSOResults[i];
            var manualPart = new ManualPartStats(0, 0, wheelSO.GetResistance().value, wheelSO, 0, m_ManualRobotChassisStatsOfOpponent);
            var partVariable = chassisSOInstance.AllPartSlots.Find(slot => slot.PartSlotType.Equals(wheelSlots[i].PartSlotType)).PartVariableSO;
            partVariable.value = manualPart.partSO;
            manualParts.Add(wheelSlots[i].PartSlotType, manualPart);
        }

        var opponentChassisVariable = CreateInstance<ItemSOVariable>();
        var opponentSkillInUseVariable = CreateInstance<ItemSOVariable>();
        var robotStatsOfOpponent = CreateInstance<PBRobotStatsSO>();
        robotStatsOfOpponent.chassisInUse = opponentChassisVariable;
        robotStatsOfOpponent.skillInUse = opponentSkillInUseVariable;
        robotStatsOfOpponent.skillInUse.value = activeSkillSO;
        opponentChassisVariable.value = m_ManualRobotChassisStatsOfOpponent.partSO;
        robotStatsOfOpponent.stats = m_ManualRobotChassisStatsOfOpponent;
        robotStatsOfOpponent.statsOfRobot = manualParts;
        robotStatsOfOpponent.chassisInUse.value = opponentChassisVariable;
        return robotStatsOfOpponent;
    }

    [Conditional(LGDebug.k_UnityEditorDefineSymbol), Conditional(LGDebug.k_LatteDebugDefineSymbol)]
    protected void LogMatchmakingHistory(PBBotInfo botInfo, PvPArenaSO arenaSO, float scoreDiff)
    {
        var logBuilder = new StringBuilder();
        logBuilder.AppendLine("Matchmaking History Log");
        logBuilder.AppendLine("========== LOG_BEGIN ==========");
        logBuilder.AppendLine($"Mode: {(m_CurrentChosenModeVariable.value == Mode.Normal ? "1vs1" : "Battle")}");
        logBuilder.AppendLine($"Score_Diff: {scoreDiff}");
        logBuilder.AppendLine($"NumOfMatches: {m_TotalNumOfMatches.value}");
        logBuilder.AppendLine("\t   OPPONENT");
        logBuilder.AppendLine($"Name: {botInfo.personalInfo.name}");
        logBuilder.AppendLine($"AI_Profile: {botInfo.aiProfile}");
        logBuilder.AppendLine($"Overall_Score: {botInfo.robotStatsSO.value}");
        logBuilder.AppendLine($"Chassis: {botInfo.robotStatsSO.chassisInUse.value.GetInternalName()}");
        logBuilder.AppendLine("\t   PLAYER");
        logBuilder.AppendLine($"Name: {m_PlayerDatabaseSO.playerNameVariable.value}");
        // logBuilder.AppendLine($"CurrentScore: {m_LocalPlayerRobotStatsSO.value} - AvailableScore: {FindStatsScoreFromSource(ScoreSource.AvailableScore)} - MaxScore: {FindStatsScoreFromSource(ScoreSource.MaxScore)}");
        // logBuilder.AppendLine($"Chassis: {m_ChassisManagerSO.currentGenericItemInUse.GetInternalName()}");
        var arenas = arenaSO.tournament.arenas;
        for (int i = 0; i < arenas.Count; i++)
        {
            var arena = arenas[i];
            logBuilder.AppendLine($"===== Arena_{arena.index + 1} =====");
            logBuilder.AppendLine($"NumOfWonMatches: {arena.totalNumOfWonMatches}");
            logBuilder.AppendLine($"NumOfPlayedMatches: {arena.totalNumOfPlayedMatches}");
            logBuilder.AppendLine($"WinRate: {arena.totalNumOfWonMatches / (float)arena.totalNumOfPlayedMatches}");
        }
        logBuilder.AppendLine("========== LOG_END ==========");
        LGDebug.Log(logBuilder, kDebugTag);
    }

    protected virtual bool IsInHardCodedMatch(PBPvPArenaSO arenaSO, out PvPTier tier)
    {
        if (m_CurrentChosenModeVariable.value == Mode.Normal)
        {
            if (arenaSO.totalNumOfPlayedMatches_Normal < arenaSO.NumOfHardCodedMatches)
            {
                tier = arenaSO.HardCodedInput.Tiers[arenaSO.CurrentTierIndex];
                return true;
            }
        }
        else if (m_CurrentChosenModeVariable.value == Mode.Battle)
        {
            if (arenaSO.totalNumOfPlayedMatches_Battle < arenaSO.NumOfHardCodedMatchesBattle)
            {
                tier = arenaSO.HardCodedBattleInput[arenaSO.CurrentBattleTierIndex].GetPvPBattleTier(battleBotIndex).PvPTier;
                return true;
            }
        }
        tier = null;
        return false;
    }

    protected virtual PB_AIProfile GetRandomAIProfile(PBPvPArenaSO arenaSO, bool isOpponent)
    {
        if (IsInHardCodedMatch(arenaSO, out PvPTier tier))
        {
            return tier.AIProfile;
        }
        return GetCurrentMatchmakingInput(arenaSO, isOpponent).profileTable.GetRandomAIProfile();
    }

    public override PBPlayerInfo FindOpponent(PvPArenaSO arenaSO, Predicate<PBPlayerInfo> predicate = null)
    {
        var attempt = 0;
        do
        {
            var aiProfile = GetRandomAIProfile(arenaSO as PBPvPArenaSO, true);
            var personalInfo = GetRandomPersonalInfo();
            var robotStats = FindRobotForOpponent(arenaSO as PBPvPArenaSO, attempt, out float scoreDiff, true);
            var infoOfOpponent = new PBBotInfo(personalInfo, robotStats, aiProfile);
            if (robotStats != null && ((predicate?.Invoke(infoOfOpponent) ?? true) || attempt >= MaxAttempt))
            {
                LogMatchmakingHistory(infoOfOpponent, arenaSO, scoreDiff);
                return infoOfOpponent;
            }
            attempt++;
        }
        while (true);
    }

    public virtual PBPlayerInfo FindTeammate(PvPArenaSO arenaSO, Predicate<PBPlayerInfo> predicate = null)
    {
        var attempt = 0;
        do
        {
            var aiProfile = GetRandomAIProfile(arenaSO as PBPvPArenaSO, false);
            var personalInfo = GetRandomPersonalInfo();
            var robotStats = FindRobotForOpponent(arenaSO as PBPvPArenaSO, attempt, out float scoreDiff, false);
            var infoOfOpponent = new PBBotInfo(personalInfo, robotStats, aiProfile);
            if (robotStats != null && ((predicate?.Invoke(infoOfOpponent) ?? true) || attempt >= MaxAttempt))
            {
                LogMatchmakingHistory(infoOfOpponent, arenaSO, scoreDiff);
                return infoOfOpponent;
            }
            attempt++;
        }
        while (true);
    }

    public virtual PBPlayerInfo FindOpponent(PvPArenaSO arenaSO, TestMatchMakingSO testMatchMakingSO)
    {
        var attempt = 0;
        do
        {
            var aiProfile = testMatchMakingSO._AIProfile;
            var personalInfo = GetRandomPersonalInfo();
            //Build opponent robot
            var statsScore_Player = testMatchMakingSO.GetPlayerScore();
            var statsScore_Opponent = statsScore_Player * testMatchMakingSO.scoreMultiplier;
            //Prepare for opponent finding
            var scriptedBot = testMatchMakingSO.enemyScriptedBot;
            var chassisSO_Result = scriptedBot.chassisSO;

            List<PBPartSO> wheelSOResults = new();
            List<PBPartSO> upperSOResults = new();
            List<PBPartSO> frontSOResults = new();
            var wheelSlots = chassisSO_Result.AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Wheels));
            var upperSlots = chassisSO_Result.AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Upper));
            var frontSlots = chassisSO_Result.AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Front));
            foreach (var slot in wheelSlots)
            {
                var attachedWheel = chassisSO_Result.AttachedWheels[Mathf.Min(chassisSO_Result.AttachedWheels.Count - 1, wheelSlots.IndexOf(slot))];
                wheelSOResults.Add(attachedWheel);
            }
            if (upperSlots.Count > 0)
            {
                var part = scriptedBot.upper_1;
                if (part != null)
                {
                    upperSOResults.Add(part);
                }
            }
            if (upperSlots.Count > 1)
            {
                var part = scriptedBot.upper_2;
                if (part != null)
                {
                    upperSOResults.Add(part);
                }
            }
            if (frontSlots.Count > 0)
            {
                var part = scriptedBot.front;
                if (part != null)
                {
                    frontSOResults.Add(part);
                }
            }
            var robotStats = CreateOpponentRobotStats(statsScore_Opponent, false, chassisSO_Result, wheelSOResults, wheelSlots, upperSOResults, upperSlots, frontSOResults, frontSlots, RandomActiveSkillForOpponent(arenaSO as PBPvPArenaSO), testMatchMakingSO.isFakeStats);
            var infoOfOpponent = new PBBotInfo(personalInfo, robotStats, aiProfile);
            if (robotStats != null || attempt >= MaxAttempt)
            {
                return infoOfOpponent;
            }
            attempt++;
        }
        while (true);
    }

    public override IEnumerator FindOpponent_CR(PvPArenaSO arenaSO, Action<PBPlayerInfo> callback, Predicate<PBPlayerInfo> predicate = null)
    {
        if (m_CurrentChosenModeVariable.value == Mode.Normal)
            yield return base.FindOpponent_CR(arenaSO, callback, predicate);
        else
        {
            var opponent = FindOpponent(arenaSO, predicate);
            var duration = Random.Range(1f, m_SearchingDurationBattleMode);
            yield return new WaitForSeconds(duration);
            callback?.Invoke(opponent);
        }
    }

    public virtual IEnumerator FindOpponent_CR(PvPArenaSO arenaSO, Action<PBPlayerInfo> callback, Action<PBPlayerInfo> callback2, Predicate<PBPlayerInfo> predicate = null)
    {
        if (m_CurrentChosenModeVariable.value == Mode.Normal)
            yield return base.FindOpponent_CR(arenaSO, callback, predicate);
        else
        {
            var opponent = FindOpponent(arenaSO, predicate);
            var duration = Random.Range(1f, m_SearchingDurationBattleMode);
            callback?.Invoke(opponent);
            yield return new WaitForSeconds(duration);
            callback2?.Invoke(opponent);
        }
    }

    public virtual IEnumerator FindTeammate_CR(PvPArenaSO arenaSO, Action<PBPlayerInfo> callback, Action<PBPlayerInfo> callback2, Predicate<PBPlayerInfo> predicate = null)
    {
        if (m_CurrentChosenModeVariable.value == Mode.Battle)
        {
            var opponent = FindTeammate(arenaSO, predicate);
            var duration = Random.Range(1f, m_SearchingDurationBattleMode);
            callback?.Invoke(opponent);
            yield return new WaitForSeconds(duration);
            callback2?.Invoke(opponent);
        }
        yield break;
    }

    private void OnEnable()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, HandleFinalRoundComplete);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, HandleMatchStarted);
    }

    private void OnDisable()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, HandleFinalRoundComplete);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, HandleMatchStarted);
    }

    private void HandleFinalRoundComplete(object[] paramters)
    {
        if (paramters[0] is not PvPMatch pvpMatch) return;

        var isVictory = pvpMatch.isVictory;
        if (isVictory)
        {
            m_WinTheFirstConditionStarterPack.value += 1;
        }

        var arenaSO = pvpMatch.arenaSO.Cast<PBPvPArenaSO>();
        if (m_CurrentChosenModeVariable.value == Mode.Normal)
        {
            if (arenaSO.ScriptedBotsIndex < arenaSO.ScriptedBots.Count)
            {
                arenaSO.ScriptedBotsIndex++;
            }
            // if (pvpMatch.isVictory == true) arenaSO.CurrentTierIndex.value = Mathf.Min(tierCount - 1, arenaSO.CurrentTierIndex.value + 1);
            // else arenaSO.CurrentTierIndex.value = Mathf.Max(0, arenaSO.CurrentTierIndex.value - 2);
            var tierCount = arenaSO.HardCodedInput.Tiers.Count;
            arenaSO.CurrentTierIndex = Mathf.Min(tierCount - 1, arenaSO.CurrentTierIndex + 1);
        }
        else if (m_CurrentChosenModeVariable.value == Mode.Battle)
        {
            if (arenaSO.BattleScriptedBotsIndex < arenaSO.BattleScriptedBots.Count)
            {
                arenaSO.BattleScriptedBotsIndex++;
            }
            var tierCount = arenaSO.HardCodedBattleInput.Count;
            arenaSO.CurrentBattleTierIndex = Mathf.Min(tierCount - 1, arenaSO.CurrentBattleTierIndex + 1);
        }

        if (highestAchievedTrophyVar.value >= requiredTrophiesToUnlockActiveSkillVar.value && pvpMatch.Cast<PBPvPMatch>().mode == Mode.Normal)
        {
            var opponentInfo = pvpMatch.GetOpponentInfo();
            var opponentRobot = PBRobot.allFightingRobots.Find(robot => robot.PlayerInfoVariable.value == opponentInfo);
            if (opponentRobot.ActiveSkillCaster != null && opponentRobot.ActiveSkillCaster.totalSkillCastCount > 0)
            {
                skillUseStreakCount.value++;
                noSkillUseStreakCount.value = 0;
            }
            else
            {
                skillUseStreakCount.value = 0;
                noSkillUseStreakCount.value++;
            }
            LGDebug.Log($"TotalSkillCastCount: {opponentRobot?.ActiveSkillCaster.totalSkillCastCount ?? 0} - {skillUseStreakCount.value} - {noSkillUseStreakCount.value}");
        }
    }

    private void HandleMatchStarted()
    {
        m_MatchConditionStarterPack.value += 1;
    }

    private ActiveSkillSO RandomActiveSkillForOpponent(PBPvPArenaSO arenaSO)
    {
        // int cardQuantity = 0;
        // MatchmakingInput currentMatchmakingInput = GetCurrentMatchmakingInput(arenaSO);
        // float skillUsageRate = currentMatchmakingInput.profileTable.skillUsageRate;
        // if (m_CurrentChosenModeVariable.value == Mode.Normal)
        // {
        //     if (skillUseStreakCount.value >= decreaseSkillUsageRateThreshold)
        //     {
        //         skillUsageRate *= decreaseSkillUsageRateAmount;
        //         LGDebug.Log($"DECREASE skil usage rate to {skillUsageRate * 100}%");
        //     }
        //     else if (noSkillUseStreakCount.value >= increaseSkillUsageRateThreshold)
        //     {
        //         skillUsageRate *= increaseSkillUsageRateAmount;
        //         LGDebug.Log($"INCREASE skil usage rate to {skillUsageRate * 100}%");
        //     }
        // }
        // if (highestAchievedTrophyVar.value >= requiredTrophiesToUnlockActiveSkillVar.value && skillUsageRate > 0f && Random.value <= skillUsageRate)
        // {
        //     cardQuantity = currentMatchmakingInput.profileTable.skillCardQuantityRngInfos.GetRandomRedistribute().cardQuantity;
        // }
        // ActiveSkillSO activeSkillSO = Instantiate(activeSkillManagerSO.GetRandomItem().Cast<ActiveSkillSO>());
        // activeSkillSO.RemoveModule(activeSkillSO.GetModule<CardItemModule>());
        // activeSkillSO.AddModule(new ManualCardItemModule(cardQuantity));
        return null;
    }
}