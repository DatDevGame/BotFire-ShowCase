using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class StageTheme : MonoBehaviour
{
    [SerializeField]
    private bool isDisabledSkyboxWhenGameStarted;
    [SerializeField]
    private bool isEnabledSkyboxWhenGameEnded;
    [SerializeField]
    private List<GameObject> disabledObjectsIngame;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStarted);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStarted);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);
    }

    private void OnLevelStarted()
    {
        if (isDisabledSkyboxWhenGameStarted)
            MainCameraFindCache.Get().clearFlags = CameraClearFlags.SolidColor;
        disabledObjectsIngame.ForEach(obj => obj.SetActive(false));
    }

    private void OnLevelEnded()
    {
        if (isEnabledSkyboxWhenGameEnded)
            MainCameraFindCache.Get().clearFlags = CameraClearFlags.Skybox;
        disabledObjectsIngame.ForEach(obj => obj.SetActive(true));
    }
}