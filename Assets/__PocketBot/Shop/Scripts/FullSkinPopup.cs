using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.Monetization;
using UnityEngine;
using UnityEngine.UI;

public class FullSkinPopup : MonoBehaviour
{
    public static int SHOW_TROPHY_THRESHOLD = 50;
    static bool justWatchedSkinAds;

    [SerializeField] private Button m_CloseBtn;
    [SerializeField] private CanvasGroupVisibility m_MainCanvasGroupVisibility;
    [SerializeField] PPrefBoolVariable m_FTUEEquip_2;
    [SerializeField] PPrefBoolVariable firstTimeShow;
    // [SerializeField] PPrefBoolVariable justWatchedSkinAds;
    [SerializeField] PPrefIntVariable currentMilestoneTrophy;
    [SerializeField] TimeBasedRewardSO cooldownSO;
    [SerializeField] HighestAchievedPPrefFloatTracker highestAchievedMedal;
    [SerializeField] List<int> showingMilestoneTrophies;
    [SerializeField] int showingAfterEveryTrophy = 150;

    private LG_IAPButton m_LG_IAPButton;
    private bool m_IsAutoShow = false;
    Coroutine CheckShowConditionCoroutine;
    SelectingModeUIFlow selectingModeUIFlow;

    private void Awake()
    {
        SelectingModeUIFlow.OnCheckFlowCompleted += OnCheckFlowCompleted;
        GameEventHandler.AddActionEvent(IAPPurchased.AllSkinOffers, IAPPurchaseCompleted);
        GameEventHandler.AddActionEvent(AdvertisingEventCode.OnCloseAd, OnWatchSkinRV);

        m_CloseBtn.onClick.AddListener(OnClose);
        var _IAPButton = GetComponentInChildren<LG_IAPButton>(true);
        if (_IAPButton != null)
        {
            m_LG_IAPButton = _IAPButton;
            _IAPButton.OnInteractiveChanged.AddListener(OnInteractiveChanged);
        }
    }

    void OnInteractiveChanged(bool interactable)
    {
        if (!interactable)
        {
            Hide();
        }
    }

    private void OnDestroy()
    {
        SelectingModeUIFlow.OnCheckFlowCompleted -= OnCheckFlowCompleted;
        GameEventHandler.RemoveActionEvent(IAPPurchased.AllSkinOffers, IAPPurchaseCompleted);
        GameEventHandler.RemoveActionEvent(AdvertisingEventCode.OnCloseAd, OnWatchSkinRV);
        m_CloseBtn.onClick.RemoveListener(OnClose);
    }

    void OnWatchSkinRV(object[] _params)
    {
        var adsType = (AdsType)_params[0];
        var location = (AdsLocation)_params[1];
        var isSuccess = (bool)_params[2];
        if (adsType == AdsType.Rewarded && location == AdsLocation.RV_Getskin && isSuccess)
        {
            justWatchedSkinAds = true;
            if (CheckShowConditionCoroutine != null)
            {
                StopCoroutine(CheckShowConditionCoroutine);
            }
            CheckShowConditionCoroutine = StartCoroutine(CR_CheckShowCondition(selectingModeUIFlow));
        }
    }

    void OnCheckFlowCompleted(SelectingModeUIFlow selectingModeUIFlow)
    {
        this.selectingModeUIFlow = selectingModeUIFlow;
        if (CheckShowConditionCoroutine != null)
        {
            StopCoroutine(CheckShowConditionCoroutine);
        }
        CheckShowConditionCoroutine = StartCoroutine(CR_CheckShowCondition(selectingModeUIFlow));
    }

    private IEnumerator CR_CheckShowCondition(SelectingModeUIFlow selectingModeUIFlow)
    {
        if (m_LG_IAPButton.IAPProductSO.IsPurchased)
        {
            yield break;
        }
        yield return new WaitUntil(() => !selectingModeUIFlow.PlayModeUI.isShowingModeUI && !selectingModeUIFlow.BossModeUI.isShowing);
        if (!firstTimeShow.value)
        {
            var dockController = FindObjectOfType<PBDockController>();
            yield return new WaitUntil(() => dockController.CurrentSelectedButtonType == ButtonType.Main && LoadingScreenUI.IS_LOADING_COMPLETE && m_FTUEEquip_2.value);
            var meetConditionMilestones = showingMilestoneTrophies.FindAll(x => x <= highestAchievedMedal.value && x > currentMilestoneTrophy.value);
            bool isReachASpecialMilestone = meetConditionMilestones.Count > 0;
            if (isReachASpecialMilestone)
            {
                Show(true);
                currentMilestoneTrophy.value = (int)highestAchievedMedal.value;
                if (currentMilestoneTrophy.value >= showingMilestoneTrophies.Max())
                {
                    firstTimeShow.value = true;
                    cooldownSO.GetReward();
                }
            }
        }
        else
        {
            var dockController = FindObjectOfType<PBDockController>();
            yield return new WaitUntil(() => dockController.CurrentSelectedButtonType == ButtonType.Main &&
                LoadingScreenUI.IS_LOADING_COMPLETE &&
                ((cooldownSO.canGetReward && justWatchedSkinAds) || (highestAchievedMedal.value >= currentMilestoneTrophy.value + showingAfterEveryTrophy))
            );
            Show(true);
            currentMilestoneTrophy.value = (int)highestAchievedMedal.value;
            justWatchedSkinAds = false;
            cooldownSO.GetReward();
        }
    }

    private void IAPPurchaseCompleted(params object[] parrameter)
    {
        if (parrameter.Length <= 0 || parrameter[0] == null) return;
        IAPProductSO iAPProductSO = parrameter[0] as IAPProductSO;
        if (iAPProductSO != null)
        {
            #region Firebase Events
            string popUpName = m_LG_IAPButton.IAPProductSO.productName;
            string buttonClicked = "buy";
            string openType = m_IsAutoShow ? "auto" : "clicked";
            GameEventHandler.Invoke(LogFirebaseEventCode.PopupAction, popUpName, buttonClicked, openType);
            #endregion
        }
    }

    private void OnClose()
    {
        Hide();

        #region Design Events
        string popupName = "AllSkinsOffer";
        string operation = m_IsAutoShow ? "Automatically" : "Manually";
        string status = "Complete";
        GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        #endregion

        #region Firebase Events
        if (m_LG_IAPButton != null)
        {
            string popUpName = m_LG_IAPButton.IAPProductSO.productName;
            string buttonClicked = "close";
            string openType = m_IsAutoShow ? "auto" : "clicked";
            GameEventHandler.Invoke(LogFirebaseEventCode.PopupAction, popUpName, buttonClicked, openType);
        }
        #endregion

    }

    void Hide()
    {
        m_MainCanvasGroupVisibility.Hide();
    }

    void Show(bool isAutoShow = false)
    {
        if (m_LG_IAPButton.IAPProductSO.IsPurchased)
        {
            return;
        }
        m_IsAutoShow = isAutoShow;
        //TODO: Hide IAP & Popup
        m_MainCanvasGroupVisibility.Hide();
        //m_MainCanvasGroupVisibility.Show();

        #region Design Events
        string popupName = "AllSkinsOffer";
        string operation = m_IsAutoShow ? "Automatically" : "Manually";
        string status = "Start";
        GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        #endregion
    }
}
