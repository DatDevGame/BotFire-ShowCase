using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using I2.Loc;
using TMPro;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using LatteGames.PvP.TrophyRoad;

public class PvPPlayBattleModeButton : PvPPlayButton
{
    public bool IsUnlockedMode
    {
        get
        {
            if (m_RequiredNumOfMedalsToUnlockVariable == null || m_HighestAchievedMedalsVariable == null)
                return false;
            return m_HighestAchievedMedalsVariable.value >= m_RequiredNumOfMedalsToUnlockVariable;
        }
    }
    public int RequiredNumOfTrophyToUnlock => m_RequiredNumOfMedalsToUnlockVariable.value;

    private const string kNumOfMedalsLocalizedParam = "NumOfMedals";
    private const string kNumOfTicketsLocalizedParam = "NumOfTickets";
    private const string kMaxTicketLocalizedParam = "MaxTicket";

    [SerializeField]
    private IntVariable m_RequiredNumOfMedalsToUnlockVariable;
    [SerializeField]
    private FloatVariable m_HighestAchievedMedalsVariable;
    [SerializeField] GameObject unlockTrophyGroup;
    [SerializeField] TMP_Text lockedTrophyAmountTxt;


    protected override Mode mode => Mode.Battle;
    private bool _isShow = true;
    private string originalLockedTrophyAmountString;

    [SerializeField, BoxGroup("Ref")] private Image backGroundMode;
    [SerializeField, BoxGroup("Ref")] private GameObject DescTxt;

    [SerializeField, BoxGroup("Sprite")] private Sprite EnableBackGround;
    [SerializeField, BoxGroup("Sprite")] private Sprite DisableBackGround;

    //FTUE
    [SerializeField, TitleGroup("FTUE")] private GameObject _handFTUE;
    [SerializeField, TitleGroup("FTUE")] private PPrefBoolVariable _pPrefBoolFTUEBattleRoyal;
    private IEnumerator DelayFTUE;

    private void Awake()
    {
        imageMode.sprite = m_CurrentChosenModeVariable.SpriteMode[Mode.Battle];
        originalLockedTrophyAmountString = lockedTrophyAmountTxt.text;
    }

    protected override void Start()
    {
        base.Start();
        m_HighestAchievedMedalsVariable.onValueChanged += OnMedalsChanged;
        UpdateView();
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnOpenrophyRoad);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnOutTrophyRoad);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnShowPlayModePanel, OnShowPlayModePanel);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, Hide);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, Show);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnRoyalModeFTUE, RoyalModeFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnChoosenModeButton, ChooseModeEvent);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        m_HighestAchievedMedalsVariable.onValueChanged -= OnMedalsChanged;

        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnOpenrophyRoad);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnOutTrophyRoad);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnShowPlayModePanel, OnShowPlayModePanel);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, Hide);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, Show);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnRoyalModeFTUE, RoyalModeFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnChoosenModeButton, ChooseModeEvent);
    }
    private void OnOpenrophyRoad()
    {
        if (DelayFTUE != null)
            StopCoroutine(DelayFTUE);
    }
    private void OnOutTrophyRoad()
    {
        _isShow = true;

        FTUEBattleRoyalMode(0);
    }
    private void Show()
    {
        _isShow = true;
    }
    private void Hide()
    {
        _isShow = false;
    }

    void OnShowPlayModePanel()
    {

    }

    private void OnMedalsChanged(ValueDataChanged<float> eventData)
    {
        UpdateView();

    }
    private void FTUEBattleRoyalMode(float timedelay)
    {
        if (m_RequiredNumOfMedalsToUnlockVariable.value <= 0) return;

        if (!_isShow) return;
        var isUnlocked = m_HighestAchievedMedalsVariable.value >= m_RequiredNumOfMedalsToUnlockVariable;
        if (isUnlocked && _pPrefBoolFTUEBattleRoyal != null)
        {
            MultiImageButton multiImageButton = GetComponent<MultiImageButton>();
            if (DelayFTUE != null)
                StopCoroutine(DelayFTUE);
            DelayFTUE = DelayLoadFTUE(multiImageButton, timedelay);
            StartCoroutine(DelayFTUE);
        }
    }
    private IEnumerator DelayLoadFTUE(MultiImageButton multiImageButton, float timeDelay)
    {
        if (!_pPrefBoolFTUEBattleRoyal.value)
        {
            GameEventHandler.Invoke(LogFTUEEventCode.StartPlayBattle);
            _handFTUE.SetActive(true);
            multiImageButton.enabled = false;
            yield return new WaitForSeconds(timeDelay);
            multiImageButton.enabled = true;

            Button button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                _handFTUE.SetActive(false);
            });
        }
        yield return null;
    }
    private void UpdateView()
    {
        var isUnlocked = m_HighestAchievedMedalsVariable.value >= m_RequiredNumOfMedalsToUnlockVariable;
        backGroundMode.sprite = isUnlocked ? EnableBackGround : DisableBackGround;
        DescTxt.SetActive(isUnlocked);
        unlockTrophyGroup.SetActive(!isUnlocked);
        lockedTrophyAmountTxt.text = originalLockedTrophyAmountString.Replace("{Value}", m_RequiredNumOfMedalsToUnlockVariable.value.ToString());
    }

    protected override void OnPlayButtonClicked()
    {
        base.OnPlayButtonClicked();
    }

    private void RoyalModeFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            return;
        }
        FTUEBattleRoyalMode(0);
    }

    private void ChooseModeEvent()
    {
        if (!_pPrefBoolFTUEBattleRoyal.value)
        {
            Button button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                _handFTUE.SetActive(false);
            });
        }
    }
}