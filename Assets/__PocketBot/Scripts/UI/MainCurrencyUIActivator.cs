using System.Collections;
using System.Collections.Generic;
using LatteGames;
using UnityEngine;

public class MainCurrencyUIActivator : MonoBehaviour
{
    [SerializeField] CanvasGroupVisibility canvasGroupVisibility;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] MainCurrencyUI mainCurrencyUI;

    private void Start()
    {
        mainCurrencyUI.enabled = canvasGroup.alpha == 1;
        canvasGroupVisibility.GetOnStartHideEvent().Subscribe(OnStartHide);
        canvasGroupVisibility.GetOnStartShowEvent().Subscribe(OnStartShow);
    }

    private void OnStartHide()
    {
        mainCurrencyUI.enabled = false;
    }

    void OnStartShow()
    {
        mainCurrencyUI.enabled = true;
    }
}
