using HyrphusQ.Events;

[EventCode]
public enum CharacterUIEventCode
{
    /// <summary>
    /// This event is raised when Tab Button in CharacterUI is changed
    /// <para> <typeparamref name="GearTabButton"/>: gearTabButton </para>
    /// </summary>
    OnTabButtonChange,

    /// <summary>
    /// This event is raised when Gear Button is clicked
    /// <para> <typeparamref name="PBPartSO"/>: partType </para>
    /// <para> <typeparamref name="PBPartSlot"/>: partType </para>
    /// </summary>
    OnGearButtonClick,

    /// <summary>
    /// This event is raised when Gear Button is clicked
    /// <para> <typeparamref name="PBChassisSO"/>: partType </para>
    /// </summary>
    OnChassisButtonClick,

    /// <summary>
    /// This event is raised when you show upgrade popup
    /// <para> <typeparamref name="PBPartSO"/>: partType </para>
    /// </summary>
    OnGearCardInfoPopUpShow,

    /// <summary>
    /// This event is raised when Gear Upgrade Button is clicked
    /// <para> <typeparamref name="PBPartSO"/>: partType </para>
    /// </summary>
    OnGearCardUpgrade,

    /// <summary>
    /// This event is raised when Gear Equip Button is clicked
    /// <para> <typeparamref name="PBPartSO"/>: partType </para>
    /// </summary>
    OnGearCardEquip,
    /// <summary>
    /// This event is raised when Gear Equip Button is clicked
    /// <para> <typeparamref name="PBPartSO"/>: partType </para>
    /// </summary>
    OnGearCardUnEquip,

    /// <summary>
    /// This event is raised when you want to send Bot's like HP, ATK...
    /// <para> <typeparamref name="HP"/>: float </para>
    /// <para> <typeparamref name="ATK"/>: float </para>
    /// <para> <typeparamref name="AllPartPower"/>: float </para>
    /// <para> <typeparamref name="ChassisPower"/>: float </para>
    /// </summary>
    OnSendBotInfo,

    /// <summary>
    /// This event is raised when you click equip but not enough Power
    /// </summary>

    OnClosePopUp,
    OnSwapPart,
    OnShowPopup,
    PureOnClosePopup,
    OnShowPowerInfo,
    OnHidePowerInfo,
    SendWarning,

    //New Event
    OnEquipEnoughPower,
    OnEquipNotEnoughPower,
    OnTabSpecial,
    OnEquipedSpecial,
    OnEquipedNotSpecial,
    OnSelectSpecial,
    OnSelectTabSpecial,
    OnResetSelect,
    OnIndexTab,
    /// <summary>
    /// This event is raised when you want click the tab manually
    /// <para> <typeparamref name="PBPartSlot"/>: partType </para>
    /// </summary>
    OnManuallyClickTab,

    /// <summary>
    /// This event is raised when you want lock character UI button
    /// </summary>
    OnLockCharacterUIButton,

    /// <summary>
    /// This event is raised when you want unlock character UI button
    /// </summary>
    OnUnLockCharacterUIButton,

    /// <summary>
    /// This event is raised when claim part and complete RV
    /// </summary>
    OnCompleteClaimPartRV,

    /// <summary>
    /// This event is raised when click gear slot
    /// </summary>
    OnClickGearSlot,

    /// <summary>
    /// This event is raised when after click gear slot
    /// </summary>
    OnAutoOpenGearinfo,

    /// <summary>
    /// This event is raised when clicked tab button on UI
    /// </summary>
    OnClickTabButtonUI,
    /// <summary>
    /// This event is raised when you want click the gear button via code
    /// <para> <typeparamref name="PBPartSO"/>: partSO </para>
    /// <para> <typeparamref name="bool"/>: isShowInfo </para>
    /// </summary>
    ClickGearButtonViaCode,
}