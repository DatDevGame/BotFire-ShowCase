using System.Collections;
using System.Collections.Generic;
using LatteGames;
using UnityEngine;
using HyrphusQ.Events;
using LatteGames.PvP;
using LatteGames.Template;
using System;
using System.Linq;
using Sirenix.OdinInspector;

[EventCode]
public enum PBLevelEventCode
{
    /// <summary>
    /// Raised when complete count down ready - set - fight
    /// </summary>
    OnLevelStart,
    /// <summary>
    /// Raised when level ended
    /// <para><typeparamref name="ListCompetitor"/>: Winners</para>
    /// <para><typeparamref name="int"/>: Survivor Count</para>
    /// </summary>
    OnLevelEnded
}

public enum PBLevelResult
{
    Win,
    Lose,
    Draw
}

public class PBLevelController : LevelController
{
    public Action OnRemoveAliveCompetitor;
    public Action OnTimeUp;

    [SerializeField] PvPArenaVariable pvpArenaVariable;
    [SerializeField] TestMatchMakingSO testMatchMakingSO;

    [SerializeField, BoxGroup("PS-FTUE")] private CanvasGroupVisibility m_InFirstMatchCanvasGroupVisibility;
    [SerializeField, BoxGroup("PS-FTUE")] private PSFTUESO m_PSFTUESO;

    bool isVictory = false;
    bool isDraw = false;
    bool levelEnded = false;

    List<Competitor> competitors = new();
    List<Competitor> aliveCompetitor = new();

    bool isLevelStarted = false;
    PvPMatch currentMatch;

    Competitor localPlayer;
    int remainingTime;

    public bool IsStopCountdownMatchTime { get; set; } = false;
    public int RemainingTime => remainingTime;
    public List<Competitor> Competitors => competitors;
    public List<Competitor> AliveCompetitors => aliveCompetitor;
    public bool IsLevelStarted => isLevelStarted;

    private void Awake()
    {
        ObjectFindCache<PBLevelController>.Add(this);
        if (testMatchMakingSO != null && testMatchMakingSO.IsTest)
        {
            Time.timeScale = 1;
        }
        GameEventHandler.AddActionEvent(CompetitorStatusEventCode.OnCompetitorJoined, HandleCompetitorJoined);
        GameEventHandler.AddActionEvent(CompetitorStatusEventCode.OnCompetitorLeft, HandleCompetitorLeft);
        GameEventHandler.AddActionEvent(CompetitorStatusEventCode.OnCompetitorBeforeDied, HandleCompetitorBeforeDied);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, HandleLevelStarted);
        // GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        // GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
    }
    private void OnDestroy()
    {
        ObjectFindCache<PBLevelController>.Remove(this);
        GameEventHandler.RemoveActionEvent(CompetitorStatusEventCode.OnCompetitorJoined, HandleCompetitorJoined);
        GameEventHandler.RemoveActionEvent(CompetitorStatusEventCode.OnCompetitorLeft, HandleCompetitorLeft);
        GameEventHandler.RemoveActionEvent(CompetitorStatusEventCode.OnCompetitorBeforeDied, HandleCompetitorBeforeDied);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, HandleLevelStarted);
        // GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        // GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
    }

    private void OnUnpackStart(params object[] parram)
    {
        gameObject.SetActive(false);
    }

    private void OnUnpackDone(params object[] parram)
    {
        gameObject.SetActive(true);
    }

    void HandleCompetitorJoined(params object[] paramters)
    {
        if (paramters[0] is not Competitor player) return;
        if (competitors.Contains(player)) return;
        competitors.Add(player);
        aliveCompetitor.Add(player);
        if (player.PersonalInfo.isLocal) localPlayer = player;
    }

    void HandleLevelStarted()
    {
        var matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
        currentMatch = matchManager?.GetCurrentMatchOfPlayer() ?? null;
        isLevelStarted = true;
        if (testMatchMakingSO == null || !testMatchMakingSO.IsTest)
        {
            StartCoroutine(CR_CountdownMatchTime(currentMatch));
        }

        if (!m_PSFTUESO.FTUEInFirstMatch.value)
        {
            GameEventHandler.Invoke(PSLogFTUEEventCode.StartInFirstMatch);
            m_InFirstMatchCanvasGroupVisibility.Show();
            StartCoroutine(CommonCoroutine.Delay(3f, false, () =>
            {
                m_InFirstMatchCanvasGroupVisibility.Hide();
                m_PSFTUESO.FTUEInFirstMatch.value = true;

                GameEventHandler.Invoke(PSLogFTUEEventCode.EndInFirstMatch);
            }));
        }
    }

    IEnumerator CR_CountdownMatchTime(PvPMatch pvpMatch)
    {
        var currentMatch = pvpMatch;
        var waitForOneSecond = new WaitForSeconds(1);
        if (currentMatch == null)
        {
            remainingTime = 60;
        }
        else
        {
            remainingTime = (int)currentMatch.arenaSO.Cast<PBPvPArenaSO>().matchTime;
        }
        while (remainingTime >= 0)
        {
            if (!IsStopCountdownMatchTime)
            {
                yield return waitForOneSecond;
                remainingTime--;
            }
            else
                yield return null;
        }
        EndLevel();
        OnTimeUp?.Invoke();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (levelEnded)
            return;
        if (Input.GetKeyDown(KeyCode.W))
        {
            List<PBRobot> pBRobots = FindObjectsOfType<PBRobot>().ToList();
            PBRobot playerRobot = pBRobots.Find(v => v.PersonalInfo.isLocal);
            if (playerRobot != null)
            {
                for(int i = 0; i < 50; i++)
                    playerRobot.PlayerKDA.AddKill();
            }

            SetVictory(true);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            List<PBRobot> pBRobots = FindObjectsOfType<PBRobot>().ToList();
            PBRobot playerRobot = pBRobots.Find(v => !v.PersonalInfo.isLocal);
            if (playerRobot != null)
            {
                for (int i = 0; i < 50; i++)
                    playerRobot.PlayerKDA.AddKill();
            }

            SetVictory(false);
        }
    }
#endif

    void HandleCompetitorLeft(params object[] paramters)
    {
        if (paramters[0] is not Competitor player) return;
        if (competitors.Contains(player) == false) return;
        competitors.Remove(player);
        aliveCompetitor.Remove(player);
        HandleCompetitorBeforeDied();
    }

    void HandleCompetitorBeforeDied(params object[] parameters)
    {
        //if (isLevelStarted == false) return;
        //if (parameters[0] is PBRobot robot && robot.PersonalInfo.isLocal && currentMatch.Cast<PBPvPMatch>().mode == Mode.Boss)
        //{
        //    var reviveSystem = ObjectFindCache<ReviveSystem>.Get();
        //    if (reviveSystem != null && reviveSystem.isAbleToRevive)
        //    {
        //        reviveSystem.ShowReviveUI(isRevive =>
        //        {
        //            if (!isRevive)
        //                CheckToEndLevel();
        //        });
        //        return;
        //    }
        //}
        //CheckToEndLevel();

        //void CheckToEndLevel()
        //{
        //    foreach (var competitor in competitors)
        //    {
        //        if (competitor.Health <= 0)
        //        {
        //            if (aliveCompetitor.Contains(competitor))
        //            {
        //                aliveCompetitor.Remove(competitor);
        //                OnRemoveAliveCompetitor?.Invoke();
        //            }
        //        }
        //    }
        //    if (levelEnded == true) return;

        //    if (aliveCompetitor.Count <= 1 || localPlayer.Health <= 0)
        //    {
        //        isVictory = localPlayer != null && localPlayer.Health > 0;
        //        EndLevel();
        //    }
        //}
    }

    public virtual void SetVictory(bool isVictory)
    {
        foreach (var competitor in competitors)
        {
            if (competitor.PersonalInfo.isLocal == !isVictory)
                competitor.Health = 0;
        }
        this.isVictory = isVictory;
        EndLevel();
    }

    public override void EndLevel()
    {
        if (levelEnded == true) return;
        levelEnded = true;

        List<Competitor> winners = new();
        int survivorCount = 0;
        if (competitors.Count > 0)
        {
            var highestHealthCompetitor = competitors[0];
            foreach (var competitor in competitors) //Find highest competitor Health value
            {
                if (competitor.Health <= 0) continue; //Player has been disqualified                
                if (competitor.Health >= highestHealthCompetitor.Health)
                {
                    highestHealthCompetitor = competitor;
                }
                ++survivorCount;
            }

            foreach (var competitor in competitors)
            {
                if (competitor.Health >= highestHealthCompetitor.Health)
                {
                    winners.Add(competitor);
                }
            }
        }

        //End Level
        isDraw = winners.Count > 1;
        isVictory = winners.Contains(localPlayer) && isDraw == false; //If draw, consider it is a lost match

        NotifyLevelEnded(winners, survivorCount);
        base.EndLevel();
    }

    public virtual void Surrender()
    {
        if (isLevelStarted == false) return;
        SetVictory(false);
    }

    void NotifyLevelEnded(List<Competitor> winners, int survivorCount)
    {
        //PBLevelResult result = isVictory == true ? PBLevelResult.Win : PBLevelResult.Lose;
        //if (isDraw == true) result = PBLevelResult.Draw;
        GameEventHandler.Invoke(PBLevelEventCode.OnLevelEnded, winners, survivorCount);
    }

    public override bool IsVictory() => isVictory;
}
