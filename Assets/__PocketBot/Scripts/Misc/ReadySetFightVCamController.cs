using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;

public class ReadySetFightVCamController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera cinemachine;
    [SerializeField] float delay;

    void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnSwitchFromReadyCamToFightCam, OnSwitchFromReadyCamToFightCam);
    }

    void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnSwitchFromReadyCamToFightCam, OnSwitchFromReadyCamToFightCam);
    }

    void OnSwitchFromReadyCamToFightCam()
    {
        StartCoroutine(CommonCoroutine.Delay(delay, false, () =>
        {
            cinemachine.enabled = false;
        }));
    }
}
