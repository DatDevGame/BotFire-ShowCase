using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HyrphusQ.Events;
using LatteGames;

public class GarageButtonHandle : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private Button m_GarageButton;
    [SerializeField, BoxGroup("Ref")] private GameObject m_NewTag;
    [SerializeField, BoxGroup("Ref")] private GameObject m_Sunbunrst;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_CanvasGroupVisibility;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable m_OnClickedTheFirstTime;
    [SerializeField, BoxGroup("Data")] private GarageManagerSO m_GarageManagerSO;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable m_SingleFTUE;

    private void Awake()
    {
        m_GarageButton.onClick.AddListener(OnClickGarage);
        // SelectingModeUIFlow.OnCheckFlowCompleted += TrackSelectMode;
        // GameEventHandler.AddActionEvent(PlayModePopup.Enable, HandleOnEnablePlayModePopup);
        // GameEventHandler.AddActionEvent(PlayModePopup.Disable, HandleOnDisablePlayModePopup);

        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void OnDestroy()
    {
        m_GarageButton.onClick.RemoveListener(OnClickGarage);
        // SelectingModeUIFlow.OnCheckFlowCompleted -= TrackSelectMode;
        // GameEventHandler.RemoveActionEvent(PlayModePopup.Enable, HandleOnEnablePlayModePopup);
        // GameEventHandler.RemoveActionEvent(PlayModePopup.Disable, HandleOnDisablePlayModePopup);
    }

    private void Start()
    {
        OnLoadNewTag();
    }

    private void OnClickGarage()
    {
        if (!m_SingleFTUE.value) return;
        m_OnClickedTheFirstTime.value = true;
        OnLoadNewTag();
        GameEventHandler.Invoke(GarageEvent.Show);
    }

    private void OnLoadNewTag()
    {
        m_NewTag.SetActive(!m_OnClickedTheFirstTime.value);
        m_Sunbunrst.SetActive(!m_OnClickedTheFirstTime.value);
    }

    // private void TrackSelectMode(SelectingModeUIFlow selectingModeUIFlow)
    // {
    //     if (!m_GarageManagerSO.IsDisplayed)
    //     {
    //         m_CanvasGroupVisibility.Hide();
    //         return;
    //     }

    //     if (SelectingModeUIFlow.HasPlayAnimBoxSlot)
    //     {
    //         m_CanvasGroupVisibility.Show();
    //     }
    //     else if (!SelectingModeUIFlow.HasPlayAnimBoxSlot && !SelectingModeUIFlow.HasExitedSceneWithPlayModeUIShowing && !SelectingModeUIFlow.HasExitedSceneWithBossUIShowing && !SelectingModeUIFlow.HasExitedSceneWithBattleBetUIShowing)
    //     {
    //         m_CanvasGroupVisibility.Show();
    //     }
    //     else
    //     {
    //         m_CanvasGroupVisibility.Hide();
    //     }
    // }

    // private void HandleOnEnablePlayModePopup()
    // {
    //     if (!m_GarageManagerSO.IsDisplayed)
    //     {
    //         m_CanvasGroupVisibility.Hide();
    //         return;
    //     }

    //     m_CanvasGroupVisibility.Hide();
    // }
    // private void HandleOnDisablePlayModePopup()
    // {
    //     if (!m_GarageManagerSO.IsDisplayed)
    //     {
    //         m_CanvasGroupVisibility.Hide();
    //         return;
    //     }

    //     m_CanvasGroupVisibility.Show();
    // }
}