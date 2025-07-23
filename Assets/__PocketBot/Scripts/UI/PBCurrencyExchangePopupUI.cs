using System.Collections.Generic;
using System.Linq;
using HightLightDebug;
using HyrphusQ.Events;
using LatteGames.Monetization;
using PackReward;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

public class PBCurrencyExchangePopupUI : CurrencyExchangePopupUI
{
    [SerializeField, BoxGroup("Ref")] protected PackRewardUI m_PBFreeCoin;
    [SerializeField, BoxGroup("Property")] protected bool isEnableRVCooldownOrWinMatch;
    [SerializeField, BoxGroup("Data")] protected CurrentHighestArenaVariable currentHighestArenaVariable;
    [SerializeField, BoxGroup("Data")] protected ExchangeCurrencySO m_ExchangeCurrencySO;

    protected GrayscaleUI m_ExchangeButtonGrayscaleUI;
    protected PBPartSO currentPartSO;
    protected List<LG_IAPButton> m_IAPButtons;

    protected override void Awake()
    {
        base.Awake();
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Premium).onValueChanged += OnValueChanged;
        m_ExchangeButtonGrayscaleUI = m_ExchangeButton.GetComponent<GrayscaleUI>();
        m_PBFreeCoin.OnClaimAction += OnCompleteRV_FreeCoin;
        m_IAPButtons = GetComponentsInChildren<LG_IAPButton>().ToList();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Premium).onValueChanged -= OnValueChanged;
        m_PBFreeCoin.OnClaimAction -= OnCompleteRV_FreeCoin;
    }

    protected virtual void OnValueChanged(ValueDataChanged<float> eventData) => UpdateView();

    protected override float CalcRequiredAmountOfPremiumCurrency(float exchangeAmountOfStandardCurrency)
    {
        return Mathf.Max(1, (int)m_ExchangeCurrencySO.CalcRequiredAmountOfPremiumCurrencyFollowingArena(exchangeAmountOfStandardCurrency));
    }

    protected virtual void UpdateView()
    {
        var premiumCurrencySO = CurrencyManager.Instance.GetCurrencySO(CurrencyType.Premium);
        var requiredAmountOfPremiumCurrency = CalcRequiredAmountOfPremiumCurrency(m_ExchangeAmountOfStandardCurrency);
        var isEnoughCurrency = premiumCurrencySO.value >= requiredAmountOfPremiumCurrency;
        //m_ExchangeButton.interactable = isEnoughCurrency;
        //m_ExchangeButtonGrayscaleUI.SetGrayscale(!isEnoughCurrency);
    }

    protected override void OnShowPopup(object[] parameters)
    {
        base.OnShowPopup(parameters);
        UpdateView();
        var exchangeGemAmount = CalcRequiredAmountOfPremiumCurrency(m_ExchangeAmountOfStandardCurrency);
        bool hasFoundIAPButton = false;
        for (var i = 0; i < m_IAPButtons.Count; i++)
        {
            var button = m_IAPButtons[i];
            if (!hasFoundIAPButton && (i >= m_IAPButtons.Count + 1 || button.IAPProductSO.currencyItems[CurrencyType.Premium].value >= exchangeGemAmount))
            {
                hasFoundIAPButton = true;
                button.gameObject.SetActive(true);
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }

        currentPartSO = parameters[2] as PBPartSO;

        #region Design Events
        try
        {
            if (m_PBFreeCoin != null)
            {
                string type = m_PBFreeCoin.PackRewardState == PackReward.PackRewardState.Ready ? "FreeCoinAvailable" : "FreeCoinUnavailable";
                string rvName = $"HotOffers_{type}";
                string location = "ExchangePopup";
                GameEventHandler.Invoke(DesignEvent.RVShow, rvName, location);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }

        try
        {
            string popupName = "Upgrade";
            string status = DesignEventStatus.Start;
            string operation = "Manually";
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    protected override void OnHidePopup()
    {
        base.OnHidePopup();

        #region Design Events
        try
        {
            string popupName = "Upgrade";
            string status = DesignEventStatus.Complete;
            string operation = "Manually";
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    protected override void OnExchangeButtonClicked()
    {
        var premiumCurrencySO = CurrencyManager.Instance.GetCurrencySO(CurrencyType.Premium);
        var standardCurrencySO = CurrencyManager.Instance.GetCurrencySO(CurrencyType.Standard);
        var requiredAmountOfPremiumCurrency = CalcRequiredAmountOfPremiumCurrency(m_ExchangeAmountOfStandardCurrency);
        if (premiumCurrencySO.value >= requiredAmountOfPremiumCurrency)
        {
            // Exchange Premium currency to Standard currency
            premiumCurrencySO.Spend(requiredAmountOfPremiumCurrency, m_ResourceLocationProvider.GetLocation(), m_ResourceLocationProvider.GetItemId());
            standardCurrencySO.Acquire(m_ExchangeAmountOfStandardCurrency, m_ResourceLocationProvider.GetLocation(), m_ResourceLocationProvider.GetItemId());
            m_ExchangeResponseCallback?.Invoke(true);
            GameEventHandler.Invoke(CurrencyExchangePopupEventCode.OnHideExchangeCurrencyPopupUI);
        }
        else
        {
            //IAPGemPackPopup.Instance?.Show();
        }
    }

    protected virtual void OnCompleteRV_FreeCoin()
    {
        if (currentPartSO.TryGetCurrentUpgradeRequirement(out Requirement_Currency requirement))
        {
            var remainedAmountOfCurrency = requirement.requiredAmountOfCurrency - requirement.currentAmountOfCurrency;
            var standardCurrencySO = CurrencyManager.Instance.GetCurrencySO(CurrencyType.Standard);
            var isEnoughCurrency = standardCurrencySO.value >= requirement.requiredAmountOfCurrency;
            var requiredAmountOfPremiumCurrency = CalcRequiredAmountOfPremiumCurrency(remainedAmountOfCurrency);
            m_ExchangeAmountOfStandardCurrency = remainedAmountOfCurrency;

            if (isEnoughCurrency)
            {
                GameEventHandler.Invoke(CurrencyExchangePopupEventCode.OnHideExchangeCurrencyPopupUI);
            }
            else
            {
                m_AmountOfStandardCurrencyText.SetText(m_AmountOfStandardCurrencyText.blueprintText.Replace(Const.StringValue.PlaceholderValue, remainedAmountOfCurrency.ToRoundedText()));
                m_AmountOfPremiumCurrencyText.SetText(m_AmountOfPremiumCurrencyText.blueprintText.Replace(Const.StringValue.PlaceholderValue, requiredAmountOfPremiumCurrency.ToRoundedText()));
                UpdateView();
            }
        }
    }
}