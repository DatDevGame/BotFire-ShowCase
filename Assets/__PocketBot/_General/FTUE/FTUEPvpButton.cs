using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;

public class FTUEPvpButton : MonoBehaviour
{
    [SerializeField] GameObject ftueHand;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, HandleOnTapOnMainButton);
    }

    private void OnDestroy()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, HandleOnTapOnMainButton);
    }

    void HandleOnTapOnMainButton()
    {
        if (!FTUEMainScene.Instance.FTUE_Equip.value) return;
        if (!FTUEMainScene.Instance.FTUEUpgrade.value) return;
        if (FTUEMainScene.Instance.FTUE_PVP.value) return;
        ftueHand.SetActive(true);
    }

    public void OnClick()
    {
        //GameEventHandler.Invoke(LogFTUEEventCode.EndPlaySingle);
    }
}
