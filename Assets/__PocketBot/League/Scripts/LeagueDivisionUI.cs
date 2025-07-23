using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.Utils;
using UnityEngine;
using UnityEngine.UI;
using static LeagueDataSO;

public class LeagueDivisionUI : ComposeCanvasElementVisibilityController
{
    [SerializeField]
    private DivisionUI m_DivisionUIPrefab;
    [SerializeField]
    private RectTransform m_DivisionContainer;
    [SerializeField]
    private EZAnimSequence m_AnimSequence;
    [SerializeField]
    private Button m_CloseButton;
    [SerializeField]
    private Button m_InfoButton;
    [SerializeField]
    private CompetitionDurationUI m_CompetitionDurationUI;

    private List<EZAnimSequence.EZAnimInfo> m_PreviousAnimInfos = new List<EZAnimSequence.EZAnimInfo>();
    private List<DivisionUI> m_DivisionUIs = new List<DivisionUI>();

    private LeagueDataSO leagueDataSO => LeagueManager.leagueDataSO;

    private void Start()
    {
        m_InfoButton.onClick.AddListener(() =>
        {
            LeagueManager.ShowAndStackLeaguePopup(LeagueManager.leagueRulesUI, this);

            #region Firebase Event
            try
            {
                DateTime now = DateTime.Now;
                int dayCurrentLeague = now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)now.DayOfWeek;
                string division = GetDivisionID().ToLower();
                int position = leagueDataSO.GetLocalPlayerRank();
                int numberOfPlayers = leagueDataSO.GetCurrentDivision().playerPoolCount;
                GameEventHandler.Invoke(LogFirebaseEventCode.InfoLeagueButtonClicked, dayCurrentLeague, division, position, numberOfPlayers);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion
        });
        m_CloseButton.onClick.AddListener(() =>
        {
            LeagueManager.BackToPreviousLeaguePopup(this);
        });
        GenerateDivisionUI();
        GetOnStartShowEvent().Subscribe(OnStartShow);
    }

    private void OnDestroy()
    {
        GetOnStartShowEvent().Unsubscribe(OnStartShow);
    }

    private void OnStartShow()
    {
        UpdateView();
    }

    public void GenerateDivisionUI(bool isForceRegenerate = false)
    {
        if (m_DivisionUIs.Count > 0 && !isForceRegenerate)
            return;
        m_AnimSequence.AnimInfos.RemoveAll(animInfo => m_PreviousAnimInfos.Contains(animInfo));
        m_DivisionUIs.Clear();
        m_PreviousAnimInfos.Clear();
        int childCount = m_DivisionContainer.childCount;
        for (int i = 0; i < childCount - 1; i++)
        {
            Destroy(transform.GetChild(childCount - i - 1).gameObject);
        }
        GenerateDivisionUI(m_DivisionUIPrefab, m_DivisionContainer, divisionUI => 
        {
            var animInfo = new EZAnimSequence.EZAnimInfo() { anims = new List<EZAnimBase>() { divisionUI.visibilityAnim }, waitForSeconds = 0.1f };
            m_PreviousAnimInfos.Add(animInfo);
            m_AnimSequence.AnimInfos.Add(animInfo);
            m_DivisionUIs.Add(divisionUI);
        });
    }

    public void UpdateView(int currentDivisionIndex)
    {
        for (int i = 0; i < m_DivisionUIs.Count; i++)
        {
            m_DivisionUIs[i].UpdateStatus(GetStatus(i, currentDivisionIndex));
        }
    }

    public void UpdateView()
    {
        UpdateView(leagueDataSO.GetCurrentDivisionIndex());
        m_CompetitionDurationUI.UpdateStatus(GetCurrentTime());
    }

    public static List<DivisionUI> GenerateDivisionUI(DivisionUI divisionUIPrefab, RectTransform divisionContainer, Action<DivisionUI> action = null)
    {
        List<DivisionUI> divisionUIs = new List<DivisionUI>();
        LeagueDataSO leagueDataSO = LeagueManager.leagueDataSO;
        for (int i = 0; i < leagueDataSO.divisions.Length; i++)
        {
            DivisionUI divisionUI = Instantiate(divisionUIPrefab, divisionContainer);
            int currentDivisionIndex = leagueDataSO.GetCurrentDivisionIndex();
            int divisionIndex = i;
            divisionUI.Initialize(leagueDataSO.divisions[i], GetStatus(divisionIndex, currentDivisionIndex));
            divisionUIs.Add(divisionUI);
            action?.Invoke(divisionUI);
        }
        return divisionUIs;
    }

    string GetDivisionID()
    {
        int divisionID = leagueDataSO.GetCurrentDivisionIndex() + 1;
        return divisionID switch
        {
            1 => "Rookie",
            2 => "Contender",
            3 => "Advanced",
            4 => "Expert",
            5 => "Elite",
            _ => "Unknown",
        };
    }
}