using System.Collections;
using System.Collections.Generic;
using HightLightDebug;
using HyrphusQ.Events;
using LatteGames.Monetization;
using PackReward;
using UnityEngine;
using UnityEngine.XR;

public class HotOffersFreeCoinsUI : HotOffersCurrencyUI
{
    protected override void ClaimRVBtn_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData data)
    {
        base.ClaimRVBtn_OnRewardGranted(data);

        #region MonetizationEventCode
        if (m_ClaimRVBtn.Location == AdsLocation.None)
        {
            Debug.LogWarning("HotOffersFreeCoinsUI - AdsLocation Null");
            return;
        }    

        string offerName = $"FreeCoin";
        MonetizationEventCode monetizationEventCode = m_ClaimRVBtn.Location switch
        {
            AdsLocation.RV_FreeCoin_Upgrade_UI => MonetizationEventCode.HotOffers_UpgradePopup,
            AdsLocation.RV_FreeCoin_HotOffers_UI => MonetizationEventCode.HotOffers_Shop,
            AdsLocation.RV_FreeCoin_Resource_Popup_UI => MonetizationEventCode.HotOffers_ResourcePopup,
            _ => MonetizationEventCode.HotOffers_UpgradePopup
        };

        string adsLocation = m_ClaimRVBtn.Location switch
        {
            AdsLocation.RV_FreeCoin_Upgrade_UI => "Upgrade Popup",
            AdsLocation.RV_FreeCoin_HotOffers_UI => "Shop",
            AdsLocation.RV_FreeCoin_Resource_Popup_UI => "Resource Popup",
            _ => "None"
        };
        GameEventHandler.Invoke(monetizationEventCode, adsLocation, offerName);
        #endregion
    }
}
