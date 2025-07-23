using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.GameManagement;
using UnityEngine;

public class PBSoundUtility : MonoBehaviour
{
    private static bool IsFightingPVP = false;
    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, OnStartLevel);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);
        GameEventHandler.AddActionEvent(SceneManagementEventCode.OnLoadSceneCompleted, HandleLoadSceneCompleted);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, OnStartLevel);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);
        GameEventHandler.RemoveActionEvent(SceneManagementEventCode.OnLoadSceneCompleted, HandleLoadSceneCompleted);
    }

    private void OnStartLevel()
    {
        IsFightingPVP = true;
    }

    private void OnLevelEnded()
    {
        IsFightingPVP = false;
    }

    public static bool IsOnSound()
    {
        bool sceneCondition = SceneManager.GetActiveScene().name == SceneName.PvP.ToString()
            || SceneManager.GetActiveScene().name == SceneName.PvP_BossFight.ToString()
            || SceneManager.GetActiveScene().name == SceneName.FTUE_Fighting.ToString();

        return sceneCondition && IsFightingPVP;
    }

    void HandleLoadSceneCompleted()
    {
        AudioListener.pause = false;
    }
}
