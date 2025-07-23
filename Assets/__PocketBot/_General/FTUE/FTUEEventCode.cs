using HyrphusQ.Events;

[EventCode]
public enum FTUEEventCode
{
    OnWaitBodyTabFTUE,

    OnFinishEquipFTUE,

    OnClickMoreButton,

    OnFinishUpgradeFTUE,

    OnBossModeFTUE,

    OnRoyalModeFTUE,

    OnBuildTabFTUE,

    OnWaitEquipUpperFTUE,
    OnEquipUpperFTUE,

    OnEquipFTUE,

    OnShowEnergyFTUE,
    OnFinishShowEnergyFTUE,

    OnUpgradeFTUE,

    OnPlayBossMode,

    OnRoyalModeSelectArena,
    OnRoyalModeHighlinetArena,

    OnDuelModeFTUE,

    OnOpenBoxSlotFTUE,
    OnClickStartUnlockBoxSlotFTUE,
    OnClickOpenBoxSlotFTUE,

    OnChoosenModeButton,

    OnPlayBattleButton,

    OnClickBoxTheFisrtTime,
    OnClickStartUnlockBoxTheFirstTime,
    OnClickOpenBoxTheFirstTime,
    OnSkinUI_FTUE,
    OnPreludeSeasonUnlocked_FTUE,
    OnPreludeDoMission_FTUE,

    OnEquipUpperFTUECompleted,
}

public enum FTUEBubbleText
{
    SelectGameMode,
    ANewGameModeIsAvailable,
    GreatJobNowGetYourReward,
    TapToOpenBoxSlot
}

public enum BlockBackGround
{
    Lock,
    LockDarkHasObject,
    LockWhiteHasObject
}

public enum StateBlockBackGroundFTUE
{
    Start,
    End
}

public enum HandleFTUECharacterUIEvent
{
    ScrollToElement
}
