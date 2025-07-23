using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames.PvP;
using HyrphusQ.Events;
using System;
using System.Linq;
using System.Diagnostics;
using System.Text;
using LatteGames.Template;
using LatteGames;

public class PBPvPMatchManager : PvPMatchManager
{
    private const string kDebugTag = "PBPvPMatchManager";
    private const int _audienceCheerMaxLevel = 3;
    private int _countTimeLevelUpCheer = 3;
    private int _audienceCheerLevel = 1;
    private int _timeCountDecreaseAudienceCheer = 0;
    private IEnumerator CountTimeLevelUpAudienceCheerCR;
    private IEnumerator CountTimeAudienceCheerCR;

    private void Awake()
    {
        ObjectFindCache<PBPvPMatchManager>.Add(this);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnShakeCamera, HandleSoundAudienceCheer);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnDestroy()
    {
        ObjectFindCache<PBPvPMatchManager>.Remove(this);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnShakeCamera, HandleSoundAudienceCheer);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);

        if (CountTimeLevelUpAudienceCheerCR != null)
            StopCoroutine(CountTimeLevelUpAudienceCheerCR);

        if (CountTimeAudienceCheerCR != null)
            StopCoroutine(CountTimeAudienceCheerCR);
    }

    private void OnAnyPlayerDied(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        var robot = parameters[0] as PBRobot;
        var matchOfPlayer = GetCurrentMatchOfPlayer() as PBPvPMatch;
        matchOfPlayer.AddSurvivor(robot.PlayerInfoVariable);

        LGDebug.Log($"{robot.name} died", kDebugTag);
    }
    private void HandleSoundAudienceCheer()
    {
        if (_countTimeLevelUpCheer > 0) return;
        _countTimeLevelUpCheer = 3;

        _timeCountDecreaseAudienceCheer = 10;
        _audienceCheerLevel++;
        if (_audienceCheerLevel >= _audienceCheerMaxLevel)
            _audienceCheerLevel = _audienceCheerMaxLevel;

        // SoundManager.Instance.PlayLoopSFX(GetSFXlevel(_audienceCheerLevel), 1, true, false, this.gameObject);
    }
    private void OnFinalRoundCompleted(object[] parameters)
    {
        // SoundManager.Instance.StopLoopSFX(this.gameObject);

        if (CountTimeLevelUpAudienceCheerCR != null)
            StopCoroutine(CountTimeLevelUpAudienceCheerCR);

        if (CountTimeAudienceCheerCR != null)
            StopCoroutine(CountTimeAudienceCheerCR);
    }
    private IEnumerator CountTimeConditionLevelUpAudienceCheer()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        while (true)
        {
            _countTimeLevelUpCheer--;
            yield return waitForSeconds;
        }
    }
    private IEnumerator CountTimeAudienceCheer()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        int timeDecreaseEverySecond = 10;
        while (true)
        {
            if (_audienceCheerLevel > 1)
            {
                _timeCountDecreaseAudienceCheer--;
                if (_timeCountDecreaseAudienceCheer <= 0)
                {
                    _audienceCheerLevel--;
                    _timeCountDecreaseAudienceCheer = timeDecreaseEverySecond;

                    // SoundManager.Instance.PlayLoopSFX(GetSFXlevel(_audienceCheerLevel), 1, true, false, this.gameObject);
                }
            }
            yield return waitForSeconds;
        }
    }
    public override void StartMatch(PvPMatch match)
    {
        base.StartMatch(match);
        // SoundManager.Instance.PlayLoopSFX(GetSFXlevel(_audienceCheerLevel), 1, true, false, this.gameObject);

        if (CountTimeLevelUpAudienceCheerCR != null)
            StopCoroutine(CountTimeLevelUpAudienceCheerCR);
        CountTimeLevelUpAudienceCheerCR = CountTimeConditionLevelUpAudienceCheer();
        StartCoroutine(CountTimeLevelUpAudienceCheerCR);

        if (CountTimeAudienceCheerCR != null)
            StopCoroutine(CountTimeAudienceCheerCR);
        CountTimeAudienceCheerCR = CountTimeAudienceCheer();
        StartCoroutine(CountTimeAudienceCheerCR);
    }
    private SFX GetSFXlevel(int levelIndex)
    {
        if (levelIndex == 1)
            return SFX.Audience_Level1;
        else if (levelIndex == 2)
            return SFX.Audience_Level2;
        else
            return SFX.Audience_Level3;
    }
    [Conditional(LGDebug.k_UnityEditorDefineSymbol), Conditional(LGDebug.k_LatteDebugDefineSymbol)]
    private void LogSurvivors(PBPvPMatch pvpMatch, List<PBRobot> robots)
    {
        var logBuilder = new StringBuilder();
        logBuilder.AppendLine("Survivors Log");
        logBuilder.AppendLine($"IsVictory: {pvpMatch.isVictory}");
        for (int i = 0; i < pvpMatch.survivingContestantOrders.Count; i++)
        {
            var robot = robots.FirstOrDefault(item => item.PlayerInfoVariable.value == pvpMatch.survivingContestantOrders[i]);
            logBuilder.AppendLine($"Rank: {i + 1} - PlayerName: {pvpMatch.survivingContestantOrders[i].personalInfo.name} - Health: {robot.Health} / {robot.MaxHealth} - OverallScore: {robot.RobotStatsSO.value}");
        }
        LGDebug.Log(logBuilder, kDebugTag);
    }

    public override void EndFinalRound(PvPMatch match)
    {
        if (match is PBPvPMatch pbPvPMatch)
        {
            var survivingRobots = new List<PBRobot>();
            // Filter robots still survived (remove robots that already died)
            var robots = PBRobot.allFightingRobots;
            foreach (var robot in robots)
            {
                if (pbPvPMatch.survivingContestantOrders.Contains(robot.PlayerInfoVariable))
                    continue;
                survivingRobots.Add(robot);
            }
            if (pbPvPMatch.mode != Mode.Battle || pbPvPMatch.pbEndgameData.isTimesUp)
            {
                AddSurvivors();
            }
            else
            {
                // Handle time up case
                PBLevelController levelController = ObjectFindCache<PBLevelController>.Get();
                levelController.OnTimeUp += AddSurvivors;
            }

            void AddSurvivors()
            {
                // Health in ascending order
                survivingRobots.Sort((x, y) => x.Health.CompareTo(y.Health));
                var survivingPlayers = survivingRobots.Select(item => item.PlayerInfoVariable).ToList();
                foreach (var survivingPlayer in survivingPlayers)
                {
                    pbPvPMatch.AddSurvivor(survivingPlayer);
                }
                LogSurvivors(pbPvPMatch, robots);
            }
        }
        base.EndFinalRound(match);
    }
}