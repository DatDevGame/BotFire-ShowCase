using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using LatteGames;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeagueLeaderboardRow : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    private Image m_PlayerRankIconImage;
    [SerializeField]
    private TextMeshProUGUI m_PlayerRankText;
    [SerializeField]
    private Image m_PlayerAvatarImage;
    [SerializeField]
    private Image m_NationalFlagImage;
    [SerializeField]
    private Image m_OnlineStatusImage, m_OfflineStatusImage;
    [SerializeField]
    private TextMeshProUGUI m_PlayerNameText;
    [SerializeField]
    private TextMeshProUGUI m_NumOfCrownsText;
    [SerializeField]
    private CanvasGroup m_CanvasGroup;
    [SerializeField]
    private LeagueRewardUI[] m_Rewards;

    private int m_PlayerRank;
    private LeaguePlayer m_Player;

    public bool visibility
    {
        set
        {
            m_CanvasGroup.alpha = value ? 1f : 0f;
        }
    }
    public int playerRank => m_PlayerRank;
    public LeaguePlayer player
    {
        get => m_Player;
        set => m_Player = value;
    }
    public RectTransform rectTransform => transform as RectTransform;

    private LeagueDataSO leagueDataSO => LeagueManager.leagueDataSO;
    private LeagueLeaderboardUI.Config leaderboardConfig => leagueDataSO.configLeaderboardUI;

    public void Initialize(LeaguePlayer player, int rank)
    {
        m_Player = player;
        UpdateView(rank);
    }

    public void UpdateView(int rank)
    {
        if (m_Player == null)
            return;
        transform.SetSiblingIndex(leagueDataSO.configLeaderboardUI.CalcSiblingIndexByRank(rank));
        m_PlayerRank = rank;
        m_PlayerRankIconImage.sprite = leaderboardConfig.GetIconByRank(rank);
        m_PlayerRankIconImage.enabled = m_PlayerRankIconImage.sprite != null;
        m_PlayerRankText.SetText(rank.ToString());
        m_PlayerAvatarImage.sprite = m_Player.avatarThumbnail;
        m_NationalFlagImage.sprite = m_Player.nationalFlag;
        m_OnlineStatusImage.gameObject.SetActive(m_Player.isOnline);
        m_OfflineStatusImage.gameObject.SetActive(!m_Player.isOnline);
        m_PlayerNameText.SetText(m_Player.name);
        m_NumOfCrownsText.SetText(m_Player.numOfCrowns.ToString());
        m_Rewards.ForEach(rewardUI => rewardUI.gameObject.SetActive(false));
        LeagueDivision division = leagueDataSO.GetCurrentDivision(true);
        RewardGroupInfo rewardInfo = division.GetRewardInfoByRank(rank);
        if (rewardInfo != null)
        {
            int rewardIndex = 0;
            if (rewardInfo.generalItems != null)
            {
                List<ItemSO> sortedRewardSOs = rewardInfo.generalItems.Keys.OrderByDescending(rewardSO =>
                {
                    if (rewardSO is PBChassisSO)
                        return 100;
                    if (rewardSO is PBGachaPack gachaPack)
                        return (int)leagueDataSO.gachaPackManagerSO.GetGachaPackRarity(gachaPack);
                    return -1;
                }).ToList();
                foreach (var rewardItemSO in sortedRewardSOs)
                {
                    int rewardQuantity = (int)rewardInfo.generalItems[rewardItemSO].value;
                    bool isBoxReward = rewardItemSO is PBGachaPack;
                    Sprite rewardIcon = isBoxReward ? ((PBGachaPack)rewardItemSO).GetMiniThumbnailImage() : rewardItemSO.GetThumbnailImage();
                    LeagueRewardUI reward = m_Rewards[rewardIndex++];
                    reward.gameObject.SetActive(true);
                    reward.UpdateView(rewardIcon, rewardQuantity, isBoxReward ? LeagueRewardUI.RewardType.Box : LeagueRewardUI.RewardType.PrebuiltBot);
                }
            }
            if (rewardInfo.currencyItems != null)
            {
                int rewardQuantity = (int)rewardInfo.currencyItems.Values.First().value;
                Sprite rewardIcon = leaderboardConfig.GetIconByCurrencyType(rewardInfo.currencyItems.Keys.First());
                LeagueRewardUI reward = m_Rewards[rewardIndex++];
                reward.gameObject.SetActive(true);
                reward.UpdateView(rewardIcon, rewardQuantity, LeagueRewardUI.RewardType.Currency);
            }

            // Update reward position
            m_Rewards[0].rectTransform.anchoredPosition = rewardIndex <= 1 ? leaderboardConfig.leaderboardRowPrefab.m_Rewards[1].rectTransform.anchoredPosition : leaderboardConfig.leaderboardRowPrefab.m_Rewards[0].rectTransform.anchoredPosition;
        }
    }

    public void ScaleUp(Vector2 originalSizeDelta, Action onCompleted = null)
    {
        var originalScaleFactor = leaderboardConfig.rowScaleUpRange.minValue;
        var targetScaleFactor = leaderboardConfig.rowScaleUpRange.maxValue;
        var animConfig = leaderboardConfig.rowScaleUpAnimConfig;
        var layoutElement = GetComponent<LayoutElement>();
        var bgRectTransform = rectTransform.GetChild(0) as RectTransform;
        bgRectTransform.anchorMin = bgRectTransform.anchorMax = Vector2.one * 0.5f;
        bgRectTransform.sizeDelta = originalSizeDelta;
        StartCoroutine(CommonCoroutine.LerpFactor(animConfig.duration, t =>
        {
            var scaleFactor = Mathf.LerpUnclamped(originalScaleFactor, targetScaleFactor, animConfig.curve.Evaluate(t));
            rectTransform.sizeDelta = new Vector2(originalSizeDelta.x, originalSizeDelta.y * scaleFactor);
            layoutElement.preferredHeight = originalSizeDelta.y * scaleFactor;
            bgRectTransform.localScale = Vector3.one * scaleFactor;
            if (t == 1f)
            {
                onCompleted?.Invoke();
            }
        }));
    }

    public void ScaleDown(Vector2 originalSizeDelta, Action onCompleted = null)
    {
        var originalScaleFactor = leaderboardConfig.rowScaleDownRange.minValue;
        var targetScaleFactor = leaderboardConfig.rowScaleDownRange.maxValue;
        var animConfig = leaderboardConfig.rowScaleDownAnimConfig;
        var layoutElement = GetComponent<LayoutElement>();
        var bgRectTransform = rectTransform.GetChild(0) as RectTransform;
        bgRectTransform.anchorMin = bgRectTransform.anchorMax = Vector2.one * 0.5f;
        bgRectTransform.sizeDelta = originalSizeDelta;
        StartCoroutine(CommonCoroutine.LerpFactor(animConfig.duration, t =>
        {
            var scaleFactor = Mathf.LerpUnclamped(originalScaleFactor, targetScaleFactor, animConfig.curve.Evaluate(t));
            rectTransform.sizeDelta = new Vector2(originalSizeDelta.x, originalSizeDelta.y * scaleFactor);
            layoutElement.preferredHeight = originalSizeDelta.y * scaleFactor;
            bgRectTransform.localScale = Vector3.one * scaleFactor;
            if (t == 1f)
            {
                onCompleted?.Invoke();
            }
        }));
    }

    public void FadeOutAndDestroy(float duration)
    {
        m_CanvasGroup.FadeOut(duration, OnCompleted);

        void OnCompleted()
        {
            Destroy(gameObject);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(Vector3.one * 0.95f, 0.15f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(Vector3.one, 0.15f);
    }
}