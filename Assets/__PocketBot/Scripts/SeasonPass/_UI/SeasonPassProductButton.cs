using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using UnityEngine.UI;

public class SeasonPassProductButton : MonoBehaviour
{
    [SerializeField] bool isEarlyAccess;
    [SerializeField] GameObject activeTag;
    [SerializeField] Button button;
    [SerializeField] SeasonPassSO seasonPassSO;

    private void Awake()
    {
        if ((seasonPassSO.data.state == SeasonPassState.PreSeason || seasonPassSO.data.state == SeasonPassState.InSeason) && seasonPassSO.isPurchased)
        {
            DisableBtn();
        }
        button.onClick.AddListener(OnButtonClick);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.OnPurchaseSeasonPass, OnPurchaseSeasonPass);
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(OnButtonClick);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.OnPurchaseSeasonPass, OnPurchaseSeasonPass);
    }

    void OnButtonClick()
    {
        GameEventHandler.Invoke(SeasonPassEventCode.ShowSeasonPassPopup, isEarlyAccess);
    }

    void OnPurchaseSeasonPass()
    {
        DisableBtn();
    }

    void DisableBtn()
    {
        activeTag.SetActive(false);
    }
}
