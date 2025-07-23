using System.Collections;
using System.Collections.Generic;
using HightLightDebug;
using HyrphusQ.Events;
using LatteGames.Monetization;
using UnityEngine;

public class HotOffersFreeGemsUI : HotOffersCurrencyUI
{
    protected override void ClaimRVBtn_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData data)
    {
        base.ClaimRVBtn_OnRewardGranted(data);

        #region MonetizationEventCode
        if (m_ClaimRVBtn.Location == AdsLocation.None)
        {
            Debug.LogWarning("HotOffersFreeGemsUI - AdsLocation Null");
            return;
        }

        string offerName = $"FreeGem";
        MonetizationEventCode monetizationEventCode = m_ClaimRVBtn.Location switch
        {
            AdsLocation.RV_FreeGem_HotOffers_UI => MonetizationEventCode.HotOffers_Shop,
            AdsLocation.RV_FreeGem_Resource_Popup_UI => MonetizationEventCode.HotOffers_ResourcePopup,
            _ => MonetizationEventCode.HotOffers_UpgradePopup
        };

        string adsLocation = m_ClaimRVBtn.Location switch
        {
            AdsLocation.RV_FreeGem_HotOffers_UI => "Shop",
            AdsLocation.RV_FreeGem_Resource_Popup_UI => "Resource Popup",
            _ => "None"

        };
        GameEventHandler.Invoke(monetizationEventCode, adsLocation, offerName);
        DebugPro.CyanBold($"MonetizationEventCode: {monetizationEventCode} | AdsLocation: {adsLocation} | OfferName: {offerName}");
        #endregion
    }
}
