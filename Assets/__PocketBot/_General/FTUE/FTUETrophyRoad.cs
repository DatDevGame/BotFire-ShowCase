using HyrphusQ.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FTUETrophyRoad : MonoBehaviour
{
    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, HandleOnTapOnMainButton);
    }

    private void OnDestroy()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, HandleOnTapOnMainButton);
    }

    private void HandleOnTapOnMainButton()
    {
        if(!FTUEMainScene.Instance.FTUE_PVP.value)
        {
            gameObject.SetActive(false);
        }
    }
}
