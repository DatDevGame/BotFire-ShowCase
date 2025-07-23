using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames.PvP;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;
using HyrphusQ.SerializedDataStructure;
using UnityEditor;
using System.Linq;

[WindowMenuItem("PvPArenaSO", assetFolderPath: "Assets/__PocketBot/PvP/ScriptableObjects/_ArenaSOs", mode: WindowMenuItemAttribute.Mode.Multiple, sortByName: true)]
[CreateAssetMenu(fileName = "PvPArenaSO", menuName = "PocketBots/PvP/PvPArenaSO")]
public class PBPvPArenaSO : PvPArenaSO, IResourceLocationProvider
{
    #region Subclasses
    [Serializable]
    public class MatchmakingProfileTable
    {
        [SerializeField, TableList]
        protected List<StatsScoreDiffRngInfo> m_StatsScoreDiffRngInfos;
        [SerializeField, TableList]
        protected List<AIProfileRngInfo> m_AIProfileRngInfos;
        [SerializeField, Range(0f, 1f)]
        protected float m_SkillUsageRate;
        [SerializeField, TableList]
        protected List<SkillCardQuantityRngInfo> m_SkillCardQuantityRngInfos;

        public List<StatsScoreDiffRngInfo> statsScoreDiffRngInfos => m_StatsScoreDiffRngInfos;
        public List<AIProfileRngInfo> aiProfileRngInfos => m_AIProfileRngInfos;
        public float skillUsageRate => m_SkillUsageRate;
        public List<SkillCardQuantityRngInfo> skillCardQuantityRngInfos => m_SkillCardQuantityRngInfos;

        public void SetData(List<StatsScoreDiffRngInfo> statsScoreDiffRngInfos = null, List<AIProfileRngInfo> aIProfileRngInfos = null)
        {
            if (statsScoreDiffRngInfos != null)
                m_StatsScoreDiffRngInfos = statsScoreDiffRngInfos;
            if (aIProfileRngInfos != null)
                m_AIProfileRngInfos = aIProfileRngInfos;
        }

        public PB_AIProfile GetRandomAIProfile()
        {
            return aiProfileRngInfos.GetRandom().aiProfile;
        }

        public RangeValue<float> GetRandomScoreDiffRange()
        {
            return statsScoreDiffRngInfos.GetRandom().randomRange;
        }

        public float GetRandomScoreDiff(RangeValue<float> scoreDiffRange)
        {
            return Random.Range(scoreDiffRange.minValue, scoreDiffRange.maxValue);
        }

        public float GetRandomScoreDiff()
        {
            return GetRandomScoreDiff(GetRandomScoreDiffRange());
        }
    }

    [Serializable]
    public class StatsScoreDiffRngInfo : IRandomizable
    {
        [SerializeField]
        protected float m_Probability;
        [SerializeField]
        protected RangeFloatValue m_RandomRange;

        public StatsScoreDiffRngInfo()
        {

        }

        public StatsScoreDiffRngInfo(float probability = 0f, float minValue = 0f, float maxValue = 0f)
        {
            m_Probability = probability;
            m_RandomRange = new(minValue, maxValue);
        }

        public float Probability
        {
            get => m_Probability;
            set => m_Probability = value;
        }
        public RangeValue<float> randomRange => m_RandomRange;
    }

    [Serializable]
    public class AIProfileRngInfo : IRandomizable
    {
        [SerializeField]
        protected float m_Probability;
        [SerializeField]
        protected PB_AIProfile m_AIProfile;

        public float Probability
        {
            get => m_Probability;
            set => m_Probability = value;
        }
        public PB_AIProfile aiProfile => m_AIProfile;
    }

    [Serializable]
    public class SkillCardQuantityRngInfo : IRandomizable
    {
        [SerializeField]
        private int m_CardQuantity;
        [SerializeField, Range(0f, 1f)]
        private float m_Probability;

        public int cardQuantity => m_CardQuantity;
        public float Probability
        {
            get => m_Probability;
            set => m_Probability = value;
        }
    }

    [Serializable]
    public class HardCodedInfo
    {
        [SerializeField]
        protected float m_StatsScoreDiff = 0.9f;
        [SerializeField]
        protected PB_AIProfile m_AIProfile;

        public float statsScoreDiff => m_StatsScoreDiff;
        public PB_AIProfile aiProfile => m_AIProfile;
    }
    #endregion

    #region Serialize Fields
    [SerializeField, FoldoutGroup("BattleRoyale Bet")]
    [InfoBox("Sum of pool's distributuin is not equal to tatal price bool (num of contestant x entry fee). Are you sure it's not a bug ?", InfoMessageType.Error, "IsBattlePricePoolPercentagesInvalid")]
    protected List<float> m_PricePoolDistribute;
    [SerializeField, FoldoutGroup("BattleRoyale Bet")]
    protected Sprite m_BattleBetArenaBG;
    [SerializeField, FoldoutGroup("BattleRoyale Bet")]
    protected Sprite m_BattleBetArenaLogo;
    [SerializeField, FoldoutGroup("BattleRoyale Bet")]
    protected Color m_BattleBetPatternColor = new Color(1f, 1f, 1f, 0.05098039f);
    [SerializeField, FoldoutGroup("References")]
    protected ModeVariable m_ChosenModeVariable;
    [SerializeField, FoldoutGroup("References")]
    protected Material m_LogoArena;
    [SerializeField, FoldoutGroup("References")]
    protected Sprite m_boardSpriteArena;
    [SerializeField, FoldoutGroup("Match Config")]
    protected SerializedDictionary<Mode, MatchConfig> m_ModeConfigs;
    [SerializeField, FoldoutGroup("Stages"), PropertyOrder(2)]
    protected List<StageContainer> stages;
    [SerializeField, FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Normal")]
    protected MatchmakingProfileTable m_ProfileTable_Normal;
    [SerializeField, FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Normal")]
    protected MatchmakingProfileTable m_ProfileTable_PlayerBias;
    [SerializeField, FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Normal")]
    protected MatchmakingProfileTable m_ProfileTable_OpponentBias;
    [SerializeField, FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Battle")]
    protected MatchmakingProfileTable m_ProfileTable_Normal_Battle;
    [SerializeField, FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Battle")]
    protected MatchmakingProfileTable m_ProfileTable_PlayerBias_Battle;
    [SerializeField, FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Battle")]
    protected MatchmakingProfileTable m_ProfileTable_OpponentBias_Battle;
    [SerializeField, FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Battle")]
    protected BattleBuffInfo battleBuffInfo;
    [SerializeField, FoldoutGroup("Match Making"), BoxGroup("Match Making/Parts")]
    protected List<PBChassisSO> chassisParts;
    [SerializeField, FoldoutGroup("Match Making"), BoxGroup("Match Making/Parts")]
    protected List<PBPartSO> upperParts;
    [SerializeField, FoldoutGroup("Match Making"), BoxGroup("Match Making/Parts")]
    protected List<PBPartSO> frontParts;
    [SerializeField, FoldoutGroup("Match Making"), BoxGroup("Match Making/Parts")]
    protected List<PBWheelSO> wheelParts;
    [SerializeField, FoldoutGroup("Match Making"), BoxGroup("Match Making/Parts")]
    protected List<PBChassisSO> specialBots;
    [SerializeField, FoldoutGroup("Match Making"), BoxGroup("Match Making/Parts")]
    protected List<PBChassisSO> transformBots;
    [SerializeField, FoldoutGroup("Match Making Decoding Number"), TabGroup("Match Making Decoding Number/Decoding Number Group", "Normal")]
    protected MatchmakingRawInputData matchmakingInputArr2D_Normal;
    [SerializeField, FoldoutGroup("Match Making Decoding Number"), TabGroup("Match Making Decoding Number/Decoding Number Group", "Normal")]
    protected int thresholdNumOfLostMatchesInColumn_Normal;
    [SerializeField, FoldoutGroup("Match Making Decoding Number"), TabGroup("Match Making Decoding Number/Decoding Number Group", "Normal")]
    protected int thresholdNumOfWonMatchesInColumn_Normal;
    [SerializeField, FoldoutGroup("Match Making Decoding Number"), TabGroup("Match Making Decoding Number/Decoding Number Group", "Battle")]
    protected MatchmakingRawInputData matchmakingInputArr2D_Battle;
    [SerializeField, FoldoutGroup("Match Making Decoding Number"), TabGroup("Match Making Decoding Number/Decoding Number Group", "Battle")]
    protected int thresholdNumOfLostMatchesInColumn_Battle;
    [SerializeField, FoldoutGroup("Match Making Decoding Number"), TabGroup("Match Making Decoding Number/Decoding Number Group", "Battle")]
    protected int thresholdNumOfWonMatchesInColumn_Battle;
    [SerializeField, FoldoutGroup("Hard Coded Matches"), TabGroup("Hard Coded Matches/Hard Coded Matches Group", "Normal")]
    protected int numOfHardCodedMatches;
    [SerializeField, FoldoutGroup("Hard Coded Matches"), TabGroup("Hard Coded Matches/Hard Coded Matches Group", "Normal")]
    protected PvPArenaTierInput m_HardCodedInput;
    [SerializeField, FoldoutGroup("Hard Coded Matches"), TabGroup("Hard Coded Matches/Hard Coded Matches Group", "Battle")]
    protected int numOfHardCodedMatchesBattle;
    [SerializeField, FoldoutGroup("Hard Coded Matches"), TabGroup("Hard Coded Matches/Hard Coded Matches Group", "Battle")]
    protected List<PvPArenaBattleTierInput> m_HardCodedBattleInput;
    [SerializeField, FoldoutGroup("Scripted Bots"), TabGroup("Scripted Bots/Scripted Bots Group", "Normal")]
    protected List<ScriptedBot> scriptedBots;
    [SerializeField, FoldoutGroup("Scripted Bots"), TabGroup("Scripted Bots/Scripted Bots Group", "Battle"), ListDrawerSettings(NumberOfItemsPerPage = 3)]
    protected List<BattleScriptedBot> battleScriptedBots;
    [SerializeField, FoldoutGroup("Trophy Road")]
    protected Color insideColor, outsideColor;
    [SerializeField, FoldoutGroup("Trophy Road")]
    protected Sprite patternSprite;
    [SerializeField, FoldoutGroup("Trophy Road")]
    protected Color patternTintColor = new Color(1f, 1f, 1f, 0.05098039f);
    [SerializeField, FoldoutGroup("Scripted Boxes")]
    protected PBScriptedGachaPacks scriptedGachaBoxes;
    #endregion

    #region PlayerPref Properties
    public virtual int CurrentBattleTierIndex
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_CurrentBattleTierIndex", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_CurrentBattleTierIndex", value);
        }
    }
    public virtual int CurrentTierIndex
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_CurrentTierIndex", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_CurrentTierIndex", value);
        }
    }
    public virtual int ScriptedBotsIndex
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_ScriptedBotsIndex", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_ScriptedBotsIndex", value);
        }
    }
    public virtual int BattleScriptedBotsIndex
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_BattleScriptedBotsIndex", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_BattleScriptedBotsIndex", value);
        }
    }
    public virtual int totalNumOfPlayedMatches_Normal
    {
        get
        {
            return totalNumOfWonMatches_Normal + totalNumOfLostMatches_Normal + totalNumOfAbandonedMatches_Normal;
        }
    }
    public virtual int totalNumOfPlayedMatches_Battle
    {
        get
        {
            return totalNumOfWonMatches_Battle + totalNumOfLostMatches_Battle + totalNumOfAbandonedMatches_Battle;
        }
    }
    public virtual int totalNumOfWonMatches_Normal
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_TotalNumOfWonMatches_Normal", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_TotalNumOfWonMatches_Normal", value);
        }
    }
    public virtual int totalNumOfLostMatches_Normal
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_TotalNumOfLostMatches_Normal", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_TotalNumOfLostMatches_Normal", value);
        }
    }
    public virtual int totalNumOfAbandonedMatches_Normal
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_TotalNumOfAbandonedMatches_Normal", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_TotalNumOfAbandonedMatches_Normal", value);
        }
    }
    public virtual int totalNumOfWonMatches_Battle
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_TotalNumOfWonMatches_Battle", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_TotalNumOfWonMatches_Battle", value);
        }
    }
    public virtual int totalNumOfLostMatches_Battle
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_TotalNumOfLostMatches_Battle", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_TotalNumOfLostMatches_Battle", value);
        }
    }
    public virtual int totalNumOfAbandonedMatches_Battle
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_TotalNumOfAbandonedMatches_Battle", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_TotalNumOfAbandonedMatches_Battle", value);
        }
    }
    public virtual int totalNumOfWonMatches_Others
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_TotalNumOfWonMatches_Others", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_TotalNumOfWonMatches_Others", value);
        }
    }
    public virtual int totalNumOfLostMatches_Others
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_TotalNumOfLostMatches_Others", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_TotalNumOfLostMatches_Others", value);
        }
    }
    public virtual int totalNumOfAbandonedMatches_Others
    {
        get
        {
            return PlayerPrefs.GetInt($"{guid}_TotalNumOfAbandonedMatches_Others", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{guid}_TotalNumOfAbandonedMatches_Others", value);
        }
    }
    public override int totalNumOfWonMatches
    {
        get
        {
            return totalNumOfWonMatches_Normal + totalNumOfWonMatches_Battle + totalNumOfWonMatches_Others;
        }
        set
        {
            var delta = value - totalNumOfWonMatches;
            if (m_ChosenModeVariable.value == Mode.Normal)
            {
                totalNumOfWonMatches_Normal += delta;
            }
            else if (m_ChosenModeVariable.value == Mode.Battle)
            {
                totalNumOfWonMatches_Battle += delta;
            }
            else
            {
                totalNumOfWonMatches_Others += delta;
            }
        }
    }
    public override int totalNumOfLostMatches
    {
        get
        {
            return totalNumOfLostMatches_Normal + totalNumOfLostMatches_Battle + totalNumOfLostMatches_Others;
        }
        set
        {
            var delta = value - totalNumOfLostMatches;
            if (m_ChosenModeVariable.value == Mode.Normal)
            {
                totalNumOfLostMatches_Normal += delta;
            }
            else if (m_ChosenModeVariable.value == Mode.Battle)
            {
                totalNumOfLostMatches_Battle += delta;
            }
            else
            {
                totalNumOfLostMatches_Others += delta;
            }
        }
    }
    public override int totalNumOfAbandonedMatches
    {
        get
        {
            return totalNumOfAbandonedMatches_Normal + totalNumOfAbandonedMatches_Battle + totalNumOfAbandonedMatches_Others;
        }
        set
        {
            var delta = value - totalNumOfAbandonedMatches;
            if (m_ChosenModeVariable.value == Mode.Normal)
            {
                totalNumOfAbandonedMatches_Normal += delta;
            }
            else if (m_ChosenModeVariable.value == Mode.Battle)
            {
                totalNumOfAbandonedMatches_Battle += delta;
            }
            else
            {
                totalNumOfAbandonedMatches_Others += delta;
            }
        }
    }
    #endregion

    #region Public Properties
    public virtual List<float> PricePoolDistribute => m_PricePoolDistribute;
    public virtual Sprite BattleBetArenaBG => m_BattleBetArenaBG;
    public virtual Sprite BattleBetArenaLogo => m_BattleBetArenaLogo;
    public virtual Material LogoArena => m_LogoArena;
    public virtual Sprite BoardSpriteArena => m_boardSpriteArena;
    public virtual int NumOfRounds
    {
        get => m_NumOfRounds;
        set => m_NumOfRounds = value;
    }
    public virtual int WonNumOfCoins
    {
        get
        {
            if (TryGetReward(out CurrencyRewardModule currencyRewardModule, item => item.CurrencyType == CurrencyType.Standard))
                return currencyRewardModule.Amount;
            return Const.IntValue.Zero;
        }
        set
        {
            if (TryGetReward(out CurrencyRewardModule currencyRewardModule, item => item.CurrencyType == CurrencyType.Standard))
                currencyRewardModule.Amount = value;
        }
    }
    public virtual int WonNumOfTrophies
    {
        get
        {
            if (TryGetReward(out CurrencyRewardModule currencyRewardModule, item => item.CurrencyType == CurrencyType.Medal))
                return currencyRewardModule.Amount;
            return Const.IntValue.Zero;
        }
        set
        {
            if (TryGetReward(out CurrencyRewardModule currencyRewardModule, item => item.CurrencyType == CurrencyType.Medal))
                currencyRewardModule.Amount = value;
        }
    }
    public virtual int LostNumOfTrophies
    {
        get
        {
            if (TryGetPunishment(out CurrencyPunishmentModule currencyPunishmentModule, item => item.currencyType == CurrencyType.Medal))
                return Mathf.RoundToInt(currencyPunishmentModule.amount);
            return Const.IntValue.Zero;
        }
        set
        {
            if (TryGetPunishment(out CurrencyPunishmentModule currencyPunishmentModule, item => item.currencyType == CurrencyType.Medal))
            {
                currencyPunishmentModule.amount = value;
            }
        }
    }
    public virtual int RequiredNumOfTrophiesToUnlock
    {
        get
        {
            if (this.TryGetUnlockRequirement(out Requirement_Currency requirement, item => item.currencyType == CurrencyType.Medal))
                return Mathf.RoundToInt(requirement.requiredAmountOfCurrency);
            return Const.IntValue.Zero;
        }
        set
        {
            if (this.TryGetUnlockRequirement(out Requirement_Currency requirement, item => item.currencyType == CurrencyType.Medal))
            {
                requirement.requiredAmountOfCurrency = value;
            }
        }
    }
    public SerializedDictionary<Mode, MatchConfig> ModeConfigs => m_ModeConfigs;
    public virtual int numOfContestant => m_ModeConfigs[m_ChosenModeVariable.value].numOfContestant;
    public virtual float matchTime => m_ModeConfigs[m_ChosenModeVariable.value].matchTime;
    public virtual MatchmakingProfileTable profileTableNormal
    {
        get
        {
            return m_ProfileTable_Normal;
        }
        set
        {
            m_ProfileTable_Normal = value;
        }
    }

    public virtual MatchmakingProfileTable profileTablePlayerBias
    {
        get
        {
            return m_ProfileTable_PlayerBias;
        }
        set
        {
            m_ProfileTable_PlayerBias = value;
        }
    }

    public virtual MatchmakingProfileTable profileTableOpponentBias
    {
        get
        {
            return m_ProfileTable_OpponentBias;
        }
        set
        {
            m_ProfileTable_OpponentBias = value;
        }
    }

    public virtual MatchmakingProfileTable profileTableNormal_Battle
    {
        get
        {
            return m_ProfileTable_Normal_Battle;
        }
        set
        {
            m_ProfileTable_Normal_Battle = value;
        }
    }

    public virtual MatchmakingProfileTable profileTablePlayerBias_Battle
    {
        get
        {
            return m_ProfileTable_PlayerBias_Battle;
        }
        set
        {
            m_ProfileTable_PlayerBias_Battle = value;
        }
    }

    public virtual MatchmakingProfileTable profileTableOpponentBias_Battle
    {
        get
        {
            return m_ProfileTable_OpponentBias_Battle;
        }
        set
        {
            m_ProfileTable_OpponentBias_Battle = value;
        }
    }

    public virtual BattleBuffInfo BattleBuffInfo
    {
        get
        {
            return battleBuffInfo;
        }
        set
        {
            battleBuffInfo = value;
        }
    }

    public virtual List<StageContainer> Stages => stages;
    public virtual List<PBChassisSO> ChassisParts
    {
        get => chassisParts;
        set => chassisParts = value;
    }
    public virtual List<PBPartSO> UpperParts
    {
        get => upperParts;
        set => upperParts = value;
    }
    public virtual List<PBPartSO> FrontParts
    {
        get => frontParts;
        set => frontParts = value;
    }
    public virtual List<PBWheelSO> WheelParts
    {
        get => wheelParts;
        set => wheelParts = value;
    }
    public virtual List<PBChassisSO> SpecialBots
    {
        get => specialBots;
        set => specialBots = value;
    }
    public virtual List<PBChassisSO> TransformBots
    {
        get => transformBots;
        set => transformBots = value;
    }
    public virtual MatchmakingRawInputData MatchmakingInputArr2D_Normal
    {
        get
        {
            return matchmakingInputArr2D_Normal;
        }
        set
        {
            matchmakingInputArr2D_Normal = value;
        }
    }

    public virtual int ThresholdNumOfLostMatchesInColumn_Normal
    {
        get
        {
            return thresholdNumOfLostMatchesInColumn_Normal;
        }
        set
        {
            thresholdNumOfLostMatchesInColumn_Normal = value;
        }
    }

    public virtual int ThresholdNumOfWonMatchesInColumn_Normal
    {
        get
        {
            return thresholdNumOfWonMatchesInColumn_Normal;
        }
        set
        {
            thresholdNumOfWonMatchesInColumn_Normal = value;
        }
    }

    public virtual MatchmakingRawInputData MatchmakingInputArr2D_Battle
    {
        get
        {
            return matchmakingInputArr2D_Battle;
        }
        set
        {
            matchmakingInputArr2D_Battle = value;
        }
    }

    public virtual int ThresholdNumOfLostMatchesInColumn_Battle
    {
        get
        {
            return thresholdNumOfLostMatchesInColumn_Battle;
        }
        set
        {
            thresholdNumOfLostMatchesInColumn_Battle = value;
        }
    }

    public virtual int ThresholdNumOfWonMatchesInColumn_Battle
    {
        get
        {
            return thresholdNumOfWonMatchesInColumn_Battle;
        }
        set
        {
            thresholdNumOfWonMatchesInColumn_Battle = value;
        }
    }

    public virtual List<ScriptedBot> ScriptedBots
    {
        get
        {
            return scriptedBots;
        }
        set
        {
            scriptedBots = value;
        }
    }

    public virtual List<BattleScriptedBot> BattleScriptedBots
    {
        get
        {
            return battleScriptedBots;
        }
        set
        {
            battleScriptedBots = value;
        }
    }

    public virtual int NumOfHardCodedMatches
    {
        get
        {
            return numOfHardCodedMatches;
        }
        set
        {
            numOfHardCodedMatches = value;
        }
    }

    public virtual int NumOfHardCodedMatchesBattle
    {
        get
        {
            return numOfHardCodedMatchesBattle;
        }
        set
        {
            numOfHardCodedMatchesBattle = value;
        }
    }

    public virtual PvPArenaTierInput HardCodedInput
    {
        get
        {
            return m_HardCodedInput;
        }
        set
        {
            m_HardCodedInput = value;
        }
    }

    public virtual List<PvPArenaBattleTierInput> HardCodedBattleInput
    {
        get
        {
            return m_HardCodedBattleInput;
        }
        set
        {
            m_HardCodedBattleInput = value;
        }
    }

    public virtual Color InsideColor => insideColor;
    public virtual Color OutsideColor => outsideColor;
    public virtual Sprite PatternSprite => patternSprite;
    public virtual Color PatternTintColor => patternTintColor;
    public virtual Color BattleBetPatternColor => m_BattleBetPatternColor;
    public virtual PBScriptedGachaPacks ScriptedGachaBoxes
    {
        get
        {
            return scriptedGachaBoxes;
        }
        set
        {
            scriptedGachaBoxes = value;
        }
    }

    public override float wonNumOfPoints
    {
        get
        {
            if (TryGetReward(out CurrencyRewardModule currencyRewardModule, item => item.CurrencyType == CurrencyType.Medal))
                return currencyRewardModule.Amount;
            return Const.IntValue.Zero;
        }
    }
    public override float lostNumOfPoints
    {
        get
        {
            if (TryGetPunishment(out CurrencyPunishmentModule currencyPunishmentModule, item => item.currencyType == CurrencyType.Medal))
                return currencyPunishmentModule.amount;
            return Const.IntValue.Zero;
        }
    }
    public virtual float BattleRoyaleEntryFee
    {
        get
        {
            return TryGetEntryRequirement(out Requirement_Currency coinEntry, coinEntry => coinEntry.currencyType == CurrencyType.Standard) ? coinEntry.requiredAmountOfCurrency : 0;
        }
        set
        {
            if (TryGetEntryRequirement(out Requirement_Currency coinEntry, coinEntry => coinEntry.currencyType == CurrencyType.Standard))
            {
                coinEntry.requiredAmountOfCurrency = value;
            }
        }
    }
    public virtual float BattleRoyaleCoinBetPool => BattleRoyaleEntryFee * m_ModeConfigs[Mode.Battle].numOfContestant;
    #endregion

    protected virtual void OnEnable()
    {
        if (TryGetReward(out RandomGachaPackRewardModule rewardModule))
        {
            rewardModule.resourceLocationProvider = this;
        }
    }

    public virtual string GetItemId()
    {
        return Const.ResourceItemId.PvPBox;
    }

    public virtual ResourceLocation GetLocation()
    {
        return ResourceLocation.Box;
    }

    public virtual float GetBattleRoyaleCoinReward(int rank)
    {
        if (rank >= 1 && rank <= m_PricePoolDistribute.Count)
        {
            return m_PricePoolDistribute[rank - 1];
        }
        return 0;
    }



#if UNITY_EDITOR
    [OnInspectorGUI, PropertyOrder(100)]
    private void OnInspectorGUI()
    {
        var centerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.LabelField("=============== PlayerPref Variables ===============", centerStyle);
        CurrentTierIndex = EditorGUILayout.IntField($"{nameof(CurrentTierIndex)}_Normal", CurrentTierIndex);
        CurrentBattleTierIndex = EditorGUILayout.IntField($"{nameof(CurrentTierIndex)}_Battle", CurrentBattleTierIndex);
        ScriptedBotsIndex = EditorGUILayout.IntField($"{nameof(ScriptedBotsIndex)}_Normal", ScriptedBotsIndex);
        BattleScriptedBotsIndex = EditorGUILayout.IntField($"{nameof(ScriptedBotsIndex)}_Battle", BattleScriptedBotsIndex);
        foreach (var stage in stages)
        {
            stage.ScriptedStageIndex = EditorGUILayout.IntField($"{nameof(stage.ScriptedStageIndex)}_{stage.GetMode().ToString()}", stage.ScriptedStageIndex);
        }
        EditorGUILayout.LabelField($"totalNumOfPlayedMatches: Normal-{totalNumOfPlayedMatches_Normal} | Battle-{totalNumOfPlayedMatches_Battle} | Total-{totalNumOfPlayedMatches}");
        EditorGUILayout.LabelField($"totalNumOfWonMatches: Normal-{totalNumOfWonMatches_Normal} | Battle-{totalNumOfWonMatches_Battle} | Total-{totalNumOfWonMatches}");
        EditorGUILayout.LabelField($"totalNumOfLostMatches: Normal-{totalNumOfLostMatches_Normal} | Battle-{totalNumOfLostMatches_Battle} | Total-{totalNumOfLostMatches}");
        EditorGUILayout.LabelField($"totalNumOfAbandonedMatches: Normal-{totalNumOfAbandonedMatches_Normal} | Battle-{totalNumOfAbandonedMatches_Battle} | Total-{totalNumOfAbandonedMatches}");
        EditorGUILayout.LabelField("===================================================", centerStyle);
    }

    protected override void OnValidate()
    {
        foreach (var container in stages)
        {
            if (container.arenaSO == null)
            {
                container.arenaSO = this;
                EditorUtility.SetDirty(this);
            }
        }
    }

    private bool IsBattlePricePoolPercentagesInvalid()
    {
        return !Mathf.Approximately(m_PricePoolDistribute.Sum(), BattleRoyaleCoinBetPool);
    }
#endif
}

[Serializable]
public class MatchConfig
{
    [SerializeField]
    private float m_MatchTime;
    [SerializeField]
    private int m_NumOfContestant;

    public float matchTime => m_MatchTime;
    public int numOfContestant => m_NumOfContestant;
}
[Serializable]
public class BattleScriptedBot
{
    [ListDrawerSettings(IsReadOnly = true, ShowIndexLabels = true, ListElementLabelName = "chassisSO")]
    public ScriptedBot[] bots = new ScriptedBot[5];
}

[Serializable]
public struct ScriptedBot
{
    // [Title("@GetChassisName()", titleAlignment: TitleAlignments.Centered)]
    [BoxGroup]
    public PBChassisSO chassisSO;
    [ShowIf("@Check(PBPartSlot.Wheels_1)"), BoxGroup]
    public PBPartSO wheel_1;
    [ShowIf("@Check(PBPartSlot.Wheels_2)"), BoxGroup]
    public PBPartSO wheel_2;
    [ShowIf("@Check(PBPartSlot.Upper_1)"), BoxGroup]
    public PBPartSO upper_1;
    [ShowIf("@Check(PBPartSlot.Upper_2)"), BoxGroup]
    public PBPartSO upper_2;
    [ShowIf("@Check(PBPartSlot.Front_1)"), BoxGroup]
    public PBPartSO front;
    bool Check(PBPartSlot partSlot)
    {
        return this.chassisSO && this.chassisSO.AllPartSlots.FindAll(item => item.PartSlotType == partSlot).Count > 0;
    }
}

[Serializable]
public class BattleBuffInfo
{
    public RangeIntValue BotNumbers;
    public RangeFloatValue BuffRatio;
}

[Serializable]
public class StageContainer
{
    [SerializeField] Mode mode;
    [SerializeField] List<PBPvPStage> scriptedStageList;
    [SerializeField] List<PBPvPStage> randomStageList;
    [ReadOnly] public PBPvPArenaSO arenaSO;

    public virtual int ScriptedStageIndex
    {
        get
        {
            return PlayerPrefs.GetInt($"{arenaSO.guid}_ScriptedStageIndex_{mode}", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{arenaSO.guid}_ScriptedStageIndex_{mode}", value);
        }
    }

    public List<PBPvPStage> RandomStageList { get => randomStageList; }

    public Mode GetMode()
    {
        return mode;
    }

    public PBFightingStage GetCurrentFightingStagePrefab()
    {
        if (ScriptedStageIndex < scriptedStageList.Count)
        {
            return scriptedStageList[ScriptedStageIndex++].GetStagePrefab().GetComponent<PBFightingStage>();
        }
        else
        {
            return randomStageList.GetRandom().GetStagePrefab().GetComponent<PBFightingStage>();
        }
    }
}