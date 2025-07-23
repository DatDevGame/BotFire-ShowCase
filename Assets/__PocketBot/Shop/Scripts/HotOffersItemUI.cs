using System;
using System.Collections;
using System.Collections.Generic;
using HightLightDebug;
using LatteGames.Monetization;
using PackReward;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HyrphusQ.GUI;
using HyrphusQ.Events;
using LatteGames.UI;

public class HotOffersItemUI : PackRewardUI
{
    public HotOffersItemSO HotOffersItemSO => m_HotOffersItemSO;
    public DrawCardProcedure.PartCard PartCard => m_PartCard;

    [SerializeField, BoxGroup("Ref")] private PBCurrencyBuyButton m_PBCurrencyBuyButton;
    [SerializeField, BoxGroup("Ref")] private TextAdapter m_PriceText;
    [SerializeField, BoxGroup("Ref")] private Image m_BG;
    [SerializeField, BoxGroup("Ref")] private GameObject m_LockPanel;
    [SerializeField, BoxGroup("Ref")] private GameObject m_RVGroup;
    [SerializeField, BoxGroup("Ref")] private GameObject m_BlockPanel;
    [SerializeField, BoxGroup("Ref")] private GameObject m_AdsBlockPanel;
    [SerializeField, BoxGroup("Ref")] private GameObject m_Vtick;
    [SerializeField, BoxGroup("Ref")] private Image m_HiddenCardIconImage;
    [SerializeField, BoxGroup("Ref")] private Image m_RarityBGImage;
    [SerializeField, BoxGroup("Ref")] private Button m_ReclaimButton;
    [SerializeField, BoxGroup("Ref")] private DailyDealsDataSO m_DailyDealsDataSO;

    private HotOffersItemSO m_HotOffersItemSO => m_PackRewardSO as HotOffersItemSO;

    private DrawCardProcedure.PartCard m_PartCard;

    protected override void Start()
    {
        if (m_PackRewardSO != null)
            m_PartCard = GetPartCard();

        base.Start();
        // m_RewardText.transform.SetParent(m_HiddenCardIconImage.transform.parent);
        // m_RewardText.transform.SetAsFirstSibling();
        m_ReclaimButton.onClick.AddListener(() =>
        {
            ToastUI.Show(I2LHelper.TranslateTerm(I2LTerm.Text_ItemIsClaimed));
        });
    }

    protected override void Update()
    {
        if (m_PackRewardSO == null) return;

        if (m_HotOffersItemSO.IsClaimed)
        {
            m_RVBtn.interactable = false;
            m_ClaimBtn.interactable = false;
            return;
        }

        if (m_HotOffersItemSO.LastRewardTime > DateTime.Now)
            m_HotOffersItemSO.ResetNow();

        base.Update();
    }

    public void Active()
    {
        if (m_PackRewardSO == null) return;

        if (!m_HotOffersItemSO.IsActivePack)
        {
            m_HotOffersItemSO.ActivePack();
            GeneratePartCardProcedure();
        }
    }

    public void Reset()
    {
        if (m_PackRewardSO == null) return;
        m_PartCard = null;
        GeneratePartCardProcedure();
        m_PartCard = GetPartCard();

        m_HotOffersItemSO.ResetPack();
        UpdateView();
    }

    public void LockedBubbleText()
    {
        // MessageManager.Title = I2LHelper.TranslateTerm(I2LTerm.Text_LockedBubbleTextTitle);
        // MessageManager.Message = I2LHelper.TranslateTerm(I2LTerm.Text_LockedBubbleText);
        // MessageManager.Show();
        ToastUI.Show(I2LHelper.TranslateTerm(I2LTerm.Text_LockedBubbleText));
    }

    protected override void ClaimRVBtn_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData data)
    {
        if (m_PackRewardSO == null)
        {
            DebugPro.RedBold($"{this.name} | PackRewardSO Null");
            return;
        }
        m_PackRewardSO.SetAdsValue();
        UpdateView();
        Claim();


        #region MonetizationEventCode
        if (m_ClaimRVBtn.Location == AdsLocation.RV_FreePart_HotOffers_UI)
        {
            string offerName = m_PartCard.groupType switch
            {
                DrawCardProcedure.GroupType.NewAvailable => "NewGroup",
                DrawCardProcedure.GroupType.Duplicate => "DuplicateGroup",
                DrawCardProcedure.GroupType.InUsed => "InUsedGroup",
                _ => "None"
            };

            MonetizationEventCode monetizationEventCode = m_ClaimRVBtn.Location switch
            {
                AdsLocation.RV_FreePart_HotOffers_UI => MonetizationEventCode.HotOffers_Shop,
                _ => MonetizationEventCode.HotOffers_Shop
            };

            string adsLocation = m_ClaimRVBtn.Location switch
            {
                AdsLocation.RV_FreePart_HotOffers_UI => "Shop",
                _ => "None"
            };
            GameEventHandler.Invoke(monetizationEventCode, adsLocation, offerName);
        }
        else
            Debug.LogWarning("HotOffersItemUI - Warning AdsLocation");
        #endregion
    }

    protected override void Claim()
    {
        //Not Enough Gem
        if (m_PackRewardSO.ReducedValue.RequirementsPack != RequirementsPack.Ads)
        {
            PurchaseConfirmationPopup.Instance.Show(
                $"{m_PartCard.partSO.GetModule<NameItemModule>().displayName}",
                m_HotOffersItemSO.ReducedValue.Value,
                CloneCurrencyBuyButton(),
                OnConfirmCallback);


            void OnConfirmCallback(bool isAccept)
            {
                if (isAccept)
                    ClaimHandle();
            }

            PurchaseConfirmationPopupContent CloneCurrencyBuyButton()
            {
                return PurchaseConfirmationPopup.Instance.CreateContent(m_PartCard);
            }
            return;
        }

        ClaimHandle();
    }

    protected override void CurrencyButtonHandle()
    {
        if (PackRewardState == PackRewardState.Waiting)
        {
            m_PriceText.SetText(m_PackRewardSO.RemainingTimeInMinute);
        }
    }

    protected override void AdsButtonHandle()
    {
        string waitingStateText = $"<size=45><voffset=-10><sprite name=Clock><voffset=-2> {m_HotOffersItemSO.GetRemainingTimeHandle(35, 35)}";
        string readyStateText = $"WATCH AD";
        string adsReducedValueText = $"<size=50><voffset=-17><sprite name=Clock><voffset=-7><size=35>{m_PackRewardSO.GetTotalAdsWatched()}/{m_PackRewardSO.ReducedValue.Value}";
        string adsText = PackRewardState == PackRewardState.Waiting ? waitingStateText : readyStateText;

        m_RVBtn.gameObject.SetActive(!m_PackRewardSO.IsEnoughAds);
        m_ClaimBtnText.gameObject.SetActive(m_PackRewardSO.IsEnoughAds);

        if (PackRewardState == PackRewardState.Waiting)
        {
            m_RVBtn.interactable = false;
            m_RVBtnText.SetText(adsText);
        }
        else
        {
            m_RVBtn.interactable = !m_PackRewardSO.IsEnoughAds;
            m_RVBtnText.SetText(m_PackRewardSO.ReducedValue.Value > 1 ? adsReducedValueText : adsText);
        }
    }

    protected override void UpdateView()
    {
        if (m_PackRewardSO == null || m_PartCard == null)
        {
            m_BG.gameObject.SetActive(false);
            m_LockPanel.SetActive(true);
            m_ClaimBtn.interactable = false;
            m_ClaimRVBtn.interactable = false;
            return;
        }

        bool isAdsRequirement = m_PackRewardSO.ReducedValue.RequirementsPack == RequirementsPack.Ads;
        string rewardText = m_PartCard.numOfCards.ToString();
        string rewardHandle = $"x{rewardText}";

        m_BG.gameObject.SetActive(true);
        m_ClaimBtn.interactable = m_PackRewardSO.ReducedValue.RequirementsPack != RequirementsPack.Ads;
        m_ClaimRVBtn.interactable = m_PackRewardSO.ReducedValue.RequirementsPack == RequirementsPack.Ads;

        m_AdsBlockPanel.SetActive(m_PackRewardSO.ReducedValue.RequirementsPack == RequirementsPack.Ads);
        m_LockPanel.SetActive(false);
        m_RVGroup.SetActive(isAdsRequirement && !m_HotOffersItemSO.IsClaimed);
        m_BG.enabled = !(isAdsRequirement && !m_HotOffersItemSO.IsClaimed);
        m_Avatar.sprite = m_PartCard.partSO.GetModule<MonoImageItemModule>().thumbnailImage;
        m_ClaimRVBtn.gameObject.SetActive(!isAdsRequirement);
        m_PriceText.SetText(m_PriceText.blueprintText.Replace(Const.StringValue.PlaceholderValue, m_PackRewardSO.ReducedValue.Value.ToString()));
        m_RVBtn.gameObject.SetActive(isAdsRequirement);
        m_RewardText.SetText(rewardHandle);
        m_RVBtnText.SetText($"{m_PackRewardSO.GetTotalAdsWatched()}/{m_PackRewardSO.ReducedValue.Value}");
        m_PriceText.gameObject.SetActive(!m_HotOffersItemSO.IsClaimed);
        m_BlockPanel.SetActive(m_HotOffersItemSO.IsClaimed);
        m_Vtick.SetActive(m_HotOffersItemSO.IsClaimed);
        m_RarityBGImage.sprite = m_DailyDealsDataSO.config.GetCardIconByRarity(m_PartCard.partSO.GetRarityType());
    }

    private void ClaimHandle()
    {
        #region Sink Location Item
        string itemIDSink = m_PartCard.groupType switch
        {
            DrawCardProcedure.GroupType.NewAvailable => "New",
            DrawCardProcedure.GroupType.Duplicate => "Duplicate",
            DrawCardProcedure.GroupType.InUsed => "InUsed",
            _ => "None"
        };
        ResourceLocationProvider sinkLocationProvider = new ResourceLocationProvider(ResourceLocation.HotOffer, $"PartCard_{itemIDSink}");
        #endregion

        OnClaimAction?.Invoke();
        m_HotOffersItemSO.Claim(m_ResourceLocationProvider, sinkLocationProvider, m_PartCard);
        GameEventHandler.Invoke(MissionEventCode.OnHotOfferClaimed);
        UpdateView();

        #region Design Event
        if (m_HotOffersItemSO.ReducedValue.RequirementsPack == RequirementsPack.Gem)
        {
            string offerName = m_PartCard.groupType switch
            {
                DrawCardProcedure.GroupType.NewAvailable => "NewGroup",
                DrawCardProcedure.GroupType.Duplicate => "DuplicateGroup",
                DrawCardProcedure.GroupType.InUsed => "InUsedGroup",
                _ => "None"
            };

            string currency = "Gem";
            string offer = offerName;
            GameEventHandler.Invoke(DesignEvent.HotOfferBuy, currency, offer);
        }
        #endregion
    }

    private DrawCardProcedure.PartCard GetPartCard()
    {
        DrawCardProcedure.PartCard partCard = null;
        if (m_HotOffersItemSO.ReducedValue.RequirementsPack == RequirementsPack.Ads)
            partCard = DrawCardProcedure.Instance.GetHotOffersPartCardRV();
        else
            partCard = DrawCardProcedure.Instance.GetHotOffersPartCardGem();

        return partCard;
    }

    private void GeneratePartCardProcedure()
    {
        if (m_HotOffersItemSO.ReducedValue.RequirementsPack == RequirementsPack.Ads)
        {
            DrawCardProcedure.Instance.GenerateHotOffersCardRV(
                (int)m_HotOffersItemSO.ReducedValue.Value,
                m_HotOffersItemSO.BonusRateCardProcedure,
                m_HotOffersItemSO.NewCardRateCardProcedure);
        }
        else
        {
            DrawCardProcedure.Instance.GenerateHotOffersCardGem(
                (int)m_HotOffersItemSO.ReducedValue.Value,
                m_HotOffersItemSO.BonusRateCardProcedure,
                m_HotOffersItemSO.NewCardRateCardProcedure);
        }
    }

#if UNITY_EDITOR

    [Button]
    public void TestPro()
    {
        GeneratePartCardProcedure();
    }
#endif
}
