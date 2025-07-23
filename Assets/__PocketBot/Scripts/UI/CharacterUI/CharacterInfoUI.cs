using System.Collections;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CharacterInfoUI : MonoBehaviour
{
    public GameObject PowerPanel => powerPanel;
    public DOTweenAnimation BreakthingAnimation => breakthingAnimation;

    [SerializeField, BoxGroup("Ref")] private Image iconThunder;
    [SerializeField, BoxGroup("Ref")] private GameObject textIconThunder;
    [SerializeField, BoxGroup("Ref")] private GameObject powerPanel;
    [SerializeField, BoxGroup("Ref")] private GameObject backGroundPowerPanel;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI overallScore;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI HP;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI Atk;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI Power;
    [SerializeField, BoxGroup("Ref")] private DOTweenAnimation breakthingAnimation;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility warning_NotEnoughPowerCanvasGroup;

    [SerializeField, BoxGroup("Resource")] private Sprite bigThunderEnoughThePower;
    [SerializeField, BoxGroup("Resource")] private Sprite bigThunderOverThePower;

    [SerializeField, BoxGroup("Data")] PPrefItemSOVariable currentChassis;

    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable overThePowerLimit;

    private bool isOverPower = false;
    private void Awake()
    {
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnShowPowerInfo, ShowPowerInfo);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnHidePowerInfo, HidePowerInfo);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnSendBotInfo, SetInfo);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnEquipNotEnoughPower, OnNotEnoughPower);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnEquipEnoughPower, OnEquipEnoughPower);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnShowPowerInfo, ShowPowerInfo);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnHidePowerInfo, HidePowerInfo);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnSendBotInfo, SetInfo);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnEquipNotEnoughPower, OnNotEnoughPower);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnEquipEnoughPower, OnEquipEnoughPower);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }
    private void ShowPowerInfo()
    {
        //Power.transform.parent.gameObject.SetActive(true);
    }
    private void HidePowerInfo()
    {
        //Power.transform.parent.gameObject.SetActive(false);
    }
    void SetInfo(params object[] parameters)
    {
        float _HP = (float)parameters[0];
        float _ATK = (float)parameters[1];
        float _AllPartPower = (float)parameters[2];
        float _ChassisPower = (float)parameters[3];
        float _Overall = (float)parameters[4];

        if (overallScore != null)
        {
            overallScore.SetText(_Overall.ToRoundedText());
        }
        HP.SetText(_HP.ToRoundedText());
        Atk.SetText(_ATK.ToRoundedText());

        bool isOverPower = _AllPartPower > _ChassisPower;
        this.isOverPower = isOverPower;
        iconThunder.sprite = isOverPower ? bigThunderOverThePower : bigThunderEnoughThePower;
        textIconThunder.SetActive(isOverPower);

        if (isOverPower)
        {
            warning_NotEnoughPowerCanvasGroup.Show();
            Power.SetText($"<color=#FF0000>{_AllPartPower.ToRoundedText()}</color>/{_ChassisPower.ToRoundedText()}");
        }
        else
        {
            warning_NotEnoughPowerCanvasGroup.Hide();
            Power.SetText($"{_AllPartPower.ToRoundedText()}/{_ChassisPower.ToRoundedText()}");
        }

        if (GetHasEquipWeapons())
        {
            warning_NotEnoughPowerCanvasGroup.Hide();
        }
    }

    private void OnModelSpawned(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        PBRobot pBRobot = parameters[0] as PBRobot;
        powerPanel.SetActive(!pBRobot.ChassisInstance.ChassisSO.IsSpecial);
    }

    [ContextMenu("Play Animation")]
    public void OnNotEnoughPower()
    {
        if (!overThePowerLimit.value)
        {
            overThePowerLimit.value = true;
            GameEventHandler.Invoke(FTUEEventCode.OnShowEnergyFTUE, 1);
        }

        StartCoroutine(CR_OnNotEnoughPower());
    }

    private void OnEquipEnoughPower()
    {
        backGroundPowerPanel.transform
            .DOScale(new Vector3(1.2f, 1.2f, 1f), 0.4f) // Scale up to 120% over 0.8 seconds
            .SetEase(Ease.InOutSine) // Smooth in and out easing
            .OnComplete(() =>
            {
                // Once scaled up, scale back down
                backGroundPowerPanel.transform
                    .DOScale(Vector3.one, 0.2f) // Scale back to normal size over 0.8 seconds
                    .SetEase(Ease.InOutSine); // Smooth in and out easing
            });

    }

    IEnumerator CR_OnNotEnoughPower()
    {
        backGroundPowerPanel.transform.DOPunchPosition(Vector2.right * 8f, 1.5f, 15);
        yield return null;
    }

    private bool GetHasEquipWeapons()
    {
        PBChassisSO chassisSO = (PBChassisSO)currentChassis.value;
        if (chassisSO == null) return true;
        bool sendWarning = true;
        for (int i = 0; i < chassisSO.AllPartSlots.Count; i++)
        {
            BotPartSlot botPartSlot = chassisSO.AllPartSlots[i];
            PBPartSO partSO = (PBPartSO)botPartSlot.PartVariableSO.value;

            if (botPartSlot.PartType == PBPartType.Upper)
            {
                if (partSO != null)
                {
                    sendWarning = false;
                    break;
                }
            }
            if (botPartSlot.PartType == PBPartType.Front)
            {
                if (partSO != null)
                {
                    sendWarning = false;
                    break;
                }
            }
        }

        return sendWarning;
    }
}
