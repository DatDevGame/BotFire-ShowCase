using DG.Tweening;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ArenaOffersPopup : Singleton<ArenaOffersPopup>
{
    public Action OnUpdateView;
    public Action<string> OnUpdateTime;
    public Action<string> OnUpdateTimeForButton;

    public static int SHOW_TROPHY_THRESHOLD = 200;
    public static bool firstTimeShowAfterNewSession;
    public static ArenaOfferState staticState => (ArenaOfferState)PlayerPrefs.GetInt("ArenaOffer_arenaOfferState", 1);

    [SerializeField] protected float newSessionMin = 30;
    [SerializeField] private Button m_CloseBtn;
    [SerializeField] private CanvasGroupVisibility m_MainCanvasGroupVisibility;
    [SerializeField] private Transform pivot;
    [SerializeField] PPrefBoolVariable firstTimeShow;
    [SerializeField] HighestAchievedPPrefFloatTracker highestAchievedMedal;
    [SerializeField] CurrentHighestArenaVariable currentHighestArenaVariable;
    [SerializeField] SerializedDictionary<PBPvPArenaSO, DiscountableIAPProduct> discountableProducts;
    [SerializeField] TimeBasedRewardSO timeBasedRewardSO;
    [SerializeField] TimeBasedRewardSO discountedTimeBasedRewardSO;
    [SerializeField] ResetEveryArena resetEveryArena;
    [SerializeField, BoxGroup("WarningPopup")] CanvasGroupVisibility warningPopupVisibility;
    [SerializeField, BoxGroup("WarningPopup")] Button letLookBtn, loseItBtn;

    [ShowInInspector, ReadOnly]
    ArenaOfferState _arenaOfferState;
    DateTime pauseTimeStamp;
    ArenaOffersMainUIButton arenaOffersMainUIButton;
    Vector3 originalPivotPos;
    bool isHiding => m_MainCanvasGroupVisibility.TryGetComponent(out CanvasGroup canvasGroup) && canvasGroup.alpha == 0;
    private bool m_IsAutoShowPopup = false;
    private bool m_isShowing = false;
    SelectingModeUIFlow selectingModeUIFlow;
    Coroutine OnStartCoroutine;

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
    public ArenaOfferState state
    {
        get
        {
            if (_arenaOfferState == ArenaOfferState.UnloadData)
            {
                _arenaOfferState = (ArenaOfferState)PlayerPrefs.GetInt("ArenaOffer_arenaOfferState", 1);
            }
            return _arenaOfferState;
        }
        set
        {
            _arenaOfferState = value;
            PlayerPrefs.SetInt("ArenaOffer_arenaOfferState", (int)value);
            UpdateView();
        }
    }

    public void ConnectButton(ArenaOffersMainUIButton button)
    {
        arenaOffersMainUIButton = button;
        arenaOffersMainUIButton.button.onClick.AddListener(OnClickedOpenBtn);
    }

    private void Awake()
    {
        SelectingModeUIFlow.OnCheckFlowCompleted += OnCheckFlowCompleted;
        GameEventHandler.AddActionEvent(IAPPurchased.ArenaOffer, IAPPurchaseCompleted);

        originalPivotPos = pivot.position;
        m_CloseBtn.onClick.AddListener(OnClickedCloseBtn);
        letLookBtn.onClick.AddListener(OnClickedLetLookBtn);
        loseItBtn.onClick.AddListener(OnClickedLoseItBtn);
        var _IAPButton = GetComponentInChildren<LG_IAPButton>();
        if (_IAPButton != null)
        {
            _IAPButton.OnInteractiveChanged.AddListener(OnInteractiveChanged);
        }
        resetEveryArena.onReset += OnArenaReset;
    }

    private void OnDestroy()
    {
        SelectingModeUIFlow.OnCheckFlowCompleted -= OnCheckFlowCompleted;
        GameEventHandler.RemoveActionEvent(IAPPurchased.ArenaOffer, IAPPurchaseCompleted);
    }

    void OnCheckFlowCompleted(SelectingModeUIFlow selectingModeUIFlow)
    {
        this.selectingModeUIFlow = selectingModeUIFlow;
        WaitUntilOutToMainScreen(selectingModeUIFlow, () =>
        {
            ShowPopupAfterNewSession();
        });
    }

    private void IAPPurchaseCompleted()
    {
        #region Firebase Events
        if (currentProductSO != null)
        {
            string popUpName = $"{currentProductSO.productName}";
            string buttonClicked = "buy";
            string openType = m_IsAutoShowPopup ? "auto" : "clicked";
            LogFirebaseEvent(popUpName, buttonClicked, openType);
        }
        #endregion
    }

    void WaitUntilOutToMainScreen(SelectingModeUIFlow selectingModeUIFlow, Action callback)
    {
        if (OnStartCoroutine != null)
        {
            StopCoroutine(OnStartCoroutine);
        }
        OnStartCoroutine = StartCoroutine(CR_WaitUntilOutToMainScreen(selectingModeUIFlow, callback));
    }

    private IEnumerator CR_WaitUntilOutToMainScreen(SelectingModeUIFlow selectingModeUIFlow, Action callback)
    {
        yield return new WaitUntil(() => !selectingModeUIFlow.PlayModeUI.isShowingModeUI && !selectingModeUIFlow.BossModeUI.isShowing);
        callback?.Invoke();
    }

    void UpdateView()
    {
        OnUpdateView?.Invoke();
    }

    void Update()
    {
        if (state == ArenaOfferState.Lost || state == ArenaOfferState.HasPurchased)
        {
            return;
        }
        if (discountableProducts.ContainsKey(currentArenaSO) && discountableProducts[currentArenaSO].IsPurchased())
        {
            state = ArenaOfferState.HasPurchased;
            return;
        }
        if (state == ArenaOfferState.Hide)
        {
            if (selectingModeUIFlow != null && !firstTimeShow.value && discountableProducts.ContainsKey(currentArenaSO) && LoadingScreenUI.IS_LOADING_COMPLETE)
            {
                firstTimeShow.value = true;
                timeBasedRewardSO.GetReward();
                state = ArenaOfferState.ShowNormal;
                WaitUntilOutToMainScreen(selectingModeUIFlow, () =>
                {
                    Show(true);
                });
            }
        }
        else if (state == ArenaOfferState.ShowNormal)
        {
            var time = GetRemainingTime(timeBasedRewardSO);
            OnUpdateTime?.Invoke(time);
            OnUpdateTimeForButton?.Invoke(IsLessThanOneHour(timeBasedRewardSO) ? GetRemainingTimeInMinute(timeBasedRewardSO) : GetRemainingTimeInHour(timeBasedRewardSO));

            if (selectingModeUIFlow != null && timeBasedRewardSO.canGetReward)
            {
                discountedTimeBasedRewardSO.GetReward();
                state = ArenaOfferState.ShowDiscount;
                if (!m_MainCanvasGroupVisibility.gameObject.activeInHierarchy || (m_MainCanvasGroupVisibility.TryGetComponent(out CanvasGroup canvasGroup) && canvasGroup.alpha == 0))
                {
                    WaitUntilOutToMainScreen(selectingModeUIFlow, () =>
                    {
                        Show(true);
                    });
                }
            }
        }
        else if (state == ArenaOfferState.ShowDiscount)
        {
            var time = GetRemainingTime(discountedTimeBasedRewardSO);
            OnUpdateTime?.Invoke(time);
            OnUpdateTimeForButton?.Invoke(IsLessThanOneHour(discountedTimeBasedRewardSO) ? GetRemainingTimeInMinute(discountedTimeBasedRewardSO) : GetRemainingTimeInHour(discountedTimeBasedRewardSO));

            if (discountedTimeBasedRewardSO.canGetReward)
            {
                state = ArenaOfferState.ShowLastChance;
                if (!m_MainCanvasGroupVisibility.gameObject.activeInHierarchy || (m_MainCanvasGroupVisibility.TryGetComponent(out CanvasGroup canvasGroup) && canvasGroup.alpha == 0))
                {
                    WaitUntilOutToMainScreen(selectingModeUIFlow, () =>
                    {
                        Show(true);
                    });
                }
            }
        }
        else if (state == ArenaOfferState.ShowLastChance)
        {
            OnUpdateTime?.Invoke("00:00:00");
            OnUpdateTimeForButton?.Invoke("00M 00S");
        }
    }

    void OnArenaReset()
    {
        if (discountableProducts.ContainsKey((PBPvPArenaSO)currentHighestArenaVariable.value))
        {
            firstTimeShow.value = false;
            state = ArenaOfferState.Hide;
            firstTimeShowAfterNewSession = false;
        }
    }

    void OnInteractiveChanged(bool interactable)
    {
        if (!interactable)
        {
            m_MainCanvasGroupVisibility.Hide();
            Hide();
            if (discountableProducts.ContainsKey(currentArenaSO) && discountableProducts[currentArenaSO].IsPurchased())
            {
                state = ArenaOfferState.HasPurchased;
            }
        }
    }

    void OnClickedLetLookBtn()
    {
        warningPopupVisibility.Hide();
    }

    void OnClickedLoseItBtn()
    {
        warningPopupVisibility.Hide();
        Hide();
        state = ArenaOfferState.Lost;

        #region Design Events
        LogPopupGroup("ArenaOffer", m_IsAutoShowPopup ? "Automatically" : "Manually", "Complete");
        #endregion

        #region Firebase Events
        if (currentProductSO != null)
        {
            string popUpName = $"{currentProductSO.productName}";
            string buttonClicked = "close";
            string openType = m_IsAutoShowPopup ? "auto" : "clicked";
            LogFirebaseEvent(popUpName, buttonClicked, openType);
        }
        #endregion
    }

    void OnClickedOpenBtn()
    {
        Show();
    }

    void OnClickedCloseBtn()
    {
        if (state == ArenaOfferState.ShowLastChance)
        {
            warningPopupVisibility.Show();
        }
        else
        {
            Hide();

            #region Design Events
            LogPopupGroup("ArenaOffer", m_IsAutoShowPopup ? "Automatically" : "Manually", "Complete");
            #endregion

            #region Firebase Events
            string popUpName = $"{currentProductSO.productName}";
            string buttonClicked = "close";
            string openType = m_IsAutoShowPopup ? "auto" : "clicked";
            LogFirebaseEvent(popUpName, buttonClicked, openType);
            #endregion
        }
    }

    #region Design Event
    private void LogPopupGroup(string popupName, string operation, string status)
    {
        GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
    }
    #endregion

    #region Firebase Events
    private void LogFirebaseEvent(string popUpName, string buttonClicked, string openType)
    {
        if (currentProductSO != null)
        {
            GameEventHandler.Invoke(LogFirebaseEventCode.PopupAction, popUpName, buttonClicked, openType);
        }
    }
    #endregion

    void Hide()
    {
        m_MainCanvasGroupVisibility.Hide();
        pivot.transform.DOKill();
        pivot.transform.DOScale(0, AnimationDuration.TINY).SetEase(Ease.OutSine);
        pivot.transform.DOMove(arenaOffersMainUIButton.transform.position, AnimationDuration.TINY).SetEase(Ease.OutSine);

        m_isShowing = false;
    }

    void Show(bool isAutoShow = false)
    {
        if (discountableProducts.ContainsKey(currentArenaSO) && discountableProducts[currentArenaSO].IsPurchased())
        {
            return;
        }
        firstTimeShowAfterNewSession = true;
        m_IsAutoShowPopup = isAutoShow;
        //TODO: Hide IAP & Popup
        m_MainCanvasGroupVisibility.Hide();
        //m_MainCanvasGroupVisibility.Show();
        pivot.transform.DOKill();
        pivot.transform.DOScale(1, AnimationDuration.TINY).SetEase(Ease.InSine);
        pivot.transform.DOMove(originalPivotPos, AnimationDuration.TINY).SetEase(Ease.InSine);

        #region Design Events
        if (!m_isShowing)
        {
            LogPopupGroup("ArenaOffer", m_IsAutoShowPopup ? "Automatically" : "Manually", "Start");
            m_isShowing = true;
        }
        #endregion
    }

    private string GetRemainingTime(TimeBasedRewardSO timeBasedRewardSO)
    {
        TimeSpan interval = DateTime.Now - timeBasedRewardSO.LastRewardTime;
        var remainingSeconds = timeBasedRewardSO.CoolDownInterval - interval.TotalSeconds;
        interval = TimeSpan.FromSeconds(remainingSeconds);
        return string.Format("{0:00}:{1:00}:{2:00}", interval.Hours + (interval.Days * 24f), interval.Minutes, interval.Seconds);
    }
    private string GetRemainingTimeInHour(TimeBasedRewardSO timeBasedRewardSO)
    {
        TimeSpan interval = DateTime.Now - timeBasedRewardSO.LastRewardTime;
        var remainingSeconds = timeBasedRewardSO.CoolDownInterval - interval.TotalSeconds;
        interval = TimeSpan.FromSeconds(remainingSeconds);
        return string.Format("{0:00}H {1:00}M", interval.Hours + (interval.Days * 24f), interval.Minutes);
    }
    public virtual string GetRemainingTimeInMinute(TimeBasedRewardSO timeBasedRewardSO)
    {
        TimeSpan interval = DateTime.Now - timeBasedRewardSO.LastRewardTime;
        var remainingSeconds = timeBasedRewardSO.CoolDownInterval - interval.TotalSeconds;
        interval = TimeSpan.FromSeconds(remainingSeconds);
        return string.Format("{0:00}M {1:00}S", interval.Minutes, interval.Seconds);
    }

    private bool IsLessThanOneHour(TimeBasedRewardSO timeBasedRewardSO)
    {
        TimeSpan interval = DateTime.Now - timeBasedRewardSO.LastRewardTime;
        var remainingSeconds = timeBasedRewardSO.CoolDownInterval - interval.TotalSeconds;
        interval = TimeSpan.FromSeconds(remainingSeconds);
        return interval.Hours <= 0;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus == true)
        {
            pauseTimeStamp = DateTime.Now;
        }
        else
        {
            if ((DateTime.Now - pauseTimeStamp).Minutes >= newSessionMin)
            {
                firstTimeShowAfterNewSession = false;
                ShowPopupAfterNewSession();
            }
        }
    }

    public void ShowPopupAfterNewSession()
    {
        if (state == ArenaOfferState.ShowNormal || state == ArenaOfferState.ShowLastChance || state == ArenaOfferState.ShowDiscount)
        {
            if (!firstTimeShowAfterNewSession)
            {
                Show(true);
            }
        }
    }

    [Button]
    void ResetData()
    {
        PlayerPrefs.DeleteKey("ArenaOffer_arenaOfferState");
        firstTimeShow.ResetValue();
        timeBasedRewardSO.ResetTime();
        discountedTimeBasedRewardSO.ResetTime();
    }
}

public enum ArenaOfferState
{
    UnloadData,
    Hide,
    ShowNormal,
    ShowDiscount,
    ShowLastChance,
    Lost,
    HasPurchased
}
