using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HyrphusQ.Events;
using LatteGames.Monetization;
using LatteGames;

public class PBGemPacks : PBCoinPacks
{
    [SerializeField] PPrefBoolVariable removeAdsPPref;
    [SerializeField] Vector2 rewardTxtXPos;
    [SerializeField] Vector2 rewardTxtWidth;
    [SerializeField] private List<ShopProductSO> m_DoubleCurrencyProductSOs;

    protected override void Awake()
    {
        base.Awake();
        removeAdsPPref.onValueChanged += OnRemoveAdsPPrefChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        removeAdsPPref.onValueChanged -= OnRemoveAdsPPrefChanged;
    }

    void OnRemoveAdsPPrefChanged(HyrphusQ.Events.ValueDataChanged<bool> data)
    {
        EnableRemoveAdsView(!removeAdsPPref.value);
    }

    void EnableRemoveAdsView(bool isEnable)
    {
        var shopCell_IAPs = GetComponentsInChildren<ShopCell_IAP>().ToList().FindAll(x => x.button.shopProductSO is IAPProductSO productSO && productSO.isRemoveAds);
        foreach (var item in shopCell_IAPs)
        {
            item.removeAdsIcon.gameObject.SetActive(isEnable);
            item.rewardTxt.rectTransform.anchoredPosition = new Vector2(isEnable ? rewardTxtXPos.y : rewardTxtXPos.x, item.rewardTxt.rectTransform.anchoredPosition.y);
            item.rewardTxt.rectTransform.sizeDelta = new Vector2(isEnable ? rewardTxtWidth.y : rewardTxtWidth.x, item.rewardTxt.rectTransform.sizeDelta.y);
            item.originalRewardTxt.rectTransform.anchoredPosition = new Vector2(isEnable ? rewardTxtXPos.y : rewardTxtXPos.x, item.originalRewardTxt.rectTransform.anchoredPosition.y);
        }
    }

    protected override void Start()
    {
        m_CurrencyPackButtons = GetComponentsInChildren<ShopBuyButton>().ToList();
        m_CurrencyPackViewControllers = m_CurrencyPackButtons.Select(buyButton => buyButton.GetComponent<PromotedViewController>()).ToList();
        for (var i = 0; i < m_DoubleCurrencyProductSOs.Count; i++)
        {
            var doubleProductSO = m_DoubleCurrencyProductSOs[i];
            var productSO = m_CurrencyProductSOs[i];
            var shopCell = m_CurrencyPackViewControllers[i];
            var button = m_CurrencyPackButtons[i];
            var isDoubleValue = IsDoubleValue(doubleProductSO);
            button.OverrideSetup(isDoubleValue ? doubleProductSO : productSO);
            SetDoubleValue(doubleProductSO, isDoubleValue, true);
        }
        EnableRemoveAdsView(!removeAdsPPref.value);
        GameEventHandler.AddActionEvent(m_PurchasePackCompletedEventCode, OnPackPurchased);
    }

    protected override bool IsPurchaseProductOfMine(ShopProductSO shopProductSO)
    {
        if (base.IsPurchaseProductOfMine(shopProductSO))
        {
            return true;
        }
        if (m_DoubleCurrencyProductSOs.Contains(shopProductSO))
        {
            return true;
        }
        return false;
    }

    protected override void SetDoubleValue(ShopProductSO currencyProductSO, bool isDoubleValue, bool isInit = false)
    {
        int index = 0;
        if (m_DoubleCurrencyProductSOs.Contains(currencyProductSO))
        {
            index = m_DoubleCurrencyProductSOs.IndexOf(currencyProductSO);
        }
        else if (m_CurrencyProductSOs.Contains(currencyProductSO))
        {
            index = m_CurrencyProductSOs.IndexOf(currencyProductSO);
        }
        m_CurrencyPackViewControllers[index].EnablePromotedView(isDoubleValue);
        if (!isInit)
        {
            m_CurrencyPackButtons[index].OverrideSetup(isDoubleValue ? m_DoubleCurrencyProductSOs[index] : m_CurrencyProductSOs[index]);
            PlayerPrefs.SetInt($"{currencyProductSO.name}_IsDoubleValue", isDoubleValue ? 1 : 0);
        }
    }

    protected override void ResetDoubleValue()
    {
        for (int i = 0; i < m_DoubleCurrencyProductSOs.Count; i++)
        {
            var doubleProductSO = m_DoubleCurrencyProductSOs[i];
            SetDoubleValue(doubleProductSO, true);
        }
    }
}