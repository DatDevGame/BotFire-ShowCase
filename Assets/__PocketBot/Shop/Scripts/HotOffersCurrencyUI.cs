using System;
using System.Collections;
using System.Collections.Generic;
using HightLightDebug;
using HyrphusQ.Events;
using LatteGames.Monetization;
using LatteGames.PvP;
using PackReward;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotOffersCurrencyUI : PackRewardUI
{
    public Action OnCompleteRV = delegate { };
    public HotOffersCurrencyPackSO HotOffersCurrencyPackSO => m_HotOffersCurrencyPackSO;

    [SerializeField, BoxGroup("Ref")] protected Image m_AdsIcon;
    [SerializeField, BoxGroup("Ref")] protected Image m_LoadingIcon;
    [SerializeField, BoxGroup("Ref")] protected Image m_MainPanelImage;
    [SerializeField, BoxGroup("Ref")] protected Image m_ClaimBtnImage;
    [SerializeField, BoxGroup("Ref")] protected Image m_RVBtnImage;
    [SerializeField, BoxGroup("Ref")] protected RVReadyStateHandle m_RVReadyStateHandle;
    [SerializeField, BoxGroup("Ref")] protected List<GameObject> m_BlockPanels;
    [SerializeField, BoxGroup("Ref")] protected GrayscaleUI m_ButtonGrayScale;
    [SerializeField, BoxGroup("Ref")] protected Button m_ReclaimButton;

    [SerializeField, BoxGroup("Resource")] protected Color m_ColorTextRVWaiting;
    [SerializeField, BoxGroup("Resource")] protected Color m_ColorTextRVReady;

    [SerializeField, BoxGroup("Data")] protected CurrentHighestArenaVariable m_CurrentHighestArenaVariable;

    private HotOffersCurrencyPackSO m_HotOffersCurrencyPackSO => m_PackRewardSO as HotOffersCurrencyPackSO;

    protected int m_GetCoinReward
    {
        get
        {
            int amout = 0;
            if (m_PackRewardSO is HotOffersCurrencyPackSO hotOffersCurrencyPackSO)
                amout = hotOffersCurrencyPackSO.GetReward();
            return amout;
        }
    }

    protected override void Awake()
    {
        m_CurrentHighestArenaVariable.onValueChanged += CurrentHighestArenaVariable_onValueChanged;
        m_PackRewardSO.CurrencySO_Stand.onValueChanged += CurrencySOStand_OnValueChanged;
        m_PackRewardSO.CurrencySO_Premium.onValueChanged += CurrencySOPremium_OnValueChanged;
        m_RVReadyStateHandle.OnRVReady.AddListener(RVReadyStateHandle_OnRVReady);
        m_RVReadyStateHandle.OnRVNotReady.AddListener(RVNotReadyStateHandle_OnRVNotReady);
        m_ReclaimButton.onClick.AddListener(() =>
        {
            ToastUI.Show(I2LHelper.TranslateTerm(I2LTerm.Text_ItemIsClaimed));
        });
    }

    protected override void OnDestroy()
    {
        m_CurrentHighestArenaVariable.onValueChanged -= CurrentHighestArenaVariable_onValueChanged;
        m_PackRewardSO.CurrencySO_Stand.onValueChanged -= CurrencySOStand_OnValueChanged;
        m_PackRewardSO.CurrencySO_Premium.onValueChanged -= CurrencySOPremium_OnValueChanged;
        m_RVReadyStateHandle.OnRVReady.RemoveListener(RVReadyStateHandle_OnRVReady);
        m_RVReadyStateHandle.OnRVNotReady.RemoveListener(RVNotReadyStateHandle_OnRVNotReady);
    }

    public void Active()
    {
        m_HotOffersCurrencyPackSO.ActivePack();
    }

    public void Reset()
    {
        m_HotOffersCurrencyPackSO.ResetNow();
        UpdateView();
    }

    protected override void Claim()
    {
        if (m_PackRewardSO.PackType == PackType.Currency)
        {
            if (m_PackRewardSO.IsClaim())
            {
                m_PackRewardSO.Claim(m_ResourceLocationProvider);
                GameEventHandler.Invoke(MissionEventCode.OnHotOfferClaimed);
                CurrencyManager.Instance.PlayAcquireAnimation(m_PackRewardSO.CurrencyPack.CurrencyType, m_GetCoinReward, transform.position, null);
            }
        }
        OnClaimAction?.Invoke();
        UpdateView();
    }

    protected override void UpdateView()
    {
        if (m_PackRewardSO == null)
        {
            DebugPro.RedBold($"{this.name} | PackRewardSO Null");
            return;
        }

        TextHandle();
        ImageHandle();
        ActiveHandle();

        if (AdsManager.Instance != null)
        {
            if (AdsManager.Instance.IsReadyRewarded)
                RVReadyStateHandle_OnRVReady();
            else
                RVNotReadyStateHandle_OnRVNotReady();
        }
    }

    protected override void AdsButtonHandle()
    {
        string waitingStateText = $"<size=45><voffset=-10><sprite name=Clock><voffset=-2> {(m_PackRewardSO as HotOffersCurrencyPackSO).GetRemainingTimeHandle(35, 35)}";
        string readyStateText = $"         WATCH AD";
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

    protected override void OnChangeState(PackRewardState state)
    {
        OnChangeStateAction?.Invoke(state);
        UpdateView();
    }

    protected override void ClaimRVBtn_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData data)
    {
        base.ClaimRVBtn_OnRewardGranted(data);
        OnCompleteRV?.Invoke();

        #region MonetizationEventCode
        string adsLocation = "Upgrade UI";
        string offerName = $"FreeCoin";
        GameEventHandler.Invoke(MonetizationEventCode.DailyOffer_FreeCoin, adsLocation, offerName);
        #endregion
    }

    protected virtual void ImageHandle()
    {
        m_Avatar.sprite = m_PackRewardSO.icon;
        m_AdsIcon.gameObject.SetActive(PackRewardState == PackRewardState.Ready);
        m_BlockPanels.ForEach(v => v.SetActive(PackRewardState == PackRewardState.Waiting));
        m_ButtonGrayScale.SetGrayscale(PackRewardState == PackRewardState.Waiting);
    }

    protected virtual void ActiveHandle()
    {
        bool isAdsRequirement = m_PackRewardSO.ReducedValue.RequirementsPack == RequirementsPack.Ads;

        if (m_ClaimRVBtn != null)
            m_ClaimRVBtn.gameObject.SetActive(!isAdsRequirement);

        if (m_RVBtn != null)
            m_RVBtn.gameObject.SetActive(isAdsRequirement);
    }

    protected virtual void TextHandle()
    {
        m_RewardText.SetText($"+{m_GetCoinReward}");
        m_RVBtnText.color = PackRewardState == PackRewardState.Waiting ? m_ColorTextRVWaiting : m_ColorTextRVReady;
    }
    protected virtual void RVNotReadyStateHandle_OnRVNotReady()
    {
        if (CurrencyManager.Instance.GetCurrencySO(CurrencyType.RVTicket).value > 0)
        {
            m_LoadingIcon.gameObject.SetActive(false);
            return;
        }
        m_LoadingIcon.gameObject.SetActive(PackRewardState == PackRewardState.Ready);
    }
    protected virtual void RVReadyStateHandle_OnRVReady()
    {
        m_LoadingIcon.gameObject.SetActive(false);
    }

    private void CurrentHighestArenaVariable_onValueChanged(ValueDataChanged<PvPArenaSO> data)
    {
        UpdateView();
    }

    private void CurrencySOPremium_OnValueChanged(ValueDataChanged<float> obj) => UpdateView();

    private void CurrencySOStand_OnValueChanged(ValueDataChanged<float> obj) => UpdateView();
}
