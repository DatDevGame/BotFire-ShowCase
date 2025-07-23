using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.PvP.TrophyRoad;
using UnityEngine;

public class MainScreenUI : Singleton<MainScreenUI>
{
    public static bool IsShowing { get; set; } = true;

    [SerializeField]
    CanvasGroupVisibility visibility;
    HashSet<string> impactingPopups = new();

    protected override void Awake()
    {
        GameEventHandler.AddActionEvent(TransformerPreviewEvent.OnPreviewShowed, OnPreviewShowed);
        GameEventHandler.AddActionEvent(TransformerPreviewEvent.OnPreviewHiden, OnPreviewHiden);
        GameEventHandler.AddActionEvent(BossFightEventCode.OnUnlockBoss, OnUnlockBoss);
        GameEventHandler.AddActionEvent(BossFightEventCode.OnDisableUnlockBoss, OnDisableUnlockBoss);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, AddUnpackPopup);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, RemoveUnpackPopup);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, AddTrophyRoadPopup);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, RemoveTrophyRoadPopup);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.ShowEndSeasonPopup, AddEndSeasonPopup);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.HideEndSeasonPopup, RemoveEndSeasonPopup);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.ShowStartSeasonPopup, AddStartSeasonPopup);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.HideStartSeasonPopup, RemoveStartSeasonPopup);

        visibility.GetOnStartShowEvent().Subscribe(OnStartShow);
        visibility.GetOnEndHideEvent().Subscribe(OnEndHide);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(TransformerPreviewEvent.OnPreviewShowed, OnPreviewShowed);
        GameEventHandler.RemoveActionEvent(TransformerPreviewEvent.OnPreviewHiden, OnPreviewHiden);
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnUnlockBoss, OnUnlockBoss);
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnDisableUnlockBoss, OnDisableUnlockBoss);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, AddUnpackPopup);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, RemoveUnpackPopup);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, AddTrophyRoadPopup);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, RemoveTrophyRoadPopup);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.ShowEndSeasonPopup, AddEndSeasonPopup);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.HideEndSeasonPopup, RemoveEndSeasonPopup);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.ShowStartSeasonPopup, AddStartSeasonPopup);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.HideStartSeasonPopup, RemoveStartSeasonPopup);

        visibility.GetOnStartShowEvent().Unsubscribe(OnStartShow);
        visibility.GetOnEndHideEvent().Unsubscribe(OnEndHide);
    }

    void OnStartShow()
    {
        IsShowing = true;
    }

    void OnEndHide()
    {
        IsShowing = false;
    }

    void OnPreviewShowed()
    {
        visibility.HideImmediately();
    }

    void OnPreviewHiden()
    {
        visibility.ShowImmediately();
    }

    void OnUnlockBoss()
    {
        visibility.Hide();
    }

    void OnDisableUnlockBoss()
    {
        visibility.Show();
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

    void AddTrophyRoadPopup()
    {
        impactingPopups.Add("TrophyRoad");
        CheckImpactingPopups();
    }

    void RemoveTrophyRoadPopup()
    {
        impactingPopups.Remove("TrophyRoad");
        CheckImpactingPopups();
    }

    void CheckImpactingPopups()
    {
        if (impactingPopups.Count > 0 && IsShowing)
        {
            visibility.HideImmediately();
        }
        else if (impactingPopups.Count <= 0 && !IsShowing)
        {
            visibility.ShowImmediately();
        }
    }
}
