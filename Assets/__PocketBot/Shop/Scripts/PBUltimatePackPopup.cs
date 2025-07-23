using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using I2.Loc;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PBUltimatePackPopup : Singleton<PBUltimatePackPopup>
{
    public Action OnUpdateView;
    public Action<string> OnUpdateTime;

    [Serializable]
    public class ItemCell
    {
        public Image thumbnail;
        public Image rarityOutline;
        public TMP_Text amountTxt;
        public TMP_Text nameTxt;
    }

    private const int SHOW_TROPHY_THRESHOLD = 99999999;
    private const int MIN_TROPHY_THRESHOLD = 99999999;
    private const int SHOW_WIN_THRESHOLD = 99999999;

    [SerializeField, BoxGroup("UI Ref")] private LG_IAPButton _IAPButton;
    [SerializeField, BoxGroup("UI Ref")] private CanvasGroupVisibility _mainCanvas;
    [SerializeField, BoxGroup("UI Ref")] private Button _closeButton;
    [SerializeField, BoxGroup("UI Ref")] private PromotedViewController _promotedViewController;
    [SerializeField, BoxGroup("UI Ref")] private GameObject _offerRemainTimeBG;
    [SerializeField, BoxGroup("UI Ref")] private LocalizationParamsManager _offerRemainTimeText;
    [SerializeField, BoxGroup("UI Ref")] private List<ItemCell> _generalItem;
    [SerializeField, BoxGroup("UI Ref")] private SerializedDictionary<CurrencyType, ItemCell> _currencyItem;
    [SerializeField, BoxGroup("No Ads")] private GameObject gemItem;
    [SerializeField, BoxGroup("No Ads")] private GameObject ticketItem;
    [SerializeField, BoxGroup("No Ads")] private GameObject noAdsGroup;
    [SerializeField, BoxGroup("No Ads")] private PPrefBoolVariable removeAdsPPref;
    [SerializeField, BoxGroup("Warning Popup")] protected CanvasGroupVisibility _warningPopup;
    [SerializeField, BoxGroup("Warning Popup")] protected Button _letsLookBtn;
    [SerializeField, BoxGroup("Warning Popup")] protected Button _loseItBtn;
    [SerializeField, BoxGroup("Data")] private TimeBasedRewardSO _autoShowTime;
    [SerializeField, BoxGroup("Data")] private TimeBasedRewardSO _offerEndTime;
    [SerializeField, BoxGroup("Data")] private TimeBasedRewardSO _discountOfferEndTime;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable _offerState;
    [SerializeField, BoxGroup("Data")] private IntVariable _offerCumulatedWinMatchThisSection;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable _offerIsEverShow;
    [SerializeField, BoxGroup("Data")] private BoolVariable _isShowedInThisSection;
    [SerializeField, BoxGroup("Data")] private FloatVariable _highestAchievedMedal;
    [SerializeField, BoxGroup("Data")] private DiscountableIAPProduct _discountableOffer;
    [SerializeField, BoxGroup("Asset")] SerializedDictionary<RarityType, Material> _rarityMaterials;

    private SelectingModeUIFlow _selectingModeUIFlow = null;
    private bool _isShow = false;
    private bool _isUnpacking = false;
    private bool _isUnpackingIAPProduct = false;
    private Coroutine _autoShowCoroutine;
    private PBUltimatePackButton _ultimatePackButton = null;

    // FIXME: Dirty fix overlap with ultimate pack popup
    public bool IsShowing => _isShow;

    #region Design Event
    private string m_Operation;
    #endregion

    private bool isShowRemainTime => state == UltimatePackState.NormalActive ||
        state == UltimatePackState.DiscountActive ||
        state == UltimatePackState.LastChanceDiscount;
    private bool isDiscount => state == UltimatePackState.DiscountActive ||
        state == UltimatePackState.LastChanceDiscount;
    public IAPProductSO currentIAPProductSO => isDiscount ?
        _discountableOffer.discountProduct :
        _discountableOffer.normalProduct;
    private bool isPurchased => _discountableOffer.IsPurchased();

    public UltimatePackState state => (UltimatePackState)_offerState.value;
    public LG_IAPButton IAPButton => _IAPButton;

    public void ConnectButton(PBUltimatePackButton btn)
    {
        _ultimatePackButton = btn;
        _ultimatePackButton.UpdateView(currentIAPProductSO, isDiscount, isShowRemainTime);
        _ultimatePackButton.gameObject.SetActive(!(state == UltimatePackState.None || isPurchased));
        if (state == UltimatePackState.LastChanceDiscount)
        {
            _ultimatePackButton?.UpdateTime("00:00:00");
            OnUpdateTime?.Invoke("00:00:00");
        }
        if (state == UltimatePackState.None && _highestAchievedMedal.value >= SHOW_TROPHY_THRESHOLD)
            ToNextOfferState();
    }

    protected override void Awake()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        base.Awake();
        EnableRemoveAdsView(!removeAdsPPref.value);
        SelectingModeUIFlow.OnCheckFlowCompleted += OnCheckFlowCompleted;
        _highestAchievedMedal.onValueChanged += OnHighestAchievedMedalChanged;
        _closeButton.onClick.AddListener(OnCloseBtnClicked);
        _letsLookBtn.onClick.AddListener(OnLetsLookBtnClicked);
        _loseItBtn.onClick.AddListener(OnLoseItBtnClicked);
        InitOfferStatusTracking();
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
        removeAdsPPref.onValueChanged += OnRemoveAdsPPrefChanged;
    }

    private void OnDestroy()
    {
        SelectingModeUIFlow.OnCheckFlowCompleted -= OnCheckFlowCompleted;
        _highestAchievedMedal.onValueChanged -= OnHighestAchievedMedalChanged;
        _closeButton.onClick.RemoveListener(OnCloseBtnClicked);
        _letsLookBtn.onClick.AddListener(OnLetsLookBtnClicked);
        _loseItBtn.onClick.AddListener(OnLoseItBtnClicked);
        GameEventHandler.RemoveActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStarted);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        removeAdsPPref.onValueChanged -= OnRemoveAdsPPrefChanged;
    }

    void OnRemoveAdsPPrefChanged(HyrphusQ.Events.ValueDataChanged<bool> data)
    {
        EnableRemoveAdsView(!removeAdsPPref.value);
    }

    void EnableRemoveAdsView(bool isEnable)
    {
        noAdsGroup.SetActive(isEnable);
        gemItem.SetActive(!isEnable);
        ticketItem.SetActive(!isEnable);
    }

    private void OnCheckFlowCompleted(SelectingModeUIFlow selectingModeUIFlow)
    {
        _selectingModeUIFlow = selectingModeUIFlow;
    }

    private void OnHighestAchievedMedalChanged(ValueDataChanged<float> dataChanged)
    {
        if (dataChanged.newValue >= SHOW_TROPHY_THRESHOLD &&
            dataChanged.oldValue < SHOW_TROPHY_THRESHOLD &&
            state == UltimatePackState.None)
        {
            ToNextOfferState();
        }
    }

    private IEnumerator WaitUntilOutToMainScreen(Action callback)
    {
        yield return new WaitUntil(() => _selectingModeUIFlow != null && !_selectingModeUIFlow.PlayModeUI.isShowingModeUI && !_selectingModeUIFlow.BossModeUI.isShowing);
        var dockController = FindObjectOfType<PBDockController>();
        yield return new WaitUntil(() => dockController.CurrentSelectedButtonType == ButtonType.Main && LoadingScreenUI.IS_LOADING_COMPLETE);
        callback?.Invoke();
    }

    private IEnumerator UpdateOfferRemainTime(TimeBasedRewardSO timeBasedRewardSO, Action callback)
    {
        while (true)
        {
            string remainTimeStr = GetOfferRemainTime(timeBasedRewardSO);
            _ultimatePackButton?.UpdateTime(remainTimeStr);
            OnUpdateTime?.Invoke(remainTimeStr);
            if (_isShow)
            {
                _offerRemainTimeText.SetParameterValue("Time", remainTimeStr);
            }
            if (timeBasedRewardSO.canGetReward)
            {
                callback?.Invoke();
            }
            yield return null;
        }
    }

    private IEnumerator TrackIsOfferAutoShow()
    {
        yield return new WaitUntil(() =>
            _offerIsEverShow.value == false ||
            (_autoShowTime.canGetReward &&
            _offerCumulatedWinMatchThisSection.value >= SHOW_WIN_THRESHOLD &&
            _isShowedInThisSection.value == false));
        StartCoroutine(WaitUntilOutToMainScreen(() =>
        {
            Show("Automatically");
        }));
    }

    public void TryShowIfCan(int? transformBotID = null)
    {
        // Don't let player buy it before 2 first equipment FTUEs or the game will stuck
        if (_highestAchievedMedal.value < MIN_TROPHY_THRESHOLD)
        {
            var message = I2LHelper.TranslateTerm(I2LTerm.TransformBot_MinTrophy).Replace("{[value]}", MIN_TROPHY_THRESHOLD.ToString());
            ToastUI.Show(message);
            return;
        }
        // Let player open and buy the product even if no offer is actived (UltimatePackState.None)
        if (!transformBotID.HasValue || (currentIAPProductSO.generalItems != null &&
            currentIAPProductSO.generalItems.Keys.Any((item) =>
            {
                return item is PBChassisSO chassisSO && chassisSO.IsTransformBot && chassisSO.TransformBotID == transformBotID.Value;
            })))
        {
            Show("Manually");
        }
    }

    private void Show(params object[] parrameters)
    {
        return;
        if (_isShow)
            return;
        _isShow = true;
        _isUnpacking = false;
        _isUnpackingIAPProduct = false;
        _mainCanvas.ShowImmediately();
        GarageSO garageSO = currentIAPProductSO.generalItems.Keys.First((item) => item is GarageSO) as GarageSO;
        PBChassisSO chassisSO = currentIAPProductSO.generalItems.Keys.First((item) => item is PBChassisSO) as PBChassisSO;
        TransformersManager.Instance.ShowTransformerPreview(garageSO, chassisSO);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStarted);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);

        if (state != UltimatePackState.None)
        {
            _isShowedInThisSection.value = true;
            _offerIsEverShow.value = true;
        }

        #region Design Event
        if (parrameters.Length > 0)
        {
            m_Operation = (string)parrameters[0];
            string popupName = "TransformPack";
            string status = $"Start";
            string operation = m_Operation;
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        }
        #endregion
    }

    private void Hide()
    {
        if (!_isShow)
            return;
        _isShow = false;
        _isUnpacking = false;
        _isUnpackingIAPProduct = false;
        _mainCanvas.HideImmediately();
        TransformersManager.Instance.HideTransformerPreview();
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStarted);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);

        #region Design Event
        if (m_Operation != "")
        {
            string popupName = "TransformPack";
            string status = $"Complete";
            string operation = m_Operation;
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        }
        #endregion
    }

    private void OnCloseBtnClicked()
    {
        _offerCumulatedWinMatchThisSection.value = 0;
        if (state == UltimatePackState.LastChanceDiscount)
        {
            _warningPopup.Show();
            return;
        }
        Hide();
        if (_autoShowCoroutine != null)
        {
            StopCoroutine(_autoShowCoroutine);
            _autoShowTime.GetReward();
            _autoShowCoroutine = StartCoroutine(TrackIsOfferAutoShow());
        }
    }

    private void OnLetsLookBtnClicked()
    {
        _warningPopup.Hide();
    }

    private void OnLoseItBtnClicked()
    {
        _warningPopup.Hide();
        Hide();
        ToNextOfferState();
    }

    private void OnPurchaseCompleted(params object[] objects)
    {
        if (objects[0] is LG_IAPButton lG_IAPButton && lG_IAPButton.IAPProductSO == _IAPButton.IAPProductSO)
        {
            if (_isUnpacking)
            {
                // Hide UI but don't trigger event, to prevent showing MainUI
                _mainCanvas.HideImmediately();
                _isUnpackingIAPProduct = true;
            }
            else
            {
                Hide();
            }
            InitOfferStatusTracking();

            _IAPButton.IAPProductSO.generalItems?.ForEach((v) =>
            {
                if (v.Key is GarageSO garageSO)
                    garageSO.Own();
            });
        }
    }

    private void OnUnpackStarted()
    {
        _isUnpacking = true;
    }

    private void OnUnpackDone()
    {
        if (_isUnpackingIAPProduct)
        {
            Hide();
        }
    }

    private string GetOfferRemainTime(TimeBasedRewardSO timeBasedRewardSO)
    {
        TimeSpan interval = DateTime.Now - timeBasedRewardSO.LastRewardTime;
        var remainingSeconds = Math.Max(0, timeBasedRewardSO.CoolDownInterval - interval.TotalSeconds);
        interval = TimeSpan.FromSeconds(remainingSeconds);
        if (interval.TotalHours < 1)
        {
            return string.Format("{0:00}M {1:00}S", interval.Minutes, interval.Seconds);
        }
        else
        {
            return string.Format("{0:00}H {1:00}M", interval.Hours + (interval.Days * 24f), interval.Minutes);
        }
    }

    private void ToNextOfferState()
    {
        switch (state)
        {
            case UltimatePackState.None:
                _offerState.value = (int)UltimatePackState.NormalActive;
                _offerEndTime.GetReward();
                break;
            case UltimatePackState.NormalActive:
                _offerState.value = (int)UltimatePackState.DiscountActive;
                _discountOfferEndTime.GetReward();
                break;
            case UltimatePackState.DiscountActive:
                _offerState.value = (int)UltimatePackState.LastChanceDiscount;
                break;
            case UltimatePackState.LastChanceDiscount:
                _offerState.value = (int)UltimatePackState.PermanentNormalActive;
                break;
            case UltimatePackState.PermanentNormalActive:
                break;
        }
        InitOfferStatusTracking();
    }

    private void InitOfferStatusTracking()
    {
        StopAllCoroutines();
        if (isPurchased)
        {
            _ultimatePackButton?.gameObject.SetActive(false);
            return;
        }
        switch (state)
        {
            case UltimatePackState.None:
                break;
            case UltimatePackState.NormalActive:
                StartCoroutine(UpdateOfferRemainTime(_offerEndTime, () =>
                {
                    StartCoroutine(WaitUntilOutToMainScreen(() =>
                    {
                        ToNextOfferState();
                        Show("Automatically");
                    }));
                }));
                _autoShowCoroutine = StartCoroutine(TrackIsOfferAutoShow());
                break;
            case UltimatePackState.DiscountActive:
                StartCoroutine(UpdateOfferRemainTime(_discountOfferEndTime, ToNextOfferState));
                _autoShowCoroutine = StartCoroutine(TrackIsOfferAutoShow());
                break;
            case UltimatePackState.LastChanceDiscount:
                _ultimatePackButton?.UpdateTime("00:00:00");
                _offerRemainTimeText.SetParameterValue("Time", "00:00:00");
                OnUpdateTime?.Invoke("00:00:00");

                StartCoroutine(WaitUntilOutToMainScreen(() =>
                {
                    Show("Automatically");
                }));
                break;
            case UltimatePackState.PermanentNormalActive:
                _autoShowCoroutine = StartCoroutine(TrackIsOfferAutoShow());
                break;
        }
        _IAPButton.OverrideSetup(currentIAPProductSO);
        _promotedViewController.EnablePromotedView(isDiscount);
        _offerRemainTimeBG.SetActive(isShowRemainTime);
        _ultimatePackButton?.gameObject.SetActive(state != UltimatePackState.None);
        _ultimatePackButton?.UpdateView(currentIAPProductSO, isDiscount, isShowRemainTime);
        //setup product
        int i = 0;
        currentIAPProductSO.generalItems?.ForEach((v) =>
        {
            ItemCell itemCell = _generalItem[i];
            if (v.Key is GarageSO garageSO)
            {
                if (itemCell.thumbnail != null)
                    itemCell.thumbnail.sprite = garageSO.Avatar;
                itemCell.nameTxt?.SetText(garageSO.NameGarage);
            }
            else if (v.Key is PBChassisSO transformChassisSO)
            {
                if (itemCell.thumbnail != null)
                    itemCell.thumbnail.sprite = transformChassisSO.GetThumbnailImage();
                if (itemCell.rarityOutline != null)
                    itemCell.rarityOutline.material = _rarityMaterials[transformChassisSO.GetRarityType()];
                itemCell.nameTxt?.SetText(transformChassisSO.GetDisplayName());
            }
            i++;
        });

        currentIAPProductSO.currencyItems?.ForEach((v) =>
        {
            if (_currencyItem.TryGetValue(v.Key, out ItemCell itemCell))
            {
                itemCell.amountTxt?.SetText(v.Value.value.ToRoundedText());
            }
        });

        OnUpdateView?.Invoke();
    }
}

public enum UltimatePackState
{
    None,
    NormalActive,
    DiscountActive,
    LastChanceDiscount,
    PermanentNormalActive
}