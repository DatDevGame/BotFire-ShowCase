using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class SeasonUnlockScreen : MonoBehaviour
{
    [SerializeField] CanvasGroupVisibility visibility;
    [SerializeField] Button screenBtn;
    [SerializeField] private Transform m_IconCard;
    [SerializeField] private ParticleSystem m_ConfettiShowerRainbow_UI;
    [SerializeField] private AudioSource m_AudioSource;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(SeasonPassEventCode.ShowSeasonUnlockScreen, ShowSeasonUnlockScreen);
        screenBtn.onClick.AddListener(OnScreenBtnClick);

        m_IconCard.DOLocalRotate(new Vector3(0, 0, 5), 1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

        m_IconCard.DOScale(new Vector3(1.05f, 1.05f, 1.05f), 1.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.ShowSeasonUnlockScreen, ShowSeasonUnlockScreen);
        screenBtn.onClick.RemoveListener(OnScreenBtnClick);
    }

    private void OnScreenBtnClick()
    {
        visibility.Hide();
        SeasonPassManager.Instance.isAllowShowPopup = false;
        GameEventHandler.Invoke(SeasonPassEventCode.UpdateSeasonUI);

        if (m_AudioSource != null)
            m_AudioSource.Stop();
    }

    private void ShowSeasonUnlockScreen()
    {
        visibility.Show();

        if (gameObject.activeInHierarchy)
        {
            string keyCallOneTime = "SeasonUnlockScreen-Sound";
            if (!PlayerPrefs.HasKey(keyCallOneTime))
            {
                PlayerPrefs.SetInt(keyCallOneTime, 1);

                if (m_ConfettiShowerRainbow_UI != null)
                    m_ConfettiShowerRainbow_UI.Play();

                if (m_AudioSource != null)
                    m_AudioSource.Play();
            }

        }
    }
}
