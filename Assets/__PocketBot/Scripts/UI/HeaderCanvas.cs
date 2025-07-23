using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;

public class HeaderCanvas : MonoBehaviour
{
    [SerializeField] CanvasGroupVisibility visibility;

    HashSet<string> impactingPopups = new();

    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, RemoveDockTab);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonShop, RemoveDockTab);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonBattlePass, AddDockTab);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, RemoveDockTab);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, AddUnpackPopup);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, RemoveUnpackPopup);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnShowGameOverUI, OnShowGameOverUI);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.ShowEndSeasonPopup, AddEndSeasonPopup);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.HideEndSeasonPopup, RemoveEndSeasonPopup);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.ShowStartSeasonPopup, AddStartSeasonPopup);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.HideStartSeasonPopup, RemoveStartSeasonPopup);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnStartSearchingOpponent, DisableHeader);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, RemoveDockTab);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonShop, RemoveDockTab);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonBattlePass, AddDockTab);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, RemoveDockTab);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, AddUnpackPopup);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, RemoveUnpackPopup);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnShowGameOverUI, OnShowGameOverUI);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.ShowEndSeasonPopup, AddEndSeasonPopup);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.HideEndSeasonPopup, RemoveEndSeasonPopup);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.ShowStartSeasonPopup, AddStartSeasonPopup);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.HideStartSeasonPopup, RemoveStartSeasonPopup);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnStartSearchingOpponent, DisableHeader);
    }

    void AddUnpackPopup()
    {
        impactingPopups.Add("Unpack");
        CheckImpactingPopups();
    }

    void RemoveUnpackPopup()
    {
        impactingPopups.Remove("Unpack");
        CheckImpactingPopups();
    }

    void AddEndSeasonPopup()
    {
        impactingPopups.Add("EndSeason");
        CheckImpactingPopups();
    }

    void RemoveEndSeasonPopup()
    {
        impactingPopups.Remove("EndSeason");
        CheckImpactingPopups();
    }

    void AddStartSeasonPopup()
    {
        impactingPopups.Add("StartSeason");
        CheckImpactingPopups();
    }

    void RemoveStartSeasonPopup()
    {
        impactingPopups.Remove("StartSeason");
        CheckImpactingPopups();
    }

    void AddDockTab()
    {
        impactingPopups.Add("DockTab");
        CheckImpactingPopups();
    }

    void RemoveDockTab()
    {
        impactingPopups.Remove("DockTab");
        CheckImpactingPopups();
    }

    void CheckImpactingPopups()
    {
        if (impactingPopups.Count > 0 && gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
        }
        else if (impactingPopups.Count <= 0 && !gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
    }

    void EnableHeader()
    {
        gameObject.SetActive(true);
    }

    void DisableHeader()
    {
        gameObject.SetActive(false);
    }

    void OnMatchStarted()
    {
        visibility.HideImmediately();
    }

    void OnShowGameOverUI()
    {
        visibility.HideImmediately();
    }
}
