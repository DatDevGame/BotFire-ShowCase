using System.Collections;
using System.Collections.Generic;
using HyrphusQ.SerializedDataStructure;
using I2.Loc;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OpenNowShopPackBG : MonoBehaviour
{
    public UnityEvent OnShow, OnHide;

    [SerializeField, BoxGroup("General")] GameObject container;
    [SerializeField, BoxGroup("General")] PPrefBoolVariable removeAdsPPref;
    [SerializeField, BoxGroup("General")] GameObject noAdsTitle;
    [SerializeField, BoxGroup("General")] GameObject normalTitle;
    [SerializeField, BoxGroup("Limited")] Button limitedPurchaseBtn;
    [SerializeField, BoxGroup("Limited")] LocalizationParamsManager timeLeftTxt;
    [SerializeField, BoxGroup("Limited")] GameObject inactiveGroup;
    [SerializeField, BoxGroup("Limited")] GameObject activeGroup;
    [SerializeField, BoxGroup("Permanent")] LG_IAPButton permanentPurchaseBtn;
    [SerializeField, BoxGroup("Permanent")] DiscountableIAPProduct permanentOpenNowOfferProduct;
    [SerializeField, BoxGroup("Permanent")] LocalizationParamsManager saleTimeTxt;
    [SerializeField, BoxGroup("Permanent")] PromotedViewController promotedViewController;

    private void Awake()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (permanentOpenNowOfferProduct.IsPurchased())
        {
            return;
        }
        OpenNowOfferManager.Instance.OnMeetShowingCondition += OnMeetShowingCondition;
        OpenNowOfferManager.Instance.OnLimitedSaleTimerUpdated += OnLimitedSaleTimerUpdated;
        OpenNowOfferManager.Instance.OnLimitedStateChanged += OnLimitedStateChanged;
        OpenNowOfferManager.Instance.OnPermanentSaleTimerUpdated += OnPermanentSaleTimerUpdated;
        OpenNowOfferManager.Instance.OnPermanentStateChanged += OnPermanentStateChanged;
        permanentPurchaseBtn.OnInteractiveChanged.AddListener(OnInteractiveChanged);
        removeAdsPPref.onValueChanged += OnRemoveAdsPPrefChanged;
    }

    private void OnDestroy()
    {
        if (permanentOpenNowOfferProduct.IsPurchased())
        {
            return;
        }
        if (OpenNowOfferManager.Instance != null)
        {
            OpenNowOfferManager.Instance.OnMeetShowingCondition -= OnMeetShowingCondition;
            OpenNowOfferManager.Instance.OnLimitedSaleTimerUpdated -= OnLimitedSaleTimerUpdated;
            OpenNowOfferManager.Instance.OnLimitedStateChanged -= OnLimitedStateChanged;
            OpenNowOfferManager.Instance.OnPermanentSaleTimerUpdated -= OnPermanentSaleTimerUpdated;
            OpenNowOfferManager.Instance.OnPermanentStateChanged -= OnPermanentStateChanged;
        }
        permanentPurchaseBtn.OnInteractiveChanged.RemoveListener(OnInteractiveChanged);
        removeAdsPPref.onValueChanged -= OnRemoveAdsPPrefChanged;
    }

    private void Start()
    {
        EnableRemoveAdsView(!removeAdsPPref.value);
        OnMeetShowingCondition();
    }

    void OnRemoveAdsPPrefChanged(HyrphusQ.Events.ValueDataChanged<bool> data)
    {
        EnableRemoveAdsView(!removeAdsPPref.value);
    }

    void EnableRemoveAdsView(bool isEnable)
    {
        noAdsTitle.SetActive(isEnable);
        normalTitle.SetActive(!isEnable);
    }

    private void OnMeetShowingCondition()
    {
        if (permanentOpenNowOfferProduct.IsPurchased())
        {
            container.SetActive(false);
            OnHide?.Invoke();
            return;
        }
        UpdateView();
        OnLimitedStateChanged();
        OnPermanentStateChanged();
    }

    void OnInteractiveChanged(bool isInteractive)
    {
        if (permanentOpenNowOfferProduct.IsPurchased())
        {
            container.SetActive(false);
            OnHide?.Invoke();
            return;
        }
    }

    void UpdateView()
    {
        if (OpenNowOfferManager.Instance.IsMeetShowingCondition)
        {
            container.SetActive(true);
            limitedPurchaseBtn.enabled = !OpenNowOfferManager.Instance.IsApplyingLimitedOpenNow;
            OnShow?.Invoke();
        }
        else
        {
            container.SetActive(false);
            OnHide?.Invoke();
        }
    }

    void OnLimitedSaleTimerUpdated(string time)
    {
        timeLeftTxt.SetParameterValue("Time", $"{time}");
    }

    void OnLimitedStateChanged()
    {
        inactiveGroup.SetActive(!OpenNowOfferManager.Instance.IsApplyingLimitedOpenNow);
        activeGroup.SetActive(OpenNowOfferManager.Instance.IsApplyingLimitedOpenNow);
        limitedPurchaseBtn.enabled = !OpenNowOfferManager.Instance.IsApplyingLimitedOpenNow;
    }

    void OnPermanentSaleTimerUpdated(string time)
    {
        saleTimeTxt.SetParameterValue("Time", $"<color=#ffffffff>{time}</color>");
    }

    void OnPermanentStateChanged()
    {
        if (OpenNowOfferManager.Instance.permanentOpenNowOfferState == PermanentOpenNowOfferState.Discount)
        {
            promotedViewController.EnablePromotedView(true);
            permanentPurchaseBtn.OverrideSetup(permanentOpenNowOfferProduct.discountProduct);
            saleTimeTxt.gameObject.SetActive(true);
        }
        else if (OpenNowOfferManager.Instance.permanentOpenNowOfferState == PermanentOpenNowOfferState.Original)
        {
            promotedViewController.EnablePromotedView(false);
            permanentPurchaseBtn.OverrideSetup(permanentOpenNowOfferProduct.normalProduct);
            saleTimeTxt.gameObject.SetActive(false);
        }
    }
}
