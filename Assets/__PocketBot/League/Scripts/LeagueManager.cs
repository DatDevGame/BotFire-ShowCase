using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using LatteGames;
using LatteGames.GameManagement;
using LatteGames.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

public class LeagueManager : Singleton<LeagueManager>
{
    [SerializeField]
    private LeagueDataSO m_LeagueDataSO;
    [SerializeField]
    private RectTransform m_TemporaryLeaguePopupContainer;

    [NonSerialized]
    private LeagueLeaderboardUI m_LeagueLeaderboardUI;
    [NonSerialized]
    private LeagueDivisionUI m_LeagueDivisionUI;
    [NonSerialized]
    private LeagueRulesUI m_LeagueRulesUI;
    [NonSerialized]
    private OpenLeagueButton m_OpenLeagueButton;
    [NonSerialized]
    private Stack<IUIVisibilityController> m_LeaguePopupUIStack = new Stack<IUIVisibilityController>();

    public static LeagueDataSO leagueDataSO => Instance?.m_LeagueDataSO;
    public static LeagueLeaderboardUI leagueLeaderboardUI
    {
        get
        {
            if (Instance.m_LeagueLeaderboardUI == null)
            {
                Instance.m_LeagueLeaderboardUI = Instance.GetComponentInChildren<LeagueLeaderboardUI>();
            }
            return Instance.m_LeagueLeaderboardUI;
        }
    }
    public static LeagueDivisionUI leagueDivisionUI
    {
        get
        {
            if (Instance.m_LeagueDivisionUI == null)
            {
                Instance.m_LeagueDivisionUI = Instance.GetComponentInChildren<LeagueDivisionUI>();
            }
            return Instance.m_LeagueDivisionUI;
        }
    }
    public static LeagueRulesUI leagueRulesUI
    {
        get
        {
            if (Instance.m_LeagueRulesUI == null)
            {
                Instance.m_LeagueRulesUI = Instance.GetComponentInChildren<LeagueRulesUI>();
            }
            return Instance.m_LeagueRulesUI;
        }
    }
    public static OpenLeagueButton openLeagueButton
    {
        get
        {
            if (Instance.m_OpenLeagueButton == null)
            {
                Instance.m_OpenLeagueButton = FindObjectOfType<OpenLeagueButton>();
            }
            return Instance.m_OpenLeagueButton;
        }
    }

    protected override void Awake()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        base.Awake();
        ResetTimeForCheater();
        if (m_LeagueDataSO.IsUnlocked() && !m_LeagueDataSO.IsLeagueOver())
        {
            m_LeagueDataSO.RefreshBotOnlineStatus(false);
            m_LeagueDataSO.UpdateAllBotCrowns(false);
        }
    }

    private void Start()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Medal).onValueChanged += OnMedalChanged;
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Crown).onValueChanged += OnCrownChanged;
        m_LeagueDataSO.playerDatabaseSO.playerNameVariable.onValueChanged += OnNameChanged;
        StartCoroutine(UpdateOnlineBotCrowns_CR());
        StartCoroutine(CheckToShowPopup_CR());
        TryUnlockLeague();
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Medal).onValueChanged -= OnMedalChanged;
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Crown).onValueChanged -= OnCrownChanged;
        m_LeagueDataSO.playerDatabaseSO.playerNameVariable.onValueChanged -= OnNameChanged;
        m_LeagueDataSO.timeLeftUntilAllBotCrownUpdate.GetReward();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            ResetTimeForCheater();
            if (m_LeagueDataSO.IsUnlocked() && !m_LeagueDataSO.IsLeagueOver())
                m_LeagueDataSO.UpdateAllBotCrowns(false);
        }
    }

    [Button]
    private void ResetTimeForCheater()
    {
        if (m_LeagueDataSO.timeLeftUntilDivisionEnds.LastRewardTime > DateTime.Now)
        {
            m_LeagueDataSO.RemainInCurrentDivision(true);
            m_LeagueDataSO.UpdateSortFlag(true);
        }
    }

    private IEnumerator UpdateOnlineBotCrowns_CR()
    {
        WaitUntil waitUntilLeagueAvailable = new WaitUntil(() => m_LeagueDataSO.IsUnlocked() && !m_LeagueDataSO.IsLeagueOver());
        WaitUntil waitUntilAbleToUpdateBotCrown = new WaitUntil(() => m_LeagueDataSO.timeLeftUntilOnlineBotCrownUpdate.canGetReward);
        yield return waitUntilLeagueAvailable;
        while (true)
        {
            if (!m_LeagueDataSO.timeLeftUntilOnlineBotCrownUpdate.canGetReward)
                yield return waitUntilAbleToUpdateBotCrown;
            m_LeagueDataSO.UpdateOnlineBotCrowns();
        }
    }

    private IEnumerator CheckToShowPopup_CR()
    {
        bool isPopupShowing = false;
        WaitUntil waitUntilLeagueUnlocked = new WaitUntil(() => m_LeagueDataSO.IsUnlocked());
        WaitUntil waitUntilNoPopupShowing = new WaitUntil(() => !isPopupShowing);
        WaitUntil waitUntilOnMainScreen = new WaitUntil(() => LeagueDataSO.IsOnMainScreen());
        yield return waitUntilLeagueUnlocked;
        while (true)
        {
            yield return Yielders.Get(1f);
            if (m_LeagueDataSO.IsAbleToStartNewLeague())
            {
                // League ends
                PopupDisplayController popupDisplayController = null;
                if (m_LeagueDataSO.IsLeagueOver())
                {
                    // Start a new league
                    popupDisplayController = BuildStartNewLeagueFlowPopup();
                }
                else
                {
                    // Show end league UI -> get rewards (if any) -> then start a new league
                    popupDisplayController = BuildEndLeagueAndStartNewLeagueFlowPopup();
                }
                yield return waitUntilOnMainScreen;
                isPopupShowing = true;
                PlayModeGroup playModeGroup = FindObjectOfType<PlayModeGroup>();
                playModeGroup.CanAutoShow = false;
                popupDisplayController.ShowPopup(OnPopupClosed);
            }
            else if (!m_LeagueDataSO.IsLeagueOver() && m_LeagueDataSO.IsDivisionOver())
            {
                bool isLocalPlayerGetPromoted = m_LeagueDataSO.IsLocalPlayerGetPromoted();
                PopupDisplayController popupDisplayController = null;
                if (m_LeagueDataSO.IsReachFinalDivision() && isLocalPlayerGetPromoted)
                {
                    // League ends
                    // Show end league UI -> get rewards
                    popupDisplayController = BuildEndLeagueFlowPopup(true);
                }
                else
                {
                    // Division ends
                    // GET PROMOTED: Show end division UI -> get rewards -> then start a new league
                    // NOT GET PROMOTED: Show end division UI
                    popupDisplayController = BuildEndDivisionFlowPopup(isLocalPlayerGetPromoted);
                }
                yield return waitUntilOnMainScreen;
                isPopupShowing = true;
                PlayModeGroup playModeGroup = FindObjectOfType<PlayModeGroup>();
                playModeGroup.CanAutoShow = false;
                popupDisplayController.ShowPopup(OnPopupClosed);
            }
            if (isPopupShowing)
            {
                yield return waitUntilNoPopupShowing;
            }
        }

        void OnPopupClosed()
        {
            isPopupShowing = false;
            DestroyTemporaryLeaguePopups();
        }
    }

    private void OnMedalChanged(ValueDataChanged<float> eventData)
    {
        if (!m_LeagueDataSO.IsUnlocked())
        {
            if (m_LeagueDataSO.IsAbleToUnlock())
            {
                UnlockLeague();
            }
            return;
        }
        if (eventData.newValue < eventData.oldValue)
            return;
        CurrencyManager.Instance.AcquireWithoutLogEvent(CurrencyType.Crown, eventData.newValue - eventData.oldValue);
    }

    private void OnCrownChanged(ValueDataChanged<float> crownChangedData)
    {
        m_LeagueDataSO.UpdateCrownAndRankChangedData(crownChangedData);
    }

    private void OnNameChanged(ValueDataChanged<string> nameChangedData)
    {
        m_LeagueDataSO.UpdateLocalPlayerData();
    }

    private void TryUnlockLeague()
    {
        if (!m_LeagueDataSO.IsUnlocked())
        {
            if (m_LeagueDataSO.IsAbleToUnlock())
            {
                UnlockLeague();
            }
        }
    }

    private void UnlockLeague()
    {
        StartCoroutine(UnlockLeague_CR());

        IEnumerator UnlockLeague_CR()
        {
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == SceneName.MainScene.ToString());
            PlayModeGroup playModeGroup = FindObjectOfType<PlayModeGroup>();
            playModeGroup.CanAutoShow = false;
            // Delay after trophy road auto show
            yield return Yielders.Get(0.11f);
            yield return new WaitUntil(() => LeagueDataSO.IsOnMainScreen());
            // Start a new league
            PopupDisplayController popupDisplayController = BuildStartNewLeagueFlowPopup(true);
            popupDisplayController.ShowPopup(OnCompleted);
            GameEventHandler.Invoke(LogFTUEEventCode.StartLeague);
        }

        void OnCompleted()
        {
            GameEventHandler.Invoke(LogFTUEEventCode.EndLeague);
            m_LeagueDataSO.Unlock();
            DestroyTemporaryLeaguePopups();
        }
    }

    private void DestroyTemporaryLeaguePopups()
    {
        m_TemporaryLeaguePopupContainer.DestroyChildren();
    }

    private PopupDisplayController BuildEndLeagueAndStartNewLeagueFlowPopup()
    {
        // Build end league flow popup
        PopupDisplayController leagueEndFlowPopupController = BuildEndLeagueFlowPopup(false);
        // Build start league flow popup
        PopupDisplayController leagueStartFlowPopupController = BuildStartNewLeagueFlowPopup();
        return PopupDisplayController.Combine(leagueEndFlowPopupController, leagueStartFlowPopupController);
    }

    private PopupDisplayController BuildStartNewLeagueFlowPopup(bool isIncludeRulesUI = false)
    {
        // Start league UI
        LeagueStartUI leagueStartUI = CreateLeagueStartUIPopup();
        // League rules UI
        LeagueRulesUI leagueRulesUI = CreateLeagueRulesUIPopup();
        // League info UI
        LeagueInfoUI leagueInfoUI = CreateLeagueInfoUIPopup();

        PopupDisplayController popupDisplayController = new PopupDisplayController();
        popupDisplayController.Enqueue(leagueStartUI, false,
            onStartShowCallback: () =>
            {
                leagueDataSO.ResetLeague();
                leagueLeaderboardUI.GenerateLeaderboardRows(true);
                leagueLeaderboardUI.UpdateView(false);
                leagueLeaderboardUI.competitionDurationUI.UpdateStatus(LeagueDataSO.GetCurrentTime());
                openLeagueButton?.UpdateView(!leagueDataSO.IsLeagueOver());
            });
        popupDisplayController.Enqueue(leagueRulesUI, false);
        popupDisplayController.Enqueue(leagueInfoUI, false);
        return popupDisplayController;

        LeagueStartUI CreateLeagueStartUIPopup()
        {
            LeagueStartUI leagueStartUI = Instantiate(leagueDataSO.configLeague.leagueStartUIPrefab, m_TemporaryLeaguePopupContainer);
            return leagueStartUI;
        }

        LeagueRulesUI CreateLeagueRulesUIPopup()
        {
            if (isIncludeRulesUI)
                return LeagueManager.leagueRulesUI;
            return null;
        }

        LeagueInfoUI CreateLeagueInfoUIPopup()
        {
            LeagueInfoUI leagueInfoUI = Instantiate(leagueDataSO.configLeague.leagueInfoUIPrefab, m_TemporaryLeaguePopupContainer);
            return leagueInfoUI;
        }
    }

    private PopupDisplayController BuildEndLeagueFlowPopup(bool isShowPromotionUI, Action onCompleted = null)
    {
        // End division UI
        LeagueEndUI leagueEndUI = CreateLeagueEndUIPopup();
        // Open rewards UI
        LeagueOpenRewardsUI leagueRewardUI = CreateLeagueOpenRewardsUIPopup();
        // Promotion UI
        LeaguePromotionUI leaguePromotionUI = CreateLeaguePromotionUIPopup();

        PopupDisplayController popupDisplayController = new PopupDisplayController();
        popupDisplayController.Enqueue(leagueEndUI, true,
            onStartShowCallback: () =>
            {
                if (isShowPromotionUI)
                {
                    leagueDataSO.PromoteToNextDivision();
                    openLeagueButton?.UpdateView(!leagueDataSO.IsLeagueOver());
                }
            });
        popupDisplayController.Enqueue(leagueRewardUI, true);
        popupDisplayController.Enqueue(leaguePromotionUI, true);
        popupDisplayController.onCompleted += OnCompleted;
        return popupDisplayController;

        LeagueEndUI CreateLeagueEndUIPopup()
        {
            m_LeagueDataSO.UpdateFinalBotCrows();
            LeagueEndUI leagueEndUI = Instantiate(leagueDataSO.configLeague.leagueEndUIPrefab, m_TemporaryLeaguePopupContainer);
            leagueEndUI.Initialize(LeagueEndUI.Status.LeagueEnd);
            return leagueEndUI;
        }

        LeagueOpenRewardsUI CreateLeagueOpenRewardsUIPopup()
        {
            LeagueOpenRewardsUI leagueRewardUI = null;
            RewardGroupInfo rewardInfo = leagueDataSO.GetCurrentDivision(true).GetRewardInfoByRank(leagueDataSO.GetLocalPlayerRank());
            if (rewardInfo != null)
            {
                leagueRewardUI = new LeagueOpenRewardsUI();
                leagueRewardUI.Initialize(rewardInfo);
            }
            return leagueRewardUI;
        }

        LeaguePromotionUI CreateLeaguePromotionUIPopup()
        {
            LeaguePromotionUI leaguePromotionUI = null;
            if (isShowPromotionUI)
            {
                int currentDivisionIndex = leagueDataSO.GetCurrentDivisionIndex();
                int nextDivisionIndex = currentDivisionIndex + 1;
                leaguePromotionUI = Instantiate(leagueDataSO.configLeague.leaguePromotionUIPrefab, m_TemporaryLeaguePopupContainer);
                leaguePromotionUI.Initialize(currentDivisionIndex, nextDivisionIndex);
            }
            return leaguePromotionUI;
        }

        void OnCompleted()
        {
            popupDisplayController.onCompleted -= OnCompleted;
            onCompleted?.Invoke();
        }
    }

    private PopupDisplayController BuildEndDivisionFlowPopup(bool isLocalPlayerGetPromoted)
    {
        // End division UI
        LeagueEndUI leagueEndUI = CreateLeagueEndUIPopup();
        // Open rewards UI
        LeagueOpenRewardsUI leagueRewardUI = CreateLeagueOpenRewardsUIPopup();
        // Promotion UI
        LeaguePromotionUI leaguePromotionUI = CreateLeaguePromotionUIPopup();
        // Back to Leaderboard UI
        LeagueLeaderboardUI leagueLeaderboardUI = LeagueManager.leagueLeaderboardUI;

        PopupDisplayController popupDisplayController = new PopupDisplayController();
        popupDisplayController.Enqueue(leagueEndUI, true,
            onStartShowCallback: () =>
            {
                if (isLocalPlayerGetPromoted)
                    leagueDataSO.PromoteToNextDivision();
                else
                    leagueDataSO.RemainInCurrentDivision();
                openLeagueButton?.UpdateView(!leagueDataSO.IsLeagueOver());
            });
        popupDisplayController.Enqueue(leagueRewardUI, true);
        popupDisplayController.Enqueue(leaguePromotionUI, true);
        popupDisplayController.Enqueue(leagueLeaderboardUI, false,
            onStartShowCallback: () =>
            {
                leagueLeaderboardUI.GenerateLeaderboardRows(true);
                leagueLeaderboardUI.UpdateView(false);
                leagueLeaderboardUI.FocusOnLocalPlayer();
                leagueLeaderboardUI.competitionDurationUI.UpdateStatus(LeagueDataSO.GetToday().AddDays(-1), true);
            },
            onEndShowCallback: () =>
            {
                leagueLeaderboardUI.competitionDurationUI.PlayPassedAnimation(0.5f);
            });
        return popupDisplayController;

        LeagueEndUI CreateLeagueEndUIPopup()
        {
            m_LeagueDataSO.UpdateFinalBotCrows();
            LeagueEndUI leagueEndUI = Instantiate(leagueDataSO.configLeague.leagueEndUIPrefab, m_TemporaryLeaguePopupContainer);
            leagueEndUI.Initialize(isLocalPlayerGetPromoted ? LeagueEndUI.Status.DivisionEndPromoted : LeagueEndUI.Status.DivisionEndNotPromoted);
            return leagueEndUI;
        }

        LeagueOpenRewardsUI CreateLeagueOpenRewardsUIPopup()
        {
            LeagueOpenRewardsUI leagueRewardUI = null;
            RewardGroupInfo rewardInfo = leagueDataSO.GetCurrentDivision().GetRewardInfoByRank(leagueDataSO.GetLocalPlayerRank());
            if (rewardInfo != null)
            {
                leagueRewardUI = new LeagueOpenRewardsUI();
                leagueRewardUI.Initialize(rewardInfo);
            }
            return leagueRewardUI;
        }

        LeaguePromotionUI CreateLeaguePromotionUIPopup()
        {
            LeaguePromotionUI leaguePromotionUI = null;
            if (isLocalPlayerGetPromoted)
            {
                int currentDivisionIndex = leagueDataSO.GetCurrentDivisionIndex();
                int nextDivisionIndex = currentDivisionIndex + 1;
                leaguePromotionUI = Instantiate(leagueDataSO.configLeague.leaguePromotionUIPrefab, m_TemporaryLeaguePopupContainer);
                leaguePromotionUI.Initialize(currentDivisionIndex, nextDivisionIndex);
            }
            return leaguePromotionUI;
        }
    }

    public static int GetRemainingPopupsInStack()
    {
        return Instance.m_LeaguePopupUIStack.Count;
    }

    public static void ShowAndStackLeaguePopup(IUIVisibilityController popupToOpen, IUIVisibilityController popupToClose)
    {
        if (!UnityEngine.Object.Equals(popupToClose, null))
        {
            popupToClose.HideImmediately();
            Instance.m_LeaguePopupUIStack.Push(popupToClose);
        }
        popupToOpen.Show();
    }

    public static void BackToPreviousLeaguePopup(IUIVisibilityController popupToClose)
    {
        popupToClose.HideImmediately();
        if (Instance.m_LeaguePopupUIStack.Count <= 0)
            return;
        IUIVisibilityController popupInStack = Instance.m_LeaguePopupUIStack.Pop();
        popupInStack.Show();
    }
}