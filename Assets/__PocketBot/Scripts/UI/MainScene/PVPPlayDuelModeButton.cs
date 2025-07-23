using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using I2.Loc;
using LatteGames.PvP.TrophyRoad;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class PVPPlayDuelModeButton : PvPPlayButton
{
    [SerializeField]
    private GameObject _unlockTrophyGroup;
    [SerializeField]
    private LocalizationParamsManager _localizationParamsManager;
    [SerializeField]
    private IntVariable m_RequiredNumOfMedalsToUnlockVariable;
    [SerializeField]
    private FloatVariable m_HighestAchievedMedalsVariable;

    private bool _isShow = true;

    //FTUE
    [SerializeField, TitleGroup("FTUE")] private GameObject _handFTUE;
    [SerializeField, TitleGroup("FTUE")] private PPrefBoolVariable _pPrefBoolFTUEDuel;

    private IEnumerator DelayFTUE;

    private void Awake()
    {
        imageMode.sprite = m_CurrentChosenModeVariable.SpriteMode[Mode.Normal];
    }

    protected override void Start()
    {
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnOpenrophyRoad);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnOutTrophyRoad);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnShowPlayModePanel, OnShowPlayModePanel);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, Hide);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, Show);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnDuelModeFTUE, OnDuelModeFTUE);

        base.Start();
        m_HighestAchievedMedalsVariable.onValueChanged += OnMedalsChanged;
        UpdateView();
        //FTUEDuelMode(0.2f);
    }

    protected override void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnOpenrophyRoad);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnOutTrophyRoad);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnShowPlayModePanel, OnShowPlayModePanel);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, Hide);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, Show);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnDuelModeFTUE, OnDuelModeFTUE);

        base.OnDestroy();
        m_HighestAchievedMedalsVariable.onValueChanged -= OnMedalsChanged;
    }

    private void OnMedalsChanged(ValueDataChanged<float> eventData)
    {
        UpdateView();
        //FTUEDuelMode(0);
    }

    private void UpdateView()
    {
        _localizationParamsManager.SetParameterValue("NumOfMedals", m_RequiredNumOfMedalsToUnlockVariable.value.ToString());
        var isUnlocked = m_HighestAchievedMedalsVariable.value >= m_RequiredNumOfMedalsToUnlockVariable;
        m_PlayButton.interactable = isUnlocked;
        _unlockTrophyGroup.SetActive(!isUnlocked);
    }

    protected override void OnPlayButtonClicked()
    {
        base.OnPlayButtonClicked();
        _handFTUE.SetActive(false);
    }

    private void OnOpenrophyRoad()
    {
        if (DelayFTUE != null)
            StopCoroutine(DelayFTUE);
    }
    private void OnOutTrophyRoad()
    {
        _isShow = true;
        //FTUEDuelMode(0);
    }
    private void Show()
    {
        _isShow = true;
        //FTUEDuelMode(0);
    }
    private void Hide()
    {
        _isShow = false;
    }

    void OnShowPlayModePanel()
    {
        //FTUEDuelMode(0);
    }

    private void FTUEDuelMode(float timedelay)
    {
        if (m_RequiredNumOfMedalsToUnlockVariable.value <= 0) return;

        if (!_isShow) return;
        var isUnlocked = m_HighestAchievedMedalsVariable.value >= m_RequiredNumOfMedalsToUnlockVariable;
        if (isUnlocked && _pPrefBoolFTUEDuel != null)
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
        if (!_pPrefBoolFTUEDuel.value)
        {
            multiImageButton.enabled = false;
            yield return new WaitForSeconds(timeDelay);
            _handFTUE.SetActive(true);
            multiImageButton.enabled = true;

            Button button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                _handFTUE.SetActive(false);
            });
        }
        yield return null;
    }

    private void OnDuelModeFTUE()
    {
        FTUEDuelMode(0);
        _handFTUE.SetActive(true);
    }
}
