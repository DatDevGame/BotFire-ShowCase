using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PackReward;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using HightLightDebug;
using LatteGames.Monetization;
using HyrphusQ.Events;

public enum LinkRewardState
{
    InProgress,
    WaitingClaim,
    Claimed
}
public class PBLinkRewardCellUI : PackRewardUI
{
    public int Index { get; set; }
    public bool IsPromoted { get; set; }
    public bool IsShop { get; set; }
    public AdsLocation AdsLocation
    {
        set => m_ClaimRVBtn.Location = value;
    }

    public Action<LinkRewardState> OnChangeLinkRewardStateAction = delegate { };
    public PBLinkRewardCellSO LinkRewardPackSO => m_PBLinkRewardCellSO;
    public LinkRewardState LinkRewardState
    {
        get => m_LinkRewardState;
        set
        {
            if (value != m_LinkRewardState)
            {
                m_LinkRewardState = value;
                OnChangeLinkRewardStateAction.Invoke(m_LinkRewardState);
                UpdateView();
            }
        }
    }

    [SerializeField, BoxGroup("Ref")] private GameObject m_ButtonGroup;
    [SerializeField, BoxGroup("Ref")] private GameObject m_OnTickV;
    [SerializeField, BoxGroup("Ref")] private Button m_BlockPanel;
    [SerializeField, BoxGroup("Ref")] private GameObject m_PressClickPanel;

    [ShowInInspector, ReadOnly, PropertyOrder(-1)] private LinkRewardState m_LinkRewardState;
    private PBLinkRewardCellSO m_PBLinkRewardCellSO => m_PackRewardSO as PBLinkRewardCellSO;

    public void Load() => UpdateView();

    private void UpdateState()
    {
        LinkRewardState = m_PBLinkRewardCellSO switch
        {
            null => LinkRewardState.InProgress,
            { IsActivePack: false } => LinkRewardState.InProgress,
            { IsActivePack: true, IsClaimed: false } => LinkRewardState.WaitingClaim,
            { IsActivePack: true, IsClaimed: true } => LinkRewardState.Claimed
        };

        if (m_PBLinkRewardCellSO.LastRewardTime > DateTime.Now)
            m_PBLinkRewardCellSO.ResetNow();
    }

    private void UpdateButton()
    {
        if (m_LinkRewardState == LinkRewardState.Claimed)
        {
            m_ButtonGroup.SetActive(false);
            return;
        }

        PackRewardState = m_PackRewardSO.IsGetReward ? PackRewardState.Ready : PackRewardState.Waiting;
        if (m_PackRewardSO.ReducedValue.RequirementsPack == RequirementsPack.Ads)
        {
            AdsButtonHandle();
        }
        else if (m_PackRewardSO.ReducedValue.RequirementsPack == RequirementsPack.Coin || m_PackRewardSO.ReducedValue.RequirementsPack == RequirementsPack.Gem)
        {
            CurrencyButtonHandle();
        }
    }

    protected override void Start()
    {
        m_BlockPanel.onClick.AddListener(() =>
        {
            ToastUI.Show(I2LHelper.TranslateTerm(I2LTerm.Text_ItemIsClaimed));
        });
        m_ClaimBtn.onClick.AddListener(Claim);
        m_ClaimRVBtn.OnRewardGranted += ClaimRVBtn_OnRewardGranted;
        m_ClaimRVBtn.OnStartWatchAds += ClaimRVBtn_OnStartWatchAds;
        m_ClaimRVBtn.OnFailedWatchAds += ClaimRVBtn_OnFailedWatchAds;
        UpdateView();
    }

    protected override void Update()
    {
        if (m_PBLinkRewardCellSO == null) return;
        UpdateState();
        UpdateButton();
    }

    protected override void Claim()
    {
        if (m_PBLinkRewardCellSO.IsClaim())
        {
            m_PBLinkRewardCellSO.Claim(m_ResourceLocationProvider);
            CurrencyManager.Instance.PlayAcquireAnimation(m_PackRewardSO.CurrencyPack.CurrencyType, m_PBLinkRewardCellSO.CurrencyPack.Value, transform.position, null);
        }

        UpdateView();

        if (m_PBLinkRewardCellSO.IsClaimed)
            OnClaimAction?.Invoke();
    }

    protected override void ClaimRVBtn_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData data)
    {
        base.ClaimRVBtn_OnRewardGranted(data);

        #region MonetizationEventCode
        if (m_ClaimRVBtn.Location != AdsLocation.None)
        {
            MonetizationEventCode monetizationEventCode = IsShop ? MonetizationEventCode.LinkRewards_Shop : MonetizationEventCode.LinkRewards_MainUI;
            string name = $"LinkRewards";
            string set = IsPromoted ? "Promoted" : "Normal";
            string rewardID = $"{Index}";
            string location = IsShop ? "Shop" : "MainUI";

            GameEventHandler.Invoke(monetizationEventCode, location, name, set, rewardID);
        }
        else
            Debug.LogWarning("HotOffersFreeGemsUI - AdsLocation Null");

        #endregion
    }

    protected override void UpdateView()
    {
        UpdateState();
        UpdateButton();
        if (m_PackRewardSO == null)
        {
            DebugPro.RedBold($"{this.name} | PackRewardSO Null");
            return;
        }

        bool isAdsRequirement = m_PackRewardSO.ReducedValue.RequirementsPack == RequirementsPack.Ads;
        string rewardText = "+" + (m_PackRewardSO.PackType == PackType.Currency ? m_PackRewardSO.CurrencyPack.Value : m_PackRewardSO.ItemPack.Value).ToString();

        m_PressClickPanel.SetActive(m_LinkRewardState == LinkRewardState.WaitingClaim);
        m_Avatar.sprite = m_PackRewardSO.icon;
        m_ClaimRVBtn.gameObject.SetActive(!isAdsRequirement);
        m_RVBtn.gameObject.SetActive(isAdsRequirement);
        m_RewardText.SetText(rewardText);
        m_RewardText.gameObject.SetActive(m_PackRewardSO.PackType == PackType.Currency);
        m_RVBtnText.SetText($"{m_PackRewardSO.GetTotalAdsWatched()}/{m_PackRewardSO.ReducedValue.Value}");
        m_ButtonGroup.gameObject.SetActive(LinkRewardPackSO.IsActivePack);
        m_OnTickV.SetActive(m_LinkRewardState == LinkRewardState.Claimed);
        m_BlockPanel.gameObject.SetActive(m_LinkRewardState == LinkRewardState.Claimed);
    }

    protected override void AdsButtonHandle()
    {
        if (m_RVBtn == null) return;
        string waitingStateText = $"<size=45><voffset=-10><sprite name=Clock><voffset=-2> {m_PBLinkRewardCellSO.GetRemainingTimeHandle(25, 25)}";
        string readyStateText = $"WATCH AD";
        string adsReducedValueText = $"<size=40><voffset=-17><sprite name=Clock><voffset=-7><size=25>{m_PackRewardSO.GetTotalAdsWatched()}/{m_PackRewardSO.ReducedValue.Value}";
        string adsText = PackRewardState == PackRewardState.Waiting ? waitingStateText : readyStateText;

        m_RVBtn.gameObject.SetActive(!m_PackRewardSO.IsEnoughAds);
        m_ClaimBtnText.gameObject.SetActive(m_PackRewardSO.IsEnoughAds);

        if (m_PBLinkRewardCellSO.IsClaimed)
        {
            m_RVBtn.interactable = false;
            return;
        }

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

    protected override void CurrencyButtonHandle()
    {
        if (m_ClaimBtn == null) return;
        base.CurrencyButtonHandle();
    }
}
