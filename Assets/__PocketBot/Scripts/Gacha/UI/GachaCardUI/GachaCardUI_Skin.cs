using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.SerializedDataStructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaCardUI_Skin : GachaCardUI<GachaCard_Skin>
{
    [SerializeField]
    protected Image m_ThumbnailImage;
    [SerializeField]
    protected TMP_Text m_GearNameTxt;

    public override void UpdateView(DuplicateGachaCardsGroup cardInfo, GachaCard_Skin gachaCard, Action callback, bool isInSummary = false)
    {
        m_ThumbnailImage.sprite = gachaCard.GetThumbnailImage();
        m_GearNameTxt.text = gachaCard.SkinSO.partSO.GetDisplayName();
        callback?.Invoke();
    }
}