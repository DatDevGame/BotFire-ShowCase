using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using static PBPvPArenaSO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class ArenaData
{
    /// *******************************************************************************************************
    ///                                            General data
    /// *******************************************************************************************************

    [TableColumnWidth(50, false)]
    public int numOfRounds;
    // [TableColumnWidth(50, false)]
    // public int wonNumOfCoins;
    [TableColumnWidth(50, false)]
    public int wonNumOfTrophies;
    [TableColumnWidth(50, false)]
    public int lostNumOfTrophies;
    [TableColumnWidth(50, false)]
    public int requiredNumOfTrophiesToUnlock;

    // /// *******************************************************************************************************
    // ///                                            Matchmaking profile
    // /// *******************************************************************************************************
    // [FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Normal")]
    // public MatchmakingProfileTable profileTable_Normal;
    // [FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Normal")]
    // public MatchmakingProfileTable profileTable_PlayerBias;
    // [FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Normal")]
    // public MatchmakingProfileTable profileTable_OpponentBias;
    // [FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Battle")]
    // public MatchmakingProfileTable profileTable_Normal_Battle;
    // [FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Battle")]
    // public MatchmakingProfileTable profileTable_PlayerBias_Battle;
    // [FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Battle")]
    // public MatchmakingProfileTable profileTable_OpponentBias_Battle;
    // [FoldoutGroup("Profile Table"), TabGroup("Profile Table/Profile Table Group", "Battle")]
    // public BattleBuffInfo battleBuffInfo;

    /// *******************************************************************************************************
    ///                                            Matchmaking parts
    /// *******************************************************************************************************
    [FoldoutGroup("Match Making Parts")]
    public List<PBChassisSO> chassisParts;
    [FoldoutGroup("Match Making Parts")]
    public List<PBPartSO> upperParts;
    [FoldoutGroup("Match Making Parts")]
    public List<PBPartSO> frontParts;
    [FoldoutGroup("Match Making Parts")]
    public List<PBWheelSO> wheelParts;
    [FoldoutGroup("Match Making Parts")]
    public List<PBChassisSO> specialBots;

    // /// *******************************************************************************************************
    // ///                                            Single & Battle DDA
    // /// *******************************************************************************************************
    // [FoldoutGroup("Match Making Decoding Number"), TabGroup("Match Making Decoding Number/Decoding Number Group", "Normal")]
    // public MatchmakingRawInputData matchmakingInputArr2D_Normal;
    // [FoldoutGroup("Match Making Decoding Number"), TabGroup("Match Making Decoding Number/Decoding Number Group", "Normal")]
    // public int thresholdNumOfLostMatchesInColumn_Normal;
    // [FoldoutGroup("Match Making Decoding Number"), TabGroup("Match Making Decoding Number/Decoding Number Group", "Normal")]
    // public int thresholdNumOfWonMatchesInColumn_Normal;
    // [FoldoutGroup("Match Making Decoding Number"), TabGroup("Match Making Decoding Number/Decoding Number Group", "Battle")]
    // public MatchmakingRawInputData matchmakingInputArr2D_Battle;
    // [FoldoutGroup("Match Making Decoding Number"), TabGroup("Match Making Decoding Number/Decoding Number Group", "Battle")]
    // public int thresholdNumOfLostMatchesInColumn_Battle;
    // [FoldoutGroup("Match Making Decoding Number"), TabGroup("Match Making Decoding Number/Decoding Number Group", "Battle")]
    // public int thresholdNumOfWonMatchesInColumn_Battle;

    /// *******************************************************************************************************
    ///                                            Scripted matches & bots
    /// *******************************************************************************************************
    // [FoldoutGroup("Hard Coded Matches"), TabGroup("Hard Coded Matches/Hard Coded Matches Group", "Normal")]
    // public int numOfHardCodedMatches;
    // [FoldoutGroup("Hard Coded Matches"), TabGroup("Hard Coded Matches/Hard Coded Matches Group", "Normal")]
    // public PvPArenaTierInput hardCodedInput;
    // [FoldoutGroup("Hard Coded Matches"), TabGroup("Hard Coded Matches/Hard Coded Matches Group", "Battle")]
    // public int numOfHardCodedMatchesBattle;
    // [FoldoutGroup("Hard Coded Matches"), TabGroup("Hard Coded Matches/Hard Coded Matches Group", "Battle")]
    // public List<PvPArenaBattleTierInput> hardCodedBattleInput;
    // [FoldoutGroup("Scripted Bots"), TabGroup("Scripted Bots/Scripted Bots Group", "Normal")]
    // public List<ScriptedBot> scriptedBots;
    // [FoldoutGroup("Scripted Bots"), TabGroup("Scripted Bots/Scripted Bots Group", "Battle"), ListDrawerSettings(NumberOfItemsPerPage = 3)]
    // public List<BattleScriptedBot> battleScriptedBots;

    /// *******************************************************************************************************
    ///                                            Scripted boxes
    /// *******************************************************************************************************
    [FoldoutGroup("Scripted Boxes")]
    public PBScriptedGachaPacks scriptedGachaBoxes;

    public void RetrieveDataFromArena(PBPvPArenaSO arenaSO)
    {
        // Let unity do the job
        arenaSO = UnityEngine.Object.Instantiate(arenaSO);
        // General data
        numOfRounds = arenaSO.NumOfRounds;
        // wonNumOfCoins = arenaSO.WonNumOfCoins;
        wonNumOfTrophies = arenaSO.WonNumOfTrophies;
        lostNumOfTrophies = arenaSO.LostNumOfTrophies;
        requiredNumOfTrophiesToUnlock = arenaSO.RequiredNumOfTrophiesToUnlock;
        // // Matchmaking profile
        // profileTable_Normal = arenaSO.profileTableNormal;
        // profileTable_PlayerBias = arenaSO.profileTablePlayerBias;
        // profileTable_OpponentBias = arenaSO.profileTableOpponentBias;
        // profileTable_Normal_Battle = arenaSO.profileTableNormal_Battle;
        // profileTable_PlayerBias_Battle = arenaSO.profileTablePlayerBias_Battle;
        // profileTable_OpponentBias_Battle = arenaSO.profileTableOpponentBias_Battle;
        // battleBuffInfo = arenaSO.BattleBuffInfo;
        // Matchmaking parts
        chassisParts = arenaSO.ChassisParts;
        upperParts = arenaSO.UpperParts;
        frontParts = arenaSO.FrontParts;
        wheelParts = arenaSO.WheelParts;
        specialBots = arenaSO.SpecialBots;
        // // Single & Battle DDA
        // matchmakingInputArr2D_Normal = arenaSO.MatchmakingInputArr2D_Normal;
        // thresholdNumOfLostMatchesInColumn_Normal = arenaSO.ThresholdNumOfLostMatchesInColumn_Normal;
        // thresholdNumOfWonMatchesInColumn_Normal = arenaSO.ThresholdNumOfWonMatchesInColumn_Normal;
        // matchmakingInputArr2D_Battle = arenaSO.MatchmakingInputArr2D_Battle;
        // thresholdNumOfLostMatchesInColumn_Battle = arenaSO.ThresholdNumOfLostMatchesInColumn_Battle;
        // thresholdNumOfWonMatchesInColumn_Battle = arenaSO.ThresholdNumOfWonMatchesInColumn_Battle;
        // Scripted matches & bots
        // numOfHardCodedMatches = arenaSO.NumOfHardCodedMatches;
        // hardCodedInput = arenaSO.HardCodedInput;
        // numOfHardCodedMatchesBattle = arenaSO.NumOfHardCodedMatchesBattle;
        // hardCodedBattleInput = arenaSO.HardCodedBattleInput;
        // scriptedBots = arenaSO.ScriptedBots;
        // battleScriptedBots = arenaSO.BattleScriptedBots;
        // Scripted boxes
        scriptedGachaBoxes = arenaSO.ScriptedGachaBoxes;
        UnityEngine.Object.DestroyImmediate(arenaSO);
    }

    public void RetrieveDataFromArena(PBEmptyPvPArenaSO arenaSO)
    {
        // General data
        numOfRounds = arenaSO.NumOfRounds;
        // wonNumOfCoins = arenaSO.WonNumOfCoins;
        wonNumOfTrophies = arenaSO.WonNumOfTrophies;
        lostNumOfTrophies = arenaSO.LostNumOfTrophies;
        requiredNumOfTrophiesToUnlock = arenaSO.RequiredNumOfTrophiesToUnlock;
    }

    public void InjectDataToArena(PBPvPArenaSO arenaSO)
    {
        // General data
        arenaSO.NumOfRounds = numOfRounds;
        // arenaSO.WonNumOfCoins = wonNumOfCoins;
        arenaSO.WonNumOfTrophies = wonNumOfTrophies;
        arenaSO.LostNumOfTrophies = lostNumOfTrophies;
        arenaSO.RequiredNumOfTrophiesToUnlock = requiredNumOfTrophiesToUnlock;
        // // Matchmaking profile
        // arenaSO.profileTableNormal = profileTable_Normal;
        // arenaSO.profileTablePlayerBias = profileTable_PlayerBias;
        // arenaSO.profileTableOpponentBias = profileTable_OpponentBias;
        // arenaSO.profileTableNormal_Battle = profileTable_Normal_Battle;
        // arenaSO.profileTablePlayerBias_Battle = profileTable_PlayerBias_Battle;
        // arenaSO.profileTableOpponentBias_Battle = profileTable_OpponentBias_Battle;
        // arenaSO.BattleBuffInfo = battleBuffInfo;
        // Matchmaking parts
        arenaSO.ChassisParts = chassisParts;
        arenaSO.UpperParts = upperParts;
        arenaSO.FrontParts = frontParts;
        arenaSO.WheelParts = wheelParts;
        arenaSO.SpecialBots = specialBots;
        // // Single & Battle DDA
        // arenaSO.MatchmakingInputArr2D_Normal = matchmakingInputArr2D_Normal;
        // arenaSO.ThresholdNumOfLostMatchesInColumn_Normal = thresholdNumOfLostMatchesInColumn_Normal;
        // arenaSO.ThresholdNumOfWonMatchesInColumn_Normal = thresholdNumOfWonMatchesInColumn_Normal;
        // arenaSO.MatchmakingInputArr2D_Battle = matchmakingInputArr2D_Battle;
        // arenaSO.ThresholdNumOfLostMatchesInColumn_Battle = thresholdNumOfLostMatchesInColumn_Battle;
        // arenaSO.ThresholdNumOfWonMatchesInColumn_Battle = thresholdNumOfWonMatchesInColumn_Battle;
        // Scripted matches & bots
        // arenaSO.NumOfHardCodedMatches = numOfHardCodedMatches;
        // arenaSO.HardCodedInput = hardCodedInput;
        // arenaSO.NumOfHardCodedMatchesBattle = numOfHardCodedMatchesBattle;
        // arenaSO.HardCodedBattleInput = hardCodedBattleInput;
        // arenaSO.ScriptedBots = scriptedBots;
        // arenaSO.BattleScriptedBots = battleScriptedBots;
        // Scripted boxes
        arenaSO.ScriptedGachaBoxes = scriptedGachaBoxes;
    }

    public void InjectDataToArena(PBEmptyPvPArenaSO arenaSO)
    {
        // General data
        arenaSO.NumOfRounds = numOfRounds;
        // arenaSO.WonNumOfCoins = wonNumOfCoins;
        arenaSO.WonNumOfTrophies = wonNumOfTrophies;
        arenaSO.LostNumOfTrophies = lostNumOfTrophies;
        arenaSO.RequiredNumOfTrophiesToUnlock = requiredNumOfTrophiesToUnlock;
    }
}
[Serializable]
public class ArenaDataTierGroup
{
    [TableList]
    public List<ArenaData> tiers = new List<ArenaData>(new ArenaData[4]);
}
[CreateAssetMenu(fileName = "ABTestArenaSO", menuName = "PocketBots/ABTest/ABTestArenaSO")]
public class ABTestArenaSO : GroupBasedABTestSO
{
    [SerializeField]
    private PBEmptyPvPArenaSO emptyArenaSO;
    [SerializeField]
    private PBPvPTournamentSO tournamentSO;
    [SerializeField]
    private List<ArenaDataTierGroup> arenaDataTierGroups;

    public override void InjectData(int groupIndex)
    {
        for (int i = 0; i < tournamentSO.arenas.Count; i++)
        {
            var arenaSO = tournamentSO.arenas[i].Cast<PBPvPArenaSO>();
            arenaDataTierGroups[i].tiers[groupIndex].InjectDataToArena(arenaSO);
        }
        arenaDataTierGroups[^1].tiers[groupIndex].InjectDataToArena(emptyArenaSO);
    }

#if UNITY_EDITOR
    // Editor Only
    public void RetrieveDataFromArena(PBPvPArenaSO arenaSO, int groupIndex)
    {
        arenaDataTierGroups[arenaSO.index].tiers[groupIndex] = new ArenaData();
        arenaDataTierGroups[arenaSO.index].tiers[groupIndex].RetrieveDataFromArena(arenaSO);
        EditorUtility.SetDirty(this);
    }
    // Editor Only
    public void RetrieveDataFromArena(PBEmptyPvPArenaSO arenaSO, int groupIndex)
    {
        arenaDataTierGroups[^1].tiers[groupIndex] = new ArenaData();
        arenaDataTierGroups[^1].tiers[groupIndex].RetrieveDataFromArena(arenaSO);
        EditorUtility.SetDirty(this);
    }
#endif
}