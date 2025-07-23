using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.PvP;
using LatteGames.Template;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class PBPvPPreparingMatchUI : ComposeCanvasElementVisibilityController
{
    [SerializeField] ModeVariable chosenModeVariable;
    [SerializeField] PvPArenaVariable chosenArenaVariable;
    [SerializeField] PBPvPMatchMakingSO matchmakingSO;
    [SerializeField] TextMeshProUGUI timerTMP;
    [SerializeField] PlayerInfoVariable infoOfPlayer;
    [SerializeField] PBPvPPlayerNameUI playerInfoUI;
    [SerializeField] PBPvPPlayerNameUI opponentInfoUI;
    [SerializeField] RawImage playerRobotImage;
    [SerializeField] RawImage opponentRobotImage;
    [SerializeField] TextMeshProUGUI matchRewardTMP, playerHP_TMP, opponentHP_TMP, playerATK_TMP, opponentATK_TMP, higherPlayerHP_TMP, higherOpponentHP_TMP, higherPlayerATK_TMP, higherOpponentATK_TMP;
    [SerializeField] List<PBPvPStatUI> statUIs;

    [SerializeField, BoxGroup("Ref")] private GameObject m_FlagOpponent;
    [SerializeField, BoxGroup("Ref")] private GameObject m_LoadingHPEffect, m_LoadingATKEffect;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI m_SearchingText;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI m_SearchingTimeText;
    [SerializeField, BoxGroup("Ref")] private LayoutElement m_RPSLayoutElement;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_CanvasGroupRPS;
    [SerializeField, BoxGroup("Ref")] private GameObject m_HeaderObject;
    [SerializeField, BoxGroup("Ref")] private Transform m_ArrowLeftHP, m_ArrowRightHP, m_ArrowLeftATK, m_ArrowRightATK;
    [SerializeField, BoxGroup("Ref")] private Transform m_RectRobotMaskOpponent;
    [SerializeField, BoxGroup("Ref")] private Transform m_FakeSearchingOpponent;
    [SerializeField, BoxGroup("Ref")] private GameObject m_FakeSearchingName;
    [SerializeField, BoxGroup("Ref")] private GameObject m_OpponentName;
    [SerializeField, BoxGroup("Ref")] private List<Transform> m_OpponentInfoTexts;

    private string matchStartInOriginText;
    private PBModelRendererSpawner modelRendererSpawner;
    private PlayerInfoVariable infoOfOpponent;
    private bool _isMatching = false;
    private int m_SearchingTime = 0;
    private Camera _mainCamera;

    private void Awake()
    {
        if (chosenModeVariable == Mode.Battle)
            return;
        matchStartInOriginText = timerTMP.text;
        _mainCamera = MainCameraFindCache.Get();
        GetOnEndShowEvent().Subscribe(OnEndShow);
        GetOnStartHideEvent().Subscribe(OnStartHide);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchPrepared, OnMatchPrepared);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, Hide);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnOpponentFounded, HandleOpponentFounded);
    }

    private void OnDestroy()
    {
        if (chosenModeVariable == Mode.Battle)
            return;
        GetOnEndShowEvent().Unsubscribe(OnEndShow);
        GetOnStartHideEvent().Unsubscribe(OnStartHide);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchPrepared, OnMatchPrepared);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, Hide);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnOpponentFounded, HandleOpponentFounded);
    }

    private void Start()
    {
        if (chosenModeVariable == Mode.Battle)
            return;        
        modelRendererSpawner = ObjectFindCache<PBModelRendererSpawner>.Get();
        playerInfoUI.PersonalInfo = infoOfPlayer.value.personalInfo;
    }

    private void OnMatchPrepared(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;

        StartCoroutine(SearchingOpponent(parameters));
    }

    private void OnEndShow()
    {
        _mainCamera.enabled = false;
    }

    private void OnStartHide()
    {
        _mainCamera.enabled = true;
    }

    private IEnumerator SearchingOpponent(object[] parameters)
    {
        GameEventHandler.Invoke(PBPvPEventCode.OnStartSearchingOpponent);

        m_FlagOpponent.SetActive(false);
        m_HeaderObject.SetActive(true);
        m_FakeSearchingName.gameObject.SetActive(true);
        m_OpponentName.gameObject.SetActive(false);
        CalCompareInfo(true);
        m_RectRobotMaskOpponent.DOScale(Vector3.zero, 0);
        m_RPSLayoutElement.ignoreLayout = true;
        m_CanvasGroupRPS.Hide();
        m_FakeSearchingOpponent.DOScale(Vector3.one, 1);

        ShowImmediately();
        UpdateView();
        timerTMP.gameObject.SetActive(false);
        m_SearchingText.gameObject.SetActive(true);

        while (true)
        {
            m_SearchingTime++;
            m_SearchingTimeText.text = m_SearchingTime.ToString();

            if (m_SearchingTime >= 3)
                break;

            yield return new WaitForSeconds(1);
        }

        m_SearchingText.gameObject.SetActive(false);
        m_RPSLayoutElement.ignoreLayout = false;
        m_CanvasGroupRPS.Show();
        m_FakeSearchingOpponent
            .DOScale(Vector3.zero, 0.5f).SetEase(Ease.OutFlash)
            .OnComplete(() =>
            {
                CalCompareInfo(false);
                m_FakeSearchingName.gameObject.SetActive(false);
                m_OpponentName.gameObject.SetActive(true);

                m_RectRobotMaskOpponent
                .DOScale(Vector3.one, 0.5f).SetEase(Ease.OutFlash)
                .OnComplete(() =>
                {
                    _isMatching = true;
                    if (modelRendererSpawner != null)
                        modelRendererSpawner.OpponentsCharacter[0].CurrentState = CharacterState.ReadyFight;
                    opponentInfoUI.PersonalInfo = infoOfOpponent.value.personalInfo;
                    m_FlagOpponent.SetActive(true);
                    StartCoroutine(PrepareMatch_CR(parameters[0] as PvPMatch));
                });
            });
    }
    private void HandleOpponentFounded(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0) return;
        if (parameters[1] is not PlayerInfoVariable opponent) return;
        infoOfOpponent = opponent;
    }
    private void CalCompareInfo(bool isOnlyPlayer)
    {
        var playerStats = infoOfPlayer.value.Cast<PBPlayerInfo>().robotStatsSO.stats;
        var opponentStats = infoOfOpponent.value.Cast<PBPlayerInfo>().robotStatsSO.stats;
        var playerHP = playerStats.GetHealth().value;
        var playerATK = playerStats.GetAttack().value;
        var opponentHP = opponentStats.GetHealth().value;
        var opponentATK = opponentStats.GetAttack().value;

        if (isOnlyPlayer)
        {
            m_OpponentInfoTexts.ForEach(v => v.DOScale(Vector3.zero, 0));
        }
        else
        {
            m_LoadingHPEffect.SetActive(false);
            m_LoadingATKEffect.SetActive(false);

            m_OpponentInfoTexts.ForEach(v => v.DOScale(Vector3.one, 0.75f));

            if (opponentHP >= playerHP)
                m_ArrowRightHP.GetChild(0).gameObject.SetActive(true);
            else
                m_ArrowLeftHP.GetChild(0).gameObject.SetActive(true);

            if (opponentATK >= playerATK)
                m_ArrowRightATK.GetChild(0).gameObject.SetActive(true);
            else
                m_ArrowLeftATK.GetChild(0).gameObject.SetActive(true);
        }
    }
    private IEnumerator PrepareMatch_CR(PvPMatch match)
    {
        var countdownDuration = matchmakingSO.preparingMatchDuration;
        timerTMP.text = matchStartInOriginText.Replace("{second}", countdownDuration.ToString());
        timerTMP.gameObject.SetActive(true);
        yield return CountdownToStartGame_CR();
        var matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
        matchManager.StartMatch(match);
        modelRendererSpawner.DestroyAllInstances();
        yield return null;
        _isMatching = false;
        yield return Yielders.Get(0.5f);
        if (playerRobotImage.texture != null)
        {
            Debug.Log("[LG-LOG]:: RELEASE PLAYER RENDERTEXTURE");
            RenderTexture.ReleaseTemporary(playerRobotImage.texture as RenderTexture);
        }
        if (opponentRobotImage.texture != null)
        {
            Debug.Log("[LG-LOG]:: RELEASE OPPONENT RENDERTEXTURE");
            RenderTexture.ReleaseTemporary(opponentRobotImage.texture as RenderTexture);
        }
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

    void UpdateView()
    {
        playerRobotImage.texture = modelRendererSpawner.SpawnPlayerModelRenderer();
        opponentRobotImage.texture = modelRendererSpawner.SpawnOpponentModelRenderer(infoOfOpponent);

        var arenaSO = chosenArenaVariable.value;

        //Reward
        var currencyRewardModule = arenaSO.GetReward<CurrencyRewardModule>(module => module.CurrencyType.Equals(CurrencyType.Standard));
        matchRewardTMP.text = currencyRewardModule.Amount.ToRoundedText();

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
    }

    private void OnApplicationPause(bool pause)
    {
        if (_isMatching)
        {
            _isMatching = false;

            string name = "";
            GameObject pbFightingStage = PBFightingStage.Instance.gameObject;
            if (pbFightingStage != null)
            {
                name = pbFightingStage.name.Replace("(Clone)", "");
            }

            string stageName = name;
            string mainPhase = "Matchmaking";
            string subPhase = "MatchStarting";
            GameEventHandler.Invoke(DesignEvent.QuitCheck, stageName, mainPhase, subPhase);
        }
    }
}
