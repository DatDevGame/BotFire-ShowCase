using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;

public class IAPProcessingScreen : MonoBehaviour
{
    [SerializeField] CanvasGroupVisibility canvasGroupVisibility;
    [SerializeField] CanvasGroupVisibility canvasGroupVisibilityFailed;
    [SerializeField] CanvasGroupVisibility canvasGroupVisibilityCanceled;
    private void Awake()
    {
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemStarted, () =>
        {
            canvasGroupVisibility.Show();
        });
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemFailed, () =>
        {
            canvasGroupVisibility.Hide();
            canvasGroupVisibilityFailed.ShowImmediately();
        });
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCanceled, () =>
        {
            canvasGroupVisibility.Hide();
            canvasGroupVisibilityCanceled.ShowImmediately();
        });
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, () =>
        {
            canvasGroupVisibility.Hide();
        });
    }
}
