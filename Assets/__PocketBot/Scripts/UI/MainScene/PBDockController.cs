using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public class PBDockController : DockerController
{
    [SerializeField]
    protected EventCode onSelectingTabButtonClicked;
    [SerializeField]
    protected EventCode selectManuallyButtonEventCode;
    [SerializeField]
    protected EventCode exitTabEventCode;
    [ShowInInspector]
    public ButtonType CurrentSelectedButtonType => s_CurrentSelectedButtonType;

    public override IDockerButton SelectedDockerButton
    {
        get => selectedDockerButton;
        set
        {
            if (selectedDockerButton == value || value == null)
            {
                GameEventHandler.Invoke(onSelectingTabButtonClicked);
                return;
            }
            var previousSelectedDockerButton = selectedDockerButton;
            selectedDockerButton = value;
            s_CurrentSelectedButtonType = value.ButtonType;

            #region Firebase Event
            if (s_CurrentSelectedButtonType == ButtonType.Shop)
                GameEventHandler.Invoke(LogFirebaseEventCode.ShopMenuReached);
            #endregion

            if (previousSelectedDockerButton != null)
            {
                GameEventHandler.Invoke(exitTabEventCode, previousSelectedDockerButton.ButtonType);
            }
            OnButtonClick(previousSelectedDockerButton, selectedDockerButton);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        GameEventHandler.AddActionEvent(selectManuallyButtonEventCode, OnManuallyClickButton);
    }

    protected virtual void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(selectManuallyButtonEventCode, OnManuallyClickButton);
    }

    protected virtual void OnManuallyClickButton(object[] _params)
    {
        var buttonType = (ButtonType)_params[0];
        SelectManuallyButtonOfType(buttonType);
    }
}
