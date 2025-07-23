using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using HyrphusQ.SerializedDataStructure;
using LatteGames;
using LatteGames.Utils;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

[DefaultExecutionOrder(-1)]
public class LeagueLeaderboardUI : ComposeCanvasElementVisibilityController
{
    [Serializable]
    public class Config
    {
        [SerializeField]
        private bool m_IsFocusOnLocalPlayerWhenOpening;
        [SerializeField]
        private int m_RankUpScrollingAnimThreshold = 4;
        [SerializeField]
        private float m_RankUpScrollDurationPerRow = 0.015f;
        [SerializeField]
        private float m_MinRankUpScrollDuration = 0.15f;
        [SerializeField]
        private float m_MaxRankUpScrollDuration = 1.5f;
        [SerializeField]
        private float m_RowFadeOutDuration = 0.25f;
        [SerializeField]
        private Vector2 m_SpecialScrollViewPos;
        [SerializeField]
        private RangeFloatValue m_RowScaleUpRange = new RangeFloatValue(0.975f, 1.15f);
        [SerializeField]
        private RangeFloatValue m_RowScaleDownRange = new RangeFloatValue(1.15f, 1f);
        [SerializeField]
        private AnimationConfig m_RowScaleUpAnimConfig;
        [SerializeField]
        private AnimationConfig m_RowScaleDownAnimConfig;
        [SerializeField]
        private LeagueLeaderboardRow m_LeaderboardRowPrefab;
        [SerializeField]
        private Sprite[] m_RankIconSprites;
        [SerializeField]
        private SerializedDictionary<CurrencyType, Sprite> m_CurrencyTypeToIconDictionary;

        public bool isFocusOnLocalPlayerWhenOpening => m_IsFocusOnLocalPlayerWhenOpening;
        public int rankUpScrollingAnimThreshold => m_RankUpScrollingAnimThreshold;
        public float rankUpScrollDurationPerRow => m_RankUpScrollDurationPerRow;
        public float minRankUpScrollDuration => m_MinRankUpScrollDuration;
        public float maxRankUpScrollDuration => m_MaxRankUpScrollDuration;
        public float rowFadeOutDuration => m_RowFadeOutDuration;
        public Vector2 specialScrollViewPos => m_SpecialScrollViewPos;
        public RangeFloatValue rowScaleUpRange => m_RowScaleUpRange;
        public RangeFloatValue rowScaleDownRange => m_RowScaleDownRange;
        public AnimationConfig rowScaleUpAnimConfig => m_RowScaleUpAnimConfig;
        public AnimationConfig rowScaleDownAnimConfig => m_RowScaleDownAnimConfig;
        public LeagueLeaderboardRow leaderboardRowPrefab => m_LeaderboardRowPrefab;

        public int CalcSiblingIndexByRank(int rank)
        {
            return LeagueManager.leagueDataSO.GetCurrentDivision(true).IsGetPromotedByRank(rank) ? rank - 1 : rank;
        }

        public Sprite GetIconByCurrencyType(CurrencyType currencyType)
        {
            return m_CurrencyTypeToIconDictionary.Get(currencyType);
        }

        public Sprite GetIconByRank(int rank)
        {
            if (m_RankIconSprites.IsValidIndex(rank - 1))
                return m_RankIconSprites[rank - 1];
            return null;
        }
    }

    private const string k_RankChangedDataKey = "Leaderboard";

    [SerializeField]
    private LeagueDataSO m_LeagueDataSO;
    [SerializeField]
    private ScrollRect m_ScrollRect;
    [SerializeField]
    private Button m_CloseButton;
    [SerializeField]
    private Button m_InfoButton;
    [SerializeField]
    private Button m_OpenDivisionButton;
    [SerializeField]
    private Image m_DivisionIconImage;
    [SerializeField]
    private TextMeshProUGUI m_TimeLeftText;
    [SerializeField]
    private TextMeshProUGUI m_DivisionNameText;
    [SerializeField]
    private RectTransform m_PromotionZonePrefab;
    [SerializeField]
    private RectTransform m_LeaderboardContainer;
    [SerializeField]
    private LeagueLeaderboardRow m_LeaderboardRowPrefab;
    [SerializeField]
    private LeagueLeaderboardRow m_LocalPlayerLeaderboardRowPrefab;
    [SerializeField]
    private AnchoredElementScrollRect m_AnchoredElementScrollRect;
    [SerializeField]
    private GameObject m_ContentRoot;
    [SerializeField]
    private RecycleCellUI m_RecycleCellUI;


    private CompetitionDurationUI m_CompetitionDurationUI;
    private LeagueLeaderboardRow m_LocalPlayerRow;
    private RectTransform promotionZoneInstance;
    private List<LeagueLeaderboardRow> m_LeaderboardRows = new List<LeagueLeaderboardRow>();
    private EZAnimSequence m_AnimSequence;

    #region Design Event
    private bool m_IsShowPopupLogEvent;
    private string m_Operation;
    #endregion

    private bool isShowing { get; set; }
    private ValueDataChanged<int> rankChangedData
    {
        get => leagueDataSO.GetRankChangedData(k_RankChangedDataKey);
        set => leagueDataSO.SetRankChangedData(k_RankChangedDataKey, value);
    }
    private EZAnimSequence animSequence
    {
        get
        {
            if (m_AnimSequence == null)
            {
                m_AnimSequence = GetComponent<EZAnimSequence>();
            }
            return m_AnimSequence;
        }
    }

    public bool isAutoUpdateView { get; set; }
    public Config config => m_LeagueDataSO.configLeaderboardUI;
    public LeagueDataSO leagueDataSO => m_LeagueDataSO;
    public CompetitionDurationUI competitionDurationUI
    {
        get
        {
            if (m_CompetitionDurationUI == null)
                m_CompetitionDurationUI = GetComponentInChildren<CompetitionDurationUI>();
            return m_CompetitionDurationUI;
        }
    }

    private IEnumerator Start()
    {
        if (!m_LeagueDataSO.IsUnlocked())
        {
            m_ContentRoot.SetActive(false);
            yield return new WaitUntil(() => m_LeagueDataSO.IsUnlocked());
            m_ContentRoot.SetActive(true);
        }
        m_LeagueDataSO.AddCrownAndRankChangedDataKey(k_RankChangedDataKey);
        m_InfoButton.onClick.AddListener(() =>
        {
            LeagueManager.ShowAndStackLeaguePopup(LeagueManager.leagueRulesUI, this);

            #region Firebase Event
            try
            {
                DateTime now = DateTime.Now;
                int dayCurrentLeague = now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)now.DayOfWeek;
                string division = GetDivisionID().ToLower();
                int position = m_LeagueDataSO.GetLocalPlayerRank();
                int numberOfPlayers = m_LeaderboardRows.Count();
                GameEventHandler.Invoke(LogFirebaseEventCode.InfoLeagueButtonClicked, dayCurrentLeague, division, position, numberOfPlayers);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion
        });
        m_OpenDivisionButton.onClick.AddListener(() =>
        {
            LeagueManager.ShowAndStackLeaguePopup(LeagueManager.leagueDivisionUI, this);
        });
        m_CloseButton.onClick.AddListener(() =>
        {
            LeagueManager.BackToPreviousLeaguePopup(this);
        });
        GetOnStartShowEvent().Subscribe(OnStartShow);
        GetOnEndShowEvent().Subscribe(OnEndShow);
        GetOnStartHideEvent().Subscribe(OnStartHide);
        GetOnEndHideEvent().Subscribe(OnEndHide);
        UpdateView(false);
        GenerateLeaderboardRows();
        m_ScrollRect.enabled = false;
        // Wait after layout group rebuild the child elements
        yield return Yielders.EndOfFrame;
        m_ContentRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        GetOnStartShowEvent().Unsubscribe(OnStartShow);
        GetOnEndShowEvent().Unsubscribe(OnEndShow);
        GetOnStartHideEvent().Unsubscribe(OnStartHide);
        GetOnEndHideEvent().Unsubscribe(OnEndHide);
    }

    private void OnStartShow()
    {
        #region Design Events
        if (!m_IsShowPopupLogEvent)
        {
            m_IsShowPopupLogEvent = true;
            try
            {
                m_Operation = isAutoUpdateView ? "Manually" : "Automatically";
                string popupName = "League";
                string status = DesignEventStatus.Start;
                string operation = m_Operation;
                GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
        }
        #endregion

        #region Firebase Event
        if (isAutoUpdateView)
        {
            try
            {
                DateTime now = DateTime.Now;
                int dayCurrentLeague = now.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)now.DayOfWeek;
                string division = GetDivisionID().ToLower();
                int position = m_LeagueDataSO.GetLocalPlayerRank();
                int numberOfPlayers = m_LeaderboardRows.Count();
                GameEventHandler.Invoke(LogFirebaseEventCode.LeagueMenuReached, dayCurrentLeague, division, position, numberOfPlayers);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
        }
        #endregion

        if (!m_ContentRoot.activeSelf)
            m_ContentRoot.SetActive(true);
        if (!isAutoUpdateView)
            return;

        isAutoUpdateView = false;
        if (rankChangedData.oldValue == 0 || rankChangedData.newValue >= rankChangedData.oldValue)
        {
            UpdateView(true);
            return;
        }
        PlayPlayerRankUpAnimation(rankChangedData.oldValue, rankChangedData.newValue);
        rankChangedData = default;
    }

    private void OnEndShow()
    {
        isShowing = true;
        m_ScrollRect.enabled = true;
    }

    private void OnStartHide()
    {
        m_ScrollRect.enabled = false;

        #region Design Events
        if (m_IsShowPopupLogEvent)
        {
            try
            {
                m_IsShowPopupLogEvent = false;
                string popupName = "League";
                string status = DesignEventStatus.Complete;
                string operation = m_Operation;
                GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
        }
        #endregion
    }

    private void OnEndHide()
    {
        if (m_ContentRoot.activeSelf)
            m_ContentRoot.SetActive(false);
        isShowing = false;
    }

    private LeagueLeaderboardRow InstantiateLeaderboardRow(bool isLocalPlayer)
    {
        return Instantiate(isLocalPlayer ? m_LocalPlayerLeaderboardRowPrefab : m_LeaderboardRowPrefab, m_LeaderboardContainer);
    }

    private float CalcDurationFromDistance(Vector3 pointA, Vector3 pointB, float segmentLength, float durationPerSegment, float minDuration = 0f, float maxDuration = float.MaxValue)
    {
        float distance = Vector3.Distance(pointA, pointB);
        float duration = distance / segmentLength * durationPerSegment;
        return Mathf.Min(minDuration + duration, maxDuration);
    }

    private IEnumerator PlayPlayerRankUpAnimation_CR(int oldRank, int newRank)
    {
        LeagueLeaderboardRow previousLocalPlayerRow = Instantiate(m_LocalPlayerRow, m_LeaderboardContainer.parent);
        previousLocalPlayerRow.player = m_LocalPlayerRow.player;
        UpdateView(true);
        yield return CommonCoroutine.EndOfFrame;
        LeagueLeaderboardRow currentLocalPlayerRow = m_LocalPlayerRow;
        Vector2 leaderboardRowSizeDelta = currentLocalPlayerRow.rectTransform.sizeDelta;
        currentLocalPlayerRow.gameObject.SetActive(false);
        previousLocalPlayerRow.transform.SetParent(m_LeaderboardContainer);
        previousLocalPlayerRow.transform.SetAsLastSibling();
        previousLocalPlayerRow.GetComponent<LayoutElement>().ignoreLayout = true;
        previousLocalPlayerRow.transform.localScale = Vector3.one;
        yield return new WaitUntil(() => isShowing);
        previousLocalPlayerRow.ScaleUp(leaderboardRowSizeDelta);
        yield return CommonCoroutine.EndOfFrame;
        bool isSpecialCase = oldRank == m_LeaderboardRows.Count && m_LeagueDataSO.GetCurrentDivision().IsGetPromotedByRank(newRank);
        Vector2 oldPlayerCellPos = m_RecycleCellUI.GetCellPos(oldRank - 1);
        previousLocalPlayerRow.transform.localPosition = oldPlayerCellPos;
        oldPlayerCellPos.y = m_ScrollRect.content.rect.size.y - Mathf.Abs(oldPlayerCellPos.y);
        Vector2 currentScrollRectPos = m_ScrollRect.CalculateFocusedScrollPosition(oldPlayerCellPos, false);
        Vector2 targetScrollRectPos = m_ScrollRect.CalculateFocusedScrollPosition(m_LocalPlayerRow.rectTransform, false);
        Vector2 middleScrollRectPos = (currentScrollRectPos + targetScrollRectPos) / 2f;
        middleScrollRectPos.Set(Mathf.Clamp01(middleScrollRectPos.x), Mathf.Clamp01(middleScrollRectPos.y));
        currentScrollRectPos.Set(Mathf.Clamp01(currentScrollRectPos.x), Mathf.Clamp01(currentScrollRectPos.y));
        targetScrollRectPos.Set(Mathf.Clamp01(targetScrollRectPos.x), Mathf.Clamp01(targetScrollRectPos.y));
        if (isSpecialCase)
        {
            currentScrollRectPos = config.specialScrollViewPos;
            m_ScrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        }
        m_ScrollRect.enabled = false;
        m_ScrollRect.normalizedPosition = (oldRank - newRank <= config.rankUpScrollingAnimThreshold) ? middleScrollRectPos : currentScrollRectPos;
        m_ScrollRect.onValueChanged.Invoke(m_ScrollRect.normalizedPosition);
        yield return new WaitForSeconds(config.rowScaleUpAnimConfig.duration);
        currentLocalPlayerRow.gameObject.SetActive(true);
        currentLocalPlayerRow.visibility = false;
        previousLocalPlayerRow.transform.SetParent(m_LeaderboardContainer.parent);
        yield return new WaitForSeconds(0.2f);
        if (oldRank - newRank > config.rankUpScrollingAnimThreshold)
        {
            float scrollDuration = CalcDurationFromDistance(currentScrollRectPos, targetScrollRectPos, currentLocalPlayerRow.rectTransform.rect.size.y / m_ScrollRect.content.rect.size.y, config.rankUpScrollDurationPerRow, config.minRankUpScrollDuration, config.maxRankUpScrollDuration);
            yield return CommonCoroutine.LerpFactor(scrollDuration, t =>
            {
                m_ScrollRect.normalizedPosition = Vector2.Lerp(currentScrollRectPos, targetScrollRectPos, t);
                m_ScrollRect.onValueChanged.Invoke(m_ScrollRect.normalizedPosition);
            });
            yield return new WaitForSeconds(AnimationDuration.TINY);
        }
        Vector3 previousRowPos = previousLocalPlayerRow.rectTransform.position;
        Vector3 targetRowPos = currentLocalPlayerRow.rectTransform.CalcWorldPositionAtPivot(previousLocalPlayerRow.rectTransform.pivot);
        float duration = CalcDurationFromDistance(previousRowPos, targetRowPos, currentLocalPlayerRow.rectTransform.rect.size.y, 0.05f);
        previousLocalPlayerRow.rectTransform.DOMove(targetRowPos, duration);
        yield return new WaitForSeconds(duration);
        previousLocalPlayerRow.ScaleDown(currentLocalPlayerRow.rectTransform.sizeDelta);
        yield return new WaitForSeconds(config.rowScaleDownAnimConfig.duration);
        currentLocalPlayerRow.visibility = true;
        previousLocalPlayerRow.FadeOutAndDestroy(config.rowFadeOutDuration);
        if (isSpecialCase)
            m_ScrollRect.movementType = ScrollRect.MovementType.Elastic;
        m_ScrollRect.enabled = true;
    }

    [Button]
    public void GenerateLeaderboardRows(bool isForceRegenerate = false)
    {
        if (!m_LeagueDataSO.IsUnlocked() || m_LeagueDataSO.IsLeagueOver())
            return;
        if (m_LeaderboardRows.Count > 0 && !isForceRegenerate)
            return;
        // Destroy previous rows
        if (m_LeaderboardRows.Count > 0)
        {
            m_LeaderboardContainer.transform.DestroyChildren();
            m_LeaderboardContainer.transform.DetachChildren();
            m_LeaderboardRows.Clear();
        }
        // Reset data
        rankChangedData = default;
        // Generate new rows
        LeagueDivision currentDivision = m_LeagueDataSO.GetCurrentDivision();
        List<LeaguePlayer> sortedPlayers = m_LeagueDataSO.GetSortedPlayersByCrown();

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

        m_LeaderboardRows = new List<LeagueLeaderboardRow>(m_ScrollRect.content.GetComponentsInChildren<LeagueLeaderboardRow>());

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
        //     m_LeaderboardRows.Add(leaderboardRow);
        //     if (sortedPlayers[i].isLocalPlayer)
        //         m_LocalPlayerRow = leaderboardRow;
        // }
    }

    void OnUpdateCell(RectTransform cell, int index)
    {
        List<LeaguePlayer> sortedPlayers = m_LeagueDataSO.GetSortedPlayersByCrown();
        var leaderboardRow = cell.GetComponent<LeagueLeaderboardRow>();
        var localPlayerIndex = sortedPlayers.FindIndex(x => x.isLocalPlayer);
        index = index >= localPlayerIndex ? index + 1 : index;
        leaderboardRow.Initialize(sortedPlayers[index], index + 1);
    }

    public void UpdateView(bool isForceUpdateEntries)
    {
        if (m_LeagueDataSO.IsLeagueOver())
            return;
        LeagueDivision currentDivision = m_LeagueDataSO.GetCurrentDivision();
        m_DivisionIconImage.sprite = currentDivision.icon;
        m_DivisionNameText.SetText(currentDivision.displayName);
        m_TimeLeftText.SetText(m_LeagueDataSO.timeLeftUntilDivisionEnds.GetRemainingTimeInShort());
        if (isForceUpdateEntries)
        {
            List<LeaguePlayer> sortedPlayers = m_LeagueDataSO.GetSortedPlayersByCrown();

            DestroyImmediate(m_LocalPlayerRow.gameObject);
            DestroyImmediate(promotionZoneInstance.gameObject);
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
            m_RecycleCellUI.Init(m_LeaderboardRowPrefab.gameObject, currentDivision.playerPoolCount - 1, insertedCells);

            m_LeaderboardRows = new List<LeagueLeaderboardRow>(m_ScrollRect.content.GetComponentsInChildren<LeagueLeaderboardRow>());

            // List<LeaguePlayer> sortedPlayers = m_LeagueDataSO.GetSortedPlayersByCrown();
            // for (int i = 0; i < sortedPlayers.Count; i++)
            // {
            //     if (sortedPlayers[i].isLocalPlayer != m_LeaderboardRows[i].player.isLocalPlayer)
            //     {
            //         bool isLocalPlayer = m_LeaderboardRows[i].player.isLocalPlayer;
            //         DestroyImmediate(m_LeaderboardRows[i].gameObject);
            //         m_LeaderboardRows[i] = InstantiateLeaderboardRow(!isLocalPlayer);
            //     }
            //     m_LeaderboardRows[i].Initialize(sortedPlayers[i], i + 1);
            //     if (sortedPlayers[i].isLocalPlayer)
            //         m_LocalPlayerRow = m_LeaderboardRows[i];
            // }
        }
    }

    public void PlayPlayerRankUpAnimation(int oldRank, int newRank)
    {
        LGDebug.Log($"OldRank: {oldRank} - NewRank: {newRank}");
        if (oldRank == newRank)
            return;
        StartCoroutine(PlayPlayerRankUpAnimation_CR(oldRank, newRank));
    }

    public void FocusOnLocalPlayer()
    {
        animSequence.SetToEnd();
        m_ScrollRect.FocusOnItem(m_LocalPlayerRow.rectTransform);
        m_ScrollRect.onValueChanged.Invoke(m_ScrollRect.normalizedPosition);
        animSequence.SetToStart();
    }

    public override void Show()
    {
        if (config.isFocusOnLocalPlayerWhenOpening)
        {
            FocusOnLocalPlayer();
        }
        base.Show();
    }

    public override void ShowImmediately()
    {
        if (config.isFocusOnLocalPlayerWhenOpening)
        {
            FocusOnLocalPlayer();
        }
        base.ShowImmediately();
    }

    string GetDivisionID()
    {
        int divisionID = m_LeagueDataSO.GetCurrentDivisionIndex() + 1;
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