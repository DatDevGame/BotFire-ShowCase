using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HyrphusQ.Events;
using LatteGames.Monetization;
using PBAnalyticsEvents;
using UnityEngine;

public class PBGeneralAnalyticsEventEmitter : MonoBehaviour
{
    private void Awake()
    {
        GameEventHandler.AddActionEvent(AdvertisingEventCode.OnCloseAd, OnCloseAd);
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseItemCompleted);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(AdvertisingEventCode.OnCloseAd, OnCloseAd);
        GameEventHandler.RemoveActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseItemCompleted);
    }

    void OnCloseAd(object[] _params)
    {
        var adsType = (AdsType)_params[0];
        var location = (AdsLocation)_params[1];
        var isSuccess = (bool)_params[2];
        if (adsType == AdsType.Rewarded && isSuccess)
        {
            PBAnalyticsManager.Instance.RewardedAdCompleted(location);
        }
    }

    void OnPurchaseItemCompleted(object[] _params)
    {
        LG_IAPButton IAPButton = (LG_IAPButton)_params[0];
        var iapProductInfo = IAPButton.IAPProductSO;
        var defaultPriceMatch = Regex.Match(iapProductInfo.defaultPrice, @"([-+]?[0-9]*\.?[0-9]+)");
        var defaultPrice = Convert.ToSingle(defaultPriceMatch.Groups[1].Value);
        var amount = Mathf.RoundToInt(defaultPrice * 100);
        Dictionary<string, object> parameters = new Dictionary<string, object>() {
                {"Type",iapProductInfo.productType},
                {"PackageName",iapProductInfo.productName}
            };
        PBAnalyticsManager.Instance.IAPPurchaseComplete(
                "USD",
                amount,
                iapProductInfo.itemType.ToString(),
                iapProductInfo.itemID,
                IAPButton.CartType.ToString(),
                parameters
            );
    }
}
