using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using I2.Loc;
using LatteGames;
using LatteGames.Template;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ThumbnailImageSizeType
{
    Currency,
    Skin,
    PrebuiltBot,
    GachaPack,
}
public class OpenLeagueButton : CanvasGroupVisibility
{
    private const string k_RankChangedDataKey = "OpenLeagueButton";

    [SerializeField]
    private Color m_BelowPromotionZoneTextColor = Color.white;
    [SerializeField]
    private Color m_AbovePromotionZoneTextColor = Color.yellow;
    [SerializeField]
    private LeagueDataSO m_LeagueDataSO;
    [SerializeField]
    private Button m_OpenButton;
    [SerializeField]
    private GameObject m_AvailableGroup, m_UnavailableGroup;
    [SerializeField]
    private GameObject m_HasRewardGroup, m_NoRewardGroup;
    [SerializeField]
    private GameObject m_TimeBox, m_DailyTrophyAnimPoint;
    [SerializeField]
    private LocalizationParamsManager unlockTxt;
    [SerializeField]
    private Image m_CurrentRewardImage;
    [SerializeField]
    private Image m_ArrowImage;
    [SerializeField]
    private CanvasGroup m_ArrowCanvasGroup;
    [SerializeField]
    private TextMeshProUGUI m_RewardQuantityText;
    [SerializeField]
    private TextMeshProUGUI m_NextInText;
    [SerializeField]
    private TextMeshProUGUI m_RankText;
    [SerializeField]
    private TextMeshProUGUI m_RankText_NoReward;
    [SerializeField]
    private TextMeshProUGUI m_TimeLeftToResetLeague;
    [SerializeField]
    private TextMeshProUGUI m_TimeLeftToNextLeague;
    [SerializeField]
    private Material m_MaskColorOnlyMaterial;
    [SerializeField]
    private SerializedDictionary<ThumbnailImageSizeType, float> thumbnailImageSizeDict;

    private bool m_HasReward = false;
    private bool m_IsShowing = false;
    private Vector2 m_OriginalArrowAnchoredPos;
    private List<Graphic> m_Graphics;
    private List<Material> m_OriginalMaterials;

    private TextMeshProUGUI rankText => m_HasReward ? m_RankText : m_RankText_NoReward;

    private ValueDataChanged<int> rankChangedData
    {
        get => m_LeagueDataSO.GetRankChangedData(k_RankChangedDataKey);
        set => m_LeagueDataSO.SetRankChangedData(k_RankChangedDataKey, value);
    }
    private ValueDataChanged<float> crownChangedData
    {
        get => m_LeagueDataSO.GetCrownChangedData(k_RankChangedDataKey);
        set => m_LeagueDataSO.SetCrownChangedData(k_RankChangedDataKey, value);
    }

    private void Awake()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        m_OriginalArrowAnchoredPos = m_ArrowImage.rectTransform.anchoredPosition;
        m_Graphics = new List<Graphic>(GetComponentsInChildren<Graphic>(true));
        m_Graphics.RemoveAll(graphic => graphic is TextMeshProUGUI);
        m_OriginalMaterials = new List<Material>(m_Graphics.Count);
        for (int i = 0; i < m_Graphics.Count; i++)
        {
            m_OriginalMaterials.Add(m_Graphics[i].material);
        }
        GetOnStartShowEvent().Subscribe(() => m_IsShowing = true);
        GetOnStartHideEvent().Subscribe(() => m_IsShowing = false);
    }

    private IEnumerator Start()
    {
        if (!m_LeagueDataSO.IsUnlocked())
        {
            EnableLockedUI(true);
            yield return new WaitUntil(() => m_LeagueDataSO.IsUnlocked());
            EnableLockedUI(false);
        }
        m_LeagueDataSO.AddCrownAndRankChangedDataKey(k_RankChangedDataKey);
        m_OpenButton.onClick.AddListener(() =>
        {
            LeagueManager.leagueDataSO.RefreshBotOnlineStatus();
            LeagueManager.leagueLeaderboardUI.isAutoUpdateView = true;
            LeagueManager.ShowAndStackLeaguePopup(LeagueManager.leagueLeaderboardUI, this);
        });
        UpdateView(!m_LeagueDataSO.IsLeagueOver());
        StartCoroutine(UpdateTimeLeft_CR());
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance[CurrencyType.Crown].onValueChanged += OnNumOfCrownsChanged;
        GameEventHandler.AddActionEvent(PlayModePopup.Enable, Hide);
        GameEventHandler.AddActionEvent(PlayModePopup.Disable, Show);
        m_LeagueDataSO.onPlayerDataChanged += OnPlayerDataChanged;

        if (!m_LeagueDataSO.IsLeagueOver())
        {
            bool isAbleToPlayRankupAnim = rankChangedData.newValue < rankChangedData.oldValue;
            bool isAbleToPlayCrownAcquisitionAnim = crownChangedData.newValue > crownChangedData.oldValue;
            if (isAbleToPlayRankupAnim || isAbleToPlayCrownAcquisitionAnim)
            {
                StartCoroutine(WaitToPlayAnim_CR(() =>
                {
                    if (isAbleToPlayRankupAnim)
                    {
                        PlayRankupAnim();
                    }
                    if (isAbleToPlayCrownAcquisitionAnim)
                    {
                        PlayCrownAcquisitionAnim();
                    }
                }));
            }
        }

        LogEventLeagueRank();
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance[CurrencyType.Crown].onValueChanged -= OnNumOfCrownsChanged;
        GameEventHandler.RemoveActionEvent(PlayModePopup.Enable, Hide);
        GameEventHandler.RemoveActionEvent(PlayModePopup.Disable, Show);
        m_LeagueDataSO.onPlayerDataChanged -= OnPlayerDataChanged;
    }

    private void OnNumOfCrownsChanged(ValueDataChanged<float> eventData)
    {
        if (m_LeagueDataSO.IsLeagueOver())
            return;
        UpdateView(!m_LeagueDataSO.IsLeagueOver());
    }

    private void OnPlayerDataChanged()
    {
        UpdateView(!m_LeagueDataSO.IsLeagueOver());
    }

    private IEnumerator UpdateTimeLeft_CR()
    {
        var timeToDelay = new WaitForSecondsRealtime(1f);
        while (true)
        {
            if (m_IsShowing)
                UpdateTimeLeftText(!m_LeagueDataSO.IsLeagueOver());
            yield return timeToDelay;
        }
    }

    private void LogEventLeagueRank()
    {
        int rank = LeagueManager.leagueDataSO.GetLocalPlayerRank();
        var leaguePlayedTimeToReachRank = LeagueManager.leagueDataSO.LeaguePlayedTimeToReachRank;
        if (leaguePlayedTimeToReachRank.ContainsKey(rank) && !LeagueManager.leagueDataSO.IsDivisionOver())
        {
            if (rank == 1)
            {
                leaguePlayedTimeToReachRank[1].PlayedTimeToReachRank.value = DateTime.Now;
                leaguePlayedTimeToReachRank[2].PlayedTimeToReachRank.value = DateTime.Now;
                leaguePlayedTimeToReachRank[3].PlayedTimeToReachRank.value = DateTime.Now;
            }
            else if (rank == 2)
            {
                leaguePlayedTimeToReachRank[2].PlayedTimeToReachRank.value = DateTime.Now;
                leaguePlayedTimeToReachRank[3].PlayedTimeToReachRank.value = DateTime.Now;
            }
            else
            {
                leaguePlayedTimeToReachRank[rank].PlayedTimeToReachRank.value = DateTime.Now;
            }
        }
    }

    public void UpdateTimeLeftText(bool isAvailable)
    {
        if (isAvailable)
        {
            m_TimeLeftToResetLeague.SetText(m_LeagueDataSO.timeLeftUntilDivisionEnds.GetRemainingTimeInShort());
        }
        else
        {
            m_TimeLeftToNextLeague.SetText(m_LeagueDataSO.timeLeftUntilLeagueEnds.ToRemainingTime(2));
        }
    }

    private void EnableLockedUI(bool isEnable)
    {
        if (isEnable)
        {
            m_OpenButton.interactable = false;
            m_AvailableGroup.SetActive(false);
            m_UnavailableGroup.SetActive(true);
            m_TimeBox.SetActive(false);
            unlockTxt.gameObject.SetActive(true);
            unlockTxt.SetParameterValue("Value", $"{m_LeagueDataSO.configLeague.unlockTrophyThreshold}<size=45><voffset=-5>{CurrencyManager.Instance[CurrencyType.Medal].TMPSprite}");
        }
        else
        {
            m_OpenButton.interactable = true;
            m_AvailableGroup.SetActive(true);
            m_UnavailableGroup.SetActive(false);
            m_TimeBox.SetActive(true);
            unlockTxt.gameObject.SetActive(false);
        }
    }

    private IEnumerator WaitToPlayAnim_CR(Action callback, float maxTime = 0.4f)
    {
        float t = 0f;
        while (t < maxTime)
        {
            if (LeagueDataSO.IsOnMainScreen())
                t += Time.deltaTime;
            else
                t = 0f;
            yield return null;
        }
        callback();
    }

    [Button]
    public void UpdateView(bool isAvailable)
    {
        if (isAvailable)
        {
            // Check if player has reward or not
            RewardGroupInfo rewardInfo = m_LeagueDataSO.GetCurrentDivision().GetRewardInfoByRank(m_LeagueDataSO.GetLocalPlayerRank());
            m_HasReward = rewardInfo != null;

            // Update rank text
            int rank = m_LeagueDataSO.GetLocalPlayerRank();
            var rankContent = m_HasReward ? $"#{rank}" : $"Rank #{rank}";
            rankText.SetText(rankContent);
            rankText.color = m_LeagueDataSO.GetCurrentDivision().IsGetPromotedByRank(rank) ? m_AbovePromotionZoneTextColor : m_BelowPromotionZoneTextColor;
            rankText.gameObject.SetActive(true);
            m_NextInText.gameObject.SetActive(false);

            if (m_HasReward)
            {
                int rewardQuantity = Const.IntValue.Invalid;
                Sprite rewardIcon = null;
                if (rewardInfo.generalItems != null)
                {
                    rewardQuantity = (int)rewardInfo.generalItems.Values.First().value;
                    ItemSO rewardItemSO = rewardInfo.generalItems.Keys.First();
                    if (rewardItemSO is PBGachaPack pbGachaPack)
                    {
                        rewardIcon = pbGachaPack.GetMiniThumbnailImage();
                        m_CurrentRewardImage.rectTransform.sizeDelta = Vector2.one * thumbnailImageSizeDict[ThumbnailImageSizeType.GachaPack];
                    }
                    else
                    {
                        rewardIcon = rewardItemSO.GetThumbnailImage();
                        m_CurrentRewardImage.rectTransform.sizeDelta = Vector2.one * thumbnailImageSizeDict[ThumbnailImageSizeType.PrebuiltBot];
                    }
                }
                else if (rewardInfo.currencyItems != null)
                {
                    rewardQuantity = (int)rewardInfo.currencyItems.Values.First().value;
                    rewardIcon = m_LeagueDataSO.configLeaderboardUI.GetIconByCurrencyType(rewardInfo.currencyItems.Keys.First());
                    m_CurrentRewardImage.rectTransform.sizeDelta = Vector2.one * thumbnailImageSizeDict[ThumbnailImageSizeType.Currency];
                }
                m_CurrentRewardImage.gameObject.SetActive(true);
                m_CurrentRewardImage.sprite = rewardIcon;
                m_RewardQuantityText.gameObject.SetActive(rewardQuantity > Const.IntValue.One);
                m_RewardQuantityText.SetText($"x{rewardQuantity}");
            }
            else
            {
                m_CurrentRewardImage.gameObject.SetActive(false);
            }
            m_OpenButton.interactable = true;
            m_AvailableGroup.SetActive(true);
            m_UnavailableGroup.SetActive(false);
            m_HasRewardGroup.SetActive(m_HasReward);
            m_NoRewardGroup.SetActive(!m_HasReward);
        }
        else
        {
            rankText.gameObject.SetActive(false);
            m_NextInText.gameObject.SetActive(true);
            m_CurrentRewardImage.gameObject.SetActive(false);
            m_OpenButton.interactable = false;
            m_TimeBox.SetActive(true);
            unlockTxt.gameObject.SetActive(false);
            m_AvailableGroup.SetActive(false);
            m_UnavailableGroup.SetActive(true);
        }
        UpdateTimeLeftText(isAvailable);
    }

    [Button]
    public void PlayCrownAcquisitionAnim(float numOfCrowns = 8f)
    {
        CurrencyManager.Instance.PlayAcquireAnimation(CurrencyType.Crown, numOfCrowns, new Vector3(Screen.width / 2f, Screen.height / 2f, 0f), m_DailyTrophyAnimPoint.transform.position, callback: OnAllMoveCompleted, onStartEmission: OnEmissionStarted, onEachMoveComplete: OnEachMoveCompleted);
        crownChangedData = default;

        void OnEmissionStarted(float _)
        {
            foreach (var graphic in m_Graphics)
            {
                graphic.material = m_MaskColorOnlyMaterial;
            }
            m_MaskColorOnlyMaterial.SetFloat("_ColorWeight", 0f);
        }

        void OnEachMoveCompleted(float _)
        {
            SoundManager.Instance.PlaySFX(GeneralSFX.UIFillUpPremiumCurrency);
            HapticManager.Instance.PlayFlashHaptic(HapticTypes.HeavyImpact);
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * 0.1f, 0.25f, 5, 1f);
            m_MaskColorOnlyMaterial.DOKill(true);
            m_MaskColorOnlyMaterial.SetFloat("_ColorWeight", 0.5f);
            m_MaskColorOnlyMaterial.DOFloat(0f, Shader.PropertyToID("_ColorWeight"), 0.25f);
        }

        void OnAllMoveCompleted()
        {
            for (int i = 0; i < m_Graphics.Count; i++)
            {
                m_Graphics[i].material = m_OriginalMaterials[i];
            }
        }
    }

    [Button]
    public void PlayRankupAnim(float distance = 50f, float arrowMovingDuration = 0.5f, Ease ease = Ease.OutQuad, int loops = 3, float fadeOutDuration = 1f)
    {
        m_ArrowCanvasGroup.FadeIn(0f);
        m_ArrowImage.rectTransform.DOKill();
        m_ArrowImage.rectTransform.anchoredPosition = m_OriginalArrowAnchoredPos;
        m_ArrowImage.rectTransform
            .DOAnchorPos(m_ArrowImage.rectTransform.anchoredPosition + Vector2.up * distance, arrowMovingDuration, false)
            .SetEase(ease)
            .SetLoops(loops, LoopType.Restart)
            .OnComplete(() => m_ArrowCanvasGroup.FadeOut(fadeOutDuration));
        rankText.transform.localScale = Vector2.one * 2.5f;
        rankText.transform.DOScale(Vector3.one, AnimationDuration.SSHORT);
        rankChangedData = default;
    }
}