using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BattleRoyaleLeaderboardRow : MonoBehaviour
{
    [Serializable]
    public class PlayerInfoUI
    {
        [SerializeField]
        private Image m_AvatarImage;
        [SerializeField]
        private Image m_NationalFlagImage;
        [SerializeField]
        private TextMeshProUGUI m_PlayerNameText;
        [SerializeField]
        private TextMeshProUGUI m_NumOfTrophiesText;

        public void Init(PBPvPMatch pvpMatch, PersonalInfo personalInfo)
        {
            m_AvatarImage.sprite = personalInfo.avatar;
            m_NationalFlagImage.sprite = personalInfo.nationalFlag;
            m_PlayerNameText.SetText(personalInfo.name);
            m_NumOfTrophiesText.SetText(personalInfo.isLocal 
                ? personalInfo.GetTotalNumOfPoints().ToRoundedText() 
                : (pvpMatch.GetLocalPlayerInfo().personalInfo.GetTotalNumOfPoints() + Random.Range(-0.1f, 0.1f) * pvpMatch.GetLocalPlayerInfo().personalInfo.GetTotalNumOfPoints()).ToRoundedText());
        }
    }

    [SerializeField]
    private Image m_RankingIconImage;
    [SerializeField]
    private TextMeshProUGUI m_RankingText;
    [SerializeField]
    private PlayerInfoUI m_PlayerInfoUI;
    [SerializeField]
    private Image m_BoxIconImage;
    [SerializeField]
    private TextMeshProUGUI m_CoinsText;
    [SerializeField]
    private TextMeshProUGUI m_TrophiesText;
    [SerializeField]
    private EZAnimVisibility m_EzAnimVisibility;
    [SerializeField]
    private BattleRoyaleLeaderboardDataSO m_DataSO;

    public PBPlayerInfo playerInfo { get; set; }

    private string ToSign(bool isVictory, float amount)
    {
        if (Mathf.Approximately(amount, 0f))
            return string.Empty;
        return isVictory ? "+" : "-";
    }

    public void Init(PBPvPMatch pvpMatch, PBPlayerInfo playerInfo)
    {
        var isVictory = pvpMatch.CheckVictoryStatus(playerInfo);
        var rank = pvpMatch.GetRank(playerInfo);

        m_RankingIconImage.sprite = m_DataSO.config.rankingToIconDictionary.Get(rank);
        m_RankingIconImage.gameObject.SetActive(isVictory);
        m_RankingText.color = m_DataSO.config.rankingToTextColorDictionary.Get(rank);
        m_RankingText.SetText(rank.ToString());
        m_PlayerInfoUI.Init(pvpMatch, playerInfo.personalInfo);
        if (rank == 1)
        {
            m_BoxIconImage.sprite = playerInfo.isLocal ? m_DataSO.config.GetBoxIcon(FindObjectOfType<PBPvPGameOverUI>().GetRandomGachaPack(pvpMatch.arenaSO)) : m_DataSO.config.GetRandomBoxIcon();
            m_BoxIconImage.gameObject.SetActive(true);
        }
        else
        {
            m_BoxIconImage.gameObject.SetActive(false);
        }

        var coinsAmount = (pvpMatch.arenaSO as PBPvPArenaSO).GetBattleRoyaleCoinReward(rank);

        var trophiesAmount =
            isVictory ? pvpMatch.arenaSO.TryGetReward(out CurrencyRewardModule medalReward, item => item.CurrencyType == CurrencyType.Medal) ?
            medalReward.Amount * PBPvPGameOverUI.GetRewardMultiplier(pvpMatch.mode, CurrencyType.Medal, pvpMatch.GetRank(playerInfo)) : 0f :
            pvpMatch.arenaSO.TryGetPunishment(out CurrencyPunishmentModule medalPunishment) ? medalPunishment.amount * PBPvPGameOverUI.GetLoseTrophyMultiplier(pvpMatch.mode, pvpMatch.GetRank(playerInfo)) : 0f;

        m_CoinsText.SetText($"{ToSign(isVictory, coinsAmount)}{coinsAmount.ToRoundedText()}");
        m_TrophiesText.color = isVictory ? m_DataSO.config.winTrophyTextColor : m_DataSO.config.loseTrophyTextColor;
        m_TrophiesText.SetText($"{ToSign(isVictory, trophiesAmount)}{trophiesAmount.ToRoundedText()}");
        this.playerInfo = playerInfo;
    }

    [Button]
    public void Show()
    {
        gameObject.SetActive(true);
        m_EzAnimVisibility.Show();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}