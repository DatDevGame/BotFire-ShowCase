using System.Collections;
using System.Collections.Generic;
using LatteGames;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeagueInfoUI : ComposeCanvasElementVisibilityController
{
    [SerializeField]
    private Button m_TapToContinueButton;
    [SerializeField]
    private TextMeshProUGUI m_TapToContinueText;
    [SerializeField]
    private Animation m_TapToContinueTextAnimation;
    [SerializeField]
    private DivisionUI m_DivisionUIPrefab;
    [SerializeField]
    private RectTransform m_DivisionContainer;

    private LeagueDataSO leagueDataSO => LeagueManager.leagueDataSO;

    private void Awake()
    {
        m_TapToContinueButton.onClick.AddListener(() =>
        {
            if (m_TapToContinueText.transform.localScale.x <= 0f)
                return;
            HideImmediately();
        });
        m_TapToContinueTextAnimation.enabled = false;
        GetOnEndShowEvent().Subscribe(() =>
        {
            m_TapToContinueTextAnimation.enabled = true;
            m_TapToContinueTextAnimation.Play(PlayMode.StopAll);
        });
        List<DivisionUI> divisionUIs = LeagueDivisionUI.GenerateDivisionUI(m_DivisionUIPrefab, m_DivisionContainer);
        for (int i = 0; i < divisionUIs.Count; i++)
        {
            divisionUIs[i].Initialize(leagueDataSO.divisions[i], i == 0 ? LeagueDataSO.Status.Present : LeagueDataSO.Status.Upcoming);
            divisionUIs[i].visibilityAnim.SetToEnd();
        }
    }
}