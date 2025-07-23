using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;
using LatteGames.PvP;

public class PvPCheckBotImmobilized : MonoBehaviour
{
    protected const float RESETCOOLDOWN = 2f;

    protected List<PBRobot> robots = new();
    protected List<PBRobot> immobilizedRobots = new();

    protected float lastResetTime;
    protected bool isMatchComplete;

    protected bool _isPlayerImmobilized = false;
    private void Awake()
    {
        GameEventHandler.AddActionEvent(CompetitorStatusEventCode.OnCompetitorJoined, HandleCompetitorJoined);
        GameEventHandler.AddActionEvent(CompetitorStatusEventCode.OnCompetitorLeft, HandleCompetitorLeft);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotImmobilized, HandleBotImmobilized);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotRecoveredFromImmobilized, HandleBotRecovered);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, HandleMatchComplete);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnAnyPlayerDied, HandleAnyPlayerDied);
        lastResetTime = Time.time;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(CompetitorStatusEventCode.OnCompetitorJoined, HandleCompetitorJoined);
        GameEventHandler.RemoveActionEvent(CompetitorStatusEventCode.OnCompetitorLeft, HandleCompetitorLeft);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotImmobilized, HandleBotImmobilized);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotRecoveredFromImmobilized, HandleBotRecovered);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, HandleMatchComplete);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnAnyPlayerDied, HandleAnyPlayerDied);
    }

    void HandleMatchComplete()
    {
        isMatchComplete = true;
    }

    protected virtual void HandleAnyPlayerDied(object[] parameters)
    {
        var match = ObjectFindCache<PBPvPMatchManager>.Get(isCallFromAwake: true)?.GetCurrentMatchOfPlayer() as PBPvPMatch;
        if (match == null || match.mode != Mode.Battle) return; // Ignore 1vs1 mode and FTUE
        if (parameters[0] is not PBRobot pbRobot) return;
        if (robots.Contains(pbRobot)) robots.Remove(pbRobot);
    }

    protected virtual void HandleCompetitorJoined(object[] parameters)
    {
        if (parameters[0] is not Competitor competitor) return;
        var robot = competitor as PBRobot;
        if (robots.Contains(robot)) return;
        robots.Add(robot);
    }

    void HandleCompetitorLeft(object[] parameters)
    {
        if (parameters[0] is not Competitor competitor) return;
        var robot = competitor as PBRobot;
        if (robots.Contains(robot) == false) return;
        robots.Remove(robot);
    }

    void HandleBotImmobilized(object[] parameters)
    {
        _timeDelayReset = TIME_DELAY_RESET_BOT;
        if (parameters[0] is not PBRobot robot) return;
        if (!robots.Contains(robot)) return;
        if (immobilizedRobots.Contains(robot) == true) return;
        immobilizedRobots.Add(robot);

        if (robot.name.Contains("Player"))
            _isPlayerImmobilized = true;
    }

    void HandleBotRecovered(object[] parameters)
    {
        _timeDelayReset = TIME_DELAY_RESET_BOT;
        if (parameters[0] is not PBRobot robot) return;
        if (immobilizedRobots.Contains(robot) == false) return;
        immobilizedRobots.Remove(robot);

        if (robot.name.Contains("Player"))
            _isPlayerImmobilized = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBot();
            foreach (var robot in robots)
            {
                robot.Health = robot.MaxHealth;
            }
        }
        CheckToResetBots();
    }

    private float _timeDelayReset = 0;
    private const float TIME_DELAY_RESET_BOT = 10f;
    void CheckToResetBots()
    {
        if (isMatchComplete == true) return;
        if (Time.time - lastResetTime < RESETCOOLDOWN) return;
        if (immobilizedRobots.Count == robots.Count && robots.Count > 1)
        {
            _timeDelayReset -= Time.deltaTime;
            if (_timeDelayReset <= 0)
                ResetBot();
        }
    }

    void ResetBot()
    {
        _timeDelayReset = TIME_DELAY_RESET_BOT;
        lastResetTime = Time.time;
        foreach (var robot in robots)
        {
            robot.BuildRobot(false);
        }
        immobilizedRobots.Clear();
        GameEventHandler.Invoke(PBPvPEventCode.OnResetBots);
    }

    private void OnApplicationPause(bool pause)
    {
        if (_isPlayerImmobilized)
        {
            _isPlayerImmobilized = false;

            string name = "";
            GameObject pbFightingStage = PBFightingStage.Instance.gameObject;
            if (pbFightingStage != null)
            {
                name = pbFightingStage.name.Replace("(Clone)", "");
            }

            string stageName = name;
            string mainPhase = "Fighting";
            string subPhase = "UpsideDown";
            GameEventHandler.Invoke(DesignEvent.QuitCheck, stageName, mainPhase, subPhase);
        }
    }
}
