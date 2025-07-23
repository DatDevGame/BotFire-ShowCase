[EventCode]
public enum PBPvPEventCode
{
    /// <summary>
    /// This event is raised when a match is in preparing
    /// <para> <typeparamref name="PvPMatch"/>: match </para>
    /// </summary>
    OnMatchPrepared,
    /// <summary>
    /// This event is raised when a match is started (ready to fight)
    /// <para> <typeparamref name="PvPMatch"/>: match </para>
    /// </summary>
    OnMatchStarted,
    /// <summary>
    /// This event is raised when a match is completed
    /// <para> <typeparamref name="PvPMatch"/>: match </para>
    /// </summary>
    OnMatchCompleted,
    /// <summary>
    /// This event is raised when a round of match is completed
    /// <para> <typeparamref name="PvPMatch"/>: match </para>
    /// </summary>
    OnRoundCompleted,
    /// <summary>
    /// This event is raised when a final round of match is completed
    /// <para> <typeparamref name="PvPMatch"/>: match </para>
    /// </summary>
    OnFinalRoundCompleted,
    /// <summary>
    /// This event is raised when a opponent is founded
    /// <para> <typeparamref name="PBPvPArenaSO"/>: arenaSO </para>
    /// <para> <typeparamref name="PlayerInfoVariable"/>: infoOfOpponent </para>
    /// </summary>
    OnOpponentFounded,
    /// <summary>
    /// This event is raised when a opponent is founded
    /// <para> <typeparamref name="PBPlayerInfo"/>: infoOfBoosterUser </para>
    /// <para> <typeparamref name="PvPBooster"/>: booster </para>
    /// </summary>
    OnBoosterUsed,
    /// <summary>
    /// This event is raised when an arena is selected
    /// <para><typeparamref name="PBArenaSO"/>Arena SO info</para>
    /// </summary>
    OnEnterArena,
    /// <summary>
    /// This event is raised to call show PvPAreaChoosingUI
    /// </summary>
    OnShowArenaChoosingUI,
    /// <summary>
    /// This event is raised to call hide PvPAreaChoosingUI
    /// </summary>
    OnHideArenaChoosingUI,
    /// <summary>
    /// This event is raised to call show PvPArenaInfoUI
    /// </summary>
    OnShowArenaInfoUI,
    /// <summary>
    /// This event is raised to call hide PvPArenaInfoUI
    /// </summary>
    OnHideArenaInfoUI,
    /// <summary>
    /// This event is raised when any contestant leave in the middle of match
    /// <para> <typeparamref name="PvPMatch"/>: match </para>
    /// <para> <typeparamref name="PBPlayerInfo"/>: Player that surrender </para>
    /// </summary>
    OnLeaveInMiddleOfMatch,
    /// <summary>
    /// This event is raise when a new arena is unlocked
    /// <para> <typeparamref name="PBPvPArenaSO"/>: arenaSO </para>
    /// </summary>
    OnNewArenaUnlocked,
    /// <summary>
    /// This event is raised when any Player Died
    /// <para> <typeparamref name="PBRobot"/>: robot </para>
    /// </summary>
    OnAnyPlayerDied,
    /// <summary>
    /// This event the counting the at "Fight" SFX
    /// </summary>
    OnCountToFightSFX,
    /// <summary>
    /// This event is raise when camera shaking
    /// <para><typeparamref name="PBChassis"/>: Chassis is taken damage (Receiver)</para>
    /// <para><typeparamref name="PBChassis"/>: Chassis caused damage (Attacker)</para>
    /// <para><typeparamref name="Float"/>: Force taken amount</para>
    /// <para><typeparamref name="Bool"/>: Determine whether ignore condition</para>
    /// </summary>
    OnShakeCamera,
    /// <summary>
    /// This event is raise when completing camera rotation
    /// </summary>
    OnCompleteCamRotation,


    /// <summary>
    /// This event Start Searching Arena
    /// </summary>
    OnStartSearchingArena,
    /// <summary>
    /// This event End Searching Arena
    /// </summary>
    OnEndSearchingArena,
    /// <summary>
    /// This event is raised when switching the cam from ready view to fight view.
    /// </summary>
    OnSwitchFromReadyCamToFightCam,
    /// <summary>
    /// This event is raised when showing game over UI
    /// <para> <typeparamref name="PvPMatch"/>: match </para>
    /// </summary>
    OnShowGameOverUI,
    /// <summary>
    /// This event is raised when resetting bots
    /// </summary>
    OnResetBots,
    /// <summary>
    /// This event Start Searching Opponent
    /// </summary>
    OnStartSearchingOpponent,
}
