using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;
#if LatteGames_CLIK
using Tabtale.TTPlugins;
#endif

public class SettingUI : MonoBehaviour
{
    [SerializeField] CanvasGroupVisibility visibility;
    [SerializeField] Button rateUsBtn;
    [SerializeField] Button termsOfUseBtn;
    [SerializeField] Button privacyPolicyBtn;
    [SerializeField] Button privacySettingBtn;
    [SerializeField] GameObject panel;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.ShowSettingsUI, Show);
        GameEventHandler.AddActionEvent(MainSceneEventCode.HideSettingUI, Hide);
        rateUsBtn.onClick.AddListener(RateUs);
        privacySettingBtn.onClick.AddListener(OnPrivacySettingBtnClicked);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.ShowSettingsUI, Show);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.HideSettingUI, Hide);
        rateUsBtn.onClick.RemoveListener(RateUs);
        privacySettingBtn.onClick.RemoveListener(OnPrivacySettingBtnClicked);
    }

    void RateUs()
    {
#if LatteGames_CLIK
        TTPRateUs.Button();
#endif
    }

    void OnPrivacySettingBtnClicked()
    {
#if LatteGames_CLIK
        TTPPrivacySettings.ShowPrivacySettings(null);
#endif
    }

    void Show()
    {
        CheckToShowButton();
        visibility.Show();
    }

    void Hide()
    {
        visibility.Hide();
    }

    void CheckToShowButton()
    {
#if LatteGames_CLIK
        bool ShouldShowPrivacySettings()
        {
            TTPPrivacySettings.ConsentType currentConsent = TTPPrivacySettings.CustomConsentGetConsent();

            if (currentConsent == TTPPrivacySettings.ConsentType.NPA || currentConsent == TTPPrivacySettings.ConsentType.PA)
                return true;

            return false;
        }
        if (ShouldShowPrivacySettings())
        {
            // TODO :: show Privacy Settings button
            privacySettingBtn.gameObject.SetActive(true);
            termsOfUseBtn.gameObject.SetActive(false);
            privacyPolicyBtn.gameObject.SetActive(false);
        }
        else
        {
            // TODO :: show Privacy Policy and Terms of Use buttons
            privacySettingBtn.gameObject.SetActive(false);
            termsOfUseBtn.gameObject.SetActive(true);
            privacyPolicyBtn.gameObject.SetActive(true);
        }
        panel.SetActive(false);
        panel.SetActive(true);
#endif
    }
}