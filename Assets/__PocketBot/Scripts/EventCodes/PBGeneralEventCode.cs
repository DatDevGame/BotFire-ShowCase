[EventCode]
public enum PBGeneralEventCode
{
    /// <summary>
    /// This event is raised when any tab button is clicked
    /// </summary>
    OnAnyTabButtonClicked,
    /// <summary>
    /// This event is raised when leaderboard tab button is clicked
    /// </summary>
    OnLeaderboardTabButtonClicked,
    /// <summary>
    /// This event is raised when shop tab button is clicked
    /// </summary>
    OnShopTabButtonClicked,
    /// <summary>
    /// This event is raised when main tab button is clicked
    /// </summary>
    OnMainTabButtonClicked,
    /// <summary>
    /// This event is raised when Part tab button is clicked
    /// </summary>
    OnPartTabButtonClicked,
    /// <summary>
    /// This event is raised when battlepass tab button is clicked
    /// </summary>
    OnBattlepassTabButtonClicked,
    /// <summary>
    /// This event is raised to show main canvas UI
    /// </summary>
    OnShowMainCanvasUI,
    /// <summary>
    /// This event is raised to hide main canvas UI
    /// </summary>
    OnHideMainCanvasUI,
    /// <summary>
    /// This event is raised when play button is clicked
    /// </summary>
    OnPlayButtonClicked,
    /// <summary>
    /// This event is raised when any stat (Chassis & Part) of player changed
    /// </summary>
    OnAnyStatChanged,
    /// <summary>
    /// This event is raised to show PvP booster shop UI
    /// <para> <typeparamref name="PvPBooster"/>: booster </para>
    /// </summary>
    OnShowBoosterShopUI,
    /// <summary>
    /// This event is raised to hide PvP booster shop UI
    /// </summary>
    OnHideBoosterShopUI,
}
