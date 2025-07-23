using DG.Tweening;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames;
using LatteGames.Monetization;
using LatteGames.PvP.TrophyRoad;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StarterPack : Singleton<StarterPack>
{
    public Action OnUpdateView;
    public Action<string> OnUpdateTime;
    public Action<string> OnUpdateTimeForButton;

    public static int SHOW_TROPHY_THRESHOLD = 50;
    public static bool firstTimeShowAfterNewSession;
    public static StarterPackState staticState => (StarterPackState)PlayerPrefs.GetInt("StarterPack_starterPackState", 1);

    [SerializeField, BoxGroup("Property")] protected float newSessionMin = 30;
    [SerializeField, BoxGroup("Ref")] protected CanvasGroupVisibility mainPackCanvasGroup;
    [SerializeField, BoxGroup("Ref")] protected CanvasGroupVisibility starterPackDarkLayer;
    [SerializeField, BoxGroup("Ref")] protected LocalizationParamsManager timeLimitedLastChance;
    [SerializeField, BoxGroup("Ref")] protected GameObject timeBox;
    [SerializeField, BoxGroup("Ref")] protected GameObject lastChanceTxt;
    [SerializeField, BoxGroup("Ref")] protected Button closeBtn;
    [SerializeField, BoxGroup("Ref")] protected RectTransform infoPanelRect;
    [SerializeField, BoxGroup("Ref")] protected GameObject infoPanel;
    [SerializeField, BoxGroup("Ref")] protected LG_StarterPack purchaseBtn;
    [SerializeField, BoxGroup("Ref")] protected HighestAchievedPPrefFloatTracker highestAchievedMedal;
    [SerializeField, BoxGroup("Ref")] protected PromotedViewController promotedViewController;
    [SerializeField, BoxGroup("Ref")] protected LocalizationParamsManager discountTxt;
    [SerializeField, BoxGroup("Warning Popup")] protected CanvasGroupVisibility warningPopup;
    [SerializeField, BoxGroup("Warning Popup")] protected Button letsLookBtn;
    [SerializeField, BoxGroup("Warning Popup")] protected Button loseItBtn;
    [SerializeField, BoxGroup("Data")] protected PPrefIntVariable matchConditionStarterPack;
    [SerializeField, BoxGroup("Data")] protected PPrefIntVariable queuingShowStarterPack;
    [SerializeField, BoxGroup("Data")] protected PPrefBoolVariable showStarterPackTheFirstTimeCondition;
    [SerializeField, BoxGroup("Data")] protected TimeBasedRewardSO timeBasedRewardSO;
    [SerializeField, BoxGroup("Data")] protected TimeBasedRewardSO discountedTimeBasedRewardSO;
    [SerializeField, BoxGroup("Data")] protected TimeBasedRewardSO timeAppearAgainStartPack;
    [SerializeField, BoxGroup("Data")] protected OperationStarterPackVariable operationStarterPackVariable;
    [SerializeField, BoxGroup("Data")] public IAPProductSO starterPackSO;
    [SerializeField, BoxGroup("Data")] public IAPProductSO discountedStarterPackSO;
    [SerializeField, BoxGroup("Data")] public DiscountableIAPProduct discountableIAPProduct;

    [ShowInInspector, ReadOnly]
    StarterPackState _starterPackState;
    Vector3 originalInfoPanelPos;
    DateTime pauseTimeStamp;
    private bool canClose = false;
    bool isPurchased => discountableIAPProduct.IsPurchased();
    Button starterPackBtn;

    public IAPProductSO currentProductSO => starterPackState == StarterPackState.ShowDiscount || starterPackState == StarterPackState.ShowLastChance ? discountableIAPProduct.discountProduct : discountableIAPProduct.normalProduct;
    public bool isEnoughTrophy => highestAchievedMedal.value >= SHOW_TROPHY_THRESHOLD;
    public bool hasShowedFirstTime
    {
        get
        {
            return PlayerPrefs.GetInt("StarterPack_TheFirstTimeStartPack", 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("StarterPack_TheFirstTimeStartPack", value ? 1 : 0);
        }
    }

    public StarterPackState starterPackState
    {
        get
        {
            if (isPurchased)
            {
                PlayerPrefs.SetInt("StarterPack_starterPackState", (int)StarterPackState.Lost);
                return StarterPackState.Lost;
            }
            if (_starterPackState == StarterPackState.UnloadData)
            {
                _starterPackState = (StarterPackState)PlayerPrefs.GetInt("StarterPack_starterPackState", 1);
            }
            return _starterPackState;
        }
        set
        {
            _starterPackState = value;
            PlayerPrefs.SetInt("StarterPack_starterPackState", (int)value);
            UpdateView();
        }
    }

    public LG_StarterPack PurchaseBtn => purchaseBtn;

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

    protected override void Awake()
    {
        base.Awake();
        originalInfoPanelPos = infoPanel.transform.position;

        #region Subcribe Event
        SelectingModeUIFlow.OnCheckFlowCompleted += OnCheckFlowCompleted;
        GameEventHandler.AddActionEvent(FTUEEventCode.OnClickBoxTheFisrtTime, OnClickOpenBoxTheFirstTime);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnClickOpenBoxTheFirstTime, OnClickOpenBoxTheFirstTime);
        GameEventHandler.AddActionEvent(StateBlockBackGroundFTUE.Start, StateBlockFTUE_Start);
        GameEventHandler.AddActionEvent(IAPPurchased.StarterPack, IAPPurchaseCompleted);
        #endregion

        //TODO: V1.8.1 Removed 
        // timeAppearAgainStartPack.GetReward();
        purchaseBtn.OnInteractiveChanged.AddListener(OnProductBtnInteractiveChanged);
        closeBtn.onClick.AddListener(OnCloseStarterPack);
        letsLookBtn.onClick.AddListener(OnLetsLook);
        loseItBtn.onClick.AddListener(OnLoseIt);
    }

    private void OnDestroy()
    {
        #region Remove Event
        SelectingModeUIFlow.OnCheckFlowCompleted -= OnCheckFlowCompleted;
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnClickBoxTheFisrtTime, OnClickOpenBoxTheFirstTime);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnClickOpenBoxTheFirstTime, OnClickOpenBoxTheFirstTime);
        GameEventHandler.RemoveActionEvent(StateBlockBackGroundFTUE.Start, StateBlockFTUE_Start);
        GameEventHandler.RemoveActionEvent(IAPPurchased.StarterPack, IAPPurchaseCompleted);
        #endregion

        purchaseBtn.OnInteractiveChanged.RemoveListener(OnProductBtnInteractiveChanged);
        closeBtn.onClick.RemoveAllListeners();
        letsLookBtn.onClick.RemoveListener(OnLetsLook);
        loseItBtn.onClick.RemoveListener(OnLoseIt);

        infoPanel.transform.DOKill();
        infoPanelRect.DOKill();
    }

    private void Update()
    {
        // bool isAppearAginStartPackCoolDown = !appearAgainStarterPackCondition.value;
        // starterPackBtn.gameObject.SetActive(isEnoughTrophy && isAppearAginStartPackCoolDown && !isPurchased);
        if (starterPackState == StarterPackState.ShowNormal)
        {
            timeLimitedLastChance.SetParameterValue("Time", GetRemainingTime(timeBasedRewardSO));
            OnUpdateTime?.Invoke(GetRemainingTime(timeBasedRewardSO));
            OnUpdateTimeForButton?.Invoke(IsLessThanOneHour(timeBasedRewardSO) ? GetRemainingTimeInMinute(timeBasedRewardSO) : GetRemainingTimeInHour(timeBasedRewardSO));
            if (timeBasedRewardSO.canGetReward)
            {
                discountedTimeBasedRewardSO.GetReward();
                starterPackState = StarterPackState.ShowDiscount;
                if (!mainPackCanvasGroup.gameObject.activeInHierarchy || (mainPackCanvasGroup.TryGetComponent(out CanvasGroup canvasGroup) && canvasGroup.alpha == 0))
                {
                    OnClickStarterPack(OperationStarterPack.Automatically);
                }
            }
        }
        else if (starterPackState == StarterPackState.ShowDiscount)
        {
            timeLimitedLastChance.SetParameterValue("Time", GetRemainingTime(discountedTimeBasedRewardSO));
            OnUpdateTime?.Invoke(GetRemainingTime(discountedTimeBasedRewardSO));
            OnUpdateTimeForButton?.Invoke(IsLessThanOneHour(discountedTimeBasedRewardSO) ? GetRemainingTimeInMinute(discountedTimeBasedRewardSO) : GetRemainingTimeInHour(discountedTimeBasedRewardSO));
            if (discountedTimeBasedRewardSO.canGetReward)
            {
                starterPackState = StarterPackState.ShowLastChance;
                if (!mainPackCanvasGroup.gameObject.activeInHierarchy || (mainPackCanvasGroup.TryGetComponent(out CanvasGroup canvasGroup) && canvasGroup.alpha == 0))
                {
                    OnClickStarterPack(OperationStarterPack.Automatically);
                }
            }
        }
        else if (starterPackState == StarterPackState.ShowLastChance)
        {
            timeLimitedLastChance.SetParameterValue("Time", "00:00:00");
            OnUpdateTime?.Invoke("00:00:00");
            OnUpdateTimeForButton?.Invoke("00M 00S");
        }
    }

    private void UpdateView()
    {
        if (starterPackState == StarterPackState.Lost)
        {
            mainPackCanvasGroup.HideImmediately();
            OnUpdateView?.Invoke();
            return;
        }
        discountTxt.SetParameterValue("Multiplier", starterPackState == StarterPackState.ShowNormal ? "20" : "40");
        purchaseBtn.OverrideSetup(currentProductSO);
        promotedViewController.EnablePromotedView(starterPackState == StarterPackState.ShowDiscount || starterPackState == StarterPackState.ShowLastChance);
        // timeBox.SetActive(starterPackState == StarterPackState.ShowDiscount || starterPackState == StarterPackState.ShowLastChance);
        lastChanceTxt.SetActive(starterPackState == StarterPackState.ShowLastChance);
        OnUpdateView?.Invoke();
    }

    void OnCheckFlowCompleted(SelectingModeUIFlow selectingModeUIFlow)
    {
        if (starterPackState == StarterPackState.Hide && !hasShowedFirstTime)
        {
            timeBasedRewardSO.GetReward();
            hasShowedFirstTime = true;
            starterPackState = StarterPackState.ShowNormal;
        }
        if (starterPackState == StarterPackState.Hide || starterPackState == StarterPackState.Lost)
        {
            mainPackCanvasGroup.HideImmediately();
        }
        else
        {
            StartCoroutine(OnStarterPack(selectingModeUIFlow));
        }
        UpdateView();
    }

    private IEnumerator OnStarterPack(SelectingModeUIFlow selectingModeUIFlow)
    {
        yield return new WaitUntil(() => !selectingModeUIFlow.PlayModeUI.isShowingModeUI && !selectingModeUIFlow.BossModeUI.isShowing);

        var timeBasedRewardSO = starterPackState == StarterPackState.ShowNormal ? this.timeBasedRewardSO : this.discountedTimeBasedRewardSO;
        if (!isPurchased && !timeBasedRewardSO.canGetReward && isEnoughTrophy && !firstTimeShowAfterNewSession)
        {
            OnClickStarterPack(OperationStarterPack.Automatically);
        }
    }

    private void OnProductBtnInteractiveChanged(bool isInteractiable)
    {
        if (!isInteractiable)
        {
            starterPackDarkLayer.HideImmediately();
            mainPackCanvasGroup.HideImmediately();
            GameEventHandler.Invoke(StarterPackEventCode.PopupComplete, operationStarterPackVariable.value);
            starterPackState = StarterPackState.Lost;
        }
    }

    public void ConnectButton(OpenStarterPackBtn openStarterPackBtn)
    {
        starterPackBtn = openStarterPackBtn.button;
        starterPackBtn.onClick.AddListener(() => { OnClickStarterPack(OperationStarterPack.Manually); });
    }

    private void OnClickStarterPack(OperationStarterPack operation)
    {
        if (infoPanel == null || !isEnoughTrophy || isPurchased) return;
        firstTimeShowAfterNewSession = true;
        showStarterPackTheFirstTimeCondition.value = true;
        canClose = true;
        operationStarterPackVariable.value = operation;

        //TODO: Hide IAP & Popup
        //mainPackCanvasGroup.gameObject.SetActive(true);
        //mainPackCanvasGroup.ShowImmediately();
        mainPackCanvasGroup.gameObject.SetActive(false);
        mainPackCanvasGroup.HideImmediately();

        infoPanel.transform.DOKill();
        infoPanel.transform.localScale = Vector3.zero;
        // infoPanel.transform.position = starterPackBtn.transform.position;
        infoPanel.gameObject.SetActive(true);
        infoPanel.transform.DOScale(1, AnimationDuration.TINY).SetEase(Ease.InSine);
        infoPanel.transform.DOMove(originalInfoPanelPos, AnimationDuration.TINY).SetEase(Ease.InSine);
        starterPackDarkLayer.Show();
        GameEventHandler.Invoke(StarterPackEventCode.PopupStart, operationStarterPackVariable.value);

        #region Design Events
        string popupName = "StarterPack";
        string operationLog = operationStarterPackVariable.value == OperationStarterPack.Automatically ? "Automatically" : "Manually";
        string status = "Start";
        GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operationLog, status);
        #endregion
    }

    private void OnCloseStarterPack()
    {
        if (!canClose) return;
        canClose = false;

        if (infoPanel == null || infoPanelRect == null) return;

        if (starterPackState == StarterPackState.ShowLastChance)
        {
            warningPopup.Show();
            starterPackDarkLayer.Hide();
            return;
        }
        infoPanel.transform.DOKill();
        starterPackDarkLayer.Hide();
        infoPanel.transform
            .DOScale(0, AnimationDuration.TINY)
            .SetEase(Ease.OutSine)
            .OnComplete(() =>
            {
                infoPanel.gameObject.SetActive(false);
                infoPanelRect.anchoredPosition = Vector3.zero;
                mainPackCanvasGroup.Hide();
            });
        infoPanel.transform.DOMove(starterPackBtn.transform.position, AnimationDuration.TINY).SetEase(Ease.OutSine);
        GameEventHandler.Invoke(StarterPackEventCode.PopupComplete, operationStarterPackVariable.value);

        #region Design Events
        string popupName = "StarterPack";
        string operation = operationStarterPackVariable.value == OperationStarterPack.Automatically ? "Automatically" : "Manually";
        string status = "Complete";
        GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        #endregion

        #region Firebase Events
        string popUpName = "Starter Pack";
        string buttonClicked = "close";
        string openType = operationStarterPackVariable.value == OperationStarterPack.Manually ? "clicked" : "auto";
        GameEventHandler.Invoke(LogFirebaseEventCode.PopupAction, popUpName, buttonClicked, openType);
        #endregion
    }

    private void OnLetsLook()
    {
        starterPackDarkLayer.Show();
        warningPopup.Hide();
        canClose = true;
    }

    private void OnLoseIt()
    {
        GameEventHandler.Invoke(StarterPackEventCode.PopupComplete, operationStarterPackVariable.value);
        infoPanel.SetActive(false);
        starterPackDarkLayer.Hide();
        warningPopup.Hide();
        mainPackCanvasGroup.Hide();
        canClose = true;

        starterPackState = StarterPackState.Lost;
        //24 hour
        // appearAgainStarterPackCondition.value = true;
        // timeAppearAgainStartPack.GetReward();

        #region Design Events
        string popupName = "StarterPack";
        string operation = operationStarterPackVariable.value == OperationStarterPack.Automatically ? "Automatically" : "Manually";
        string status = "Complete";
        GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        #endregion

        #region Firebase Events
        string popUpName = "Starter Pack";
        string buttonClicked = "close";
        string openType = operationStarterPackVariable.value == OperationStarterPack.Manually ? "clicked" : "auto";
        GameEventHandler.Invoke(LogFirebaseEventCode.PopupAction, popUpName, buttonClicked, openType);
        #endregion
    }

    public void ShowPopupAfterNewSession()
    {
        if (!timeBasedRewardSO.canGetReward && isEnoughTrophy && !firstTimeShowAfterNewSession)
        {
            OnClickStarterPack(OperationStarterPack.Automatically);
        }
    }

    // private void HandleOnEnablePlayModePopup() => starterpackButtonCanvasGroup.Hide();
    // private void HandleOnDisablePlayModePopup() => starterpackButtonCanvasGroup.Show();

    private void OnClickOpenBoxTheFirstTime()
    {
        mainPackCanvasGroup.HideImmediately();
    }

    private void StateBlockFTUE_Start()
    {
        if (mainPackCanvasGroup.GetComponent<CanvasGroup>().alpha > 0)
        {
            GameEventHandler.AddActionEvent(StateBlockBackGroundFTUE.End, StateBlockFTUE_End);
            mainPackCanvasGroup.Hide();
        }
    }

    private void StateBlockFTUE_End()
    {
        GameEventHandler.RemoveActionEvent(StateBlockBackGroundFTUE.End, StateBlockFTUE_End);
        //TODO: Hide IAP & Popup
        //mainPackCanvasGroup.Show();
        mainPackCanvasGroup.Hide();
    }

    private void IAPPurchaseCompleted()
    {
        if (infoPanel.activeSelf)
        {
            #region Design Events
            string popupName = "StarterPack";
            string operation = operationStarterPackVariable.value == OperationStarterPack.Automatically ? "Automatically" : "Manually";
            string status = "Complete";
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
            #endregion

            #region Firebase Events
            string popUpName = "Starter Pack";
            string buttonClicked = "buy";
            string openType = operationStarterPackVariable.value == OperationStarterPack.Manually ? "clicked" : "auto";
            GameEventHandler.Invoke(LogFirebaseEventCode.PopupAction, popUpName, buttonClicked, openType);
            #endregion
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (starterPackState == StarterPackState.Hide || starterPackState == StarterPackState.Lost)
        {
            return;
        }
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

    [Button]
    void ResetData()
    {
        PlayerPrefs.DeleteKey("StarterPack_starterPackState");
        PlayerPrefs.DeleteKey("StarterPack_TheFirstTimeStartPack");
        timeBasedRewardSO.ResetTime();
        discountedTimeBasedRewardSO.ResetTime();
    }
}

public enum OperationStarterPack
{
    Manually,
    Automatically
}

public enum StarterPackState
{
    UnloadData,
    Hide,
    ShowNormal,
    ShowDiscount,
    ShowLastChance,
    Lost
}
