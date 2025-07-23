using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.GUI;
using I2.Loc;
using LatteGames.PvP;
using LatteGames.PvP.TrophyRoad;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class PBOpenBossMapButton : MonoBehaviour
{
    public TMP_Text StateText => stateText;

    [SerializeField] TMP_Text stateText;
    [SerializeField] LocalizationParamsManager localizationParamsManager;
    [SerializeField] GameObject m_CallToFightGroup, m_RPSGroup;
    [SerializeField] RPSCalculatorSO m_RPSCalculatorSO;
    [SerializeField] PBRobotStatsSO bossPBRobotStatsSO, playerPBRobotStatsSO;
    [SerializeField] RectTransform m_Arrow;
    [SerializeField] Sprite previousBossSprite, currentBossSprite, nextBossSprite;
    // [SerializeField] Image bossNodeImgPrefab;
    [SerializeField] AnimationCurve m_CurveX;
    [SerializeField] private GameObject rpsObject;
    // [SerializeField] private GameObject progressNodes;
    [SerializeField] private IntVariable m_RequiredNumOfMedalsToUnlockVariable;
    [SerializeField] private FloatVariable m_HighestAchievedMedalsVariable;
    [SerializeField] private ModeVariable currentChosenModeVariable;
    [SerializeField] private Image imageMode;
    [SerializeField] private GameObject unlockAtGroup, bossInfoGroup, unlockTrophyGroup;
    [SerializeField] TMP_Text availableTxt, trophyAmountTxt, lockedTrophyAmountTxt, bossNameTxt;
    [SerializeField] private GameObject newTag;
    [SerializeField] private GameObject skillDisableNoticeGO;
    [SerializeField] private IntVariable requiredTrophiesToUnlockActiveSkill;

    private BossMapSO bossMapSO => BossFightManager.Instance.bossMapSO;
    BossChapterSO currentChapterSO => bossMapSO.currentChapterSO;
    public bool isUnlocked => m_HighestAchievedMedalsVariable.value >= m_RequiredNumOfMedalsToUnlockVariable;
    public bool isComingSoonChapter => currentChapterSO.isComingSoonChapter;
    public bool isUnlockedTrophyRequirement => currentChapterSO.isUnlockedTrophyRequirement;
    public bool currentBossIsUnlocked => m_HighestAchievedMedalsVariable.value >= currentChapterSO.currentBossSO.unlockedTrophyAmount;

    // private List<Image> bossNodes = new List<Image>();
    private bool _isShow = true;
    private string originalTrophyAmountString, originalLockedTrophyAmountString, originalBossNameString;

    [SerializeField, BoxGroup("Ref")] private Image backGroundMode;

    [SerializeField, BoxGroup("Sprite")] private Sprite EnableBackGround;
    [SerializeField, BoxGroup("Sprite")] private Sprite DisableBackGround;

    //FTUE
    [SerializeField, TitleGroup("FTUE")] private GameObject _handFTUE;
    [SerializeField, TitleGroup("FTUE")] private PPrefBoolVariable _pPrefBoolFTUEBattleBoss;
    private IEnumerator DelayFTUE;
    private void Awake()
    {
        originalTrophyAmountString = trophyAmountTxt.text;
        originalLockedTrophyAmountString = lockedTrophyAmountTxt.text;
        originalBossNameString = bossNameTxt.text;

        imageMode.sprite = currentChosenModeVariable.SpriteMode[Mode.Boss];
        // bossNodes.Add(bossNodeImgPrefab);

        // GetComponent<MultiImageButton>().onClick.AddListener(() =>
        // {
        //     bool isUnlocked = m_HighestAchievedMedalsVariable.value >= m_RequiredNumOfMedalsToUnlockVariable;
        //     if (isUnlocked)
        //     {
        //         //GameEventHandler.Invoke(BossFightEventCode.OnBossMapOpened);
        //         return;
        //     }
        // });
    }

    private void Start()
    {
        UpdateView();
        //FTUEBossMode(0.2f);

        m_HighestAchievedMedalsVariable.onValueChanged += OnMedalsChanged;
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnOpenrophyRoad);
        GameEventHandler.AddActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnOutTrophyRoad);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnShowPlayModePanel, OnShowPlayModePanel);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonCharacter, Hide);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonMain, Show);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnBossModeFTUE, OnBossModeFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnChoosenModeButton, ChooseModeEvent);
    }

    private void OnDestroy()
    {
        m_HighestAchievedMedalsVariable.onValueChanged -= OnMedalsChanged;

        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadOpened, OnOpenrophyRoad);
        GameEventHandler.RemoveActionEvent(TrophyRoadEventCode.OnTrophyRoadClosed, OnOutTrophyRoad);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnShowPlayModePanel, OnShowPlayModePanel);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonCharacter, Hide);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonMain, Show);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnBossModeFTUE, OnBossModeFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnChoosenModeButton, ChooseModeEvent);

        if (DelayFTUE != null)
            StopCoroutine(DelayFTUE);
    }

    private void OnMedalsChanged(ValueDataChanged<float> eventData)
    {
        UpdateView();
        //FTUEBossMode(0);
    }
    private void OnOpenrophyRoad()
    {
        if (DelayFTUE != null)
            StopCoroutine(DelayFTUE);
    }
    private void OnOutTrophyRoad()
    {
        //FTUEBossMode(0);
    }
    private void Show()
    {
        _isShow = true;
        UpdateView();
        //FTUEBossMode(0);
    }
    private void Hide()
    {
        _isShow = false;
    }

    private void UpdateView()
    {
        rpsObject.SetActive(isUnlocked);
        bossInfoGroup.SetActive(isUnlocked);
        unlockAtGroup.SetActive(!isUnlocked);
        m_RPSGroup.SetActive(true);

        availableTxt.gameObject.SetActive(isUnlocked && !isComingSoonChapter && currentBossIsUnlocked);
        newTag.gameObject.SetActive(isUnlockedTrophyRequirement && isUnlocked && !isComingSoonChapter && currentBossIsUnlocked);
        lockedTrophyAmountTxt.gameObject.SetActive(!isUnlocked);
        bossNameTxt.gameObject.SetActive(isUnlocked);

        lockedTrophyAmountTxt.text = originalLockedTrophyAmountString.Replace("{Value}", m_RequiredNumOfMedalsToUnlockVariable.value.ToString());

        if (!isComingSoonChapter)
        {
            unlockTrophyGroup.SetActive(isUnlocked && !currentBossIsUnlocked);
            trophyAmountTxt.text = originalTrophyAmountString.Replace("{Value}", currentChapterSO.currentBossSO.unlockedTrophyAmount.ToString());
            bossNameTxt.text = originalBossNameString.Replace("{Index}", (currentChapterSO.bossIndex + 1).ToString()).Replace("{Name}", currentChapterSO.currentBossSO.botInfo.name);
        }
        else
        {
            unlockTrophyGroup.SetActive(true);
            trophyAmountTxt.text = originalTrophyAmountString.Replace("{Value}", "???");
            bossNameTxt.SetText(I2LHelper.TranslateTerm(I2LTerm.ButtonTitle_ComingSoon));
        }

        if (isUnlocked)
        {
            //GetComponent<MultiImageButton>().enabled = true;
            if (!currentChapterSO.isComingSoonChapter)
            {
                //Update RPS UI
                var rpsData = m_RPSCalculatorSO.CalcRPSValue(playerPBRobotStatsSO, bossPBRobotStatsSO);
                stateText.text = rpsData.stateLabel;
                m_Arrow.anchoredPosition = new Vector2(m_CurveX.Evaluate(rpsData.rpsInverseLerp), m_Arrow.anchoredPosition.y);
                m_RPSGroup.SetActive(true);
                m_CallToFightGroup.SetActive(false);
            }
        }
        else
        {
            //GetComponent<MultiImageButton>().enabled = false;
            m_CallToFightGroup.SetActive(false);
            //m_RPSGroup.SetActive(false);

            localizationParamsManager.SetParameterValue("NumOfMedals", m_RequiredNumOfMedalsToUnlockVariable.value.ToString());
        }

        backGroundMode.sprite = isUnlocked ? EnableBackGround : DisableBackGround;
        skillDisableNoticeGO.SetActive(m_HighestAchievedMedalsVariable >= requiredTrophiesToUnlockActiveSkill);
    }

    private void FTUEBossMode(float timedelay)
    {
        if (!_isShow) return;

        if (isUnlocked && _pPrefBoolFTUEBattleBoss != null)
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
        if (!_pPrefBoolFTUEBattleBoss.value)
        {
            GameEventHandler.Invoke(LogFTUEEventCode.StartEnterBossUI);
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
    }

    void OnShowPlayModePanel()
    {
        UpdateView();
        //FTUEBossMode(0);
    }

    private void OnBossModeFTUE(params object[] eventData)
    {
        if (eventData.Length <= 0 || eventData[0] == null)
        {
            return;
        }
        FTUEBossMode(0);
    }

    private void ChooseModeEvent()
    {
        if (!_pPrefBoolFTUEBattleBoss.value)
        {
            Button button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                _handFTUE.SetActive(false);
            });
        }
    }

#if UNITY_EDITOR
    [BoxGroup("Editor Only"), OnValueChanged("UpdateViewOnEditor"), SerializeField] float playerStats, bossStats;
    private void UpdateViewOnEditor()
    {
        var currentChapterSO = bossMapSO.currentChapterSO;

        if (!currentChapterSO.isComingSoonChapter)
        {
            //Update RPS UI
            var rpsData = m_RPSCalculatorSO.CalcRPSValue(playerStats, bossStats);
            stateText.text = rpsData.stateLabel;
            m_Arrow.anchoredPosition = new Vector2(m_CurveX.Evaluate(rpsData.rpsInverseLerp), m_Arrow.anchoredPosition.y);
            if (rpsData.rpsValue > 0.3f)
            {
                m_CallToFightGroup.SetActive(true);
                m_RPSGroup.SetActive(false);
            }
            else
            {
                m_RPSGroup.SetActive(true);
                m_CallToFightGroup.SetActive(false);
            }
        }
    }
#endif
}