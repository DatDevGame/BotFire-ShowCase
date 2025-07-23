using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LadderedOfferPopup : Singleton<LadderedOfferPopup>
{
    [SerializeField, BoxGroup("UI Ref")] private Transform _cellParentLeft, _cellParentRight, _linkParent, _pivot;
    [SerializeField, BoxGroup("UI Ref")] private CanvasGroupVisibility _mainCanvas;
    [SerializeField, BoxGroup("UI Ref")] private Button _closeButton;
    [SerializeField, BoxGroup("UI Ref")] private TextMeshProUGUI _offerRemainTimeText;
    [SerializeField, BoxGroup("UI Ref")] private Image _normalLink, _selectedLink;
    [SerializeField, BoxGroup("UI Ref")] private ScrollRect _cellScroll;
    [SerializeField, BoxGroup("Config")] private Vector2 _linkDirection = Vector2.down;
    [SerializeField, BoxGroup("Asset")] private LadderedOfferCell _cellTemplate;
    [SerializeField, BoxGroup("Data")] private LadderedOfferSO _ladderedOfferSO;

    private List<LadderedOfferCell> _offerCells = new();
    private List<Image> _links = new();
    private SelectingModeUIFlow _selectingModeUIFlow = null;
    private LadderedOfferButton _ladderedOfferButton = null;
    private bool _isShow = false;
    private Vector3 _pivotOriginal;
    private string _remainTimeStr = string.Empty;
    private Vector2 _spriteSize;
    private Vector4 _spriteBorder;
    private float _pixelPerUnitMul;

    public LadderedOfferSO ladderedOfferSO => _ladderedOfferSO;

    #region Design Event
    private string m_Operation;
    #endregion
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

        SelectingModeUIFlow.OnCheckFlowCompleted += OnCheckFlowCompleted;
        _closeButton.onClick.AddListener(OnCloseBtnClicked);
        Image image = _normalLink.GetComponent<Image>();
        Sprite sprite = image != null ? image.sprite : null;
        _pixelPerUnitMul = image != null ? image.pixelsPerUnitMultiplier : 1;
        float pixelPerUnit = sprite != null ? sprite.pixelsPerUnit : 0;
        _spriteBorder = sprite != null ? sprite.border : Vector4.zero;
        Bounds bound = sprite != null ? sprite.bounds : default;
        _spriteSize = bound.size * pixelPerUnit;
    }

    private void OnDestroy()
    {
        SelectingModeUIFlow.OnCheckFlowCompleted -= OnCheckFlowCompleted;
        _closeButton.onClick.RemoveListener(OnCloseBtnClicked);
    }

    private IEnumerator Start()
    {
        yield return null;
        _pivotOriginal = _pivot.position;
        StartCoroutine(WaitForAutoShow());
        StartCoroutine(UpdateOfferRemainTime());
        if (!_ladderedOfferSO.canShow)
        {
            yield break;
        }
        if (!_ladderedOfferSO.isResetOffer)
        {
            yield return ReBuildLadder_CR();
        }
    }

    private IEnumerator ReBuildLadder_CR()
    {
        List<LadderedOfferReward> rewards = _ladderedOfferSO.GetOfferSet();
        while (_offerCells.Count != rewards.Count)
        {
            if (_offerCells.Count < rewards.Count)
            {
                _offerCells.Add(Instantiate(_cellTemplate, _linkParent));
                yield return null;
            }
            else
            {
                LadderedOfferCell exceededCell = _offerCells[_offerCells.Count - 1];
                _offerCells.RemoveAt(_offerCells.Count - 1);
                Destroy(exceededCell);
                exceededCell.transform.parent = null;
            }
        }
        for (int i = 0, length = _offerCells.Count; i < length; ++i)
        {
            LadderedOfferCell cell = _offerCells[i];
            cell.Init(rewards[i], i);
            cell.transform.parent = (i % 4) switch
            {
                0 => _cellParentLeft,
                1 => _cellParentRight,
                2 => _cellParentRight,
                3 => _cellParentLeft,
                _ => _cellParentLeft,
            };
        }
        yield return null;
        ContentSizeFitter sizeFitter = _cellScroll.content.GetComponent<ContentSizeFitter>();
        if (sizeFitter)
        {
            sizeFitter.enabled = false;
            sizeFitter.enabled = true;
        }
        yield return null;
        while (_links.Count != _offerCells.Count - 1)
        {
            if (_links.Count < _offerCells.Count - 1)
            {
                Image newLink = Instantiate(_normalLink, _linkParent);
                newLink.enabled = true;
                Transform newLinkTrs = newLink.transform;
                newLinkTrs.localScale = Vector3.one;
                Vector3 from = _linkParent.InverseTransformPoint(_offerCells[_links.Count].globalCenter);
                Vector3 to = _linkParent.InverseTransformPoint(_offerCells[_links.Count + 1].globalCenter);
                Quaternion rotateToOther = Quaternion.FromToRotation(_linkDirection, to - from);
                newLinkTrs.rotation *= rotateToOther;
                newLinkTrs.localPosition = 0.5f * (from + to);
                RectTransform linkRect = newLinkTrs as RectTransform;
                Vector2 baseSize = linkRect.sizeDelta;
                if (Mathf.Abs(Vector2.Dot(_linkDirection, Vector2.up)) < Vector2.kEpsilon)
                {
                    baseSize.x = (to - from).magnitude;
                    float wrapCount = (baseSize.x * _pixelPerUnitMul - _spriteSize.x) / (_spriteSize.x - _spriteBorder.x - _spriteBorder.z) + 1f;
                    wrapCount = Mathf.FloorToInt(wrapCount) % 2 == 0 ? Mathf.Floor(wrapCount) + 1 : Mathf.Floor(wrapCount);
                    baseSize.x = wrapCount * _spriteSize.x / _pixelPerUnitMul - (_spriteBorder.x + _spriteBorder.z) * (wrapCount - 1) / _pixelPerUnitMul;
                }
                else
                {
                    baseSize.y = (to - from).magnitude;
                    float wrapCount = (baseSize.y * _pixelPerUnitMul - _spriteSize.y) / (_spriteSize.y - _spriteBorder.y - _spriteBorder.w) + 1f;
                    wrapCount = Mathf.FloorToInt(wrapCount) % 2 == 0 ? Mathf.Floor(wrapCount) + 1 : Mathf.Floor(wrapCount);
                    baseSize.y = wrapCount * _spriteSize.y / _pixelPerUnitMul - (_spriteBorder.y + _spriteBorder.w) * (wrapCount - 1) / _pixelPerUnitMul;
                }
                linkRect.sizeDelta = baseSize;
                _links.Add(newLink);
                yield return null;
            }
            else
            {
                Image exceededLink = _links[_links.Count - 1];
                _links.RemoveAt(_links.Count - 1);
                exceededLink.enabled = false;
                exceededLink.transform.parent = null;
                Destroy(exceededLink);
            }
        }
        UpdateCellsStatus();
    }

    public void UpdateCellsStatus()
    {
        int ladderStep = _ladderedOfferSO.ladderStepIndex;
        for (int i = 0, length = _offerCells.Count; i < length; ++i)
        {
            _offerCells[i].UpdateStatus(ladderStep);
        }
        _selectedLink.enabled = false;
        for (int i = 0, length = _links.Count; i < length; ++i)
        {
            Image link = _links[i];
            if (i == ladderStep)
            {
                _selectedLink.enabled = true;
                link.enabled = false;
                _selectedLink.transform.localRotation = link.transform.localRotation;
                _selectedLink.transform.localPosition = link.transform.localPosition;
                (_selectedLink.transform as RectTransform).sizeDelta = (link.transform as RectTransform).sizeDelta;
            }
            else
            {
                link.enabled = true;
            }
        }
    }

    private IEnumerator WaitForAutoShow()
    {
        yield return new WaitUntil(() => _ladderedOfferSO.canShow);
        _ladderedOfferButton.Show();
        var wait = new WaitUntil(() => _ladderedOfferSO.canAutoShow);
        while (true)
        {
            yield return wait;
            if (_ladderedOfferSO.isResetOffer)
            {
                yield return ReBuildLadder_CR();
            }
            _ladderedOfferSO.SetLastAutoShowTrophy();
            yield return WaitUntilOutToMainScreen(() =>
            {
                Show(true);
            });
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
        yield return new WaitUntil(() => _ladderedOfferSO.canShow);
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.9f));
        var wait = new WaitForSeconds(1f);
        while (true)
        {
            _remainTimeStr = _ladderedOfferSO.GetOfferRemainTime();
            _ladderedOfferButton?.UpdateTime(_remainTimeStr);
            if (_isShow)
            {
                _offerRemainTimeText.SetText(_remainTimeStr);
            }
            yield return wait;
        }
    }

    private void OnCheckFlowCompleted(SelectingModeUIFlow selectingModeUIFlow)
    {
        _selectingModeUIFlow = selectingModeUIFlow;
    }

    private void OnCloseBtnClicked()
    {
        Hide();
    }

    [Button]
    public void Show(bool isAutoShow = false)
    {
        if (_isShow)
            return;
        _isShow = true;
        //TODO: Hide IAP & Popup
        _mainCanvas.HideImmediately();
        //_mainCanvas.ShowImmediately();
        _pivot.position = _ladderedOfferButton.transform.position;
        _pivot.localScale = Vector3.zero;
        _pivot.DOMove(_pivotOriginal, AnimationDuration.TINY);
        _pivot.DOScale(Vector3.one, AnimationDuration.TINY);

        #region Design Event
        try
        {
            string popupName = "LadderedOffer";
            string status = DesignEventStatus.Start;
            m_Operation = isAutoShow ? "Automatically" : "Manually";
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, m_Operation, status);
            Debug.Log($"Key popupName: {popupName} | m_Operation: {m_Operation} | status: {status}");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    public void Hide()
    {
        if (!_isShow)
            return;
        _isShow = false;
        _pivot.DOMove(_ladderedOfferButton.transform.position, AnimationDuration.TINY);
        _pivot.DOScale(Vector3.zero, AnimationDuration.TINY).OnComplete(() =>
        {
            _mainCanvas.Hide();
        });

        #region Design Event
        try
        {
            string popupName = "LadderedOffer";
            string status = DesignEventStatus.Complete;
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, m_Operation, status);
            Debug.Log($"Key popupName: {popupName} | m_Operation: {m_Operation} | status: {status}");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    public void ConnectButton(LadderedOfferButton btn)
    {
        _ladderedOfferButton = btn;
        //_ladderedOfferButton.UpdateView(_IAPButton.IAPProductSO);
        if (_ladderedOfferSO.canShow)
        {
            _ladderedOfferButton.Show();
        }
        else
        {
            _ladderedOfferButton.Hide();
        }
    }
}