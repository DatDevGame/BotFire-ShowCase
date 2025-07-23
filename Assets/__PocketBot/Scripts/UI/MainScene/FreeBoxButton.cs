using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Events;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using LatteGames.PvP;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using TMPro;
using DG.Tweening;
using Coffee.UIExtensions;
using LatteGames;

public class FreeBoxButton : MonoBehaviour
{
    [SerializeField, BoxGroup("Property")] private int minCurrencyActive;

    [SerializeField, BoxGroup("Ads")] private RVButtonBehavior _rvButtonBehavior;

    [SerializeField, BoxGroup("Object")] private ShinyEffectForUGUI _shinyEffectForUGUI;
    [SerializeField, BoxGroup("Object")] private Button _freeBoxButton;
    [SerializeField, BoxGroup("Object")] private TMP_Text _timeRewardedTxt;
    [SerializeField, BoxGroup("Object")] private RectTransform _boxRect;
    [SerializeField, BoxGroup("Object")] private GameObject _mainFreeBox;
    [SerializeField, BoxGroup("Object")] private GameObject _titleBox;
    [SerializeField, BoxGroup("Object")] private GameObject _iconWarning;
    [SerializeField, BoxGroup("Object")] private GameObject _iconLoadingAds;
    [SerializeField, BoxGroup("Object")] private CanvasGroupVisibility canvasGroupVisibility;

    [SerializeField, BoxGroup("Data")] protected PBGachaPackManagerSO _pbGachaPackManagerSO;
    [SerializeField, BoxGroup("Data")] private CurrencySO _currencySO;
    [SerializeField, BoxGroup("Data")] private TimeBasedRewardSO _timeBasedRewardSO;

    private IEnumerator _rotateBoxCR;
    private Sequence _shinyEffectSequence;

    private void Awake()
    {
        _rvButtonBehavior.OnRewardGranted += OnRewardGrantedRevenge;
        GameEventHandler.AddActionEvent(PlayModePopup.Enable, HandleOnEnablePlayModePopup);
        GameEventHandler.AddActionEvent(PlayModePopup.Disable, HandleOnDisablePlayModePopup);
    }

    private void OnDestroy()
    {
        _rvButtonBehavior.OnRewardGranted -= OnRewardGrantedRevenge;
        GameEventHandler.RemoveActionEvent(PlayModePopup.Enable, HandleOnEnablePlayModePopup);
        GameEventHandler.RemoveActionEvent(PlayModePopup.Disable, HandleOnDisablePlayModePopup);

        if (_rotateBoxCR != null)
            StopCoroutine(_rotateBoxCR);

        _boxRect.DOKill();
        _shinyEffectSequence.Kill();
    }

    private void Start()
    {
        _rotateBoxCR = RotateBox();
        StartCoroutine(_rotateBoxCR);
    }

    private void Update()
    {
        UpdateFreeBox();
    }

    private void UpdateFreeBox()
    {
        if (_timeBasedRewardSO == null || _timeRewardedTxt == null || _titleBox == null || _currencySO == null) return;
        bool isReadyRewaredAds = AdsManager.Instance.IsReadyRewarded;
        bool canGetReward = _timeBasedRewardSO.canGetReward;

        _timeRewardedTxt.SetText(_timeBasedRewardSO.remainingTime);
        _freeBoxButton.interactable = canGetReward && isReadyRewaredAds;
        _mainFreeBox.gameObject.SetActive(_currencySO.value >= minCurrencyActive);
        _timeRewardedTxt.gameObject.SetActive(!canGetReward);
        _titleBox.gameObject.SetActive(canGetReward);
        _iconWarning.gameObject.SetActive(canGetReward);

        if (!canGetReward)
            _shinyEffectForUGUI.location = 0;

        if (_iconLoadingAds != null)
            _iconLoadingAds.SetActive(!isReadyRewaredAds);

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F))
            _timeBasedRewardSO.ResetTime();
#endif
    }

    [Button("Free Box", ButtonSizes.Large, Icon = SdfIconType.Activity, IconAlignment = IconAlignment.RightOfText), GUIColor(0, 1, 0), PropertyOrder(-99)]
    private void ResetTimeFreeBox() => _timeBasedRewardSO.ResetTime();

    public void ReadyFreeBox()
    {
        //Do Notthing
    }

    public void NotReadyFreeBox()
    {
        //Do Notthing
    }

    private void OnFreeBox()
    {
        List<GachaPack> gachaPacks = new List<GachaPack>();

        if (_pbGachaPackManagerSO != null)
            gachaPacks.Add(_pbGachaPackManagerSO.GetGachaPackCurrentArena(GachaPackRarity.Classic));

        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, null, gachaPacks, null);
    }

    private void OnRewardGrantedRevenge(RVButtonBehavior.RewardGrantedEventData data)
    {
        OnFreeBox();

        if (_timeBasedRewardSO != null)
            _timeBasedRewardSO.GetReward();

        #region MonetizationEventCode
        string adsLocation = "MainUI";
        GameEventHandler.Invoke(MonetizationEventCode.FreeBox_MainUI, adsLocation);
        #endregion

        #region DesignEvent
        string openStatus = "NoTimer";
        string location = "";
        GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
        #endregion
    }

    private IEnumerator RotateBox()
    {
        WaitForSeconds waitForSecondsNextAction = new WaitForSeconds(2);
        while (true)
        {
            if (_timeBasedRewardSO.canGetReward)
            {
                if (_boxRect != null)
                {
                    if (_shinyEffectForUGUI != null)
                    {
                        _shinyEffectSequence = DOTween.Sequence();
                        _shinyEffectSequence.Append(DOVirtual.Float(0, 1f, 1.5f, v => _shinyEffectForUGUI.location = v));
                    }

                    _boxRect.DOLocalRotate(new Vector3(0, 0, 10), 0.1f).SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            _boxRect.DOLocalRotate(new Vector3(0, 0, -10), 0.2f).SetEase(Ease.Linear)
                            .OnComplete(() =>
                            {
                                _boxRect.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.Flash);
                            });
                        });
                }
            }
            yield return waitForSecondsNextAction;
        }
    }

    private void HandleOnEnablePlayModePopup() => canvasGroupVisibility.Hide();
    private void HandleOnDisablePlayModePopup() => canvasGroupVisibility.Show();
}
