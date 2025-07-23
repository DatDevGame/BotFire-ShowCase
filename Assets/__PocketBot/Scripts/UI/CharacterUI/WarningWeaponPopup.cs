using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class WarningWeaponPopup : MonoBehaviour, IPopup
{
    public event Action<IPopup> onPopupClosed;

    [SerializeField] PPrefItemSOVariable currentChassis;
    [SerializeField] PPrefBoolVariable hasUpgradedFTUE;
    [SerializeField] CanvasGroupVisibility visibility;
    [SerializeField] Button BuildBtn, closeBtn;

    bool isInCharacterUI;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickNonCharacterTab);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, OnClickCharacterTab);
        closeBtn.onClick.AddListener(Hide);
        BuildBtn.onClick.AddListener(OnBuildBtnClicked);
    }

    private void Start()
    {
        HideImmediately();
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickNonCharacterTab);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, OnClickCharacterTab);
        closeBtn.onClick.RemoveListener(Hide);
        BuildBtn.onClick.RemoveListener(OnBuildBtnClicked);
    }

    void OnClickNonCharacterTab()
    {
        if (isInCharacterUI)
        {
            if (hasUpgradedFTUE.value)
            {
                if (IsNoWeapon())
                {
                    Show();
                }
                else
                {
                    HideImmediately();
                }
            }
            isInCharacterUI = false;
        }
    }

    void OnClickCharacterTab()
    {
        isInCharacterUI = true;
    }

    void OnBuildBtnClicked()
    {
        Hide();
        PBChassisSO chassisSO = (PBChassisSO)currentChassis.value;
        GearTabSelection gearTabSelection = ObjectFindCache<GearTabSelection>.Get();
        foreach (var slot in chassisSO.AllPartSlots)
        {
            if (slot.PartType == PBPartType.Upper)
            {
                gearTabSelection.SetDefaultReloadButton(PBPartSlot.Upper_1);
                break;
            }
            else if (slot.PartType == PBPartType.Front)
            {
                gearTabSelection.SetDefaultReloadButton(PBPartSlot.Front_1);
                break;
            }
        }
        GameEventHandler.Invoke(MainSceneEventCode.OnManuallyClickButton, ButtonType.Character);
    }

    bool IsNoWeapon()
    {
        PBChassisSO chassisSO = (PBChassisSO)currentChassis.value;
        bool isNoWeapon = true;

        for (int i = 0; i < chassisSO.AllPartSlots.Count; i++)
        {
            BotPartSlot botPartSlot = chassisSO.AllPartSlots[i];
            PBPartSO partSO = (PBPartSO)botPartSlot.PartVariableSO.value;

            if (botPartSlot.PartType == PBPartType.Upper)
            {
                if (partSO != null)
                {
                    isNoWeapon = false;
                    break;
                }
            }
            if (botPartSlot.PartType == PBPartType.Front)
            {
                if (partSO != null)
                {
                    isNoWeapon = false;
                    break;
                }
            }
        }
        return isNoWeapon;
    }

    public void Show()
    {
        visibility.Show();
    }

    public void Hide()
    {
        visibility.Hide();
    }

    public void HideImmediately()
    {
        visibility.HideImmediately();
    }

    public void Open()
    {
        throw new NotImplementedException();
    }
}