using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;

public class PvpUICanvas : MonoBehaviour
{
    void Start()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnShowGameOverUI);
    }
    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnShowGameOverUI);
    }
    void OnShowGameOverUI()
    {
        this.gameObject.SetActive(false);
    }
}
