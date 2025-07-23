using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using UnityEngine.UI;

public class NoAdsCarouselPanel : CarouselPanel
{
    [SerializeField] Button button;
    [SerializeField] PPrefBoolVariable removeAdsPPref;

    public override bool isAvailable => !removeAdsPPref.value;

    private void Awake()
    {
        button.onClick.AddListener(OnClickButton);
        removeAdsPPref.onValueChanged += OnRemoveAdsChange;
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnClickButton);
        removeAdsPPref.onValueChanged -= OnRemoveAdsChange;
    }

    void OnClickButton()
    {
        NoAdsOffersPopup.Instance.Show();
    }

    void OnRemoveAdsChange(ValueDataChanged<bool> data)
    {
        if (removeAdsPPref.value)
        {
            OnPurchased?.Invoke(this, index);
        }
    }
}
