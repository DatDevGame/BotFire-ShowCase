using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class SettingsButton : MonoBehaviour
{
    [SerializeField] private PPrefBoolVariable _ftue_PVP;
    [SerializeField] private Button _settingButton;
    [SerializeField] private CanvasGroupVisibility canvasGroupVisibility;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PlayModePopup.Enable, HandleOnEnablePlayModePopup);
        GameEventHandler.AddActionEvent(PlayModePopup.Disable, HandleOnDisablePlayModePopup);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnChoosenModeButton, OnChoosenModeButton);
        _settingButton.onClick.AddListener(OnClickedSettingBtn);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PlayModePopup.Enable, HandleOnEnablePlayModePopup);
        GameEventHandler.RemoveActionEvent(PlayModePopup.Disable, HandleOnDisablePlayModePopup);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnChoosenModeButton, OnChoosenModeButton);
        _settingButton.onClick.RemoveListener(OnClickedSettingBtn);
    }

    private void Start()
    {
        if (_settingButton != null)
        {
            if (_ftue_PVP != null)
            {
                _settingButton.gameObject.SetActive(_ftue_PVP);
            }
        }
    }

    void OnClickedSettingBtn()
    {
        GameEventHandler.Invoke(MainSceneEventCode.ShowSettingsUI);
    }

    private void OnChoosenModeButton() => GameEventHandler.Invoke(MainSceneEventCode.HideSettingUI);
    private void HandleOnEnablePlayModePopup() => canvasGroupVisibility.Hide();
    private void HandleOnDisablePlayModePopup() => canvasGroupVisibility.Show();
}
