using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HyrphusQ.Events;
using Sirenix.OdinInspector;

public class MoreButton : MonoBehaviour
{
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable Equip;
    [SerializeField] GameObject ftueHand;

    Button button;
    private void Awake()
    {
        button = GetComponent<Button>();
        GameEventHandler.AddActionEvent(FTUEEventCode.OnFinishShowEnergyFTUE, CheckFTUE);
        button.onClick.AddListener(HandleOnClick);
    }
    private void OnDestroy()
    {
        GameEventHandler.AddActionEvent(FTUEEventCode.OnFinishShowEnergyFTUE, CheckFTUE);
    }
    void OnEnable()
    {
        CheckFTUE();
    }

    private void OnDisable()
    {
        ftueHand.SetActive(false);
    }

    void CheckFTUE()
    {
        if (FTUEMainScene.Instance?.FTUEUpgrade.value ?? true) return;
        if (!Equip.value)
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
            ftueHand.SetActive(true);
        }
    }

    void HandleOnClick()
    {
        ftueHand.SetActive(false);
        GameEventHandler.Invoke(FTUEEventCode.OnClickMoreButton);
    }
}
