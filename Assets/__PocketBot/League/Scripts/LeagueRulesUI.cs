using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class LeagueRulesUI : ComposeCanvasElementVisibilityController
{
    [SerializeField]
    private Button m_CloseButton;

    private void Awake()
    {
        m_CloseButton.onClick.AddListener(OnCloseButtonClicked);
    }

    private void OnDestroy()
    {
        m_CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
    }

    private void OnCloseButtonClicked()
    {
        LeagueManager.BackToPreviousLeaguePopup(this);
    }
}