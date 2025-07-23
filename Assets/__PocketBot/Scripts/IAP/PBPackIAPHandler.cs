using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using LatteGames.Monetization;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using DG.Tweening;
using System;
using TMPro;
using HyrphusQ.SerializedDataStructure;

public class PBPackIAPHandler : IAPPurchasingHandler
{
    [SerializeField, BoxGroup("Data")] protected OperationStarterPackVariable operationStarterPackVariable;
    [SerializeField, BoxGroup("StaterPack")] DiscountableIAPProduct starterPackProduct;
    [SerializeField, BoxGroup("StaterPack")] PBChassisSO cicadaSO;
    [SerializeField, BoxGroup("ArenaOffers")] SerializedDictionary<PBPvPArenaSO, DiscountableIAPProduct> discountableProducts;
    [SerializeField, BoxGroup("FullSkin")] DiscountableIAPProduct fullSkinProductSO;
    [SerializeField, BoxGroup("OpenNowOffers")] IAPProductSO limitedOpenNowOfferProduct;
    [SerializeField, BoxGroup("OpenNowOffers")] DiscountableIAPProduct permanentOpenNowOfferProduct;
    [SerializeField, BoxGroup("SeasonPass")] IAPProductSO seasonPassProduct;
    [SerializeField, BoxGroup("SeasonPass")] SeasonPassSO seasonPassSO;
    [SerializeField, BoxGroup("Ultimate")] DiscountableIAPProduct ultimateOfferProduct;

    private bool m_IsMainUI = true;

    private void Awake()
    {
        // Sync the purchasing state of old IAP products to new IAP products
        if (!PlayerPrefs.HasKey("FIRST_TIME_PLAY_V240"))
        {
            PlayerPrefs.SetInt("FIRST_TIME_PLAY_V240", 1);
            if (starterPackProduct.IsPurchased())
            {
                starterPackProduct.SetIsPurchased();
            }
            if (fullSkinProductSO.IsPurchased())
            {
                fullSkinProductSO.SetIsPurchased();
            }
            if (permanentOpenNowOfferProduct.IsPurchased())
            {
                permanentOpenNowOfferProduct.SetIsPurchased();
            }
            if (ultimateOfferProduct.IsPurchased())
            {
                ultimateOfferProduct.SetIsPurchased();
            }
            foreach (var item in discountableProducts)
            {
                if (item.Value.IsPurchased())
                {
                    item.Value.SetIsPurchased();
                }
            }
        }
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickButtonMain);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonShop, OnClickButtonShop);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickButtonMain);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonShop, OnClickButtonShop);
    }

    protected override void HandleProcessPurchase(IAPProductSO productSO, bool processFromRestorePurchases)
    {
        if (processFromRestorePurchases && productSO.productType == ProductType.Consumable)
            return;

        if (!processFromRestorePurchases)
            GameEventHandler.Invoke(LogIAPEventCode.IAPPack, productSO, m_IsMainUI);

        #region Firebase Events
        if (!processFromRestorePurchases && m_IsMainUI)
        {
            switch (productSO.shopPackType)
            {
                case ShopPackType.StarterPack:
                    GameEventHandler.Invoke(IAPPurchased.StarterPack, productSO);
                    break;

                case ShopPackType.ArenaOffer:
                    GameEventHandler.Invoke(IAPPurchased.ArenaOffer, productSO);
                    break;

                case ShopPackType.AllSkinsOffer:
                    GameEventHandler.Invoke(IAPPurchased.AllSkinOffers, productSO);
                    break;
            }
        }
        #endregion

        base.HandleProcessPurchase(productSO, processFromRestorePurchases);
        Dictionary<GachaCard_ActiveSkill, int> skillCardDictionary = new Dictionary<GachaCard_ActiveSkill, int>();
        //General logic
        if ((productSO.generalItems == null || productSO.generalItems.Count == 0) && (productSO.consumableItems == null || productSO.consumableItems.Count == 0) && productSO.currencyItems != null && productSO.currencyItems.Count > 0)
        {
            if (processFromRestorePurchases)
            {
                foreach (var item in productSO.currencyItems)
                {
                    CurrencyManager.Instance.AcquireWithoutLogEvent(item.Key, item.Value.value);
                }
            }
            else
            {
                foreach (var item in productSO.currencyItems)
                {
                    if (MainCurrencyUI.Instance == null)
                    {
                        CurrencyManager.Instance.Acquire(item.Key, item.Value.value, _IAPButton.sourceLocationProvider.GetLocation(), _IAPButton.sourceLocationProvider.GetItemId());
                    }
                    else
                    {
                        CurrencyManager.Instance.AcquireWithAnimation(item.Key, item.Value.value, _IAPButton.sourceLocationProvider.GetLocation(), _IAPButton.sourceLocationProvider.GetItemId(), _IAPButton.CurrencyEmitPoints[item.Key].position);
                    }
                }
            }
        }
        else if ((productSO.generalItems != null && productSO.generalItems.Count > 0) || (productSO.consumableItems != null && productSO.consumableItems.Count > 0))
        {
            List<GachaCard> gachaCards = (GachaCardGenerator.Instance as PBGachaCardGenerator).Generate(productSO);
            if (processFromRestorePurchases)
            {
                foreach (var card in gachaCards)
                {
                    card.GrantReward();
                }
            }
            else
            {
                foreach (var card in gachaCards)
                {
                    if (card is GachaCard_Currency gachaCard_Currency)
                    {
                        gachaCard_Currency.ResourceLocationProvider = _IAPButton.sourceLocationProvider;
                    }

                    #region Log Source Skill Card
                    try
                    {
                        if (card is GachaCard_ActiveSkill activeSkill)
                        {
                            if (!skillCardDictionary.ContainsKey(activeSkill))
                            {
                                skillCardDictionary.Add(activeSkill, 1);
                            }
                            else
                            {
                                skillCardDictionary[activeSkill]++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
                    }
                    #endregion
                }

                GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, gachaCards, null, null);
            }
        }

        #region Log Source Skill Card
        try
        {
            ResourceLocationProvider resourceLocationProvider = new ResourceLocationProvider(ResourceLocation.Purchase, $"SkillPack");
            float skillCardCount = 0;
            for (int i = 0; i < skillCardDictionary.Count; i++)
            {
                skillCardCount += skillCardDictionary.ElementAt(i).Value;
            }
            GameEventHandler.Invoke(LogSinkSource.SkillCard, skillCardCount, resourceLocationProvider);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion

        //Starter Pack
        if (starterPackProduct.Contains(productSO))
        {
            if (!processFromRestorePurchases)
            {
                //GameEventHandler.Invoke(StarterPackEventCode.StarterPackIAP, productSO);
            }
            starterPackProduct.SetIsPurchased();
            if (cicadaSO.TryGetModule(out SkinItemModule skinItemModule))
            {
                foreach (var skin in skinItemModule.skins)
                {
                    skin.TryUnlockIgnoreRequirement();
                }
            }
        }

        //Arena Offers
        foreach (var product in discountableProducts)
        {
            if (product.Value.Contains(productSO))
            {
                product.Value.SetIsPurchased();
            }
        }

        //Full Skin
        if (fullSkinProductSO.Contains(productSO))
        {
            fullSkinProductSO.SetIsPurchased();
            SkinUnlockableItemModule.IsUnlockAll = true;
        }

        //Open Now Offer
        if (permanentOpenNowOfferProduct.Contains(productSO))
        {
            permanentOpenNowOfferProduct.SetIsPurchased();
            OpenNowOfferManager.Instance.PurchasePermanentOpenNow();
        }
        if (productSO == limitedOpenNowOfferProduct)
        {
            OpenNowOfferManager.Instance.PurchaseLimitedOpenNow();
        }

        //Season Pass
        if (productSO == seasonPassProduct)
        {
            seasonPassSO.PurchaseSeasonPass();
        }
    }

    private void OnClickButtonMain()
    {
        m_IsMainUI = true;
    }

    private void OnClickButtonShop()
    {
        m_IsMainUI = false;
    }
}

[Serializable]
public class DiscountableIAPProduct
{
    public IAPProductSO normalProduct;
    public IAPProductSO discountProduct;
    public List<IAPProductSO> linkedProducts = new();


    public bool Contains(IAPProductSO productSO)
    {
        var isLinked = linkedProducts.Contains(productSO);
        return (normalProduct != null && productSO == normalProduct) || (discountProduct != null && productSO == discountProduct) || isLinked;
    }

    public void SetIsPurchased()
    {
        if (normalProduct != null)
        {
            normalProduct.IsPurchased = true;
        }
        if (discountProduct != null)
        {
            discountProduct.IsPurchased = true;
        }
        foreach (var productSO in linkedProducts)
        {
            productSO.IsPurchased = true;
        }
    }

    public bool IsPurchased()
    {
        var isLinked = linkedProducts.Any(x => x.IsPurchased);
        return (normalProduct != null && normalProduct.IsPurchased) || (discountProduct != null && discountProduct.IsPurchased) || isLinked;
    }
}
