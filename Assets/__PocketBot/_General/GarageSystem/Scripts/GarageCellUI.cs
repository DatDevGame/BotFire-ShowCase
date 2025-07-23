using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using HyrphusQ.Events;
using LatteGames.Monetization;
using System;
using TMPro;
using HyrphusQ.GUI;
using HightLightDebug;
using LatteGames.Template;

public class GarageCellUI : MonoBehaviour
{
    public GarageSO GarageSO => m_GarageSO;

    [SerializeField, BoxGroup("Ref")] private SoundID m_SelectSound;
    [SerializeField, BoxGroup("Ref")] private SoundID m_UnlockSound;

    [SerializeField, BoxGroup("Ref")] private Image m_Avatar;
    [SerializeField, BoxGroup("Ref")] private Image m_BannerSpecial;
    [SerializeField, BoxGroup("Ref")] private MultiImageButton m_SpecialBuyButton;
    [SerializeField, BoxGroup("Ref")] private MultiImageButton m_BuyButton;
    [SerializeField, BoxGroup("Ref")] private TextAdapter m_BuyText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_RVText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_NameGarge;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_OutlineNameGarge;
    [SerializeField, BoxGroup("Ref")] private Button m_PreviewButton;
    [SerializeField, BoxGroup("Ref")] private GameObject m_OwnedPanel;
    [SerializeField, BoxGroup("Ref")] private GameObject m_OutLineSelected;
    [SerializeField, BoxGroup("Ref")] private GrayscaleUI m_GrayscaleBuyButton;
    [SerializeField, BoxGroup("Ref")] private RVButtonBehavior m_RVButtonBehavior;

    [SerializeField, BoxGroup("Data")] private GarageManagerSO m_GarageManagerSO;

    private GarageSO m_GarageSO;
    private bool m_IsPreviewSelecting = false;
    private void Awake()
    {
        m_PreviewButton.onClick.AddListener(OnClickPreview);
        m_SpecialBuyButton.onClick.AddListener(OnBuy);
        m_BuyButton.onClick.AddListener(OnBuy);
        m_RVButtonBehavior.OnRewardGranted += RVButtonBehavior_OnRewardGranted;
    }

    private void OnDestroy()
    {
        m_PreviewButton.onClick.RemoveListener(OnClickPreview);
        m_SpecialBuyButton.onClick.RemoveListener(OnBuy);
        m_BuyButton.onClick.RemoveListener(OnBuy);
        m_RVButtonBehavior.OnRewardGranted -= RVButtonBehavior_OnRewardGranted;
    }

    public void Load(GarageSO garageSO = null)
    {
        // Initialize m_GarageSO if needed
        if (m_GarageSO == null && garageSO != null)
            m_GarageSO = garageSO;

        if (!m_GarageManagerSO.IsDisplayedGarage(m_GarageSO))
        {
            this.gameObject.SetActive(false);
            return;
        }

        // Handle null m_GarageSO scenario
        if (m_GarageSO == null)
        {
            Debug.LogError("GarageCell Is Null");
            return;
        }

        // Update UI components based on m_GarageSO properties
        UpdateAvatar();
        UpdateButtons();
        UpdateOwnedPanel();
        UpdateOutline();
        UpdateGarageName();
        UpdateBuyButtonText();
    }

    // Helper methods for readability and single-responsibility
    private void UpdateAvatar()
    {
        m_Avatar.sprite = m_GarageSO.Avatar;

        m_BannerSpecial.gameObject.SetActive(false);
        if (!m_GarageSO.IsOwned && m_GarageManagerSO.BannerSpecial.ContainsKey(m_GarageSO.BuyType))
        {
            m_BannerSpecial.gameObject.SetActive(true);
            m_BannerSpecial.sprite = m_GarageManagerSO.BannerSpecial[m_GarageSO.BuyType];
        }
        else if(m_GarageSO.IsOwned && m_GarageManagerSO.BannerSpecial.ContainsKey(m_GarageSO.BuyType))
            m_BannerSpecial.gameObject.SetActive(false);
    }

    private void UpdateButtons()
    {
        bool isOwned = m_GarageSO.IsOwned;
        m_RVButtonBehavior.gameObject.SetActive(!isOwned && m_GarageSO.BuyType == BuyType.Ads);
        m_BuyButton.gameObject.SetActive(!isOwned && m_GarageSO.BuyType == BuyType.Coin || !isOwned && m_GarageSO.BuyType == BuyType.Gem);
        m_SpecialBuyButton.gameObject.SetActive(!isOwned && m_GarageSO.BuyType != BuyType.Ads && m_GarageSO.BuyType != BuyType.Coin && m_GarageSO.BuyType != BuyType.Gem);
    }

    private void UpdateOwnedPanel()
    {
        m_OwnedPanel.SetActive(m_GarageSO.IsOwned);
    }

    private void UpdateOutline()
    {
        m_OutLineSelected.SetActive(m_GarageSO.IsOwned ? m_GarageSO.IsSelected : m_IsPreviewSelecting);
    }

    private void UpdateGarageName()
    {
        m_NameGarge.SetText(m_GarageSO.NameGarage);
        m_OutlineNameGarge.SetText(m_GarageSO.NameGarage);
    }

    private void UpdateBuyButtonText()
    {
        if (m_GarageSO.BuyType == BuyType.Coin || m_GarageSO.BuyType == BuyType.Gem)
        {
            string currencyType = m_GarageSO.BuyType == BuyType.Coin ? "Coin" : "Gem";
            string buyText = m_BuyText.blueprintText
                .Replace("{value}", m_GarageSO.PriceCondition.ToString())
                .Replace("{currencytype}", currencyType);

            m_BuyText.SetText(buyText);
            m_GrayscaleBuyButton.SetGrayscale(IsEnoughAmount());
        }
        else if (m_GarageSO.BuyType != BuyType.Ads)
        {
            
        }

        if (m_GarageSO.BuyType == BuyType.Ads)
        {
            m_RVText.SetText($"AD {m_GarageSO.AdsWatchedCount}/{m_GarageSO.PriceCondition}");
        }
    }


    public void PreviewSelect()
    {
        m_IsPreviewSelecting = true;
        m_OutLineSelected.SetActive(m_IsPreviewSelecting);
    }
    public void PreviewUnSelect()
    {
        m_IsPreviewSelecting = false;
        m_OutLineSelected.SetActive(m_IsPreviewSelecting);
    }

    private void OnBuy()
    {
        //Garage Transformer
        if (m_GarageSO.BuyType != BuyType.Coin && m_GarageSO.BuyType != BuyType.Gem)
        {
            GameEventHandler.Invoke(m_GarageSO.BuyType);
            //DebugPro.AquaBold($"Subcribe Transformer Popup this Event");
            PBUltimatePackPopup.Instance.TryShowIfCan();
            return;
        }

        int garageIndex = m_GarageManagerSO.GarageSOs.IndexOf(m_GarageSO);
        ResourceLocationProvider garageResourceLocationProvider = new ResourceLocationProvider(ResourceLocation.UnlockSkin, $"{garageIndex}");
        //Buy Garage by Coin or Gem
        if (CurrencyManager.Instance.Spend(GetCurrencyType(), m_GarageSO.PriceCondition, garageResourceLocationProvider.GetLocation(), garageResourceLocationProvider.GetItemId()))
        {
            GarageSO.Own();
            GameEventHandler.Invoke(GarageEvent.PreviewGarage, this, true);
            Load(GarageSO);
        }
    }

    private void RVButtonBehavior_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData obj)
    {
        GarageSO.WatchedAds();

        #region MonetizationEventCode
        int garageIndex = m_GarageManagerSO.GarageSOs.IndexOf(m_GarageSO);
        int adsCount = m_GarageSO.AdsWatchedCount;
        GameEventHandler.Invoke(MonetizationEventCode.BuyGarage, garageIndex, adsCount);
        #endregion

        if (GarageSO.IsEnoughAds)
        {
            GarageSO.Own();
            GameEventHandler.Invoke(GarageEvent.PreviewGarage, this, true);
            SoundManager.Instance.PlaySFX(m_UnlockSound);
        }
        Load(GarageSO);
    }

    private void OnClickPreview()
    {
        GameEventHandler.Invoke(GarageEvent.PreviewGarage, this, false);
        SoundManager.Instance.PlaySFX(m_SelectSound);
    }

    private CurrencyType GetCurrencyType()
    {
        CurrencyType currencyType = m_GarageSO.BuyType switch
        {
            BuyType.Coin => CurrencyType.Standard,
            BuyType.Gem => CurrencyType.Premium,
            _ => CurrencyType.Standard
        };
        return currencyType;
    }

    private float GetAmountCurrentCurrency()
    {
        CurrencySO currencySO = CurrencyManager.Instance[GetCurrencyType()];
        return currencySO.value;
    }

    private bool IsEnoughAmount()
    {
        return !CurrencyManager.Instance.IsAffordable(GetCurrencyType(), GetAmountCurrentCurrency());
    }
}
