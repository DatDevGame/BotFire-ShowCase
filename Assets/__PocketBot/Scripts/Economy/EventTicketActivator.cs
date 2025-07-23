using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class EventTicketActivator : MonoBehaviour
{
    [SerializeField]
    private GameObject eventTicketUI;
    [SerializeField]
    private IntVariable m_RequiredNumOfMedalsToUnlockVariable;
    [SerializeField]
    private FloatVariable m_HighestAchievedMedalsVariable;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickOnAnyButton, OnClickOnAnyButton);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonShop, OnClickButtonShop);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickOnAnyButton, OnClickOnAnyButton);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonShop, OnClickButtonShop);
    }

    void OnClickOnAnyButton()
    {
        eventTicketUI.SetActive(false);
    }

    void OnClickButtonShop()
    {
        if (m_HighestAchievedMedalsVariable.value >= m_RequiredNumOfMedalsToUnlockVariable.value)
        {
            eventTicketUI.SetActive(true);
        }
    }
}
