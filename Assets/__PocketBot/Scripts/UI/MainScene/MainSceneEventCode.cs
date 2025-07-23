using HyrphusQ.Events;

//[EventCode]
//public enum MainSceneEventCode
//{

//}

[EventCode]
public enum MainSceneEventCode
{
    OnClickButtonLeaderBoard,
    OnClickButtonShop,
    OnClickButtonMain,
    OnClickButtonCharacter,
    OnClickButtonBattlePass,
    OnClickOnAnyButton,
    OnClickOnSelectingButton,
    OnShowPlayModePanel,
    /// <summary>
    /// This event is raised when you want to click tab button manually.
    /// <para> <typeparamref name="buttonType"/>: ButtonType </para>
    /// </summary>
    OnManuallyClickButton,
    /// <summary>
    /// This event is raised when exiting the previous button.
    /// <para> <typeparamref name="buttonType"/>: ButtonType </para>
    /// </summary>
    OnExitPreviousButton,
    ShowSettingsUI,
    HideSettingUI,
    ShowMainUICurrency,
    HideMainUICurrency,
    HideChangeNameUI,
}
