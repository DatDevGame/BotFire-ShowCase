using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System.Linq;
using LatteGames.PvP.TrophyRoad;
using LatteGames.UnpackAnimation;
using System;
using HyrphusQ.SerializedDataStructure;
using HyrphusQ.Helpers;

public class FTUEMainScene : Singleton<FTUEMainScene>
{
    private const int BLOCK_SORTING_ORDER = 99;
    private const int HIGH_LINE_SORTING_ORDER = 9000;

    public PPrefBoolVariable FTUE_Equip;
    public PPrefBoolVariable FTUEUpgrade;
    public PPrefBoolVariable FTUE_PVP;
    public PPrefBoolVariable FTUE_OpenBoxSlot;
    public PPrefBoolVariable FTUE_LootBoxSlotFTUE;
    public PPrefBoolVariable FTUE_OnClickBoxSlotTheFirstTime;
    public PPrefBoolVariable FTUE_LootBoxSlotTheFirstTimeFTUE;
    public PPrefBoolVariable FTUE_ActiveBuildTab;
    public PPrefBoolVariable FTUE_ActiveSkillEnter;
    public PPrefBoolVariable FTUE_ActiveSkillClaim;
    public PPrefBoolVariable FTUE_ActiveSkillEquip;

    [SerializeField, BoxGroup("Ref")] private GameObject tutorialHandPrefab;
    [SerializeField, BoxGroup("Ref")] private GameObject eventSystem;
    [SerializeField, BoxGroup("Ref")] private GameObject playButtons;
    [SerializeField, BoxGroup("Ref")] private GameObject ftueShopHand;
    [SerializeField, BoxGroup("Ref")] private GameObject equipBackground;
    [SerializeField, BoxGroup("Ref")] private GameObject _darkenBackgroundPrefab;
    [SerializeField, BoxGroup("Ref")] private GameObject _whiteBackgroundPrefab;
    [SerializeField, BoxGroup("Ref")] private GameObject _buttonTrophyRoad;
    [SerializeField, BoxGroup("Ref")] private ScrollRect scrollRect;
    [SerializeField, BoxGroup("Ref")] private Transform _mainTabUI;
    [SerializeField, BoxGroup("Ref")] private Transform _mainSceneUI;
    [SerializeField, BoxGroup("Ref")] private Canvas _headerCanvas;
    [SerializeField, BoxGroup("Ref")] private Canvas _footerCanvas;
    [SerializeField, BoxGroup("Ref")] private Button _tabMainButton;
    [SerializeField, BoxGroup("Ref")] private SerializedDictionary<CurrencyType, CurrencyUI> _mainSceneCurrencies = new SerializedDictionary<CurrencyType, CurrencyUI>();

    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility equipBubbleText;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility buildTabText;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility equipBubbleText_2;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility equipBubbleText_3;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility equipBubbleText_4;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility equipBubbleText_5;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility selectGameMode_BubbleText;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility aNewGameModeIsAvailable_BubbleText;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility tapToOpenBoxSlot_BubbleText;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility greatJobNowGetYourReward_BubbleText;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility itemsConsumesTheRobotsBodyPower_BubbleText;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility tapToContinue_itemsConsumesTheRobotsBodyPower_BubbleText;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility overThePowerLimit;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility tapToContinue_OverThePowerLimit;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility tapToContinueFTUE;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility skinUI_TapToContinue_FTUE;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility seasonPass_PreludeSeason_FTUE;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility seasonPass_CompleteMissions_FTUE;
    [SerializeField, BoxGroup("Canvas Group")] private CanvasGroupVisibility activeSkillTutorialBubbleText1, activeSkillTutorialBubbleText2, activeSkillTutorialBubbleText3;

    [SerializeField, BoxGroup("EZ Anim")] private EZAnimBase skinUI_PanelAnim;

    [SerializeField, BoxGroup("Data")] private Variable<bool> m_IsFTUEFocus;
    [SerializeField, BoxGroup("Data")] private PBPartSO _tombrockBody;
    [SerializeField, BoxGroup("Data")] private PBPartManagerSO _specialManagerSO;
    [SerializeField, BoxGroup("Data")] private HighestAchievedPPrefFloatTracker highestTrophiesVar;
    [SerializeField, BoxGroup("Data")] private IntVariable requiredTrophiesToUnlockActiveSkillVar;

    private const int HEADER_SORTING_ORDER_DEFAULT = 50;
    private const int FOOTER_SORTING_ORDER_DEFAULT = 2;

    private bool hasDoneEquipButNotUpgrade;
    private GameObject lockBackGroundFTUE;
    private GraphicRaycaster _raycastTemp;
    private Canvas _canvasTemp;
    private List<Canvas> _currencyCanvasTemps = new();
    private OpenPackAnimationSM openPackAnimation;

    // HACK: temporary hack to check if FTUE is showing
    public bool IsShowingFTUE => _canvasTemp != null;

    private void Awake()
    {
        #region FTUE
        GameEventHandler.AddActionEvent(BlockBackGround.Lock, BlockBackGroundFTUE);
        GameEventHandler.AddActionEvent(BlockBackGround.LockDarkHasObject, BlockDarkBackGroundHasObjectTFTUE);
        GameEventHandler.AddActionEvent(BlockBackGround.LockWhiteHasObject, BlockWhiteBackGroundHasObjectTFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnBossModeFTUE, BossModeFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnSkinUI_FTUE, OnHandleSkinUI_FTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnRoyalModeFTUE, RoyalModeFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnRoyalModeHighlinetArena, RoyalModeHighlineArenaFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnBuildTabFTUE, BuildTabFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnEquipUpperFTUE, OnEquipUpperFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnEquipUpperFTUECompleted, DestroyCanvasFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnDuelModeFTUE, DuelModeFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnFinishEquipFTUE, HandleOnCharacterOpen);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnClickMoreButton, HandleOnFinishEquipFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnFinishUpgradeFTUE, HandleOnFinishUpgrade);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnChoosenModeButton, HandleOnChoosenModeButton);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnPlayBattleButton, HandleOnPlayBattleButton);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnOpenBoxSlotFTUE, OnOpenBoxSlotFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnClickStartUnlockBoxSlotFTUE, OnClickStartUnlockBoxSlotFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnClickOpenBoxSlotFTUE, OnClickOpenBoxSlotFTUE);
        GameEventHandler.AddActionEvent(FTUEBubbleText.SelectGameMode, SelectGameMode_BubleText);
        GameEventHandler.AddActionEvent(FTUEBubbleText.ANewGameModeIsAvailable, ANewGameModeIsAvailable_BubleText);
        GameEventHandler.AddActionEvent(FTUEBubbleText.TapToOpenBoxSlot, TapToOpenBoxSlot_BubleText);
        GameEventHandler.AddActionEvent(FTUEBubbleText.GreatJobNowGetYourReward, GreatJobNowGetYourReward_BubleText);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnClickBoxTheFisrtTime, OnClickBoxTheFisrtTime);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnClickStartUnlockBoxTheFirstTime, OnClickStartUnlockBoxTheFirstTime);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnClickOpenBoxTheFirstTime, OnClickOpenBoxTheFirstTime);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnShowEnergyFTUE, HandleEnergyUIFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnWaitEquipUpperFTUE, OnWaitEquipUpperFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnPreludeSeasonUnlocked_FTUE, OnPreludeSeasonUnlocked_FTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnPreludeDoMission_FTUE, OnPreludeDoMission_FTUE);
        #endregion

        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickButtonMain);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, HandleOnCharacterOpen);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnOpenrophyRoad);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnOutTrophyRoad);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnOpenSubPackStart, OnOpenSubPackStart);

    }

    private void OnDestroy()
    {
        #region FTUE
        GameEventHandler.RemoveActionEvent(BlockBackGround.Lock, BlockBackGroundFTUE);
        GameEventHandler.RemoveActionEvent(BlockBackGround.LockDarkHasObject, BlockDarkBackGroundHasObjectTFTUE);
        GameEventHandler.RemoveActionEvent(BlockBackGround.LockWhiteHasObject, BlockWhiteBackGroundHasObjectTFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnFinishEquipFTUE, HandleOnCharacterOpen);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnClickMoreButton, HandleOnFinishEquipFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnFinishUpgradeFTUE, HandleOnFinishUpgrade);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnChoosenModeButton, HandleOnChoosenModeButton);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnPlayBattleButton, HandleOnPlayBattleButton);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnBossModeFTUE, BossModeFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnSkinUI_FTUE, OnHandleSkinUI_FTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnRoyalModeFTUE, RoyalModeFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnRoyalModeHighlinetArena, RoyalModeHighlineArenaFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnBuildTabFTUE, BuildTabFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnEquipUpperFTUE, OnEquipUpperFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnEquipUpperFTUECompleted, DestroyCanvasFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnDuelModeFTUE, DuelModeFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnOpenBoxSlotFTUE, OnOpenBoxSlotFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnClickStartUnlockBoxSlotFTUE, OnClickStartUnlockBoxSlotFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnClickOpenBoxSlotFTUE, OnClickOpenBoxSlotFTUE);
        GameEventHandler.RemoveActionEvent(FTUEBubbleText.SelectGameMode, SelectGameMode_BubleText);
        GameEventHandler.RemoveActionEvent(FTUEBubbleText.ANewGameModeIsAvailable, ANewGameModeIsAvailable_BubleText);
        GameEventHandler.RemoveActionEvent(FTUEBubbleText.TapToOpenBoxSlot, TapToOpenBoxSlot_BubleText);
        GameEventHandler.RemoveActionEvent(FTUEBubbleText.GreatJobNowGetYourReward, GreatJobNowGetYourReward_BubleText);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnClickBoxTheFisrtTime, OnClickBoxTheFisrtTime);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnClickStartUnlockBoxTheFirstTime, OnClickStartUnlockBoxTheFirstTime);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnClickOpenBoxTheFirstTime, OnClickOpenBoxTheFirstTime);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnShowEnergyFTUE, HandleEnergyUIFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnWaitEquipUpperFTUE, OnWaitEquipUpperFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnPreludeSeasonUnlocked_FTUE, OnPreludeSeasonUnlocked_FTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnPreludeDoMission_FTUE, OnPreludeDoMission_FTUE);
        #endregion

        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, OnClickButtonMain);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, HandleOnCharacterOpen);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnOpenrophyRoad);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnOutTrophyRoad);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnOpenSubPackStart, OnOpenSubPackStart);
    }

    private void Start()
    {
        if (FTUE_LootBoxSlotFTUE.value)
        {
            if (_tombrockBody != null)
            {
                NewItemModule newItemModule = _tombrockBody.GetModule<NewItemModule>();
                newItemModule.isNew = false;
            }
        }
        if (!FTUE_ActiveSkillEnter.value && highestTrophiesVar >= requiredTrophiesToUnlockActiveSkillVar)
        {
            StartActiveSkillFTUE();
        }

        if (FTUE_PVP.value) return;

        _specialManagerSO.initialValue.ForEach(v => (v as PBChassisSO).IsEquipped = false);

        if (!FTUE_LootBoxSlotFTUE.value)
        {
            openPackAnimation = FindObjectOfType<OpenPackAnimationSM>();
            if (openPackAnimation != null)
            {
                openPackAnimation.OnSkipButtonClicked += OpenPackAnimation_OnSkipButtonClicked;
            }
            return;
        }

        if (FTUE_Equip.value && !FTUEUpgrade.value)
        {
            hasDoneEquipButNotUpgrade = true;
        }

        if (!FTUE_Equip.value)
        {
            StartCoroutine(DelayMainFTUE());

            eventSystem.SetActive(false);
            //playButtons.SetActive(false);

            _headerCanvas.sortingOrder = 0;
            if (_darkenBackgroundPrefab != null)
                lockBackGroundFTUE = Instantiate(_darkenBackgroundPrefab, _mainSceneUI);
            lockBackGroundFTUE.GetComponent<Canvas>().sortingOrder = 2;
            _footerCanvas.sortingOrder = 2;

            equipBubbleText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -500, 0);
            equipBubbleText.Show();
            scrollRect.enabled = false;
            StartCoroutine(CR_Start());
        }

        if (FTUE_Equip.value && !FTUEUpgrade)
        {
            scrollRect.enabled = false;
            ftueShopHand.SetActive(true);
            equipBubbleText_4.Show();
        }
        if (!FTUEUpgrade)
        {
            ftueShopHand.SetActive(true);
            //playButtons.SetActive(false);
        }
    }

    private IEnumerator DelayMainFTUE()
    {
        yield return new WaitForSeconds(0);
        _mainTabUI.gameObject.SetActive(true);
        _headerCanvas.GetComponent<Canvas>().enabled = false;
        _headerCanvas.GetComponent<Canvas>().enabled = true;
        _buttonTrophyRoad.SetActive(true);
    }

    private IEnumerator CR_Start()
    {
        yield return new WaitForSeconds(0);
        eventSystem.SetActive(true);
        ftueShopHand.SetActive(true);
    }

    private void EnableCanvasFTUE(GameObject objFTUE)
    {
        //Set Canvas Temp
        if (objFTUE.GetComponent<Canvas>() == null)
            _canvasTemp = objFTUE.AddComponent<Canvas>();

        //Set Raycast Temp
        if (objFTUE.GetComponent<GraphicRaycaster>() == null)
            _raycastTemp = objFTUE.AddComponent<GraphicRaycaster>();
    }

    private void DestroyCanvasFTUE()
    {
        if (_raycastTemp != null)
            Destroy(_raycastTemp);

        if (_canvasTemp != null)
            Destroy(_canvasTemp);

        if (lockBackGroundFTUE != null)
        {
            m_IsFTUEFocus.value = false;
            GameEventHandler.Invoke(StateBlockBackGroundFTUE.End);
            Destroy(lockBackGroundFTUE);
        }

        if (_currencyCanvasTemps.Count > 0)
        {
            foreach (Canvas canvas in _currencyCanvasTemps)
            {
                Destroy(canvas);
            }
            _currencyCanvasTemps.Clear();
        }
    }

    private void ShowCurrencyBarOnFTUECanvas(params CurrencyType[] currencyTypes)
    {
        if (currencyTypes == null)
        {
            currencyTypes = (CurrencyType[])Enum.GetValues(typeof(CurrencyType));
        }
        //Set Currency Canvas Temp
        foreach (CurrencyType currencyType in currencyTypes)
        {
            if (_mainSceneCurrencies == null ||
                !_mainSceneCurrencies.TryGetValue(currencyType, out var currencyUI) ||
                currencyUI.GetComponent<Canvas>())
                continue;
            bool oldActiveState = currencyUI.gameObject.activeSelf;
            Canvas tempCanvas = currencyUI.gameObject.AddComponent<Canvas>();
            currencyUI.gameObject.SetActive(true);//Magic. I can't explain this, but if the game object is disabled in prefab then Unity auto set overrideSorting to false on first SetActive(true)
            tempCanvas.overrideSorting = true;
            tempCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
            tempCanvas.sortingOrder = HIGH_LINE_SORTING_ORDER;
            currencyUI.gameObject.SetActive(oldActiveState);
            _currencyCanvasTemps.Add(tempCanvas);
        }
    }

    private void HandleFTUEMode()
    {
        lockBackGroundFTUE = Instantiate(_darkenBackgroundPrefab, _mainSceneUI);
        lockBackGroundFTUE.GetComponent<Canvas>().sortingOrder = BLOCK_SORTING_ORDER;
        if (_canvasTemp != null)
        {
            GameEventHandler.Invoke(StateBlockBackGroundFTUE.Start);
            m_IsFTUEFocus.value = true;
            _canvasTemp.overrideSorting = true;
            _canvasTemp.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
            _canvasTemp.sortingOrder = HIGH_LINE_SORTING_ORDER;
        }
    }

    private void HandleLockBackGroundHightLightFTUEMode()
    {
        lockBackGroundFTUE = Instantiate(_darkenBackgroundPrefab, _mainSceneUI);
        lockBackGroundFTUE.GetComponent<Canvas>().sortingOrder = BLOCK_SORTING_ORDER;
        if (_canvasTemp != null)
        {
            _canvasTemp.overrideSorting = true;
            _canvasTemp.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
            _canvasTemp.sortingOrder = HIGH_LINE_SORTING_ORDER;
        }
    }

    private void BuildTabFTUE(params object[] eventData)
    {
        //if (eventData.Length <= 0 || eventData[0] == null)
        //{
        //    buildTabText.Hide();
        //    DestroyCanvasFTUE();
        //    return;
        //}
        //buildTabText.Show();
        //GameObject objFTUE = eventData[0] as GameObject;
        //HandleDarkenUI(objFTUE);
    }

    private void OnEquipUpperFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE, true);
    }

    private void BossModeFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            GameEventHandler.Invoke(LogFTUEEventCode.EndEnterBossUI);
            GameEventHandler.Invoke(LogFTUEEventCode.StartPlayBossFight);
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
    }

    private void RoyalModeFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
    }

    private void RoyalModeHighlineArenaFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
        ShowCurrencyBarOnFTUECanvas(CurrencyType.Standard, CurrencyType.EventTicket);
    }

    private void DuelModeFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
    }

    public IEnumerator DelayDisableFTUERoyalMode()
    {
        yield return new WaitForSeconds(0.2f);
        DestroyCanvasFTUE();
    }

    private void HandleDarkenUI(GameObject hightlightObject, bool isTransparent = false)
    {
        DestroyCanvasFTUE();
        EnableCanvasFTUE(hightlightObject);

        //Adjust Sorting Order On BackGround Dark
        HandleFTUEMode();

        // Adjust Alpha BackGround
        if (isTransparent)
        {
            Image imageBackGroundDarken = lockBackGroundFTUE.GetComponentInChildren<Image>();
            if (imageBackGroundDarken != null)
            {
                // Set the background color to black with full transparency (alpha = 0)
                Color32 newColor = new Color32(0, 0, 0, 0); // RGB = 0, Alpha = 0
                imageBackGroundDarken.color = newColor;
            }
        }

    }

    private void HandleDarkenUINotObject()
    {
        DestroyCanvasFTUE();

        //Adjust Sorting Order On BackGround Highlight
        HandleFTUEMode();
    }

    private void ShowDarkenUI()
    {
        if (_raycastTemp != null)
            _raycastTemp.enabled = true;

        if (_canvasTemp != null)
            _canvasTemp.overrideSorting = true;

        if (lockBackGroundFTUE != null)
            lockBackGroundFTUE.SetActive(true);

    }
    private void HideDarkenUI()
    {
        if (_raycastTemp != null)
            _raycastTemp.enabled = false;

        if (_canvasTemp != null)
            _canvasTemp.overrideSorting = false;

        if (lockBackGroundFTUE != null)
            lockBackGroundFTUE.SetActive(false);
    }

    private void OnClickButtonMain()
    {
        equipBubbleText_3.Hide();
    }

    void HandleOnCharacterOpen()
    {
        _headerCanvas.sortingOrder = HEADER_SORTING_ORDER_DEFAULT;
        _footerCanvas.sortingOrder = FOOTER_SORTING_ORDER_DEFAULT;

        if (lockBackGroundFTUE != null)
            Destroy(lockBackGroundFTUE);

        equipBubbleText.Hide();
        ftueShopHand.SetActive(false);
        StartCoroutine(CR_HandleOnCharacterOpen());
    }

    bool hasOpenCharacterTab;
    IEnumerator CR_HandleOnCharacterOpen()
    {
        if (hasOpenCharacterTab) yield break;

        if (hasDoneEquipButNotUpgrade)
        {
            equipBubbleText_4.Hide();
            yield break;
        }

        yield return new WaitForSeconds(0.5f);
        if (equipBackground != null)
            equipBackground.SetActive(false);
        equipBubbleText_2.Hide();
        hasOpenCharacterTab = true;
    }

    void HandleOnFinishEquipFTUE()
    {
        equipBubbleText_5.Hide();
        StartCoroutine(CR_HandleOnFinishEquipFTUE());
    }

    IEnumerator CR_HandleOnFinishEquipFTUE()
    {
        if (FTUEUpgrade.value) yield break;
        yield return new WaitForSeconds(0.5f);
        equipBubbleText_3.Show();
        //yield return new WaitForSeconds(1f);
        //equipBubbleText_3.Hide();
    }

    void HandleOnFinishUpgrade()
    {
        //playButtons.SetActive(true);
        scrollRect.enabled = true;
    }

    public void ShowUpgradeMakeYouStronger()
    {
        equipBubbleText_5.Show();
    }

    private void HandleOnChoosenModeButton(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
    }

    private void HandleOnPlayBattleButton(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
    }

    private void OnOpenBoxSlotFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            FTUE_OpenBoxSlot.value = true;
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
    }

    private void OnClickStartUnlockBoxSlotFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            //DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        //HandleDarkenUI(objFTUE);
    }

    private void OnClickOpenBoxSlotFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            GameEventHandler.Invoke(FTUEBubbleText.GreatJobNowGetYourReward, true);
            FTUE_LootBoxSlotFTUE.value = true;
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
    }

    private void OnOpenrophyRoad()
    {
        HideDarkenUI();
    }

    private void OnOutTrophyRoad()
    {
        ShowDarkenUI();
    }

    private void OnUnpackStart()
    {
        HideDarkenUI();
    }

    private void OnUnpackDone()
    {
        ShowDarkenUI();
        if (!FTUE_Equip.value)
        {
            StartCoroutine(DelayMainFTUE());

            eventSystem.SetActive(false);
            //playButtons.SetActive(false);

            _headerCanvas.sortingOrder = 0;
            if (_darkenBackgroundPrefab != null)
                lockBackGroundFTUE = Instantiate(_darkenBackgroundPrefab, _mainSceneUI);
            lockBackGroundFTUE.GetComponent<Canvas>().sortingOrder = 2;
            _footerCanvas.sortingOrder = 2;

            equipBubbleText.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -500, 0);
            equipBubbleText.Show();
            scrollRect.enabled = false;
            StartCoroutine(CR_Start());
        }

        if (FTUE_LootBoxSlotFTUE.value)
        {
            if (_tombrockBody != null)
            {
                NewItemModule newItemModule = _tombrockBody.GetModule<NewItemModule>();
                newItemModule.isNew = false;
            }
        }
    }

    private void OnOpenSubPackStart()
    {
        if (FTUE_LootBoxSlotFTUE.value)
            GameEventHandler.Invoke(FTUEBubbleText.GreatJobNowGetYourReward);
    }

    private void OpenPackAnimation_OnSkipButtonClicked()
    {
        if (FTUE_LootBoxSlotFTUE.value)
            GameEventHandler.Invoke(FTUEBubbleText.GreatJobNowGetYourReward);
    }

    #region Bubble Text

    private void SelectGameMode_BubleText(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            selectGameMode_BubbleText.Hide();
            return;
        }
        selectGameMode_BubbleText.Show();
    }

    private void ANewGameModeIsAvailable_BubleText(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            aNewGameModeIsAvailable_BubbleText.Hide();
            return;
        }
        aNewGameModeIsAvailable_BubbleText.Show();
    }

    private void TapToOpenBoxSlot_BubleText(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            tapToOpenBoxSlot_BubbleText.Hide();
            return;
        }
        tapToOpenBoxSlot_BubbleText.Show();
        HandleDarkenUINotObject();
    }

    private void GreatJobNowGetYourReward_BubleText(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            greatJobNowGetYourReward_BubbleText.Hide();
            return;
        }
        greatJobNowGetYourReward_BubbleText.Show();
        HandleDarkenUINotObject();
    }
    #endregion

    private void BlockBackGroundFTUE()
    {
        HandleDarkenUINotObject();
    }

    private void OnHandleSkinUI_FTUE(params object[] eventData)
    {
        Action action = () =>
        {
            GameEventHandler.Invoke(LogFTUEEventCode.EndSkinUI);
            skinUI_TapToContinue_FTUE.Hide();
            skinUI_PanelAnim.InversePlay();
            DestroyCanvasFTUE();
        };

        GameObject objFTUE = eventData[0] as GameObject;
        StartCoroutine(CommonCoroutine.Delay(AnimationDuration.TINY, false, () =>
        {
            skinUI_PanelAnim.Play();
        }));
        HandleDarkenUI(objFTUE);
        StartCoroutine(DelayTapToContinue(action, 2, skinUI_TapToContinue_FTUE));
    }

    private void BlockDarkBackGroundHasObjectTFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
    }

    private void BlockWhiteBackGroundHasObjectTFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE, true);
    }

    private void OnClickBoxTheFisrtTime(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            FTUE_OnClickBoxSlotTheFirstTime.value = true;
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
    }

    private void OnClickStartUnlockBoxTheFirstTime(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            //if (PBRemoteConfigManager.GetInt("box1_timer", -1) == -1)
            //    DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        //HandleDarkenUI(objFTUE);
    }

    private void OnClickOpenBoxTheFirstTime(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            FTUE_LootBoxSlotTheFirstTimeFTUE.value = true;
            FTUE_ActiveBuildTab.value = true;
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
    }

    private void HandleEnergyUIFTUE(params object[] eventData)
    {
        int bubleTextIndex = 0;
        bubleTextIndex = (int)eventData[0];

        CharacterInfoUI characterInfoUI = FindObjectOfType<CharacterInfoUI>();
        if (characterInfoUI != null)
        {
            if (characterInfoUI.PowerPanel == null) return;
            Action tapToContinueCallBack = () =>
            {
                tapToContinueFTUE.Hide();

                characterInfoUI.BreakthingAnimation.DOKill();
                if (bubleTextIndex == 0)
                {
                    GameEventHandler.Invoke(LogFTUEEventCode.EndPower);
                    GameEventHandler.Invoke(LogFTUEEventCode.StartUpgrade);
                    itemsConsumesTheRobotsBodyPower_BubbleText.Hide();
                }
                else if (bubleTextIndex == 1)
                {

                    string status = "Complete";
                    GameEventHandler.Invoke(DesignEvent.Popup, status);
                    overThePowerLimit.Hide();
                }

                GraphicRaycaster graphicRaycaster = characterInfoUI.PowerPanel.GetComponent<GraphicRaycaster>();
                Canvas canvas = characterInfoUI.PowerPanel.GetComponent<Canvas>();

                if (graphicRaycaster != null)
                    Destroy(graphicRaycaster);

                if (canvas != null)
                    Destroy(canvas);

                GameEventHandler.Invoke(FTUEEventCode.OnFinishShowEnergyFTUE);
            };
            HandleDarkenUI(characterInfoUI.PowerPanel.gameObject);

            characterInfoUI.BreakthingAnimation.DOPlay();
            if (bubleTextIndex == 0)
            {
                GameEventHandler.Invoke(LogFTUEEventCode.StartPower);
                StartCoroutine(DelayTapToContinue(tapToContinueCallBack, 2, tapToContinueFTUE));
                itemsConsumesTheRobotsBodyPower_BubbleText.Show();
            }
            else if (bubleTextIndex == 1)
            {
                string status = "Start";
                GameEventHandler.Invoke(DesignEvent.Popup, status);
                StartCoroutine(DelayTapToContinue(tapToContinueCallBack, 2, tapToContinueFTUE));
                overThePowerLimit.Show();
            }

        }
    }

    private void OnWaitEquipUpperFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            DestroyCanvasFTUE();
            return;
        }
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE, true);
    }

    private IEnumerator DelayTapToContinue(Action actionTapToContinue, float timeWait, CanvasGroupVisibility canvasGroupVisibility_TapToContinue)
    {
        yield return new WaitForSeconds(timeWait);
        canvasGroupVisibility_TapToContinue.Show();
        if (actionTapToContinue != null)
        {
            GameEventHandler.Invoke(StateBlockBackGroundFTUE.End);
            PointerDarkenUICallBack pointerDarkenUICallBack = lockBackGroundFTUE.gameObject.AddComponent<PointerDarkenUICallBack>();
            pointerDarkenUICallBack.SetupPointerAction(actionTapToContinue);
        }
        //tapToContinueFTUE.Show();
    }

    private void OnPreludeSeasonUnlocked_FTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            DestroyCanvasFTUE();
            seasonPass_PreludeSeason_FTUE.Hide();
            return;
        }
        seasonPass_PreludeSeason_FTUE.Show();
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
    }

    private void OnPreludeDoMission_FTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            DestroyCanvasFTUE();
            seasonPass_CompleteMissions_FTUE.Hide();
            return;
        }
        seasonPass_CompleteMissions_FTUE.Show();
        GameObject objFTUE = eventData[0] as GameObject;
        HandleDarkenUI(objFTUE);
    }

    private void StartActiveSkillFTUE()
    {
        RectTransform tutorialHand = null;
        List<Canvas> highlightCanvases = new List<Canvas>();
        List<GraphicRaycaster> hightlightRaycasters = new List<GraphicRaycaster>();
        // DestroyHighlightCanvases();

        Vector2 clickSkillCardHandPos = new Vector2(47f, -39.5f);
        Vector2 claimFreeSkillCardHandPos = new Vector2(69.3f, -127.3f);
        Vector2 equipSkillHandPos = new Vector2(110f, 0f);
        CharacterTabButton characterTabButton = ObjectFindCache<CharacterTabButton>.Get();
        Button button = characterTabButton.GetComponent<Button>();
        GearTabSelection gearTabSelection = ObjectFindCache<GearTabSelection>.Get();
        gearTabSelection.DefaultReloadTabButton = gearTabSelection.ActiveSkillTabButton;
        ActiveSkillGridView activeSkillGridView = ObjectFindCache<ActiveSkillGridView>.Get();
        ActiveSkillInfoPopup activeSkillInfoPopup = ObjectFindCache<ActiveSkillInfoPopup>.Get();
        NoAdsOffersPopup noAdsOffersPopup = ObjectFindCache<NoAdsOffersPopup>.Get();
        noAdsOffersPopup.RootCanvas.enabled = false;
        ItemCell highlightActiveSkillCard = activeSkillGridView.itemCells[0];
        HighlightObject(characterTabButton.gameObject);
        characterTabButton.FTUEHand.SetActive(true);
        button.onClick.AddListener(OnCharacterUIClicked);
        ShowBubbleText(activeSkillTutorialBubbleText1);

        #region FTUE Event
        GameEventHandler.Invoke(LogFTUEEventCode.StartActiveSkillEnter);
        #endregion

        void OnCharacterUIClicked()
        {
            #region FTUE Event
            GameEventHandler.Invoke(LogFTUEEventCode.EndActiveSkillEnter);
            GameEventHandler.Invoke(LogFTUEEventCode.StartActiveSkillClaim);
            #endregion

            button.onClick.RemoveListener(OnCharacterUIClicked);
            HideBubbleText(activeSkillTutorialBubbleText1);
            ShowBubbleText(activeSkillTutorialBubbleText2);
            List<(GameObject, int)> highlightObjects = new List<(GameObject, int)>()
            {
                (gearTabSelection.ActiveSkillTabButton.gameObject, -1),
                (gearTabSelection.SubTabGO, 0),
                (activeSkillGridView.transform.parent.gameObject, 0),
            };
            HighlightObjects(highlightObjects);
            CallToAction(highlightActiveSkillCard.transform, clickSkillCardHandPos);
            activeSkillGridView.DisableAllButtons(highlightActiveSkillCard);
            highlightActiveSkillCard.onItemClicked += OnActiveSkillCardClicked;
        }

        void OnActiveSkillCardClicked(ItemCell.ClickedEventData data)
        {
            activeSkillGridView.EnableAllButtons();
            highlightActiveSkillCard.onItemClicked -= OnActiveSkillCardClicked;
            HideBubbleText(activeSkillTutorialBubbleText2);
            HighlightObject(activeSkillInfoPopup.gameObject);
            if (!FTUE_ActiveSkillClaim.value)
            {
                ShowBubbleText(activeSkillTutorialBubbleText3);
                activeSkillInfoPopup.DisableAllButtons();
                activeSkillInfoPopup.StartTutorialClaimFreeSkillCard(OnActiveSkillClaimFreeCardTutorialCompleted, OnActiveSkillEquipTutorialCompleted);
                CallToAction(activeSkillInfoPopup.GetActiveSkillCardFreeOffer().transform, claimFreeSkillCardHandPos);
            }
            else
            {
                activeSkillInfoPopup.DisableAllButtons();
                activeSkillInfoPopup.StartTutorialEquipSkillCard(OnActiveSkillEquipTutorialCompleted);
                CallToAction(activeSkillInfoPopup.GetEquipButton().transform, equipSkillHandPos);
            }
        }

        void OnActiveSkillClaimFreeCardTutorialCompleted()
        {
            #region FTUE Event
            GameEventHandler.Invoke(LogFTUEEventCode.EndActiveSkillClaim);
            GameEventHandler.Invoke(LogFTUEEventCode.StartActiveSkillEquip);
            #endregion

            FTUE_ActiveSkillClaim.value = true;
            HideBubbleText(activeSkillTutorialBubbleText3);
            CallToAction(activeSkillInfoPopup.GetEquipButton().transform, equipSkillHandPos);
        }

        void OnActiveSkillEquipTutorialCompleted()
        {
            #region FTUE Event
            GameEventHandler.Invoke(LogFTUEEventCode.EndActiveSkillEquip);
            #endregion

            FTUE_ActiveSkillEnter.value = true;
            FTUE_ActiveSkillEquip.value = true;
            noAdsOffersPopup.RootCanvas.enabled = true;
            activeSkillInfoPopup.EnableAllButtons();
            DestroyHighlightCanvases();
            GetOrCreateTutorialHand().gameObject.SetActive(false);
        }

        void CallToAction(Transform targetTransform, Vector2 anchoredPosition)
        {
            GetOrCreateTutorialHand().gameObject.SetActive(true);
            GetOrCreateTutorialHand().SetParent(targetTransform);
            GetOrCreateTutorialHand().anchoredPosition = anchoredPosition;
        }

        RectTransform GetOrCreateTutorialHand()
        {
            if (tutorialHand == null)
            {
                tutorialHand = Instantiate(tutorialHandPrefab, transform.GetChild(0)).transform as RectTransform;
                Canvas handCanvas = tutorialHand.GetOrAddComponent<Canvas>();
                handCanvas.overrideSorting = true;
                handCanvas.sortingOrder = HIGH_LINE_SORTING_ORDER + 50;
            }
            return tutorialHand;
        }

        void DestroyHighlightCanvases()
        {
            for (int i = 0; i < highlightCanvases.Count; i++)
            {
                DestroyImmediate(hightlightRaycasters[i]);
                DestroyImmediate(highlightCanvases[i]);
            }
            highlightCanvases.Clear();
            hightlightRaycasters.Clear();
            DestroyCanvasFTUE();
        }

        void HighlightObject(GameObject highlightObject, int orderOffset = 0)
        {
            HighlightObjects(new List<(GameObject, int)>() { (highlightObject, orderOffset) });
        }

        void HighlightObjects(List<(GameObject, int)> highlightObjects)
        {
            DestroyHighlightCanvases();
            AddHighlightCanvases(highlightObjects);
            _canvasTemp = highlightCanvases[0];
            HandleFTUEMode();
        }

        void AddHighlightCanvases(List<(GameObject, int)> highlightObjects)
        {
            for (int i = 0; i < highlightObjects.Count; i++)
            {
                GameObject highlightObject = highlightObjects[i].Item1;
                Canvas hightlightCanvas = highlightObject.GetOrAddComponent<Canvas>();
                GraphicRaycaster graphicRaycaster = highlightObject.GetOrAddComponent<GraphicRaycaster>();
                hightlightCanvas.overrideSorting = true;
                hightlightCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
                hightlightCanvas.sortingOrder = HIGH_LINE_SORTING_ORDER + highlightObjects[i].Item2;
                highlightCanvases.Add(hightlightCanvas);
                hightlightRaycasters.Add(graphicRaycaster);
            }
        }

        void ShowBubbleText(CanvasGroupVisibility bubbleTextVisibility)
        {
            bubbleTextVisibility.gameObject.SetActive(true);
            bubbleTextVisibility.Show();
        }

        void HideBubbleText(CanvasGroupVisibility bubbleTextVisibility)
        {
            bubbleTextVisibility.gameObject.SetActive(false);
        }
    }

    [Button]
    private void ResetActiveSkillFTUE()
    {
        FTUE_ActiveSkillEnter.Clear();
        FTUE_ActiveSkillEquip.Clear();
        FTUE_ActiveSkillClaim.Clear();
        ActiveSkillGridView activeSkillGridView = ObjectFindCache<ActiveSkillGridView>.Get();
        if (activeSkillGridView != null)
        {
            activeSkillGridView.itemManagerSO.Use(activeSkillGridView.itemManagerSO[1]);
        }
    }
}