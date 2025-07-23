using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.GUI;
using HyrphusQ.SerializedDataStructure;
using UnityEngine;
using UnityEngine.UI;
using static DrawCardProcedure;

public class PurchaseConfirmationPopupContent : MonoBehaviour
{
    [SerializeField]
    private Image m_BGImage;
    [SerializeField]
    private Image m_RarityBGImage;
    [SerializeField]
    private Image m_CurrencyImage;
    [SerializeField]
    private Image m_PartIconImage;
    [SerializeField]
    private Image m_HiddenCardIconImage;
    [SerializeField]
    private TextAdapter m_QuatityText;
    [SerializeField]
    private DailyDealsDataSO m_DailyDealsDataSO;

    public void Init(PartCard partCard)
    {
        m_RarityBGImage.gameObject.SetActive(true);
        m_RarityBGImage.sprite = m_DailyDealsDataSO.config.GetCardIconByRarity(partCard.partSO.GetRarityType());
        m_PartIconImage.sprite = partCard.partSO.GetThumbnailImage();
        m_PartIconImage.gameObject.SetActive(true);
        m_CurrencyImage.gameObject.SetActive(false);
        m_QuatityText.SetText($"x{partCard.numOfCards.ToRoundedText()}");
    }

    public void Init(ShopProductSO shopProductSO)
    {
        m_RarityBGImage.gameObject.SetActive(false);
        m_CurrencyImage.sprite = shopProductSO.icon;
        m_CurrencyImage.gameObject.SetActive(true);
        m_PartIconImage.gameObject.SetActive(false);
        if (shopProductSO.currencyItems != null)
            m_QuatityText.SetText($"+{shopProductSO.currencyItems.Values.ToList()[0].value.ToRoundedText()}");
        else if (shopProductSO.generalItems != null)
            m_QuatityText.SetText($"+{shopProductSO.generalItems.Values.ToList()[0].value.ToRoundedText()}");
    }
}