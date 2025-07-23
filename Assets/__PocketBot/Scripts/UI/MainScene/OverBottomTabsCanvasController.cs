using System.Collections;
using System.Collections.Generic;
using LatteGames;
using UnityEngine;

public class OverBottomTabsCanvasController : MonoBehaviour
{
    [SerializeField] PlayModeGroup playModeUI;
    [SerializeField] List<CanvasGroup> popupCanvasGroups;
    [SerializeField] List<CanvasGroupVisibility> controlPopupVisibilities;

    Coroutine trackGameModeUICoroutine;
    Coroutine trackPopupOrderCoroutine;
    int currentShowingPopup = -1;

    private void OnDisable()
    {
        if (trackGameModeUICoroutine != null)
        {
            StopCoroutine(trackGameModeUICoroutine);
        }
        if (trackPopupOrderCoroutine != null)
        {
            StopCoroutine(trackPopupOrderCoroutine);
        }
    }

    private void OnEnable()
    {
        if (trackGameModeUICoroutine != null)
        {
            StopCoroutine(trackGameModeUICoroutine);
        }
        trackGameModeUICoroutine = StartCoroutine(CR_TrackGameModeUI());
    }

    IEnumerator CR_TrackGameModeUI()
    {
        while (true)
        {
            // HACK: temporary fix auto popup and FTUE order.
            if (playModeUI.isShowingModeUI || FTUEMainScene.Instance.IsShowingFTUE)
            {
                foreach (var visibility in controlPopupVisibilities)
                {
                    visibility.HideImmediately();
                }
                if (trackPopupOrderCoroutine != null)
                {
                    StopCoroutine(trackPopupOrderCoroutine);
                }
                yield return new WaitUntil(() => !(playModeUI.isShowingModeUI || FTUEMainScene.Instance.IsShowingFTUE));
            }
            else if (!(playModeUI.isShowingModeUI || FTUEMainScene.Instance.IsShowingFTUE))
            {
                foreach (var visibility in controlPopupVisibilities)
                {
                    visibility.ShowImmediately();
                }
                if (trackPopupOrderCoroutine != null)
                {
                    StopCoroutine(trackPopupOrderCoroutine);
                }
                trackPopupOrderCoroutine = StartCoroutine(CR_TrackPopupOrder());
                yield return new WaitUntil(() => playModeUI.isShowingModeUI || FTUEMainScene.Instance.IsShowingFTUE);
            }
            else
            {
                yield return null;
            }
        }
    }

    IEnumerator CR_TrackPopupOrder()
    {
        while (true)
        {
            yield return new WaitUntil(() => IsAtLeastOnePopupShowing());
            for (var i = 0; i < controlPopupVisibilities.Count; i++)
            {
                if (i == currentShowingPopup)
                {
                    controlPopupVisibilities[i].ShowImmediately();
                }
                else if (i != currentShowingPopup)
                {
                    controlPopupVisibilities[i].HideImmediately();
                }
            }
            while (true)
            {
                if (PBUltimatePackPopup.Instance.IsShowing)
                {
                    for (var i = 0; i < controlPopupVisibilities.Count; i++)
                    {
                        controlPopupVisibilities[i].HideImmediately();
                    }
                    yield return new WaitUntil(() => !PBUltimatePackPopup.Instance.IsShowing);
                    controlPopupVisibilities[currentShowingPopup].ShowImmediately();
                }
                else if (popupCanvasGroups[currentShowingPopup].alpha == 0)
                {
                    break;
                }
                yield return null;
            }
        }
    }

    bool IsAtLeastOnePopupShowing()
    {
        foreach (var canvasGroup in popupCanvasGroups)
        {
            if (canvasGroup.alpha != 0)
            {
                currentShowingPopup = popupCanvasGroups.IndexOf(canvasGroup);
                return true;
            }
        }
        currentShowingPopup = -1;
        return false;
    }
}
