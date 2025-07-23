using System.Collections.Generic;
using UnityEngine;
using LatteGames.Monetization;
using GameAnalyticsSDK;
using HyrphusQ.Events;

namespace PBAnalyticsEvents
{
    /// <summary>
    /// Resource events
    /// </summary>
    public static class ResourceEvent
    {
        public interface ILogger
        {
            void ConsumeResource(string itemType, string itemId, string resourceCurrency, float amount);
            void AcquireResource(string itemType, string itemId, string resourceCurrency, float amount);
        }
        public static void ConsumeResource(this PBAnalyticsManager manager, string itemType, string itemId, string resourceCurrency, float amount)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.ConsumeResource(itemType, itemId, resourceCurrency, amount));
        }
        public static void AcquireResource(this PBAnalyticsManager manager, string itemType, string itemId, string resourceCurrency, float amount)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.AcquireResource(itemType, itemId, resourceCurrency, amount));
        }
    }

    /// <summary>
    /// Ad events
    /// </summary>
    public static class AdEvents
    {
        public interface ILogger
        {
            void InterstitialAdShowed(AdsLocation location, params string[] parameters);
            void RewardedAdCompleted(AdsLocation location, params string[] parameters);
            void CustomShowInterstitialAdCompleted(string adsPlacement);
            void CustomRewardReceivedInterstitialAdCompleted(string adsPlacement);
            void CustomRewardedAdCompleted(string adsPlacement);
            void DesignAdsEvent(string eventName);
        }
        public static void InterstitialAdShowed(this PBAnalyticsManager manager, AdsLocation location, params string[] parameters)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.InterstitialAdShowed(location, parameters));
        }
        public static void RewardedAdCompleted(this PBAnalyticsManager manager, AdsLocation location, params string[] parameters)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.RewardedAdCompleted(location, parameters));
        }
        public static void CustomShowInterstitialAdCompleted(this PBAnalyticsManager manager, string adsPlacement)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.CustomShowInterstitialAdCompleted(adsPlacement));
        }
        public static void CustomRewardReceivedInterstitialAdCompleted(this PBAnalyticsManager manager, string adsPlacement)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.CustomRewardReceivedInterstitialAdCompleted(adsPlacement));
        }
        public static void CustomRewardedAdCompleted(this PBAnalyticsManager manager, string adsPlacement)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.CustomRewardedAdCompleted(adsPlacement));
        }
        public static void DesignAdsEvent(this PBAnalyticsManager manager, string eventName)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.DesignAdsEvent(eventName));
        }
    }

    /// <summary>
    /// IAP events
    /// </summary>
    public static class IAPEvent
    {
        public interface ILogger
        {
            void IAPPurchaseComplete(string currency, int amount, string itemType, string itemId, string cartType, Dictionary<string, object> parameters);
            void CustomIAPPurchaseComplete(string productName, string type);
        }
        public static void IAPPurchaseComplete(this PBAnalyticsManager manager, string currency, int amount, string itemType, string itemId, string cartType, Dictionary<string, object> parameters)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.IAPPurchaseComplete(currency, amount, itemType, itemId, cartType, parameters));
            CustomIAPPurchaseComplete(manager, itemId, cartType);
        }
        public static void CustomIAPPurchaseComplete(this PBAnalyticsManager manager, string productName, string type)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.CustomIAPPurchaseComplete(productName, type));
        }
    }

    /// <summary>
    /// Fighting events
    /// </summary>
    public static class FightingEvent
    {
        public interface ILogger
        {
            void LogSkillPerformedCount(int formIndex, int skillIndex, int count);
        }
        /// <summary>
        /// Send how many times a skill is performed when the match is over
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="formIndex">Form index of the character</param>
        /// <param name="skillIndex"></param>
        /// <param name="count">The times a skill is peformed by skill index</param>
        public static void LogSkillPerformedCount(this PBAnalyticsManager manager, int formIndex, int skillIndex, int count)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogSkillPerformedCount(formIndex, skillIndex, count));
        }
    }

    /// <summary>
    /// PVP events
    /// </summary>
    public static class PVPEvent
    {
        public interface ILogger
        {
            void UnlockPVP(string status, int arena, int daysSinceInstall, int valueTimeFollowingArena);
            void UnlockBattlePvP(int daysSinceInstall);
            void Match(int count, int daysSinceInstall);
            void LogPVPStatus(string status, int arena, int timesPlayed);
            void LogSinglePVP(string status, int arena, string combination, int timesPlayed, int timeEndRound);
            void LogPVPVictoryOrDefeated(string status, int arena, int timesPlayed, int timeEndRound);
            void LogBattleInteraction(int arena, int playerTop, int times, int interactionMilestones);
            void LogPVPPlayerKillBattleMode(int arena, string stageName, int killerNumber, int timeEndRound);
            void LogAIProfile(string profileName, string advantageState, string status);
            void Streak(string status, int count);
            void Ranking(int rank, int daysSinceInstall);
            void PlayWithoutAttack();
            void PlayWithFullSlot(string mode);
            void LogPlayWithBoss(int arenaCurrent, string mode, string specialBot);
            void LogPVPBotDeadth(string mode, string stageName, string whoDie, string causeOfDeath);
            void LogQuitCheck(string mode, string stageName, string mainPhase, string subPhase);
            void LogBattleStatus(string status, int arena, string combination, int time, float timePlayed);
            void LogFlipTiming(int arenaIndex, string status);
        }
        /// <summary>
        /// Send this event when an arena unlocked
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="arena"></param>
        public static void UnlockPVP(this PBAnalyticsManager manager, string status, int arena, int valueTimeFollowingArena)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.UnlockPVP(status, arena, DaysSinceInstalledGettter.Get(), valueTimeFollowingArena));
        }
        /// <summary>
        /// Send this event when the battle mode is unlocked
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="arena"></param>
        public static void UnlockBattlePvP(this PBAnalyticsManager manager)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.UnlockBattlePvP(DaysSinceInstalledGettter.Get()));
        }
        /// <summary>
        /// Send this event when a match started, only count at 5, 10, 15, 20, etc
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="count">Only count at 5, 10, 15, 20, etc</param>
        public static void Match(this PBAnalyticsManager manager, int count)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.Match(count, DaysSinceInstalledGettter.Get()));
        }
        /// <summary>
        /// Send this event when a match status changed
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="status">Start, Victory, Defeated, Abandon</param>
        /// <param name="arena"></param>
        /// <param name="timesPlayed">Only count for this arena</param>
        public static void LogPVPStatus(this PBAnalyticsManager manager, string status, int arena, int timesPlayed)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogPVPStatus(status, arena, timesPlayed));
        }
        public static void LogSinglePVP(this PBAnalyticsManager manager, string status, int arena, string combination, int timesPlayed, int timeEndRound)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogSinglePVP(status, arena, combination, timesPlayed, timeEndRound));
        }

        /// <summary>
        /// Send this event when a match status changed
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="status">Start, Abandon, End</param>
        /// <param name="arena"></param>
        /// <param name="stageName"> Stage name</param>
        /// <param name="timesPlayed">Only count for this arena</param>
        public static void LogBattleStatus(this PBAnalyticsManager manager, string status, int arena, string combination, int time, float timePlayed)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogBattleStatus(status, arena, combination, time, timePlayed));
        }

        /// <summary>
        /// Send this event when a match status changed
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="status">Start, Victory, Defeated, Abandon</param>
        /// <param name="arena"></param>
        /// <param name="timesPlayed">Only count for this arena</param>
        public static void LogPVPVictoryOrDefeated(this PBAnalyticsManager manager, string status, int arena, int timesPlayed, int timeEndRound)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogPVPVictoryOrDefeated(status, arena, timesPlayed, timeEndRound));
        }
        /// <summary>
        /// Send this event when player completing a battle match
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="playerTop">Top of local player. Ex: Top 1, Top 2,...</param>
        /// <param name="arena"></param>
        /// <param name="times">Time to complete a match</param>
        /// <param name="interactionMilestones">Interaction time of the user in the match calculated as a percentage as percent</param>
        public static void LogBattleInteraction(this PBAnalyticsManager manager, int arena, int playerTop, int times, int interactionMilestones)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogBattleInteraction(arena, playerTop, times, interactionMilestones));
        }
        /// <summary>
        /// Send this event when player completing a battle match
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="arena"></param>
        /// <param name="KillerNumber">Count number kill by player</param>
        public static void LogPVPPlayerKillBattleMode(this PBAnalyticsManager manager, int arena, string stageName, int KillerNumber, int timeEndRound)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogPVPPlayerKillBattleMode(arena, stageName, KillerNumber, timeEndRound));
        }
        /// <summary>
        /// Send the AI profile when the match is over
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="profileName">The difficulty of the AI</param>
        /// <param name="advantageState">PlayerAdvantage, OpponentAdvantage, Even</param>
        /// <param name="status">Victory, Defeated, Abandon</param>
        public static void LogAIProfile(this PBAnalyticsManager manager, string profileName, string advantageState, string status)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogAIProfile(profileName, advantageState, status));
        }
        /// <summary>
        /// Send this event when the player is on a win/lose streak, only count form 2
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="status">Victory, Defeated</param>
        /// <param name="count">Only count form 2</param>
        public static void Streak(this PBAnalyticsManager manager, string status, int count)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.Streak(status, count));
        }
        /// <summary>
        /// Send this event when the player reaches one of these ranks for the first time:
        /// <para>210, 200, 150, 100, 50, 20, 10, 3, 2, 1</para>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="rank"></param>
        public static void Ranking(this PBAnalyticsManager manager, int rank)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.Ranking(rank, DaysSinceInstalledGettter.Get()));
        }

        public static void PlayWithoutAttack(this PBAnalyticsManager manager)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.PlayWithoutAttack());
        }

        /// <summary>
        /// Send this event when the player match not full slot weapons:
        /// </summary>
        /// <param name="mode">name mode</param>
        public static void PlayWithFullSlot(this PBAnalyticsManager manager, string mode)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.PlayWithFullSlot(mode));
        }

        /// <summary>
        /// Send this event when user use Boss in battle
        /// </summary>
        /// <param name="bossName">Name boss user use in battle</param>
        public static void LogPlayWithBoss(this PBAnalyticsManager manager, int arenaCurrent, string mode, string specialBot)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogPlayWithBoss(arenaCurrent, mode, specialBot));
        }

        /// <summary>
        /// Send this event when user or opponent dead by Lave or Obstacle
        /// </summary>
        public static void LogPVPBotDeadth(this PBAnalyticsManager manager, string mode, string stageName, string whoDie, string causeOfDeath)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogPVPBotDeadth(mode, stageName, whoDie, causeOfDeath));
        }

        /// <summary>
        /// Send this event when user matching and quit game
        /// </summary>
        public static void LogQuitCheck(this PBAnalyticsManager manager, string mode, string stageName, string mainPhase, string subPhase)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogQuitCheck(mode, stageName, mainPhase, subPhase));
        }

        /// <summary>
        /// Send this event when flip bot
        /// </summary>
        public static void LogFlipTiming(this PBAnalyticsManager manager, int arenaIndex, string status)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogFlipTiming(arenaIndex, status));
        }
    }

    /// <summary>
    /// Character UI events
    /// </summary>
    public static class CharacterUIEvent
    {
        public interface ILogger
        {
            void LogFeatureUsed(string feature, int value);
        }

        /// <summary>
        /// Send a FTUE event
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="feature">Use in Upgrade PopUp</param>
        public static void LogFeatureUsed(this PBAnalyticsManager manager, string feature, int value)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogFeatureUsed(feature, value));
        }
    }


    /// <summary>
    /// FTUE events
    /// </summary>
    public static class FTUEEvent
    {
        public interface ILogger
        {
            void LogFTUEEvent(int status, string type, string step);
        }
        /// <summary>
        /// Send a FTUE event
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="status">0: Start, 1: Complete</param>
        /// <param name="type"></param>
        /// <param name="step"></param>
        public static void LogFTUEEvent(this PBAnalyticsManager manager, int status, string type, string step = "")
        {
            var eventID = $"FTUE{status}{type}{step}";
            if (CheckIfEventHasRaised(eventID)) return;
            //GameEventHandler.Invoke(LogFirebaseEventCode.Tutorials, status, type, step);
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogFTUEEvent(status, type, step));
        }
        static bool CheckIfEventHasRaised(string eventID)
        {
            if (PlayerPrefs.GetInt($"HasRaisedEvent_{eventID}") == 1)
            {
                return true;
            }
            else
            {
                PlayerPrefs.SetInt($"HasRaisedEvent_{eventID}", 1);
                return false;
            }
        }
    }

    /// <summary>
    /// Clicking button events
    /// </summary>
    public static class ClickButtonEvent
    {
        public interface ILogger
        {
            void ClickButton(string buttonName, int clickTimes);
        }
        public static void ClickButton(this PBAnalyticsManager manager, string buttonName, int clickTimes)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.ClickButton(buttonName, clickTimes));
        }
    }

    /// <summary>
    /// Upgradable events
    /// </summary>
    public static class UpgradableEvent
    {
        public interface ILogger
        {
            void Upgrade(string type, string name, int formIndex, int upgradeStepIndex, int currentArena);
            void LogCalcUpgrade(string name, int upgradeStepIndex, int currentArena);
            void Owned(string type, string status, int count, int currentMilestone, int currentArena);
        }
        /// <summary>
        /// Send this event when upgrade a character/gear
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="type">Character, Gear</param>
        /// <param name="name">Internal name</param>
        /// <param name="formIndex"></param>
        /// <param name="upgradeStepIndex"></param>
        public static void Upgrade(this PBAnalyticsManager manager, string type, string name, int formIndex, int upgradeStepIndex, int currentArena)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.Upgrade(type, name, formIndex, upgradeStepIndex, currentArena));
        }

        /// <summary>
        /// Send this event when upgrade a character/gear
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="type">Character, Gear</param>
        /// <param name="name">Internal name</param>
        /// <param name="formIndex"></param>
        /// <param name="upgradeStepIndex"></param>
        public static void LogCalcUpgrade(this PBAnalyticsManager manager, string name, int upgradeStepIndex, int currentArena)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogCalcUpgrade(name, upgradeStepIndex, currentArena));
        }

        /// <summary>
        /// Send this event when a character/gear is unlocked or max upgraded
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="type">Character, Gear</param>
        /// <param name="status">Unlocked, MaxUpgraded</param>
        /// <param name="count">The count number classified by type and status</param>
        public static void Owned(this PBAnalyticsManager manager, string type, string status, int count, int currentMilestone, int currentArena)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.Owned(type, status, count, currentMilestone, currentArena));
        }
    }

    /// <summary>
    /// TrophyRoad events
    /// </summary>
    public static class TrophyRoadEvent
    {
        public interface ILogger
        {
            void ReachMilestone(string status, int trophyMilestonesID, int daysSinceInstall, int valueTimeFollowingMistone);
        }
        /// <summary>
        /// Send this event when player reaches (unlocks) a new milestone
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="trophyMilestonesID">milestone  ex:1, 2, 3, 4,...</param>
        public static void ReachMilestone(this PBAnalyticsManager manager, string status, int trophyMilestonesID, int valueTimeFollowingMistone)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.ReachMilestone(status, trophyMilestonesID, DaysSinceInstalledGettter.Get(), valueTimeFollowingMistone));
        }
    }

    public static class BossEvent
    {
        public interface ILogger
        {
            void LogBossFight(string status, int arenaIndex, int bossID, string overallScore, float timeCompletedMatch);
            void LogBossFightStreak(int bossID, int streak);
            void LogRevivePopup(string bossName, string status);
        }

        public static void LogBossFight(this PBAnalyticsManager manager, string status, int arenaIndex, int bossID, string overrallScore, float timeCompletedMatch)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogBossFight(status, arenaIndex, bossID, overrallScore, timeCompletedMatch));
        }

        public static void LogBossFightStreak(this PBAnalyticsManager manager, int bossID, int streak)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogBossFightStreak(bossID, streak));
        }

        public static void LogRevivePopup(this PBAnalyticsManager manager, string bossName, string status)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogRevivePopup(bossName, status));
        }
    }

    /// <summary>
    /// Main UI
    /// </summary>
    public static class MainUI
    {
        public interface ILogger
        {
            void LogOpenBox(int arenaIndex, int boxNumber, string openStatus, string location);
            void LogPopupStarterPack(string popupName, string operation, string status);
            void LogFullBoxSlot();
            void LogCollectBox(int arenaIndex, string mode, string boxType, string slotState);
            void LogBossOutEnergy(string bossName);
            void LogBossOutEnergyAlert(string bossName, string status);
            void LogPowerDilemma(string popupName, string operation, string status);
            void LogPopupGroup(int arenaIndex, string popupName, string operation, string status);
            void LogRVShow(int arenaIndex, string rvName, string location);
        }

        /// <summary>
        /// Send this event when open box
        /// </summary>
        /// <param name="arenaIndex"></param>
        /// <param name="boxNumber"></param>
        public static void LogOpenBox(this PBAnalyticsManager manager, int arenaIndex, int boxNumber, string openStatus, string location)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogOpenBox(arenaIndex, boxNumber, openStatus, location));
        }

        /// <summary>
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="popupName">Name Popup</param>
        /// <param name="operation">How this popup is showed: Manually, Automatically</param>
        /// <param name="status">1. Start: When the popup is showed, 2. Complete: When the popup is closed</param>
        public static void LogPopupStarterPack(this PBAnalyticsManager manager, string popupName, string operation, string status)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogPopupStarterPack(popupName, operation, status));
        }

        /// <summary>
        /// Send this event when full box slot
        /// </summary>
        public static void LogFullBoxSlot(this PBAnalyticsManager manager)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogFullBoxSlot());
        }

        /// <summary>
        /// Send this event when collect box from SingePVP or BattlePVP
        /// </summary>
        public static void LogCollectBox(this PBAnalyticsManager manager, int arenaIndex, string mode, string boxType, string slotState)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogCollectBox(arenaIndex, mode, boxType, slotState));
        }

        /// <summary>
        /// Send this event when Boss Out Of Energy
        /// </summary>
        public static void LogBossOutEnergy(this PBAnalyticsManager manager, string bossName)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogBossOutEnergy(bossName));
        }

        /// <summary>
        /// Send this event when Show popup charge energy
        /// </summary>
        public static void LogBossOutEnergyAlert(this PBAnalyticsManager manager, string bossName, string status)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogBossOutEnergyAlert(bossName, status));
        }

        /// <summary>
        /// Send this event when over the power
        /// </summary>
        public static void LogPowerDilemma(this PBAnalyticsManager manager, string popupName, string operation, string status)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogPowerDilemma(popupName, operation, status));
        }

        /// <summary>
        /// Trigger when a popup is shown
        /// </summary>
        public static void LogPopupGroup(this PBAnalyticsManager manager, int arenaIndex, string popupName, string operation, string status)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogPopupGroup(arenaIndex, popupName, operation, status));
        }

        /// <summary>
        /// Trigger when a popup is shown
        /// </summary>
        public static void LogRVShow(this PBAnalyticsManager manager, int arenaIndex, string rvName, string location)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogRVShow(arenaIndex, rvName, location));
        }
    }

    public static class DesignEvent
    {
        public interface ILogger
        {
            void HotOfferBuy(int arenaIndex, string currency, string offer);
            void BossFightUnlock(int bossID);
            void BattlePvPChooseArena(int currentArena, int arenaChoosen);
            void BodyUsedDesignEvent(int trophyMilestonesID, string mode, string bodyName);
            void FrontWeaponUsedDesignEvent(int trophyMilestonesID, string mode, string frontName);
            void UpperWeaponUsedDesignEvent(int trophyMilestonesID, string mode, string UpperName);
            void LeagueRankDesignEvent(int weekID, string divisionID, int rankRange, int playedTimeToReachRank);
            void PreludeMissionDesignEvent(string status, int seasonID, string missionID, string missionDifficulty);
            void TodayMissionDesignEvent(string status, int seasonID, string missionID, string missionDifficulty);
            void WeeklyMissionDesignEvent(string status, int seasonID, string missionID, string missionDifficulty);
            void SeasonMissionDesignEvent(string status, int seasonID, string missionID, string missionDifficulty);
            void SkillUsageDuelEquipedDesignEvent(string status, int milestone, string skillName, string isOpponentUseSkill, int value);
            void SkillUsageDuelUsedDesignEvent(string status, int milestone, string skillName, string isOpponentUseSkill, int value);
            void SkillUsageBattleEquipedDesignEvent(string status, int milestone, string skillName, string isOpponentUseSkill, int value);
            void SkillUsageBattleUsedDesignEvent(string status, int milestone, string skillName, string isOpponentUseSkill, int value);
            void MissedEquipSkillDesignEvent(int milestone);
        }

        /// <summary>
        /// trigger when the player buy an offer whether in Shop or from Popups
        /// </summary>
        public static void HotOfferBuy(this PBAnalyticsManager manager, int arenaIndex, string currency, string offer)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.HotOfferBuy(arenaIndex, currency, offer));
        }

        /// <summary>
        /// trigger when the player reach the trophy requirement of the boss
        /// </summary>
        public static void BossFightUnlock(this PBAnalyticsManager manager, int bossID)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.BossFightUnlock(bossID));
        }

        /// <summary>
        /// trigger when player completing a Battle match
        /// </summary>
        public static void BattlePvPChooseArena(this PBAnalyticsManager manager, int currentArena, int arenaChoosen)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.BattlePvPChooseArena(currentArena, arenaChoosen));
        }

        public static void LeagueRankDesignEvent(this PBAnalyticsManager manager, int weekID, string divisionID, int rankRange, int playedTimeToReachRank)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LeagueRankDesignEvent(weekID, divisionID, rankRange, playedTimeToReachRank));
        }        
        
        public static void PreludeMissionDesignEvent(this PBAnalyticsManager manager, string status, int seasonID, string missionID, string missionDifficulty)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.PreludeMissionDesignEvent(status, seasonID, missionID, missionDifficulty));
        }       
        public static void TodayMissionDesignEvent(this PBAnalyticsManager manager, string status, int seasonID, string missionID, string missionDifficulty)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.TodayMissionDesignEvent(status, seasonID, missionID, missionDifficulty));
        }        
        public static void WeeklyMissionDesignEvent(this PBAnalyticsManager manager, string status, int seasonID, string missionID, string missionDifficulty)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.WeeklyMissionDesignEvent(status, seasonID, missionID, missionDifficulty));
        }        
        public static void SeasonMissionDesignEvent(this PBAnalyticsManager manager, string status, int seasonID, string missionID, string missionDifficulty)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.SeasonMissionDesignEvent(status, seasonID, missionID, missionDifficulty));
        }

        public static void SkillUsageDuelEquipedDesignEvent(this PBAnalyticsManager manager, string status, int milestone, string skillName, string isOpponentUseSkill, int value)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.SkillUsageDuelEquipedDesignEvent(status, milestone, skillName, isOpponentUseSkill, value));
        }

        public static void SkillUsageDuelUsedDesignEvent(this PBAnalyticsManager manager, string status, int milestone, string skillName, string isOpponentUseSkill, int value)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.SkillUsageDuelUsedDesignEvent(status, milestone, skillName, isOpponentUseSkill, value));
        }

        public static void SkillUsageBattleEquipedDesignEvent(this PBAnalyticsManager manager, string status, int milestone, string skillName, string isOpponentUseSkill, int value)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.SkillUsageBattleEquipedDesignEvent(status, milestone, skillName, isOpponentUseSkill, value));
        }

        public static void SkillUsageBattleUsedDesignEvent(this PBAnalyticsManager manager, string status, int milestone, string skillName, string isOpponentUseSkill, int value)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.SkillUsageBattleUsedDesignEvent(status, milestone, skillName, isOpponentUseSkill, value));
        }
        public static void MissedEquipSkillDesignEvent(this PBAnalyticsManager manager, int milestone)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.MissedEquipSkillDesignEvent(milestone));
        }

        /// <summary>
        /// trigger when the player complete a match
        /// </summary>
        public static void BodyUsedDesignEvent(this PBAnalyticsManager manager, int trophyMilestonesID, string mode, string bodyName)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.BodyUsedDesignEvent(trophyMilestonesID, mode, bodyName));
        }        
        public static void FrontWeaponUsedDesignEvent(this PBAnalyticsManager manager, int trophyMilestonesID, string mode, string frontName)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.FrontWeaponUsedDesignEvent(trophyMilestonesID, mode, frontName));
        }
        public static void UpperWeaponUsedDesignEvent(this PBAnalyticsManager manager, int trophyMilestonesID, string mode, string UpperName)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.UpperWeaponUsedDesignEvent(trophyMilestonesID, mode, UpperName));
        }
    }

    static class DaysSinceInstalledGettter { public static int Get() => DateTimeUtils.DaysSinceInstalled; }

    public static class FireBaseEvent
    {
        public interface ILogger
        {
            void SendEventMissionStarted(int missionID, Dictionary<string, object> parameters = null);
            void SendEventMissionComplete(Dictionary<string, object> parameters = null);
            void SendEventMissionFailed(Dictionary<string, object> parameters = null);
            void LogMiniLevelStarted(Dictionary<string, object> parameters);
            void LogMiniLevelCompleted();
            void LogMiniLevelFailed();
            void LogArenaStarted(Dictionary<string, object> parameters = null);
            void LogArenaCompleted(Dictionary<string, object> parameters = null);
            void LogItemEquip(Dictionary<string, object> parameters = null);
            void LogItemUpgrade(Dictionary<string, object> parameters = null);
            void LogTutorial(Dictionary<string, object> parameters = null);
            void LogBossMenu(string eventName, Dictionary<string, object> parameters = null);
            void LogBossFight(string eventName, Dictionary<string, object> parameters = null);
            void LogBoxAvailable( Dictionary<string, object> parameters = null);
            void LogBoxOpen( Dictionary<string, object> parameters = null);
            void LogBattleRoyale(string eventName ,Dictionary<string, object> parameters = null);
            void LogCurrencyTransaction(Dictionary<string, object> parameters = null);
            void LogFlip(string eventName, Dictionary<string, object> parameters = null);
            void LogUpgradeNowShown(Dictionary<string, object> parameters = null);
            void LogUpgradeNowClicked(Dictionary<string, object> parameters = null);
            void LogPopupAction(Dictionary<string, object> parameters = null);
            void LogIAPLocationPurchased(Dictionary<string, object> parameters = null);
            void LogTrophyChange(Dictionary<string, object> parameters = null);
            void CustomLogEvent(string eventName, Dictionary<string, object> parameters = null);
        }
        public static string GetFirstMissionStartKey() => "PBFirebaseAnalyticsService_FirstMission";
        public static string GetLastMissionStartKey() => "PBFirebaseAnalyticsService_LastMission";
        public static string GetTrophyPointCurrentKey() => "PBFirebaseAnalyticsService_TrophyPointCurrentKey";
        public static string GetMiniMissionKey() => "PBFirebaseAnalyticsService_MinimissionID";


        /// <summary>
        /// Send this event Run App and New Milestone
        /// </summary>
        public static void SendEventMissionStarted(this PBAnalyticsManager manager, int missionID, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.SendEventMissionStarted(missionID, parameters));
        }

        /// <summary>
        /// Send this event when Win Match
        /// </summary>
        public static void SendEventMissionComplete(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.SendEventMissionComplete(parameters));
        }

        /// <summary>
        /// Send this event when Lost Match
        /// </summary>
        public static void SendEventMissionFailed(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.SendEventMissionFailed(parameters));
        }

        /// <summary>
        /// Send this event Start Match
        /// </summary>
        public static void LogMiniLevelStarted(this PBAnalyticsManager manager, Dictionary<string, object> parameters)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogMiniLevelStarted(parameters));
        }

        /// <summary>
        /// Send this event Win Match
        /// </summary>
        public static void LogMiniLevelCompleted(this PBAnalyticsManager manager)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogMiniLevelCompleted());
        }

        /// <summary>
        /// Send this event Win Match
        /// </summary>
        public static void LogMiniLevelFailed(this PBAnalyticsManager manager)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogMiniLevelFailed());
        }

        /// <summary>
        /// Triggered when a new arena is started (more precisely, when first miniMission per arena is started). 
        /// </summary>
        public static void LogArenaStarted(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogArenaStarted(parameters));
        }

        /// <summary>
        /// Triggered when an is finished (more precisely, when the last miniMission per arena is finished). 
        /// </summary>
        public static void LogArenaCompleted(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogArenaCompleted(parameters));
        }

        /// <summary>
        /// Triggered when some item is equiped in a bot
        /// </summary>
        public static void LogItemEquip(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogItemEquip(parameters));
        }

        /// <summary>
        /// Triggered when some item is upgraded
        /// </summary>
        public static void LogItemUpgrade(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogItemUpgrade(parameters));
        }

        /// <summary>
        /// Triggered when some tutorial step is started or completed
        /// </summary>
        public static void LogTutorial(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogTutorial(parameters));
        }

        /// <summary>
        /// Triggered when the Boss Fight menu is opened or closed
        /// </summary>
        public static void LogBossMenu(this PBAnalyticsManager manager, string eventName, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogBossMenu(eventName, parameters));
        }

        /// <summary>
        /// Triggered when a Boss Fight match is started or completed
        /// </summary>
        public static void LogBossFight(this PBAnalyticsManager manager, string eventName, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogBossFight(eventName, parameters));
        }

        /// <summary>
        /// Triggered when a box is available to fill a slot
        /// </summary>
        public static void LogBoxAvailable(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogBoxAvailable(parameters));
        }

        /// <summary>
        /// Triggered when a box is available to fill a slot
        /// </summary>
        public static void LogBoxOpen(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogBoxOpen(parameters));
        }

        /// <summary>
        /// Triggered when a Battle Royale match is started or completed
        /// </summary>
        public static void LogBattleRoyale(this PBAnalyticsManager manager, string eventName, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogBattleRoyale(eventName, parameters));
        }

        /// <summary>
        /// Triggered when a coins or gems transaction is happened (tracking of game economy)
        /// </summary>
        public static void LogCurrencyTransaction(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogCurrencyTransaction(parameters));
        }

        /// <summary>
        /// Triggered when a Battle Royale match is started or completed
        /// </summary>
        public static void LogFlip(this PBAnalyticsManager manager, string eventName, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogFlip(eventName, parameters));
        }

        /// <summary>
        /// Triggered when the Upgrade Now! button is shown
        /// </summary>
        public static void LogUpgradeNowShown(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogUpgradeNowShown(parameters));
        }

        /// <summary>
        /// Triggered when the Upgrade Now! button is clicked
        /// </summary>
        public static void LogUpgradeNowClicked(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogUpgradeNowClicked(parameters));
        }

        /// <summary>
        /// Triggered when a user interacts with a pop up
        /// </summary>
        public static void LogPopupAction(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogPopupAction(parameters));
        }

        /// <summary>
        /// To track from where the IAP was purchased (only for fully processed purchases)
        /// </summary>
        public static void LogIAPLocationPurchased(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogIAPLocationPurchased(parameters));
        }

        /// <summary>
        /// Triggered when the trophy points are changed
        /// </summary>
        public static void LogTrophyChange(this PBAnalyticsManager manager, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogTrophyChange(parameters));
        }

        /// <summary>
        /// Send this event Win Match
        /// </summary>
        public static void CustomLogEvent(this PBAnalyticsManager manager, string eventName, Dictionary<string, object> parameters = null)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.CustomLogEvent(eventName, parameters));
        }
    }

    public static class ProgressionEvent
    {
        public interface ILogger
        {
            void ClaimBossProgressionEvent(string status, int bossID, string type);
            void WinStreakProgressionEvent(string status, int milestoneIndex, int currentTrophyMilestone);
            void PiggyBankProgressionEvent(string status, int currentArena, int level);
            void SeasonProgressionEvent(string status, int seasonID, int SeasonPassMilestonesID, int playedTimeToCompleteAMilestone);
            void MissionRoundProgressionEvent(string status, int seasonID, int SeasonPassMilestonesID);
            void EnterSeasonTabProgressionEvent(string status, int currentArena, string missionStatus);
            void BuySkinProgressionEvent(string status, string partname_SkinID, string currentcyType);
            void LeaguePromotionProgressionEvent(string status, int weeklyID, int divisionID);
            void LogProgressionEvent(string status, string content);
            void SinkProgressionEvent(string status, int trophyMilestonesID, CurrencyType currencyType, int amount);
            void SourceProgressionEvent(string status, int trophyMilestonesID, CurrencyType currencyType, int amount);
            void StageProgressionEvent(string status, string stageID, string difficulty, int playedTime);
            void LadderedOfferProgressionEvent(string status, string set, string pack);
        }

        // Trigger when player claim a boss successfully
        public static void ClaimBossProgressionEvent(this PBAnalyticsManager manager, string status, int bossID, string type)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.ClaimBossProgressionEvent(status, bossID, type));
        }

        // trigger when player reach a win streak milestone successfully
        public static void WinStreakProgressionEvent(this PBAnalyticsManager manager, string status, int milestoneIndex, int currentTrophyMilestone)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.WinStreakProgressionEvent(status, milestoneIndex, currentTrophyMilestone));
        }

        public static void PiggyBankProgressionEvent(this PBAnalyticsManager manager, string status, int currentArena, int level)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.PiggyBankProgressionEvent(status, currentArena, level));
        }
        public static void SeasonProgressionEvent(this PBAnalyticsManager manager, string status, int seasonID, int SeasonPassMilestonesID, int playedTimeToCompleteAMilestone)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.SeasonProgressionEvent(status, seasonID, SeasonPassMilestonesID, playedTimeToCompleteAMilestone));
        }
        public static void MissionRoundProgressionEvent(this PBAnalyticsManager manager, string status, int seasonID, int SeasonPassMilestonesID)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.MissionRoundProgressionEvent(status, seasonID, SeasonPassMilestonesID));
        }         
        public static void EnterSeasonTabProgressionEvent(this PBAnalyticsManager manager, string status, int currentArena, string missionStatus)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.EnterSeasonTabProgressionEvent(status, currentArena, missionStatus));
        }        
        public static void BuySkinProgressionEvent(this PBAnalyticsManager manager, string status, string partname_SkinID, string currentcyType)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.BuySkinProgressionEvent(status, partname_SkinID, currentcyType));
        }        
        public static void LeaguePromotionProgressionEvent(this PBAnalyticsManager manager, string status, int weeklyID, int divisionID)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LeaguePromotionProgressionEvent(status, weeklyID, divisionID));
        }       
        public static void LogProgressionEvent(this PBAnalyticsManager manager, string status, string content)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LogProgressionEvent(status, content));
        }        
        public static void LadderedOfferProgressionEvent(this PBAnalyticsManager manager, string status, string set, string pack)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.LadderedOfferProgressionEvent(status, set, pack));
        }

        public static void SinkProgressionEvent(this PBAnalyticsManager manager, string status, int trophyMilestonesID, CurrencyType currencyType, int amout)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.SinkProgressionEvent(status, trophyMilestonesID, currencyType, amout));
        }
        public static void SourceProgressionEvent(this PBAnalyticsManager manager, string status, int trophyMilestonesID, CurrencyType currencyType, int amout)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.SourceProgressionEvent(status, trophyMilestonesID, currencyType, amout));
        }
        public static void StageProgressionEvent(this PBAnalyticsManager manager, string status, string stageID, string difficulty, int playedTime)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.StageProgressionEvent(status, stageID, difficulty, playedTime));
        }
    }

    public static class PSDesignEvent
    {
        public interface ILogger
        {
            void TeamDeathMatchSurrender(int arena, int matchCount, int score);
            void TeamDeathMatchUserKillCount(string status, int arena, int killCount, int matchCount);
            void TeamDeathMatchTeamKillCount(string status, int arena, int killCount, int matchCount);
            void TeamDeathMatchUserAssistCount(string status, int arena, int assistCount, int matchCount);
            void TeamDeathMatchTeamAssistCount(string status, int arena, int assistCount, int matchCount);
            void TeamDeathMatchUserDeathCount(string status, int arena, int deathCount, int matchCount);
            void TeamDeathMatchTeamDeathCount(string status, int arena, int deathCount, int matchCount);
            void PSUpgrade(string status, string itemName, int arena, int upgradeStepIndex);
            void UnlockNewItem(int arena, string itemType, string itemName);
            void PlayWithUnequippedSlot(int unuquippedSlotCount);
        }
        public static void TeamDeathMatchSurrender(this PBAnalyticsManager manager, int arena, int matchCount, int score)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.TeamDeathMatchSurrender(arena, matchCount, score));
        }

        public static void TeamDeathMatchUserKillCount(this PBAnalyticsManager manager, string status, int arena, int killCount, int matchCount)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.TeamDeathMatchUserKillCount(status, arena, killCount, matchCount));
        }

        public static void TeamDeathMatchTeamKillCount(this PBAnalyticsManager manager, string status, int arena, int killCount, int matchCount)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.TeamDeathMatchTeamKillCount(status, arena, killCount, matchCount));
        }

        public static void TeamDeathMatchUserAssistCount(this PBAnalyticsManager manager, string status, int arena, int assistCount, int matchCount)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.TeamDeathMatchUserAssistCount(status, arena, assistCount, matchCount));
        }        
        public static void TeamDeathMatchTeamAssistCount(this PBAnalyticsManager manager, string status, int arena, int assistCount, int matchCount)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.TeamDeathMatchTeamAssistCount(status, arena, assistCount, matchCount));
        }
        public static void TeamDeathMatchUserDeathCount(this PBAnalyticsManager manager, string status, int arena, int deathCount, int matchCount)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.TeamDeathMatchUserDeathCount(status, arena, deathCount, matchCount));
        }        
        public static void TeamDeathMatchTeamDeathCount(this PBAnalyticsManager manager, string status, int arena, int deathCount, int matchCount)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.TeamDeathMatchTeamDeathCount(status, arena, deathCount, matchCount));
        }
        public static void PSUpgrade(this PBAnalyticsManager manager, string status, string itemName, int arena, int upgradeStepIndex)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.PSUpgrade(status, itemName, arena, upgradeStepIndex));
        }
        public static void UnlockNewItem(this PBAnalyticsManager manager, int arena, string itemType, string itemName)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.UnlockNewItem(arena, itemType, itemName));
        }
        public static void PlayWithUnequippedSlot(this PBAnalyticsManager manager, int unuquippedSlotCount)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.PlayWithUnequippedSlot(unuquippedSlotCount));
        }
    }

    public static class PSProgressionEvent
    {
        public interface ILogger
        {
            void TeamDeathmatch(string status, int arena, int time, int score);
        }

        public static void TeamDeathmatch(this PBAnalyticsManager manager, string status, int arena, int time, int score)
        {
            manager.LogEvent(
                service => service as ILogger,
                service => service.TeamDeathmatch(status, arena, time, score));
        }
    }
}