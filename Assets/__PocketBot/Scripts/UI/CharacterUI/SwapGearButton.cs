using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HyrphusQ.Events;
using LatteGames.Template;
using Unity.VisualScripting;

public class SwapGearButton : MonoBehaviour
{
    Button button;
    [SerializeField] GearButton gearButton;
    [SerializeField] GearInfoPopup gearInfoPopup;
    CurrentGearHolder currentGearHolder;
    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(() => OnButtonClick());
        currentGearHolder = CurrentGearHolder.Instance;
    }
    private void OnButtonClick()
    {
        if (!RobotPreviewSpawner.Instance.CheckEnoughPower())
        {
            GameEventHandler.Invoke(CharacterUIEventCode.OnEquipNotEnoughPower);
            gearButton.DeSelect();
            return;
        }

        if(currentGearHolder.currentSO != null)
        if(currentGearHolder.currentSO.PartType != currentGearHolder.swapSO.PartType)
        {
            currentGearHolder.currentSO = null;
        }

        if(currentGearHolder.swapSO == null)
        {
            GearSaver.Instance.EquipGear(null, currentGearHolder.swapSlot);
            GearSaver.Instance.EquipGear(currentGearHolder.currentSO, currentGearHolder.currentSlot);
        }

        else if(currentGearHolder.swapSO == currentGearHolder.currentSO)
        {
            GearSaver.Instance.EquipGear(null, currentGearHolder.swapSlot);
            GearSaver.Instance.EquipGear(currentGearHolder.currentSO, currentGearHolder.currentSlot);
        }
        else if(currentGearHolder.currentSO == null)
        {
            GearSaver.Instance.EquipGear(null, currentGearHolder.swapSlot);
            GearSaver.Instance.EquipGear(currentGearHolder.swapSO, currentGearHolder.currentSlot);
        }
        else
        {
            GearSaver.Instance.EquipGear(currentGearHolder.currentSO, currentGearHolder.swapSlot);
            GearSaver.Instance.EquipGear(currentGearHolder.swapSO, currentGearHolder.currentSlot);

            currentGearHolder.currentSO.IsEquipped = true;
            currentGearHolder.swapSO.IsEquipped = true;

        }
        foreach (var item in currentGearHolder.GetComponent<GearListSelection>().gearButtons)
        {
            item.equippedOutline.enabled = false;
            item.equippedOutline.gameObject.SetActive(false);
        }

        gearButton.equippedOutline.enabled = true;
        gearButton.equippedOutline.gameObject.SetActive(true);

        GameEventHandler.Invoke(CharacterUIEventCode.OnSwapPart);
        gearButton.OnSwapEvent();
        gearButton.OnEquipEvent();

        if(gearInfoPopup  != null)
        {
            gearInfoPopup.hasSwap = true;
        }
    }
}
