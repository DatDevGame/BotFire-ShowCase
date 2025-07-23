using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.SerializedDataStructure;
using I2.Loc;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NoAdsOfferShopPackBG : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] List<NoAdsBtn> noAdsBtns;
    [SerializeField, BoxGroup("Ref")] GameObject container;

    private void Start()
    {
        if (NoAdsOffersPopup.Instance.hasRemovedAds)
        {
            container.SetActive(false);
            return;
        }
        UpdateView();
        if (!NoAdsOffersPopup.Instance.isEnoughTrophyToDiscount)
        {
            StartCoroutine(CommonCoroutine.WaitUntil(() => NoAdsOffersPopup.Instance.isEnoughTrophyToDiscount, () =>
            {
                UpdateView();
                StopAllCoroutines();
            }));
        }
        if (!NoAdsOffersPopup.Instance.hasRemovedAds)
        {
            StartCoroutine(CommonCoroutine.WaitUntil(() => NoAdsOffersPopup.Instance.hasRemovedAds, () =>
            {
                container.SetActive(!NoAdsOffersPopup.Instance.hasRemovedAds);
                StopAllCoroutines();
            }));
        }
    }

    void UpdateView()
    {
        foreach (var noAdsBtn in noAdsBtns)
        {
            noAdsBtn.m_IAPButton.OverrideSetup(NoAdsOffersPopup.Instance.isEnoughTrophyToDiscount ? noAdsBtn.discountableIAPProduct.discountProduct : noAdsBtn.discountableIAPProduct.normalProduct);
            noAdsBtn.m_PromotedViewController.EnablePromotedView(NoAdsOffersPopup.Instance.isEnoughTrophyToDiscount);
        }
    }

    [Serializable]
    public class NoAdsBtn
    {
        public PromotedViewController m_PromotedViewController;
        public LG_IAPButton m_IAPButton;
        public DiscountableIAPProduct discountableIAPProduct;
    }
}

