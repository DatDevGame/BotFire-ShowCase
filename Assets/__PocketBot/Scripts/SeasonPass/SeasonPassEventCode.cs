[EventCode]
public enum SeasonPassEventCode
{
    /// <summary>
    /// This event occurs when the SeasonPass product is purchased
    /// </summary>
    OnPurchaseSeasonPass,
    /// <summary>
    /// This event occurs when showing the SeasonPass popup
    /// <para> <typeparamref name="bool"/>: isEarlyAccess </para>
    /// </summary>
    ShowSeasonPassPopup,
    /// <summary>
    /// This event occurs when hiding the SeasonPass popup
    /// </summary>
    HideSeasonPassPopup,
    /// <summary>
    /// This event occurs when showing the UnlockSeason screen
    /// </summary>
    ShowSeasonUnlockScreen,
    /// <summary>
    /// This event occurs when updating the season UI
    /// </summary>
    UpdateSeasonUI,
    /// <summary>
    /// This event occurs when showing the StartSeason popup
    /// </summary>
    ShowStartSeasonPopup,
    /// <summary>
    /// This event occurs when update new daily missions
    /// </summary>
    OnUpdateNewDailyMissions,
    /// <summary>
    /// This event occurs when update new weekly missions
    /// </summary>
    OnUpdateNewWeeklyMissions,
    /// <summary>
    /// This event occurs when update new half-season missions
    /// </summary>
    OnUpdateNewHalfSeasonMissions,
    /// <summary>
    /// This event occurs when showing the EndSeason popup
    /// </summary>
    ShowEndSeasonPopup,
    /// <summary>
    /// This event occurs when hiding the EndSeason popup
    /// </summary>
    HideEndSeasonPopup,
    /// <summary>
    /// This event occurs when hiding the StartSeason popup
    /// </summary>
    HideStartSeasonPopup,
    /// <summary>
    /// This event occurs when resetting the StartSeason
    /// </summary>
    ResetSeasonUI,
    /// <summary>
    /// This event occurs when setting the first day of the season
    /// </summary>
    OnSetNewSeasonFirstDay,
}
