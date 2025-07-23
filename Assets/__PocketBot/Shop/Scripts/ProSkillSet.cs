using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames.Monetization;
using UnityEngine;

public class ProSkillSet : MonoBehaviour
{
    public event Action OnItemPurchased = delegate { };

    [SerializeField]
    private IntVariable requiredTrophiesToUnlockActiveSkillVar;
    [SerializeField]
    private HighestAchievedPPrefFloatTracker highestAchievedTrophyVar;
    [SerializeField]
    private PPrefIntVariable currentStateVar;
    [SerializeField]
    private PPrefDatetimeVariable expirationTimeVar;
    [SerializeField]
    private DiscountableIAPProduct proSkillSetIAPProduct;
    [SerializeField]
    private LG_IAPButton iapButton;
    [SerializeField]
    private FloatVariable fullPricePackDurationInHoursVar;
    [SerializeField]
    private FloatVariable discountPricePackDurationInHoursVar;
    [SerializeField]
    private LocalizationParamsManager remainingTimeParamsManager;
    [SerializeField]
    private GameObject remainingTimeBarGO;
    [SerializeField]
    private PromotedViewController promotedViewController;

    public DiscountableIAPProduct ProSkillSetIAPProduct => proSkillSetIAPProduct;
    public IntVariable RequiredTrophiesToUnlockActiveSkillVar => requiredTrophiesToUnlockActiveSkillVar;
    public HighestAchievedPPrefFloatTracker HighestAchievedTrophyVar => highestAchievedTrophyVar;
    public PPrefIntVariable CurrentStateVar => currentStateVar;

    private void Start()
    {
        if (proSkillSetIAPProduct.IsPurchased())
        {
            OnItemPurchased.Invoke();
            gameObject.SetActive(false);
            return;
        }
        if (highestAchievedTrophyVar < requiredTrophiesToUnlockActiveSkillVar)
        {
            gameObject.SetActive(false);
            highestAchievedTrophyVar.onValueChanged += OnNumOfTrophiesChanged;
            return;
        }
        currentStateVar.onValueChanged += OnStateChanged;
        UpdateData();
        SetupIAPProduct();
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
    }

    private void OnDestroy()
    {
        highestAchievedTrophyVar.onValueChanged -= OnNumOfTrophiesChanged;
        currentStateVar.onValueChanged -= OnStateChanged;
        GameEventHandler.RemoveActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
    }

    private void OnPurchaseCompleted(object[] parameters)
    {
        if (proSkillSetIAPProduct.IsPurchased())
        {
            GameEventHandler.RemoveActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
            gameObject.SetActive(false);
            OnItemPurchased.Invoke();
        }
    }

    private void Update()
    {
        UpdateData();
        UpdateView();
    }

    private void OnNumOfTrophiesChanged(ValueDataChanged<float> eventData)
    {
        if (eventData.newValue >= requiredTrophiesToUnlockActiveSkillVar && eventData.oldValue < requiredTrophiesToUnlockActiveSkillVar)
        {
            gameObject.SetActive(true);
            UpdateData();
            SetupIAPProduct();
        }
    }

    private void UpdateData()
    {
        if (currentStateVar.value == 0 && !expirationTimeVar.hasKey)
        {
            // Full-price pack
            currentStateVar.value = 1;
            expirationTimeVar.value = DateTime.Now.AddHours(fullPricePackDurationInHoursVar.value);
        }
        else if (currentStateVar.value == 1 && DateTime.Now >= expirationTimeVar.value)
        {
            // Discount-price pack
            currentStateVar.value = 2;
            expirationTimeVar.value = DateTime.Now.AddHours(discountPricePackDurationInHoursVar.value);
        }
        else if (currentStateVar.value == 2 && DateTime.Now >= expirationTimeVar.value)
        {
            // Discount-price pack has expired -> back to Full-price pack and do not show popup anymore
            currentStateVar.value = 3;
        }
    }

    private void OnStateChanged(ValueDataChanged<int> changed)
    {
        SetupIAPProduct();
    }

    private void SetupIAPProduct()
    {
        if (currentStateVar.value == 1)
        {
            promotedViewController.EnablePromotedView(false);
            iapButton.OverrideSetup(proSkillSetIAPProduct.normalProduct);
        }
        else if (currentStateVar.value == 2)
        {
            promotedViewController.EnablePromotedView(true);
            iapButton.OverrideSetup(proSkillSetIAPProduct.discountProduct);
        }
        else if (currentStateVar.value == 3)
        {
            promotedViewController.EnablePromotedView(false);
            iapButton.OverrideSetup(proSkillSetIAPProduct.normalProduct);
            if (remainingTimeBarGO != null)
                remainingTimeBarGO.SetActive(false);
        }
        UpdateView();
    }

    private void UpdateView()
    {
        if (remainingTimeParamsManager != null)
            remainingTimeParamsManager.SetParameterValue("Time", DateTime.Now.ToReadableTimeSpan(expirationTimeVar.value));
    }

    public bool IsUnlocked()
    {
        return highestAchievedTrophyVar >= requiredTrophiesToUnlockActiveSkillVar;
    }
}