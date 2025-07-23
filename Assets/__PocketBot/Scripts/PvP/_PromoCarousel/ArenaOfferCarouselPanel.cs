using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using I2.Loc;
using LatteGames.Monetization;
using UnityEngine;
using UnityEngine.UI;

public class ArenaOfferCarouselPanel : CarouselPanel
{
    [SerializeField] CurrentHighestArenaVariable currentHighestArenaVariable;
    [SerializeField] SerializedDictionary<PBPvPArenaSO, DiscountableIAPProduct> discountableProducts;
    [SerializeField] List<ArenaOfferShopPackBG.PartCell> partCells;
    [SerializeField] SerializedDictionary<RarityType, Sprite> raritySprites;
    [SerializeField] LocalizationParamsManager titleTxt;
    [SerializeField] PromotedViewController promotedViewController;
    [SerializeField] LG_IAPButton button;

    public PBPvPArenaSO currentArenaSO => (PBPvPArenaSO)currentHighestArenaVariable.value;
    public IAPProductSO currentProductSO
    {
        get
        {
            if (discountableProducts.ContainsKey(currentArenaSO))
            {
                var discountableProduct = discountableProducts[currentArenaSO];
                return (state == ArenaOfferState.ShowDiscount || state == ArenaOfferState.ShowLastChance) ? discountableProduct.discountProduct : discountableProduct.normalProduct;
            }
            else
            {
                return null;
            }
        }
    }
    public override bool isAvailable => currentProductSO != null && !currentProductSO.IsPurchased;
    ArenaOfferState state => ArenaOffersPopup.staticState;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
        button.OverrideSetup(currentProductSO);
        int i = 0;
        foreach (var item in currentProductSO.generalItems)
        {
            var partCell = partCells[i];
            partCell.thumbnail.sprite = item.Key.GetThumbnailImage();
            partCell.rarityOutline.sprite = raritySprites[item.Key.GetRarityType()];
            partCell.amountTxt.text = item.Value.value.ToString();
            i++;
        }
        titleTxt.SetParameterValue("Index", (currentArenaSO.index + 1).ToString());
        promotedViewController.EnablePromotedView(state == ArenaOfferState.ShowDiscount || state == ArenaOfferState.ShowLastChance);
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
