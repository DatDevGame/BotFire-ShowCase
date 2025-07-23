using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LatteGames.Template;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LeagueDataSO;

public class CompetitionDurationNodeUI : MonoBehaviour
{
    [SerializeField]
    private Image m_PassedNodeImage, m_PresentNodeImage, m_UpcomingNodeImage;
    [SerializeField]
    private Image m_PassedLinkImage, m_UpcomingLinkImage;
    [SerializeField]
    private Image m_TickIconImage;
    [SerializeField]
    private TextMeshProUGUI m_DateText;

    public RectTransform rectTransform => transform as RectTransform;

    private float GetPassedLinkMaxWidth()
    {
        return rectTransform.sizeDelta.x * Mathf.Abs(m_PassedLinkImage.rectTransform.anchorMin.x - m_PassedLinkImage.rectTransform.anchorMax.x);
    }

    private float GetLinkProgress()
    {
        return Mathf.InverseLerp(-GetPassedLinkMaxWidth(), 0f, m_PassedLinkImage.rectTransform.sizeDelta.x);
    }

    private void UpdateLinkProgress(float value)
    {
        var rectTransform = m_PassedLinkImage.rectTransform;
        rectTransform.sizeDelta = Mathf.Lerp(-GetPassedLinkMaxWidth(), 0f, value) * Vector2.right;
        rectTransform.anchoredPosition = rectTransform.sizeDelta / 2f;
    }

    public void DisableLink()
    {
        m_PassedLinkImage.enabled = false;
        m_UpcomingLinkImage.enabled = false;
    }

    public void SetStatus(Status status, int dayOfWeek, bool showTickIcon = false)
    {
        m_PassedNodeImage.gameObject.SetActive(status == Status.Passed);
        m_PassedLinkImage.gameObject.SetActive(status == Status.Passed);
        m_PresentNodeImage.gameObject.SetActive(status == Status.Present);
        m_UpcomingNodeImage.gameObject.SetActive(status == Status.Upcoming);
        m_UpcomingLinkImage.gameObject.SetActive(status == Status.Upcoming || status == Status.Present);
        if (status == Status.Present)
        {
            m_DateText.SetText(dayOfWeek.ToString());
            m_DateText.gameObject.SetActive(!showTickIcon);
            m_TickIconImage.gameObject.SetActive(showTickIcon);
            m_PresentNodeImage.transform.localScale = Vector3.one;
        }
    }

    [Button]
    public Sequence PlayPassedAnimation(Status status, int dayOfWeek)
    {
        var sequence = DOTween.Sequence();
        if (status == Status.Passed)
        {
            m_UpcomingNodeImage.gameObject.SetActive(false);
            m_UpcomingLinkImage.gameObject.SetActive(true);
            m_PassedNodeImage.gameObject.SetActive(true);
            m_PassedNodeImage.rectTransform.localScale = Vector3.one;
            m_PassedLinkImage.gameObject.SetActive(true);
            m_PresentNodeImage.gameObject.SetActive(true);
            m_PresentNodeImage.rectTransform.localScale = Vector3.one;
            m_TickIconImage.gameObject.SetActive(true);
            m_DateText.gameObject.SetActive(false);
            UpdateLinkProgress(0f);
            sequence
                .Append(m_PresentNodeImage.rectTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBack))
                .OnStart(() => SoundManager.Instance.PlaySFX(PBSFX.UIProgressBarUp))
                .Join(m_PassedNodeImage.rectTransform.DOPunchScale(-0.1f * Vector3.one, 0.15f, 10, 1f).SetEase(Ease.Linear).SetDelay(0.5f * 0.7f))
                .Append(DOTween.To(GetLinkProgress, UpdateLinkProgress, 1f, 0.5f))
                .Play()
                .OnComplete(OnAnimationCompleted);
        }
        else if (status == Status.Present)
        {
            m_UpcomingNodeImage.gameObject.SetActive(true);
            m_PresentNodeImage.gameObject.SetActive(true);
            m_PresentNodeImage.rectTransform.localScale = Vector3.zero;
            m_DateText.SetText(dayOfWeek.ToString());
            sequence
                .Append(m_PresentNodeImage.rectTransform.DOScale(Vector3.one, AnimationDuration.SSHORT).SetEase(Ease.OutBack))
                .OnStart(() => SoundManager.Instance.PlaySFX(PBSFX.UINewWeek))
                .Play()
                .OnComplete(OnAnimationCompleted);
        }
        return sequence;

        void OnAnimationCompleted()
        {
            SetStatus(status, dayOfWeek);
        }
    }
}