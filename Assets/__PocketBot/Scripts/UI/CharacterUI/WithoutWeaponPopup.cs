using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using UnityEngine.UI;

public class WithoutWeaponPopup : MonoBehaviour
{
    [SerializeField] PPrefItemSOVariable currentChassis;
    [SerializeField] GameObject popup;

    void Awake()
    {
        GameEventHandler.AddActionEvent(CharacterUIEventCode.SendWarning, GetWarning);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.SendWarning, GetWarning);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, GetWarning);
    }

    void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.SendWarning, GetWarning);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.SendWarning, GetWarning);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, GetWarning);
    }

    private void Start()
    {
        GetWarning();
    }

    void GetWarning()
    {
        PBChassisSO chassisSO = (PBChassisSO)currentChassis.value;
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
        if (sendWarning)
        {
            popup.SetActive(true);
        }
        else
        {
            popup.SetActive(false);
        }
    }
}
