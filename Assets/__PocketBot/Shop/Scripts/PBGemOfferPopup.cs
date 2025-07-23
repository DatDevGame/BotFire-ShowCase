using System;
using System.Collections;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PBGemOfferPopup : Singleton<PBGemOfferPopup>
{
    private const int SHOW_TROPHY_THRESHOLD = 400;

    [SerializeField, BoxGroup("UI Ref")] private LG_IAPButton _IAPButton;
    [SerializeField, BoxGroup("UI Ref")] private CanvasGroupVisibility _mainCanvas;
    [SerializeField, BoxGroup("UI Ref")] private Button _closeButton;
    [SerializeField, BoxGroup("UI Ref")] private TextMeshProUGUI _offerRemainTimeText;
    [SerializeField, BoxGroup("UI Ref")] private Transform _pivot;
    [SerializeField, BoxGroup("Data")] private TimeBasedRewardSO _autoShowTime;
    [SerializeField, BoxGroup("Data")] private TimeBasedRewardSO _offerEndTime;
    [SerializeField, BoxGroup("Data")] private TimeBasedRewardSO _offerCooldownTime;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable _isOfferActive;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable _isOfferEverActive;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable _isOfferWantToActive;
    [SerializeField, BoxGroup("Data")] private FloatVariable _highestAchivedMedal;

    private SelectingModeUIFlow _selectingModeUIFlow = null;
    private bool _isShow = false;
    private string _RemainTimeStr = string.Empty;
    private Coroutine _autoShowCoroutine;
    private PBGemOfferButton _gemOfferButton = null;
    private Vector3 _pivotOriginal;

    #region Event
    private string m_Operation;
    #endregion

    public void WantToActiveOffer()
    {
        if (_isOfferEverActive && _highestAchivedMedal.value >= SHOW_TROPHY_THRESHOLD)
        {
            _isOfferWantToActive.value = true;
        }
    }

    public void ConnectButton(PBGemOfferButton btn)
    {
        _gemOfferButton = btn;
        _gemOfferButton.UpdateView(_IAPButton.IAPProductSO);
        if (_isOfferActive.value)
        {
            _gemOfferButton.Show();
        }
        else
        {
            _gemOfferButton.Hide();
        }
    }

    private void Awake()
    {
        _pivotOriginal = _pivot.position;
        SelectingModeUIFlow.OnCheckFlowCompleted += OnCheckFlowCompleted;
        _closeButton.onClick.AddListener(OnCloseBtnClicked);
        InitOfferStatusTracking();
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
    }

    private void OnDestroy()
    {
        SelectingModeUIFlow.OnCheckFlowCompleted -= OnCheckFlowCompleted;
        _closeButton.onClick.RemoveListener(OnCloseBtnClicked);
        GameEventHandler.RemoveActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseCompleted);
    }

    void OnCheckFlowCompleted(SelectingModeUIFlow selectingModeUIFlow)
    {
        _selectingModeUIFlow = selectingModeUIFlow;
    }

    private void InitOfferStatusTracking()
    {
        StopAllCoroutines();
        _autoShowCoroutine = null;
        if (_isOfferActive.value)
        {
            StartCoroutine(TrackIsOfferExpired());
            StartCoroutine(UpdateOfferRemainTime());
            _autoShowCoroutine = StartCoroutine(TrackIsOfferAutoShow());
        }
        else
        {
            StartCoroutine(TrackIsOfferCanEnable());
        }
    }

    private IEnumerator WaitUntilOutToMainScreen(Action callback)
    {
        yield return new WaitUntil(() => _selectingModeUIFlow != null && !_selectingModeUIFlow.PlayModeUI.isShowingModeUI && !_selectingModeUIFlow.BossModeUI.isShowing);
        var dockController = FindObjectOfType<PBDockController>();
        yield return new WaitUntil(() => dockController.CurrentSelectedButtonType == ButtonType.Main && LoadingScreenUI.IS_LOADING_COMPLETE);
        callback?.Invoke();
    }

    private IEnumerator UpdateOfferRemainTime()
    {
        while(true)
        {
            _RemainTimeStr = GetOfferRemainTime();
            _gemOfferButton?.UpdateTime(_RemainTimeStr);
            if (_isShow)
            {
                _offerRemainTimeText.SetText(_RemainTimeStr);
            }
            yield return null;
        }
    }

    private IEnumerator TrackIsOfferExpired()
    {
        yield return new WaitUntil(() =>{
            return _offerEndTime.canGetReward && _gemOfferButton != null;
        });
        DisableOffer();
    }

    private IEnumerator TrackIsOfferAutoShow()
    {
        yield return new WaitUntil(() =>_autoShowTime.canGetReward);
        StartCoroutine(WaitUntilOutToMainScreen(()=>{
            Show("Automatically");
        }));
    }

    private IEnumerator TrackIsOfferCanEnable()
    {
        yield return new WaitUntil(() =>{
            return _offerCooldownTime.canGetReward &&
                _highestAchivedMedal >= SHOW_TROPHY_THRESHOLD &&
                _gemOfferButton != null &&
                (!_isOfferEverActive || _isOfferWantToActive);
        });
        StartCoroutine(WaitUntilOutToMainScreen(()=>{            
            EnableOffer();
        }));
    }

    private void DisableOffer()
    {
        _offerCooldownTime.GetReward();
        _autoShowTime.ResetTime();
        _isOfferActive.value = false;
        _isOfferWantToActive.value = false;
        InitOfferStatusTracking();
        _gemOfferButton.Hide();
    }

    private void EnableOffer()
    {
        _offerEndTime.GetReward();
        _autoShowTime.ResetTime();
        _isOfferActive.value = true;
        _isOfferEverActive.value = true;
        InitOfferStatusTracking();
        _gemOfferButton.Show();
    }

    public void Show(params object[] parameters)
    {
        if (_isShow)
            return;
        _isShow = true;
        //TODO: Hide IAP & Popup
        _mainCanvas.HideImmediately();
        //_mainCanvas.ShowImmediately();
        _pivot.position = _gemOfferButton.transform.position;
        _pivot.localScale = Vector3.zero;
        _pivot.DOMove(_pivotOriginal, AnimationDuration.TINY);
        _pivot.DOScale(Vector3.one, AnimationDuration.TINY);

        #region Design Event
        if (parameters.Length > 0 && parameters[0] != null)
        {
            m_Operation = (string)parameters[0];
            string popupName = "GemOffer";
            string status = $"Start";
            string operation = m_Operation;
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        }
        #endregion
    }

    public void Hide()
    {
        if (!_isShow)
            return;
        _isShow = false;
        _pivot.DOMove(_gemOfferButton.transform.position, AnimationDuration.TINY);
        _pivot.DOScale(Vector3.zero, AnimationDuration.TINY).OnComplete(() =>
        {                
            _mainCanvas.Hide();
        });

        #region Design Event
        if (m_Operation != "")
        {
            string popupName = "GemOffer";
            string status = $"Complete";
            string operation = m_Operation;
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        }
        #endregion
    }

    private void OnCloseBtnClicked()
    {
        Hide();
        _isOfferWantToActive.value = false;
        if (_autoShowCoroutine != null)
        {
            StopCoroutine(_autoShowCoroutine);
            _autoShowTime.GetReward();
            _autoShowCoroutine = StartCoroutine(TrackIsOfferAutoShow());
        }
    }

    private void OnPurchaseCompleted(params object[] objects)
    {
        if (objects[0] is LG_IAPButton lG_IAPButton && lG_IAPButton.IAPProductSO == _IAPButton.IAPProductSO)
        {
            Hide();
            DisableOffer();
        }
    }

    private string GetOfferRemainTime()
    {
        TimeSpan interval = DateTime.Now - _offerEndTime.LastRewardTime;
        var remainingSeconds = _offerEndTime.CoolDownInterval - interval.TotalSeconds;
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
}