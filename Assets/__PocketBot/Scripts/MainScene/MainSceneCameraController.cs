using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using HyrphusQ.Events;
using UnityEngine;

public class MainSceneCameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera mainCamera;
    [SerializeField] CinemachineVirtualCamera shopCamera;
    [SerializeField] CinemachineVirtualCamera playModeCamera;

    void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickOnAnyButton, HandleOnAnyButtonClick);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, HandleOnCharacterButtonClick);
        GameEventHandler.AddActionEvent(GarageEvent.Show, HandleOnAnyButtonClick);
        GameEventHandler.AddActionEvent(GarageEvent.Hide, HandleOnCharacterButtonClick);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickOnAnyButton, HandleOnAnyButtonClick);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, HandleOnCharacterButtonClick);
        GameEventHandler.RemoveActionEvent(GarageEvent.Show, HandleOnAnyButtonClick);
        GameEventHandler.RemoveActionEvent(GarageEvent.Hide, HandleOnCharacterButtonClick);
    }

    void HandleOnCharacterButtonClick()
    {
        shopCamera.m_Priority = 1;
        mainCamera.m_Priority = 0;
        playModeCamera.m_Priority = -1;
    }
    void HandleOnAnyButtonClick()
    {
        shopCamera.m_Priority = 0;
        mainCamera.m_Priority = 1;
        playModeCamera.m_Priority = -1;
    }
}
