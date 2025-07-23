using GachaSystem.Core;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.PvP;
using LatteGames.Template;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KDAUIBoard : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_CanvasGroupVisibility;
    [SerializeField, BoxGroup("Ref")] private EZAnimSequence m_EZAnimSequence;
    [SerializeField, BoxGroup("Ref")] private EZAnimVector3 m_ContinueBtnEZ;

    [SerializeField, BoxGroup("Ref Header")] private GameObject m_TitleWin;
    [SerializeField, BoxGroup("Ref Header")] private GameObject m_TitleLose;
    [SerializeField, BoxGroup("Ref Header")] private TMP_Text m_StatsTitleText;
    [SerializeField, BoxGroup("Ref Header")] private Image m_TileBannerImage;

    [SerializeField, BoxGroup("Ref Body")] private List<KDAUIRow> m_TeamAKDARows;
    [SerializeField, BoxGroup("Ref Body")] private List<KDAUIRow> m_TeamBKDARows;

    [SerializeField, BoxGroup("Ref Footer")] private CanvasGroupVisibility m_RewardBoxGroupVisibility;
    [SerializeField, BoxGroup("Ref Footer")] private MultiImageButton m_ContinueButton;
    [SerializeField, BoxGroup("Ref Footer")] private Image m_BoxRewardImage;
    [SerializeField, BoxGroup("Ref Footer")] private TMP_Text m_CoinAmountText;
    [SerializeField, BoxGroup("Ref Footer")] private TMP_Text m_TrophyAmountText;

    [SerializeField, BoxGroup("Resource")] private Sprite m_WinBannerSprite;
    [SerializeField, BoxGroup("Resource")] private Sprite m_LoseBannerSprite;
    [SerializeField, BoxGroup("Resource")] private Sprite m_NoBoxSprite;

    [SerializeField, BoxGroup("Data")] private CurrentHighestArenaVariable m_CurrentHighestArenaVariable;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_SaveArena;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_TeamDeathmatchPPref;
    [SerializeField, BoxGroup("Data")] private PSFTUESO m_PSFTUESO;

    private List<PBRobot> m_AllRobots;
    private GachaPack m_CachedGachaPack;
    private bool m_IsVictory;
    private bool m_IsSurrender;

    private void Awake()
    {
        m_ContinueButton.onClick.AddListener(ContinueButton);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnLeaveInMiddleOfMatch, OnLeaveInMiddleOfMatch);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, BotModelSpawned);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
    }

    private void OnDestroy()
    {
        m_ContinueButton.onClick.RemoveListener(ContinueButton);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnLeaveInMiddleOfMatch, OnLeaveInMiddleOfMatch);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, BotModelSpawned);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
    }

    private void InitData(PBPvPMatch pBPvPMatch)
    {
        m_EZAnimSequence.SetToStart();
        UpdateBody(pBPvPMatch);
        m_EZAnimSequence.Play();
    }
    private void UpdateHeader(int totalKillBlueTeam, int totalKillRedTeam)
    {
        m_TitleWin.SetActive(m_IsVictory);
        m_TitleLose.SetActive(!m_IsVictory);
        m_TileBannerImage.sprite = m_IsVictory ? m_WinBannerSprite : m_LoseBannerSprite;
        m_StatsTitleText.SetText($"{totalKillBlueTeam.ToString("D2")} - {totalKillRedTeam.ToString("D2")}");

    }
    private void UpdateBody(PBPvPMatch pvPMatch)
    {
        List<PBRobot> teamARobots = m_AllRobots
            .Where(v => v.TeamId == 1)
            .OrderByDescending(r => r.PlayerKDA.GetKDA())
            .ToList();

        List<PBRobot> teamBRobots = m_AllRobots
            .Where(v => v.TeamId != 1)
            .OrderByDescending(r => r.PlayerKDA.GetKDA())
            .ToList();

        int minA = Mathf.Min(m_TeamAKDARows.Count, teamARobots.Count);
        for (int i = 0; i < minA; i++)
        {
            m_TeamAKDARows[i].Init(teamARobots[i], pvPMatch);
            m_TeamAKDARows[i].transform.SetSiblingIndex(i);
        }

        int minB = Mathf.Min(m_TeamBKDARows.Count, teamBRobots.Count);
        for (int i = 0; i < minB; i++)
        {
            m_TeamBKDARows[i].Init(teamBRobots[i], pvPMatch);
            m_TeamBKDARows[i].transform.SetSiblingIndex(i);
        }

        int totalKillA = teamARobots.Sum(r => r.PlayerKDA.Kills);
        int totalKillB = teamBRobots.Sum(r => r.PlayerKDA.Kills);

        UpdateHeader(totalKillA, totalKillB);

        int playerLocalIndex = teamARobots
                .Select((r, index) => new { r, index })
                .FirstOrDefault(x => x.r.PersonalInfo.isLocal)?.index ?? -1;
        int rankMineTeam = playerLocalIndex + 1;

        m_RewardBoxGroupVisibility.Show();
        UpdateFooter(rankMineTeam);
    }

    private void UpdateFooter(int rank)
    {
        if (m_IsVictory)
        {
            GachaPack gachaPack = GetRandomGachaPack(m_CurrentHighestArenaVariable.value);
            if (gachaPack != null)
            {
                Sprite gachaSprite = gachaPack.GetThumbnailImage();
                if (gachaSprite == null && gachaPack is PBManualGachaPack manualGachaPack)
                {
                    gachaSprite = manualGachaPack.SimulationFromGachaPack.GetThumbnailImage();
                }
                m_BoxRewardImage.enabled = true;
                m_BoxRewardImage.sprite = gachaSprite;
            }
            if (m_CurrentHighestArenaVariable.value.TryGetReward(out CurrencyRewardModule medalReward, item => item.CurrencyType == CurrencyType.Medal))
            {
                float trophyAmount = medalReward.Amount * GetTrophyRewardMultiplier(rank);
                m_TrophyAmountText.SetText($"+{trophyAmount}");
                CurrencyManager.Instance.Acquire(CurrencyType.Medal, trophyAmount, ResourceLocation.BattlePvP, $"ID");
            }

            if (m_CurrentHighestArenaVariable.value.TryGetReward(out CurrencyRewardModule coinReward, item => item.CurrencyType == CurrencyType.Standard))
            {
                float cointAmount = coinReward.Amount;
                m_CoinAmountText.SetText($"+{cointAmount}");
                CurrencyManager.Instance.Acquire(CurrencyType.Standard, cointAmount, ResourceLocation.BattlePvP, $"ID");
            }

            StartCoroutine(CommonCoroutine.Delay(0.5f, false, () =>
            {
                SoundManager.Instance.PlaySFX(PBSFX.UIWinner);
                SoundManager.Instance.PlaySFX(PBSFX.UIWinFlameThrower);
            }));
        }
        else
        {
            m_BoxRewardImage.enabled = true;
            m_BoxRewardImage.sprite = m_NoBoxSprite;
            m_BoxRewardImage.GetComponent<AutoShinyEffect>().enabled = false;
            if (m_CurrentHighestArenaVariable.value.TryGetPunishment(out CurrencyPunishmentModule medalPunishment, item => item.currencyType == CurrencyType.Medal))
            {
                float trophyAmount = medalPunishment.amount;
                m_TrophyAmountText.SetText($"-{trophyAmount}");
                CurrencyManager.Instance.Spend(CurrencyType.Medal, trophyAmount, ResourceLocation.BattlePvP, $"ID");
            }

            m_CoinAmountText.SetText($"+0");
            StartCoroutine(CommonCoroutine.Delay(0.5f, false, () =>
            {
                SoundManager.Instance.PlaySFX(PBSFX.UIYouLose);
            }));
        }
    }
    private void ContinueButton()
    {
        if (m_IsVictory)
        {
            m_ContinueButton.enabled = false;
            m_ContinueBtnEZ.InversePlay();
            if (m_CachedGachaPack != null)
            {
                GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, null, new List<GachaPack>() { m_CachedGachaPack }, null, false);
            }
        }
        else
        {
            m_ContinueButton.enabled = false;
            ObjectFindCache<PBPvPStateGameController>.Get().LeaveGame();
        }
    }

    private void OnMatchStarted()
    {
        m_TeamDeathmatchPPref.value++;

        int matchCount = m_TeamDeathmatchPPref.value <= 100 ? m_TeamDeathmatchPPref.value : 100;
        if (m_SaveArena.value != m_CurrentHighestArenaVariable.value.index)
        {
            m_TeamDeathmatchPPref.value = 1;
            matchCount = m_TeamDeathmatchPPref.value;
            m_SaveArena.value = m_CurrentHighestArenaVariable.value.index;
        }

        #region Design Event
        string status = "Start";
        int userKillCount = 0;
        int teamKillCount = 0;
        int userAssistCount = 0;
        int teamAssistCount = 0;
        int userDeathCount = 0;
        int teamDeathCount = 0;
        GameEventHandler.Invoke(PSDesignEvent.TeamDeathMatchStats, status, userKillCount, teamKillCount, userAssistCount, teamAssistCount, userDeathCount, teamDeathCount, matchCount);
        #endregion

        #region Progresstion Event
        string teamDeathmatchPPrefStatus = "Start";

        int score = 0;
        GameEventHandler.Invoke(PSProgressionEvent.TeamDeathmatch, teamDeathmatchPPrefStatus, matchCount, score);
        #endregion
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete)
            return;

        List<PBRobot> teamARobots = m_AllRobots
            .Where(v => v.TeamId == 1)
            .ToList();

        List<PBRobot> teamBRobots = m_AllRobots
            .Where(v => v.TeamId != 1)
            .ToList();

        m_IsVictory = matchOfPlayer.isVictory;
        if (m_IsSurrender)
            m_IsVictory = false;

        StartCoroutine(DelayShowInfoGameOver(matchOfPlayer));

        int matchCount = m_TeamDeathmatchPPref.value <= 100 ? m_TeamDeathmatchPPref.value : 100;
        int teamPlayerkill = teamARobots.Sum(v => v.PlayerKDA.Kills);
        int teamOpponentKill = teamBRobots.Sum(v => v.PlayerKDA.Kills);

        #region Design Event
        string status = m_IsVictory ? "Complete" : "Fail";
        PlayerKDA userKDA = teamARobots.Find(v => v.PersonalInfo.isLocal).PlayerKDA;

        int userKillCount = userKDA.Kills;
        int teamKillCount = teamARobots.Sum(v => v.PlayerKDA.Kills);
        int userAssistCount = userKDA.Assists;
        int teamAssistCount = teamARobots.Sum(v => v.PlayerKDA.Assists); ;
        int userDeathCount = userKDA.Deaths;
        int teamDeathCount = teamARobots.Sum(v => v.PlayerKDA.Deaths);
        if (!m_IsSurrender)
            GameEventHandler.Invoke(PSDesignEvent.TeamDeathMatchStats, status, userKillCount, teamKillCount, userAssistCount, teamAssistCount, userDeathCount, teamDeathCount, matchCount);
        else
        {
            if (teamKillCount == 0)
                teamKillCount = 1;
            if (teamOpponentKill == 0)
                teamOpponentKill = 1;
            int designScore = (teamKillCount * 100) / teamOpponentKill;
            GameEventHandler.Invoke(PSDesignEvent.TeamDeathMatchStatsSurrender, matchCount, designScore);
        }

        #endregion

        #region Progresstion Event
        string teamDeathmatchPPrefStatus = m_IsVictory ? "Complete" : "Fail";
        if (teamKillCount == 0)
            teamKillCount = 1;
        if (teamOpponentKill == 0)
            teamOpponentKill = 1;
        int progressionScore = (teamKillCount * 100) / teamOpponentKill;

        GameEventHandler.Invoke(PSProgressionEvent.TeamDeathmatch, teamDeathmatchPPrefStatus, matchCount, progressionScore);
        #endregion
    }

    private void OnLeaveInMiddleOfMatch()
    {
        m_IsSurrender = true;
    }

    private IEnumerator DelayShowInfoGameOver(PBPvPMatch matchOfPlayer)
    {
        yield return new WaitForSeconds(1f);
        m_CanvasGroupVisibility.Show();
        InitData(matchOfPlayer);
    }
    private void BotModelSpawned(params object[] parameters)
    {
        if (parameters[0] is not PBRobot pbBot) return;

        if (m_AllRobots == null)
            m_AllRobots = new List<PBRobot>();

        if (!m_AllRobots.Contains(pbBot))
            m_AllRobots.Add(pbBot);
    }

    private GachaPack GetRandomGachaPack(PvPArenaSO arenSO)
    {
        if (m_CachedGachaPack == null)
        {
            var scriptedGachaPacks = arenSO.Cast<PBPvPArenaSO>().ScriptedGachaBoxes;
            if (scriptedGachaPacks.isRemainPack)
            {
                m_CachedGachaPack = scriptedGachaPacks.currentManualGachaPack;
                scriptedGachaPacks.Next();
            }
            else
            {
                m_CachedGachaPack = arenSO.GetReward<RandomGachaPackRewardModule>().gachaPackCollection.GetRandom();
            }
        }
        return m_CachedGachaPack;
    }

    private float GetTrophyRewardMultiplier(int rank)
    {
        return 1;
        // switch (rank)
        // {
        //     case 1:
        //         return 2.5f;
        //     case 2:
        //         return 2f;
        //     case 3:
        //         return 1.5f;
        //     case 4:
        //         return 1f;
        //     default:
        //         return 1;
        // }
    }

    private void OnUnpackStart()
    {

    }

    private void OnUnpackDone()
    {
        ObjectFindCache<PBPvPStateGameController>.Get().LeaveGame();

        if (!m_PSFTUESO.FTUEStartFirstMatch.value)
            m_PSFTUESO.FTUEStartFirstMatch.value = true;

        if (m_PSFTUESO.FTUEOpenInfoPopup.value && !m_PSFTUESO.FTUEStart2ndMatch.value)
            m_PSFTUESO.FTUEStart2ndMatch.value = true;
    }
}
