using HyrphusQ.Events;
using LatteGames;
using LatteGames.Template;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TeamMatchRevive : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private ReviveSystem m_ReviveSystem;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_RevivePanelCanvasGroup;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_RespawnText;


    [SerializeField, BoxGroup("Config")] private float m_ReviveDuration;

    private IEnumerator ShowRespawnCountdownCR;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnAnyPlayerDied(params object[] parameters)
    {
        PBRobot pbRobot = (PBRobot)parameters[0];
        if (pbRobot.IsDead && m_ReviveSystem != null && !pbRobot.PlayerInfoVariable.value.personalInfo.isLocal)
            StartCoroutine(ReviveWithDelay(pbRobot, m_ReviveDuration));
        else if (pbRobot.IsDead && m_ReviveSystem != null && pbRobot.PlayerInfoVariable.value.personalInfo.isLocal)
        {
            ShowRespawnCountdownCR = ShowRespawnCountdown(pbRobot, m_ReviveDuration);
            StartCoroutine(ShowRespawnCountdownCR);
        }
    }
    private void OnFinalRoundCompleted(params object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;

        if (ShowRespawnCountdownCR != null)
            StopCoroutine(ShowRespawnCountdownCR);
        m_RevivePanelCanvasGroup.Hide();
    }
    private IEnumerator ReviveWithDelay(PBRobot robot, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (robot != null && robot.IsDead)
        {
            Action OnCompletedRevive = () =>
            {
                robot.BotController.StartStateMachine();
            };

            m_ReviveSystem.ReviveBot(robot, OnCompletedRevive);
        }
    }

    private IEnumerator ShowRespawnCountdown(PBRobot robot, float time)
    {
        yield return new WaitForSeconds(1f);
        m_RespawnText.text = $"Respawn in {time}s...";
        SoundManager.Instance.PlaySFX(SFX.Countdown, 1f);
        m_RevivePanelCanvasGroup.Show();
        yield return new WaitForSeconds(0.5f);
        float remaining = time;
        while (remaining > 0f)
        {
            m_RespawnText.text = $"Respawn in {Mathf.CeilToInt(remaining)}s...";
            SoundManager.Instance.PlaySFX(SFX.Countdown, 1f);
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        m_RespawnText.text = "Ready!";
        yield return new WaitForSeconds(1f); // Hiện chữ "Ready!" trong 1 giây
        m_RevivePanelCanvasGroup.Hide();

        Action OnCompletedRevive = () =>
        {
            robot.BotController.StartStateMachine();
        };
        m_ReviveSystem.ReviveBot(robot, OnCompletedRevive);
        SoundManager.Instance.PlaySFX(SFX.Respawn, 1f);
    }

}
