using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.GUI;
using I2.Loc;
using LatteGames;
using LatteGames.Monetization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ReviveUI : MonoBehaviour
{
    [SerializeField]
    private Image m_CountdownProgressImage;
    [SerializeField]
    private TextMeshProUGUI m_CountdownText;
    [SerializeField]
    private Button m_NoThanksButton;
    [SerializeField]
    private TextAdapter m_GemToReviveText;
    [SerializeField]
    private Button m_ReviveByGemButton;
    [SerializeField]
    private RVButtonBehavior m_ReviveByRVButton;
    [SerializeField]
    private LocalizationParamsManager m_RevivesLeftParams;
    [SerializeField]
    private CanvasGroupVisibility m_UIVisibility;
    [SerializeField]
    private EZAnimBase m_ShowHidePanelAnim;
    [SerializeField]
    private ReviveDataSO m_ReviveDataSO;

    private Action<bool> m_ReviveCallback;
    private Coroutine m_StartCountdownCoroutine;

    private void Awake()
    {
        m_NoThanksButton.onClick.AddListener(() =>
        {
            m_ReviveCallback?.Invoke(false);
            Hide();
        });
        m_ReviveByGemButton.onClick.AddListener(() =>
        {
            // TODO: Require GD provide information about the ResourceLocation and ItemId for Sink/Source event
            CurrencyManager.Instance[CurrencyType.Premium].SpendWithoutLogEvent(m_ReviveDataSO.requiredNumOfGemsToRevive);
            m_ReviveCallback?.Invoke(true);
            Hide();
        });
        m_ReviveByRVButton.OnStartWatchAds += () =>
        {
            Hide();
        };
        m_ReviveByRVButton.OnFailedWatchAds += () =>
        {
            StartCoroutine(OnFailedWatchAds());
            IEnumerator OnFailedWatchAds()
            {
                yield return new WaitForSecondsRealtime(0.5f);
                m_ReviveCallback?.Invoke(false);
            }
        };
        m_ReviveByRVButton.OnRewardGranted += eventData =>
        {
            m_ReviveCallback?.Invoke(true);

            #region MonetizationEventCode
            string adsLocation = "InMatch";
            int bossID = GetCurrentBossID();
            GameEventHandler.Invoke(MonetizationEventCode.ReviveBoss, adsLocation, bossID);
            #endregion
        };
    }

    private void StopCountdownCoroutine()
    {
        if (m_StartCountdownCoroutine != null)
            StopCoroutine(m_StartCountdownCoroutine);
        m_StartCountdownCoroutine = null;
    }

    private void ShowNoThanksButton()
    {
        m_NoThanksButton.gameObject.SetActive(true);
    }

    private IEnumerator StartCountdown_CR(int countdownTime, float timeToShowNoThanksButton)
    {
        var isShowNoThanksButton = false;
        var timeSinceStartCountdown = Time.unscaledTime;
        while (true)
        {
            var deltaTime = Time.unscaledTime - timeSinceStartCountdown;
            m_CountdownProgressImage.fillAmount = 1f - Mathf.Clamp01(deltaTime / countdownTime);
            m_CountdownText.SetText(Mathf.CeilToInt(Mathf.Clamp(countdownTime - deltaTime, 0f, countdownTime)).ToString());
            if (deltaTime >= timeToShowNoThanksButton && !isShowNoThanksButton)
            {
                isShowNoThanksButton = true;
                ShowNoThanksButton();
            }
            if (deltaTime > countdownTime)
            {
                break;
            }
            yield return null;
        }
        m_ReviveCallback?.Invoke(false);
        Hide();
    }

    public void Show(Action<bool> callback)
    {
        var isEnoughGemToRevive = CurrencyManager.Instance[CurrencyType.Premium].IsAffordable(m_ReviveDataSO.requiredNumOfGemsToRevive);
        m_ReviveByGemButton.interactable = isEnoughGemToRevive;
        m_ReviveByGemButton.GetComponent<GrayscaleUI>()?.SetGrayscale(!isEnoughGemToRevive);
        m_NoThanksButton.gameObject.SetActive(false);
        m_RevivesLeftParams.SetParameterValue("RemainedReviveTimes", m_ReviveDataSO.remainedReviveTimes.ToString());
        m_RevivesLeftParams.SetParameterValue("MaxReviveTimes", m_ReviveDataSO.maxReviveTimes.ToString());
        m_GemToReviveText.SetText(m_GemToReviveText.blueprintText.Replace(Const.StringValue.PlaceholderValue, m_ReviveDataSO.requiredNumOfGemsToRevive.ToString()));
        m_ReviveCallback = callback;
        m_StartCountdownCoroutine = StartCoroutine(StartCountdown_CR(m_ReviveDataSO.countdownTime, m_ReviveDataSO.timeToShowNoThanksButton));
        m_UIVisibility.ShowImmediately();
        m_ShowHidePanelAnim.Play();

        #region Design Event
        string bossName = $"{BossFightManager.Instance.bossMapSO.currentBossSO.chassisSO.GetModule<NameItemModule>().displayName}";
        string status = $"Start";
        GameEventHandler.Invoke(DesignEvent.RevivePopup, bossName, status);

        //=============
        int bossID = GetCurrentBossID();
        string rvName = $"Revive_Boss{bossID}";
        string location = "InMatch";
        GameEventHandler.Invoke(DesignEvent.RVShow, rvName, location);
        #endregion
    }

    public void Hide()
    {
        StopCountdownCoroutine();
        //TODO: Hide IAP & Popup
        //m_UIVisibility.HideImmediately();

        #region Design Event
        string bossName = $"{BossFightManager.Instance.bossMapSO.currentBossSO.chassisSO.GetModule<NameItemModule>().displayName}";
        string status = $"Complete";
        GameEventHandler.Invoke(DesignEvent.RevivePopup, bossName, status);
        #endregion
    }

    public int GetCurrentBossID()
    {
        BossChapterSO bossChapterSO = BossFightManager.Instance.bossMapSO.GetBossChapterDefault;
        BossSO currentBossSO = BossFightManager.Instance.bossMapSO.currentBossSO;
        int bossID = bossChapterSO.bossList.FindIndex(v => v == currentBossSO) + 1;

        return bossID;
    }
}