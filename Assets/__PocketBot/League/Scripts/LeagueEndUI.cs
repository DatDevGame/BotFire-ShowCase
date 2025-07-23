using HyrphusQ.Events;
using LatteGames;
using LatteGames.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class LeagueEndUI : ComposeCanvasElementVisibilityController
{
    public enum Status
    {
        DivisionEndPromoted,
        DivisionEndNotPromoted,
        LeagueEnd
    }

    [SerializeField]
    private ScrollRect m_ScrollRect;
    [SerializeField]
    private Button m_ContinueButton;
    [SerializeField]
    private Image m_DivisionIconImage;
    [SerializeField]
    private TextMeshProUGUI m_PromotedText;
    [SerializeField]
    private TextMeshProUGUI m_NotPromotedText;
    [SerializeField]
    private TextMeshProUGUI[] m_DivisionNameTexts;
    [SerializeField]
    private RectTransform m_LeaderboardContainer;
    [SerializeField]
    private RectTransform m_PromotionZonePrefab;
    [SerializeField]
    private CompetitionDurationUI m_CompetitionDurationUI;
    [SerializeField]
    private LeagueLeaderboardRow m_LeaderboardRowPrefab;
    [SerializeField]
    private LeagueLeaderboardRow m_LocalPlayerLeaderboardRowPrefab;
    [SerializeField]
    private AnchoredElementScrollRect m_AnchoredElementScrollRect;
    [SerializeField]
    private GameObject[] m_DivisionEndUIComponents;
    [SerializeField]
    private GameObject[] m_LeagueEndUIComponents;
    [SerializeField]
    private RecycleCellUI m_RecycleCellUI;

    private Status m_LeagueEndStatus;
    private bool m_IsLocalPlayerGetPromoted;
    private int m_CurrentDivisionIndex;
    private LeagueLeaderboardRow m_LocalPlayerRow;
    private RectTransform promotionZoneInstance;

    private LeagueDataSO leagueDataSO => LeagueManager.leagueDataSO;

    private void Awake()
    {
        m_ContinueButton.onClick.AddListener(() =>
        {
            HideImmediately();

            #region Design Event
            try
            {
                LogEventLeagueRank();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion

            #region Progression Event
            #region LeaguePromotion Event
            try
            {
                int CurrentYear = DateTime.Now.Year;
                int dayOfYear = DateTime.Now.DayOfYear;

                string keyPromoted = $"LeaguePromoted-{CurrentYear}-{dayOfYear}";
                if (!PlayerPrefs.HasKey(keyPromoted))
                {
                    PlayerPrefs.SetInt(keyPromoted, 1);

                    int currentDivisionID = m_CurrentDivisionIndex + 1;
                    int nextDivisionID = currentDivisionID;
                    if (m_IsLocalPlayerGetPromoted)
                        nextDivisionID++;

                    //End
                    string status = m_IsLocalPlayerGetPromoted
                    ? ProgressionEventStatus.Complete
                    : ProgressionEventStatus.Fail;
                    LogEventLeaguePromotion(status, currentDivisionID);

                    if (m_LeagueEndStatus != Status.LeagueEnd)
                        LogEventLeaguePromotion(ProgressionEventStatus.Start, nextDivisionID);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion
            #region Progression
            if (m_LeagueEndStatus == Status.LeagueEnd)
            {
                string statusProgression = "Complete";
                string content = "League";
                GameEventHandler.Invoke(ProgressionEvent.Progression, statusProgression, content);
            }
            #endregion
            #endregion
        });
    }

    private LeagueLeaderboardRow InstantiateLeaderboardRow(bool isLocalPlayer)
    {
        return Instantiate(isLocalPlayer ? m_LocalPlayerLeaderboardRowPrefab : m_LeaderboardRowPrefab, m_LeaderboardContainer);
    }

    private void GenerateLeaderboardRows()
    {
        if (m_LeaderboardContainer.childCount > 0)
        {
            m_LeaderboardContainer.transform.DestroyChildren();
        }
        LeagueDivision currentDivision = leagueDataSO.GetCurrentDivision(true);
        List<LeaguePlayer> sortedPlayers = leagueDataSO.GetSortedPlayersByCrown();
        // LeagueLeaderboardRow localPlayerRow = null;

        var localPlayerIndex = sortedPlayers.FindIndex(x => x.isLocalPlayer);
        var promotionZoneIndex = currentDivision.playerGetPromotedCount;
        m_LocalPlayerRow = InstantiateLeaderboardRow(true);
        m_LocalPlayerRow.Initialize(sortedPlayers[localPlayerIndex], localPlayerIndex + 1);
        promotionZoneInstance = Instantiate(m_PromotionZonePrefab, m_LeaderboardContainer);
        m_AnchoredElementScrollRect.elementInsideScrollView = promotionZoneInstance;
        StartCoroutine(CommonCoroutine.Wait(null, () =>
        {
            promotionZoneInstance.GetComponent<LayoutElement>().minWidth = m_AnchoredElementScrollRect.elementAtTheTopOrLeft.rect.width;
        }));

        var insertedCells = new List<RecycleCellUI.InsertedCell>()
        {
            new RecycleCellUI.InsertedCell(){
                cell = m_LocalPlayerRow.GetComponent<RectTransform>(),
                index = localPlayerIndex
            },
            new RecycleCellUI.InsertedCell(){
                cell = promotionZoneInstance.GetComponent<RectTransform>(),
                index = promotionZoneIndex
            }
        };

        m_RecycleCellUI.OnUpdateCell += OnUpdateCell;
        m_RecycleCellUI.Init(m_LeaderboardRowPrefab.gameObject, currentDivision.playerPoolCount - 1, insertedCells);

        // m_LeaderboardRows = new List<LeagueLeaderboardRow>(m_ScrollRect.content.GetComponentsInChildren<LeagueLeaderboardRow>());
        // for (int i = 0; i < currentDivision.playerPoolCount; i++)
        // {
        //     if (i == currentDivision.playerGetPromotedCount)
        //     {
        //         var promotionZoneInstance = Instantiate(m_PromotionZonePrefab, m_LeaderboardContainer);
        //         m_AnchoredElementScrollRect.elementInsideScrollView = promotionZoneInstance;
        //         StartCoroutine(CommonCoroutine.Wait(null, () =>
        //         {
        //             promotionZoneInstance.GetComponent<LayoutElement>().minWidth = m_AnchoredElementScrollRect.elementAtTheTopOrLeft.rect.width;
        //         }));
        //     }
        //     LeagueLeaderboardRow leaderboardRow = InstantiateLeaderboardRow(sortedPlayers[i].isLocalPlayer);
        //     leaderboardRow.Initialize(sortedPlayers[i], i + 1);
        //     if (sortedPlayers[i].isLocalPlayer)
        //         localPlayerRow = leaderboardRow;
        // }
        StartCoroutine(UpdateViewAnchoredElementScrollRect_CR());

        IEnumerator UpdateViewAnchoredElementScrollRect_CR()
        {
            yield return CommonCoroutine.EndOfFrame;
            m_AnchoredElementScrollRect.UpdateView();
            m_ScrollRect.FocusOnItem(m_LocalPlayerRow.rectTransform);
            m_ScrollRect.onValueChanged.Invoke(m_ScrollRect.normalizedPosition);
        }
    }

    void OnUpdateCell(RectTransform cell, int index)
    {
        List<LeaguePlayer> sortedPlayers = leagueDataSO.GetSortedPlayersByCrown();
        var leaderboardRow = cell.GetComponent<LeagueLeaderboardRow>();
        var localPlayerIndex = sortedPlayers.FindIndex(x => x.isLocalPlayer);
        index = index >= localPlayerIndex ? index + 1 : index;
        leaderboardRow.Initialize(sortedPlayers[index], index + 1);
    }

    #region Design Event
    private void LogEventLeagueRank()
    {
        int weekID = GetWeekOfYear();
        string divisionID = GetDivisionID();
        int rankRange = GetRangeRank();
        int playedTimeToReachRank = GetPlayedTimeToReachRank(rankRange);

        GameEventHandler.Invoke(DesignEvent.LeagueRank, weekID, divisionID, rankRange, playedTimeToReachRank);
        leagueDataSO.LeagueStartDatetime.value = DateTime.Now;

        string GetDivisionID()
        {
            int add = !m_IsLocalPlayerGetPromoted ? 1 : 0;
            int divisionID = leagueDataSO.GetCurrentDivisionIndex() + add;
            return divisionID switch
            {
                1 => "Rookie",
                2 => "Contener",
                3 => "Advanced",
                4 => "Expert",
                5 => "Elite",
                _ => "Rookie",
            };
        }
        int GetRangeRank()
        {
            var sortedPlayersByCrown = leagueDataSO.GetSortedPlayersByCrown();
            LeaguePlayer playerLeague = sortedPlayersByCrown.Find(v => v.isLocalPlayer);
            int rank = sortedPlayersByCrown.IndexOf(playerLeague) + 1;

            return rank switch
            {
                >= 1 and <= 10 => rank,
                >= 11 and <= 100 => 11,
                _ => -1
            };
        }

        int GetPlayedTimeToReachRank(int rank)
        {
            if (GetRangeRank() > 11)
            {
                return 0;
            }

            // Ensure LeaguePlayedTimeToReachRank is not null and contains the required rank
            if (leagueDataSO.LeaguePlayedTimeToReachRank != null &&
                leagueDataSO.LeaguePlayedTimeToReachRank.ContainsKey(rank))
            {
                var rankData = leagueDataSO.LeaguePlayedTimeToReachRank[rank];
                if (rankData?.PlayedTimeToReachRank != null &&
                    leagueDataSO.LeagueStartDatetime != null)
                {
                    // Calculate the total seconds from the start datetime to the rank's played time
                    DateTime playedTime = rankData.PlayedTimeToReachRank.value;
                    DateTime startTime = leagueDataSO.LeagueStartDatetime.value;
                    TimeSpan timeSpan = playedTime - startTime;
                    return (int)timeSpan.TotalSeconds;
                }
            }

            return 0;
        }
        int GetWeekOfYear()
        {
            DateTime today = DateTime.Now;
            int weekOfYear = CalcWeekOfYear(today);
            return weekOfYear;
            int CalcWeekOfYear(DateTime time)
            {
                return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    time,
                    CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                    CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek
                );
            }
        }
    }
    #endregion

    #region Progression Event
    private void LogEventLeaguePromotion(string status, int divisionID)
    {
        int weeklyID = GetWeekOfYear();
        GameEventHandler.Invoke(ProgressionEvent.LeaguePromotion, status, weeklyID, divisionID);

        int GetWeekOfYear()
        {
            DateTime today = DateTime.Now;
            int weekOfYear = CalcWeekOfYear(today);
            return weekOfYear;

            int CalcWeekOfYear(DateTime time)
            {
                return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                    time,
                    CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                    CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek
                );
            }
        }
    }
    #endregion

    public void Initialize(Status status)
    {
        m_LeagueEndStatus = status;
        LeagueDivision currentDivision = leagueDataSO.GetCurrentDivision(true);
        m_CurrentDivisionIndex = leagueDataSO.GetCurrentDivisionIndex();
        m_DivisionEndUIComponents.ForEach(component => component.SetActive(status != Status.LeagueEnd));
        m_LeagueEndUIComponents.ForEach(component => component.SetActive(status == Status.LeagueEnd));
        m_PromotedText.gameObject.SetActive(status == Status.DivisionEndPromoted);
        m_NotPromotedText.gameObject.SetActive(status == Status.DivisionEndNotPromoted);
        m_DivisionIconImage.sprite = currentDivision.icon;
        m_DivisionNameTexts.ForEach(tmpText => tmpText.SetText(currentDivision.displayName));
        m_CompetitionDurationUI.UpdateStatus(LeagueDataSO.GetToday().AddDays(-1), true);
        m_IsLocalPlayerGetPromoted = leagueDataSO.IsLocalPlayerGetPromoted();
        GenerateLeaderboardRows();

        #region Firebase Event
        try
        {
            string currentDivisionID = GetDivisionID(m_CurrentDivisionIndex).ToLower();
            string nextDivisionID = GetNextDivisionID(m_CurrentDivisionIndex).ToLower();
            int position = leagueDataSO.GetLocalPlayerRank();
            int numberOfPlayers = leagueDataSO.divisions[m_CurrentDivisionIndex].playerPoolCount;
            GameEventHandler.Invoke(LogFirebaseEventCode.EndOfDivisionPopUp, currentDivisionID, nextDivisionID, position, numberOfPlayers);
            if (status == Status.LeagueEnd)
            {
                GameEventHandler.Invoke(LogFirebaseEventCode.EndOfLeaguePopUp, currentDivisionID);
            }

            string GetNextDivisionID(int divisionIndex)
            {
                return leagueDataSO.IsReachFinalDivision() ? "null" : (leagueDataSO.IsLocalPlayerGetPromoted() ? GetDivisionID(divisionIndex + 1) : GetDivisionID(divisionIndex));
            }

            string GetDivisionID(int divisionIndex)
            {
                int divisionID = divisionIndex + 1;
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
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }
}