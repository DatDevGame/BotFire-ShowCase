using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using HyrphusQ.Events;
using UnityEngine;

public class CameraWaitPvp : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera cinemachine;

    void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnCountToFightSFX, HandleOnCountToFightSFX);
    }

    void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnCountToFightSFX, HandleOnCountToFightSFX);
    }

    void HandleOnCountToFightSFX()
    {
        cinemachine.enabled = false;
    }
}
