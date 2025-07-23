using System.Collections;
using System.Collections.Generic;
using PackReward;
using UnityEngine;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using GachaSystem.Core;
using HyrphusQ.Events;
using TMPro;
using HyrphusQ.SerializedDataStructure;
using System.Linq;
using DG.Tweening;
using LatteGames;
using System.Drawing;
using LatteGames.Template;
using LatteGames.Monetization;
using HightLightDebug;

public enum WinStreakState
{
    Unclaimable,
    Claimable,
    Claimed
}
public class WinStreakCellUI : MonoBehaviour
{
    public static Action OnClaimNormalReward;
    public Action OnClaim = delegate { };
    public WinStreakCellSO WinStreakCellSO => m_WinStreakCellSO;
    public WinStreakState WinStreakCurrentState => m_WinStreakCurrentState;
    public ResourceLocationProvider StandSourceResourceLocationProvider => m_StandSourceResourceLocationProvider;

    [SerializeField, BoxGroup("Config")] private bool m_IsPremium;
    [SerializeField, BoxGroup("Config")] private ResourceLocationProvider m_StandSourceResourceLocationProvider;
    [SerializeField, BoxGroup("Config")] private ResourceLocationProvider m_PremiumSourceLocationProvider;
    [SerializeField, BoxGroup("Config")] private UnityEngine.Color m_ColorEnable;
    [SerializeField, BoxGroup("Config")] private UnityEngine.Color m_ColorDisable;
    [SerializeField, BoxGroup("Config")] private SerializedDictionary<WinStreakState, CanvasGroupVisibility> m_StateObject;

    [SerializeField, BoxGroup("Ref")] private RVButtonBehavior m_ClaimRV;
    [SerializeField, BoxGroup("Ref")] private SerializedDictionary<RarityType, Sprite> m_BGRarity;
    [SerializeField, BoxGroup("Ref")] private SerializedDictionary<int, Image> m_LevelStreak;
    [SerializeField, BoxGroup("Ref")] private Image m_Avatar;
    [SerializeField, BoxGroup("Ref")] private Image m_AvatarWinStreak;
    [SerializeField, BoxGroup("Ref")] private Image m_BlockPanel;
    [SerializeField, BoxGroup("Ref")] private RectTransform m_VTick;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_StreakIndexText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_RewardText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_ClaimableText;
    [SerializeField, BoxGroup("Ref")] private Button m_ClaimButton;
    [SerializeField, BoxGroup("Ref")] private Button m_ClaimRVButton;

    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_CurrentWinStreak;

    [SerializeField, BoxGroup("Reaching Animation")] private ParticleSystem reachingBadgeVFX;
    [SerializeField, BoxGroup("Reaching Animation")] private RectTransform badgeTransform;
    [SerializeField, BoxGroup("Reaching Animation")] private AnimationCurve reachingScalingCurve;

    [SerializeField, BoxGroup("Object Animation")] private RectTransform m_AvatarRect;
    [SerializeField, BoxGroup("Pos Animation")] private RectTransform m_AvatarOrigin;
    [SerializeField, BoxGroup("Pos Animation")] private RectTransform m_AvatarClaimed;

    [SerializeField] private WinStreakState m_WinStreakCurrentState;

    private WinStreakCellSO m_WinStreakCellSO;

    [HideInInspector] public WinStreakPopupUI winStreakPopupUI;

    private void Awake()
    {
        m_ClaimButton.onClick.AddListener(OnClickButton);
        m_ClaimRV.OnRewardGranted += ClaimRV_OnRewardGranted;
    }

    private void OnDestroy()
    {
        m_ClaimButton.onClick.RemoveListener(OnClickButton);
        m_ClaimRV.OnRewardGranted -= ClaimRV_OnRewardGranted;
    }

    public void Load(WinStreakCellSO winStreakCellSO, int streakIndex, bool isClaimStreak = false)
    {
        m_WinStreakCellSO = winStreakCellSO;
        if (m_WinStreakCellSO == null)
        {
            Debug.LogError("WinStreakCellSO is Null");
            return;
        }

        //Load State
        if (m_WinStreakCellSO.IsClaimed)
            m_WinStreakCurrentState = WinStreakState.Claimed;
        else
            m_WinStreakCurrentState = isClaimStreak ? WinStreakState.Claimable : WinStreakState.Unclaimable;

        if (m_WinStreakCurrentState == WinStreakState.Claimable)
            m_WinStreakCellSO.Claimable();

        //Handle Block Panel
        m_BlockPanel.gameObject.SetActive(m_WinStreakCurrentState == WinStreakState.Unclaimable || m_WinStreakCurrentState == WinStreakState.Claimed);

        //Other
        bool isFilledStreak = m_WinStreakCurrentState == WinStreakState.Claimable || m_WinStreakCurrentState == WinStreakState.Claimed;

        for (int i = 0; i < m_LevelStreak.Count; i++)
        {
            if (m_LevelStreak.ElementAt(i).Key <= streakIndex)
                m_LevelStreak.ElementAt(i).Value.gameObject.SetActive(true);
            else
                m_LevelStreak.ElementAt(i).Value.gameObject.SetActive(false);
        }

        for (int x = 0; x < m_LevelStreak.Count; x++)
        {
            //m_StreakIndexText.DOColor(isFilledStreak ? m_ColorEnable : m_ColorDisable, isFilledStreak ? 0.5f : 0);
            m_LevelStreak.ElementAt(x).Value
                .DOColor(isFilledStreak ? m_ColorEnable : m_ColorDisable, isFilledStreak ? 0 : 0)
                .OnComplete(() =>
                {
                    OnPanelFollowingState(m_WinStreakCurrentState);
                    HandleActiveObject();

                    m_ClaimableText.gameObject.SetActive(!m_WinStreakCellSO.IsPremium);
                    m_ClaimButton.gameObject.SetActive(!m_WinStreakCellSO.IsPremium);
                    m_ClaimRVButton.gameObject.SetActive(m_WinStreakCellSO.IsPremium);
                    m_ClaimButton.interactable = m_WinStreakCurrentState == WinStreakState.Claimable;
                    m_ClaimRVButton.interactable = m_WinStreakCurrentState == WinStreakState.Claimable;
                    m_StreakIndexText.SetText(streakIndex.ToString());
                    float rewardCount = m_WinStreakCellSO.PackType == PackType.Currency ? m_WinStreakCellSO.CurrencyPack.Value : m_WinStreakCellSO.ItemPack.Value;
                    m_RewardText.SetText($"+{rewardCount}");
                    m_Avatar.sprite = m_WinStreakCellSO.icon;
                    m_Avatar.color = m_WinStreakCurrentState == WinStreakState.Unclaimable ? m_ColorDisable : m_ColorEnable;
                });
        }

        //Animation
        bool isOriginPos = m_WinStreakCurrentState == WinStreakState.Unclaimable || m_WinStreakCurrentState == WinStreakState.Claimable;
        Vector2 anchoredPositionAnimation = isOriginPos ? m_AvatarOrigin.anchoredPosition : m_AvatarClaimed.anchoredPosition;
        if (m_AvatarRect.anchoredPosition != anchoredPositionAnimation)
        {
            if (m_WinStreakCurrentState == WinStreakState.Unclaimable)
                m_VTick.DOScale(0, 0.2f);

            m_AvatarRect
            .DOAnchorPos(anchoredPositionAnimation, 0.1f)
            .SetEase(Ease.InOutExpo)
            .OnComplete(() =>
            {
                if (m_WinStreakCurrentState == WinStreakState.Claimed)
                {
                    m_AvatarRect.DOPunchScale(Vector3.one * 0.1f, 0.5f);

                    if (winStreakPopupUI.IsShow)
                    {
                        SoundManager.Instance.PlayLoopSFX(PBSFX.UIClaimed, 1);
                    }
                }
                m_VTick
                    .DOScale(m_WinStreakCurrentState == WinStreakState.Claimed ? Vector3.one : Vector3.zero, 0.5f)
                    .SetEase(Ease.InOutSine);
            });
        }
    }

    public void PlayReachingFX()
    {
        badgeTransform.DOScale(1.25f, AnimationDuration.SSHORT).SetEase(reachingScalingCurve).OnComplete(() =>
        {
            if (winStreakPopupUI.IsShow)
            {
                SoundManager.Instance.PlayLoopSFX(PBSFX.UIShiningBadge, 1);
                reachingBadgeVFX.Play();
            }
        });
    }

    private void OnPanelFollowingState(WinStreakState state)
    {
        for (int i = 0; i < m_StateObject.Count; i++)
        {
            if (m_StateObject.ElementAt(i).Key != state)
                m_StateObject.ElementAt(i).Value.Hide();
        }
        m_StateObject[state].Show();
    }

    private void HandleActiveObject()
    {
        if (m_WinStreakCellSO == null)
        {
            Debug.LogError("WinStreakCellSO is null");
            return;
        }

        //Currency
        if (m_WinStreakCellSO.PackType == PackType.Currency)
        {
            m_RewardText.gameObject.SetActive(true);
        }
        else
        {

            if (m_WinStreakCellSO.ItemPack.ItemSO is GachaPack)
            {
                //GachaPack
                m_RewardText.gameObject.SetActive(false);
            }
            else
            {
                //Part
                m_RewardText.gameObject.SetActive(true);
            }

        }
    }

    private void OnClickButton()
    {
        m_WinStreakCellSO.Claim();
        if (m_WinStreakCellSO != null)
        {
            if (m_WinStreakCellSO.PackType == PackType.Currency)
            {
                ResourceLocationProvider resourceLocationProviderWinStreak = m_IsPremium ? m_PremiumSourceLocationProvider : m_StandSourceResourceLocationProvider;
                if (resourceLocationProviderWinStreak == null)
                    DebugPro.WarningAquaBoldItalic("ResourceLocationProvider of WinStreak is Null");

                ProcessRewardGroup(m_WinStreakCellSO.RewardGroupInfo, resourceLocationProviderWinStreak);
            }
            else
            {
                if (m_WinStreakCellSO.ItemPack.ItemSO is GachaPack gachaPack)
                {
                    List<GachaPack> gachaPacks = new List<GachaPack>
                    {
                        {gachaPack}
                    };
                    GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, null, gachaPacks, null, true);

                    #region Firebase Event
                    if (gachaPack != null)
                    {
                        string openType = m_IsPremium ? "RV" : "free";
                        GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, gachaPack, openType);
                    }
                    #endregion

                    #region Design Event
                    string openStatus = "NoTimer";
                    string location = m_IsPremium ? "WinStreakPremium" : "WinStreakStandard";
                    GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
                    #endregion
                }

                if (m_WinStreakCellSO.ItemPack.ItemSO is GachaCard_RandomActiveSkill cardRandomActiveSkill)
                {
                    List<GachaCard> gachaCards = (GachaCardGenerator.Instance as PBGachaCardGenerator).Generate(m_WinStreakCellSO.RewardGroupInfo);
                    GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, gachaCards, null, null, true);

                    #region Source Event
                    try
                    {
                        float skillCardCount = m_WinStreakCellSO.ItemPack.Value;
                        ResourceLocationProvider resourceProvider = m_IsPremium ? m_PremiumSourceLocationProvider : m_StandSourceResourceLocationProvider;
                        GameEventHandler.Invoke(LogSinkSource.SkillCard, skillCardCount, resourceProvider);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
                    }
                    #endregion
                }
            }
        }
        m_ClaimButton.interactable = false;
        OnClaim?.Invoke();
        if (!m_IsPremium)
        {
            OnClaimNormalReward?.Invoke();
        }
    }
    private void ClaimRV_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData obj)
    {
        #region MonetizationEventCode
        int streakMilestone = transform.GetSiblingIndex() + 1;
        string location = "WinStreakUI";
        GameEventHandler.Invoke(MonetizationEventCode.WinStreakPremium, streakMilestone, location);
        #endregion

        OnClickButton();
    }

    #region Create Rewards
    public void ProcessRewardGroup(RewardGroupInfo rewardGroupInfo, IResourceLocationProvider resourceLocationProvider)
    {
        List<GachaCard> cards = GenerateGachaCards(rewardGroupInfo);
        if (cards == null || cards.Count == 0) return;
        AssignResourceProvider(cards, resourceLocationProvider);
        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, cards, null, null);
    }

    private List<GachaCard> GenerateGachaCards(RewardGroupInfo rewardGroupInfo)
    {
        return (GachaCardGenerator.Instance as PBGachaCardGenerator)?.Generate(rewardGroupInfo);
    }

    private void AssignResourceProvider(List<GachaCard> cards, IResourceLocationProvider resourceLocationProvider)
    {
        foreach (var card in cards)
        {
            if (card is GachaCard_Currency gachaCardCurrency)
            {
                gachaCardCurrency.ResourceLocationProvider = resourceLocationProvider;
            }
        }
    }
    #endregion
}
