using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class FTUEStartUnlockBoxSlot : MonoBehaviour
{
    [SerializeField, BoxGroup("FTUE")] protected MultiImageButton packDockOpenByGem;
    [SerializeField, BoxGroup("FTUE")] protected Button closeButton;
    [SerializeField, BoxGroup("FTUE")] protected MultiImageButton startUnlockBtn;
    [SerializeField, BoxGroup("FTUE")] protected MultiImageButton openNowBuyAds;
    [SerializeField, BoxGroup("FTUE")] protected GameObject ftueHand;
    [SerializeField, BoxGroup("FTUE")] protected PPrefBoolVariable startUnlockBoxSlotFTUE;
    [SerializeField, BoxGroup("FTUE")] protected PPrefBoolVariable startUnlockBoxSlotTheFirstTimeFTUE;

    private void Awake()
    {
        startUnlockBtn.onClick.AddListener(ActionClickStartUnlock);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnOpenBoxSlotFTUE, OnOpenBoxSlotFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnClickBoxTheFisrtTime, OnClickBoxTheFisrtTime);
    }

    private void OnDestroy()
    {
        startUnlockBtn.onClick.RemoveListener(ActionClickStartUnlock);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnOpenBoxSlotFTUE, OnOpenBoxSlotFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnClickBoxTheFisrtTime, OnClickBoxTheFisrtTime);
    }

    private void ActionClickStartUnlock()
    {
        if (!startUnlockBoxSlotFTUE.value)
        {
            startUnlockBoxSlotFTUE.value = true;
            closeButton.interactable = true;
            packDockOpenByGem.interactable = true;
            openNowBuyAds.gameObject.SetActive(true);
            ftueHand.SetActive(false);
            GameEventHandler.Invoke(FTUEEventCode.OnClickStartUnlockBoxSlotFTUE);
            return;
        }

        if (!startUnlockBoxSlotTheFirstTimeFTUE.value)
        {
            startUnlockBoxSlotTheFirstTimeFTUE.value = true;
            closeButton.interactable = true;
            packDockOpenByGem.interactable = true;
            openNowBuyAds.gameObject.SetActive(true);
            ftueHand.SetActive(false);
            GameEventHandler.Invoke(FTUEEventCode.OnClickStartUnlockBoxTheFirstTime);
        }
    }

    private void OnOpenBoxSlotFTUE(params object[] eventData)
    {
        if (!startUnlockBoxSlotFTUE.value)
        {
            ftueHand.SetActive(true);
            openNowBuyAds.gameObject.SetActive(false);
            closeButton.interactable = false;
            packDockOpenByGem.interactable = false;
            GameEventHandler.Invoke(FTUEEventCode.OnClickStartUnlockBoxSlotFTUE, startUnlockBtn.gameObject);
        }
    }

    private void OnClickBoxTheFisrtTime()
    {
        if (!startUnlockBoxSlotTheFirstTimeFTUE.value)
        {
            ftueHand.SetActive(true);
            openNowBuyAds.gameObject.SetActive(startUnlockBoxSlotTheFirstTimeFTUE.value);
            closeButton.interactable = startUnlockBoxSlotTheFirstTimeFTUE.value;
            packDockOpenByGem.interactable = false;
            GameEventHandler.Invoke(FTUEEventCode.OnClickStartUnlockBoxTheFirstTime, startUnlockBtn.gameObject);
        }
    }
}
