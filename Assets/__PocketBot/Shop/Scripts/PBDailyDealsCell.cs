using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LatteGames.Monetization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PBDailyDealsCell : MonoBehaviour
{
    [SerializeField]
    private Button m_Button;
    [SerializeField]
    private Button m_ReclaimButton;
    [SerializeField]
    private Button m_LockButton;
    [SerializeField]
    private LG_IAPButton m_IAPButton;
    [SerializeField]
    private Image m_RarityBGImage;
    [SerializeField]
    private Image m_IconImage;
    [SerializeField]
    private Image m_IconPartImage;
    [SerializeField]
    private TextMeshProUGUI m_QuantityText;
    [SerializeField]
    private GameObject m_ClaimedPanelGO;
    [SerializeField]
    private GameObject m_NonLockBackgroundGO;
    [SerializeField]
    private GameObject m_LockBackgroundGO;
    [SerializeField]
    private Image m_CardIconImage;
    [SerializeField]
    private TextMeshProUGUI m_PriceText;

    private DailyDealsDataSO m_DailyDealsDataSO;
    private DailyDealsItem m_DailyDealsItem;

    public LG_IAPButton iapButton => m_IAPButton;

    private void Awake()
    {
        m_LockButton.onClick.AddListener(OnLockButtonClicked);
        m_ReclaimButton.onClick.AddListener(OnReclaimButtonClicked);
    }

    private void OnReclaimButtonClicked()
    {
        ToastUI.Show(I2LHelper.TranslateTerm(I2LTerm.Text_ItemIsClaimed));
    }

    private void OnLockButtonClicked()
    {
        ToastUI.Show(I2LHelper.TranslateTerm(I2LTerm.Text_LockedBubbleText));
    }

    public void Initialize(DailyDealsDataSO dailyDealsDataSO, DailyDealsItem dailyDealsItem)
    {
        m_DailyDealsDataSO = dailyDealsDataSO;
        m_DailyDealsItem = dailyDealsItem;
        // m_QuantityText.transform.SetParent(m_CardIconImage.transform.parent);
        // m_QuantityText.transform.SetAsFirstSibling();
        m_IconImage.gameObject.SetActive(dailyDealsItem == null ? false : dailyDealsItem.itemType != ItemType.Part);
        m_IconPartImage.gameObject.SetActive(dailyDealsItem == null ? false : dailyDealsItem.itemType == ItemType.Part);
        m_IconPartImage.sprite = m_IconImage.sprite = dailyDealsItem == null ? null : (dailyDealsItem.itemType == ItemType.Part && dailyDealsItem.partCard != null ? dailyDealsItem.partCard.partSO.GetThumbnailImage() : dailyDealsItem.productSO.icon);
        if (m_DailyDealsItem?.productSO != null)
            m_IAPButton.OverrideSetup(m_DailyDealsItem.productSO);
        UpdateView();
    }

    public void Claim()
    {
        m_DailyDealsItem.Claim();
    }

    public void UpdateView()
    {
        if (m_DailyDealsItem == null)
        {
            m_IconImage.sprite = null;
            m_IconPartImage.sprite = null;
            m_Button.interactable = false;
            m_NonLockBackgroundGO.SetActive(false);
            m_LockBackgroundGO.SetActive(true);
            m_PriceText.gameObject.SetActive(false);
        }
        else if (m_DailyDealsItem.itemType == ItemType.Part && m_DailyDealsItem.partCard == null)
        {
            // Regenerate daily deals if necessary
            m_IconImage.sprite = null;
            m_IconPartImage.sprite = null;
            m_Button.interactable = false;
            m_NonLockBackgroundGO.SetActive(false);
            m_LockBackgroundGO.SetActive(true);
            m_PriceText.gameObject.SetActive(false);
        }
        else
        {
            m_Button.interactable = !m_DailyDealsItem.isClaimed;
            m_ReclaimButton.interactable = m_DailyDealsItem.isClaimed;
            m_NonLockBackgroundGO.SetActive(true);
            m_LockBackgroundGO.SetActive(false);
            m_ClaimedPanelGO.SetActive(m_DailyDealsItem.isClaimed);
            if (m_DailyDealsItem.itemType == ItemType.Part)
            {
                m_RarityBGImage.gameObject.SetActive(true);
                m_RarityBGImage.sprite = m_DailyDealsDataSO.config.GetCardIconByRarity(m_DailyDealsItem.partCard.partSO.GetRarityType());
                m_QuantityText.SetText($"x{m_DailyDealsItem.partCard.numOfCards}");
            }
            else
            {
                m_RarityBGImage.gameObject.SetActive(false);
                m_QuantityText.SetText($"+{m_DailyDealsItem.productSO.currencyItems.Values.First().value}");
            }
            m_PriceText.gameObject.SetActive(!m_DailyDealsItem.isClaimed);
        }
    }
}