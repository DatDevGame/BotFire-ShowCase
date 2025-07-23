using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GachaSystem.Core;
using HyrphusQ.SerializedDataStructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaCardUI_Part : GachaCardUI<GachaCard_Part>
{
    [Header("Part UIs")]
    [SerializeField]
    protected bool m_IsUnpackCard = true;
    [SerializeField]
    protected Image m_NewImage;
    [SerializeField]
    protected Image m_BackgroundImage;
    [SerializeField]
    protected Image m_PartThumbnailImage;
    [SerializeField]
    protected TextMeshProUGUI m_TitleText;
    [SerializeField]
    protected TextMeshProUGUI m_QuantityText;
    [SerializeField]
    protected GearCardProgressBar m_GearCardProgressBar;
    [SerializeField]
    protected SerializedDictionary<RarityType, Sprite> m_BackgroundDictionary;

    public override void UpdateView(DuplicateGachaCardsGroup cardInfo, GachaCard_Part gachaCard, Action callback, bool isInSummary = false)
    {
        bool isBossOrTransformCard =  gachaCard.PartSO is PBChassisSO chassisSO && (chassisSO.IsSpecial || chassisSO.IsTransformBot);
        if (!isInSummary && !isBossOrTransformCard)
        {
            m_BackgroundImage.gameObject.SetActive(false);
            m_TitleText.gameObject.SetActive(false);
            m_QuantityText.gameObject.SetActive(false);
            m_NewImage.gameObject.SetActive(false);

            if (m_GearCardProgressBar != null)
            {
                m_GearCardProgressBar.gameObject.SetActive(true);
                m_GearCardProgressBar.UpdateViewWithAnimation(gachaCard.GachaItemSO, cardInfo.cardsAmount, OnAnimationCompleted);
            }
        }
        else
        {
            string displayName = gachaCard.GachaItemSO.GetDisplayName();
            m_PartThumbnailImage.sprite = gachaCard.GetThumbnailImage();
            m_BackgroundImage.sprite = m_BackgroundDictionary.Get(gachaCard.GachaItemSO.GetRarityType());
            m_TitleText.SetText(isBossOrTransformCard ? string.Empty : displayName);
            m_QuantityText.SetText(isBossOrTransformCard ? displayName : $"x{cardInfo.cardsAmount.ToRoundedText()}");
            m_QuantityText.gameObject.SetActive(isInSummary || isBossOrTransformCard);

            m_BackgroundImage.gameObject.SetActive(true);
            m_TitleText.gameObject.SetActive(true);
            m_QuantityText.gameObject.SetActive(true);
            m_NewImage.gameObject.SetActive(gachaCard.PartSO.IsNew() || (cardInfo.isBonusCard && !gachaCard.PartSO.IsUnlocked()));

            if (m_GearCardProgressBar != null)
                m_GearCardProgressBar.gameObject.SetActive(false);
            OnAnimationCompleted();
        }

        void OnAnimationCompleted()
        {
            callback?.Invoke();
        }
    }
}