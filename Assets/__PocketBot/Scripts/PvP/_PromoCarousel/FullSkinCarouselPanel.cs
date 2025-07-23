using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Monetization;
using UnityEngine;
using UnityEngine.UI;

public class FullSkinCarouselPanel : CarouselPanel
{
    [SerializeField] IAPProductSO productSO;
    public override bool isAvailable => !productSO.IsPurchased;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
    }

    private void OnPurchaseCompleted(params object[] objects)
    {
        if (objects[0] is LG_IAPButton lG_IAPButton && lG_IAPButton.IAPProductSO == productSO)
        {
            if (productSO.IsPurchased)
            {
                OnPurchased?.Invoke(this, index);
            }
        }
    }
}
