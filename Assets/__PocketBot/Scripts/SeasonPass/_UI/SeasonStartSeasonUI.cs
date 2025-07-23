using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class SeasonStartSeasonUI : MonoBehaviour
{
    [SerializeField] CanvasGroupVisibility visibility;
    [SerializeField] EZAnimBase showAnim;
    [SerializeField] Button closeBtn;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(SeasonPassEventCode.ShowStartSeasonPopup, Show);
        closeBtn.onClick.AddListener(Hide);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.ShowStartSeasonPopup, Show);
        closeBtn.onClick.RemoveListener(Hide);
    }

    void Show()
    {
        visibility.ShowImmediately();
        showAnim.SetToStart();
        showAnim.Play();

        #region Progression Event
        string status = "Start";
        string content = "Season";
        GameEventHandler.Invoke(ProgressionEvent.Progression, status, content);
        #endregion
    }

    void Hide()
    {
        showAnim.SetToEnd();
        showAnim.InversePlay(() => StartCoroutine(CommonCoroutine.Delay(AnimationDuration.TINY, false, () =>
        {
            visibility.HideImmediately();
            GameEventHandler.Invoke(SeasonPassEventCode.HideStartSeasonPopup);
        })));
    }
}
