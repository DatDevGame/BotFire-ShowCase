using System;
using HightLightDebug;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PackReward
{
    public enum PackRewardState
    {
        Ready,
        Waiting
    }

    public class PackRewardUI : MonoBehaviour
    {
        public Action OnStartReward = delegate { };
        public Action OnClaimAction = delegate { };
        public Action OnFailedReward = delegate { };

        public PackRewardState PackRewardState
        {
            get => m_PackRewardState;
            set
            {
                if (value != m_PackRewardState)
                {
                    m_PackRewardState = value;
                    OnChangeState(m_PackRewardState);
                }
            }
        }
        public Action<PackRewardState> OnChangeStateAction = delegate { };

        [SerializeField, BoxGroup("Ref")] protected Image m_Avatar;
        [SerializeField, BoxGroup("Ref")] protected TMP_Text m_RewardText;
        [SerializeField, BoxGroup("Ref")] protected TMP_Text m_ClaimBtnText;
        [SerializeField, BoxGroup("Ref")] protected TMP_Text m_RVBtnText;
        [SerializeField, BoxGroup("Ref")] protected Button m_ClaimBtn;
        [SerializeField, BoxGroup("Ref")] protected Button m_RVBtn;

        [SerializeField, BoxGroup("RV")] protected RVButtonBehavior m_ClaimRVBtn;
        [SerializeField, BoxGroup("RV")] protected ResourceLocationProvider m_ResourceLocationProvider;

        [SerializeField, BoxGroup("Data")] protected PackRewardSO m_PackRewardSO;

        protected PackRewardState m_PackRewardState;

        protected void ClaimRVBtn_OnStartWatchAds() => OnStartReward?.Invoke();
        protected void ClaimRVBtn_OnFailedWatchAds() => OnFailedReward?.Invoke();

        protected virtual void Awake() { }

        protected virtual void OnDestroy() { }

        protected virtual void Start()
        {
            m_ClaimBtn.onClick.AddListener(Claim);
            m_ClaimRVBtn.OnRewardGranted += ClaimRVBtn_OnRewardGranted;
            m_ClaimRVBtn.OnStartWatchAds += ClaimRVBtn_OnStartWatchAds;
            m_ClaimRVBtn.OnFailedWatchAds += ClaimRVBtn_OnFailedWatchAds;
            UpdateView();
        }

        protected virtual void Update()
        {
            if (m_PackRewardSO == null) return;

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

        public virtual void SetUpPack(PackRewardSO packRewardSO)
        {
            m_PackRewardSO = packRewardSO;
            UpdateView();
        }

        protected virtual void AdsButtonHandle()
        {
            string waitingStateText = $"{m_PackRewardSO.RemainingTimeInMinute}";
            string readyStateText = $"WATCH AD";
            string adsReducedValueText = $"{m_PackRewardSO.GetTotalAdsWatched()}/{m_PackRewardSO.ReducedValue.Value}";
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
                m_RVBtnText.SetText(m_PackRewardSO.ReducedValue.Value > 1 ? adsReducedValueText : adsText);
                m_RVBtn.interactable = !m_PackRewardSO.IsEnoughAds;

                m_ClaimBtn.interactable = m_PackRewardSO.IsClaim();
                m_ClaimBtnText.SetText("CLAIM");
            }
        }

        protected virtual void CurrencyButtonHandle()
        {
            if (PackRewardState == PackRewardState.Waiting)
            {
                m_ClaimBtn.interactable = false;
                m_ClaimBtnText.SetText(m_PackRewardSO.RemainingTimeInMinute);
            }
            else
            {
                string rewardText = $"{m_PackRewardSO.ReducedValue.Value}";
                m_ClaimBtn.interactable = m_PackRewardSO.IsClaim();
                m_ClaimBtnText.SetText(rewardText);
            }
        }

        protected virtual void ClaimRVBtn_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData data)
        {
            if (m_PackRewardSO == null)
            {
                DebugPro.RedBold($"{this.name} | PackRewardSO Null");
                return;
            }
            m_PackRewardSO.SetAdsValue();
            UpdateView();
            Claim();
        }

        protected virtual void UpdateView()
        {
            if (m_PackRewardSO == null)
            {
                DebugPro.RedBold($"{this.name} | PackRewardSO Null");
                return;
            }

            bool isAdsRequirement = m_PackRewardSO.ReducedValue.RequirementsPack == RequirementsPack.Ads;
            string rewardText = (m_PackRewardSO.PackType == PackType.Currency ? m_PackRewardSO.CurrencyPack.Value : m_PackRewardSO.ItemPack.Value).ToString();
            m_Avatar.sprite = m_PackRewardSO.icon;
            m_ClaimRVBtn.gameObject.SetActive(!isAdsRequirement);
            m_RVBtn.gameObject.SetActive(isAdsRequirement);
            m_RewardText.SetText(rewardText);
            m_RVBtnText.SetText($"{m_PackRewardSO.GetTotalAdsWatched()}/{m_PackRewardSO.ReducedValue.Value}");
        }

        protected virtual void Claim()
        {
            OnClaimAction?.Invoke();
            m_PackRewardSO.Claim(m_ResourceLocationProvider);
            UpdateView();
        }

        protected virtual void OnChangeState(PackRewardState state) { OnChangeStateAction?.Invoke(state); }
    }
}
