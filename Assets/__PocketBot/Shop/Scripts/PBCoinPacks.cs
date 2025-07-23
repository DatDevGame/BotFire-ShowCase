using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HyrphusQ.Events;
using Sirenix.OdinInspector;

public class PBCoinPacks : MonoBehaviour
{
    [SerializeField]
    protected CurrentHighestArenaVariable m_CurrentHighestArenaVar;
    [SerializeField]
    protected EventCode m_PurchasePackCompletedEventCode;
    [SerializeField]
    protected List<ShopProductSO> m_CurrencyProductSOs;
    [SerializeField]
    private PPrefDatetimeVariable m_NextMondayTimeVar;

    protected IResetStrategy m_ResetStrategy;
    protected List<ShopBuyButton> m_CurrencyPackButtons;
    protected List<PromotedViewController> m_CurrencyPackViewControllers;
    protected Dictionary<ShopProductSO, float> m_OriginalCurrencyValueDict = new Dictionary<ShopProductSO, float>();

    protected virtual void Awake()
    {
        m_ResetStrategy = GetComponent<IResetStrategy>();
        m_ResetStrategy.onReset += ResetDoubleValue;
    }

    protected virtual void Start()
    {
        Initialize();
        if (!m_NextMondayTimeVar.hasKey)
            ResetDoubleValue();
        GameEventHandler.AddActionEvent(m_PurchasePackCompletedEventCode, OnPackPurchased);
    }

    protected virtual void OnDestroy()
    {
        foreach (var currencyProductSO in m_CurrencyProductSOs)
        {
            SetDoubleValue(currencyProductSO, false, true);
        }
        GameEventHandler.RemoveActionEvent(m_PurchasePackCompletedEventCode, OnPackPurchased);
    }

    protected virtual bool IsPurchaseProductOfMine(ShopProductSO shopProductSO)
    {
        return m_CurrencyProductSOs.Contains(shopProductSO);
    }

    protected void OnPackPurchased(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        var shopBuyButton = (ShopBuyButton)parameters[0];
        if (!IsPurchaseProductOfMine(shopBuyButton.shopProductSO))
            return;
        SetDoubleValue(shopBuyButton.shopProductSO, false);
    }

    protected bool IsDoubleValue(ShopProductSO currencyProductSO)
    {
        return PlayerPrefs.GetInt($"{currencyProductSO.name}_IsDoubleValue", 1) == 1;
    }

    protected virtual void Initialize()
    {
        var isInitialized = m_CurrencyPackButtons != null;
        if (isInitialized)
            return;
        m_CurrencyPackButtons = GetComponentsInChildren<ShopBuyButton>().ToList();
        m_CurrencyPackViewControllers = m_CurrencyPackButtons.Select(buyButton => buyButton.GetComponent<PromotedViewController>()).ToList();
        for (int i = 0; i < m_CurrencyProductSOs.Count; i++)
        {
            var currencyProductSO = m_CurrencyProductSOs[i] as PBCurrencyProductSO;
            if (currencyProductSO == null)
                continue;
            var price = currencyProductSO.price;
            var exchangeRate = ExchangeRateTableSO.GetExchangeRateOfOtherItems(ExchangeRateTableSO.ItemType.Coin, (ExchangeRateTableSO.ArenaFlags)(1 << m_CurrentHighestArenaVar.value.index));
            var bonusValue = currencyProductSO.GetBonusValue();
            var arenaMultiplier = currencyProductSO.GetArenaMultiplier(m_CurrentHighestArenaVar.value.index);
            var originalValue = price / exchangeRate * bonusValue * arenaMultiplier;
            var isDoubleValue = IsDoubleValue(currencyProductSO);
            m_OriginalCurrencyValueDict.Set(currencyProductSO, originalValue);
            SetDoubleValue(currencyProductSO, isDoubleValue, true);
            // Dirty fix null currencyProductSO when try to get currencySO
            m_CurrencyPackButtons[i].shopProductSO = currencyProductSO;
            m_CurrencyPackButtons[i].OverrideSetup(currencyProductSO);
        }
    }

    protected virtual void SetDoubleValue(ShopProductSO currencyProductSO, bool isDoubleValue, bool isInit = false)
    {
        var index = m_CurrencyProductSOs.IndexOf(currencyProductSO);
        var originalValue = m_OriginalCurrencyValueDict[currencyProductSO];
        currencyProductSO.currencyItems.Set(CurrencyType.Standard, new ShopProductSO.DiscountableValue() { originalValue = originalValue, value = isDoubleValue ? originalValue * 2f : originalValue });
        m_CurrencyPackViewControllers[index].EnablePromotedView(isDoubleValue);
        if (!isInit)
        {
            m_CurrencyPackButtons[index].UpdateView();
            PlayerPrefs.SetInt($"{currencyProductSO.name}_IsDoubleValue", isDoubleValue ? 1 : 0);
        }
    }

    [Button]
    protected virtual void ResetDoubleValue()
    {
        Initialize();
        for (int i = 0; i < m_CurrencyProductSOs.Count; i++)
        {
            var currencyProductSO = m_CurrencyProductSOs[i];
            SetDoubleValue(currencyProductSO, true);
        }
        m_NextMondayTimeVar.value = DateTime.Today.AddDays(1).GetDayOfNextWeek(DayOfWeek.Monday);
    }
}