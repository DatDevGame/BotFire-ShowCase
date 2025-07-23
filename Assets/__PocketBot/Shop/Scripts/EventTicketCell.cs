using HyrphusQ.Events;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventTicketCell : PBCurrencyBuyButton
{
    [SerializeField, BoxGroup("Config")] protected bool m_IsShop;
    [SerializeField, BoxGroup("Config")] protected ResourceLocationProvider m_ShopSinkResourceLocationProvider;
    [SerializeField, BoxGroup("Config")] protected ResourceLocationProvider m_ShopSinkResourceLocationProviderX2;
    [SerializeField, BoxGroup("Config")] protected ResourceLocationProvider m_ShopSourceResourceLocationProvider;
    [SerializeField, BoxGroup("Config")] protected ResourceLocationProvider m_ShopSourceResourceLocationProviderX2;

    protected override void OnButtonClicked()
    {
        if (currencyProductSO.currencyItems != null && currencyProductSO.currencyItems.Count > 0)
        {
            PurchaseConfirmationPopup.Instance.Show(currencyProductSO.productName, currencyProductSO.price, ClonePopupContent(), OnConfirmCallback);
        }
        else if (currencyProductSO.generalItems != null && currencyProductSO.generalItems.Count > 0)
        {
            PurchaseConfirmationPopup.Instance.Show(currencyProductSO.generalItems.Keys.First().Cast<PBGachaPack>(), currencyProductSO.price, OnConfirmCallback);
        }

        void OnConfirmCallback(bool isAccept)
        {
            if (isAccept && CurrencyManager.Instance[currencyProductSO.currencyType].Spend(currencyProductSO.price, GetSinkLocationProvider().GetLocation(), GetSinkLocationProvider().GetItemId()))
            {
                GameEventHandler.Invoke(EconomyEventCode.OnPurchaseItemCompleted, this);
                OnPurchaseItem?.Invoke(true);

                if (currencyProductSO.generalItems != null)
                {
                    ItemSO itemSO = currencyProductSO.generalItems.Keys.FirstOrDefault();
                    if (itemSO != null && itemSO is PBGachaPack pBGachaPack)
                    {
                        #region DesignEvent
                        PBBoxOffers pbBoxOffers = gameObject.GetComponentInParent<PBBoxOffers>();
                        if (pbBoxOffers != null)
                        {
                            string openStatus = "NoTimer";
                            string location = pbBoxOffers.IsShop ? "Shop" : "BoxShopPopup";
                            GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
                        }

                        #endregion

                        #region Firebase Event
                        if (pBGachaPack != null)
                        {
                            string openType = "gems";
                            GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, pBGachaPack, openType);
                        }
                        #endregion
                    }
                }
            }
            else
            {
                GameEventHandler.Invoke(EconomyEventCode.OnPurchaseUnaffordableItem, this);
                OnPurchaseItem?.Invoke(false);
            }
        }
    }

    public ResourceLocationProvider GetSourceLocationProvider()
    {
        ResourceLocationProvider locationProvider = sourceLocationProvider;
        if (currencyProductSO.currencyItems != null && currencyProductSO.currencyItems.Count > 0)
        {
            var discountableValue = currencyProductSO.currencyItems.Values.First();
            ResourceLocationProvider sourceLocationProviderX1Handle = m_IsShop ? m_ShopSourceResourceLocationProvider : sourceLocationProvider;
            ResourceLocationProvider sourceLocationProviderX2Handle = m_IsShop ? m_ShopSourceResourceLocationProviderX2 : sourceLocationProviderX2;
            locationProvider = discountableValue.value == discountableValue.originalValue ? sourceLocationProviderX1Handle : sourceLocationProviderX2Handle;
            return locationProvider;
        }

        return locationProvider;
    }

    public ResourceLocationProvider GetSinkLocationProvider()
    {
        ResourceLocationProvider locationProvider = new ResourceLocationProvider(ResourceLocation.Purchase, $"{shopProductSO.productName}");
        if (currencyProductSO.currencyItems != null && currencyProductSO.currencyItems.Count > 0)
        {
            var discountableValue = currencyProductSO.currencyItems.Values.First();
            return locationProvider;
        }
        return locationProvider;
    }
}
