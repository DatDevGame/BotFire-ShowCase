using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.PvP;
using LatteGames.Template;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using DG.Tweening;

public class PBPvPPreparingBattleModeMatchUI : ComposeCanvasElementVisibilityController
{

    [SerializeField] ModeVariable chosenModeVariable;
    [SerializeField] BattleBetArenaVariable battleBetArenaVariable;
    [SerializeField] TextMeshProUGUI searchingForOpponentText;
    [SerializeField] TextMeshProUGUI lookingForOpponentTimerTMP;
    [SerializeField] PBPvPMatchMakingSO matchmakingSO;
    [SerializeField] TextMeshProUGUI matchStartInTimerTMP;
    [SerializeField] PlayerInfoVariable infoOfPlayer;
    [SerializeField] TextMeshProUGUI matchRewardTMP;
    [SerializeField] PBPvPBattleModePlayerUI playerInfoUI;
    [SerializeField] List<PBPvPBattleModePlayerUI> opponentInfoUIs;

    [SerializeField, BoxGroup("Config")] private float m_TimeDurationTransitionCharacter;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_BackGround;
    [SerializeField, BoxGroup("Ref Preview Character")] private CanvasGroupVisibility m_PrizePoolCanvasGroup;
    [SerializeField, BoxGroup("Ref Preview Character")] private CanvasGroupVisibility m_PreviewCharacterCanvasGroup;
    [SerializeField, BoxGroup("Ref Preview Character")] private CanvasGroupVisibility m_PreviewCardCanvasGroup;
    [SerializeField, BoxGroup("Ref Preview Character")] private PBPvPBattleModePlayerUI m_PlayerCharacterInfoUI;
    [SerializeField, BoxGroup("Ref Preview Character")] private RectTransform m_PointWaiting;
    [SerializeField, BoxGroup("Ref Preview Character")] private RectTransform m_PointPreview;
    [SerializeField, BoxGroup("Ref Preview Character")] private RectTransform m_PointOut;
    [SerializeField, BoxGroup("Ref Preview Character")] private List<PBPvPBattleModePlayerUI> m_CharacterOpponentInfoUIs;

    string matchStartInOriginText;
    Coroutine coutingCoroutine;
    PvPMatchManager matchManager;
    PBModelRendererSpawner modelRendererSpawner;
    List<PlayerInfoVariable> opponentInfoVariables = new();
    private float _entryFee;
    private float _prizePoolSize;

    private bool isEndCamRotation = false;
    void Awake()
    {
        _parametersFounded = new List<object[]>();

        if (chosenModeVariable.value == Mode.Normal)
            return;
        matchStartInOriginText = matchStartInTimerTMP.text;
    }

    void OnDestroy()
    {
        if (chosenModeVariable.value == Mode.Normal)
            return;
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchPrepared, OnMatchPrepared);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, Hide);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnOpponentFounded, HandleOpponentFounded);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnCompleteCamRotation, OnCompleteCamRotation);
    }

    void Start()
    {
        if (chosenModeVariable.value == Mode.Normal)
            return;
        matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
        modelRendererSpawner = ObjectFindCache<PBModelRendererSpawner>.Get();
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchPrepared, OnMatchPrepared);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, Hide);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnOpponentFounded, HandleOpponentFounded);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnCompleteCamRotation, OnCompleteCamRotation);
        //if (chosenModeVariable.value == Mode.Normal)
        //    return;
        //coutingCoroutine = StartCoroutine(CountLookingForOpponent_CR());
        //UpdateView();
        //ShowImmediately();
    }

    private void OnCompleteCamRotation()
    {
        isEndCamRotation = true;
        StartCoroutine(DelayOnOpponentFounded());
    }

    void OnMatchPrepared(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        StartCoroutine(PrepareMatch_CR(parameters[0] as PvPMatch));
    }
    private IEnumerator DelayOnOpponentFounded()
    {
        if (chosenModeVariable.value == Mode.Normal)
            yield break;

        yield return new WaitUntil(() => isEndCamRotation);
        PBModelRendererSpawner modelRendererSpawner = ObjectFindCache<PBModelRendererSpawner>.Get();
        coutingCoroutine = StartCoroutine(CountLookingForOpponent_CR());
        UpdateView();
        ShowImmediately();

        //m_BackGround.Show();
        //m_PreviewCharacterCanvasGroup.Show();
        //m_PreviewCardCanvasGroup.Hide();
        //m_PrizePoolCanvasGroup.Hide();

        m_PreviewCardCanvasGroup.Show();
        m_BackGround.Hide();
        m_PreviewCharacterCanvasGroup.Hide();
        //m_PrizePoolCanvasGroup.Show();

        //Card UI
        if (modelRendererSpawner != null)
            modelRendererSpawner.SetCameraRobotPreview();

        //PlayCoinToPoolEffect(playerInfoUI.transform.position);
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < _parametersFounded.Count; i++)
        {
            if (_parametersFounded[i] == null) yield break;
            if (_parametersFounded[i][0] is not PBPvPArenaSO arenaSO) yield break;
            if (_parametersFounded[i][1] is not PlayerInfoVariable opponentVariable) yield break;

            opponentInfoVariables.Add(opponentVariable);

            if (modelRendererSpawner != null)
                modelRendererSpawner.SetCameraRobotPreview();

            var characterModelRenderer = modelRendererSpawner.SpawnOpponentModelRenderer(opponentInfoVariables[^1]);
            m_CharacterOpponentInfoUIs[opponentInfoVariables.Count - 1].RobotRenderTexture = characterModelRenderer;
            m_CharacterOpponentInfoUIs[opponentInfoVariables.Count - 1].PlayerInfoVariable = opponentInfoVariables[^1];

            opponentInfoUIs[opponentInfoVariables.Count - 1].RobotRenderTexture = characterModelRenderer;
            opponentInfoUIs[opponentInfoVariables.Count - 1].PlayerInfoVariable = opponentInfoVariables[^1];

            //PlayCoinToPoolEffect(opponentInfoUIs[i].transform.position);
            if ((opponentInfoVariables.Count + 1) == arenaSO.numOfContestant) // +1 player
            {
                searchingForOpponentText.gameObject.SetActive(false);
                StopCoroutine(coutingCoroutine);
                StartCoroutine(CommonCoroutine.Delay(0.1f, false, () => PrepareMatch(arenaSO)));
            }
            LGDebug.Log($"Opponent founded: {opponentInfoVariables.Count}");
            SoundManager.Instance.PlaySFX(PBSFX.UIOpponentAppear);
            yield return new WaitForSeconds(0.5f);
        }

        ////Character UI
        //for (int i = 0; i < _parametersFounded.Count; i++)
        //{
        //    yield return new WaitForSeconds(1f);
        //    if (_parametersFounded[i] == null) yield break;
        //    if (_parametersFounded[i][0] is not PBPvPArenaSO arenaSO) yield break;
        //    if (_parametersFounded[i][1] is not PlayerInfoVariable opponentVariable) yield break;

        //    if (i == 0)
        //    {
        //        m_PlayerCharacterInfoUI.transform.SetParent(m_PointOut);
        //        m_PlayerCharacterInfoUI.transform
        //            .DOLocalMove(Vector3.zero, m_TimeDurationTransitionCharacter)
        //            .SetEase(Ease.InOutSine);
        //    }
        //    if (i >= 1)
        //    {
        //        m_CharacterOpponentInfoUIs[i - 1].transform.SetParent(m_PointOut);
        //        m_CharacterOpponentInfoUIs[i - 1].transform
        //            .DOLocalMove(Vector3.zero, m_TimeDurationTransitionCharacter)
        //            .SetEase(Ease.InOutSine);
        //    }
        //    m_CharacterOpponentInfoUIs[i].transform.SetParent(m_PointPreview);
        //    m_CharacterOpponentInfoUIs[i].gameObject.SetActive(true);
        //    m_CharacterOpponentInfoUIs[i].transform
        //        .DOLocalMove(Vector3.zero, m_TimeDurationTransitionCharacter)
        //        .SetEase(Ease.InOutSine);

        //    opponentInfoVariables.Add(opponentVariable);

        //    var characterModelRenderer = modelRendererSpawner.SpawnOpponentModelRenderer(opponentInfoVariables[^1]);
        //    m_CharacterOpponentInfoUIs[opponentInfoVariables.Count - 1].RobotRenderTexture = characterModelRenderer;
        //    m_CharacterOpponentInfoUIs[opponentInfoVariables.Count - 1].PlayerInfoVariable = opponentInfoVariables[^1];

        //    opponentInfoUIs[opponentInfoVariables.Count - 1].RobotRenderTexture = characterModelRenderer;
        //    opponentInfoUIs[opponentInfoVariables.Count - 1].PlayerInfoVariable = opponentInfoVariables[^1];


        //}
        //yield return new WaitForSeconds(3);

        ////Card UI
        //if (modelRendererSpawner != null)
        //    modelRendererSpawner.SetCameraRobotPreview();

        //m_BackGround.Hide();
        //m_PreviewCharacterCanvasGroup.Hide();
        //m_PreviewCardCanvasGroup.Show();
        //m_PrizePoolCanvasGroup.Show();
        //yield return new WaitForSeconds(0.5f);
        //PlayCoinToPoolEffect(playerInfoUI.transform.position);
        //for (int i = 0; i < _parametersFounded.Count; i++)
        //{
        //    yield return new WaitForSeconds(0.2f);
        //    PBPvPArenaSO arenaSO = _parametersFounded[i][0] as PBPvPArenaSO;
        //    PlayerInfoVariable opponentVariable = _parametersFounded[i][1] as PlayerInfoVariable;
        //    PlayCoinToPoolEffect(opponentInfoUIs[i].transform.position);
        //    if ((opponentInfoVariables.Count + 1) == arenaSO.numOfContestant) // +1 player
        //    {
        //        searchingForOpponentText.gameObject.SetActive(false);
        //        StopCoroutine(coutingCoroutine);
        //        StartCoroutine(CommonCoroutine.Delay(0.1f, false, () => PrepareMatch(arenaSO)));
        //    }

        //    LGDebug.Log($"Opponent founded: {opponentInfoVariables.Count}");
        //    SoundManager.Instance.PlaySFX(PBSFX.UIOpponentAppear);
        //}
    }
    [SerializeField] private List<object[]> _parametersFounded;
    void HandleOpponentFounded(object[] parameters)
    {
        _parametersFounded.Add(parameters);
        if (_parametersFounded.Count >= 7)
        {
            GameEventHandler.Invoke(PBPvPEventCode.OnEndSearchingArena);
            GameEventHandler.Invoke(CharacterRoomEvent.OnBuildRobotCharacterBattlePVP, _parametersFounded);
        }
    }

    IEnumerator PrepareMatch_CR(PvPMatch match)
    {
        var countdownDuration = matchmakingSO.preparingMatchDuration;
        matchStartInTimerTMP.gameObject.SetActive(true);
        matchStartInTimerTMP.text = matchStartInOriginText.Replace("{second}", countdownDuration.ToString());
        yield return CountdownToStartGame_CR();
        var matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
        matchManager.StartMatch(match);
        modelRendererSpawner.DestroyAllInstances();
        yield return null;
    }

    IEnumerator CountdownToStartGame_CR()
    {
        var countdownDuration = matchmakingSO.preparingMatchDuration;
        var waitForOneSecs = new WaitForSeconds(1f);
        while (countdownDuration >= 0)
        {
            if (countdownDuration <= 3 && countdownDuration > 0)
                SoundManager.Instance.PlaySFX(PBSFX.UICountdown);
            if (countdownDuration == 0)
                SoundManager.Instance.PlaySFX(PBSFX.UIStartFight);
            matchStartInTimerTMP.text = matchStartInOriginText.Replace("{second}", countdownDuration--.ToString());
            yield return waitForOneSecs;
        }
    }

    IEnumerator CountLookingForOpponent_CR()
    {
        var totalTimeToWait = 0;
        var waitForOneSecs = new WaitForSeconds(1f);
        while (true)
        {
            lookingForOpponentTimerTMP.text = totalTimeToWait++.ToString();
            yield return waitForOneSecs;
        }
    }

    void PrepareMatch(PvPArenaSO arenaSO)
    {
        var playerInfoVariables = new List<PlayerInfoVariable>(opponentInfoVariables)
        {
            infoOfPlayer
        };
        var match = PBPvPMatch.CreateMatch(arenaSO, Mode.Battle, playerInfoVariables.ToArray());

        matchManager.PrepareMatch(match);
    }

    void UpdateView()
    {
        matchStartInTimerTMP.gameObject.SetActive(false);
        // Spawn player
        var modelRendererPlayer = modelRendererSpawner.SpawnPlayerModelRenderer();
        playerInfoUI.PlayerInfoVariable = infoOfPlayer;
        playerInfoUI.RobotRenderTexture = modelRendererPlayer;
        m_PlayerCharacterInfoUI.PlayerInfoVariable = infoOfPlayer;
        m_PlayerCharacterInfoUI.RobotRenderTexture = modelRendererPlayer;
        // Bet Prize Pool
        var arenaSO = battleBetArenaVariable.RewardArena;
        matchRewardTMP.text = "0";//arenaSO.BattleRoyaleCoinBetPool.ToRoundedText();
        _prizePoolSize = 0;
        _entryFee = battleBetArenaVariable.RewardArena.BattleRoyaleEntryFee;

        m_PlayerCharacterInfoUI.transform.SetParent(m_PointPreview);
        m_PlayerCharacterInfoUI.gameObject.SetActive(true);
        m_PlayerCharacterInfoUI.transform.DOLocalMove(Vector3.zero, 0);
    }

    private void PlayCoinToPoolEffect(Vector3 from)
    {
        CurrencyManager.Instance.PlayAcquireAnimation(
            CurrencyType.Standard,
            _entryFee,
            from,
            matchRewardTMP.transform.position,
            null,
            null,
            AquireAnimationComplete);
    }

    private void AquireAnimationComplete(float coinAdd)
    {
        _prizePoolSize += coinAdd;
        matchRewardTMP.text = _prizePoolSize.ToRoundedText();
    }
}