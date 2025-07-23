using HyrphusQ.Events;
using PBAnalyticsEvents;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PSFTUEType
{
    public static string Movement = "Movement";
    public static string Combat = "Combat";
    public static string StartFirstMatch = "StartFirstMatch";
    public static string InFirstMatch = "InFirstMatch";
    public static string Upgrade = "Upgrade";
    public static string Start2ndMatch = "Start2ndMatch";
    public static string NewWeapon = "NewWeapon";
}
public static class PSFTUEStep
{
    public static string OpenBuildUI = "OpenBuildUI";
    public static string OpenInfoPopup = "OpenInfoPopup";
    public static string ClickUpgrade = "ClickUpgrade";
    public static string Equip = "Equip";
}
public static class PSFTUEProgressState
{
    public static int Start = 0;
    public static int Completed = 1;
}
public class PSAnalyticsEmitter : MonoBehaviour
{
    [SerializeField, BoxGroup("Data")] private CurrentHighestArenaVariable m_CurrentHighestArenaVariable;
    private PBAnalyticsManager m_AnalyticsManager => PBAnalyticsManager.Instance;
    private void Awake()
    {
        #region FTUE
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.StartMovement, StartMovement);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.EndMovement, EndMovement);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.StartCombat, StartCombat);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.EndCombat, EndCombat);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.StartFirstMatch, StartFirstMatch);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.EndFirstMatch, EndFirstMatch);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.StartInFirstMatch, StartInFirstMatch);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.EndInFirstMatch, EndInFirstMatch);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.StartOpenBuildUI, StartOpenBuildUI);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.EndOpenBuildUI, EndOpenBuildUI);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.StartOpenInfoPopup, StartOpenInfoPopup);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.EndOpenInfoPopup, EndOpenInfoPopup);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.StartClickUpgrade, StartClickUpgrade);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.EndClickUpgrade, EndClickUpgrade);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.Start2ndMatch, Start2ndMatch);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.End2ndMatch, End2ndMatch);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.StartNewWeapon_OpenBuildUI, StartNewWeapon_OpenBuildUI);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.EndNewWeapon_OpenBuildUI, EndNewWeapon_OpenBuildUI);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.StartNewWeapon_Equip, StartNewWeapon_Equip);
        GameEventHandler.AddActionEvent(PSLogFTUEEventCode.EndNewWeapon_Equip, EndNewWeapon_Equip);
        #endregion

        #region Design Event
        GameEventHandler.AddActionEvent(PSDesignEvent.TeamDeathMatchStatsSurrender, TeamDeathMatchStatsSurrender);
        GameEventHandler.AddActionEvent(PSDesignEvent.TeamDeathMatchStats, TeamDeathMatchStats);
        GameEventHandler.AddActionEvent(PSDesignEvent.Upgrade, PSUpgrade);
        GameEventHandler.AddActionEvent(PSDesignEvent.UnlockNewItem, UnlockNewItem);
        GameEventHandler.AddActionEvent(PSDesignEvent.PlayWithUnequipped, PlayWithUnequipped);
        #endregion

        #region Progression Event
        GameEventHandler.AddActionEvent(PSProgressionEvent.TeamDeathmatch, TeamDeathmatchProgression);
        #endregion

    }

    private void OnDestroy()
    {
        #region FTUE
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.StartMovement, StartMovement);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.EndMovement, EndMovement);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.StartCombat, StartCombat);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.EndCombat, EndCombat);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.StartFirstMatch, StartFirstMatch);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.EndFirstMatch, EndFirstMatch);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.StartInFirstMatch, StartInFirstMatch);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.EndInFirstMatch, EndInFirstMatch);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.StartOpenBuildUI, StartOpenBuildUI);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.EndOpenBuildUI, EndOpenBuildUI);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.StartOpenInfoPopup, StartOpenInfoPopup);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.EndOpenInfoPopup, EndOpenInfoPopup);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.StartClickUpgrade, StartClickUpgrade);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.EndClickUpgrade, EndClickUpgrade);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.Start2ndMatch, Start2ndMatch);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.End2ndMatch, End2ndMatch);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.StartNewWeapon_OpenBuildUI, StartNewWeapon_OpenBuildUI);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.EndNewWeapon_OpenBuildUI, EndNewWeapon_OpenBuildUI);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.StartNewWeapon_Equip, StartNewWeapon_Equip);
        GameEventHandler.RemoveActionEvent(PSLogFTUEEventCode.EndNewWeapon_Equip, EndNewWeapon_Equip);
        #endregion

        #region Design Event
        GameEventHandler.RemoveActionEvent(PSDesignEvent.TeamDeathMatchStatsSurrender, TeamDeathMatchStatsSurrender);
        GameEventHandler.RemoveActionEvent(PSDesignEvent.TeamDeathMatchStats, TeamDeathMatchStats);
        GameEventHandler.RemoveActionEvent(PSDesignEvent.Upgrade, PSUpgrade);
        GameEventHandler.RemoveActionEvent(PSDesignEvent.UnlockNewItem, UnlockNewItem);
        GameEventHandler.RemoveActionEvent(PSDesignEvent.PlayWithUnequipped, PlayWithUnequipped);
        #endregion

        #region Progression Event
        GameEventHandler.RemoveActionEvent(PSProgressionEvent.TeamDeathmatch, TeamDeathmatchProgression);
        #endregion
    }

    #region FTUE
    private void StartMovement() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Start, PSFTUEType.Movement);
    private void EndMovement() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Completed, PSFTUEType.Movement);
    private void StartCombat() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Start, PSFTUEType.Combat);
    private void EndCombat() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Completed, PSFTUEType.Combat);
    private void StartFirstMatch() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Start, PSFTUEType.StartFirstMatch);
    private void EndFirstMatch() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Completed, PSFTUEType.StartFirstMatch);
    private void StartInFirstMatch() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Start, PSFTUEType.InFirstMatch);
    private void EndInFirstMatch() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Completed, PSFTUEType.InFirstMatch);
    private void StartOpenBuildUI() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Start, PSFTUEType.Upgrade, PSFTUEStep.OpenBuildUI);
    private void EndOpenBuildUI() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Completed, PSFTUEType.Upgrade, PSFTUEStep.OpenBuildUI);
    private void StartOpenInfoPopup() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Start, PSFTUEType.Upgrade, PSFTUEStep.OpenInfoPopup);
    private void EndOpenInfoPopup() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Completed, PSFTUEType.Upgrade, PSFTUEStep.OpenInfoPopup);
    private void StartClickUpgrade() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Start, PSFTUEType.Upgrade, PSFTUEStep.ClickUpgrade);
    private void EndClickUpgrade() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Completed, PSFTUEType.Upgrade, PSFTUEStep.ClickUpgrade);
    private void Start2ndMatch() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Start, PSFTUEType.Start2ndMatch);
    private void End2ndMatch() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Completed, PSFTUEType.Start2ndMatch);
    private void StartNewWeapon_OpenBuildUI() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Start, PSFTUEType.NewWeapon, PSFTUEStep.OpenBuildUI);
    private void EndNewWeapon_OpenBuildUI() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Completed, PSFTUEType.NewWeapon, PSFTUEStep.OpenBuildUI);
    private void StartNewWeapon_Equip() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Start, PSFTUEType.NewWeapon, PSFTUEStep.Equip);
    private void EndNewWeapon_Equip() => m_AnalyticsManager.LogFTUEEvent(PSFTUEProgressState.Completed, PSFTUEType.NewWeapon, PSFTUEStep.Equip);
    #endregion

    #region Design Event
    private void TeamDeathMatchStatsSurrender(params object[] parrams)
    {
        if (parrams.Length <= 0) return;
        int arena = m_CurrentHighestArenaVariable.value.index + 1;
        int matchCount = (int)parrams[0];
        int score = (int)parrams[1];
        m_AnalyticsManager.TeamDeathMatchSurrender(arena, matchCount, score);
    }
    private void TeamDeathMatchStats(params object[] parrams)
    {
        if (parrams.Length <= 0) return;
        int arena = m_CurrentHighestArenaVariable.value.index + 1;
        string status = (string)parrams[0];
        int userKillCount = (int)parrams[1];
        int teamKillCount = (int)parrams[2];
        int userAssistCount = (int)parrams[3];
        int teamAssistCount = (int)parrams[4];
        int userDeathCount = (int)parrams[5];
        int teamDeathCount = (int)parrams[6];
        int matchCount = (int)parrams[7];

        m_AnalyticsManager.TeamDeathMatchUserKillCount(status, arena, userKillCount, matchCount);
        m_AnalyticsManager.TeamDeathMatchTeamKillCount(status, arena, teamKillCount, matchCount);
        m_AnalyticsManager.TeamDeathMatchUserAssistCount(status, arena, userAssistCount, matchCount);
        m_AnalyticsManager.TeamDeathMatchTeamAssistCount(status, arena, teamAssistCount, matchCount);
        m_AnalyticsManager.TeamDeathMatchUserDeathCount(status, arena, userDeathCount, matchCount);
        m_AnalyticsManager.TeamDeathMatchTeamDeathCount(status, arena, teamDeathCount, matchCount);
    }

    private void PSUpgrade(params object[] parrams)
    {
        if (parrams.Length <= 0) return;
        int arena = m_CurrentHighestArenaVariable.value.index + 1;
        string status = (string)parrams[0];
        PBPartSO partSO = (PBPartSO)parrams[1];
        int upgradeStepIndex = (int)parrams[2];

        string itemName = partSO.GetInternalName();

        m_AnalyticsManager.PSUpgrade(status, itemName, arena, upgradeStepIndex);
    }

    private void UnlockNewItem(params object[] parrams)
    {
        if (parrams.Length <= 0) return;
        int arena = m_CurrentHighestArenaVariable.value.index + 1;
        PBPartSO partSO = (PBPartSO)parrams[0];
        string itemType = partSO.PartType.ToString();
        string itemName = partSO.GetInternalName();

        m_AnalyticsManager.UnlockNewItem(arena, itemType, itemName);
    }

    private void PlayWithUnequipped(params object[] parrams)
    {
        if (parrams.Length <= 0) return;
        int emptySlot = (int)parrams[0];

        m_AnalyticsManager.PlayWithUnequippedSlot(emptySlot);
    }
    #endregion

    #region Progression Event
    private void TeamDeathmatchProgression(params object[] parrams)
    {
        if (parrams.Length <= 0) return;
        int arena = m_CurrentHighestArenaVariable.value.index + 1;
        string teamDeathmatchPPrefStatus = (string)parrams[0];
        int matchCount = (int)parrams[1];
        int score = (int)parrams[2];

        m_AnalyticsManager.TeamDeathmatch(teamDeathmatchPPrefStatus, arena, matchCount, score);
    }
    #endregion
}
