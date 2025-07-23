using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeagueRewardUI : MonoBehaviour
{
    public enum RewardType
    {
        Currency,
        Box,
        PrebuiltBot,
    }

    [SerializeField]
    private Image m_CurrencyRewardIconImage;
    [SerializeField]
    private Image m_BoxRewardIconImage;
    [SerializeField]
    private Image m_PrebuiltBotRewardIconImage;
    [SerializeField]
    private TextMeshProUGUI m_RewardQuantityText;

    public RewardType rewardType
    {
        get
        {
            if (m_CurrencyRewardIconImage.gameObject.activeSelf)
                return RewardType.Currency;
            if (m_BoxRewardIconImage.gameObject.activeSelf)
                return RewardType.Box;
            return RewardType.PrebuiltBot;
        }
    }
    public Sprite rewardIcon
    {
        get
        {
            switch (rewardType)
            {
                case RewardType.Currency:
                    return m_CurrencyRewardIconImage.sprite;
                case RewardType.Box:
                    return m_BoxRewardIconImage.sprite;
                default:
                    return m_PrebuiltBotRewardIconImage.sprite;
            }
        }
    }
    public int rewardQuantity
    {
        get
        {
            return int.Parse(m_RewardQuantityText.text.Replace($"x", ""));
        }
    }
    public RectTransform rectTransform => transform as RectTransform;

    public void UpdateView(Sprite rewardIcon, int rewardQuantity, RewardType rewardType)
    {
        switch (rewardType)
        {
            case RewardType.Currency:
                if (m_CurrencyRewardIconImage != null)
                    m_CurrencyRewardIconImage.sprite = rewardIcon;
                break;
            case RewardType.Box:
                if (m_BoxRewardIconImage != null)
                    m_BoxRewardIconImage.sprite = rewardIcon;
                break;
            case RewardType.PrebuiltBot:
                if (m_PrebuiltBotRewardIconImage != null)
                    m_PrebuiltBotRewardIconImage.sprite = rewardIcon;
                break;
            default:
                break;
        }
        m_CurrencyRewardIconImage?.gameObject.SetActive(rewardType == RewardType.Currency);
        m_BoxRewardIconImage?.gameObject.SetActive(rewardType == RewardType.Box);
        m_PrebuiltBotRewardIconImage?.gameObject.SetActive(rewardType == RewardType.PrebuiltBot);
        if (m_RewardQuantityText != null)
        {
            m_RewardQuantityText.SetText($"x{rewardQuantity}");
            m_RewardQuantityText.gameObject.SetActive(rewardQuantity > Const.IntValue.One);
        }
    }

    public void UpdateViewWithBestReward(RewardGroupInfo bestRewardInfo)
    {
        List<ItemSO> sortedRewardSOs = bestRewardInfo.generalItems.Keys.OrderByDescending(rewardSO =>
        {
            if (rewardSO is PBChassisSO)
                return 100;
            if (rewardSO is PBGachaPack gachaPack)
                return (int)LeagueManager.leagueDataSO.gachaPackManagerSO.GetGachaPackRarity(gachaPack);
            return -1;
        }).ToList();
        ItemSO reward = sortedRewardSOs[0];
        int quantity = (int)bestRewardInfo.generalItems[reward].value;
        if (reward is PBGachaPack pbGachaPack)
        {
            UpdateView(pbGachaPack.GetMiniThumbnailImage(), quantity, RewardType.Box);
        }
        else
        {
            UpdateView(reward.GetThumbnailImage(), quantity, RewardType.PrebuiltBot);
        }
    }
}