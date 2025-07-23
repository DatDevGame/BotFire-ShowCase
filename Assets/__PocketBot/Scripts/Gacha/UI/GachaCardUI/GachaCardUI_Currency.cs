using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.SerializedDataStructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaCardUI_Currency : GachaCardUI<GachaCard_Currency>
{
    [SerializeField]
    protected Image m_BackgroundImage;
    [SerializeField]
    protected TextMeshProUGUI m_TitleText;
    [SerializeField]
    protected TextMeshProUGUI m_QuantityText;

    public override void UpdateView(DuplicateGachaCardsGroup cardInfo, GachaCard_Currency currencyGachaCard, Action callback, bool isInSummary = false)
    {
        m_BackgroundImage.sprite = currencyGachaCard.GetThumbnailImage();
        m_TitleText.SetText(currencyGachaCard.CurrencyType.ToLabelText());
        m_QuantityText.SetText($"+{(cardInfo.cardsAmount * currencyGachaCard.Amount).ToRoundedText()}");
        callback?.Invoke();
    }
}