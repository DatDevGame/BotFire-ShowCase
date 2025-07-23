using System;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonPassPopup : MonoBehaviour
{
    [SerializeField] Button closeBtn;
    [SerializeField] CanvasGroupVisibility visibility;
    [SerializeField] GameObject normalContentGroup;
    [SerializeField] GameObject accessEarlyContentGroup;
    [SerializeField] GameObject activateBtn;
    [SerializeField] GameObject activatedBtn;
    [SerializeField] LocalizationParamsManager preSeasonDescriptionTxt, inSeasonDescriptionTxt;
    [SerializeField] TMP_Text seasonIndexTxt;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(SeasonPassEventCode.ShowSeasonPassPopup, Show);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.HideSeasonPassPopup, Hide);
        GameEventHandler.AddActionEvent(SeasonPassEventCode.OnPurchaseSeasonPass, OnPurchaseSeasonPass);
        closeBtn.onClick.AddListener(OnClickedCloseBtn);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.ShowSeasonPassPopup, Show);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.HideSeasonPassPopup, Hide);
        GameEventHandler.RemoveActionEvent(SeasonPassEventCode.OnPurchaseSeasonPass, OnPurchaseSeasonPass);
        closeBtn.onClick.RemoveListener(OnClickedCloseBtn);
    }

    void OnClickedCloseBtn()
    {
        GameEventHandler.Invoke(SeasonPassEventCode.HideSeasonPassPopup);
    }

    void Show(object[] parameters)
    {
        var isEarlyAccess = (bool)parameters[0];
        visibility.Show();

        var seasonIndex = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex().ToString();
        var isPurchased = SeasonPassManager.Instance.seasonPassSO.isPurchased;
        preSeasonDescriptionTxt.SetParameterValue("Index", seasonIndex);
        inSeasonDescriptionTxt.SetParameterValue("Count", SeasonPassManager.Instance.seasonPassSO.milestones.FindAll(x => x.Unlocked).Count.ToString());
        preSeasonDescriptionTxt.gameObject.SetActive(isEarlyAccess);
        inSeasonDescriptionTxt.gameObject.SetActive(!isEarlyAccess);
        seasonIndexTxt.text = seasonIndex;

        normalContentGroup.SetActive(!isEarlyAccess);
        accessEarlyContentGroup.SetActive(isEarlyAccess);

        activateBtn.SetActive(!isPurchased);
        activatedBtn.SetActive(isPurchased);

        #region Design Events
        try
        {
            string popupName = SeasonPassManager.Instance.seasonPassSO.data.state == SeasonPassState.InSeason 
                ? "InSeason"
                : "PreSeason";
            string status = DesignEventStatus.Start;
            string operation = "Manually";
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    void OnPurchaseSeasonPass()
    {
        var isPurchased = SeasonPassManager.Instance.seasonPassSO.isPurchased;
        activateBtn.SetActive(!isPurchased);
        activatedBtn.SetActive(isPurchased);
        GameEventHandler.Invoke(SeasonPassEventCode.HideSeasonPassPopup);
    }

    void Hide()
    {
        visibility.Hide();

        #region Design Events
        try
        {
            string popupName = SeasonPassManager.Instance.seasonPassSO.data.state == SeasonPassState.InSeason
                 ? "InSeason"
                 : "PreSeason";
            string status = DesignEventStatus.Complete;
            string operation = "Manually";
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    [Button]
    void ResetPurchased()
    {
        SeasonPassManager.Instance.seasonPassSO.data.isPurchasedPass = false;
    }
}
