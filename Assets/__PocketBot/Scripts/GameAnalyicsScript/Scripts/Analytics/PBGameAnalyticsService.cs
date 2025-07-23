using System.Collections.Generic;
using UnityEngine;
using LatteGames.Analytics;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using GameAnalyticsSDK;

#if LatteGames_GA
using GameAnalyticsSDK;
#endif
[OptionalDependency("GameAnalyticsSDK.GameAnalytics", "LatteGames_GA")]
public class PBGameAnalyticsService : AnalyticsService,
    PBAnalyticsEvents.ResourceEvent.ILogger,
    PBAnalyticsEvents.AdEvents.ILogger,
    PBAnalyticsEvents.IAPEvent.ILogger,
    PBAnalyticsEvents.FightingEvent.ILogger,
    PBAnalyticsEvents.PVPEvent.ILogger,
    PBAnalyticsEvents.FTUEEvent.ILogger,
    PBAnalyticsEvents.ClickButtonEvent.ILogger,
    PBAnalyticsEvents.UpgradableEvent.ILogger,
    PBAnalyticsEvents.TrophyRoadEvent.ILogger,
    PBAnalyticsEvents.BossEvent.ILogger,
    PBAnalyticsEvents.CharacterUIEvent.ILogger,
    PBAnalyticsEvents.MainUI.ILogger,
    PBAnalyticsEvents.DesignEvent.ILogger,
    PBAnalyticsEvents.ProgressionEvent.ILogger,
    PBAnalyticsEvents.PSDesignEvent.ILogger,
    PBAnalyticsEvents.PSProgressionEvent.ILogger

{

    private string groupName;
    public string GroupName => groupName;
#if LatteGames_GA
    private void Awake()
    {
        groupName = PBRemoteConfigManager.GetGroupName();
        GameAnalytics.Initialize();
    }
#endif
    public override void SendEventLog(string eventKey, Dictionary<string, object> additionData)
    {
        // Nothing
    }

    // Resource events
    #region ResourceEvent
    public void ConsumeResource(string itemType, string itemId, string resourceCurrency, float amount)
    {
#if LatteGames_GA
        GameAnalytics.NewResourceEvent(GAResourceFlowType.Sink, resourceCurrency, amount, itemType, $"{itemId}{groupName}");
#endif
    }

    public void AcquireResource(string itemType, string itemId, string resourceCurrency, float amount)
    {
#if LatteGames_GA
        GameAnalytics.NewResourceEvent(GAResourceFlowType.Source, resourceCurrency, amount, itemType, $"{itemId}{groupName}");
#endif
    }
    #endregion

    // Ad events
    #region AdEvent
    public void InterstitialAdShowed(AdsLocation location, params string[] parameters)
    {
#if LatteGames_GA
        GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.Interstitial, "UnityAds", string.Format(ToGAAdsLocation(location, parameters[0]), parameters));
#endif
    }

    public void RewardedAdCompleted(AdsLocation location, params string[] parameters)
    {
#if LatteGames_GA
        GameAnalytics.NewAdEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo, "UnityAds", string.Format(ToGAAdsLocation(location, parameters[0]), parameters));
#endif
    }

    public void CustomShowInterstitialAdCompleted(string adsPlacement)
    {
#if LatteGames_GA
        GameAnalytics.NewAdEvent(GAAdAction.Show, GAAdType.Interstitial, "UnityAds", adsPlacement);
#endif
    }
    public void CustomRewardReceivedInterstitialAdCompleted(string adsPlacement)
    {
#if LatteGames_GA
        GameAnalytics.NewAdEvent(GAAdAction.RewardReceived, GAAdType.Interstitial, "UnityAds", adsPlacement);
#endif
    }

    public void CustomRewardedAdCompleted(string adsPlacement)
    {
#if LatteGames_GA
        GameAnalytics.NewAdEvent(GAAdAction.RewardReceived, GAAdType.RewardedVideo, "UnityAds", adsPlacement);
#endif
    }

    public void DesignAdsEvent(string eventName)
    {
#if LatteGames_GA
        //GANewDesignEvent(eventName);
#endif
    }

    private string ToGAAdsLocation(AdsLocation location, string adsLocation)
    {
        return location switch
        {
            AdsLocation.RV_Multiplier_Rewards_Win_UI => $"RV{groupName}:MultiplierRewards:{adsLocation}",
            AdsLocation.RV_Revenge_Boss_Lose_Boss_UI => $"RV{groupName}:RevengeBoss:{adsLocation}",
            AdsLocation.RV_Revenge_Boss_Boss_Fight_UI => $"RV{groupName}:RevengeBoss:{adsLocation}",
            AdsLocation.RV_Claim_Boss_Win_Boss_UI => $"RV{groupName}:ClaimBoss:{adsLocation}",
            AdsLocation.RV_Claim_Boss_Inventory_UI => $"RV{groupName}:ClaimBoss:{adsLocation}",
            AdsLocation.RV_Free_Box_Main_UI => $"RV{groupName}:FreeBox:{adsLocation}",
            AdsLocation.RV_Upgrade_Card_Upgrade_Exchange_UI => $"RV{groupName}:Upgrade:{adsLocation}",
            AdsLocation.RV_Bonus_Card_Open_Box_UI => $"RV{groupName}:BonusCard:{adsLocation}",
            AdsLocation.RV_Speed_Up_Box_Slot_UI => $"RV{groupName}:SpeedupBox:{adsLocation}",
            AdsLocation.RV_Open_Now_Box_Slot_UI => $"RV{groupName}:OpenNowBox:{adsLocation}",
            AdsLocation.RV_Open_Now_Box_Slot_GameOver_UI => $"RV{groupName}:OpenNowBox:{adsLocation}",
            AdsLocation.RV_Recharge_Boss_Alert_Popup => $"RV{groupName}:RechargeBoss:{adsLocation}",
            AdsLocation.RV_Recharge_Boss_Build_UI => $"RV{groupName}:RechargeBoss:{adsLocation}",
            AdsLocation.RV_Recharge_Boss_Popup => $"RV{groupName}:RechargeBoss:{adsLocation}",
            AdsLocation.RV_Revive_Boss => $"RV{groupName}:Revive:{adsLocation}",
            AdsLocation.RV_Getskin => $"RV{groupName}:UnlockSkin:{adsLocation}",
            _ => "None"
        };
    }
    #endregion

    // IAP events
    #region IAPEvent
    public void IAPPurchaseComplete(string currency, int amount, string itemType, string itemId, string cartType, Dictionary<string, object> parameters)
    {
#if LatteGames_GA
        GameAnalytics.NewBusinessEvent($"{currency}", amount, itemType, itemId, cartType);
        GANewDesignEvent($"{currency}:{amount}:{itemType}:{itemId}:{cartType}");
#endif
    }

    public void CustomIAPPurchaseComplete(string productName, string type)
    {
        //GANewDesignEvent($"AppPurchased:{productName}:{type}");
    }
    #endregion

    // Fighting events
    #region FightingEvent
    public void LogSkillPerformedCount(int formIndex, int skillIndex, int count)
    {
        GANewDesignEvent($"SSkill:Form_{formIndex + 1}:SSkill_{skillIndex + 1}:{count}");
    }
    #endregion

    // PVP events
    #region PVPEvent
    public void UnlockPVP(string status, int arena, int daysSinceInstall, int valueTimeFollowingArena)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            "Complete" => GAProgressionStatus.Complete,
            "Fail" => GAProgressionStatus.Fail,
            _ => GAProgressionStatus.Complete,
        };
        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"UnlockPvP{groupName}", $"{arena + 1}");
    }

    public void UnlockBattlePvP(int daysSinceInstall)
    {
        //GANewDesignEvent($"UnlockBattlePvP{groupName}:{daysSinceInstall}");
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, $"UnlockBattlePvP{groupName}");
    }

    public void Match(int count, int daysSinceInstall)
    {
        //GANewDesignEvent($"Match{groupName}:{count}:{daysSinceInstall}");
    }

    public void LogPVPStatus(string status, int arena, int timesPlayed)
    {
        GANewDesignEvent($"PvP:{status}:Arena {arena + 1}:Times_{timesPlayed}");
    }

    public void LogSinglePVP(string status, int arena, string combination, int timesPlayed, int timeEndRound)
    {
        //GANewDesignEvent($"SinglePvP{groupName}:{status}:{arena + 1}:Stage_{stageName}:Times_{timesPlayed}", timeEndRound);
        //GANewDesignEvent($"SinglePvP{groupName}:{status}:{arena + 1}:{combination}:Times_{timesPlayed}", timeEndRound);

        GAProgressionStatus statusProgression = status switch
        {
            "Start" => GAProgressionStatus.Start,
            "Victory" => GAProgressionStatus.Complete,
            "Defeated" => GAProgressionStatus.Fail,
            "Abandon" => GAProgressionStatus.Fail,
            _ => GAProgressionStatus.Undefined
        };
        GameAnalytics.NewProgressionEvent(statusProgression, $"SinglePvP{groupName}", $"{arena + 1}", $"Times_{timesPlayed}", timeEndRound);
    }

    public void LogPVPVictoryOrDefeated(string status, int arena, int timesPlayed, int timeEndRound)
    {
#if LatteGames_GA
        GameAnalytics.NewDesignEvent($"PvP:{status}:Arena {arena + 1}:Times_{timesPlayed}", timeEndRound);
#endif

    }
    public void LogBattleStatus(string status, int arena, string combination, int time, float timePlayed)
    {
        //GANewDesignEvent($"BattlePvP{groupName}:{status}:{arena}:{combination}:Times_{time}", timePlayed);
    }
    public void LogBattleInteraction(int arena, int playerTop, int times, int interactionMilestones)
    {
        //GANewDesignEvent($"BattleInteractionTime:{arena}:Top_{playerTop}:Times_{times}:{interactionMilestones}");
    }
    public void LogPVPPlayerKillBattleMode(int arena, string stageName, int killernumber, int timeEndRound)
    {
        //GANewDesignEvent($"BattlePvP_Top1{groupName}:{arena + 1}:Stage_{stageName}:Kill_{killernumber}", timeEndRound);
    }

    public void LogAIProfile(string profileName, string advantageState, string status)
    {
        //GANewDesignEvent($"AIProfile{groupName}:{profileName}_{advantageState}:{status}");
    }

    public void Streak(string status, int count)
    {
        //GANewDesignEvent($"Streak{GroupName}:{status}:Times_{count}");
    }

    public void Ranking(int rank, int daysSinceInstall)
    {
        GANewDesignEvent($"Ranking:{rank}:{daysSinceInstall}");
    }

    public void PlayWithoutAttack()
    {
        GANewDesignEvent($"PlayWithNoAttack{groupName}");
    }

    public void PlayWithFullSlot(string mode)
    {
        //GANewDesignEvent($"PlayWithFullSlot{groupName}:{mode}");
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, $"PlayWithFullSlot{groupName}", $"{mode}");
    }

    public void LogPVPBotDeadth(string mode, string stageName, string whoDie, string causeOfDeath)
    {
        //GANewDesignEvent($"BotDeath:{mode}:Stage_{stageName}:{whoDie}:{causeOfDeath}");
    }

    public void LogQuitCheck(string mode, string stageName, string mainPhase, string subPhase)
    {
        //GANewDesignEvent($"QuitCheck:{mode}:Stage_{stageName}:{mainPhase}:{subPhase}");
    }

    public void LogFlipTiming(int currentArena, string status)
    {
        //GANewDesignEvent($"FlipTiming{groupName}:{arenaIndex}:{status}");
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            "Complete" => GAProgressionStatus.Complete,
            "Fail" => GAProgressionStatus.Fail,
            _ => GAProgressionStatus.Undefined,
        };
        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"FlipTiming{groupName}", $"{currentArena}");
    }
    #endregion

    // FTUE events
    #region FTUEEvent
    public void LogFTUEEvent(int status, string type, string step)
    {
#if LatteGames_GA
        if (PlayerPrefs.HasKey($"{status}{type}{step}")) return;

        GAProgressionStatus handleStatus = status == 0 ? GAProgressionStatus.Start : GAProgressionStatus.Complete;
        GameAnalytics.NewProgressionEvent(handleStatus, $"FTUE{groupName}", type, step);
#endif
    }
    #endregion

    // Clicking button events
    #region ClickButtonEvent
    public void ClickButton(string buttonName, int clickTimes)
    {
        //GANewDesignEvent($"Click:{buttonName}:Times_{clickTimes}");
    }
    #endregion

    // Upgradable events
    #region UpgradableEvent
    public void Upgrade(string type, string name, int formIndex, int upgradeStepIndex, int currentArena)
    {
        //GANewDesignEvent($"Upgrade{groupName}:{currentArena}:{name}:Complete:UpgradeStep_{upgradeStepIndex + 1}");
    }

    public void LogCalcUpgrade(string name, int upgradeStepIndex, int currentArena)
    {
        //string key = $"LogCalcUpgrade{name}{upgradeStepIndex}";
        //if (!PlayerPrefs.HasKey(key))
        //{
        //    GANewDesignEvent($"Upgrade{groupName}:{currentArena}:{name}:Start:UpgradeStep_{upgradeStepIndex}");
        //    PlayerPrefs.SetInt(key, 1);
        //}
    }

    public void Owned(string name, string status, int count, int currentMilestone, int currentArena)
    {
        if (!PlayerPrefs.HasKey(name + status))
        {
            PlayerPrefs.SetString(name + status, "");
            //GANewDesignEvent($"Upgrade{groupName}:{currentArena}:{name}:Start:UpgradeStep_1");
            //GANewDesignEvent($"Upgrade{groupName}:{currentArena}:{name}:Complete:UpgradeStep_1");
        }

    }
    #endregion

    // TrophyRoad events
    #region TrophyRoadEvent
    public void ReachMilestone(string status, int trophyMilestonesID, int daysSinceInstall, int valueTimeFollowingMistone)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            "Complete" => GAProgressionStatus.Complete,
            "Fail" => GAProgressionStatus.Fail,
            _ => GAProgressionStatus.Complete,
        };
        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"TrophyRoad{groupName}", $"{trophyMilestonesID}");
    }
    #endregion

    //BossFighting
    #region BossFightingEvent
    public void LogBossFight(string status, int currentArena, int bossID, string overallScore, float timeCompletedMatch)
    {
        //GANewDesignEvent($"BossFight{groupName}:A{arenaIndex}:Boss{bossID}:{status}:{overallScore}", timeCompletedMatch);
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            "Complete" => GAProgressionStatus.Complete,
            "Fail" => GAProgressionStatus.Fail,
            _ => GAProgressionStatus.Undefined,
        };
        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"BossFight{groupName}", $"A{currentArena}", $"Boss{bossID}");
    }

    public void LogPlayWithBoss(int arenaCurrent, string mode, string specialBot)
    {
        //GANewDesignEvent($"PlayWithBoss{groupName}:Boss{bossID}");
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, $"PlayWithBoss{groupName}", $"A{arenaCurrent}-{mode}", specialBot);
    }

    public void LogBossFightStreak(int bossID, int streak)
    {
        GANewDesignEvent($"BossFightStreak{groupName}:Boss{bossID}:Times_{streak}");
    }

    public void LogRevivePopup(string bossName, string status)
    {
        //GANewDesignEvent($"RevivePopup{groupName}:{bossName}:{status}");
    }
    #endregion

    //Character UI Event
    #region CharacterUIEvent
    public void LogFeatureUsed(string feature, int value)
    {
        //GANewDesignEvent($"FeatureUsed{groupName}:{feature}", value);
    }
    #endregion

    //Main UI
    #region Main UI
    public void LogOpenBox(int arenaIndex, int boxNumber, string openStatus, string location)
    {
        GANewDesignEvent($"OpenBox{groupName}:{arenaIndex}:{location}:{openStatus}");
    }

    public void LogPopupStarterPack(string popupName, string operation, string status)
    {
        GANewDesignEvent($"Popup{groupName}:{popupName}:{operation}:{status}");
    }

    public void LogFullBoxSlot()
    {
        //GANewDesignEvent($"FullSlot{groupName}");
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, $"FullSlot{groupName}");
    }

    public void LogCollectBox(int arenaIndex, string mode, string boxType, string slotState)
    {
        GANewDesignEvent($"CollectBox{groupName}:{arenaIndex}:{mode}:{boxType}:{slotState}");
    }

    public void LogBossOutEnergy(string bossName)
    {
        GANewDesignEvent($"BossOutEnergy{groupName}:{bossName}");
    }

    public void LogBossOutEnergyAlert(string bossName, string status)
    {
        GANewDesignEvent($"BossOutEnergyAlert{groupName}:{bossName}:{status}");
    }

    public void LogPowerDilemma(string popupName, string operation, string status)
    {
        GANewDesignEvent($"Popup{groupName}:{popupName}:{operation}:{status}");
    }

    public void LogPopupGroup(int arenaIndex, string popupName, string operation, string status)
    {
        GANewDesignEvent($"Popup{groupName}:{arenaIndex}:{popupName}:{operation}:{status}");
    }

    public void LogRVShow(int arenaIndex, string rvName, string location)
    {
        GANewDesignEvent($"RVShow{groupName}:A{arenaIndex}:{rvName}:{location}");
    }

    #endregion

    //Progression Event
    #region Progression Event
    public void ClaimBossProgressionEvent(string status, int bossID, string type)
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, $"ClaimBoss{groupName}", $"Boss{bossID}", $"{type}");
    }

    public void WinStreakProgressionEvent(string status, int milestoneIndex, int currentArena)
    {
        GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, $"WinStreak{groupName}", $"{milestoneIndex}", $"{currentArena}");
    }
    public void PiggyBankProgressionEvent(string status, int currentArena, int level)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            _ => GAProgressionStatus.Complete,
        };

        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"PiggyBank{groupName}", $"{currentArena}", $"{level}");
    }

    public void SeasonProgressionEvent(string status, int seasonID, int SeasonPassMilestonesID, int playedTimeToCompleteAMilestone)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            _ => GAProgressionStatus.Complete,
        };
        GANewDesignEvent($"Season{groupName}:{seasonID.ToString()}:{SeasonPassMilestonesID.ToString()}", playedTimeToCompleteAMilestone);
    }

    public void MissionRoundProgressionEvent(string status, int seasonID, int SeasonPassMilestonesID)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            _ => GAProgressionStatus.Complete,
        };

        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"MissionRound{groupName}", seasonID.ToString(), SeasonPassMilestonesID.ToString());
    }

    public void EnterSeasonTabProgressionEvent(string status, int currentArena, string missionStatus)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            _ => GAProgressionStatus.Complete,
        };

        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"EnterSeasonTab{groupName}", currentArena.ToString(), missionStatus);
    }

    public void BuySkinProgressionEvent(string status, string partname_SkinID, string currentcyType)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            _ => GAProgressionStatus.Complete,
        };

        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"BuySkin{groupName}", partname_SkinID, currentcyType);
    }
    public void LeaguePromotionProgressionEvent(string status, int weeklyID, int divisionID)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            "Complete" => GAProgressionStatus.Complete,
            "Fail" => GAProgressionStatus.Fail,
            _ => GAProgressionStatus.Complete,
        };

        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"LeaguePromotion{groupName}", weeklyID.ToString(), divisionID.ToString());
    }
    public void LogProgressionEvent(string status, string content)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            "Complete" => GAProgressionStatus.Complete,
            "Fail" => GAProgressionStatus.Fail,
            _ => GAProgressionStatus.Undefined,
        };

        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"Progression{groupName}", content);
    }
    public void SinkProgressionEvent(string status, int trophyMilestonesID, CurrencyType currencyType, int amount)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            _ => GAProgressionStatus.Start,
        };
        GANewDesignEvent($"Sink{groupName}:{trophyMilestonesID}:{currencyType}", amount);
    }

    public void SourceProgressionEvent(string status, int trophyMilestonesID, CurrencyType currencyType, int amount)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            _ => GAProgressionStatus.Start,
        };
        GANewDesignEvent($"Source{groupName}:{trophyMilestonesID}:{currencyType}", amount);
    }

    public void StageProgressionEvent(string status, string stageID, string difficulty, int playedTime)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            "Complete" => GAProgressionStatus.Complete,
            "Fail" => GAProgressionStatus.Fail,
            _ => GAProgressionStatus.Start,
        };
        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"StageGroup{groupName}", stageID, difficulty, playedTime);
    }
    public void LadderedOfferProgressionEvent(string status, string set, string pack)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            _ => GAProgressionStatus.Start,
        };
        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"LadderedOffer{GroupName}", set, pack);
    }
    #endregion

    #region Design Event
    public void HotOfferBuy(int arenaIndex, string currency, string offer)
    {
        GANewDesignEvent($"HotOffersBuy{groupName}:A{arenaIndex}:{currency}:{offer}");
    }

    public void BossFightUnlock(int bossID)
    {
        GANewDesignEvent($"BossFightUnlock{groupName}:Boss{bossID}");
    }

    public void BattlePvPChooseArena(int currentArena, int arenaChoosen)
    {
        //GANewDesignEvent($"BattlePvPChooseArena{groupName}:{currentArena}:{arenaChoosen}");
    }
    public void BodyUsedDesignEvent(int trophyMilestonesID, string mode, string bodyName)
    {
        GANewDesignEvent($"BodyUsed{groupName}:{trophyMilestonesID}:{mode}:{bodyName}");
    }
    public void FrontWeaponUsedDesignEvent(int trophyMilestonesID, string mode, string frontName)
    {
        GANewDesignEvent($"FrontWeaponUsed{groupName}:{trophyMilestonesID}:{mode}:{frontName}");
    }
    public void UpperWeaponUsedDesignEvent(int trophyMilestonesID, string mode, string upperName)
    {
        GANewDesignEvent($"UpperWeaponUsed{groupName}:{trophyMilestonesID}:{mode}:{upperName}");
    }
    public void LeagueRankDesignEvent(int weekID, string divisionID, int rankRange, int playedTimeToReachRank)
    {
        GANewDesignEvent($"LeagueRank{groupName}:{weekID}:{divisionID}:{rankRange}", playedTimeToReachRank);
    }
    public void PreludeMissionDesignEvent(string status, int seasonID, string missionName, string missionDifficulty)
    {
        GANewDesignEvent($"PreludeMission{groupName}:{seasonID}:{status}:{missionName}:{missionDifficulty}");
    }
    public void TodayMissionDesignEvent(string status, int seasonID, string missionID, string missionDifficulty)
    {
        GANewDesignEvent($"TodayMission{groupName}:{seasonID}:{status}:{missionID}:{missionDifficulty}");
    }
    public void WeeklyMissionDesignEvent(string status, int seasonID, string missionID, string missionDifficulty)
    {
        GANewDesignEvent($"WeeklyMission{groupName}:{seasonID}:{status}:{missionID}:{missionDifficulty}");
    }
    public void SeasonMissionDesignEvent(string status, int seasonID, string missionID, string missionDifficulty)
    {
        GANewDesignEvent($"SeasonMission{groupName}:{seasonID}:{status}:{missionID}:{missionDifficulty}");
    }

    public void SkillUsageDuelEquipedDesignEvent(string status, int milestone, string skillName, string isOpponentUseSkill, int value)
    {
        GANewDesignEvent($"SkillUsage_Duel{groupName}:{status}:{milestone}:{skillName}:{isOpponentUseSkill}", value);
    }
    public void SkillUsageDuelUsedDesignEvent(string status, int milestone, string skillName, string isOpponentUseSkill, int value)
    {
        GANewDesignEvent($"SkillUsage_Duel{groupName}:{status}:{milestone}:{skillName}:{isOpponentUseSkill}", value);
    }
    public void SkillUsageBattleEquipedDesignEvent(string status, int milestone, string skillName, string isOpponentUseSkill, int value)
    {
        GANewDesignEvent($"SkillUsage_Battle{groupName}:{status}:{milestone}:{skillName}:{isOpponentUseSkill}", value);
    }
    public void SkillUsageBattleUsedDesignEvent(string status, int milestone, string skillName, string isOpponentUseSkill, int value)
    {
        GANewDesignEvent($"SkillUsage_Battle{groupName}:{status}:{milestone}:{skillName}:{isOpponentUseSkill}", value);
    }
    public void MissedEquipSkillDesignEvent(int milestone)
    {
        GANewDesignEvent($"MissedEquipSkill{groupName}:{milestone}");
    }
    #endregion

    private void GANewDesignEvent(string eventName)
    {
#if LatteGames_GA
        GameAnalytics.NewDesignEvent(eventName);
#endif
    }

    private void GANewDesignEvent(string eventName, float value)
    {
#if LatteGames_GA
        GameAnalytics.NewDesignEvent(eventName, value);
#endif
    }

    //PSEvent

    #region DesignEvent
    public void TeamDeathMatchSurrender(int arena, int matchCount, int score)
    {
        GANewDesignEvent($"TeamDeathMatchStats{groupName}:Surrender:{arena}:SurrenderCount:Times_{matchCount}", score);
    }
    public void TeamDeathMatchUserKillCount(string status, int arena, int killCount, int matchCount)
    {
        GANewDesignEvent($"TeamDeathMatchStats{groupName}:{status}:{arena}:UserKillCount:Times_{matchCount}", killCount);
    }
    public void TeamDeathMatchTeamKillCount(string status, int arena, int killCount, int matchCount)
    {
        GANewDesignEvent($"TeamDeathMatchStats{groupName}:{status}:{arena}:TeamKillCount:Times_{matchCount}", killCount);
    }
    public void TeamDeathMatchUserAssistCount(string status, int arena, int assistCount, int matchCount)
    {
        GANewDesignEvent($"TeamDeathMatchStats{groupName}:{status}:{arena}:UserAssistCount:Times_{matchCount}", assistCount);
    }
    public void TeamDeathMatchTeamAssistCount(string status, int arena, int assistCount, int matchCount)
    {
        GANewDesignEvent($"TeamDeathMatchStats{groupName}:{status}:{arena}:TeamAssistCount:Times_{matchCount}", assistCount);
    }
    public void TeamDeathMatchUserDeathCount(string status, int arena, int deathCount, int matchCount)
    {
        GANewDesignEvent($"TeamDeathMatchStats{groupName}:{status}:{arena}:UserDeathCount:Times_{matchCount}", deathCount);
    }
    public void TeamDeathMatchTeamDeathCount(string status, int arena, int deathCount, int matchCount)
    {
        GANewDesignEvent($"TeamDeathMatchStats{groupName}:{status}:{arena}:TeamDeathCount:Times_{matchCount}", deathCount);
    }

    public void PSUpgrade(string status, string itemName,int arena, int upgradeStepIndex)
    {
        GANewDesignEvent($"Upgrade{groupName}:{arena}:{itemName}:{status}:UpgradeStep_{upgradeStepIndex}");
    }
    public void UnlockNewItem(int arena, string itemType, string itemName)
    {
        GANewDesignEvent($"UnlockNewItem{groupName}:{arena}:{itemType}:{itemName}");
    }
    public void PlayWithUnequippedSlot(int unuquippedSlotCount)
    {
        GANewDesignEvent($"PlayWithUnequippedSlot{groupName}:{unuquippedSlotCount}");
    }
    #endregion

    #region Progression Event
    public void TeamDeathmatch(string status, int arena, int time, int score)
    {
        GAProgressionStatus gAProgressionStatus = status switch
        {
            "Start" => GAProgressionStatus.Start,
            "Complete" => GAProgressionStatus.Complete,
            "Fail" => GAProgressionStatus.Fail,
            _ => GAProgressionStatus.Undefined,
        };

        GameAnalytics.NewProgressionEvent(gAProgressionStatus, $"TeamDeathmatch{groupName}", $"{arena}", $"Times_{time}", score);
    }
    #endregion
}