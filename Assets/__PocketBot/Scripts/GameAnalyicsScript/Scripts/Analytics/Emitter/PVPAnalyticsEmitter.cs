using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GachaSystem.Core;
using HightLightDebug;
using HyrphusQ.Events;
using LatteGames.Monetization;
using LatteGames.PvP;
using LatteGames.PvP.TrophyRoad;
using PBAnalyticsEvents;
using Sirenix.OdinInspector;
using UnityEngine;
using HyrphusQ.SerializedDataStructure;
using PackReward;
using static DrawCardProcedure;
using System;
using System.Linq;
using static Cinemachine.DocumentationSortingAttribute;
using static LatteGames.PvP.RPSCalculatorSO;
using Unity.VisualScripting;
using static CurrencyIconsEmitter;

//
public class PVPAnalyticsEmitter : MonoBehaviour
{
    [SerializeField, BoxGroup("PvP")] private PBRobotStatsSO playerRobotStatsSO;
    [SerializeField, BoxGroup("PvP")] private IntVariable totalPlayedMatchesCount;
    [SerializeField, BoxGroup("PvP")] private RPSCalculatorSO rpsCalculatorSO;
    [SerializeField, BoxGroup("PvP")] private PPrefIntVariable pprefWinStreak;
    [SerializeField, BoxGroup("PvP")] private PPrefIntVariable pprefLoseStreak;
    [SerializeField, BoxGroup("PvP")] private PPrefIntVariable _pprefTotalMatch;
    [SerializeField, BoxGroup("PvP")] private CurrentHighestArenaVariable currentHighestArenaVariable;
    [SerializeField, BoxGroup("PvP")] private PvPTournamentSO _tournamentSO;
    [SerializeField, BoxGroup("PvP")] private PPrefIntVariable _pprefTotalTimeUnlockPVP_FollowingArena;
    [SerializeField, BoxGroup("PvP")] private PPrefIntVariable _pprefTotalTimeUnlockMistone;

    [SerializeField, BoxGroup("Property")] private IntVariable requiredNumOfMedalsToUnlockVariable;
    [SerializeField, BoxGroup("Property")] private FloatVariable highestAchievedMedalsVariable;
    [SerializeField, BoxGroup("Property")] private TrophyRoadSO _trophyRoadSO;
    [SerializeField, BoxGroup("Property")] private ModeVariable _currentChosenModeVariable;
    [SerializeField, BoxGroup("Property")] private PPrefStringVariable playerName;

    [SerializeField, BoxGroup("Battle Mode")] private PPrefIntVariable pprefBattleMode;
    [SerializeField, BoxGroup("Battle Mode")] private PPrefIntVariable pprefWinStreakBattleMode;
    [SerializeField, BoxGroup("Battle Mode")] private PPrefIntVariable pprefLoseStreakBattleMode;
    [SerializeField, BoxGroup("Battle Mode")] private PPrefIntVariable pprefPlayerKillNumber;

    [SerializeField, BoxGroup("Data")] private BossFightStreakHandle bossFightStreakHandle;
    [SerializeField, BoxGroup("Data")] private RangeOfRPSCacl rangeOfRPSCacl;
    [SerializeField, BoxGroup("Data")] private Variable<int> totalDoubleTapReverse;
    [SerializeField, BoxGroup("Data")] private CurrencySO m_MedalCurrency;
    [SerializeField, BoxGroup("Data")] private CurrencySO m_RVTicketCurrency;
    [SerializeField, BoxGroup("Data")] private CurrencyDictionarySO m_CurrencyDic;
    [SerializeField, BoxGroup("Data")] private BossChapterSO m_BossChapterSO;
    [SerializeField, BoxGroup("Data")] private BattleBetArenaVariable m_BattleBetArenaVariable;
    [SerializeField, BoxGroup("Data")] private CharacterManagerSO m_CharacterManagerSO;
    [SerializeField, BoxGroup("Data")] private PBTrophyRoadSO m_PBTrophyRoadSO;
    [SerializeField, BoxGroup("Data")] private ActiveSkillManagerSO m_ActiveSkillManagerSO;

    [SerializeField, BoxGroup("Data")] private PBPartManagerSO m_SpecialPartManager;
    [SerializeField, BoxGroup("Data")] private PPrefItemSOVariable m_CurrentBodySO;
    [SerializeField, BoxGroup("Data")] private PPrefItemSOVariable m_CurrentFrontSO;
    [SerializeField, BoxGroup("Data")] private PPrefItemSOVariable m_CurrentUpper1SO;
    [SerializeField, BoxGroup("Data")] private PPrefItemSOVariable m_CurrentUpper2SO;

    [SerializeField, BoxGroup("Economy")] private SerializedDictionary<ResourceLocation, string> m_ResourceType;

    [SerializeField, BoxGroup("Starter Pack")] private OperationStarterPackVariable operationStarterPackVariable;

    private PBGameAnalyticsService pbGameAnalyticsService;
    private PBAnalyticsManager AnalyticsManager => PBAnalyticsManager.Instance;
    private PVPBattleInteraction _pvpBattleInteraction;
    private List<CarPhysics> _carPhysicOpponents;
    private PBFightingStage _pbFightingStage;

    private bool _callOneTimeBotDead = false;
    private bool matchAbandoned = false;
    private bool m_IsHasRVTicket;
    private int _timeAfterEndRound;
    private string m_DifficultyStage;

    private IEnumerator _countTime;

    private string groupName;

    #region Active Skill
    private int m_PlayerSkillUse = 0;
    private int m_OpponentSkillUse = 0;
    private ActiveSkillSO m_PlayerActiveSkillSO;
    #endregion

    private void Awake()
    {
        groupName = PBRemoteConfigManager.GetGroupName();
        
        #region Progression Event
        GameEventHandler.AddActionEvent(ProgressionEvent.ClaimBoss, ClaimBossProgressionEvent);
        GameEventHandler.AddActionEvent(ProgressionEvent.WinStreak, WinStreakProgressionEvent);
        GameEventHandler.AddActionEvent(ProgressionEvent.PiggyBank, PiggyBankProgressionEvent);
        GameEventHandler.AddActionEvent(ProgressionEvent.Season, SeasonProgressionEvent);
        GameEventHandler.AddActionEvent(ProgressionEvent.MissionRound, MissionRoundProgressionEvent);
        GameEventHandler.AddActionEvent(ProgressionEvent.EnterSeasonTab, EnterSeasonTabProgressionEvent);
        GameEventHandler.AddActionEvent(ProgressionEvent.BuySkin, BuySkinProgressionEvent);
        GameEventHandler.AddActionEvent(ProgressionEvent.LeaguePromotion, LeaguePromotionProgressionEvent);
        GameEventHandler.AddActionEvent(ProgressionEvent.Progression, LogProgressionEvent);
        GameEventHandler.AddActionEvent(ProgressionEvent.LadderedOffer, LadderedOfferProgressionEvent);
        #endregion

        #region Design Event
        GameEventHandler.AddActionEvent(DesignEvent.HotOfferBuy, HotOfferBuy);
        GameEventHandler.AddActionEvent(DesignEvent.BossFightUnlock, BossFightUnlock);
        GameEventHandler.AddActionEvent(DesignEvent.BattlePvPChooseArena, BattlePvPChooseArena);
        GameEventHandler.AddActionEvent(DesignEvent.LeagueRank, LeagueRankDesignEvent);
        GameEventHandler.AddActionEvent(DesignEvent.PreludeMission, PreludeMissionDesignEvent);
        GameEventHandler.AddActionEvent(DesignEvent.TodayMission, TodayMissionDesignEvent);
        GameEventHandler.AddActionEvent(DesignEvent.WeeklyMission, WeeklyMissionDesignEvent);
        GameEventHandler.AddActionEvent(DesignEvent.SeasonMission, SeasonMissionDesignEvent);
        GameEventHandler.AddActionEvent(DesignEvent.SkillUsage_Duel_Equiped, SkillUsageDuelEquipedDesignEvent);
        GameEventHandler.AddActionEvent(DesignEvent.SkillUsage_Duel_Used, SkillUsageDuelUsedDesignEvent);
        GameEventHandler.AddActionEvent(DesignEvent.SkillUsage_Battle_Equiped, SkillUsageBattleEquipedDesignEvent);
        GameEventHandler.AddActionEvent(DesignEvent.SkillUsage_Battle_Used, SkillUsageBattleUsedDesignEvent);
        #endregion

        #region OpenBox
        GameEventHandler.AddActionEvent(DesignEvent.OpenBox, OpenBoxEvent);
        GameEventHandler.AddActionEvent(DesignEvent.CollectBox, CollectBox);
        #endregion

        #region OnValueChanged
        playerName.onValueChanged += PlayerNameOnchange;
        highestAchievedMedalsVariable.onValueChanged += OnHighestAchievedMedalsChanged;
        #endregion

        #region PvP Event
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnNewArenaUnlocked, HandleNewArenaUnlocked);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, HandleMatchStarted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, HandleFinalRoundCompleted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnLeaveInMiddleOfMatch, HandleMatchAbandoned);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, HandleBotModelSpawned);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotDamaged, HandleReceiveKilled);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStart);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnd);
        GameEventHandler.AddActionEvent(DesignEvent.FlipTiming, FlipTiming);
        #endregion

        #region Achieve Event
        GameEventHandler.AddActionEvent(LeaderboardEventCode.OnTopMilestoneRankPassed, HandlePlayerPassingMilestone);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        #endregion

        #region Accessibility Event
        GameEventHandler.AddActionEvent(CameraViewEvent.Switch, SwitchCameraEvent);
        GameEventHandler.AddActionEvent(DesignEvent.FeatureUsed, FeatureUsed);
        GameEventHandler.AddActionEvent(AssistiveEventCode.Flip, FlipButtonClicked);
        #endregion

        #region Resource Event
        GameEventHandler.AddActionEvent(EconomyEventCode.AcquireResource, OnAcquireCurrency);
        GameEventHandler.AddActionEvent(EconomyEventCode.ConsumeResource, OnSpendCurrency);
        GameEventHandler.AddActionEvent(LogSinkSource.SkillCard, SkillCardSource);
        GameEventHandler.AddActionEvent(ActiveSkillManagementEventCode.OnSkillCardChanged, SkillCardSink);
        #endregion

        #region FTUE Event
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartFightButton, StartFightButton);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndFightButton, EndFightButton);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartControl, StartControlFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartReverse, StartReverseFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndReverse, EndReverseFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndControl, EndControlFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartOpenBox1, StartOpenBox1);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndOpenBox1, EndOpenBox1);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartEquip_1, StartEquip_1FTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndEquip_1, EndEquip_1FTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartPower, StartPowerFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndPower, EndPowerFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartUpgrade, StartUpgradeFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndUpgrade, EndUpgradeFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartPlaySingle, StartPlaySingleFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndPlaySingle, EndPlaySingleFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartOpenBox2, StartOpenBox2);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndOpenBox2, EndOpenBox2);
        //GameEventHandler.AddActionEvent(LogFTUEEventCode.StartBuildTab, StartBuildTab);
        //GameEventHandler.AddActionEvent(LogFTUEEventCode.EndBuildTab, EndBuildTab);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartEquip_2, StartEquip_2FTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndEquip_2, EndEquip_2FTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartEnterBossUI, StartEnterBossUIFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndEnterBossUI, EndEnterBossUIFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartPlayBossFight, StartPlayBossFightFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndPlayBossFight, EndPlayBossFightFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartSkinUI, StartSkinUIFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndSkinUI, EndSkinUIFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartPlayBattle, StartPlayBattleFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndPlayBattle, EndPlayBattleFTUE);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartPreludeSeason, StartPreludeSeason);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndPreludeSeason, EndPreludeSeason);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartLeague, StartLeague);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndLeague, EndLeague);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartActiveSkillEnter, StartActiveSkillEnter);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndActiveSkillEnter, EndActiveSkillEnter);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartActiveSkillClaim, StartActiveSkillClaim);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndActiveSkillClaim, EndActiveSkillClaim);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartActiveSkillEquip, StartActiveSkillEquip);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndActiveSkillEquip, EndActiveSkillEquip);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.StartActiveSkillInGame, StartActiveSkillInGame);
        GameEventHandler.AddActionEvent(LogFTUEEventCode.EndActiveSkillInGame, EndActiveSkillInGame);
        #endregion

        #region Monetization Event
        pbGameAnalyticsService = FindObjectOfType<PBGameAnalyticsService>();
        GameEventHandler.AddActionEvent(MonetizationEventCode.MultiplierRewards, MonetizationMultiplierRewards);
        GameEventHandler.AddActionEvent(MonetizationEventCode.RevengeBoss_BossFightUI, MonetizationRevengeBoss_BossFightUI);
        GameEventHandler.AddActionEvent(MonetizationEventCode.RevengeBoss_LoseBossUI, MonetizationRevengeBoss_LoseBossUI);
        GameEventHandler.AddActionEvent(MonetizationEventCode.ClaimBoss_InventoryUI, MonetizationClaimBoss_InventoryUI);
        GameEventHandler.AddActionEvent(MonetizationEventCode.ClaimBoss_WinBossUI, MonetizationClaimBoss_WinBossUI);
        GameEventHandler.AddActionEvent(MonetizationEventCode.FreeBox_MainUI, MonetizationFreeBox_MainUI);
        GameEventHandler.AddActionEvent(MonetizationEventCode.Upgrade, MonetizationUpgrade);
        GameEventHandler.AddActionEvent(MonetizationEventCode.BonusCard_OpenBoxUI, MonetizationBonusCard_OpenBoxUI);
        GameEventHandler.AddActionEvent(MonetizationEventCode.BonusCard_YouGotUI, MonetizationBonusCard_YouGotUI);
        GameEventHandler.AddActionEvent(MonetizationEventCode.OpenNowBox_BoxPopup, OpenNowBox_BoxPopup);
        GameEventHandler.AddActionEvent(MonetizationEventCode.OpenNowBox_GameOverUI, OpenNowBox_GameOverUI);
        GameEventHandler.AddActionEvent(MonetizationEventCode.SpeedUpBox, SpeedUpBox);
        GameEventHandler.AddActionEvent(MonetizationEventCode.RechargeBoss_AlertPopup, Monetization_RechargeBoss_AlertPopup);
        GameEventHandler.AddActionEvent(MonetizationEventCode.RechargeBoss_BuildUI, Monetization_RechargeBoss_BuildUI);
        GameEventHandler.AddActionEvent(MonetizationEventCode.RechargeBoss_BossPopup, Monetization_RechargeBoss_BossPopup);
        GameEventHandler.AddActionEvent(MonetizationEventCode.ReviveBoss, Monetization_ReviveBoss);
        GameEventHandler.AddActionEvent(MonetizationEventCode.GetSkin, Monetization_GetSkin);
        GameEventHandler.AddActionEvent(MonetizationEventCode.HotOffers_UpgradePopup, Monetization_HotOffersUpgradePopup);
        GameEventHandler.AddActionEvent(MonetizationEventCode.HotOffers_ResourcePopup, Monetization_HotOffersResourcePopup);
        GameEventHandler.AddActionEvent(MonetizationEventCode.HotOffers_Shop, Monetization_HotOffersShop);
        GameEventHandler.AddActionEvent(MonetizationEventCode.TrophyBypass_BossUI, Monetization_TrophyBypassBossUI);
        GameEventHandler.AddActionEvent(MonetizationEventCode.LinkRewards_MainUI, Monetization_LinkRewardsMainUI);
        GameEventHandler.AddActionEvent(MonetizationEventCode.LinkRewards_Shop, Monetization_LinkRewardsShop);
        GameEventHandler.AddActionEvent(MonetizationEventCode.KeepWinStreak, Monetization_KeepWinStreak);
        GameEventHandler.AddActionEvent(MonetizationEventCode.WinStreakPremium, Monetization_WinStreakPremium);
        GameEventHandler.AddActionEvent(MonetizationEventCode.GetBack, Monetization_GetBack);
        GameEventHandler.AddActionEvent(MonetizationEventCode.GetMissingCard, Monetization_GetMissingCard);
        GameEventHandler.AddActionEvent(MonetizationEventCode.BuyGarage, Monetization_BuyGarage);
        GameEventHandler.AddActionEvent(MonetizationEventCode.SkipMission, Monetization_SkipMission);
        GameEventHandler.AddActionEvent(MonetizationEventCode.ClaimDoubleMission, Monetization_ClaimDoubleMission);
        GameEventHandler.AddActionEvent(MonetizationEventCode.BuyCharacter, Monetization_BuyCharacter);
        GameEventHandler.AddActionEvent(MonetizationEventCode.FreeEventTicket, Monetization_FreeEventTicket);
        GameEventHandler.AddActionEvent(MonetizationEventCode.LadderedOffer, Monetization_LadderedOffer);
        GameEventHandler.AddActionEvent(MonetizationEventCode.FreesSkill, Monetization_FreesSkill);
        #endregion

        #region BossEvent
        GameEventHandler.AddActionEvent(BossEventCode.StartBossFight, StartBossFight);
        GameEventHandler.AddActionEvent(BossEventCode.CompleteBossFight, CompleteBossFight);
        GameEventHandler.AddActionEvent(BossEventCode.FailBossFight, FailBossFight);
        GameEventHandler.AddActionEvent(BossEventCode.BossFightStreak, BossFightStreak);
        GameEventHandler.AddActionEvent(DesignEvent.RevivePopup, RevivePopup);
        #endregion

        #region Starter Pack Event
        GameEventHandler.AddActionEvent(StarterPackEventCode.StarterPackIAP, StarterPackIAP);
        GameEventHandler.AddActionEvent(StarterPackEventCode.PopupStart, StartPopupStarterPack);
        GameEventHandler.AddActionEvent(StarterPackEventCode.PopupComplete, CompletePopupStarterPack);
        #endregion

        #region MainUI
        GameEventHandler.AddActionEvent(DesignEvent.FullSlotBoxSlot, FullSlotBoxSlot);
        GameEventHandler.AddActionEvent(DesignEvent.BossOutEnergy, BossOutEnergy);
        GameEventHandler.AddActionEvent(DesignEvent.BossOutEnergyAlert, BossOutEnergyAlert);
        GameEventHandler.AddActionEvent(DesignEvent.Popup, PowerDilemma);
        GameEventHandler.AddActionEvent(DesignEvent.PopupGroup, PopupGroup);
        GameEventHandler.AddActionEvent(DesignEvent.RVShow, RVShow);
        #endregion

        #region IAP
        GameEventHandler.AddActionEvent(LogIAPEventCode.IAPPack, IAPPackPurchased);
        #endregion

        #region Interstitial
        GameEventHandler.AddActionEvent(AdvertisingEventCode.OnShowAd, OnShowAdInterstitial);
        GameEventHandler.AddActionEvent(AdvertisingEventCode.OnCloseAd, OnCloseAdInterstitial);
        #endregion

        m_RVTicketCurrency.onValueChanged += RVTicketCurrency_OnValueChanged;
        OnMilestoneInitialized();
    }

    private void RVTicketCurrency_OnValueChanged(ValueDataChanged<float> data)
    {
        if (data.oldValue != 1 && data.newValue > 0)
            m_IsHasRVTicket = true;
        else
            m_IsHasRVTicket = false;

        if (data.oldValue == 1 && data.newValue == 0)
            m_IsHasRVTicket = true;
    }

    private void OnDestroy()
    {
        #region Progression Event
        GameEventHandler.RemoveActionEvent(ProgressionEvent.ClaimBoss, ClaimBossProgressionEvent);
        GameEventHandler.RemoveActionEvent(ProgressionEvent.WinStreak, WinStreakProgressionEvent);
        GameEventHandler.RemoveActionEvent(ProgressionEvent.PiggyBank, PiggyBankProgressionEvent);
        GameEventHandler.RemoveActionEvent(ProgressionEvent.Season, SeasonProgressionEvent);
        GameEventHandler.RemoveActionEvent(ProgressionEvent.MissionRound, MissionRoundProgressionEvent);
        GameEventHandler.RemoveActionEvent(ProgressionEvent.EnterSeasonTab, EnterSeasonTabProgressionEvent);
        GameEventHandler.RemoveActionEvent(ProgressionEvent.BuySkin, BuySkinProgressionEvent);
        GameEventHandler.RemoveActionEvent(ProgressionEvent.LeaguePromotion, LeaguePromotionProgressionEvent);
        GameEventHandler.RemoveActionEvent(ProgressionEvent.Progression, LogProgressionEvent);
        GameEventHandler.RemoveActionEvent(ProgressionEvent.LadderedOffer, LadderedOfferProgressionEvent);
        #endregion

        #region Design Event
        GameEventHandler.RemoveActionEvent(DesignEvent.HotOfferBuy, HotOfferBuy);
        GameEventHandler.RemoveActionEvent(DesignEvent.BossFightUnlock, BossFightUnlock);
        GameEventHandler.RemoveActionEvent(DesignEvent.BattlePvPChooseArena, BattlePvPChooseArena);
        GameEventHandler.RemoveActionEvent(DesignEvent.LeagueRank, LeagueRankDesignEvent);
        GameEventHandler.RemoveActionEvent(DesignEvent.PreludeMission, PreludeMissionDesignEvent);
        GameEventHandler.RemoveActionEvent(DesignEvent.TodayMission, TodayMissionDesignEvent);
        GameEventHandler.RemoveActionEvent(DesignEvent.WeeklyMission, WeeklyMissionDesignEvent);
        GameEventHandler.RemoveActionEvent(DesignEvent.SeasonMission, SeasonMissionDesignEvent);
        GameEventHandler.RemoveActionEvent(DesignEvent.SkillUsage_Duel_Equiped, SkillUsageDuelEquipedDesignEvent);
        GameEventHandler.RemoveActionEvent(DesignEvent.SkillUsage_Duel_Used, SkillUsageDuelUsedDesignEvent);
        GameEventHandler.RemoveActionEvent(DesignEvent.SkillUsage_Battle_Equiped, SkillUsageBattleEquipedDesignEvent);
        GameEventHandler.RemoveActionEvent(DesignEvent.SkillUsage_Battle_Used, SkillUsageBattleUsedDesignEvent);
        #endregion

        #region OpenBox
        GameEventHandler.RemoveActionEvent(DesignEvent.OpenBox, OpenBoxEvent);
        GameEventHandler.RemoveActionEvent(DesignEvent.CollectBox, CollectBox);
        #endregion

        #region OnValueChanged
        playerName.onValueChanged -= PlayerNameOnchange;
        highestAchievedMedalsVariable.onValueChanged -= OnHighestAchievedMedalsChanged;
        #endregion

        #region PvP Event
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnNewArenaUnlocked, HandleNewArenaUnlocked);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, HandleMatchStarted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, HandleFinalRoundCompleted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnLeaveInMiddleOfMatch, HandleMatchAbandoned);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, HandleBotModelSpawned);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotDamaged, HandleReceiveKilled);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStart);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnd);
        GameEventHandler.RemoveActionEvent(DesignEvent.FlipTiming, FlipTiming);
        #endregion

        #region Achieve Event
        GameEventHandler.RemoveActionEvent(LeaderboardEventCode.OnTopMilestoneRankPassed, HandlePlayerPassingMilestone);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        #endregion

        #region Accessibility Event
        GameEventHandler.RemoveActionEvent(CameraViewEvent.Switch, SwitchCameraEvent);
        GameEventHandler.RemoveActionEvent(DesignEvent.FeatureUsed, FeatureUsed);
        GameEventHandler.RemoveActionEvent(AssistiveEventCode.Flip, FlipButtonClicked);
        #endregion

        #region Resource Event
        GameEventHandler.RemoveActionEvent(EconomyEventCode.AcquireResource, OnAcquireCurrency);
        GameEventHandler.RemoveActionEvent(EconomyEventCode.ConsumeResource, OnSpendCurrency);
        GameEventHandler.RemoveActionEvent(LogSinkSource.SkillCard, SkillCardSource);
        GameEventHandler.RemoveActionEvent(ActiveSkillManagementEventCode.OnSkillCardChanged, SkillCardSink);
        #endregion

        #region FTUE Event
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartFightButton, StartFightButton);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndFightButton, EndFightButton);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartControl, StartControlFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartReverse, StartReverseFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndReverse, EndReverseFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndControl, EndControlFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartOpenBox1, StartOpenBox1);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndOpenBox1, EndOpenBox1);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartEquip_1, StartEquip_1FTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndEquip_1, EndEquip_1FTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartPower, StartPowerFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndPower, EndPowerFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartUpgrade, StartUpgradeFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndUpgrade, EndUpgradeFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartPlaySingle, StartPlaySingleFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndPlaySingle, EndPlaySingleFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartOpenBox2, StartOpenBox2);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndOpenBox2, EndOpenBox2);
        //GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartBuildTab, StartBuildTab);
        //GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndBuildTab, EndBuildTab);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartEquip_2, StartEquip_2FTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndEquip_2, EndEquip_2FTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartEnterBossUI, StartEnterBossUIFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndEnterBossUI, EndEnterBossUIFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartPlayBossFight, StartPlayBossFightFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndPlayBossFight, EndPlayBossFightFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartPlayBattle, StartPlayBattleFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndPlayBattle, EndPlayBattleFTUE);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartPreludeSeason, StartPreludeSeason);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndPreludeSeason, EndPreludeSeason);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartLeague, StartLeague);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndLeague, EndLeague);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartActiveSkillEnter, StartActiveSkillEnter);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndActiveSkillEnter, EndActiveSkillEnter);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartActiveSkillClaim, StartActiveSkillClaim);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndActiveSkillClaim, EndActiveSkillClaim);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartActiveSkillEquip, StartActiveSkillEquip);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndActiveSkillEquip, EndActiveSkillEquip);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.StartActiveSkillInGame, StartActiveSkillInGame);
        GameEventHandler.RemoveActionEvent(LogFTUEEventCode.EndActiveSkillInGame, EndActiveSkillInGame);
        #endregion

        #region Monetization Event
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.MultiplierRewards, MonetizationMultiplierRewards);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.RevengeBoss_BossFightUI, MonetizationRevengeBoss_BossFightUI);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.RevengeBoss_LoseBossUI, MonetizationRevengeBoss_LoseBossUI);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.ClaimBoss_InventoryUI, MonetizationClaimBoss_InventoryUI);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.ClaimBoss_WinBossUI, MonetizationClaimBoss_WinBossUI);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.FreeBox_MainUI, MonetizationFreeBox_MainUI);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.Upgrade, MonetizationUpgrade);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.BonusCard_OpenBoxUI, MonetizationBonusCard_OpenBoxUI);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.BonusCard_YouGotUI, MonetizationBonusCard_YouGotUI);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.OpenNowBox_BoxPopup, OpenNowBox_BoxPopup);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.OpenNowBox_GameOverUI, OpenNowBox_GameOverUI);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.SpeedUpBox, SpeedUpBox);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.RechargeBoss_AlertPopup, Monetization_RechargeBoss_AlertPopup);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.RechargeBoss_BuildUI, Monetization_RechargeBoss_BuildUI);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.RechargeBoss_BossPopup, Monetization_RechargeBoss_BossPopup);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.ReviveBoss, Monetization_ReviveBoss);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.GetSkin, Monetization_GetSkin);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.HotOffers_UpgradePopup, Monetization_HotOffersUpgradePopup);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.HotOffers_ResourcePopup, Monetization_HotOffersResourcePopup);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.HotOffers_Shop, Monetization_HotOffersShop);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.TrophyBypass_BossUI, Monetization_TrophyBypassBossUI);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.LinkRewards_MainUI, Monetization_LinkRewardsMainUI);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.LinkRewards_Shop, Monetization_LinkRewardsShop);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.KeepWinStreak, Monetization_KeepWinStreak);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.WinStreakPremium, Monetization_WinStreakPremium);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.GetBack, Monetization_GetBack);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.GetMissingCard, Monetization_GetMissingCard);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.BuyGarage, Monetization_BuyGarage);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.SkipMission, Monetization_SkipMission);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.ClaimDoubleMission, Monetization_ClaimDoubleMission);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.BuyCharacter, Monetization_BuyCharacter);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.FreeEventTicket, Monetization_FreeEventTicket);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.LadderedOffer, Monetization_LadderedOffer);
        GameEventHandler.RemoveActionEvent(MonetizationEventCode.FreesSkill, Monetization_FreesSkill);
        #endregion

        #region BossEvent
        GameEventHandler.RemoveActionEvent(BossEventCode.StartBossFight, StartBossFight);
        GameEventHandler.RemoveActionEvent(BossEventCode.CompleteBossFight, CompleteBossFight);
        GameEventHandler.RemoveActionEvent(BossEventCode.FailBossFight, FailBossFight);
        GameEventHandler.RemoveActionEvent(BossEventCode.BossFightStreak, BossFightStreak);
        GameEventHandler.RemoveActionEvent(DesignEvent.RevivePopup, RevivePopup);
        #endregion

        #region Starter Pack Event
        GameEventHandler.RemoveActionEvent(StarterPackEventCode.StarterPackIAP, StarterPackIAP);
        GameEventHandler.RemoveActionEvent(StarterPackEventCode.PopupStart, StartPopupStarterPack);
        GameEventHandler.RemoveActionEvent(StarterPackEventCode.PopupComplete, CompletePopupStarterPack);
        #endregion

        #region MainUI
        GameEventHandler.RemoveActionEvent(DesignEvent.FullSlotBoxSlot, FullSlotBoxSlot);
        GameEventHandler.RemoveActionEvent(DesignEvent.BossOutEnergy, BossOutEnergy);
        GameEventHandler.RemoveActionEvent(DesignEvent.BossOutEnergyAlert, BossOutEnergyAlert);
        GameEventHandler.RemoveActionEvent(DesignEvent.Popup, PowerDilemma);
        GameEventHandler.RemoveActionEvent(DesignEvent.PopupGroup, PopupGroup);
        GameEventHandler.RemoveActionEvent(DesignEvent.RVShow, RVShow);
        #endregion

        #region IAP
        GameEventHandler.RemoveActionEvent(LogIAPEventCode.IAPPack, IAPPackPurchased);
        #endregion

        #region Interstitial
        GameEventHandler.RemoveActionEvent(AdvertisingEventCode.OnShowAd, OnShowAdInterstitial);
        GameEventHandler.RemoveActionEvent(AdvertisingEventCode.OnCloseAd, OnCloseAdInterstitial);
        #endregion

        m_RVTicketCurrency.onValueChanged -= RVTicketCurrency_OnValueChanged;
    }

    private void Start()
    {
        StartCoroutine(StartTimeLifeCycle());

        string keyCallTheFirstUnlockPVP = $"UnlockPVP-TheFirstTime@@!!~";
        if (!PlayerPrefs.HasKey(keyCallTheFirstUnlockPVP))
        {
            PlayerPrefs.SetInt(keyCallTheFirstUnlockPVP, 1);
            AnalyticsManager.UnlockPVP("Start", 0, _pprefTotalTimeUnlockPVP_FollowingArena.value);
        }
    }

    private IEnumerator StartTimeLifeCycle()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        while (true)
        {
            _pprefTotalTimeUnlockPVP_FollowingArena.value++;
            _pprefTotalTimeUnlockMistone.value++;
            yield return waitForSeconds;
        }
    }

    #region Progress Event
    private void ClaimBossProgressionEvent(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null || parameters[2] == null) return;
        string status = (string)parameters[0];
        int bossID = (int)parameters[1];
        string type = (string)parameters[2];

        AnalyticsManager.ClaimBossProgressionEvent(status, bossID, type);
    }

    private void WinStreakProgressionEvent(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null || parameters[2] == null) return;
        string status = (string)parameters[0];
        int milestones = (int)parameters[1];
        int currentArena = currentHighestArenaVariable.value.index + 1;

        AnalyticsManager.WinStreakProgressionEvent(status, milestones + 1, currentArena);
    }

    private void PiggyBankProgressionEvent(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        string status = (string)parameters[0];
        int level = (int)parameters[1];

        AnalyticsManager.PiggyBankProgressionEvent(status, currentArena, level);
    }

    private void SeasonProgressionEvent(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null || parameters[2] == null || parameters[3] == null) return;
        string status = (string)parameters[0];
        int seasonID = (int)parameters[1];
        int mileStoneID = (int)parameters[2];
        int playedTimeToCompleteAMilestone = (int)parameters[3];

        AnalyticsManager.SeasonProgressionEvent(status, seasonID, mileStoneID, playedTimeToCompleteAMilestone);
    }

    private void MissionRoundProgressionEvent(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null || parameters[2] == null) return;
        string status = (string)parameters[0];
        int seasonID = (int)parameters[1];
        int milestoneID = (int)parameters[2];

        AnalyticsManager.MissionRoundProgressionEvent(status, seasonID, milestoneID);
    }

    private void EnterSeasonTabProgressionEvent(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        string status = (string)parameters[0];
        string missionStatus = (string)parameters[1];


        AnalyticsManager.EnterSeasonTabProgressionEvent(status, currentArena, missionStatus);
    }

    private void BuySkinProgressionEvent(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null || parameters[2] == null) return;
        string status = (string)parameters[0];
        string partname_SkinID = (string)parameters[1];
        string currentcyType = (string)parameters[2];

        AnalyticsManager.BuySkinProgressionEvent(status, partname_SkinID, currentcyType);
    }    

    private void LeaguePromotionProgressionEvent(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null || parameters[2] == null) return;
        string status = (string)parameters[0];
        int weeklyID = (int)parameters[1];
        int divisionID = (int)parameters[2];

        AnalyticsManager.LeaguePromotionProgressionEvent(status, weeklyID, divisionID);
    }    
    
    private void LogProgressionEvent(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;
        string status = (string)parameters[0];
        string content = (string)parameters[1];

        AnalyticsManager.LogProgressionEvent(status, content);
    }    
    private void LadderedOfferProgressionEvent(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null || parameters[2] == null) return;
        string status = (string)parameters[0];
        string set = (string)parameters[1];
        string pack = (string)parameters[2];

        AnalyticsManager.LadderedOfferProgressionEvent(status, set, pack);
    }
    #endregion

    #region OpenBoxEvent
    private void PlayWithFullSlot()
    {
        if (PBPackDockManager.Instance != null && PBPackDockManager.Instance.IsFull)
            AnalyticsManager.PlayWithFullSlot($"{GetNameMode()}");

    }

    private void OpenBoxEvent(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;
        string openStatus = (string)parameters[0];
        string location = (string)parameters[1];
        int currentArenaIndex = currentHighestArenaVariable.value.index + 1;
        AnalyticsManager.LogOpenBox(currentArenaIndex, GetQuantityBoxFollowingCurrentArena(currentArenaIndex, 1), openStatus, location);
    }

    private void CollectBox(params object[] parameters)
    {
        if (_currentChosenModeVariable == Mode.Boss) return;
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;
        int currentArenaIndex = currentHighestArenaVariable.value.index + 1;
        GachaPack gachaPack = (GachaPack)parameters[0];
        string slotState = (string)parameters[1];

        string input = $"{gachaPack.GetModule<NameItemModule>().displayName}";
        string typeBox = input.Split(' ')[0];

        AnalyticsManager.LogCollectBox(currentArenaIndex, GetNameMode(), typeBox, slotState);
    }

    #endregion

    #region Design Event
    private void HotOfferBuy(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;

        int currentArena = currentHighestArenaVariable.value.index + 1;
        string currency = (string)parrams[0];
        string offer = (string)parrams[1];

        AnalyticsManager.HotOfferBuy(currentArena, currency, offer);
    }

    private void BossFightUnlock(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null) return;
        int bossID = (int)parrams[0];

        AnalyticsManager.BossFightUnlock(bossID);
    }

    private void BattlePvPChooseArena(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        int arenaChoosen = (int)parrams[0];

        AnalyticsManager.BattlePvPChooseArena(currentArena, arenaChoosen);
    }

    private void LeagueRankDesignEvent(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null || parrams[2] == null || parrams[3] == null) return;
        int weekID = (int)parrams[0];
        string divisionID = (string)parrams[1];
        int rankRange = (int)parrams[2];
        int playedTimeToReachRank = (int)parrams[3];

        AnalyticsManager.LeagueRankDesignEvent(weekID, divisionID, rankRange, playedTimeToReachRank);
    }    
    private void PreludeMissionDesignEvent(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null || parrams[2] == null || parrams[3] == null) return;
        string status = (string)parrams[0];
        int seasonID = (int)parrams[1];
        string missionName = (string)parrams[2];
        MissionDifficulty missionDifficulty = (MissionDifficulty)parrams[3];

        AnalyticsManager.PreludeMissionDesignEvent(status, seasonID, missionName, missionDifficulty.ToString());
    }    
    private void TodayMissionDesignEvent(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null || parrams[2] == null || parrams[3] == null) return;
        string status = (string)parrams[0];
        int seasonID = (int)parrams[1];
        string missionID = (string)parrams[2];
        MissionDifficulty missionDifficulty = (MissionDifficulty)parrams[3];

        AnalyticsManager.TodayMissionDesignEvent(status, seasonID, missionID, missionDifficulty.ToString());
    }    
    private void WeeklyMissionDesignEvent(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null || parrams[2] == null || parrams[3] == null) return;
        string status = (string)parrams[0];
        int seasonID = (int)parrams[1];
        string missionID = (string)parrams[2];
        MissionDifficulty missionDifficulty = (MissionDifficulty)parrams[3];

        AnalyticsManager.WeeklyMissionDesignEvent(status, seasonID, missionID, missionDifficulty.ToString());
    }    
    private void SeasonMissionDesignEvent(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null || parrams[2] == null || parrams[3] == null) return;
        string status = (string)parrams[0];
        int seasonID = (int)parrams[1];
        string missionID = (string)parrams[2];
        MissionDifficulty missionDifficulty = (MissionDifficulty)parrams[3];

        AnalyticsManager.SeasonMissionDesignEvent(status, seasonID, missionID, missionDifficulty.ToString());
    }

    private void SkillUsageDuelEquipedDesignEvent(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        PBRobot robot = (PBRobot)parrams[0];
        ActiveSkillSO activeSkillSO = (ActiveSkillSO)parrams[1];
        Mode mode = _currentChosenModeVariable.value;
        if (activeSkillSO == null) return;

        if (mode == Mode.Normal)
        {
            string status = "Equipped";
            int milestone = m_PBTrophyRoadSO.GetHighestAchievedMilestoneNumber();
            string skillName = $"{activeSkillSO.GetDisplayName().Replace(" ", "_")}";
            string isOpponentUseSkill = "null";
            int value = 0;
            AnalyticsManager.SkillUsageDuelEquipedDesignEvent(status, milestone, skillName, isOpponentUseSkill, value);
        }
    }    
    private void SkillUsageDuelUsedDesignEvent(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        PBRobot robot = (PBRobot)parrams[0];
        ActiveSkillSO activeSkillSO = (ActiveSkillSO)parrams[1];
        if (robot == null && activeSkillSO == null)
        {
            Debug.LogError($"Robot Or ActiveSkillSO Is Null");
            return;
        }
        Mode mode = _currentChosenModeVariable.value;
        if(mode == Mode.Normal)
        {
            if (robot.PersonalInfo.isLocal)
            {
                m_PlayerActiveSkillSO = activeSkillSO;
                m_PlayerSkillUse++;
            }
            else
            {
                m_OpponentSkillUse++;
            }
        }
    }
    
    private void LogSkillUsageDuelUsedDesignEvent()
    {
        if (m_PlayerActiveSkillSO == null) return;

        string status = "Used";
        int milestone = m_PBTrophyRoadSO.GetHighestAchievedMilestoneNumber();
        string skillName = $"{m_PlayerActiveSkillSO.GetDisplayName().Replace(" ", "_")}";
        string isOpponentUseSkill = m_OpponentSkillUse > 0 ? "OpponentSkillUsed" : "OpponentSkillUnused";
        int value = m_PlayerSkillUse;

        AnalyticsManager.SkillUsageDuelUsedDesignEvent(status, milestone, skillName, isOpponentUseSkill, value);
    }

    private void SkillUsageBattleEquipedDesignEvent(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        PBRobot robot = (PBRobot)parrams[0];
        ActiveSkillSO activeSkillSO = (ActiveSkillSO)parrams[1];
        Mode mode = _currentChosenModeVariable.value;
        if (activeSkillSO == null) return;

        if (mode == Mode.Battle) 
        {
            string status = "Equipped";
            int milestone = m_PBTrophyRoadSO.GetHighestAchievedMilestoneNumber();
            string skillName = $"{activeSkillSO.GetDisplayName().Replace(" ", "_")}";
            string isOpponentUseSkill = "null";
            int value = 0;
            AnalyticsManager.SkillUsageBattleEquipedDesignEvent(status, milestone, skillName, isOpponentUseSkill, value);
        }
    }    
    private void SkillUsageBattleUsedDesignEvent(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        PBRobot robot = (PBRobot)parrams[0];
        ActiveSkillSO activeSkillSO = (ActiveSkillSO)parrams[1];
        if (robot == null && activeSkillSO == null)
        {
            Debug.LogError($"Robot Or ActiveSkillSO Is Null");
            return;
        }
        Mode mode = _currentChosenModeVariable.value;
        if (mode == Mode.Battle)
        {
            if (robot.PersonalInfo.isLocal)
            {
                m_PlayerActiveSkillSO = activeSkillSO;
                m_PlayerSkillUse++;
            }
            else
            {
                m_OpponentSkillUse++;
            }
        }
    }
    private void LogSkillUsageBattleUsedDesignEvent()
    {
        if (m_PlayerActiveSkillSO == null) return;

        string status = "Used";
        int milestone = m_PBTrophyRoadSO.GetHighestAchievedMilestoneNumber();
        string skillName = $"{m_PlayerActiveSkillSO.GetDisplayName().Replace(" ", "_")}";
        string isOpponentUseSkill = m_OpponentSkillUse > 0 ? "OpponentSkillUsed" : "OpponentSkillUnused";
        int value = m_PlayerSkillUse;

        AnalyticsManager.SkillUsageBattleUsedDesignEvent(status, milestone, skillName, isOpponentUseSkill, value);
    }
    #endregion

    #region OnValueChanged
    private void PlayerNameOnchange(ValueDataChanged<string> name)
    {
        
    }

    private void OnHighestAchievedMedalsChanged(ValueDataChanged<float> eventData)
    {
        if (eventData.oldValue < requiredNumOfMedalsToUnlockVariable.value && eventData.newValue >= requiredNumOfMedalsToUnlockVariable.value)
        {
            AnalyticsManager.UnlockBattlePvP();
        }
    }
    #endregion

    #region PvP Event
    private void HandleNewArenaUnlocked(object[] objs)
    {
        if (objs[0] is not PvPArenaSO arenaSO) return;
        AnalyticsManager.UnlockPVP("Complete", arenaSO.index - 1, _pprefTotalTimeUnlockPVP_FollowingArena.value);
        AnalyticsManager.UnlockPVP("Start", arenaSO.index, _pprefTotalTimeUnlockPVP_FollowingArena.value);
        _pprefTotalTimeUnlockPVP_FollowingArena.value = 0;
    }

    private void HandleMatchStarted(object[] objs)
    {
        _callOneTimeBotDead = false;
        matchAbandoned = false;
        if (objs[0] is not PvPMatch match) return;

        m_PlayerSkillUse = 0;
        m_OpponentSkillUse = 0;
        m_PlayerActiveSkillSO = null;

        PBChassisSO pbChassisSO = playerRobotStatsSO.chassisInUse.value.Cast<PBChassisSO>();
        pprefPlayerKillNumber.value = 0;

        _timeAfterEndRound = 1;
        if (_countTime != null)
            StopCoroutine(_countTime);
        _countTime = CountTimeBattle();
        StartCoroutine(_countTime);

        Mode mode = _currentChosenModeVariable.value;

        #region Design Event
        try
        {
            if (mode == Mode.Normal || mode == Mode.Battle)
            {
                ItemSO skillSOUsing = m_ActiveSkillManagerSO.currentItemInUse;
                int totalCardSkillNumber = m_ActiveSkillManagerSO.initialValue.Sum(v => v.GetNumOfCards());
                int milestone = m_PBTrophyRoadSO.GetHighestAchievedMilestoneNumber();
                if (skillSOUsing == null && totalCardSkillNumber > 0)
                {
                    AnalyticsManager.MissedEquipSkillDesignEvent(milestone);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion

        #region Progression Event - PlayWithBoss
        if (pbChassisSO != null)
        {
            if (pbChassisSO.IsSpecial)
            {
                int currentArena = currentHighestArenaVariable.value.index + 1;
                string modeName = mode switch
                {
                    Mode.Normal => "SinglePvP",
                    Mode.Boss => "BossFight",
                    _ => "BattlePvP"
                };

                BossChapterSO bossChapterSO = BossFightManager.Instance.bossMapSO.GetBossChapterDefault;
                BossSO currentBossSO = BossFightManager.Instance.bossMapSO.currentBossSO;

                string specialBot = $"";
                if (pbChassisSO.IsTransformBot)
                {
                    var specialChassisSOs = m_SpecialPartManager.initialValue
                        .OfType<PBChassisSO>()
                        .ToList();

                    int transformBotID = specialChassisSOs
                        .Where(v => v.IsTransformBot && v.Equals(pbChassisSO))
                        .Select(v => v.TransformBotID)
                        .FirstOrDefault();

                    specialBot = $"Transformbot{transformBotID}";
                }
                else
                {
                    int bossID = bossChapterSO.bossList.FindIndex(v => v.chassisSO == pbChassisSO) + 1;
                    specialBot = $"Boss{bossID}";
                }
                AnalyticsManager.LogPlayWithBoss(currentArena, modeName, specialBot);
            }
        }

        PlayWithFullSlot();
        #endregion

        int botStrength = (int)match.GetLocalPlayerInfo().Cast<PBPlayerInfo>().robotStatsSO.value;
        if (mode == Mode.Battle)
        {
            PBPvPArenaSO pBPvPArenaSO = match.arenaSO as PBPvPArenaSO;
            SetTotalMatchArena(pBPvPArenaSO, mode);

            #region Firebase Event
            string key = pBPvPArenaSO.index.ToString() + mode;
            int totalMatch = PlayerPrefs.GetInt(key);
            string driver = m_CharacterManagerSO.PlayerCharacterSO.value.GetDisplayName();

            GameEventHandler.Invoke(LogFirebaseEventCode.BattleRoyale, false, totalMatch, driver, botStrength);
            #endregion
        }

        _pprefTotalMatch.value += 1;
        var totalStartedMatchedCount = _pprefTotalMatch.value;
        if (totalStartedMatchedCount % 5 == 0 && totalStartedMatchedCount > 0)
        {
            AnalyticsManager.Match(totalStartedMatchedCount);
        }

        //Lof Info Bot
        AnalyticsManager.BodyUsedDesignEvent(GetMissionIDCurrent(), GetNameMode(), GetBodyName());
        if (m_CurrentFrontSO.value != null)
            AnalyticsManager.FrontWeaponUsedDesignEvent(GetMissionIDCurrent(), GetNameMode(), GetFrontName());
        if (m_CurrentUpper1SO.value != null || m_CurrentUpper2SO.value != null)
            AnalyticsManager.UpperWeaponUsedDesignEvent(GetMissionIDCurrent(), GetNameMode(), GetUpperName());
        //

        if (mode == Mode.Boss || mode == Mode.Battle) return;

        SetTotalMatchArena(match.arenaSO as PBPvPArenaSO, _currentChosenModeVariable.value);

        HandleEventMatchSingleMode("Start", match, 0);

        #region Progression Event
        string nameStage = PBFightingStage.Instance.gameObject.name.Replace("(Clone)", "");
        m_DifficultyStage = "even";
        if (rpsCalculatorSO != null)
            m_DifficultyStage = rpsCalculatorSO.CalcCurrentRPSValue().stateLabel.ToLower();

        AnalyticsManager.StageProgressionEvent("Start", nameStage, m_DifficultyStage, 0);
        #endregion


        if (playerRobotStatsSO.stats.GetAttack().value <= 0f)
        {
            AnalyticsManager.PlayWithoutAttack();
        }

        #region FireBaseEvent
        var botInfo = match.GetOpponentInfo().Cast<PBBotInfo>();
        var data = (PBRPSCalculatorSO.PBRPSData)rpsCalculatorSO.CalcCurrentRPSValue();

        var localPlayer = match.GetLocalPlayerInfo().Cast<PBPlayerInfo>().robotStatsSO;
        var botPlayer = match.GetOpponentInfo().Cast<PBPlayerInfo>().robotStatsSO;
        var rpsData = rpsCalculatorSO.CalcRPSValue(localPlayer, botPlayer);
        string difficulty = rpsData.stateLabel switch
        {
            "Hard" => "hard",
            "Even" => "even",
            _ => "easy",
        };
        int trophyPoints = (int)m_MedalCurrency.value;

        PBChassisSO chassisSO = m_CurrentBodySO.value as PBChassisSO;
        string special = chassisSO.IsSpecial ? $"{chassisSO.GetModule<NameItemModule>().displayName}" : "null";
        string body = $"{chassisSO.GetModule<NameItemModule>().displayName}";
        string front = "null";
        string upper1 = "null";
        string upper2 = "null";

        ItemSO frontGearSO = m_CurrentFrontSO.value;
        ItemSO upper1GearSO = m_CurrentUpper1SO.value;
        ItemSO upper2GearSO = m_CurrentUpper2SO.value;

        if (frontGearSO != null)
        {
            if (frontGearSO.TryGetModule<NameItemModule>(out var frontModule))
                front = $"{frontModule.displayName}";
        }

        if (upper1GearSO != null)
        {
            if (upper1GearSO.TryGetModule<NameItemModule>(out var upper1GeaModule))
                upper1 = $"{upper1GeaModule.displayName}";
        }

        if (upper2GearSO != null)
        {
            if (upper2GearSO.TryGetModule<NameItemModule>(out var upper2GeaModule))
                upper2 = $"{upper2GeaModule.displayName}";
        }

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            {"difficulty", difficulty},
            {"trophyPoints", trophyPoints},
            {"special", special},
            {"body", body},
            {"front", front},
            {"upper1", upper1},
            {"upper2", upper2},
            {"currentArena", $"{currentHighestArenaVariable.value.index + 1}"},
            {"driver", m_CharacterManagerSO.PlayerCharacterSO.value.GetDisplayName()},
            {"botStrength", botStrength}
        };
        AnalyticsManager.LogMiniLevelStarted(parameters);
        #endregion
    }

    private void HandleFinalRoundCompleted(object[] objs)
    {
        if (_countTime != null)
            StopCoroutine(_countTime);

        if (objs[0] is not PvPMatch match) return;
        AnalyticsManager.LogFeatureUsed("DoubleTapReverse", totalDoubleTapReverse.value);

        if (!match.isAbleToComplete) return; // Check all rounds completed

        if (_currentChosenModeVariable.value == Mode.Boss) return;

        if (_currentChosenModeVariable.value == Mode.Battle)
        {
            #region Design Event
            try
            {
                LogSkillUsageBattleUsedDesignEvent();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion

            #region Firebase Event
            PBPvPArenaSO pBPvPArenaSO = match.arenaSO as PBPvPArenaSO;
            PBPvPMatch pBPvPMatch = match as PBPvPMatch;
            string key = pBPvPArenaSO.index.ToString() + _currentChosenModeVariable.value;
            int totalMatch = PlayerPrefs.GetInt(key);
            string driver = m_CharacterManagerSO.PlayerCharacterSO.value.GetDisplayName();
            int botStrength = (int)match.GetLocalPlayerInfo().Cast<PBPlayerInfo>().robotStatsSO.value;
            GameEventHandler.Invoke(LogFirebaseEventCode.BattleRoyale, true, totalMatch, pBPvPMatch.rankOfMine, driver, botStrength);
            #endregion
        }

        if (matchAbandoned)
        {
            AnalyticsManager.LogMiniLevelFailed();
            LogMatchCompleteEvent(match, matchAbandoned);
            return;
        }

        bool isDualMode = _currentChosenModeVariable.value == Mode.Normal;
        if (match.isVictory)
        {
            if (isDualMode)
            {
                AnalyticsManager.LogMiniLevelCompleted();
                UpdatePPrefStreak(pprefWinStreak, pprefLoseStreak, match);
            }
            else
                UpdatePPrefStreak(pprefWinStreakBattleMode, pprefLoseStreakBattleMode, match);
        }
        else
        {
            if (isDualMode)
            {
                AnalyticsManager.LogMiniLevelFailed();
                UpdatePPrefStreak(pprefLoseStreak, pprefWinStreak, match);
            }
            else
                UpdatePPrefStreak(pprefLoseStreakBattleMode, pprefWinStreakBattleMode, match);
        }
        if (isDualMode)
        {
            #region Design Event
            try
            {
                LogSkillUsageDuelUsedDesignEvent();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion
        }

        if (matchAbandoned) return;
        LogMatchCompleteEvent(match, matchAbandoned);
    }

    private void HandleMatchAbandoned(object[] objs)
    {
        matchAbandoned = true;
    }

    private void HandleBotModelSpawned(params object[] parameters)
    {
        Mode mode = _currentChosenModeVariable.value;
        if (mode != Mode.Battle) return;

        if (parameters[0] is PBRobot pbBot)
        {
            if (pbBot.PersonalInfo.isLocal == true)
            {
                _pvpBattleInteraction = pbBot.ChassisInstance.CarPhysics.gameObject.GetOrAddComponent<PVPBattleInteraction>();
            }
            else
            {
                if (_carPhysicOpponents == null)
                    _carPhysicOpponents = new List<CarPhysics>();
                _carPhysicOpponents.Add(pbBot.ChassisInstance.CarPhysics);
            }
        }
    }

    private void HandleReceiveKilled(object[] objs)
    {
        if (objs[0] is not PBRobot)
            return;

        PBRobot pBRobot = objs[0] as PBRobot;
        float health = pBRobot.Health;

        Transform attacker = null;
        if (objs[3] is IAttackable)
            attacker = ((objs[3] as IAttackable) as MonoBehaviour).transform;


        if (!_callOneTimeBotDead)
        {
            var infoAttacker = attacker.GetComponentInParent<PBRobot>();
            if (infoAttacker != null)
            {
                if (infoAttacker.name == "Player(Clone)" && health <= 0)
                {
                    if (pprefPlayerKillNumber != null)
                        pprefPlayerKillNumber.value++;
                }
            }

            // var lavePlane = attacker.transform.GetComponent<LavaPlane>();

            // if (lavePlane != null)
            // {
            //     string stageName = PBFightingStage.Instance.gameObject.name.Replace("(Clone)", "");
            //     string whoDie = pBRobot.ChassisInstance.CarPhysics.transform.name.Contains("Player") ? "User" : "Opponent";
            //     string CauseOfDeath = lavePlane != null ? "FallSuicide" : "KilledByObstacle";

            //     if (lavePlane != null && health <= 0 && pBRobot.TimeReceiveDamageDurationFromBot <= 0)
            //     {
            //         PBAnalyticsManager.Instance.LogPVPBotDeadth(GetNameMode(), stageName, whoDie, CauseOfDeath);
            //         _callOneTimeBotDead = true;
            //     }
            //     else if (lavePlane == null && health <= 0 && pBRobot.TimeReceiveDamageDurationFromBot <= 0)
            //     {
            //         PBAnalyticsManager.Instance.LogPVPBotDeadth(GetNameMode(), stageName, whoDie, CauseOfDeath);
            //         _callOneTimeBotDead = true;
            //     }
            // }
        }

    }

    private void OnLevelStart()
    {
        string keyCallFTUEFightingStart = "Key-CallOneTimeStartMiniMission-Start";
        if (!PlayerPrefs.HasKey(keyCallFTUEFightingStart))
        {
            //AnalyticsManager.LogMiniLevelStarted();
            PlayerPrefs.SetInt(keyCallFTUEFightingStart, 1);
        }

        if (_pvpBattleInteraction != null)
        {
            if (_carPhysicOpponents != null)
                _pvpBattleInteraction.StartInteraction(_carPhysicOpponents);
        }
    }

    private void OnLevelEnd()
    {
        string keyCallFTUEFightingCompleted = "Key-CallOneTimeStartMiniMission-Completed";
        if (!PlayerPrefs.HasKey(keyCallFTUEFightingCompleted))
        {
            //AnalyticsManager.LogMiniLevelCompleted();
            PlayerPrefs.SetInt(keyCallFTUEFightingCompleted, 1);
        }

        if (_pvpBattleInteraction != null)
        {
            _pvpBattleInteraction.StopInterection();
        }
    }


    private void HandleEventMatchSingleMode(string status, PvPMatch match, int timeEndRound)
    {
        float calcRpsValue = CalcRPSRaw(match);
        //float resultValueRPS = rangeOfRPSCacl.GetResultForValue(calcRpsValue);

        if (_currentChosenModeVariable.value == Mode.Normal)
        {
            //string combination = GetCombinationBot();
            string placeHolder = "Placeholder";
            PBPvPArenaSO pBPvPArenaSO = match.arenaSO as PBPvPArenaSO;
            Mode mode = _currentChosenModeVariable.value;
            AnalyticsManager.LogSinglePVP(status, match.arenaSO.index, placeHolder, GetTotalMatch(pBPvPArenaSO, mode), timeEndRound);
        }
    }
    private void HandleEventWinOrLoseSingleMode(bool isVictory, PvPMatch match)
    {
        //float calcRpsValue = CalcRPSRaw(match);
        string status = isVictory ? "Victory" : "Defeated";
        //string combination = GetCombinationBot();
        string placeHolder = "Placeholder";
        PBPvPArenaSO pBPvPArenaSO = match.arenaSO as PBPvPArenaSO;

        Mode mode = _currentChosenModeVariable.value;
        AnalyticsManager.LogSinglePVP(status, match.arenaSO.index, placeHolder, GetTotalMatch(pBPvPArenaSO, mode), _timeAfterEndRound);

        #region Progression Event
        string statusStage = isVictory ? "Complete" : "Fail";
        string nameStage = PBFightingStage.Instance.gameObject.name.Replace("(Clone)", "");
        AnalyticsManager.StageProgressionEvent(statusStage, nameStage, m_DifficultyStage, _timeAfterEndRound);
        #endregion
    }

    private string GetCombinationBot()
    {
        PBChassisSO pbChassis = m_CurrentBodySO.value as PBChassisSO;

        if (pbChassis == null) return "";
        string combination = "";

        // Check if the chassis is not marked as special
        if (!pbChassis.IsSpecial)
        {
            // Initialize variables to store the names of body and front parts
            string bodyName = "";
            string frontName = "";

            // Set bodyName to the name of the chassis
            bodyName = m_CurrentBodySO.value.name;

            // If frontSO exists, set frontName with a hyphen prefix
            if (m_CurrentFrontSO.value != null)
                frontName = $"-{m_CurrentFrontSO.value.name}";

            // Get the upper part items from gearSaver
            ItemSO upper1SO = m_CurrentUpper1SO.value;
            ItemSO upper2SO = m_CurrentUpper2SO.value;

            // Initialize a list to store the upper parts and their names
            List<ItemSO> upperList = new List<ItemSO>();
            List<string> upperNames = new List<string>();

            // Add upper1 and upper2 to upperList if they are not null
            if (upper1SO != null)
                upperList.Add(upper1SO);
            if (upper2SO != null)
                upperList.Add(upper2SO);

            // Sort upperList alphabetically by the display name of each item
            upperList.Sort((a, b) => string.Compare(
                a.GetModule<NameItemModule>().displayName,
                b.GetModule<NameItemModule>().displayName,
                StringComparison.Ordinal
            ));

            // Loop through sorted upperList and add each internal name (with hyphen) to upperNames
            for (int i = 0; i < upperList.Count; i++)
                upperNames.Add($"-{upperList[i].GetInternalName()}");

            // Join all upper names into a single string with no separator
            string allUpperName = string.Join("", upperNames);

            // Construct the final combination by concatenating body, front, and upper names
            combination = $"{bodyName}{frontName}{allUpperName}";
        }
        else
        {
            if (pbChassis.IsTransformBot)
            {
                combination = $"Transformbot_{m_CurrentBodySO.value.GetDisplayName()}";
            }
            else
            {
                combination = $"Boss_{m_CurrentBodySO.value.GetDisplayName()}";
            }
        }

        // Return the constructed combination string
        return combination;
    }
    private string GetBodyName()
    {
        PBChassisSO pbChassis = m_CurrentBodySO.value as PBChassisSO;

        if (pbChassis == null) return "";
        // Check if the chassis is not marked as special
        string bodyName = m_CurrentBodySO.value.name;
        return bodyName;
    }
    private string GetFrontName()
    {
        PBChassisSO pbChassis = m_CurrentBodySO.value as PBChassisSO;

        if (pbChassis == null) return "";
        string frontName = "";

        // Check if the chassis is not marked as special
        if (!pbChassis.IsSpecial)
        {
            if (m_CurrentFrontSO.value != null)
                frontName = $"{m_CurrentFrontSO.value.name}";
        }
        else
            frontName = "";

        return frontName;
    }    
    private string GetUpperName()
    {
        PBChassisSO pbChassis = m_CurrentBodySO.value as PBChassisSO;

        if (pbChassis == null) return "";
        string upperName = "";

        if (!pbChassis.IsSpecial)
        {
            ItemSO upper1SO = m_CurrentUpper1SO.value;
            ItemSO upper2SO = m_CurrentUpper2SO.value;

            List<ItemSO> upperList = new List<ItemSO>();
            List<string> upperNames = new List<string>();

            if (upper1SO != null)
                upperList.Add(upper1SO);
            if (upper2SO != null)
                upperList.Add(upper2SO);

            upperList = upperList.Where(v => v != null).ToList();
            upperList.Sort((a, b) => string.Compare(
                a.GetModule<NameItemModule>().displayName,
                b.GetModule<NameItemModule>().displayName,
                StringComparison.Ordinal
            ));

            if (upperList.Count == 2)
            {
                upperNames.Add($"{upperList[0].GetInternalName()}");
                upperNames.Add($"-{upperList[1].GetInternalName()}");
            }
            else if (upperList.Count == 1)
                upperNames.Add($"{upperList[0].GetInternalName()}");
            else
                return "";
            

            upperName = string.Join("", upperNames);
        }
        else
        {
            upperName = "";
        }
        return upperName;
    }


    private float CalcRPSRaw(PvPMatch match)
    {
        var localPlayer = match.GetLocalPlayerInfo().Cast<PBPlayerInfo>().robotStatsSO;
        var botPlayer = match.GetOpponentInfo().Cast<PBPlayerInfo>().robotStatsSO;
        var rawRps = (localPlayer - botPlayer) / (localPlayer + botPlayer);
        float calcRpsValue = Mathf.Clamp(rawRps * 3f, -1000, 1000);

        return calcRpsValue;
    }

    private void LogMatchCompleteEvent(PvPMatch match, bool matchAbandoned)
    {
        if (_currentChosenModeVariable.value == Mode.Battle)
        {
            PBPvPMatch pbMatch = match as PBPvPMatch;
            string nameStage = PBFightingStage.Instance.gameObject.name.Replace("(Clone)", "");
            PBPvPArenaSO pBPvPArenaSO = match.arenaSO as PBPvPArenaSO;
            Mode mode = _currentChosenModeVariable.value;

            //string combination = GetCombinationBot();
            string placeHolder = "placeHolder";
            if (matchAbandoned)
            {
                AnalyticsManager.LogBattleStatus("Abandon", pbMatch.arenaSO.index + 1, placeHolder, GetTotalMatch(pBPvPArenaSO, mode), _timeAfterEndRound);
            } 
            else
            {
                AnalyticsManager.LogBattleStatus($"Top_{pbMatch.rankOfMine}", m_BattleBetArenaVariable.value.index + 1, placeHolder, GetTotalMatch(pBPvPArenaSO, mode), _timeAfterEndRound);
                AnalyticsManager.LogBattleInteraction(pbMatch.arenaSO.index + 1, pbMatch.rankOfMine, GetTotalMatch(pBPvPArenaSO, mode), _pvpBattleInteraction.CaculatedPercentInteraction());
            }


            if (pbMatch.rankOfMine == 1)
            {
                if (pprefPlayerKillNumber != null)
                    AnalyticsManager.LogPVPPlayerKillBattleMode(pbMatch.arenaSO.index, nameStage, pprefPlayerKillNumber.value, _timeAfterEndRound);
            }
        }

        string status;
        if (matchAbandoned)
        {
            status = "Abandon";
            if (_currentChosenModeVariable.value == Mode.Normal)
            {
                pprefWinStreak.value = 0;
                pprefLoseStreak.value = 0;
                //HandleEventMatchSingleMode(status, match, _timeAfterEndRound);
            }
        }
        else
        {
            status = match.isVictory ? "Victory" : "Defeated";
            if (_currentChosenModeVariable.value == Mode.Normal)
            {
                HandleEventWinOrLoseSingleMode(match.isVictory, match);
            }
        }
        var botInfo = match.GetOpponentInfo().Cast<PBBotInfo>();
        var data = (PBRPSCalculatorSO.PBRPSData)rpsCalculatorSO.CalcCurrentRPSValue();
        AnalyticsManager.LogAIProfile(botInfo.aiProfile.BossType.ToString(), GetAdvantageStateString(data), status);
    }

    private void UpdatePPrefStreak(PPrefIntVariable pPrefIntVariable_1, PPrefIntVariable pPrefIntVariable_2, PvPMatch match)
    {
        pPrefIntVariable_1.value++;
        pPrefIntVariable_2.value = 0;
        if (pPrefIntVariable_1.value >= 2)
        {
            AnalyticsManager.Streak(match.isVictory ? "Victory" : "Defeated", pPrefIntVariable_1.value);
        }
    }

    private string GetAdvantageStateString(PBRPSCalculatorSO.PBRPSData data) =>
        data.stateLabelInternal == 1 ? "PlayerAdvantage" : (data.stateLabelInternal == 0 ? "Even" : "OpponentAdvantage");

    private IEnumerator CountTimeBattle()
    {
        WaitForSeconds time = new WaitForSeconds(1);
        while (true)
        {
            _timeAfterEndRound++;
            yield return time;
        }
    }

    private string GetNameMode()
    {
        if (_currentChosenModeVariable == Mode.Normal)
            return "SinglePvP";
        else if (_currentChosenModeVariable == Mode.Boss)
            return "BossFight";
        else
            return "BattlePvP";
    }

    private void SetTotalMatchArena(PBPvPArenaSO pBPvPArenaSO, Mode mode)
    {
        string key = pBPvPArenaSO.index.ToString() + mode;
        if (PlayerPrefs.HasKey(key))
        {
            int currentCountMatch = PlayerPrefs.GetInt(key);
            PlayerPrefs.SetInt(key, currentCountMatch + 1);
            return;
        }
        PlayerPrefs.SetInt(key, 1);
    }

    private int GetTotalMatch(PvPArenaSO pvpArenaSO, Mode mode)
    {
        if (_tournamentSO == null) return 0;

        PvPArenaSO pBPvPArena = _tournamentSO.arenas.Find(v => v == pvpArenaSO);

        string key = pBPvPArena.index.ToString() + mode;

        int countMatch = 0;
        if (PlayerPrefs.HasKey(key))
        {
            countMatch = PlayerPrefs.GetInt(key);
        }
        else
        {
            PlayerPrefs.SetInt(key, 1);
            countMatch = 1;
        }
        return countMatch;
    }

    private void FlipTiming(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;

        string status = (string)parameters[0];
        int arenaIndex = currentHighestArenaVariable.value.index + 1;
        AnalyticsManager.LogFlipTiming(arenaIndex, status);
    }
    #endregion

    #region Achieve Event
    private void HandlePlayerPassingMilestone(object[] objs)
    {
        if (objs[0] is not int milestone) return;
        AnalyticsManager.Ranking(milestone);
    }

    private void OnUnpackStart(params object[] parameters)
    {
        //if (parameters.Length <= 0) return;

        //if (parameters[1] is List<GachaPack>)
        //{
        //    var packsToOpen = (List<GachaPack>)parameters[1];
        //    if (packsToOpen.Count > 0)
        //    {
        //        int currentArenaIndex = currentHighestArenaVariable.value.index + 1;
        //        AnalyticsManager.LogOpenBox(currentArenaIndex, GetQuantityBoxFollowingCurrentArena(currentArenaIndex, packsToOpen.Count));
        //    }
        //}
    }

    private int GetQuantityBoxFollowingCurrentArena(int currentArenaIndex, int boxCount)
    {
        string keyBoxFollowingArena = $"QuantityBox-{currentArenaIndex}";
        int count = PlayerPrefs.GetInt(keyBoxFollowingArena, 0);
        count += boxCount;
        PlayerPrefs.SetInt(keyBoxFollowingArena, count);
        return count;
    }
    #endregion

    #region Accessibility Event
    private void FeatureUsed(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null) return;
        string usedName = (string)parrams[0];
        int value = (int)parrams[1];

        AnalyticsManager.LogFeatureUsed(usedName, value);
    }

    private void FlipButtonClicked(object[] objs)
    {
        
    }

    private void SwitchCameraEvent(object[] objs)
    {
        if (objs[0] is not int) return;
        int cameraIndex = (int)objs[0];

        string nameCameraSwitch = "";
        if (cameraIndex == 0)
            nameCameraSwitch = "Cam_ThirdView";
        else if (cameraIndex == 1)
            nameCameraSwitch = "Cam_TopDownView";
        else
            nameCameraSwitch = "Cam_FirstView";

        
    }
    #endregion

    #region Resource Event
    private void OnAcquireCurrency(params object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0) return;
        var currencyType = (CurrencyType)parameters[0];

        if (currencyType == CurrencyType.Medal) return;
        var amount = (float)parameters[1];
        var resourceLocation = (ResourceLocation)parameters[2];
        var itemId = (string)parameters[3];

        PBAnalyticsManager.Instance.AcquireResource(currencyType.ToString(), itemId, resourceLocation.ToString(), amount);

        #region Progression Event
        string keyCurrencyProgression = $"SaveSource-{GetMissionIDCurrent()}-{currencyType}";
        int valueLogProgressionEvent = 0;
        if (PlayerPrefs.HasKey(keyCurrencyProgression))
        {
            int valueCurrent = PlayerPrefs.GetInt(keyCurrencyProgression);
            int valueHandle = valueCurrent + (int)amount;
            PlayerPrefs.SetInt(keyCurrencyProgression, valueHandle);
        }
        else
        {
            valueLogProgressionEvent = (int)amount;
            PlayerPrefs.SetInt(keyCurrencyProgression, valueLogProgressionEvent);
        }
        #endregion

        #region Firebase Event
        ResourceLocation resourceLocationCurrency = (ResourceLocation)parameters[2];
        if (m_ResourceType.ContainsKey(resourceLocationCurrency))
        {
            int balance = (int)m_CurrencyDic.initialValue[currencyType].value;
            string source = itemId;
            string currencyName = currencyType switch
            {
                CurrencyType.Standard => "coins",
                CurrencyType.Premium => "red_diamonds",
                CurrencyType.RVTicket => "RVTicket",
                CurrencyType.EventTicket => "EventTicket",
                _ => "null",
            };

            string sourceName = $"{resourceLocation}_{itemId}";
            if (resourceLocation == ResourceLocation.Box)
                sourceName = itemId;

            if (currencyName != "null")
                GameEventHandler.Invoke(LogFirebaseEventCode.CurrencyTransaction, +(int)amount, balance, sourceName, currencyName);
        }
        #endregion
    }

    private void OnSpendCurrency(params object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0) return;
        var currencyType = (CurrencyType)parameters[0];
        var amount = (float)parameters[1];
        var resourceLocation = (ResourceLocation)parameters[2];
        var itemId = (string)parameters[3];

        PBAnalyticsManager.Instance.ConsumeResource(currencyType.ToString(), itemId, resourceLocation.ToString(), amount);

        #region Progression Event
        string keyCurrencyProgression = $"SaveSink-{GetMissionIDCurrent()}-{currencyType}";
        int valueLogProgressionEvent = 0;
        if (PlayerPrefs.HasKey(keyCurrencyProgression))
        {
            int valueCurrent = PlayerPrefs.GetInt(keyCurrencyProgression, 0);
            int valueHandle = valueCurrent + (int)amount;
            PlayerPrefs.SetInt(keyCurrencyProgression, valueHandle);
        }
        else
        {
            valueLogProgressionEvent = (int)amount;
            PlayerPrefs.SetInt(keyCurrencyProgression, valueLogProgressionEvent);
        }
        #endregion

        #region Firebase Event
        ResourceLocation resourceLocationCurrency = (ResourceLocation)parameters[2];
        if (m_ResourceType.ContainsKey(resourceLocationCurrency))
        {
            int balance = (int)m_CurrencyDic.initialValue[currencyType].value;
            string currencyName = currencyType switch
            {
                CurrencyType.Standard => "coins",
                CurrencyType.Premium => "red_diamonds",
                CurrencyType.RVTicket => "RVTicket",
                CurrencyType.EventTicket => "EventTicket",
                _ => "null",
            };

            string sourceName = $"{resourceLocation}_{itemId}";
            if (resourceLocation == ResourceLocation.Box)
                sourceName = itemId;

            if (currencyName != "null")
                GameEventHandler.Invoke(LogFirebaseEventCode.CurrencyTransaction, -(int)amount, balance, sourceName, currencyName);
        }
        #endregion
    }    
    private void SkillCardSource(params object[] parameters)
    {
        if (parameters == null || parameters[0] == null && parameters[1] == null) return;
        float skillCardCount = (float)parameters[0];
        ResourceLocationProvider resourceLocationProvider = (ResourceLocationProvider)parameters[1];
        PBAnalyticsManager.Instance.AcquireResource(CurrencyType.SkillCard.ToString(), resourceLocationProvider.GetItemId(), resourceLocationProvider.GetLocation().ToString(), skillCardCount);
    }    
    private void SkillCardSink(params object[] parameters)
    {
        if (parameters == null || parameters[0] == null || parameters[1] == null || parameters[2] == null || parameters[3] == null) return;
        int numOfCard = (int)parameters[2];
        int changedAmount = (int)parameters[3];

        if (changedAmount < 0)
        {
            ResourceLocationProvider resourceLocationProvider = new ResourceLocationProvider(_currentChosenModeVariable.value == Mode.Normal ? ResourceLocation.SinglePvP : ResourceLocation.BattlePvP, "");
            PBAnalyticsManager.Instance.ConsumeResource(CurrencyType.SkillCard.ToString(), resourceLocationProvider.GetItemId(), resourceLocationProvider.GetLocation().ToString(), Math.Abs(changedAmount));
        }
    }
    private int GetMissionIDCurrent()
    {
        if (!PlayerPrefs.HasKey(FireBaseEvent.GetLastMissionStartKey()))
            PlayerPrefs.SetInt(FireBaseEvent.GetLastMissionStartKey(), 1);
        return PlayerPrefs.GetInt(FireBaseEvent.GetLastMissionStartKey(), 1);
    }
    private void OnMilestoneInitialized()
    {
        if (_trophyRoadSO == null || _trophyRoadSO.ArenaSections == null)
            return;
        foreach (var arenaSection in _trophyRoadSO.ArenaSections)
        {
            foreach (var milestone in arenaSection.milestones)
            {
                milestone.OnUnlocked += OnMilestoneUnlocked;

                void OnMilestoneUnlocked()
                {
                    milestone.OnUnlocked -= OnMilestoneUnlocked;

                    string keyCurrencySourceStandard = $"SaveSource-{GetMissionIDCurrent()}-{CurrencyType.Standard}";
                    string keyCurrencySourcePremium = $"SaveSource-{GetMissionIDCurrent()}-{CurrencyType.Premium}";
                    string keyCurrencySourceRVTicket = $"SaveSource-{GetMissionIDCurrent()}-{CurrencyType.RVTicket}";
                    string keyCurrencySourceEventTicket = $"SaveSource-{GetMissionIDCurrent()}-{CurrencyType.EventTicket}";

                    string keyCurrencySinkStandard = $"SaveSink-{GetMissionIDCurrent()}-{CurrencyType.Standard}";
                    string keyCurrencySinkPremium = $"SaveSink-{GetMissionIDCurrent()}-{CurrencyType.Premium}";
                    string keyCurrencySinkRVTicket = $"SaveSink-{GetMissionIDCurrent()}-{CurrencyType.RVTicket}";
                    string keyCurrencySinkEventTicket = $"SaveSink-{GetMissionIDCurrent()}-{CurrencyType.EventTicket}";

                    int valueCurrentSourceStand = PlayerPrefs.GetInt(keyCurrencySourceStandard);
                    int valueCurrentSourcePremium = PlayerPrefs.GetInt(keyCurrencySourcePremium);
                    int valueCurrentSourceRVTicket = PlayerPrefs.GetInt(keyCurrencySourceRVTicket);
                    int valueCurrentSourceEventTicket = PlayerPrefs.GetInt(keyCurrencySourceEventTicket);                    
                    
                    int valueCurrentSinkStand = PlayerPrefs.GetInt(keyCurrencySinkStandard);
                    int valueCurrentSinkPremium = PlayerPrefs.GetInt(keyCurrencySinkPremium);
                    int valueCurrentSinkRVTicket = PlayerPrefs.GetInt(keyCurrencySinkRVTicket);
                    int valueCurrentSinkEventTicket = PlayerPrefs.GetInt(keyCurrencySinkEventTicket);

                    AnalyticsManager.SourceProgressionEvent("Start", GetMissionIDCurrent(), CurrencyType.Standard, valueCurrentSourceStand);
                    AnalyticsManager.SinkProgressionEvent("Start", GetMissionIDCurrent(), CurrencyType.Standard, valueCurrentSinkStand);
                    AnalyticsManager.SourceProgressionEvent("Start", GetMissionIDCurrent(), CurrencyType.Premium, valueCurrentSourcePremium);
                    AnalyticsManager.SinkProgressionEvent("Start", GetMissionIDCurrent(), CurrencyType.Premium, valueCurrentSinkPremium);
                    AnalyticsManager.SourceProgressionEvent("Start", GetMissionIDCurrent(), CurrencyType.RVTicket, valueCurrentSourceRVTicket);
                    AnalyticsManager.SinkProgressionEvent("Start", GetMissionIDCurrent(), CurrencyType.RVTicket, valueCurrentSinkRVTicket);
                    AnalyticsManager.SourceProgressionEvent("Start", GetMissionIDCurrent(), CurrencyType.EventTicket, valueCurrentSourceEventTicket);
                    AnalyticsManager.SinkProgressionEvent("Start", GetMissionIDCurrent(), CurrencyType.EventTicket, valueCurrentSinkEventTicket);
                }
            }
        }
    }
    #endregion

    #region FTUE Event
    private void StartFightButton() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.FightButton);

    private void EndFightButton() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.FightButton);

    private void StartControlFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.Control);

    private void StartReverseFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.Reverse);

    private void EndReverseFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.Reverse);

    private void EndControlFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.Control);

    private void StartOpenBox1() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.OpenBox1);

    private void EndOpenBox1() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.OpenBox1);

    private void StartEquip_1FTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.Equip_1);

    private void EndEquip_1FTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.Equip_1);

    private void StartPowerFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.Power);

    private void EndPowerFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.Power);

    private void StartUpgradeFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.Upgrade);

    private void EndUpgradeFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.Upgrade);

    private void StartPlaySingleFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.PvP);

    private void EndPlaySingleFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.PvP);

    private void StartOpenBox2() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.OpenBox2);

    private void EndOpenBox2() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.OpenBox2);

    //private void StartBuildTab() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.BuildTab);

    //private void EndBuildTab() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.BuildTab);

    private void StartEquip_2FTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.Equip_2);

    private void EndEquip_2FTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.Equip_2);

    private void StartEnterBossUIFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.EnterBossUI);

    private void EndEnterBossUIFTUE()
    {
        //Avoid Missing Event
        AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.EnterBossUI);
        //==================
        AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.EnterBossUI);
    }

    private void StartPlayBossFightFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.PlayBossFight);

    private void EndPlayBossFightFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.PlayBossFight);

    private void StartSkinUIFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.SkinUI);

    private void EndSkinUIFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.SkinUI);

    private void StartPlayBattleFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.PlayBattle);

    private void EndPlayBattleFTUE() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.PlayBattle);
    private void StartPreludeSeason() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.PreludeSeason);
    private void EndPreludeSeason() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.PreludeSeason);
    private void StartLeague() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.League);
    private void EndLeague() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.League);
    private void StartActiveSkillEnter() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.ActiveSkillEnter);
    private void EndActiveSkillEnter() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.ActiveSkillEnter);
    private void StartActiveSkillClaim() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.ActiveSkillClaim);
    private void EndActiveSkillClaim() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.ActiveSkillClaim);
    private void StartActiveSkillEquip() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.ActiveSkillEquip);
    private void EndActiveSkillEquip() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.ActiveSkillEquip);
    private void StartActiveSkillInGame() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Start, FTUEType.ActiveSkillIngame);
    private void EndActiveSkillInGame() => AnalyticsManager.LogFTUEEvent(FTUEProgressState.Completed, FTUEType.ActiveSkillIngame);
    #endregion

    #region Monetization Event

    private string[] PlaceAdsLocationAndArenaIndex(params object[] parrams)
    {
        string location = $"{(string)parrams[0]}";
        string arenaIndex = $"{currentHighestArenaVariable.value.index + 1}";
        string[] placeAds = { $"{location}", $"{arenaIndex}" };

        return placeAds;
    }
    private string[] PlaceAdsBossNameLocationAndArenaIndex(params object[] parrams)
    {
        string location = $"{(string)parrams[0]}";
        string bossName = $"{(string)parrams[1]}";
        string arenaIndex = $"{currentHighestArenaVariable.value.index + 1}";
        string[] placeAds = { $"{location}", $"{bossName}", $"{arenaIndex}" };

        return placeAds;
    }

    private string GetGroupName()
    {
        if (pbGameAnalyticsService != null)
        {
            return pbGameAnalyticsService.GroupName;
        }
        return "";

    }

    private void MonetizationMultiplierRewards(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null || parrams[2] == null) return;
        string arenaIndex = $"{currentHighestArenaVariable.value.index + 1}";
        string set = (string)parrams[0];
        string location = (string)parrams[1];
        Mode mode = (Mode)parrams[2];

        if (!m_IsHasRVTicket)
        {
            string adsPlacement = $"RV{groupName}:A{arenaIndex}:MultiplierRewards:{set}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
        }
        else
        {
            string adsPlacement = $"RVTicket{groupName}:A{arenaIndex}:MultiplierRewards:{set}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            ResourceLocation resourceLocationRVTicketSink = mode switch
            {
                Mode.Normal => ResourceLocation.SinglePvP,
                Mode.Battle => ResourceLocation.BattlePvP,
                _ => ResourceLocation.None
            };
            if (resourceLocationRVTicketSink == ResourceLocation.None)
                Debug.LogError("ResourceLocation None");

            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"Multiplier{set}";
            OnSpendCurrency(currencyType, amount, resourceLocationRVTicketSink, itemId);
            #endregion
        }
    }

    private void Monetization_ReviveBoss(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        int bossID = (int)parrams[1];
        string location = (string)parrams[0];
        string arenaIndex = $"{currentHighestArenaVariable.value.index + 1}";
        string groupName = GetGroupName();

        if (!m_IsHasRVTicket)
        {
            string adsPlacement = $"RV{groupName}:A{arenaIndex}:Revive:Boss{bossID}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
        }
        else
        {
            string adsPlacement = $"RVTicket{groupName}:A{arenaIndex}:Revive:Boss{bossID}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            if (BossFightManager.Instance != null)
            {
                float amount = 1f;
                var itemId = $"Revive_{bossID}";
                OnSpendCurrency(CurrencyType.RVTicket, amount, ResourceLocation.BossFight, itemId);
            }
            #endregion
        }
    }

    //====================================================================================================================================
    private void MonetizationRevengeBoss_BossFightUI(params object[] parrams)
    {
        //HandleMonetizaionBoss("RevengeBoss", AdsLocation.RV_Revenge_Boss_Boss_Fight_UI, parrams);
    }

    private void MonetizationRevengeBoss_LoseBossUI(params object[] parrams)
    {
        //HandleMonetizaionBoss("ClaimBoss", AdsLocation.RV_Revenge_Boss_Lose_Boss_UI, parrams);
    }

    private void MonetizationClaimBoss_InventoryUI(params object[] parrams)
    {
        HandleMonetizaionBoss("ClaimBoss", AdsLocation.RV_Claim_Boss_Inventory_UI, parrams);
    }

    private void MonetizationClaimBoss_WinBossUI(params object[] parrams)
    {
        HandleMonetizaionBoss("ClaimBoss", AdsLocation.RV_Claim_Boss_Win_Boss_UI, parrams);
    }

    private void HandleMonetizaionBoss(string typeName, AdsLocation adsLocation, params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null || parrams[2] == null) return;
        int bossID = (int)parrams[1];
        string location = (string)parrams[0];
        string currentArena = $"{currentHighestArenaVariable.value.index + 1}";
        string groupName = GetGroupName();
        int adsCount = (int)parrams[2];

        if (!m_IsHasRVTicket)
        {
            string adsPlacement = $"RV{groupName}:A{currentArena}:ClaimBoss:Boss{bossID}:Times_{adsCount}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
        }
        else
        {
            string adsPlacement = $"RVTicket{groupName}:A{currentArena}:ClaimBoss:Boss{bossID}:Times_{adsCount}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            float amount = 1f;
            var itemId = $"ClaimBoss_{bossID}";
            OnSpendCurrency(CurrencyType.RVTicket, amount, ResourceLocation.BossFight, itemId);
            #endregion
        }
    }

    //====================================================================================================================================
    private void MonetizationFreeBox_MainUI(params object[] parrams)
    {
        //HandleMonetizationEvent("FreeBox", AdsLocation.RV_Free_Box_Main_UI, parrams);
    }

    private void MonetizationUpgrade(params object[] parrams)
    {
        //HandleMonetizationEvent("Upgrade", AdsLocation.RV_Upgrade_Card_Upgrade_Exchange_UI, parrams);
    }

    private void MonetizationBonusCard_OpenBoxUI(params object[] parrams)
    {
        if (parrams.Length <= 1) return;
        if (parrams[0] == null || parrams[1] == null) return;

        string arenaIndex = $"{currentHighestArenaVariable.value.index + 1}";
        string location = (string)parrams[0];
        GroupType groupType = (GroupType)parrams[1];

        string cardGroup = groupType switch
        {
            GroupType.NewAvailable => "NewGroup",
            GroupType.Duplicate => "DuplicateGroup",
            GroupType.InUsed => "InUsedGroup",
            _ => "AvailableGroup"
        }; ;


        if (!m_IsHasRVTicket)
        {
            string adsPlacement = $"RV{groupName}:A{arenaIndex}:BonusCard:{cardGroup}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
        }
        else
        {
            string adsPlacement = $"RVTicket{groupName}:A{arenaIndex}:BonusCard:{cardGroup}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            string typeCard = groupType switch
            {
                GroupType.NewAvailable => "New",
                GroupType.Duplicate => "Duplicate",
                GroupType.InUsed => "InUsed",
                _ => "null"
            };
            float amount = 1f;
            var itemId = $"Part_{typeCard}";
            OnSpendCurrency(CurrencyType.RVTicket, amount, ResourceLocation.BonusCard, itemId);
            #endregion
        }
    }

    private void MonetizationBonusCard_YouGotUI(params object[] parrams)
    {
        //HandleMonetizationEvent("BonusCard", AdsLocation.RV_Bonus_Card_Open_Box_UI, parrams);
    }

    private void HandleMonetizationEvent(string typeName, AdsLocation adsLocation, params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null) return;
        string location = (string)parrams[0];
        string arenaIndex = $"{currentHighestArenaVariable.value.index + 1}";
        string groupName = GetGroupName();

        AnalyticsManager.RewardedAdCompleted(adsLocation, PlaceAdsLocationAndArenaIndex(parrams));
        AnalyticsManager.DesignAdsEvent($"RV{groupName}:A{arenaIndex}:{typeName}:{location}");
    }

    //====================================================================================================================================
    private void OpenNowBox_BoxPopup(params object[] parrams)
    {
        HandleBoxAction(AdsLocation.RV_Open_Now_Box_Slot_UI, "OpenNowBox", parrams);
    }

    private void OpenNowBox_GameOverUI(params object[] parrams)
    {
        HandleBoxAction(AdsLocation.RV_Open_Now_Box_Slot_GameOver_UI, "OpenNowBox", parrams);
    }

    private void SpeedUpBox(params object[] parrams)
    {
        HandleBoxAction(AdsLocation.RV_Speed_Up_Box_Slot_UI, "SpeedupBox", parrams);
    }

    private void HandleBoxAction(AdsLocation adsLocation, string actionType, params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;

        string typeBox = (string)parrams[1];
        string location = (string)parrams[0];
        string arenaIndex = $"{currentHighestArenaVariable.value.index + 1}";
        string groupName = GetGroupName();

        if (!m_IsHasRVTicket)
        {
            string adsPlacement = $"RV{groupName}:A{arenaIndex}:{actionType}:{typeBox}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
        }
        else
        {
            string adsPlacement = $"RVTicket{groupName}:A{arenaIndex}:{actionType}:{typeBox}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            float amount = 1f;
            var itemId = $"{actionType}_{typeBox}";
            OnSpendCurrency(CurrencyType.RVTicket, amount, ResourceLocation.RV, itemId);
            #endregion
        }
    }

    //====================================================================================================================================
    private void Monetization_RechargeBoss_AlertPopup(params object[] parrams)
    {
        //Monetization_RechargeBoss(parrams);
    }

    private void Monetization_RechargeBoss_BuildUI(params object[] parrams)
    {
        //Monetization_RechargeBoss(parrams);
    }

    private void Monetization_RechargeBoss_BossPopup(params object[] parrams)
    {
        //Monetization_RechargeBoss(parrams);
    }

    private void Monetization_RechargeBoss(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;

        string bossName = (string)parrams[1];
        string location = (string)parrams[0];
        string arenaIndex = $"{currentHighestArenaVariable.value.index + 1}";
        string groupName = GetGroupName();

        AnalyticsManager.RewardedAdCompleted(AdsLocation.RV_Recharge_Boss_Alert_Popup, PlaceAdsLocationAndArenaIndex(parrams));
        AnalyticsManager.DesignAdsEvent($"RV{groupName}:A{arenaIndex}:RechargeBoss:{bossName}:{location}");
    }

    private void Monetization_GetSkin(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null || parrams[2] == null || parrams[3] == null) return;

        string partType = (string)parrams[0];
        string partName = (string)parrams[1];
        string skinIndex =  (string)parrams[2];
        int adsCount = (int)parrams[3];
        string currentArena = $"{currentHighestArenaVariable.value.index + 1}";
        string groupName = GetGroupName();

        if (!m_IsHasRVTicket)
        {
            string adsPlacement = $"RV{groupName}:A{currentArena}:UnlockSkin:{partName}-{skinIndex}:Times_{adsCount}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
        }
        else
        {
            string adsPlacement = $"RVTicket{groupName}:A{currentArena}:UnlockSkin:{partName}-{skinIndex}:Times_{adsCount}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"{partName}_{skinIndex}";
            OnSpendCurrency(currencyType, amount, ResourceLocation.UnlockSkin, itemId);
            #endregion
        }
    }

    private void Monetization_HotOffers(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;

        string arenaIndex = $"{currentHighestArenaVariable.value.index + 1}";
        string location = (string)parrams[0];
        string offer = (string)parrams[1];

        #region Design Event
        string currency = m_IsHasRVTicket ? "RVTicket" : "RV";
        GameEventHandler.Invoke(DesignEvent.HotOfferBuy, currency, offer);
        #endregion

        if (!m_IsHasRVTicket)
        {
            string adsPlacement = $"RV{groupName}:A{arenaIndex}:HotOffers:{offer}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
        }
        else
        {
            string adsPlacement = $"RVTicket{groupName}:A{arenaIndex}:HotOffers:{offer}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"{offer}";
            itemId = itemId switch
            {
                "NewGroup" => "Part_New",
                "DuplicateGroup" => "Part_Duplicate",
                "InUsedGroup" => "Part_InUsed",
                _ => itemId
            };

            OnSpendCurrency(currencyType, amount, ResourceLocation.HotOffer, itemId);
            #endregion
        }
    }

    private void Monetization_HotOffersUpgradePopup(params object[] parrams)
    {
        Monetization_HotOffers(parrams);
    }

    private void Monetization_HotOffersResourcePopup(params object[] parrams)
    {
        Monetization_HotOffers(parrams);
    }

    private void Monetization_HotOffersShop(params object[] parrams)
    {
        Monetization_HotOffers(parrams);
    }

    private void Monetization_TrophyBypassBossUI(params object[] parrams)
    {
        if(parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;

        string arenaIndex = $"{currentHighestArenaVariable.value.index + 1}";
        string bossname = (string)parrams[0];
        string location = (string)parrams[1];
        string bossID = (string)parrams[2];

        if (!m_IsHasRVTicket)
        {
            string adsPlacement = $"RV{groupName}:A{arenaIndex}:TrophyBypass:Boss{bossID}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
        }
        else
        {
            string adsPlacement = $"RVTicket{groupName}:A{arenaIndex}:TrophyBypass:Boss{bossID}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"TrophyBypass_{bossID}";
            OnSpendCurrency(currencyType, amount, ResourceLocation.BossFight, itemId);
            #endregion
        }
    }

    private void Monetization_LinkRewards(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null || parrams[2] == null || parrams[3] == null) return;

        string arenaIndex = $"{currentHighestArenaVariable.value.index + 1}";
        string location = (string)parrams[0];
        string name = (string)parrams[1];
        string set = (string)parrams[2];
        string rewardID = (string)parrams[3];

        if (!m_IsHasRVTicket)
        {
            string adsPlacement = $"RV{groupName}:A{arenaIndex}:{name}:{set}_Reward{rewardID}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
        }
        else
        {
            string adsPlacement = $"RVTicket{groupName}:A{arenaIndex}:{name}:{set}_Reward{rewardID}:{location}";
            AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
            AnalyticsManager.DesignAdsEvent(adsPlacement);
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"Reward_{rewardID}";
            OnSpendCurrency(currencyType, amount, ResourceLocation.LinkRewards, itemId);
            #endregion
        }
    }

    private void Monetization_LinkRewardsMainUI(params object[] parrams)
    {
        Monetization_LinkRewards(parrams);
    }

    private void Monetization_LinkRewardsShop(params object[] parrams)
    {
        Monetization_LinkRewards(parrams);
    }

    private void Monetization_KeepWinStreak(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        int currentStreak = (int)parrams[0];
        string location = (string)parrams[1];
        int currentArena = currentHighestArenaVariable.value.index + 1;
        string adsPlacement = "";
        if (!m_IsHasRVTicket)
        {
            adsPlacement = $"RV{groupName}:A{currentArena}:KeepWinStreak:Streak{currentStreak}:{location}";
        }
        else
        {
            adsPlacement = $"RVTicket{groupName}:A{currentArena}:KeepWinStreak:Streak{currentStreak}:{location}";
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"WinStreakMilestone";
            OnSpendCurrency(currencyType, amount, ResourceLocation.WinStreak, itemId);
            #endregion
        }
        AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
        AnalyticsManager.DesignAdsEvent(adsPlacement);
    }

    private void Monetization_WinStreakPremium(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        int streakCount = (int)parrams[0];
        string location = (string)parrams[1];
        string adsPlacement = "";

        if (!m_IsHasRVTicket)
        {
            adsPlacement = $"RV{groupName}:A{currentArena}:WinStreakPremium:{streakCount}:{location}";
        }
        else
        {
            adsPlacement = $"RVTicket{groupName}:A{currentArena}:WinStreakPremium:{streakCount}:{location}";
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"WinStreakPremium";
            OnSpendCurrency(currencyType, amount, ResourceLocation.SinglePvP, itemId);
            #endregion
        }
        AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
        AnalyticsManager.DesignAdsEvent(adsPlacement);
    }

    private void Monetization_GetBack(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        int choosenArena = (int)parrams[0];
        string location = (string)parrams[1];
        string adsPlacement = "";
        if (!m_IsHasRVTicket)
        {
            adsPlacement = $"RV{groupName}:A{currentArena}:GetBack:{choosenArena}:{location}";
        }
        else
        {
            adsPlacement = $"RVTicket{groupName}:A{currentArena}:GetBack:{choosenArena}:{location}";
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"GetBack";
            OnSpendCurrency(currencyType, amount, ResourceLocation.BattlePvP, itemId);
            #endregion
        }
        AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
        AnalyticsManager.DesignAdsEvent(adsPlacement);
    }

    private void Monetization_GetMissingCard(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        string partName = (string)parrams[0];
        string location = (string)parrams[1];

        string adsPlacement = "";
        if (!m_IsHasRVTicket)
        {
            adsPlacement = $"RV{groupName}:A{currentArena}:GetMissingCard:{partName}:{location}";
        }
        else
        {
            adsPlacement = $"RVTicket{groupName}:A{currentArena}:GetMissingCard:{partName}:{location}";
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"GetMissingCard";
            OnSpendCurrency(currencyType, amount, ResourceLocation.UpgradePart, itemId);
            #endregion
        }
        AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
        AnalyticsManager.DesignAdsEvent(adsPlacement);
    }

    private void Monetization_BuyGarage(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        int garageIndex = (int)parrams[0];
        int adsCount = (int)parrams[1];

        string adsPlacement = "";
        if (!m_IsHasRVTicket)
        {
            adsPlacement = $"RV{groupName}:A{currentArena}:BuyGarage:{garageIndex}:Times_{adsCount}";
        }
        else
        {
            adsPlacement = $"RVTicket{groupName}:A{currentArena}:BuyGarage:{garageIndex}:Times_{adsCount}";
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"Garage_{garageIndex}";
            OnSpendCurrency(currencyType, amount, ResourceLocation.UnlockSkin, itemId);
            #endregion
        }
        AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
        AnalyticsManager.DesignAdsEvent(adsPlacement);
    }

    private void Monetization_SkipMission(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        string missionType = (string)parrams[0];
        string missionID = (string)parrams[1];

        string adsPlacement = "";
        if (!m_IsHasRVTicket)
        {
            adsPlacement = $"RV{groupName}:A{currentArena}:SkipMission:{missionType}:{missionID}";
        }
        else
        {
            adsPlacement = $"RVTicket{groupName}:A{currentArena}:SkipMission:{missionType}:{missionID}";
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"SkipMission";
            OnSpendCurrency(currencyType, amount, ResourceLocation.Season, itemId);
            #endregion
        }

        AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
        AnalyticsManager.DesignAdsEvent(adsPlacement);
    }
    private void Monetization_ClaimDoubleMission(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        string missionType = (string)parrams[0];
        int accumulate = (int)parrams[1];

        string adsPlacement = "";
        if (!m_IsHasRVTicket)
        {
            adsPlacement = $"RV{groupName}:A{currentArena}:ClaimDoubleMission:{missionType}:{accumulate}";
        }
        else
        {
            adsPlacement = $"RVTicket{groupName}:A{currentArena}:ClaimDoubleMission:{missionType}:{accumulate}";
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"ClaimDoubleMission";
            OnSpendCurrency(currencyType, amount, ResourceLocation.Season, itemId);
            #endregion
        }

        AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
        AnalyticsManager.DesignAdsEvent(adsPlacement);
    }
    private void Monetization_BuyCharacter(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        int characterID = (int)parrams[0];
        int timeRV = (int)parrams[1];

        string adsPlacement = "";
        if (!m_IsHasRVTicket)
        {
            adsPlacement = $"RV{groupName}:A{currentArena}:BuyCharacter:{characterID}:Times_{timeRV}";
        }
        else
        {
            adsPlacement = $"RVTicket{groupName}:A{currentArena}:BuyCharacter:{characterID}:Times_{timeRV}";
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = $"Character_{characterID}";
            OnSpendCurrency(currencyType, amount, ResourceLocation.UnlockSkin, itemId);
            #endregion
        }

        AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
        AnalyticsManager.DesignAdsEvent(adsPlacement);
    }
    private void Monetization_FreeEventTicket(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        string location = (string)parrams[0];

        string adsPlacement = "";
        if (!m_IsHasRVTicket)
        {
            adsPlacement = $"RV{groupName}:A{currentArena}:FreeEventTicket:{location}";
        }
        else
        {
            adsPlacement = $"RVTicket{groupName}:A{currentArena}:FreeEventTicket:{location}";
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = location == "Shop" ? "FreeEventTicket_Shop" : "FreeEventTicket_ResourcePopup";
            OnSpendCurrency(currencyType, amount, ResourceLocation.RV, itemId);
            #endregion
        }

        AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
        AnalyticsManager.DesignAdsEvent(adsPlacement);
    }    
    private void Monetization_LadderedOffer(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null || parrams[1] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        string set = (string)parrams[0];
        string packName = (string)parrams[1];

        string adsPlacement = "";
        if (!m_IsHasRVTicket)
        {
            adsPlacement = $"RV{groupName}:A{currentArena}:LadderedOffer:{set}:{packName}";
        }
        else
        {
            adsPlacement = $"RVTicket{groupName}:A{currentArena}:LadderedOffer:{set}:{packName}";
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = "LadderedOffer";
            OnSpendCurrency(currencyType, amount, ResourceLocation.RV, itemId);
            #endregion
        }

        AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
        AnalyticsManager.DesignAdsEvent(adsPlacement);
    }
    private void Monetization_FreesSkill(params object[] parrams)
    {
        string adsPlacement = "";
        int currentArena = currentHighestArenaVariable.value.index + 1;
        if (!m_IsHasRVTicket)
        {
            adsPlacement = $"RV{groupName}:A{currentArena}:FreeSkill";
        }
        else
        {
            adsPlacement = $"RVTicket{groupName}:A{currentArena}:FreeSkill";
            m_IsHasRVTicket = false;

            #region Sink RVTicket
            CurrencyType currencyType = CurrencyType.RVTicket;
            float amount = 1f;
            var itemId = "FreeSkill";
            OnSpendCurrency(currencyType, amount, ResourceLocation.RV, itemId);
            #endregion
        }

        AnalyticsManager.CustomRewardedAdCompleted(adsPlacement);
        AnalyticsManager.DesignAdsEvent(adsPlacement);
    }

    //====================================================================================================================================
    #endregion

    #region BossEvent
    private string GetBossDifficulty()
    {
        string difficulty = "even";
        if (rpsCalculatorSO != null)
            difficulty = rpsCalculatorSO.CalcCurrentRPSValue().stateLabel.ToLower();
        return difficulty;
    }
    private void StartBossFight(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        if (parameters[0] is BossMapSO)
        {
            BossMapSO bossMapSO = parameters[0] as BossMapSO;
            CallEventBossChapter(bossMapSO, "Start", bossMapSO.currentChapterSO.chapterName, bossMapSO.currentChapterSO.bossIndex.value + 1);

            #region Firebase Event
            bool isStarted = true;
            string bossName = bossMapSO.currentChapterSO.currentBossSO.chassisSO.GetModule<NameItemModule>().displayName;
            string bossDifficulty = GetBossDifficulty();
            string driver = m_CharacterManagerSO.PlayerCharacterSO.value.GetDisplayName();
            RPSData rpsData = rpsCalculatorSO.CalcCurrentRPSValue();
            int scorePlayer = (int)rpsData.scoreOfPlayer;
            GameEventHandler.Invoke(LogFirebaseEventCode.BossFight, "Started", bossName, bossDifficulty, driver, scorePlayer);
            #endregion
        }
    }

    private void CompleteBossFight(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        if (parameters[0] is BossMapSO)
        {
            BossMapSO bossMapSO = parameters[0] as BossMapSO;
            CallEventBossChapter(bossMapSO, "Complete", bossMapSO.currentChapterSO.chapterName, bossMapSO.currentChapterSO.bossIndex.value + 1);

            #region Firebase Event
            bool isStarted = false;
            string bossName = bossMapSO.currentChapterSO.currentBossSO.chassisSO.GetModule<NameItemModule>().displayName;
            string bossDifficulty = GetBossDifficulty();
            string driver = m_CharacterManagerSO.PlayerCharacterSO.value.GetDisplayName();
            RPSData rpsData = rpsCalculatorSO.CalcCurrentRPSValue();
            int scorePlayer = (int)rpsData.scoreOfPlayer;
            GameEventHandler.Invoke(LogFirebaseEventCode.BossFight, "Completed", bossName, bossDifficulty, driver, scorePlayer);
            #endregion
        }
    }

    private void FailBossFight(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        if (parameters[0] is BossMapSO)
        {
            BossMapSO bossMapSO = parameters[0] as BossMapSO;
            CallEventBossChapter(bossMapSO, "Fail", bossMapSO.currentChapterSO.chapterName, bossMapSO.currentChapterSO.bossIndex.value + 1);

            #region Firebase Event
            bool isStarted = false;
            string bossName = bossMapSO.currentChapterSO.currentBossSO.chassisSO.GetModule<NameItemModule>().displayName;
            string bossDifficulty = GetBossDifficulty();
            string driver = m_CharacterManagerSO.PlayerCharacterSO.value.GetDisplayName();
            RPSData rpsData = rpsCalculatorSO.CalcCurrentRPSValue();
            int scorePlayer = (int)rpsData.scoreOfPlayer;
            GameEventHandler.Invoke(LogFirebaseEventCode.BossFight, "Failed", bossName, bossDifficulty, driver, scorePlayer);
            #endregion
        }
    }

    private void CallEventBossChapter(BossMapSO bossMapSO, string Status, string chapterName, int bossID)
    {
        if (bossMapSO != null)
        {
            if (rpsCalculatorSO != null)
            {
                string overalScoreLabel = "Even";
                if (rpsCalculatorSO.CalcCurrentRPSValue().stateLabel == "Hard")
                    overalScoreLabel = "Opponent Advantage";
                else if (rpsCalculatorSO.CalcCurrentRPSValue().stateLabel == "Easy")
                    overalScoreLabel = "Player Advantage";

                int currentArenaIndex = currentHighestArenaVariable.value.index + 1;
                PBAnalyticsManager.Instance.LogBossFight($"{Status}", currentArenaIndex, bossID, overalScoreLabel, _timeAfterEndRound);
            }
        }
    }

    private void BossFightStreak(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        if (parameters[0] is BossChapterSO)
        {
            BossChapterSO bossChapterSO = parameters[0] as BossChapterSO;
            int bossFightStreak = bossFightStreakHandle.GetStreakBoss(bossChapterSO);

            if (bossFightStreak >= 1)
            {
                AnalyticsManager.LogBossFightStreak(bossChapterSO.bossIndex.value + 1, bossFightStreak);
            }
        }
    }

    private void RevivePopup(params object[] parameters)
    {
        if (parameters.Length <= 0 && (string)parameters[0] == null || (string)parameters[1] == null) return;

        string bossName = (string)parameters[0];
        string status = (string)parameters[1];
        AnalyticsManager.LogRevivePopup(bossName, status);
    }

    #endregion

    #region Starter Pack Event
    private void StarterPackIAP(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;

        if (parameters[0] is IAPProductSO)
        {
            IAPProductSO productSO = parameters[0] as IAPProductSO;

            float dollars = productSO.price;
            int cents = Mathf.RoundToInt(dollars * 100);

            int amount = cents;
            string validCurrencyCode = "USD";
            string typeItem = productSO.itemType.ToString();
            string ItemID = productSO.productName;
            string cardType = "IAP_Popup";
            AnalyticsManager.IAPPurchaseComplete(validCurrencyCode, amount, typeItem, ItemID, cardType, new Dictionary<string, object>());
        }
    }

    private void StartPopupStarterPack(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        //string popupName = "StarterPack";
        //string status = "Start";
        //AnalyticsManager.LogPopupStarterPack(popupName, GetCurrentOperation(), status);
    }

    private void CompletePopupStarterPack(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        //string popupName = "StarterPack";
        //string status = "Complete";
        //AnalyticsManager.LogPopupStarterPack(popupName, GetCurrentOperation(), status);
    }

    private string GetCurrentOperation()
    {
        string operationPack = operationStarterPackVariable.value switch
        {
            OperationStarterPack.Manually => $"Manually",
            OperationStarterPack.Automatically => $"Automatically",
            _ => "None"
        };
        return operationPack;
    }
    #endregion

    #region MainUI

    private void FullSlotBoxSlot()
    {
        AnalyticsManager.LogFullBoxSlot();
    }

    private void BossOutEnergy(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        string bossName = "";
        if (parameters[0] != null)
        {
            if (parameters[0] is PBChassisSO chassisSO)
            {
                bossName = chassisSO.GetModule<NameItemModule>().displayName;
            }
        }
        AnalyticsManager.LogBossOutEnergy(bossName);
    }

    private void BossOutEnergyAlert(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        string bossName = "";
        string status = "";
        if (parameters[0] != null)
        {
            if (parameters[0] is PBChassisSO chassisSO)
            {
                bossName = chassisSO.GetModule<NameItemModule>().displayName;
            }
        }

        if (parameters[1] != null)
        {
            if (parameters[1] is string nameStatus)
            {
                status = nameStatus;
            }
        }

        AnalyticsManager.LogBossOutEnergyAlert(bossName, status);
    }

    private void PowerDilemma(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        string popupName = "PowerDilemma";
        string status = (string)parameters[0];
        AnalyticsManager.LogPowerDilemma(popupName, status, "Automatically");
    }

    private void PopupGroup(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null || parameters[2] == null) return;
        int arenaIndex = currentHighestArenaVariable.value.index + 1;
        string popupName = (string)parameters[0];
        string operation = (string)parameters[1];
        string status = (string)parameters[2];

        AnalyticsManager.LogPopupGroup(arenaIndex, popupName, operation, status);
    }

    private void RVShow(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;

        int arenaIndex = currentHighestArenaVariable.value.index + 1;
        string rvName = (string)parameters[0];
        string location = (string)parameters[1];

        AnalyticsManager.LogRVShow(arenaIndex, rvName, location);
    }
    #endregion

    #region IAP
    private void IAPPackPurchased(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;

        IAPProductSO iapProductSO = (IAPProductSO)parameters[0];
        bool isMainScene = (bool)parameters[1];

        float dollars = iapProductSO.price;
        int cents = Mathf.RoundToInt(dollars * 100);

        string currency = "USD";
        int amount = cents;
        string itemType = iapProductSO.shopPackType.ToString();
        string itemID = $"{iapProductSO.itemID}{groupName}";
        string cardType = isMainScene ? "Popup" : "Shop";

        AnalyticsManager.IAPPurchaseComplete(currency, amount, itemType, itemID, cardType, new Dictionary<string, object>());

        #region Firebase Event
        string location = isMainScene ? "Main UI" : "Shop";
        GameEventHandler.Invoke(LogFirebaseEventCode.IAPLocationPuchased, iapProductSO, location);
        #endregion
    }
    #endregion

    #region Interstitial
    private void OnShowAdInterstitial(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        AdsType adsTypeEnum = (AdsType)parameters[0];
        if (adsTypeEnum != AdsType.Interstitial) return;

        AdsLocation adsLocation = (AdsLocation)parameters[1];
        string adsLocationHandleName = adsLocation.ToString();
        string adsLocationNameAfterUnderscore = adsLocationHandleName.Substring(adsLocationHandleName.IndexOf('_') + 1);
        string adsPlacement = $"IS{groupName}_A{currentArena}_{adsLocationNameAfterUnderscore}";

        AnalyticsManager.CustomShowInterstitialAdCompleted(adsPlacement);
    }
    private void OnCloseAdInterstitial(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;
        int currentArena = currentHighestArenaVariable.value.index + 1;
        AdsType adsTypeEnum = (AdsType)parameters[0];
        if (adsTypeEnum != AdsType.Interstitial) return;

        AdsLocation adsLocation = (AdsLocation)parameters[1];
        bool isSuccess = (bool)parameters[2];
        if (!isSuccess) return;
        string adsLocationHandleName = adsLocation.ToString();
        string adsLocationNameAfterUnderscore = adsLocationHandleName.Substring(adsLocationHandleName.IndexOf('_') + 1);
        string adsPlacement = $"IS{groupName}_A{currentArena}_{adsLocationNameAfterUnderscore}";

        AnalyticsManager.CustomRewardReceivedInterstitialAdCompleted(adsPlacement);
    }
    #endregion
    //
}
