using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CombatEffectStatusUI : MonoBehaviour
{
    [SerializeField]
    private HealthBar m_HealthBar;
    [SerializeField]
    private Transform m_PlayerInfoUI;
    [SerializeField]
    private Transform m_StunEffectUI;
    [SerializeField]
    private RoundedProgressBar m_StunProgressBar;

    private Coroutine m_ShowStunUICoroutine;

    private void Awake()
    {
        m_StunEffectUI.gameObject.SetActive(false);
        m_HealthBar.onSetupCompleted += OnSetupCompleted;
    }

    private void OnDestroy()
    {
        m_HealthBar.onSetupCompleted -= OnSetupCompleted;
        if (m_HealthBar.Competitor != null && m_HealthBar.Competitor is PBRobot robot)
        {
            robot.CombatEffectController.onEffectApplied -= OnEffectApplied;
            robot.CombatEffectController.onEffectStacked -= OnEffectApplied;
            robot.CombatEffectController.onEffectRemoved -= OnEffectRemoved;
        }
    }

    private void OnSetupCompleted()
    {
        if (m_HealthBar.Competitor != null && m_HealthBar.Competitor is PBRobot robot)
        {
            robot.CombatEffectController.onEffectApplied -= OnEffectApplied;
            robot.CombatEffectController.onEffectStacked -= OnEffectApplied;
            robot.CombatEffectController.onEffectRemoved -= OnEffectRemoved;
            robot.CombatEffectController.onEffectApplied += OnEffectApplied;
            robot.CombatEffectController.onEffectStacked += OnEffectApplied;
            robot.CombatEffectController.onEffectRemoved += OnEffectRemoved;
        }
    }

    private void OnEffectRemoved(CombatEffect effect)
    {
        if ((effect.effectStatus & CombatEffectStatuses.Stunned) != 0)
        {
            OnRecoveryFromStunned();
        }
    }

    private void OnEffectApplied(CombatEffect effect)
    {
        if ((effect.effectStatus & CombatEffectStatuses.Stunned) != 0)
        {
            OnBeingStunnedOrSlowed(effect.remainingDuration);
        }
    }

    private void OnRecoveryFromStunned()
    {
        if (m_ShowStunUICoroutine != null)
            StopCoroutine(m_ShowStunUICoroutine);
        m_StunEffectUI.transform.localScale = Vector3.zero;
        m_StunEffectUI.gameObject.SetActive(false);
        m_PlayerInfoUI.gameObject.SetActive(true);
        m_ShowStunUICoroutine = null;
    }

    private void OnBeingStunnedOrSlowed(float stunDuration)
    {
        if (!gameObject.activeSelf)
            return;
        if (m_ShowStunUICoroutine != null)
            StopCoroutine(m_ShowStunUICoroutine);
        m_ShowStunUICoroutine = StartCoroutine(ShowStunUI_CR(stunDuration));
    }

    private IEnumerator ShowStunUI_CR(float stunDuration)
    {
        if (!m_StunEffectUI.gameObject.activeSelf)
        {
            m_StunEffectUI.gameObject.SetActive(true);
            m_StunEffectUI.transform.localScale = Vector3.zero;
            m_StunEffectUI.DOScale(Vector3.one, AnimationDuration.TINY).SetEase(Ease.OutBounce);
            m_PlayerInfoUI.gameObject.SetActive(false);
        }
        float remainingStunTime = stunDuration;
        while (remainingStunTime > 0f)
        {
            float t = remainingStunTime / stunDuration;
            m_StunProgressBar.fillAmount = t;
            remainingStunTime -= Time.deltaTime;
            yield return null;
        }
        m_StunEffectUI.transform.localScale = Vector3.zero;
        m_StunEffectUI.gameObject.SetActive(false);
        m_PlayerInfoUI.gameObject.SetActive(true);
        m_ShowStunUICoroutine = null;
    }
}