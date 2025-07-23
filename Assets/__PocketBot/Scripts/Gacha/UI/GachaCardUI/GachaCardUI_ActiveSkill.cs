using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.GUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaCardUI_ActiveSkill : GachaCardUI<GachaCard_ActiveSkill>
{
    [SerializeField]
    protected TextMeshProUGUI m_TitleText;
    [SerializeField]
    protected TextMeshProUGUI m_QuantityText;
    [SerializeField]
    protected Image m_SkillIconImage;

    public override void UpdateView(DuplicateGachaCardsGroup cardInfo, GachaCard_ActiveSkill gachaCard, Action callback, bool isInSummary = false)
    {
        m_SkillIconImage.sprite = gachaCard.GachaItemSO.GetThumbnailImage();
        m_TitleText.SetText(gachaCard.GachaItemSO.GetDisplayName());
        m_QuantityText.SetText($"x{cardInfo.cardsAmount.ToRoundedText()}");
        callback?.Invoke();
    }
}