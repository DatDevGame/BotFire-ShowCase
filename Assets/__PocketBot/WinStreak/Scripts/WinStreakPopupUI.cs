using System.Collections;
using System.Collections.Generic;
using PackReward;
using Sirenix.OdinInspector;
using UnityEngine;
using HyrphusQ.SerializedDataStructure;
using System.Linq;
using System;
using UnityEngine.UI;
using DG.Tweening;
using LatteGames;
using HyrphusQ.Events;
using TMPro;
using System.Xml.Linq;
using HyrphusQ.GUI;
using LatteGames.Monetization;
using Coffee.UIExtensions;
using Sirenix.Utilities;
using LatteGames.PvP.TrophyRoad;
using System.Security.Cryptography;
using LatteGames.Template;
using GachaSystem.Core;

public enum WinStreakPopup
{
    Show,
    Hide,
    CheckGameOver,
    OnAfterLoseItAnimation,
    OnLoadWinStreak
}
public class WinStreakPopupUI : Singleton<WinStreakPopupUI>
{
    public Sprite AvatarDefault => m_AvatarDefault;
    public bool IsWaitingShow => m_IsWaitingShow;

    public SerializedDictionary<int, WinStreakInfo> WinStreakCellUIs => m_WinStreakCellUIs;

    [SerializeField, BoxGroup("Config")] private float m_SliderDuration;
    [SerializeField, BoxGroup("Config")] private float m_ShowLoseItTimeDuration;
    [SerializeField, BoxGroup("Config")] private int m_ConditionShowRVWinStreak;
    [SerializeField, BoxGroup("Config")] private Color m_ColorOriginCurrentSlider;
    [SerializeField, BoxGroup("Config")] private Color m_ColorWarningCurrentSlider;
    [SerializeField, BoxGroup("Config")] private Color m_ColorOriginHightestSlider;
    [SerializeField, BoxGroup("Config")] private Color m_ColorWarningHighestSlider;
    [SerializeField, BoxGroup("Config")] private DOTweenAnimation m_WarningCurrentSilderAnimation;

    [SerializeField, BoxGroup("Ref")] private Sprite m_AvatarDefault;
    [SerializeField, BoxGroup("Ref")] private Slider m_SliderCurrent;
    [SerializeField, BoxGroup("Ref")] private Slider m_SliderHighest;
    [SerializeField, BoxGroup("Ref")] private WinStreakLevel m_WinStreakLevel;
    [SerializeField, BoxGroup("Ref")] private MultiImageButton m_ClaimAllButton;
    [SerializeField, BoxGroup("Ref")] private MultiImageButton m_CompleteClaimAllButton;
    [SerializeField, BoxGroup("Ref")] private MultiImageButton m_CloseButton;
    [SerializeField, BoxGroup("Ref")] private MultiImageButton m_LoseItButton;
    [SerializeField, BoxGroup("Ref")] private Image m_AvatarRank;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_DescriptionClaimAll;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_TimeUpText;
    [SerializeField, BoxGroup("Ref")] private TextAdapter m_TimeText;
    [SerializeField, BoxGroup("Ref")] private TextAdapter m_OutLineTimeText;
    [SerializeField, BoxGroup("Ref")] private GameObject m_Description;
    [SerializeField, BoxGroup("Ref")] private GameObject m_TimeRunningPanel;
    [SerializeField, BoxGroup("Ref")] private HandleSlideWinStreak m_HandleSlideWinStreak;
    [SerializeField, BoxGroup("Ref")] private RVButtonBehavior m_KeepItStreakRV;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_MainCanvasGroup;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_ButtonPanelCanvasGroup;
    [SerializeField, BoxGroup("Ref")] private SerializedDictionary<int, WinStreakInfo> m_WinStreakCellUIs;
    [SerializeField, BoxGroup("Ref")] private GameObject m_SliderCurrentRightSide;

    [SerializeField, BoxGroup("VFX")] private List<ParticleSystem> m_NewRankEffects;
    [SerializeField, BoxGroup("VFX")] private List<ParticleSystem> m_LoseWinStreakSliderVFXs;

    [SerializeField, BoxGroup("Data")] private WinStreakManagerSO m_WinStreakManagerSO;
    [SerializeField, BoxGroup("Data")] private HighestAchivedWinStreak m_HighestAchivedWinStreakPPref;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_CurrentWinStreakPPref;
    [SerializeField, BoxGroup("Data")] private ModeVariable m_CurrentChosenModeVariable;
    [SerializeField, BoxGroup("Data")] private PBTrophyRoadSO m_PBTrophyRoadSO;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_ConditionTriggerWinStreak;

    private Action m_OnCompletedSliderAnimation = delegate { };
    private bool m_IsOnInit = false;
    private bool m_IsCanLoadData = true;
    private bool m_IsShow = false;
    private bool m_IsSoundSliderTheFirstTime = false;
    private bool m_IsWaitingShow = false;
    private bool m_IsClaimAll = false;
    private string m_OperationPopup;

    public bool IsShow => m_IsShow;

    public bool IsLoseStreak
    {
        get
        {
            return PlayerPrefs.GetInt("WinStreakManager_IS_LOSE_STREAK", 0) == 1 ? true : false;
        }
        set
        {
            PlayerPrefs.SetInt("WinStreakManager_IS_LOSE_STREAK", value ? 1 : 0);
        }
    }

    public int avoidStuckCount
    {
        get
        {
            return PlayerPrefs.GetInt("WinStreakManager_avoidStuckCount", 0);
        }
        set
        {
            PlayerPrefs.SetInt("WinStreakManager_avoidStuckCount", value);
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

        if (IsLoseStreak)
        {
            IsLoseStreak = false;
            m_CurrentWinStreakPPref.value = 0;
        }
        m_CurrentWinStreakPPref.onValueChanged += WinStreakPPref_OnValueChanged;
        m_ClaimAllButton.onClick.AddListener(OnClaimAll);
        m_CompleteClaimAllButton.onClick.AddListener(OnCompleteClaimAll);
        m_CloseButton.onClick.AddListener(OnClickClose);
        m_LoseItButton.onClick.AddListener(OnClickLoseIt);
        m_SliderCurrent.onValueChanged.AddListener(delegate { OnSliderCurrentOnchange(); });
        m_KeepItStreakRV.OnRewardGranted += KeepItStreakRV_OnRewardGranted;
        GameEventHandler.AddActionEvent(WinStreakPopup.Show, OnShow);
        GameEventHandler.AddActionEvent(WinStreakPopup.Hide, OnHide);
        GameEventHandler.AddActionEvent(WinStreakPopup.CheckGameOver, CheckGameOver);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.AddActionEvent(PlayModePopup.Enable, PlayModeGroupEnable);
        GameEventHandler.AddActionEvent(StateBlockBackGroundFTUE.Start, StartStateBlockBackGroundFTUE);
        GameEventHandler.AddActionEvent(StateBlockBackGroundFTUE.End, EndStateBlockBackGroundFTUE);
        GameEventHandler.AddActionEvent(SceneManagementEventCode.OnLoadSceneStarted, OnLoadSceneStarted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnNewArenaUnlocked, OnNewArenaUnlocked);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnTrophyRoadOpened);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnTrophyRoadClosed);

        m_ButtonPanelCanvasGroup.GetOnStartShowEvent().Subscribe(() =>
        {
            m_KeepItStreakRV.gameObject.SetActive(m_HighestAchivedWinStreakPPref.value > 0);
            OnEnableWarningPopup();
            StartCoroutine(CommonCoroutine.Delay(m_ShowLoseItTimeDuration, false, () =>
            {
                m_LoseItButton.gameObject.SetActive(true);
            }));
        });

        for (int i = 0; i < m_WinStreakCellUIs.Values.Count; i++)
        {
            m_WinStreakCellUIs.ElementAt(i).Value.WinStreakCellUINomal.winStreakPopupUI = this;
            m_WinStreakCellUIs.ElementAt(i).Value.WinStreakCellUIPremium.winStreakPopupUI = this;
        }
    }

    private void OnDestroy()
    {
        m_CurrentWinStreakPPref.onValueChanged -= WinStreakPPref_OnValueChanged;
        m_ClaimAllButton.onClick.RemoveListener(OnClaimAll);
        m_CompleteClaimAllButton.onClick.RemoveListener(OnCompleteClaimAll);
        m_CloseButton.onClick.RemoveListener(OnClickClose);
        m_LoseItButton.onClick.RemoveListener(OnClickLoseIt);
        m_SliderCurrent.onValueChanged.RemoveAllListeners();
        m_KeepItStreakRV.OnRewardGranted -= KeepItStreakRV_OnRewardGranted;
        GameEventHandler.RemoveActionEvent(WinStreakPopup.Show, OnShow);
        GameEventHandler.RemoveActionEvent(WinStreakPopup.Hide, OnHide);
        GameEventHandler.RemoveActionEvent(WinStreakPopup.CheckGameOver, CheckGameOver);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.RemoveActionEvent(PlayModePopup.Enable, PlayModeGroupEnable);
        GameEventHandler.RemoveActionEvent(StateBlockBackGroundFTUE.Start, StartStateBlockBackGroundFTUE);
        GameEventHandler.RemoveActionEvent(StateBlockBackGroundFTUE.End, EndStateBlockBackGroundFTUE);
        GameEventHandler.RemoveActionEvent(SceneManagementEventCode.OnLoadSceneStarted, OnLoadSceneStarted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnNewArenaUnlocked, OnNewArenaUnlocked);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnTrophyRoadOpened);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnTrophyRoadClosed);
    }

    private void Update()
    {
        if (m_WinStreakManagerSO == null) return;

        if (IsLoseStreak)
            m_CloseButton.gameObject.SetActive(false);
        else
            m_CloseButton.gameObject.SetActive(!m_WinStreakManagerSO.IsResetReward);

        UpdateTime();

        if (!m_IsOnInit || !m_IsCanLoadData) return;

        if (m_WinStreakManagerSO.IsResetReward && !m_IsClaimAll)
        {
            m_IsClaimAll = true;
            UpdateClaimAllPanel();
            //if (m_CurrentWinStreakPPref == 0)
            //{
            //    LoadAnimation();
            //    ResetWinStreak();
            //    return;
            //}

            //if (!AnyRewardsClaimable())
            //{
            //    ResetWinStreak();
            //}
        }
    }

    private void Start()
    {
        OnLoad();
        m_HandleSlideWinStreak.Load(m_CurrentWinStreakPPref.value);
    }

    private void OnLoad()
    {
        LoadAnimation();
        OnLoadAvatar();
        UpdateClaimAllPanel();
    }

    private void OnLoadAvatar()
    {
        if (m_WinStreakLevel != null)
            m_WinStreakLevel.LoadAvatar(m_CurrentWinStreakPPref.value, isBreakLevel: IsLoseStreak);
    }

    private void KeepItStreakRV_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData obj)
    {
        #region MonetizationEventCode
        int currentStreak = m_CurrentWinStreakPPref.value;
        string location = "KeepWinStreakPopup";
        GameEventHandler.Invoke(MonetizationEventCode.KeepWinStreak, currentStreak, location);
        #endregion

        OnDisableWarningPopup();
        m_ButtonPanelCanvasGroup.Hide();
    }

    private void UpdateTime()
    {
        m_TimeUpText.gameObject.SetActive(m_WinStreakManagerSO.IsResetReward);
        m_TimeRunningPanel.SetActive(!m_WinStreakManagerSO.IsResetReward);

        if (m_WinStreakManagerSO.IsResetReward)
        {
            m_TimeText.SetText(m_TimeText.blueprintText.Replace(Const.StringValue.PlaceholderValue, "Time Up!"));
            m_OutLineTimeText.SetText(m_TimeText.blueprintText.Replace(Const.StringValue.PlaceholderValue, "Time Up!"));
        }
        else
        {
            m_TimeText.SetText(m_TimeText.blueprintText.Replace(Const.StringValue.PlaceholderValue, $"{m_WinStreakManagerSO.GetRemainingTimeHandle(31, 31)}"));
            m_OutLineTimeText.SetText(m_TimeText.blueprintText.Replace(Const.StringValue.PlaceholderValue, $"{m_WinStreakManagerSO.GetRemainingTimeHandle(31, 31)}"));
        }
    }

    [Button]
    private void OnShow(params object[] parameters)
    {
        if (m_WinStreakManagerSO.IsResetReward)
        {
            avoidStuckCount++;
            if (avoidStuckCount > 1)
            {
                m_CompleteClaimAllButton.onClick.Invoke();
                avoidStuckCount = 0;
            }
        }

        if (!m_WinStreakManagerSO.ConditionDisplayedWinStreak)
            return;

        if (m_IsShow)
            return;

        if (!m_WinStreakManagerSO.PprefTheFirstTime.value)
        {
            m_WinStreakManagerSO.PprefTheFirstTime.value = true;
            ResetWinStreak();
        }

        m_IsShow = true;
        m_ButtonPanelCanvasGroup.Hide();
        m_MainCanvasGroup.Show();
        LoadAnimation(m_SliderDuration);

        #region Design Events
        try
        {
            if (parameters[0] != null && parameters.Length > 0)
            {
                string popupName = "WinStreak";
                string status = "Start";
                string operation = (string)parameters[0];
                m_OperationPopup = operation;
                GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
            }
        }
        catch (Exception ex) { }

        #endregion
    }

    [Button]
    private void OnHide(params object[] parameters)
    {
        if (!m_WinStreakManagerSO.ConditionDisplayedWinStreak)
            return;
        if (!m_IsShow)
            return;

        m_IsWaitingShow = false;
        m_IsShow = false;
        m_MainCanvasGroup.Hide();
        LoadAnimation();

        #region Design Events
        try
        {
            if (parameters != null && parameters[0] != null && parameters.Length > 0)
            {
                string popupName = "WinStreak";
                string status = "Complete";
                string operation = (string)parameters[0];
                m_OperationPopup = operation;
                if (m_OperationPopup == "") return;
                GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
            }
        }
        catch { }
        #endregion
    }

    private void OnSliderCurrentOnchange()
    {
        int streakRankIndex = (int)m_SliderCurrent.value;
        m_HandleSlideWinStreak.Load(m_SliderCurrent.value);
        bool isInteger = m_SliderCurrent.value == Mathf.Floor(m_SliderCurrent.value);
        if (isInteger)
            OnLoadAvatar();

        if (m_WinStreakCellUIs.ContainsKey(streakRankIndex))
        {
            if (streakRankIndex == m_HighestAchivedWinStreakPPref.value && m_WinStreakCellUIs[streakRankIndex].WinStreakCellUINomal.WinStreakCurrentState == WinStreakState.Unclaimable)
            {
                m_WinStreakCellUIs[streakRankIndex].WinStreakCellUINomal.PlayReachingFX();
                m_WinStreakCellUIs[streakRankIndex].WinStreakCellUIPremium.PlayReachingFX();
            }
        }
    }

    private void OnClaimAll()
    {
        List<RewardGroupInfo> rewards = new List<RewardGroupInfo>();
        ResourceLocationProvider resourceLocationProviderCell = null;
        for (int i = 0; i < m_WinStreakCellUIs.Count; i++)
        {
            WinStreakCellUI winStreakCellUI = m_WinStreakCellUIs.ElementAt(i).Value.WinStreakCellUINomal;
            if (winStreakCellUI.WinStreakCurrentState == WinStreakState.Claimable)
            {
                winStreakCellUI.WinStreakCellSO.Claim();
                resourceLocationProviderCell = winStreakCellUI.StandSourceResourceLocationProvider;
                rewards.Add(winStreakCellUI.WinStreakCellSO.RewardGroupInfo);
            }
        }

        ((PBGachaCardGenerator)GachaCardGenerator.Instance).GenerateRewards(rewards, out List<GachaCard> gachaCards, out List<GachaPack> gachaPacks);

        if (gachaCards != null && gachaCards.Count > 0)
        {
            foreach (var card in gachaCards)
            {
                if (card is GachaCard_Currency gachaCard_Currency)
                    gachaCard_Currency.ResourceLocationProvider = resourceLocationProviderCell;
            }
        }

        if (gachaPacks != null && gachaPacks.Count > 0)
        {
            foreach (var pack in gachaPacks)
            {
                #region Firebase Event
                if (pack != null)
                {
                    string openType = "free";
                    GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, pack, openType);
                }
                #endregion

                #region Design Event
                string openStatus = "NoTimer";
                string location = "WinStreakStandard";
                GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
                #endregion
            }
        }

        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, gachaCards, gachaPacks, null);

        OnLoad();
    }
    private void OnCompleteClaimAll()
    {
        ResetWinStreak();
        OnLoad();
        m_IsClaimAll = false;
    }

    private void OnClickClose()
    {
        if (m_IsShow)
            OnHide(m_OperationPopup);
    }
    private void UpdateClaimAllPanel()
    {
        m_ButtonPanelCanvasGroup.gameObject.SetActive(!m_WinStreakManagerSO.IsResetReward);
        m_Description.gameObject.SetActive(!m_WinStreakManagerSO.IsResetReward);
        m_ClaimAllButton.gameObject.SetActive(m_WinStreakManagerSO.IsResetReward);
        m_CompleteClaimAllButton.gameObject.SetActive(false);
        m_DescriptionClaimAll.gameObject.SetActive(m_WinStreakManagerSO.IsResetReward);

        if (m_WinStreakManagerSO.IsResetReward && AnyRewardsClaimable())
        {
            m_ClaimAllButton.gameObject.SetActive(true);
            m_CompleteClaimAllButton.gameObject.SetActive(false);
        }
        else if (m_WinStreakManagerSO.IsResetReward && !AnyRewardsClaimable())
        {
            m_ClaimAllButton.gameObject.SetActive(false);
            m_CompleteClaimAllButton.gameObject.SetActive(true);
        }
    }
    private void OnClickLoseIt()
    {
        m_ButtonPanelCanvasGroup.Hide();
        //m_OnCompletedSliderAnimation += OnWinStreakLoseIt;

        StartCoroutine(DecreaseWinStreakOverTime(() =>
        {
            OnDisableWarningPopup(false);
            OnHide();
            GameEventHandler.Invoke(WinStreakPopup.OnAfterLoseItAnimation);
        }));
    }

    private IEnumerator DecreaseWinStreakOverTime(Action OnCompleteDecreseCallBack)
    {
        // while (m_CurrentWinStreakPPref.value > 0)
        // {
        //     m_CurrentWinStreakPPref.value -= 1;
        //     yield return new WaitForSeconds(m_WinStreakCellUIs.ContainsKey(m_CurrentWinStreakPPref.value) ? 1f : 0.05f);
        // }
        m_CurrentWinStreakPPref.value = 0;

        yield return new WaitForSeconds(AnimationDuration.SHORT);
        OnCompleteDecreseCallBack?.Invoke();
    }


    private void OnWinStreakLoseIt()
    {
        StartCoroutine(CommonCoroutine.Delay(1, false, () =>
        {
            m_OnCompletedSliderAnimation -= OnWinStreakLoseIt;
            StartCoroutine(DecreaseWinStreakOverTime(() =>
            {
                OnDisableWarningPopup(false);
                OnHide();
            }));
        }));
    }

    private void OnUnpackStart()
    {
        if (m_IsShow)
        {
            m_IsCanLoadData = false;
            m_MainCanvasGroup.Hide();
        }
    }

    private void OnUnpackDone()
    {
        Action OnCompletedLoadCellAction = () =>
        {
            if (m_WinStreakManagerSO.IsResetReward && !AnyRewardsClaimable())
                UpdateClaimAllPanel();
        };

        if (m_IsShow)
        {
            m_IsCanLoadData = true;
            m_MainCanvasGroup.Show();
            LoadAnimation(OnCompletedLoadCell: OnCompletedLoadCellAction);
        }
    }

    [Button]
    private void LoadAnimation(float timeDurationAnimation = 0, Action OnCompletedLoadCell = null)
    {
        if (m_CurrentWinStreakPPref.value < m_HighestAchivedWinStreakPPref.value)
        {
            m_SliderHighest.DOValue(m_HighestAchivedWinStreakPPref.value, 0);
            //Run Current Slider
            RunCurrentSliderAnimation();

            return;
        }

        //Run Highest Slider
        m_SliderHighest
        .DOValue(m_HighestAchivedWinStreakPPref.value, m_SliderHighest.value == m_HighestAchivedWinStreakPPref.value ? 0 : timeDurationAnimation)
        .OnComplete(() =>
        {
            //Run Current Slider
            RunCurrentSliderAnimation();
        });

        void RunCurrentSliderAnimation()
        {
            if (m_SliderCurrent.value != m_CurrentWinStreakPPref.value)
            {
                m_LoseWinStreakSliderVFXs.ForEach((v) =>
                {
                    v.loop = true;
                    v.Play();
                });
            }

            if (SoundManager.Instance != null && m_IsSoundSliderTheFirstTime)
            {
                if (m_CurrentWinStreakPPref.value > m_SliderCurrent.value)
                    SoundManager.Instance.PlayLoopSFX(PBSFX.UIProgressBarUp, 1);
                else if (m_CurrentWinStreakPPref.value < m_SliderCurrent.value)
                    SoundManager.Instance.PlayLoopSFX(PBSFX.UIProgressBarDown, 1);
            }
            m_IsSoundSliderTheFirstTime = true;

            m_SliderCurrent
            .DOValue(m_CurrentWinStreakPPref.value, timeDurationAnimation)
            .OnComplete(() =>
            {
                m_AvatarRank.sprite = m_AvatarDefault;
                m_LoseWinStreakSliderVFXs.ForEach((v) =>
                {
                    v.loop = false;
                    v.Stop();
                });

                m_OnCompletedSliderAnimation?.Invoke();
                OnLoadCell();
                OnCompletedLoadCell?.Invoke();
            });
        }
    }

    private void OnLoadCell()
    {
        List<WinStreakCellSO> WinStreakCellSOs = m_WinStreakManagerSO.GetQueue();
        List<WinStreakCellSO> WinStreakCellSOPremiums = m_WinStreakManagerSO.GetQueue(true);
        if (WinStreakCellSOs.Count != m_WinStreakCellUIs.Count)
        {
            Debug.LogError("WinStreakCells NOT Equal m_WinStreakCellUIs");
            return;
        }

        for (int i = 0; i < WinStreakCellSOs.Count; i++)
        {
            int keyStreakIndex = m_WinStreakCellUIs.ElementAt(i).Key;
            WinStreakCellUI winStreakCellUINomals = m_WinStreakCellUIs.ElementAt(i).Value.WinStreakCellUINomal;
            WinStreakCellUI winStreakCellUIPremiums = m_WinStreakCellUIs.ElementAt(i).Value.WinStreakCellUIPremium;
            winStreakCellUINomals.Load(WinStreakCellSOs[i], keyStreakIndex, m_HighestAchivedWinStreakPPref.value >= keyStreakIndex);
            winStreakCellUIPremiums.Load(WinStreakCellSOPremiums[i], keyStreakIndex, m_HighestAchivedWinStreakPPref.value >= keyStreakIndex);

            #region Progression Event
            if (winStreakCellUINomals.WinStreakCurrentState == WinStreakState.Claimable)
            {
                SetKeyReachMilestoneClamable(i);
            }
            #endregion
        }
        m_IsOnInit = true;
    }

    private void OnEnableWarningPopup()
    {
        SoundManager.Instance.PlayLoopSFX(PBSFX.UIBrokenBadge, 1);

        LoadAnimation();
        IsLoseStreak = true;
        m_Description.SetActive(false);
        m_CloseButton.gameObject.SetActive(false);
        m_LoseItButton.gameObject.SetActive(false);
        m_SliderCurrentRightSide.SetActive(false);
        m_SliderCurrent.fillRect.GetComponent<Image>().color = m_ColorWarningCurrentSlider;
        m_SliderHighest.fillRect.GetComponent<Image>().color = m_ColorWarningHighestSlider;
        m_WarningCurrentSilderAnimation.DOPlay();
        m_WinStreakLevel.LoadAvatarBreakStreak(true);

        #region Design Events
        string popupName = "KeepWinStreak";
        string status = "Start";
        string operation = "Automatically";
        GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        #endregion
    }

    private void OnDisableWarningPopup(bool isPlayBadgeAnimation = true)
    {
        IsLoseStreak = false;
        m_Description.SetActive(true);
        m_CloseButton.gameObject.SetActive(true);
        m_SliderCurrentRightSide.SetActive(true);
        m_SliderCurrent.fillRect.GetComponent<Image>().color = m_ColorOriginCurrentSlider;
        m_SliderHighest.fillRect.GetComponent<Image>().color = m_ColorOriginHightestSlider;
        m_WarningCurrentSilderAnimation.DOPause();
        m_WinStreakLevel.LoadAvatar(m_CurrentWinStreakPPref.value, IsLoseStreak, isPlayBadgeAnimation);

        #region Design Events
        string popupName = "KeepWinStreak";
        string status = "Complete";
        string operation = "Automatically";
        GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        #endregion
    }

    [Button]
    private void ResetWinStreak()
    {
        if (m_WinStreakManagerSO == null)
        {
            Debug.LogError("WinStreakManagerSO is Null");
            return;
        }

        PlayerPrefs.DeleteKey("WinStreakManager_IS_LOSE_STREAK");
        m_WinStreakManagerSO.ResetData();
        for (int i = 0; i < m_WinStreakManagerSO.WinStreakCellSOs.Count; i++)
        {
            m_WinStreakManagerSO.WinStreakCellSOs[i].ResetClaim();
            m_WinStreakManagerSO.WinStreakCellSOPremiums[i].ResetClaim();
        }

        #region Progression Event
        ClearKeyAllReachMilestoneClamableCell();
        #endregion
    }

    private int GetTotalClaimable()
    {
        int claimableCout = 0;
        foreach (var winStreakCell in m_WinStreakCellUIs)
        {
            if (winStreakCell.Value.WinStreakCellUINomal.WinStreakCurrentState == WinStreakState.Claimable)
            {
                claimableCout++;
            }
        }
        return claimableCout;
    }

    private bool AnyRewardsClaimable()
    {
        int claimedRewards = GetTotalClaimable();
        return claimedRewards > 0;
    }

    private bool IsCompleteStreak() => m_HighestAchivedWinStreakPPref >= m_WinStreakCellUIs.ElementAt(m_WinStreakCellUIs.Count - 1).Key;
    private bool HasAnyUnclaimedCell() => m_WinStreakCellUIs
        .Any(v => v.Value.WinStreakCellUINomal.WinStreakCurrentState == WinStreakState.Unclaimable || v.Value.WinStreakCellUINomal.WinStreakCurrentState == WinStreakState.Claimable);


    private void WinStreakPPref_OnValueChanged(ValueDataChanged<int> data)
    {
        if (m_IsShow)
            LoadAnimation(m_SliderDuration);
    }

    private void ConditionDisplayedWinStreak_OnValueChanged(ValueDataChanged<bool> data)
    {
        if (data.newValue)
            ResetWinStreak();
    }

    private void OnFinalRoundCompleted(params object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        if (!matchOfPlayer.isAbleToComplete)
            return;
        if (!m_WinStreakManagerSO.ConditionDisplayedWinStreak)
            return;

        m_IsWaitingShow = false;
        var isVictory = matchOfPlayer.isVictory;

        if (m_CurrentChosenModeVariable.value == Mode.Normal)
        {
            ResetWinStreakTheFirstTime();
            if (isVictory)
            {
                m_CloseButton.gameObject.SetActive(true);

                if (m_CurrentWinStreakPPref.value < m_WinStreakCellUIs.ElementAt(m_WinStreakCellUIs.Count - 1).Key)
                    m_CurrentWinStreakPPref.value++;

                //Waiting
                if (m_WinStreakCellUIs.ContainsKey(m_CurrentWinStreakPPref.value))
                {
                    if (m_WinStreakCellUIs[m_CurrentWinStreakPPref].WinStreakCellUINomal.WinStreakCurrentState == WinStreakState.Unclaimable ||
                        m_WinStreakCellUIs[m_CurrentWinStreakPPref].WinStreakCellUINomal.WinStreakCurrentState == WinStreakState.Claimable)
                    {
                        m_IsWaitingShow = true;
                    }
                }
            }
            else
            {
                if (HasAnyUnclaimedCell())
                {
                    m_CloseButton.gameObject.SetActive(false);
                }
                else
                {
                    m_CurrentWinStreakPPref.value = 0;
                }
            }
        }
    }

    private void CheckGameOver(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;

        if (!m_WinStreakManagerSO.ConditionDisplayedWinStreak)
            return;
        ResetWinStreakTheFirstTime();

        bool isVictory = (bool)parameters[0];
        if (isVictory)
        {
            m_ButtonPanelCanvasGroup.Hide();
            if (m_WinStreakCellUIs.ContainsKey(m_CurrentWinStreakPPref.value))
            {
                if (m_WinStreakCellUIs[m_CurrentWinStreakPPref].WinStreakCellUINomal.WinStreakCurrentState == WinStreakState.Unclaimable ||
                    m_WinStreakCellUIs[m_CurrentWinStreakPPref].WinStreakCellUINomal.WinStreakCurrentState == WinStreakState.Claimable)
                {
                    OnShow("Automatically");
                }
            }
        }
        else
        {
            if (HasAnyUnclaimedCell() && m_CurrentWinStreakPPref.value >= m_ConditionShowRVWinStreak)
            {
                m_OperationPopup = "";
                OnShow("Automatically");
                m_ButtonPanelCanvasGroup.Show();
            }

            if (m_CurrentWinStreakPPref.value < m_ConditionShowRVWinStreak)
                m_CurrentWinStreakPPref.value = 0;

        }
    }

    private void PlayModeGroupEnable()
    {
        if (m_WinStreakManagerSO.IsResetReward)
            OnShow("Automatically");
    }

    private void StartStateBlockBackGroundFTUE()
    {
        if (m_IsShow)
            m_MainCanvasGroup.Hide();
    }

    private void EndStateBlockBackGroundFTUE()
    {
        if (m_IsShow)
            m_MainCanvasGroup.Show();
    }

    private void OnLoadSceneStarted(params object[] parramter)
    {
        m_IsShow = false;
        m_MainCanvasGroup.Hide();

        if (parramter == null || parramter[0] == null || parramter[1] == null) return;
        string destinationSceneName = (string)parramter[0];
        string originSceneName = (string)parramter[1];

        if (destinationSceneName.Contains("MainScene"))
        {
            if (m_WinStreakManagerSO.ConditionDisplayedWinStreak && !m_WinStreakManagerSO.PprefTheFirstTime.value)
            {
                m_WinStreakManagerSO.PprefTheFirstTime.value = true;
                ResetWinStreak();
                OnLoad();
            }
        }

        if (originSceneName.Contains("MainScene"))
        {
            if (!m_WinStreakManagerSO.ConditionDisplayedWinStreak)
                ResetWinStreak();
        }
        GameEventHandler.Invoke(WinStreakPopup.OnLoadWinStreak);
    }

    [Button]
    private void OnNewArenaUnlocked()
    {
        StartCoroutine(CommonCoroutine.Delay(1, false, () =>
        {
            List<WinStreakCellSO> winStreakCellSOs = new List<WinStreakCellSO>();
            for (int i = 0; i < m_WinStreakCellUIs.Count; i++)
            {
                WinStreakCellSO winStreakCellSONomal = m_WinStreakCellUIs.ElementAt(i).Value.WinStreakCellUINomal.WinStreakCellSO;
                WinStreakCellSO winStreakCellSOPremium = m_WinStreakCellUIs.ElementAt(i).Value.WinStreakCellUIPremium.WinStreakCellSO;

                if (winStreakCellSONomal != null)
                    winStreakCellSOs.Add(winStreakCellSONomal);
                if (winStreakCellSOPremium != null)
                    winStreakCellSOs.Add(winStreakCellSOPremium);
            }
            m_WinStreakManagerSO.LoadRewardFollowingArena(winStreakCellSOs);
            LoadAnimation();
        }));
    }

    private void OnTrophyRoadOpened()
    {
        if (m_IsShow)
            m_MainCanvasGroup.Hide();
    }

    private void OnTrophyRoadClosed()
    {
        if (m_IsShow)
            m_MainCanvasGroup.Show();
    }

    private void ResetWinStreakTheFirstTime()
    {
        string keyTheFirstTime = "WinStreakTheFirstTime-Key";
        if (!PlayerPrefs.HasKey(keyTheFirstTime))
        {
            ResetWinStreak();
            PlayerPrefs.SetInt(keyTheFirstTime, 1);
        }
    }

    #region Progression Event
    private void SetKeyReachMilestoneClamable(int milestoneIndex)
    {
        string key = $"{milestoneIndex}-ReachMilestoneClamable";
        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetInt(key, 1);
            string status = "Start";
            int currentTrophyMilestone = m_PBTrophyRoadSO.GetCurrentMilestoneIndex();
            GameEventHandler.Invoke(ProgressionEvent.WinStreak, status, milestoneIndex, currentTrophyMilestone);
        }
    }

    private void ClearKeyAllReachMilestoneClamableCell()
    {
        for (int i = 0; i < m_WinStreakCellUIs.Count; i++)
        {
            string keyReachMilestoneClamable = $"{i}-ReachMilestoneClamable";
            PlayerPrefs.DeleteKey(keyReachMilestoneClamable);
        }
    }
    #endregion
}

[Serializable]
public class WinStreakInfo
{
    public WinStreakCellUI WinStreakCellUINomal;
    public WinStreakCellUI WinStreakCellUIPremium;
}
