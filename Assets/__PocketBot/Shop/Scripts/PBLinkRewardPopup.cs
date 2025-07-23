using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HightLightDebug;
using HyrphusQ.Events;
using HyrphusQ.GUI;
using LatteGames;
using LatteGames.PvP;
using LatteGames.PvP.TrophyRoad;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum LinkRewardPopupState
{
    Show,
    Hide
}
public class PBLinkRewardPopup : MonoBehaviour
{
    public UnityEvent OnInstantiateCellsCompleted;

    private const string AUTO_OPERATION = "Automatically";

    [SerializeField, BoxGroup("Config")] private int m_TotalWinConditionShowing;
    [SerializeField, BoxGroup("Config")] private bool m_IsShop;

    [SerializeField, BoxGroup("Ref")] private RectTransform pivot;
    [SerializeField, BoxGroup("Ref")] private Transform m_LinkRewardBoard;
    [SerializeField, BoxGroup("Ref")] private Transform m_CellHolder;
    [SerializeField, BoxGroup("Ref")] private Button m_CloseBtn;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_MainCanvasGroupVisibility;
    [SerializeField, BoxGroup("Ref")] private TextAdapter m_RemainingTimeText;
    [SerializeField, BoxGroup("Ref")] private List<PBLinkRewardCellUI> m_PBLinkRewardCellUIs;

    [SerializeField, BoxGroup("Resource")] private PBLinkRewardCellUI m_PBLinkRewardCellUIPrefab;

    [SerializeField, BoxGroup("Data")] private Variable<int> m_PBLinkRewardConditionShowPopup;
    [SerializeField, BoxGroup("Data")] private Variable<bool> m_IsRunningDarkBackGroundFTUE;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable m_PPrefBoolVariable_ShowTheFirstTimeShopTab;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable m_PPrefBoolVariable_ShowTheFirstTimeLinkRewards;
    [SerializeField, BoxGroup("Data")] private HighestAchievedPPrefFloatTracker m_HighestAchievedPPrefFloatTracker;

    private PBLinkRewardManagerSO m_PBLinkRewardManagerSO => PBLinkRewardManager.Instance.PBLinkRewardManagerSO;
    private LinkRewardPopupState m_LinkRewardPopupState = LinkRewardPopupState.Hide;
    private bool isEnoughTrophyDisplayed => m_HighestAchievedPPrefFloatTracker.value >= m_PBLinkRewardManagerSO.TrophyDisplayed;
    private bool m_CanShowPopup = false;
    private bool m_IsPlayModeGroupEnable = false;
    private bool m_IsWatchingAds = false;
    bool isWaitForShowing = false;
    Vector3 originalPivotPos;
    Transform openButton;
    Transform OpenButton
    {
        get
        {
            if (openButton == null)
            {
                openButton = FindObjectOfType<PBLinkRewardButton>().transform;
            }
            return openButton;
        }
    }

    private string m_OperationPopup;
    private void Awake()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        PBLinkRewardManager.Instance.LoadPopup += LoadData;
        GameEventHandler.AddActionEvent(LinkRewardAction.AnyCellOnRewardGranted, LoadAllCell);
        GameEventHandler.AddActionEvent(LinkRewardPopupState.Show, Show);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnNewArenaUnlocked, OnNewArenaUnlocked);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonShop, OnClickButtonShop);
        if (m_IsShop)
        {
            m_PPrefBoolVariable_ShowTheFirstTimeShopTab.onValueChanged += PPrefBoolVariable_ShowTheFirstTimeShopTab_OnValueChanged;
            LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                m_MainCanvasGroupVisibility.GetOnStartShowEvent().Subscribe(() => { layoutElement.ignoreLayout = false; });
                m_MainCanvasGroupVisibility.GetOnStartHideEvent().Subscribe(() => { layoutElement.ignoreLayout = true; });
            }
            m_MainCanvasGroupVisibility.Hide();
            return;
        }

        originalPivotPos = pivot.position;
        SelectingModeUIFlow.OnCheckFlowCompleted += OnCheckFlowCompleted;
        GameEventHandler.AddActionEvent(LinkRewardPopupState.Hide, Hide);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnTrophyRoadOpened);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnTrophyRoadClosed);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        GameEventHandler.AddActionEvent(PlayModePopup.Enable, PlayModeGroupEnable);
        GameEventHandler.AddActionEvent(PlayModePopup.Disable, PlayModeGroupDisable);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickButtonMain);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, OnClickButtonCharacter);

        m_CloseBtn.onClick.AddListener(Hide);
        m_IsRunningDarkBackGroundFTUE.onValueChanged += RunningDarkBackGroundFTUE_OnValueChanged;
    }
    private void OnDestroy()
    {
        if (PBLinkRewardManager.Instance != null)
            PBLinkRewardManager.Instance.LoadPopup -= LoadData;

        SelectingModeUIFlow.OnCheckFlowCompleted -= OnCheckFlowCompleted;
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonShop, OnClickButtonShop);
        GameEventHandler.RemoveActionEvent(LinkRewardAction.AnyCellOnRewardGranted, LoadAllCell);
        GameEventHandler.RemoveActionEvent(LinkRewardPopupState.Show, Show);
        GameEventHandler.RemoveActionEvent(LinkRewardPopupState.Hide, Hide);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnTrophyRoadOpened);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnTrophyRoadClosed);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        GameEventHandler.RemoveActionEvent(PlayModePopup.Enable, PlayModeGroupEnable);
        GameEventHandler.RemoveActionEvent(PlayModePopup.Disable, PlayModeGroupDisable);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickButtonMain);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, OnClickButtonCharacter);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnNewArenaUnlocked, OnNewArenaUnlocked);

        m_CloseBtn.onClick.RemoveListener(Hide);
        m_IsRunningDarkBackGroundFTUE.onValueChanged -= RunningDarkBackGroundFTUE_OnValueChanged;
        m_PPrefBoolVariable_ShowTheFirstTimeShopTab.onValueChanged -= PPrefBoolVariable_ShowTheFirstTimeShopTab_OnValueChanged;
    }

    void OnCheckFlowCompleted(SelectingModeUIFlow selectingModeUIFlow)
    {
        StartCoroutine(OnStart(selectingModeUIFlow));
    }

    private IEnumerator OnStart(SelectingModeUIFlow selectingModeUIFlow)
    {

        yield return new WaitUntil(() => !selectingModeUIFlow.PlayModeUI.isShowingModeUI && !selectingModeUIFlow.BossModeUI.isShowing);
        ///////////////
        //MainScene

        //Show The First Time
        if (isEnoughTrophyDisplayed && !m_PPrefBoolVariable_ShowTheFirstTimeLinkRewards.value && !m_IsShop)
        {
            m_PBLinkRewardManagerSO.ActivePack();
            m_PPrefBoolVariable_ShowTheFirstTimeLinkRewards.value = true;
            m_PBLinkRewardConditionShowPopup.value = m_TotalWinConditionShowing + 1;

            //m_PBLinkRewardManagerSO.ResetNow();
            Show(AUTO_OPERATION);
        }

        //It will show again after the player reopens the game and wins 2 matches
        if (isEnoughTrophyDisplayed && m_PBLinkRewardConditionShowPopup.value == m_TotalWinConditionShowing && m_PPrefBoolVariable_ShowTheFirstTimeLinkRewards.value)
        {
            Show(AUTO_OPERATION);
            m_PBLinkRewardConditionShowPopup.value++;
        }

        yield return new WaitUntil(() => isWaitForShowing);
        Show(AUTO_OPERATION);

        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        //In Shop
        if (isEnoughTrophyDisplayed && m_IsShop)
        {
            GenerateCell();
            m_MainCanvasGroupVisibility.Show();
            return;
        }

        if (!isEnoughTrophyDisplayed && m_IsShop)
            m_MainCanvasGroupVisibility.Hide();
    }

    private void Update()
    {
        UpdateRemainingTime();
    }

    private void LoadData()
    {
        GenerateCell();
    }

    private void GenerateCell()
    {
        if (!m_PBLinkRewardManagerSO.IsActivePack) return;
        ClearAllCell();

        List<PBLinkRewardCellSO> pbLinkRewardCellSOs = PBLinkRewardManager.Instance.Queue;
        InstantiateRewardCells(pbLinkRewardCellSOs);
        ActivateFirstInactivePack();
        SubscribeToRewardEvents();
        LoadAllCell();
    }

    private void InstantiateRewardCells(List<PBLinkRewardCellSO> pbLinkRewardCellSOs)
    {
        int index = 1;
        pbLinkRewardCellSOs.ForEach(pbLinkRewardCellSO =>
        {
            PBLinkRewardCellUI pbLinkRewardCellUI = Instantiate(m_PBLinkRewardCellUIPrefab, m_CellHolder);
            m_PBLinkRewardCellUIs.Add(pbLinkRewardCellUI);
            pbLinkRewardCellUI.SetUpPack(pbLinkRewardCellSO);
            pbLinkRewardCellUI.Index = index;
            pbLinkRewardCellUI.IsPromoted = m_PBLinkRewardManagerSO.RewardSequenceType == RewardSequenceType.Promoted;
            pbLinkRewardCellUI.IsShop = m_IsShop;
            pbLinkRewardCellUI.AdsLocation = m_IsShop ? AdsLocation.RV_LinkRewards_Shop_UI : AdsLocation.RV_LinkRewards_Main_UI;

            index++;
            if (m_IsShop)
                pbLinkRewardCellUI.GetComponent<RectTransform>().sizeDelta = new Vector2(175, 175);
        });
        OnInstantiateCellsCompleted?.Invoke();
    }

    private void ActivateFirstInactivePack()
    {
        PBLinkRewardCellUI firstInactivePack = m_PBLinkRewardCellUIs
            .Where(v => v.LinkRewardState == LinkRewardState.InProgress || v.LinkRewardState == LinkRewardState.WaitingClaim)
            .FirstOrDefault();

        if (firstInactivePack != null)
            firstInactivePack.LinkRewardPackSO.ActivePack();
    }

    private void SubscribeToRewardEvents()
    {
        m_PBLinkRewardCellUIs.ForEach(v =>
        {
            v.Load();
            v.OnStartReward += OnStartReward_Cell;
            v.OnClaimAction += OnRewardGranted_Cell;
            v.OnFailedReward += OnFailedReward_Cell;
        });
    }

    private void LoadAllCell()
    {
        m_IsWatchingAds = false;

        foreach (var cell in m_PBLinkRewardCellUIs)
        {
            if (cell.LinkRewardState == LinkRewardState.InProgress || cell.LinkRewardState == LinkRewardState.WaitingClaim)
            {
                cell.LinkRewardPackSO.ActivePack();
                break;
            }
        }

        for (int i = 0; i < m_PBLinkRewardCellUIs.Count; i++)
        {
            m_PBLinkRewardCellUIs[i].Load();
        }
    }


    private void ClearAllCell()
    {
        if (m_PBLinkRewardCellUIs != null)
        {
            for (int i = 0; i < m_PBLinkRewardCellUIs.Count; i++)
            {
                if (m_PBLinkRewardCellUIs[i] != null)
                    Destroy(m_PBLinkRewardCellUIs[i].gameObject);
            }
            m_PBLinkRewardCellUIs.Clear();
        }
    }

    private int CountClaimedRewards()
    {
        int claimedCount = 0;

        foreach (var rewardCellUI in m_PBLinkRewardCellUIs)
        {
            if (rewardCellUI.LinkRewardPackSO.IsClaimed)
            {
                claimedCount++;
            }
        }

        return claimedCount;
    }

    private bool AreAllRewardsClaimed()
    {
        int totalClaimedRewards = CountClaimedRewards();
        return totalClaimedRewards >= m_PBLinkRewardCellUIs.Count;
    }

    private void OnStartReward_Cell()
    {
        m_IsWatchingAds = true;
        PBLinkRewardManager.Instance.NoticeWatchingAdsCell(m_IsWatchingAds);
    }

    private void OnRewardGranted_Cell()
    {
        m_IsWatchingAds = false;
        GameEventHandler.Invoke(LinkRewardAction.AnyCellOnRewardGranted);
        PBLinkRewardManager.Instance.NoticeWatchingAdsCell(m_IsWatchingAds);
    }

    private void OnFailedReward_Cell()
    {
        m_IsWatchingAds = false;
        PBLinkRewardManager.Instance.NoticeWatchingAdsCell(m_IsWatchingAds);
    }

    private void UpdateRemainingTime()
    {
        if (m_IsWatchingAds) return;

        if (m_PBLinkRewardManagerSO == null) return;
        string remainingTime = m_PBLinkRewardManagerSO.GetRemainingTimeHandle(36, 36);
        m_RemainingTimeText.SetText(m_RemainingTimeText.blueprintText.Replace(Const.StringValue.PlaceholderValue, remainingTime));
    }

    private void OnClickButtonShop()
    {
        bool isEnoughTrophyDisplayed = m_HighestAchievedPPrefFloatTracker.value >= m_PBLinkRewardManagerSO.TrophyDisplayed;
        if (isEnoughTrophyDisplayed)
        {
            if(m_PBLinkRewardCellUIs.Count <= 0)
                GenerateCell();
        }
    }

    private void Show(params object[] parrams)
    {
        bool isEnoughTrophyDisplayed = m_HighestAchievedPPrefFloatTracker.value >= m_PBLinkRewardManagerSO.TrophyDisplayed;
        if (!isEnoughTrophyDisplayed) return;

        m_LinkRewardPopupState = LinkRewardPopupState.Show;
        if (m_PBLinkRewardCellUIs.Count <= 0)
            GenerateCell();

        //TODO: Hide IAP & Popup
        m_MainCanvasGroupVisibility.Hide();
        //m_MainCanvasGroupVisibility.Show();
        if (m_IsShop) return;

        m_LinkRewardBoard.transform.localScale = Vector3.zero;
        m_LinkRewardBoard.DOScale(Vector3.one, AnimationDuration.TINY);
        pivot.position = OpenButton.position;
        pivot.DOMove(originalPivotPos, AnimationDuration.TINY);

        if (parrams.Length > 0 && parrams[0] != null)
        {
            m_OperationPopup = (string)parrams[0];

            #region Design Events
            string popupName = "LinkRewards";
            string operation = m_OperationPopup;
            string status = "Start";
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
            #endregion
        }
    }

    private void Hide()
    {
        m_LinkRewardPopupState = LinkRewardPopupState.Hide;
        if (m_IsShop)
        {
            m_MainCanvasGroupVisibility.Hide();
            return;
        }

        m_LinkRewardBoard.transform.localScale = Vector3.one;
        m_LinkRewardBoard
            .DOScale(Vector3.zero, AnimationDuration.TINY)
            .OnComplete(() =>
            {
                m_MainCanvasGroupVisibility.Hide();
            });

        if (!m_IsShop)
        {
            pivot.position = originalPivotPos;
            pivot.DOMove(OpenButton.position, AnimationDuration.TINY);
        }

        #region Design Events
        string popupName = "LinkRewards";
        string operation = m_OperationPopup;
        string status = "Complete";
        GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        #endregion
    }


    private void TryShow()
    {
        if (m_IsShop) return;

        if (m_LinkRewardPopupState == LinkRewardPopupState.Show
            && m_CanShowPopup
            && !m_IsPlayModeGroupEnable)
        {
            m_MainCanvasGroupVisibility.Show();
        }
    }

    private void OnTrophyRoadOpened()
    {
        m_MainCanvasGroupVisibility.Hide();
    }

    private void OnTrophyRoadClosed()
    {
        TryShow();
    }

    private void OnUnpackStart()
    {
        m_MainCanvasGroupVisibility.Hide();
    }

    private void OnUnpackDone()
    {
        TryShow();
    }

    private void PlayModeGroupEnable()
    {
        m_IsPlayModeGroupEnable = true;
        m_MainCanvasGroupVisibility.Hide();
    }

    private void PlayModeGroupDisable()
    {
        m_IsPlayModeGroupEnable = false;
        TryShow();
    }

    private void OnClickButtonMain()
    {
        m_CanShowPopup = true;
        if (AreAllRewardsClaimed())
        {
            m_LinkRewardPopupState = LinkRewardPopupState.Hide;
            m_MainCanvasGroupVisibility.Hide();
            return;
        }

        if (m_LinkRewardPopupState == LinkRewardPopupState.Show)
            m_MainCanvasGroupVisibility.Show();
    }

    private void OnClickButtonCharacter()
    {
        m_CanShowPopup = false;
    }

    private void RunningDarkBackGroundFTUE_OnValueChanged(ValueDataChanged<bool> data)
    {
        if (data.newValue)
        {
            m_MainCanvasGroupVisibility.Hide();
        }
        else if (m_CanShowPopup && !m_IsPlayModeGroupEnable && m_LinkRewardPopupState == LinkRewardPopupState.Show)
        {
            m_MainCanvasGroupVisibility.Show();
        }
    }

    private void OnNewArenaUnlocked(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        PBPvPArenaSO pbPvPArenaSO = parameters[0] as PBPvPArenaSO;

        if (pbPvPArenaSO != null)
            StartCoroutine(DelayLoadNewArena());
    }

    private IEnumerator DelayLoadNewArena()
    {
        yield return new WaitForSeconds(3);
        GenerateCell();
    }

    private void PPrefBoolVariable_ShowTheFirstTimeShopTab_OnValueChanged(ValueDataChanged<bool> obj)
    {
        if (obj.newValue)
            isWaitForShowing = true;
    }
}