using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class KillAndDeathTracker : MonoBehaviour
{
    [SerializeField] float maxThresholdToCountKill = 3;
    Dictionary<PBRobot, KillDeathInfo> robotKillDeathInfos = new Dictionary<PBRobot, KillDeathInfo>();

    public Dictionary<PBRobot, KillDeathInfo> RobotKillDeathInfos { get => robotKillDeathInfos; }

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
    }

    void OnAnyPlayerDied(params object[] parameters)
    {
        PBRobot pbRobot = (PBRobot)parameters[0];
        PBRobot killer = null;
        PBRobot victim = pbRobot;
        if (Time.time - pbRobot.LastTimeReceiveDamage < maxThresholdToCountKill && pbRobot.LastRobotCauseDamageToMe != null)
        {
            if (!robotKillDeathInfos.ContainsKey(pbRobot.LastRobotCauseDamageToMe))
            {
                robotKillDeathInfos.Add(pbRobot.LastRobotCauseDamageToMe, new KillDeathInfo() { kills = 0, deaths = 0 });
            }
            robotKillDeathInfos[pbRobot.LastRobotCauseDamageToMe].kills++;
            killer = pbRobot.LastRobotCauseDamageToMe;
        }
        if (!robotKillDeathInfos.ContainsKey(pbRobot))
        {
            robotKillDeathInfos.Add(pbRobot, new KillDeathInfo() { kills = 0, deaths = 0 });
        }
        robotKillDeathInfos[pbRobot].deaths++;
        GameEventHandler.Invoke(KillingSystemEvent.OnKillDeathInfosUpdated, this, killer, victim);
    }
}

public class KillDeathInfo
{
    public int kills;
    public int deaths;
}