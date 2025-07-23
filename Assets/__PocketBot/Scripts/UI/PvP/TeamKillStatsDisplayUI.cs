using HyrphusQ.Events;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TeamKillStatsDisplayUI : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_BlueKillTexts;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_RedKillTexts;

    [ShowInInspector] private List<PBRobot> m_AllRobots;

    private void Awake()
    {
        m_AllRobots = new List<PBRobot> ();
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, BotModelSpawned);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, OnStartLevel);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, BotModelSpawned);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, OnStartLevel);

        m_AllRobots
            .Where(v => v != null && v.PlayerKDA != null).ToList()
            .ForEach(x => x.PlayerKDA.OnChangedKDA -= UpdateView);
    }
    private void BotModelSpawned(params object[] parameters)
    {
        if (parameters[0] is not PBRobot pbBot) return;

        if (m_AllRobots == null)
            m_AllRobots = new List<PBRobot>();

        if (!m_AllRobots.Contains(pbBot))
            m_AllRobots.Add(pbBot);
    }

    private void UpdateView(PlayerKDA playerKDA)
    {
        int totalKillTeamA = m_AllRobots
            .Where(v => v.TeamId == 1)
            .Sum(v => v.PlayerKDA.Kills);

        int totalKillTeamB = m_AllRobots
            .Where(v => v.TeamId != 1)
            .Sum(v => v.PlayerKDA.Kills);

        m_BlueKillTexts.SetText(totalKillTeamA.ToString("D2"));
        m_RedKillTexts.SetText(totalKillTeamB.ToString("D2"));
    }

    private void OnAnyPlayerDied(params object[] parameters)
    {
        PBRobot pbRobot = (PBRobot)parameters[0];
        if (pbRobot == null) return;
    }

    private void OnStartLevel()
    {
        m_AllRobots
            .Where(v => v != null && v.PlayerKDA != null).ToList()
            .ForEach(x => x.PlayerKDA.OnChangedKDA += UpdateView);
    }
}
