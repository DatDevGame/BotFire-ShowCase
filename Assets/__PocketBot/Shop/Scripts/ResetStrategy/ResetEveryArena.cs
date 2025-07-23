using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class ResetEveryArena : MonoBehaviour, IResetStrategy
{
    public event Action onReset;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnNewArenaUnlocked, ResetData);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnNewArenaUnlocked, ResetData);
    }

    private void ResetData()
    {
        onReset?.Invoke();
    }
}