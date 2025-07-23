using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames;
using LatteGames.PvP;
using LatteGames.Template;
using TMPro;
using UnityEngine;

public class PvPTimeUpGameOverUI : MonoBehaviour
{
    [SerializeField] ModeVariable chosenModeVariable;
    [SerializeField]
    private bool m_IsFTUE = false;
    [SerializeField]
    private float m_TicTacTimeLeft = 5f;
    [SerializeField]
    private TextMeshProUGUI m_TimeUpText;
    [SerializeField]
    private TextMeshProUGUI m_SummaryText;

    private Coroutine m_PlaySoundCoroutine;
    private RectTransform m_ContainerRectTransform;

    private void Awake()
    {
        if (chosenModeVariable == Mode.Battle)
            return;
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStarted);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnRoundCompleted, OnRoundCompleted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnShowGameOverUI, OnShowGameOverUI);
    }

    private void Start()
    {
        m_TimeUpText.gameObject.SetActive(false);
        m_SummaryText.gameObject.SetActive(false);
        m_ContainerRectTransform = transform.GetChild(0) as RectTransform;
    }

    private void OnDestroy()
    {
        if (chosenModeVariable == Mode.Battle)
            return;
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStarted);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnRoundCompleted, OnRoundCompleted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnShowGameOverUI, OnShowGameOverUI);
    }

    private void OnLevelStarted()
    {
        // var pvpMatchManager = ObjectFindCache<PBPvPMatchManager>.Get();
        // var currentMatch = pvpMatchManager?.GetCurrentMatchOfPlayer() ?? null;
        // var remainingTime = currentMatch == null ? 20 : currentMatch.arenaSO.Cast<PBPvPArenaSO>().matchTime;

        var levelController = ObjectFindCache<PBLevelController>.Get();
        m_PlaySoundCoroutine = StartCoroutine(CommonCoroutine.WaitUntil(() => levelController.RemainingTime > 0, () =>
        {
            m_PlaySoundCoroutine = StartCoroutine(CommonCoroutine.WaitUntil(() => levelController.RemainingTime <= m_TicTacTimeLeft, PlaySoundTicTac));
        }));
    }

    private void OnLevelEnded(object[] parameters)
    {
        if (parameters[0] is not List<Competitor> winners) return;
        if (m_PlaySoundCoroutine != null)
            StopCoroutine(m_PlaySoundCoroutine);
        SoundManager.Instance.StopLoopSFX(gameObject);

        bool isPlayerWin = false;
        foreach (var player in winners)
        {
            if (player.PersonalInfo.isLocal)
            {
                isPlayerWin = true;
            }
        }

        var levelController = ObjectFindCache<PBLevelController>.Get(isCallFromAwake: true);
        if (m_IsFTUE && levelController != null && levelController.RemainingTime <= 0)
        {
            ShowTimeUpNotification(isPlayerWin);
        }
    }

    private void OnRoundCompleted(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        if (parameters[0] is PBPvPMatch match)
        {
            if (!match.pbEndgameData.isTimesUp)
                return;
            ShowTimeUpNotification(match.isVictory);
        }
    }

    private void ShowTimeUpNotification(bool isVictory)
    {
        if (!isVictory)
            m_ContainerRectTransform.localPosition = Vector3.up * 53f;
        SetSummaryText(isVictory);
        m_TimeUpText.gameObject.SetActive(true);
        m_TimeUpText.transform.localScale = Vector3.zero;
        m_SummaryText.gameObject.SetActive(true);
        m_SummaryText.transform.localScale = Vector3.zero;
        m_TimeUpText.transform.DOScale(Vector3.one, AnimationDuration.SSHORT).OnComplete(() => m_SummaryText.transform.DOScale(Vector3.one, AnimationDuration.SSHORT));
    }

    private void OnShowGameOverUI()
    {
        m_TimeUpText.gameObject.SetActive(false);
        m_SummaryText.gameObject.SetActive(false);
    }

    private void PlaySoundTicTac()
    {
        SoundManager.Instance.PlayLoopSFX(PBSFX.UITimesUpTicTac, ownedGameObject: gameObject, loop: true);
    }

    private void SetSummaryText(bool isPlayerWin)
    {
        I2LTerm summaryTerm = isPlayerWin ? I2LTerm.PvP_TimesUpSummary_Win : I2LTerm.PvP_TimesUpSummary_Lost;
        m_SummaryText.text = I2LHelper.TranslateTerm(summaryTerm);
    }
}