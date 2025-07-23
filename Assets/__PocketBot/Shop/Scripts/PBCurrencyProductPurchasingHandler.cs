using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames.Monetization;
using HyrphusQ.Events;
using GachaSystem.Core;

public class PBCurrencyProductPurchasingHandler : CurrencyProductPurchasingHandler
{
    protected override void OnProcessPurchase(object[] _params)
    {
        currencyBuyButton = (CurrencyBuyButton)_params[0];
        currencyBuyButton.currencyProductSO.IsPurchased = true;

        // Currency pack
        if (currencyBuyButton.currencyProductSO.currencyItems != null && currencyBuyButton.currencyProductSO.currencyItems.Count >= 0)
        {
            if (currencyBuyButton is EventTicketCell eventTicketCell)
            {
                foreach (var currencyItem in currencyBuyButton.currencyProductSO.currencyItems)
                    CurrencyManager.Instance.AcquireWithAnimation(currencyItem.Key, currencyItem.Value.value, eventTicketCell.GetSourceLocationProvider().GetLocation(), eventTicketCell.GetSourceLocationProvider().GetItemId(), currencyBuyButton.CurrencyEmitPoints.Get(currencyItem.Key).position);
            }
            else
            {
                var pbCurrencyBuyButton = currencyBuyButton as PBCurrencyBuyButton;
                foreach (var currencyItem in currencyBuyButton.currencyProductSO.currencyItems)
                    CurrencyManager.Instance.AcquireWithAnimation(currencyItem.Key, currencyItem.Value.value, pbCurrencyBuyButton.GetSourceLocationProvider().GetLocation(), pbCurrencyBuyButton.GetSourceLocationProvider().GetItemId(), currencyBuyButton.CurrencyEmitPoints.Get(currencyItem.Key).position);
            }
        }
        // Box offer
        else if (currencyBuyButton.currencyProductSO.generalItems != null && currencyBuyButton.currencyProductSO.generalItems.Count >= 0)
        {
            List<GachaCard> gachaCards = PBGachaCardGenerator.Instance.Generate(currencyBuyButton.currencyProductSO);
            List<GachaPack> gachaPacks = new List<GachaPack>();
            foreach (var item in currencyBuyButton.currencyProductSO.generalItems)
            {
                if (item.Key is GachaPack gachaPack)
                {
                    gachaPacks.Add(gachaPack);
                }
            }
            GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, gachaCards, gachaPacks, null);
        }
    }
}