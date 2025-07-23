using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using LatteGames;
using LatteGames.GameManagement;
using LatteGames.PvP;
using LatteGames.PvP.TrophyRoad;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[EventCode]
public enum PlayModePopup
{
    Enable,
    Disable
}

public class PlayModeGroup : MonoBehaviour
{
    private static bool HasGotoPvPSceneFromPlayModeUI;

    [SerializeField, BoxGroup("Sound")] private SoundID soundBattleClick;
    [SerializeField, BoxGroup("Sound")] private SoundID soundShowBossMapClick;

    [SerializeField, BoxGroup("Ref")] private Image imageMode;
    [SerializeField, BoxGroup("Ref")] private MultiImageButton closeBtn;
    [SerializeField, BoxGroup("Ref")] private GameObject playModePanel;
    [SerializeField, BoxGroup("Ref")] private GameObject chooseModeObject;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility visibility;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility selectModeUI;
    [SerializeField, BoxGroup("Ref")] private MultiImageButton battleBtn;
    [SerializeField, BoxGroup("Ref")] private WinStreakBanner m_WinStreakBanner;

    [SerializeField, BoxGroup("Prefabs")] private GameObject noticePrefab;

    [SerializeField, BoxGroup("Button Mode")] private SerializedDictionary<Mode, MultiImageButton> listModesBtn;
    [SerializeField, BoxGroup("Button Mode")] private GameObject singleModeObject;
    [SerializeField, BoxGroup("Button Mode")] private GameObject battleModeObject;
    [SerializeField, BoxGroup("Button Mode")] private EZAnimBase panelAnimBase;

    [SerializeField, BoxGroup("Data")] private ModeVariable currentChosenModeVariable;
    [SerializeField, BoxGroup("Data")] private FloatVariable m_HighestAchievedMedalsVariable;
    [SerializeField, BoxGroup("Data")] private IntVariable requiredNumOfMedalsToUnlockSingleMode;
    [SerializeField, BoxGroup("Data")] private IntVariable requiredNumOfMedalsToUnlockBossMode;
    [SerializeField, BoxGroup("Data")] private IntVariable requiredNumOfMedalsToUnlockBattleMode;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable isActiveFullSlotBox;
    [SerializeField, BoxGroup("Data")] private BossFightStreakHandle bossFightStreakHandle;
    [SerializeField, BoxGroup("Data")] private CurrentHighestArenaVariable highestArenaVariable;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_CumulatedWinMatch;

    [SerializeField, BoxGroup("FTUE")] private GameObject handFTUEPlay;
    [SerializeField, BoxGroup("FTUE")] private GameObject handchoosenModeFTUE;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable pprefBoolFTUE_Upgrade;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable pprefBoolFTUE_PVP;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable pPrefBoolFTUEBattleBoss;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable pPrefBoolBattleBossFightingUI;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable pPrefBoolBattleRoyal;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable FTUE_SeasonPassTab;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable FTUE_PreludeDoMissionTab;

    [SerializeField, BoxGroup("PS-FTUE")] private PSFTUESO m_PSFTUESO;

    private IEnumerator OnUpdateStartFTUECR;

    private bool hasIgnoredFirstClickButtonMainEvent = false;
    private bool isCallOneTimeSingleFTUE = false;
    private bool isConditionPreludeDoMissionFTUE => !FTUE_PreludeDoMissionTab.value && FTUE_SeasonPassTab && SeasonPassManager.Instance.seasonPassSO.data.isNewUser;
    private bool isConditionSingleFTUE => (!pprefBoolFTUE_PVP.value && pprefBoolFTUE_Upgrade) || isConditionPreludeDoMissionFTUE;
    private bool isConditionBossFTUE => !pPrefBoolBattleBossFightingUI.value && pprefBoolFTUE_PVP.value && m_HighestAchievedMedalsVariable.value >= requiredNumOfMedalsToUnlockBossMode;
    private bool isConditionBattleRoyalFTUE => !pPrefBoolBattleRoyal.value && pPrefBoolBattleBossFightingUI.value && m_HighestAchievedMedalsVariable.value >= requiredNumOfMedalsToUnlockBattleMode;
    public bool isShowingModeUI = false;
    private bool canAutoShow = true;

    private float m_ScaleDown = 0.95f;
    private float m_TimeScaleDown = 0.05f;
    private float m_ScaleUp = 1;
    private float m_TimeScaleUp = 0.1f;

    public bool CanAutoShow
    {
        get
        {
            return canAutoShow;
        }
        set
        {
            canAutoShow = value;
            if (!canAutoShow && isShowingModeUI)
                HideModeUI();
        }
    }

    private void Awake()
    {
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickButtonMain);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, OnClickButtonCharacter);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickOnSelectingButton, OnClickOnSelectingButton);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnOpenTrophyRoad);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnOutTrophyRoad);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);

        closeBtn.onClick.AddListener(HideModeUI);
        battleBtn.onClick.AddListener(OnClickPlayBtn);
        listModesBtn[Mode.Normal].onClick.AddListener(ChooseSingleMode);
        listModesBtn[Mode.Boss].onClick.AddListener(ChooseBossMode);
        listModesBtn[Mode.Battle].onClick.AddListener(ChooseBattleMode);
        m_HighestAchievedMedalsVariable.onValueChanged += HighestAchievedMedalsVariable_onValueChanged;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickButtonMain);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, OnClickButtonCharacter);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickOnSelectingButton, OnClickOnSelectingButton);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnOpenTrophyRoad);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnOutTrophyRoad);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);

        closeBtn.onClick.RemoveListener(HideModeUI);
        battleBtn.onClick.RemoveListener(OnClickPlayBtn);
        listModesBtn[Mode.Normal].onClick.RemoveListener(ChooseSingleMode);
        listModesBtn[Mode.Boss].onClick.RemoveListener(ChooseBossMode);
        listModesBtn[Mode.Battle].onClick.RemoveListener(ChooseBattleMode);
        m_HighestAchievedMedalsVariable.onValueChanged -= HighestAchievedMedalsVariable_onValueChanged;
    }


    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Tab))
            ShowModeUI();
#endif
    }

    private void OnClickPlayBtn()
    {
        if (!m_PSFTUESO.FTUEStartFirstMatch.value)
        {
            GameEventHandler.Invoke(PSLogFTUEEventCode.EndFirstMatch);
        }

        if (m_PSFTUESO.FTUEOpenInfoPopup.value && !m_PSFTUESO.FTUEStart2ndMatch.value)
        {
            GameEventHandler.Invoke(PSLogFTUEEventCode.End2ndMatch);
        }


        pprefBoolFTUE_PVP.value = true;
        currentChosenModeVariable.GameMode = Mode.Battle;
        GameEventHandler.Invoke(BattleBetEventCode.OnEnterBattle, highestArenaVariable.value);
        //battleBtn.GetComponent<EZAnimBase>().Play();
        //ShowModeUI();

        #region Firebase Event
        try
        {
            PBOpenBossMapButton pbOpenBossMapButton = listModesBtn[Mode.Boss].GetComponent<PBOpenBossMapButton>();
            PvPPlayBattleModeButton pvPPlayBattleModeButton = listModesBtn[Mode.Battle].GetComponent<PvPPlayBattleModeButton>();

            int duelMatchStatus = m_CumulatedWinMatch.value;
            string bossFightStatus = pbOpenBossMapButton.isUnlocked ? "unlocked" : "locked";
            string bossDifficulty = pbOpenBossMapButton.isUnlocked ? pbOpenBossMapButton.StateText.text : "0";
            string battleRoyaleStatus = pvPPlayBattleModeButton.IsUnlockedMode ? "unlocked" : "locked";
            int battleRoyaleRequirement = pvPPlayBattleModeButton.RequiredNumOfTrophyToUnlock;

            GameEventHandler.Invoke(LogFirebaseEventCode.PlayScreenReached, duelMatchStatus, bossFightStatus, bossDifficulty, battleRoyaleStatus, battleRoyaleRequirement);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    private void HighestAchievedMedalsVariable_onValueChanged(ValueDataChanged<float> obj)
    {
        #region FTUE
        //Boss FTUE
        if (isConditionBossFTUE)
        {
            handFTUEPlay.SetActive(true);
            GameEventHandler.Invoke(LogFTUEEventCode.StartEnterBossUI);
            GameEventHandler.Invoke(FTUEEventCode.OnPlayBattleButton, battleBtn.gameObject);
        }

        //Royal FTUE
        if (isConditionBattleRoyalFTUE)
        {
            handFTUEPlay.SetActive(true);
            GameEventHandler.Invoke(LogFTUEEventCode.StartPlayBattle);
            GameEventHandler.Invoke(FTUEEventCode.OnPlayBattleButton, battleBtn.gameObject);
        }
        #endregion
    }

    private void Start()
    {
        if (!m_PSFTUESO.FTUEStartFirstMatch.value)
        {
            handFTUEPlay.gameObject.SetActive(true);
            GameEventHandler.Invoke(PSFTUEBlockAction.Block, battleBtn.gameObject, PSFTUEBubbleText.StartFirstMatch);

            GameEventHandler.Invoke(PSLogFTUEEventCode.StartFirstMatch);
        }

        if (m_PSFTUESO.FTUEOpenInfoPopup.value && !m_PSFTUESO.FTUEStart2ndMatch.value)
        {
            handFTUEPlay.gameObject.SetActive(true);
            GameEventHandler.Invoke(PSFTUEBlockAction.Block, battleBtn.gameObject, PSFTUEBubbleText.Start2ndMatch);

            GameEventHandler.Invoke(PSLogFTUEEventCode.Start2ndMatch);
        }

        currentChosenModeVariable.AdjustSaveMode();
        // UpdateView();

        if (!isActiveFullSlotBox && PBPackDockManager.Instance.IsFull)
        {
            isActiveFullSlotBox.value = true;
            GameEventHandler.Invoke(DesignEvent.FullSlotBoxSlot);
        }

        OnUpdateStartFTUECR = OnUpdateStartFTUE();
        StartCoroutine(OnUpdateStartFTUECR);
    }

    private IEnumerator OnUpdateStartFTUE()
    {
        #region FTUE
        //FTUE Single PVP
        if (isConditionSingleFTUE && !isCallOneTimeSingleFTUE)
        {
            yield return new WaitForSeconds(0);
            isCallOneTimeSingleFTUE = true;
            handFTUEPlay.SetActive(true);
            GameEventHandler.Invoke(LogFTUEEventCode.StartPlaySingle);

            //if (!SelectingModeUIFlow.HasExitedSceneWithPlayModeUIShowing)
            //    GameEventHandler.Invoke(FTUEEventCode.OnPlayBattleButton, battleBtn.gameObject);
            //else if (isConditionPreludeDoMissionFTUE)
            //    GameEventHandler.Invoke(FTUEEventCode.OnPlayBattleButton, battleBtn.gameObject);
        }

        ////Boss FTUE
        //if (isConditionBossFTUE)
        //{
        //    yield return new WaitForSeconds(0.5f);
        //    handFTUEPlay.SetActive(true);
        //    GameEventHandler.Invoke(LogFTUEEventCode.StartEnterBossUI);

        //    if (!SelectingModeUIFlow.HasExitedSceneWithPlayModeUIShowing)
        //    {
        //        GameEventHandler.Invoke(FTUEEventCode.OnPlayBattleButton, battleBtn.gameObject);
        //        if (isShowingModeUI)
        //            closeBtn?.onClick.Invoke();
        //    }

        //    if (SelectingModeUIFlow.HasExitedSceneWithPlayModeUIShowing && SelectingModeUIFlow.HasPlayAnimBoxSlot)
        //    {
        //        GameEventHandler.Invoke(FTUEEventCode.OnPlayBattleButton, battleBtn.gameObject);

        //        if (isShowingModeUI)
        //            closeBtn?.onClick.Invoke();
        //    }
        //}

        ////Royal FTUE
        //if (isConditionBattleRoyalFTUE)
        //{
        //    yield return new WaitForSeconds(0.5f);
        //    handFTUEPlay.SetActive(true);
        //    GameEventHandler.Invoke(LogFTUEEventCode.StartPlayBattle);

        //    if (!SelectingModeUIFlow.HasExitedSceneWithPlayModeUIShowing)
        //    {
        //        GameEventHandler.Invoke(FTUEEventCode.OnPlayBattleButton, battleBtn.gameObject);
        //        if (isShowingModeUI)
        //            closeBtn?.onClick.Invoke();
        //    }


        //    if (SelectingModeUIFlow.HasExitedSceneWithPlayModeUIShowing && SelectingModeUIFlow.HasPlayAnimBoxSlot)
        //    {
        //        GameEventHandler.Invoke(FTUEEventCode.OnPlayBattleButton, battleBtn.gameObject);
        //        if (isShowingModeUI)
        //            closeBtn?.onClick.Invoke();
        //    }
        //}
        #endregion
    }

    private void OnBattle()
    {
        SoundManager.Instance.PlayLoopSFX(currentChosenModeVariable.value == Mode.Boss ? soundShowBossMapClick : soundBattleClick);
        SceneName sceneName = currentChosenModeVariable.value switch
        {
            Mode.Normal => SceneName.PvP,
            Mode.Boss => SceneName.PvP_BossFight,
            Mode.Battle => SceneName.PvP,
            _ => SceneName.PvP
        };

        if (currentChosenModeVariable != Mode.Boss)
            bossFightStreakHandle.ResetAllStreak();

        switch (currentChosenModeVariable.value)
        {
            case Mode.Normal:
                var activeScene = SceneManager.GetActiveScene();
                var mainScreenUI = activeScene.GetRootGameObjects().FirstOrDefault(go => go.name == "MainScreenUI");
                foreach (var gameObject in activeScene.GetRootGameObjects())
                {
                    if (gameObject == mainScreenUI)
                        continue;
                    gameObject.SetActive(false);
                }
                SceneManager.LoadScene(sceneName.ToString(), UnityEngine.SceneManagement.LoadSceneMode.Additive, callback: OnLoadSceneCompleted);

                void OnLoadSceneCompleted(SceneManager.LoadSceneResponse loadSceneResponse)
                {
                    mainScreenUI?.SetActive(false);
                    var previousBackgroundLoadingPriority = Application.backgroundLoadingPriority;
                    Application.backgroundLoadingPriority = ThreadPriority.Low;
                    SceneManager.UnloadSceneAsync(activeScene.name).onCompleted += () =>
                    {
                        Application.backgroundLoadingPriority = previousBackgroundLoadingPriority;
                    };
                }

                break;
            case Mode.Battle:
                GameEventHandler.Invoke(BattleBetEventCode.OnBattleBetOpened);
                break;
            case Mode.Boss:
                GameEventHandler.Invoke(BossFightEventCode.OnBossMapOpened);
                break;
        }
    }

    private void UpdateView()
    {
        imageMode.sprite = currentChosenModeVariable.CurrentSpriteMode;
        if (currentChosenModeVariable.value == Mode.Normal)
        {
            battleModeObject.SetActive(false);
            singleModeObject.SetActive(true);
            panelAnimBase.SetToEnd();
        }
        else if (currentChosenModeVariable.value == Mode.Battle)
        {
            battleModeObject.SetActive(true);
            singleModeObject.SetActive(false);
            panelAnimBase.SetToEnd();
        }
        else if (currentChosenModeVariable.value == Mode.Boss)
        {
            battleModeObject.SetActive(false);
            singleModeObject.SetActive(false);
            panelAnimBase.SetToStart();
        }
    }

    private void ChooseSingleMode()
    {
        if (isConditionSingleFTUE)
        {
            if (isConditionPreludeDoMissionFTUE)
            {
                FTUE_PreludeDoMissionTab.value = true;
            }
            pprefBoolFTUE_PVP.value = true;
            handchoosenModeFTUE.SetActive(false);
            GameEventHandler.Invoke(LogFTUEEventCode.EndPlaySingle);
            GameEventHandler.Invoke(FTUEEventCode.OnDuelModeFTUE);
        }
        ScaleEffectWinStreak();
        HandleChoosenMode(Mode.Normal, requiredNumOfMedalsToUnlockSingleMode);
    }

    private void ChooseBossMode()
    {
        if (isConditionBossFTUE)
        {
            pPrefBoolFTUEBattleBoss.value = true;
            handchoosenModeFTUE.SetActive(false);
            GameEventHandler.Invoke(LogFTUEEventCode.EndEnterBossUI);
            GameEventHandler.Invoke(LogFTUEEventCode.StartPlayBossFight);
            GameEventHandler.Invoke(FTUEEventCode.OnBossModeFTUE);
        }
        HandleChoosenMode(Mode.Boss, requiredNumOfMedalsToUnlockBossMode);
    }

    private void ChooseBattleMode()
    {
        if (isConditionBattleRoyalFTUE)
        {
            //pPrefBoolBattleRoyal.value = true;
            handchoosenModeFTUE.SetActive(false);

            GameEventHandler.Invoke(LogFTUEEventCode.EndPlayBattle);
            GameEventHandler.Invoke(FTUEEventCode.OnRoyalModeSelectArena);
        }
        HandleChoosenMode(Mode.Battle, requiredNumOfMedalsToUnlockBattleMode);
    }

    private void HandleChoosenMode(Mode mode, int requiredNumOfMedals)
    {
        ScaleEffectButton(listModesBtn[mode]);
        var isUnlocked = m_HighestAchievedMedalsVariable.value >= requiredNumOfMedals;
        if (isUnlocked)
        {
            currentChosenModeVariable.GameMode = mode;
            OnBattle();
            return;
        }
        CreateNoticeTextFade($"{requiredNumOfMedals}");
    }

    private void ScaleEffectWinStreak()
    {
        if (m_WinStreakBanner == null) return;

        m_WinStreakBanner.GetComponent<RectTransform>()
            .DOScale(m_ScaleDown, m_TimeScaleDown)
            .OnComplete(() =>
            {
                m_WinStreakBanner.GetComponent<RectTransform>().DOScale(m_ScaleUp, m_TimeScaleUp);
            });
    }

    private void ScaleEffectButton(MultiImageButton multiImageButton)
    {
        RectTransform rect = multiImageButton.GetComponent<RectTransform>();
        rect.DOScale(m_ScaleDown, m_TimeScaleDown)
            .OnComplete(() =>
            {
                rect.DOScale(m_ScaleUp, m_TimeScaleUp);
            });
    }

    public void ShowModeUI()
    {
        isShowingModeUI = true;
        handFTUEPlay.SetActive(false);
        handchoosenModeFTUE.SetActive(false);

        #region FTUE
        //FTUE Single PVP
        if (isConditionSingleFTUE)
        {
            handFTUEPlay.SetActive(false);
            GameEventHandler.Invoke(FTUEEventCode.OnPlayBattleButton);
            GameEventHandler.Invoke(FTUEEventCode.OnDuelModeFTUE, listModesBtn[Mode.Normal].gameObject);
        }

        //Boss FTUE
        if (isConditionBossFTUE)
        {
            handFTUEPlay.SetActive(false);
            GameEventHandler.Invoke(FTUEEventCode.OnPlayBattleButton);
            GameEventHandler.Invoke(FTUEEventCode.OnBossModeFTUE, listModesBtn[Mode.Boss].gameObject);
        }

        //Royal FTUE
        if (isConditionBattleRoyalFTUE)
        {
            handFTUEPlay.SetActive(false);
            GameEventHandler.Invoke(FTUEEventCode.OnPlayBattleButton);
            GameEventHandler.Invoke(FTUEEventCode.OnRoyalModeFTUE, listModesBtn[Mode.Battle].gameObject);
        }
        #endregion

        selectModeUI.Show();
        GameEventHandler.Invoke(PlayModePopup.Enable);
    }

    public void HideModeUI()
    {
        isShowingModeUI = false;
        selectModeUI.Hide();
        GameEventHandler.Invoke(PlayModePopup.Disable);
    }

    private void OnClickOnSelectingButton()
    {
        HideModeUI();
    }

    private void OnClickButtonMain()
    {
        if (m_PSFTUESO.FTUEOpenInfoPopup.value && !m_PSFTUESO.FTUEStart2ndMatch.value)
        {
            handFTUEPlay.gameObject.SetActive(true);
            GameEventHandler.Invoke(PSFTUEBlockAction.Block, battleBtn.gameObject, PSFTUEBubbleText.Start2ndMatch);
        }

        if (!hasIgnoredFirstClickButtonMainEvent)
        {
            hasIgnoredFirstClickButtonMainEvent = true;
            return;
        }
        if (OnUpdateStartFTUECR != null)
            StopCoroutine(OnUpdateStartFTUECR);
        OnUpdateStartFTUECR = OnUpdateStartFTUE();
        StartCoroutine(OnUpdateStartFTUECR);
        HideModeUI();
    }

    private void OnClickButtonCharacter()
    {
        if (OnUpdateStartFTUECR != null)
            StopCoroutine(OnUpdateStartFTUECR);
    }

    private void OnOpenTrophyRoad()
    {
        if (OnUpdateStartFTUECR != null)
            StopCoroutine(OnUpdateStartFTUECR);
    }

    private void OnOutTrophyRoad()
    {
        OnUpdateStartFTUECR = OnUpdateStartFTUE();
        StartCoroutine(OnUpdateStartFTUECR);
    }

    private void OnUnpackStart()
    {
        if (OnUpdateStartFTUECR != null)
            StopCoroutine(OnUpdateStartFTUECR);
    }

    private void OnUnpackDone()
    {
        if (!pprefBoolFTUE_Upgrade.value) return;
        OnUpdateStartFTUECR = OnUpdateStartFTUE();
        StartCoroutine(OnUpdateStartFTUECR);
    }

    private void CreateNoticeTextFade(string text)
    {
        // NoticeFadeText noticeFadeText = Instantiate(noticePrefab, selectModeUI.transform).GetComponent<NoticeFadeText>();
        // noticeFadeText.SetText(text);

        // RectTransform rectNotice = noticeFadeText.GetComponent<RectTransform>();
        // if (rectNotice != null)
        //     rectNotice.anchoredPosition = new Vector3(0, 500, 0);

        var message = I2LHelper.TranslateTerm(I2LTerm.PlayModeUI_RequiresMedal).Replace("{[value]}", text);
        ToastUI.Show(message, selectModeUI.transform);
    }

    public void ShowFromButton()
    {
        Show();
    }

    void Show()
    {
        visibility.ShowImmediately();
        GameEventHandler.Invoke(MainSceneEventCode.OnShowPlayModePanel);
    }
}