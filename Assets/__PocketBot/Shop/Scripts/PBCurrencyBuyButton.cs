using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public class PBCurrencyBuyButton : CurrencyBuyButton
{
    // TODO: Require GD provide information about the ResourceLocation and ItemId for Sink/Source event
    [TitleGroup("Mandatory"), PropertyOrder(1)]
    public ResourceLocationProvider sinkLocationProviderX2;
    // TODO: Require GD provide information about the ResourceLocation and ItemId for Sink/Source event
    [TitleGroup("Mandatory"), PropertyOrder(1)]
    public ResourceLocationProvider sourceLocationProviderX2;

    [TitleGroup("Optional"), SerializeField]
    private TextMeshProUGUIPair nameText;

    protected override void OnButtonClicked()
    {
        if (currencyProductSO.generalItems != null && currencyProductSO.generalItems.Count > 0 && currencyProductSO.generalItems.Keys.First() is PBGachaPack gachaPack)
        {
            PurchaseConfirmationPopup.Instance.Show(gachaPack, currencyProductSO.price, OnConfirmCallback);
        }
        else
        {
            PurchaseConfirmationPopup.Instance.Show(currencyProductSO.productName, currencyProductSO.price, ClonePopupContent(), OnConfirmCallback);
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

                    #region Source Event
                    try
                    {
                        if (itemSO is GachaCard_RandomActiveSkill)
                        {
                            float skillCardCount = shopProductSO.generalItems.Values.FirstOrDefault().value;
                            ResourceLocationProvider resourceProvider = new ResourceLocationProvider(ResourceLocation.Purchase, $"SkillPack");
                            GameEventHandler.Invoke(LogSinkSource.SkillCard, skillCardCount, resourceProvider);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
                    }
                    #endregion
                }
            }
            else
            {
                GameEventHandler.Invoke(EconomyEventCode.OnPurchaseUnaffordableItem, this);
                OnPurchaseItem?.Invoke(false);
            }
        }
    }

    protected override void SetupView()
    {
        base.SetupView();
        if (!nameText.IsNull())
        {
            nameText.SetText(currencyProductSO.productName);
        }
    }

    protected PurchaseConfirmationPopupContent ClonePopupContent()
    {
        return PurchaseConfirmationPopup.Instance.CreateContent(currencyProductSO);
    }

    public virtual ResourceLocationProvider GetSourceLocationProvider()
    {
        if (currencyProductSO.currencyItems != null && currencyProductSO.currencyItems.Count > 0)
        {
            var discountableValue = currencyProductSO.currencyItems.Values.First();
            return discountableValue.value == discountableValue.originalValue ? sourceLocationProvider : sourceLocationProviderX2;
        }
        return sourceLocationProvider;
    }

    public virtual ResourceLocationProvider GetSinkLocationProvider()
    {
        if (currencyProductSO.currencyItems != null && currencyProductSO.currencyItems.Count > 0)
        {
            var discountableValue = currencyProductSO.currencyItems.Values.First();
            return discountableValue.value == discountableValue.originalValue ? sinkLocationProvider : sinkLocationProviderX2;
        }
        return sinkLocationProvider;
    }
}