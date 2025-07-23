using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerDetailCanvasHandle : MonoBehaviour
{
    [SerializeField] private PPrefBoolVariable _fTUE_PVP;
    [SerializeField] private Button _avatarButton;

    private void OnEnable()
    {
        _avatarButton.interactable = _fTUE_PVP.value;
    }
}
