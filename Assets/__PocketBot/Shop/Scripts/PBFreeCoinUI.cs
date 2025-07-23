using System;
using HightLightDebug;
using HyrphusQ.Events;
using LatteGames.Monetization;
using LatteGames.PvP;
using PackReward;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PBFreeCoinUI : PackRewardUI
{
    public Action OnCompleteRV = delegate { };

    [SerializeField, BoxGroup("Ref")] protected Image m_MainPanelImage;
    [SerializeField, BoxGroup("Ref")] protected Image m_ClaimBtnImage;
    [SerializeField, BoxGroup("Ref")] protected Image m_RVBtnImage;
    [SerializeField, BoxGroup("Ref")] protected EventTrigger m_EventTriggerRVBtn;
    [SerializeField, BoxGroup("Ref")] protected AutoShinyEffect m_AutoShinyEffectRVButton;

    [SerializeField, BoxGroup("Resource")] protected Sprite m_OnCoinCard;
    [SerializeField, BoxGroup("Resource")] protected Sprite m_OffCoinCard;
    [SerializeField, BoxGroup("Resource")] protected Sprite m_RVOnButtonSprite;
    [SerializeField, BoxGroup("Resource")] protected Sprite m_RVOffButtonSprite;
    [SerializeField, BoxGroup("Resource")] protected Color m_ColorTextRVWaiting;
    [SerializeField, BoxGroup("Resource")] protected Color m_ColorTextRVReady;

    [SerializeField, BoxGroup("Data")] protected CurrentHighestArenaVariable m_CurrentHighestArenaVariable;

    protected int m_GetCoinReward
    {
        get
        {
            int amout = 0;
            if (m_PackRewardSO is PBPackFreeCoinFollowingArenaSO pbPackFreeCoin)
                amout = pbPackFreeCoin.GetRewardArena();
            return amout;
        }
    }

    protected override void Awake()
    {
        //
        m_PackRewardSO.ActivePack();

        m_CurrentHighestArenaVariable.onValueChanged += CurrentHighestArenaVariable_onValueChanged;
        m_PackRewardSO.CurrencySO_Stand.onValueChanged += CurrencySOStand_OnValueChanged;
        m_PackRewardSO.CurrencySO_Premium.onValueChanged += CurrencySOPremium_OnValueChanged;
    }

    protected override void Start()
    {
        m_ClaimBtn.onClick.AddListener(Claim);
        m_ClaimRVBtn.OnRewardGranted += ClaimRVBtn_OnRewardGranted;

        UpdateView();
    }

    protected override void Claim()
    {
        if (m_PackRewardSO.PackType == PackType.Currency)
        {
            if (m_PackRewardSO.IsClaim())
            {
                m_PackRewardSO.Claim(m_ResourceLocationProvider);
                CurrencyManager.Instance.PlayAcquireAnimation(m_PackRewardSO.CurrencyPack.CurrencyType, m_GetCoinReward, transform.position, null);
            }
        }
        UpdateView();
    }

    protected override void UpdateView()
    {
        if (m_PackRewardSO == null)
        {
            DebugPro.RedBold($"{this.name} | PackRewardSO Null");
            return;
        }

        m_AutoShinyEffectRVButton.enabled = PackRewardState == PackRewardState.Ready;

        TextHandle();
        ImageHandle();
        ActiveHandle();
    }

    protected override void AdsButtonHandle()
    {
        string waitingStateText = $"<size=42><voffset=-8><sprite name=Clock><voffset=-3><size=35> {(m_PackRewardSO as PBPackFreeCoinFollowingArenaSO).GetRemainingTimeHandle(32, 28)}";
        string readyStateText = $" <size=60><voffset=-4><sprite name=Ads><voffset=8><size=32>WATCH AD";
        string adsReducedValueText = $"<size=50><voffset=-17><sprite name=Clock><voffset=-7><size=35>{m_PackRewardSO.GetTotalAdsWatched()}/{m_PackRewardSO.ReducedValue.Value}";
        string adsText = PackRewardState == PackRewardState.Waiting ? waitingStateText : readyStateText;

        m_RVBtn.gameObject.SetActive(!m_PackRewardSO.IsEnoughAds);
        m_ClaimBtnText.gameObject.SetActive(m_PackRewardSO.IsEnoughAds);

        if (PackRewardState == PackRewardState.Waiting)
        {
            m_RVBtn.interactable = false;
            m_RVBtnText.SetText(adsText);

            m_EventTriggerRVBtn.enabled = false;
        }
        else
        {
            m_RVBtn.interactable = !m_PackRewardSO.IsEnoughAds;
            m_RVBtnText.SetText(m_PackRewardSO.ReducedValue.Value > 1 ? adsReducedValueText : adsText);

            m_EventTriggerRVBtn.enabled = true;
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
        m_Avatar.sprite = PackRewardState == PackRewardState.Waiting ? m_OffCoinCard : m_OnCoinCard;
        m_ClaimBtnImage.sprite = PackRewardState == PackRewardState.Waiting ? m_RVOffButtonSprite : m_RVOnButtonSprite;
        m_RVBtnImage.sprite = PackRewardState == PackRewardState.Waiting ? m_RVOffButtonSprite : m_RVOnButtonSprite;
    }

    protected virtual void ActiveHandle()
    {
        bool isAdsRequirement = m_PackRewardSO.ReducedValue.RequirementsPack == RequirementsPack.Ads;

        if(m_ClaimRVBtn != null)
            m_ClaimRVBtn.gameObject.SetActive(!isAdsRequirement);

        if(m_RVBtn != null)
            m_RVBtn.gameObject.SetActive(isAdsRequirement);
    }

    protected virtual void TextHandle()
    {
        m_RewardText.SetText($"+{m_GetCoinReward}");
        m_RVBtnText.color = PackRewardState == PackRewardState.Waiting ? m_ColorTextRVWaiting : m_ColorTextRVReady;
    }

    private void CurrentHighestArenaVariable_onValueChanged(ValueDataChanged<PvPArenaSO> data)
    {
        UpdateView();
    }

    private void CurrencySOPremium_OnValueChanged(ValueDataChanged<float> obj) => UpdateView();

    private void CurrencySOStand_OnValueChanged(ValueDataChanged<float> obj) => UpdateView();
}
