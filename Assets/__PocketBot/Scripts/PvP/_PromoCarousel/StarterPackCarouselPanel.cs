using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Monetization;
using UnityEngine;
using UnityEngine.UI;

public class StarterPackCarouselPanel : CarouselPanel
{
    [SerializeField] PromotedViewController promotedViewController;
    [SerializeField] DiscountableIAPProduct discountableIAPProduct;
    [SerializeField] LG_IAPButton button;
    public override bool isAvailable => !discountableIAPProduct.IsPurchased();
    public IAPProductSO currentProductSO => starterPackState == StarterPackState.ShowDiscount || starterPackState == StarterPackState.ShowLastChance ? discountableIAPProduct.discountProduct : discountableIAPProduct.normalProduct;
    StarterPackState starterPackState => StarterPack.staticState;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
        button.OverrideSetup(currentProductSO);
        promotedViewController.EnablePromotedView(starterPackState == StarterPackState.ShowDiscount || starterPackState == StarterPackState.ShowLastChance);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
    }

    private void OnPurchaseCompleted(params object[] objects)
    {
        if (objects[0] is LG_IAPButton lG_IAPButton && lG_IAPButton.IAPProductSO == currentProductSO)
        {
            if (currentProductSO.IsPurchased)
            {
                OnPurchased?.Invoke(this, index);
            }
        }
    }
}
