using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HyrphusQ.SerializedDataStructure;
using LatteGames;
using HyrphusQ.Events;
using System.Linq;
using LatteGames.Monetization;
using HightLightDebug;

public enum WarningOutOfEnergyState
{
    Open,
    OpenAutoCharge
}

public enum DescriptionEnergyPopup
{
    OutOfEnergyText,
    RobotIsChargingText,
    RobotIsFullyChargedText,
}

public enum ButtonEnergyPopup
{
    ChangeBot,
    Revive,
    Ready
}
public class WarningOutOfEnergyPopup : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private Transform bodyContent;
    [SerializeField, BoxGroup("Ref")] private Image iconPopup;
    [SerializeField, BoxGroup("Ref")] private Image sunburstImage;
    [SerializeField, BoxGroup("Ref")] private EnergyProgressBarUI energyProgressBarUI;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility warningCanvasGroupVisibility;
    [SerializeField, BoxGroup("Ref")] private RVButtonBehavior reviveRVBtn;
    [SerializeField, BoxGroup("Ref")] private SerializedDictionary<DescriptionEnergyPopup, TMP_Text> DescText;
    [SerializeField, BoxGroup("Ref")] private SerializedDictionary<ButtonEnergyPopup, MultiImageButton> groupButton;
    [SerializeField, BoxGroup("Data")] private BossChapterSO bossChapterSO;

    private PBChassisSO currentChassisSO;
    private DockerController dockerController;
    private List<GearButton> gearButtonCharges;
    private AdsLocation adsLocationCurrent;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(WarningOutOfEnergyState.Open, OpenPopupHandle);
        GameEventHandler.AddActionEvent(WarningOutOfEnergyState.OpenAutoCharge, OpenAutoCharge);
        reviveRVBtn.OnRewardGranted += ReviveRVBtn_OnRewardGranted;
        groupButton[ButtonEnergyPopup.ChangeBot].onClick.AddListener(ChangeBotButton);
        groupButton[ButtonEnergyPopup.Ready].onClick.AddListener(ImReadyButton);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(WarningOutOfEnergyState.Open, OpenPopupHandle);
        GameEventHandler.RemoveActionEvent(WarningOutOfEnergyState.OpenAutoCharge, OpenAutoCharge);
        reviveRVBtn.OnRewardGranted -= ReviveRVBtn_OnRewardGranted;
        groupButton[ButtonEnergyPopup.ChangeBot].onClick.RemoveListener(ChangeBotButton);
        groupButton[ButtonEnergyPopup.Ready].onClick.RemoveListener(ImReadyButton);
    }

    private void Start()
    {
        dockerController = FindObjectOfType<DockerController>();

        PBChassisSO pbChassisSO = GearSaver.Instance.chassisSO.value.Cast<PBChassisSO>();
        if (pbChassisSO.IsSpecial)
        {
            if (pbChassisSO.TryGetModule<EnergyItemModule>(out var energyItemModule))
            {
                if (energyItemModule.IsOutOfEnergy())
                {
                    #region Design Event
                    GameEventHandler.Invoke(DesignEvent.BossOutEnergy, pbChassisSO);
                    DebugPro.YellowBold($"Boss Out Energy: {pbChassisSO.GetDisplayName()}");
                    #endregion
                }
            }
        }
    }

    private void OpenPopupHandle(params object[] parrams)
    {
        adsLocationCurrent = AdsLocation.RV_Recharge_Boss_Alert_Popup;
        OpenWarningPopup(parrams);
    }

    private void OpenWarningPopup(params object[] parrams)
    {
        // bool isReturn = false;
        // if (abTestEnergyBossSO != null)
        // {
        //     if (abTestEnergyBossSO.GetGearCardPrefab() != null)
        //         isReturn = abTestEnergyBossSO.GetGearCardPrefab().EnergyProgressBarUI == null;
        // }
        // if (isReturn) return;

        if (parrams.Length <= 0 || parrams[0] == null) return;
        if (parrams[0] is not PBChassisSO) return;

        sunburstImage.gameObject.SetActive(false);
        groupButton[ButtonEnergyPopup.ChangeBot].gameObject.SetActive(true);
        groupButton[ButtonEnergyPopup.Revive].gameObject.SetActive(true);
        groupButton[ButtonEnergyPopup.Ready].gameObject.SetActive(false);

        PBChassisSO pbChassisSO = parrams[0] as PBChassisSO;
        currentChassisSO = pbChassisSO;
        iconPopup.sprite = bossChapterSO.bossList.Find(v => v.chassisSO == pbChassisSO).botInfo.avatar;
        EnableDescription(DescriptionEnergyPopup.OutOfEnergyText);
        energyProgressBarUI.Setup(pbChassisSO, false);

        if (parrams.Length < 2)
            warningCanvasGroupVisibility.Show();

        #region Design Event
        if (adsLocationCurrent == AdsLocation.RV_Recharge_Boss_Alert_Popup)
        {
            string status = $"Start";
            GameEventHandler.Invoke(DesignEvent.BossOutEnergyAlert, currentChassisSO, status);
        }
        #endregion
    }

    private void ChangeBotButton()
    {
        if (dockerController != null)
            dockerController.SelectManuallyButtonOfType(ButtonType.Character);

        warningCanvasGroupVisibility.Hide();

        #region Design Event
        string status = $"Complete";
        GameEventHandler.Invoke(DesignEvent.BossOutEnergyAlert, currentChassisSO, status);
        #endregion
    }

    private void ImReadyButton()
    {
        warningCanvasGroupVisibility.Hide();
    }

    private void OpenAutoCharge(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null) return;

        if (parrams[1] != null && parrams[1] is List<GearButton>)
        {
            gearButtonCharges = new List<GearButton>();
            gearButtonCharges = parrams[1] as List<GearButton>;
        }

        AdsLocation adsLocation = AdsLocation.RV_Recharge_Boss_Alert_Popup;
        if (parrams[2] != null && parrams[2] is AdsLocation adsLocationEnum)
            adsLocation = adsLocationEnum;

        adsLocationCurrent = adsLocation;
        OpenWarningPopup(parrams);
        HandleStartCharge(adsLocation);
    }

    private void ReviveRVBtn_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData obj)
    {
        HandleStartCharge(AdsLocation.RV_Recharge_Boss_Alert_Popup);
    }

    private void HandleStartCharge(AdsLocation adsLocationEnum)
    {
        #region Design Event
        if (adsLocationEnum == AdsLocation.RV_Recharge_Boss_Alert_Popup)
        {
            string status = $"Complete";
            GameEventHandler.Invoke(DesignEvent.BossOutEnergyAlert, currentChassisSO, status);
        }
        #endregion

        #region MonetizationEventCode
        MonetizationEventCode RechargeBossMonetization = adsLocationEnum switch
        {
            AdsLocation.RV_Recharge_Boss_Alert_Popup => MonetizationEventCode.RechargeBoss_AlertPopup,
            AdsLocation.RV_Recharge_Boss_Build_UI => MonetizationEventCode.RechargeBoss_BuildUI,
            AdsLocation.RV_Recharge_Boss_Popup => MonetizationEventCode.RechargeBoss_BossPopup,
            _ => MonetizationEventCode.RechargeBoss_AlertPopup
        };

        string adsLocation = adsLocationEnum switch
        {
            AdsLocation.RV_Recharge_Boss_Alert_Popup => "AlertPopup",
            AdsLocation.RV_Recharge_Boss_Build_UI => "BuildUI",
            AdsLocation.RV_Recharge_Boss_Popup => "BossPopup",
            _ => "AlertPopup"
        };

        string bossName = $"{currentChassisSO.GetModule<NameItemModule>().displayName}";
        GameEventHandler.Invoke(RechargeBossMonetization, adsLocation, bossName);
        #endregion

        groupButton[ButtonEnergyPopup.ChangeBot].gameObject.SetActive(false);
        groupButton[ButtonEnergyPopup.Revive].gameObject.SetActive(false);
        groupButton[ButtonEnergyPopup.Ready].gameObject.SetActive(true);
        // StartCoroutine(ChargingEnergy());
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            currentChassisSO = GearSaver.Instance.chassisSO.value as PBChassisSO;
            if (currentChassisSO.TryGetModule<EnergyItemModule>(out var energyItemModule))
            {
                energyItemModule.ConsumeEnergy(5);
            }
        }
    }
#endif

    // private IEnumerator ChargingEnergy()
    // {
    //     float timeLoadEnergy = 0.2f;
    //     WaitForSeconds waitForSeconds = new WaitForSeconds(timeLoadEnergy);
    //     groupButton[ButtonEnergyPopup.Ready].interactable = false;
    //     sunburstImage.gameObject.SetActive(false);
    //     bool isBreak = false;
    //     EnableDescription(DescriptionEnergyPopup.RobotIsChargingText);
    //     while (true)
    //     {
    //         if (currentChassisSO.TryGetModule<EnergyItemModule>(out var energyItemModule))
    //         {
    //             energyItemModule.RefillEnergy(1);
    //             energyProgressBarUI.Setup(currentChassisSO, true);

    //             if (gearButtonCharges != null)
    //             {
    //                 if (gearButtonCharges.Count > 0)
    //                 {
    //                     for (int i = 0; i < gearButtonCharges.Count; i++)
    //                     {
    //                         if (gearButtonCharges[i] != null)
    //                             gearButtonCharges[i].LoadEnergyUI(true);
    //                     }
    //                 }
    //             }

    //             if (energyItemModule.IsFullEnergy())
    //             {
    //                 isBreak = true;

    //                 if (gearButtonCharges != null)
    //                 {
    //                     foreach (var gearButton in gearButtonCharges)
    //                     {
    //                         if (gearButton != null)
    //                             gearButton.EnergyProgressBarUI.OnAnimationFullEnergy();
    //                     }
    //                 }
    //                 energyProgressBarUI.OnAnimationFullEnergy();

    //                 if (gearButtonCharges != null)
    //                     gearButtonCharges.Clear();

    //                 sunburstImage.gameObject.SetActive(true);
    //                 EnableDescription(DescriptionEnergyPopup.RobotIsFullyChargedText);
    //                 yield return new WaitForSeconds(0.5f);
    //                 groupButton[ButtonEnergyPopup.Ready].interactable = true;
    //             }
    //         }
    //         else
    //             yield break;

    //         if (isBreak)
    //             yield break;

    //         yield return waitForSeconds;
    //     }
    // }


    private void EnableDescription(DescriptionEnergyPopup descriptionEnergy)
    {
        for (int i = 0; i < DescText.Count; i++)
        {
            DescText.ElementAt(i).Value.gameObject.SetActive(false);
        }
        DescText[descriptionEnergy].gameObject.SetActive(true);
    }
}
