#region Design Event
public enum DesignEvent
{
    QuitCheck,
    OpenBox,
    FullSlotBoxSlot,
    CollectBox,
    BossOutEnergy,
    BossOutEnergyAlert,
    FlipTiming,
    RevivePopup,
    FeatureUsed,
    Popup,
    PopupGroup,
    RVShow,
    HotOfferBuy,
    BossFightUnlock,
    BattlePvPChooseArena,
    PreludeMission,
    TodayMission,
    WeeklyMission,
    SeasonMission,
    LeagueRank,
    SkillTrack,
    StageInteraction,
    SkillUsage_Duel_Equiped,
    SkillUsage_Duel_Used,
    SkillUsage_Battle_Equiped,
    SkillUsage_Battle_Used,
}
#endregion

#region Progression Event
public enum ProgressionEvent
{
    ClaimBoss,
    WinStreak,
    PiggyBank,
    Season,
    MissionRound,
    EnterSeasonTab,
    BuySkin,
    LeaguePromotion,
    Progression,
    Stage,
    LadderedOffer
}
#endregion

#region Monetization Event Code
public enum MonetizationEventCode
{
    /// <summary>
    /// This event is raised when click Multiplier button and watch the advertisement in SinglePVP and BattlePVP
    /// </summary>
    MultiplierRewards,
    /// <summary>
    /// This event is raised when click revenge button and watch the advertisement
    /// </summary>
    RevengeBoss_LoseBossUI,
    /// <summary>
    /// This event is raised when click revenge button and watch the advertisement
    /// </summary>
    RevengeBoss_BossFightUI,
    /// <summary>
    /// This event is raised when click claim button in UnlockBossScene and watch the advertisement
    /// </summary>
    ClaimBoss_WinBossUI,
    /// <summary>
    /// This event is raised when click claim button in UnlockBossScene and watch the advertisement
    /// </summary>
    ClaimBoss_InventoryUI,
    /// <summary>
    /// This event is raised when click FreeBox button and watch the advertisement
    /// </summary>
    FreeBox_MainUI,
    /// <summary>
    /// This event is raised when click Exchange RV button and watch the advertisement
    /// </summary>
    Upgrade,
    /// <summary>
    /// This event is raised when click Bonus Card in Cardshowwing and watch the advertisement
    /// </summary>
    BonusCard_OpenBoxUI,
    /// <summary>
    /// This event is raised when click Bonus Card in Summary and watch the advertisement
    /// </summary>
    BonusCard_YouGotUI,
    /// <summary>
    /// This event is raised when click Speed Up Box Slot and watch the advertisement
    /// </summary>
    SpeedUpBox,
    /// <summary>
    /// This event is raised when click Open Now Box Slot In Popup Box and watch the advertisement
    /// </summary>
    OpenNowBox_BoxPopup,
    /// <summary>
    /// This event is raised when click Open Now In Game Over UI and watch the advertisement
    /// </summary>
    OpenNowBox_GameOverUI,
    /// <summary>
    /// This event is raised when click Charge Energy In Alert Popup UI and watch the advertisement
    /// </summary>
    RechargeBoss_AlertPopup,
    /// <summary>
    /// This event is raised when click Charge Energy In character UI and watch the advertisement
    /// </summary>
    RechargeBoss_BuildUI,
    /// <summary>
    /// This event is raised when click Charge Energy In Boss Popup UI and watch the advertisement
    /// </summary>
    RechargeBoss_BossPopup,
    /// <summary>
    /// This event is raised when click Revive boss in match and watch the advertisement
    /// </summary>
    ReviveBoss,
    /// <summary>
    /// This event is raised when click Unlock and watch the advertisement
    /// </summary>
    GetSkin,
    /// This event is raised when triggered when the player successfully watches RV to use get FreeCoin
    /// </summary>
    DailyOffer_FreeCoin,
    /// <summary>
    /// This event is raised when click Get Free Coins and watch the advertisement
    /// </summary>
    HotOffers_UpgradePopup,
    /// <summary>
    /// This event is raised when click Get Free Gems and watch the advertisement
    /// </summary>
    HotOffers_ResourcePopup,
    /// <summary>
    /// This event is raised when click Get Item HotOffers and watch the advertisement
    /// </summary>
    HotOffers_Shop,
    /// <summary>
    /// This event is raised when click Chanllenge BossUI and watch the advertisement
    /// </summary>
    TrophyBypass_BossUI,
    /// <summary>
    /// This event is raised when watch the advertisement cell Link Rewards in Main UI
    /// </summary>
    LinkRewards_MainUI,
    /// <summary>
    /// This event is raised when watch the advertisement cell Link Rewards in Shop
    /// </summary>
    LinkRewards_Shop,
    /// <summary>
    /// triggered when the player successfully watches RV to keep his win streak
    /// </summary>
    KeepWinStreak,
    /// <summary>
    /// triggered when the player successfully watches RV to get back his entry fee
    /// </summary>
    GetBack,
    /// <summary>
    /// triggered when the player successfully watches RV to get the win streak premium rewards
    /// </summary>
    WinStreakPremium,
    /// <summary>
    /// triggered when the player successfully watches RV to get the missing part
    /// </summary>
    GetMissingCard,
    /// <summary>
    /// triggered when the player successfully buy a garage
    /// </summary>
    BuyGarage,
    /// <summary>
    /// triggered when the player successfully watches RV to skip a mission
    /// </summary>
    SkipMission,
    /// <summary>
    /// triggered when the player successfully watches RV to ClaimDoubleMission
    /// </summary>
    ClaimDoubleMission,
    /// <summary>
    /// triggered when the player successfully watches RV to BuyCharacter
    /// </summary>
    BuyCharacter,
    /// <summary>
    /// triggered when the player successfully watches RV to get an Event Ticket
    /// </summary>
    FreeEventTicket,
    /// <summary>
    /// triggerd when the player successfully claim an offer from Laddered Offer
    /// </summary>
    LadderedOffer,
    /// <summary>
    /// triggered when the player successfully watches RV to get an Free Skill
    /// </summary>
    FreesSkill,
}
#endregion

#region Starter Pack Event Code
public enum StarterPackEventCode
{
    /// <summary>
    /// This event is raised when Buy IAP starter pack
    /// </summary>
    StarterPackIAP,
    /// <summary>
    /// This event is raised when Open popup starter pack
    /// </summary>
    PopupStart,
    /// <summary>
    /// This event is raised when Close popup starter pack
    /// </summary>
    PopupComplete
}
#endregion

#region Boss Event Code
public enum BossEventCode
{
    /// <summary>
    /// This event is raised when users start a boss match
    /// </summary>
    StartBossFight,
    /// <summary>
    /// This event is raised when user wins the boss match
    /// </summary>
    CompleteBossFight,
    /// <summary>
    /// This event is raised when user failed the boss match
    /// </summary>
    FailBossFight,
    /// <summary>
    /// This event is raised when the player start a boss match
    /// </summary>
    BossFightStreak
}
#endregion

#region FTUE Event Code
public enum LogFTUEEventCode
{
    StartFightButton,
    EndFightButton,
    StartControl,
    StartReverse,
    EndReverse,
    EndControl,
    StartUseActiveSkill,
    EndUseActiveSkill,
    StartOpenBox1,
    EndOpenBox1,
    StartOpenBox,
    EndOpenBox,
    StartEquip_1,
    EndEquip_1,
    StartPower,
    EndPower,
    StartUpgrade,
    EndUpgrade,
    StartSelectSingleMode,
    EndSelectSingleMode,
    StartPlaySingle,
    EndPlaySingle,
    StartPreludeSeason,
    EndPreludeSeason,
    StartPreludeSeason_Explore,
    EndPreludeSeason_Explore,
    StartPreludeSeason_ClaimRewards,
    EndPreludeSeason_ClaimRewards,
    StartPreludeSeason_DoMission,
    EndPreludeSeason_DoMission,
    StartOpenBox2,
    EndOpenBox2,
    StartEquip_2,
    EndEquip_2,
    StartBuildTab,
    EndBuildTab,
    StartSelectBossMode,
    EndSelectBossMode,
    StartEnterBossUI,
    EndEnterBossUI,
    StartPlayBossFight,
    EndPlayBossFight,
    StartSkinUI,
    EndSkinUI,
    StartSelectBattleMode,
    EndSelectBattleMode,
    StartPlayBattle,
    EndPlayBattle,
    StartLeague,
    EndLeague,
    StartActiveSkillEnter,
    EndActiveSkillEnter,
    StartActiveSkillClaim,
    EndActiveSkillClaim,
    StartActiveSkillEquip,
    EndActiveSkillEquip,
    StartActiveSkillInGame,
    EndActiveSkillInGame,
}
#endregion

#region IAP Event Code
public enum LogIAPEventCode
{
    IAPPack
}
#endregion

#region Sink/Source
public enum LogSinkSource
{
    SkillCard
}
#endregion

#region Firebase Event Code
public enum IAPPurchased
{
    StarterPack,
    ArenaOffer,
    AllSkinOffers
}
public enum LogFirebaseEventCode
{
    ItemEquip,
    ItemUpgrade,
    Tutorials,
    BossFightMenu,
    BossFight,
    BoxAvailable,
    BoxOpen,
    BattleRoyale,
    CurrencyTransaction,
    FlipTiming,
    UpgradeNowShown,
    UpgradeNowClicked,
    PopupAction,
    IAPLocationPuchased,
    TrophyChange,
    PlayScreenReached,
    BackwardsMove,
    ShopMenuReached,
    ItemsAvailableUpgradeChange,
    SeasonMenuReached,
    ClaimMissionReward,
    ClaimSeasonReward,
    AvailableMissionReward,
    AvailableSeasonReward,
    DriversMenuReached,
    NewDriverSelected,
    RequestBonusMissionsShown,
    RequestBonusMissionsClicked,
    RefreshMissionClicked,
    LeagueStarted,
    LeagueMenuReached,
    InfoLeagueButtonClicked,
    StartOfDivisionPopUp,
    EndOfDivisionPopUp,
    EndOfLeaguePopUp,
    SkillAvailable,
    SkillUse,
    BotStunned,
    AffectedByOpponentSkill
}
#endregion

public static class DesignEventStatus
{
    public static string Start = "Start";
    public static string Complete = "Complete";
    public static string Skip = "Skip";
}
public static class ProgressionEventStatus
{
    public static string Start = "Start";
    public static string Complete = "Complete";
    public static string Fail = "Fail";
}
public static class LogErrorKeyGA
{
    public static string Key = "LogErrorKey-LatteGames";
}