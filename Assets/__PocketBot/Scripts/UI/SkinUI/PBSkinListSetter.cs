using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class PBSkinListSetter : MonoBehaviour
{
    [SerializeField] SkinListView skinListView;

    void Awake()
    {
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnGearCardInfoPopUpShow, OnGearCardInfoPopUpShow);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearCardInfoPopUpShow, OnGearCardInfoPopUpShow);
    }

    void OnGearCardInfoPopUpShow(params object[] parameters)
    {
        PBPartSO c = parameters[0] as PBPartSO;
        skinListView.currentPartSO = c;
    }
}
