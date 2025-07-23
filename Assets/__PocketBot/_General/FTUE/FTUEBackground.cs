using HyrphusQ.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FTUEBackground : MonoBehaviour
{
    [SerializeField] Image bg;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, HandleOnCharacterOpen);
    }

    private void HandleOnCharacterOpen()
    {
        if (!FTUEMainScene.Instance.FTUE_Equip) GetComponent<GraphicRaycaster>().enabled = true;
        else { Destroy(this.gameObject); }
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, HandleOnCharacterOpen);
    }
    private void Start()
    {
        if (!FTUEMainScene.Instance.FTUE_Equip) bg.enabled = true;
        else { Destroy(this.gameObject); }
    }


}
