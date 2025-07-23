using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class NerfBoss : MonoBehaviour
{
    [SerializeField]
    private float atkCoef = 0.6f;
    [SerializeField]
    private float hpCoef = 0.6f;
    [SerializeField]
    private int numOfBossesToNerf = 1;
    [SerializeField]
    private IntVariable currentBossIndexVar;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private void OnModelSpawned(object[] parameters)
    {
        if (currentBossIndexVar.value > numOfBossesToNerf - 1)
            return;
        if (parameters == null || parameters.Length <= 0)
            return;
        var robot = parameters[0] as PBRobot;
        var isInit = (bool)parameters[2];
        if (isInit && !robot.PlayerInfoVariable.value.isLocal)
        {
            robot.MaxHealth *= hpCoef;
            robot.AtkMultiplier *= atkCoef;
            LGDebug.Log($"Neft boss {robot}", context: robot);
        }
    }
}