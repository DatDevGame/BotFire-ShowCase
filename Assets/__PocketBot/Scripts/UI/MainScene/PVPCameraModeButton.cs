using System.Collections.Generic;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using TMPro;

public class PVPCameraModeButton : MonoBehaviour
{
    [SerializeField] private RectTransform _panelHolder;

    [SerializeField, BoxGroup("Property")] private float _timeScrollModeButton = 0.2f;

    [SerializeField, BoxGroup("Button Camera")] private RectTransform _recImageSelectedButton;
    [SerializeField, BoxGroup("Button Camera")] private Button _cameraModeButton;
    [SerializeField, BoxGroup("Button Camera")] private Button _thirdPersonButton;
    [SerializeField, BoxGroup("Button Camera")] private Button _topDownPersonButton;
    [SerializeField, BoxGroup("Button Camera")] private Button _firstPersonButton;
    [SerializeField, BoxGroup("Button Camera")] private List<Button> _cameraViewButtons;

    [SerializeField, BoxGroup("Data Mode")] private PPrefIntVariable _pprefModeCamera;


    private bool _isOn = false;
    private bool _isSelect = true;

    private void Awake()
    {
        Image imagePanelHolder = _panelHolder.GetComponent<Image>();
        imagePanelHolder
            .DOFillAmount(0, 0);

        InteractableButtonCameraView();

        _cameraModeButton.onClick.AddListener(() =>
        {
            if (!_isOn)
            {
                OnButtonCameraMode();
                return;
            }
            _isOn = false;
            OffButtonCameraMode();
        });

        _thirdPersonButton.onClick.AddListener(() =>
        {
            SetCameraMode(0);
        });
        _topDownPersonButton.onClick.AddListener(() =>
        {
            SetCameraMode(1);
        });
        _firstPersonButton.onClick.AddListener(() =>
        {
            SetCameraMode(2);
        });
    }

    private void SetCameraMode(int index)
    {
        if (!_isOn || !_isSelect) return;
        _pprefModeCamera.value = index;
        InteractableButtonCameraView();
        GameEventHandler.Invoke(CameraViewEvent.Switch, _pprefModeCamera.value);
    }
    private void OnButtonCameraMode()
    {
        Image imagePanelHolder = _panelHolder.GetComponent<Image>();
        imagePanelHolder
            .DOFillAmount(1, _timeScrollModeButton)
            .OnComplete(() =>
            {
                _isOn = true;
            });
    }
    private void OffButtonCameraMode()
    {
        Image imagePanelHolder = _panelHolder.GetComponent<Image>();
        imagePanelHolder
            .DOFillAmount(0, _timeScrollModeButton)
            .OnComplete(() =>
            {
                _isOn = false;
            });
    }
    private void InteractableButtonCameraView()
    {
        _isSelect = false;
        _cameraViewButtons.ForEach((v) =>
        {
            v.interactable = true;

            TMP_Text text = v.GetComponentInChildren<TMP_Text>();
            text.characterSpacing = 7.5f;
            text.color = Color.black;
            text.DOFade(0.7f, 0);
        });
        Vector3 anchorPosButtonSelected = _cameraViewButtons[_pprefModeCamera.value].GetComponent<RectTransform>().anchoredPosition;
        _cameraViewButtons[_pprefModeCamera.value].interactable = false;

        _recImageSelectedButton
            .DOSizeDelta(new Vector2(_recImageSelectedButton.sizeDelta.x - 30, _recImageSelectedButton.sizeDelta.y - 30), 0.1f);

        _recImageSelectedButton
            .DOAnchorPos(anchorPosButtonSelected, 0.2f)
            .OnComplete(() =>
            {
                TMP_Text text = _cameraViewButtons[_pprefModeCamera.value].GetComponentInChildren<TMP_Text>().GetComponentInChildren<TMP_Text>();
                text.characterSpacing = 2f;
                text.color = Color.white;
                text.DOFade(1, 0);

                _recImageSelectedButton
                    .DOSizeDelta(new Vector2(_recImageSelectedButton.sizeDelta.x + 30, _recImageSelectedButton.sizeDelta.y + 30), 0.1f)
                    .OnComplete(() =>
                    {
                        _isSelect = true;
                    });
            });

    }
}
