using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.GUI;
using LatteGames;
using LatteGames.PvP;
using LatteGames.Template;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossPreparingMatchUI : ComposeCanvasElementVisibilityController
{
    [SerializeField] PvPArenaVariable chosenArenaVariable;
    [SerializeField] PBPvPMatchMakingSO matchmakingSO;
    [SerializeField] TextMeshProUGUI timerTMP;
    [SerializeField] PlayerInfoVariable infoOfPlayer;
    [SerializeField] PlayerInfoVariable infoOfOpponent;
    [SerializeField] RawImage playerRobotImage;
    [SerializeField] RawImage opponentRobotImage;
    [SerializeField] List<PBPvPStatUI> statUIs;
    [SerializeField] PvPArenaSO arenaSO;
    [SerializeField] Image bossNodeImgPrefab;
    [SerializeField] Sprite previousBossSprite, currentBossSprite, nextBossSprite;
    [SerializeField] TextMeshProUGUI playerHP_TMP, opponentHP_TMP, playerATK_TMP, opponentATK_TMP, higherPlayerHP_TMP, higherOpponentHP_TMP, higherPlayerATK_TMP, higherOpponentATK_TMP;

    [SerializeField, BoxGroup("Ref")] private Transform m_ArrowLeftHP, m_ArrowRightHP, m_ArrowLeftATK, m_ArrowRightATK;
    [SerializeField, BoxGroup("Ref")] private TextAdapter m_BossInfoText;
    [SerializeField, BoxGroup("Data")] private BossChapterSO m_BossChapterSO;

    private BossMapSO bossMapSO => BossFightManager.Instance.bossMapSO;
    private string matchStartInOriginText;
    private PBModelRendererSpawner modelRendererSpawner;
    private List<Image> bossNodes = new List<Image>();
    private bool _isMatching = false;

    private void Awake()
    {
        matchStartInOriginText = timerTMP.text;
        modelRendererSpawner = ObjectFindCache<PBModelRendererSpawner>.Get(isCallFromAwake: true);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, Hide);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnCompleteCamRotation, OnMatchPrepared);
        bossNodes.Add(bossNodeImgPrefab);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, Hide);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnCompleteCamRotation, OnMatchPrepared);
    }

    private void OnMatchPrepared()
    {
        _isMatching = true;
        var match = PBPvPMatch.CreateMatch(arenaSO, Mode.Boss, infoOfPlayer, infoOfOpponent);
        GameEventHandler.Invoke(CharacterRoomEvent.OnBuildRobotCharacterBossPVP, match.GetOpponentInfo());
        var matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
        matchManager.PrepareMatch(match);
        UpdateView();
        ShowImmediately();
        StartCoroutine(PrepareMatch_CR(match));
    }

    private IEnumerator PrepareMatch_CR(PvPMatch match)
    {
        var countdownDuration = matchmakingSO.preparingMatchDuration;
        timerTMP.text = matchStartInOriginText.Replace("{second}", countdownDuration.ToString());
        yield return CountdownToStartGame_CR();
        var matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
        matchManager.StartMatch(match);
        modelRendererSpawner.DestroyAllInstances();
        yield return null;
        _isMatching = false;
    }

    private IEnumerator CountdownToStartGame_CR()
    {
        var countdownDuration = matchmakingSO.preparingMatchDuration;
        var waitForOneSecs = new WaitForSeconds(1f);
        while (countdownDuration >= 0)
        {
            if (countdownDuration <= 3 && countdownDuration > 0)
                SoundManager.Instance.PlaySFX(PBSFX.UICountdown);
            if (countdownDuration == 0)
                SoundManager.Instance.PlaySFX(PBSFX.UIStartFight);
            timerTMP.text = matchStartInOriginText.Replace("{second}", countdownDuration--.ToString());
            yield return waitForOneSecs;
        }
    }
    private void CalCompareInfo()
    {
        var playerStats = infoOfPlayer.value.Cast<PBPlayerInfo>().robotStatsSO.stats;
        var opponentStats = infoOfOpponent.value.Cast<PBPlayerInfo>().robotStatsSO.stats;
        var playerHP = playerStats.GetHealth().value;
        var playerATK = playerStats.GetAttack().value;
        var opponentHP = opponentStats.GetHealth().value;
        var opponentATK = opponentStats.GetAttack().value;

        if (opponentHP >= playerHP)
            m_ArrowRightHP.GetChild(0).gameObject.SetActive(true);
        else
            m_ArrowLeftHP.GetChild(0).gameObject.SetActive(true);

        if (opponentATK >= playerATK)
            m_ArrowRightATK.GetChild(0).gameObject.SetActive(true);
        else
            m_ArrowLeftATK.GetChild(0).gameObject.SetActive(true);
    }
    void UpdateView()
    {
        if (m_BossInfoText != null && m_BossChapterSO != null)
        {
            string updatedText = m_BossInfoText.blueprintText
                    .Replace("{id}", $"{m_BossChapterSO.bossIndex.value + 1}")
                    .Replace("{Name}", $"{m_BossChapterSO.bossList[m_BossChapterSO.bossIndex].botInfo.name}");
            m_BossInfoText.SetText(updatedText);
        }

        playerRobotImage.texture = modelRendererSpawner.SpawnPlayerModelRenderer();
        playerRobotImage.gameObject.SetActive(true);
        opponentRobotImage.texture = modelRendererSpawner.SpawnOpponentModelRenderer(infoOfOpponent);
        CalCompareInfo();

        var arenaSO = chosenArenaVariable.value;

        //Reward
        var currencyRewardModule = arenaSO.GetReward<CurrencyRewardModule>(module => module.CurrencyType.Equals(CurrencyType.Standard));

        //Stats
        var playerHP = infoOfPlayer.value.Cast<PBPlayerInfo>().robotStatsSO.stats.GetHealth().value;
        var playerATK = infoOfPlayer.value.Cast<PBPlayerInfo>().robotStatsSO.stats.GetAttack().value;
        var opponentHP = infoOfOpponent.value.Cast<PBPlayerInfo>().robotStatsSO.stats.GetHealth().value;
        var opponentATK = infoOfOpponent.value.Cast<PBPlayerInfo>().robotStatsSO.stats.GetAttack().value;

        if (opponentHP >= playerHP)
        {
            playerHP_TMP.gameObject.SetActive(true);
            higherPlayerHP_TMP.gameObject.SetActive(false);
            opponentHP_TMP.gameObject.SetActive(false);
            higherOpponentHP_TMP.gameObject.SetActive(true);
            playerHP_TMP.text = playerHP.RoundToInt().ToRoundedText();
            higherOpponentHP_TMP.text = opponentHP.RoundToInt().ToRoundedText();
        }
        else
        {
            playerHP_TMP.gameObject.SetActive(false);
            higherPlayerHP_TMP.gameObject.SetActive(true);
            opponentHP_TMP.gameObject.SetActive(true);
            higherOpponentHP_TMP.gameObject.SetActive(false);
            higherPlayerHP_TMP.text = playerHP.RoundToInt().ToRoundedText();
            opponentHP_TMP.text = opponentHP.RoundToInt().ToRoundedText();
        }

        if (opponentATK >= playerATK)
        {
            playerATK_TMP.gameObject.SetActive(true);
            higherPlayerATK_TMP.gameObject.SetActive(false);
            opponentATK_TMP.gameObject.SetActive(false);
            higherOpponentATK_TMP.gameObject.SetActive(true);
            playerATK_TMP.text = playerATK.RoundToInt().ToRoundedText();
            higherOpponentATK_TMP.text = opponentATK.RoundToInt().ToRoundedText();
        }
        else
        {
            playerATK_TMP.gameObject.SetActive(false);
            higherPlayerATK_TMP.gameObject.SetActive(true);
            opponentATK_TMP.gameObject.SetActive(true);
            higherOpponentATK_TMP.gameObject.SetActive(false);
            higherPlayerATK_TMP.text = playerATK.RoundToInt().ToRoundedText();
            opponentATK_TMP.text = opponentATK.RoundToInt().ToRoundedText();
        }

        UpdateChapterProgress();
    }

    void UpdateChapterProgress()
    {
        var currentChapterSO = bossMapSO.currentChapterSO;
        var currentBossIndex = currentChapterSO.bossIndex;
        for (int i = 0; i < currentChapterSO.bossCount; i++)
        {
            if (i >= bossNodes.Count)
            {
                var node = Instantiate(bossNodeImgPrefab);
                node.transform.SetParent(bossNodeImgPrefab.transform.parent);
                node.transform.localScale = Vector3.one;
                bossNodes.Add(node);
            }

            if (currentBossIndex == i)
            {
                bossNodes[i].sprite = currentBossSprite;
            }
            else if (currentBossIndex > i)
            {
                bossNodes[i].sprite = previousBossSprite;
            }
            else if (currentBossIndex < i)
            {
                bossNodes[i].sprite = nextBossSprite;
            }
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (_isMatching)
        {
            _isMatching = false;

            string stageNames = "";
            GameObject pbFightingStage = PBFightingStage.Instance.gameObject;
            if (pbFightingStage != null)
            {
                stageNames = pbFightingStage.name.Replace("(Clone)", "");
            }

            string stageName = stageNames;
            string mainPhase = "Matchmaking";
            string subPhase = "MatchStarting";

            GameEventHandler.Invoke(DesignEvent.QuitCheck, stageName, mainPhase, subPhase);
        }
    }
}
