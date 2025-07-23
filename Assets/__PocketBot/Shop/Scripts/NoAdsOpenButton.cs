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

public class NoAdsOpenButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    private void Start()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        NoAdsOffersPopup.Instance.HasShowedFirstTime.onValueChanged += OnShowedFirstTime;
        NoAdsOffersPopup.Instance.RemoveAdsPPref.onValueChanged += OnRemoveAds;
        _button.onClick.AddListener(OnButtonClicked);
        UpdateView();
    }

    private void OnDestroy()
    {
        if (NoAdsOffersPopup.Instance)
        {
            NoAdsOffersPopup.Instance.HasShowedFirstTime.onValueChanged -= OnShowedFirstTime;
            NoAdsOffersPopup.Instance.RemoveAdsPPref.onValueChanged -= OnRemoveAds;
        }
        _button.onClick.RemoveListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        NoAdsOffersPopup.Instance.ShowFromOpenButtonMainScene(_button.transform);
    }

    void OnShowedFirstTime(ValueDataChanged<bool> data)
    {
        UpdateView();
    }

    void OnRemoveAds(ValueDataChanged<bool> data)
    {
        UpdateView();
    }

    void UpdateView()
    {
        gameObject.SetActive(NoAdsOffersPopup.Instance.HasShowedFirstTime && !NoAdsOffersPopup.Instance.RemoveAdsPPref.value);
    }
}
