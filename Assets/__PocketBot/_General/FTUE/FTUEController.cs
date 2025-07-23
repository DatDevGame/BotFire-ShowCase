using System.Collections;
using Cinemachine;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using LatteGames.GameManagement;
using LatteGames.PvP;
using System;
using UnityEngine.UI;
using LatteGames.Template;
using System.Linq;
using GachaSystem.Core;
using Sirenix.OdinInspector;
using TMPro;

public class FTUEController : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private FTUEDoubleTapToReverse ftueDoubleTapToReverse;
    [SerializeField, BoxGroup("Ref-UI")] private MultiImageButton fightButton;
    [SerializeField, BoxGroup("Ref-UI")] private CanvasGroupVisibility canvasGroupVisibility_dragToMoveText;
    [HideInInlineEditors] private CinemachineVirtualCamera readySetFightCam;
    [HideInInlineEditors] private VariableJoystick variableJoystick;

    [SerializeField] CanvasGroupVisibility box_1;
    [SerializeField] CanvasGroupVisibility box_2;
    [SerializeField] CanvasGroupVisibility gameUI;
    [SerializeField] PBPvPGameOverUI gameOverUI;

    [SerializeField] CinemachineVirtualCamera firstCamera;
    [SerializeField] PPrefBoolVariable FTUE_Fighting;
    [SerializeField] PvPArenaSO ftueArenaSO;
    [SerializeField] PvPArenaVariable currentChosenArenaVariable;
    [SerializeField] PlayerInfoVariable enemyInfoVariable;
    [SerializeField] ItemSOVariable enemyChassisSOVariable;
    [SerializeField] Button continueButton;
    [SerializeField] GameObject joystick;
    [SerializeField] PBLevelController levelController;
    [SerializeField, BoxGroup("Data")] PVPGameOverConfigSO m_PVPGameOverConfigSO;

    private bool isFighting = false;
    private const int _audienceCheerMaxLevel = 3;
    private int _countTimeLevelUpCheer = 3;
    private int _audienceCheerLevel = 1;
    private int _timeCountDecreaseAudienceCheer = 0;
    private IEnumerator CountTimeLevelUpAudienceCheerCR;
    private IEnumerator CountTimeAudienceCheerCR;
    private CharacterRoomPVP m_CharacterRoomPVP;
    void Awake()
    {
        currentChosenArenaVariable.value = ftueArenaSO;
        continueButton.onClick.AddListener(OnContinueButtonClicked);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, HandleLevelEnded);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, HandleLevelStarted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnShakeCamera, HandleSoundAudienceCheer);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);

        fightButton.onClick.AddListener(FightButtonAction);
    }

    void OnDestroy()
    {
        currentChosenArenaVariable.ResetValue();
        continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, HandleLevelEnded);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, HandleLevelStarted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnShakeCamera, HandleSoundAudienceCheer);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);

        if (CountTimeLevelUpAudienceCheerCR != null)
            StopCoroutine(CountTimeLevelUpAudienceCheerCR);

        if (CountTimeAudienceCheerCR != null)
            StopCoroutine(CountTimeAudienceCheerCR);

        fightButton.onClick.RemoveListener(FightButtonAction);
    }

    void Start()
    {
        variableJoystick = FindObjectOfType<VariableJoystick>();
        readySetFightCam = FindAnyObjectByType<ReadySetFightVCamController>().GetComponent<CinemachineVirtualCamera>();
        StartCoroutine(CR_StartFTUE());
    }

    private void Update()
    {
        if (!isFighting && readySetFightCam != null)
        {
            if (!readySetFightCam.enabled)
                readySetFightCam.enabled = true;
        }
    }

    private void FightButtonAction()
    {
        isFighting = true;
        fightButton.gameObject.SetActive(false);
        GameEventHandler.Invoke(LogFTUEEventCode.EndFightButton);
    }

    IEnumerator CR_StartFTUE()
    {
        GameEventHandler.Invoke(LogFTUEEventCode.StartFightButton);

        SoundManager.Instance.PlayLoopSFX(GetSFXlevel(_audienceCheerLevel), 1, true, false, this.gameObject);

        if (CountTimeLevelUpAudienceCheerCR != null)
            StopCoroutine(CountTimeLevelUpAudienceCheerCR);
        CountTimeLevelUpAudienceCheerCR = CountTimeConditionLevelUpAudienceCheer();
        StartCoroutine(CountTimeLevelUpAudienceCheerCR);

        if (CountTimeAudienceCheerCR != null)
            StopCoroutine(CountTimeAudienceCheerCR);
        CountTimeAudienceCheerCR = CountTimeAudienceCheer();
        StartCoroutine(CountTimeAudienceCheerCR);

        var spawnPoints = PBFightingStage.Instance.GetContestantSpawnPoints().ToList();
        for (int i = 1; i < spawnPoints.Count; i++)
            levelController.GetComponent<PvPRobotSpawner>().SpawnBotRobotFTUE(enemyInfoVariable, enemyChassisSOVariable, i);

        AIBotController enemyController = FindObjectOfType<AIBotController>();
        enemyController.RadarDetector.IsEnabled = false;
        yield return new WaitForEndOfFrame();
        var carPhysics = ObjectFindCache<CarPhysics>.GetAll();
        foreach (var item in carPhysics)
        {
            item.IsPreview = true;
        }

        readySetFightCam.enabled = true;
        variableJoystick.gameObject.SetActive(false);
        canvasGroupVisibility_dragToMoveText.Hide();
        box_1.Show();

        yield return new WaitUntil(() => isFighting);
        box_1.Hide();
        gameUI.Show();
        readySetFightCam.enabled = false;

        yield return new WaitForSeconds(1f);
        box_2.Show();

        yield return new WaitForSeconds(1f);
        box_2.Hide();
        variableJoystick.gameObject.SetActive(true);
        canvasGroupVisibility_dragToMoveText.Show();

        yield return new WaitUntil(() => variableJoystick.Direction != Vector2.zero);
        GameEventHandler.Invoke(LogFTUEEventCode.StartControl);

        canvasGroupVisibility_dragToMoveText.Hide();
        enemyController.RadarDetector.IsEnabled = true;
        ftueDoubleTapToReverse.StartFTUEDoubleTapToReverse();

        yield return new WaitForSeconds(0.25f);
        GameEventHandler.Invoke(PBLevelEventCode.OnLevelStart);
        StartCoroutine(CR_DisablePreview());
    }

    void HandleLevelEnded()
    {
        StartCoroutine(CR_HandleLevelEnded());
    }

    IEnumerator CR_HandleLevelEnded()
    {
        CharacterRoomPVP characterRoomPVP = ObjectFindCache<CharacterRoomPVP>.Get();
        yield return new WaitForSeconds(m_PVPGameOverConfigSO.TimeEndWaitWinCamera + m_PVPGameOverConfigSO.TimeMoveWinCamDuration + m_PVPGameOverConfigSO.TimeWaitCharacter);
        characterRoomPVP.PlayerCharacterInfo.CharacterSystem.SetActivePVP(false);

        CinemachineBrain cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            cinemachineBrain.m_DefaultBlend.m_Time = m_PVPGameOverConfigSO.TimeMoveWinCamDuration;
        }
        gameUI.Hide();

        if (characterRoomPVP != null)
        {
            if (cinemachineBrain != null)
                cinemachineBrain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;

            gameOverUI.ShowHeader();
            characterRoomPVP.PlayerCharacterInfo.gameObject.SetActive(true);
            characterRoomPVP.CinemachineVirtualCamera.enabled = true;
            characterRoomPVP.PlayerCharacterInfo.CharacterSystem.CurrentState = CharacterState.Victory;
            characterRoomPVP.PlayerCharacterInfo.CharacterSystem.DisableController(true);

            StartCoroutine(CommonCoroutine.Delay(m_PVPGameOverConfigSO.TimeWaitCharacter, false, () =>
            {
                gameOverUI.Show(ftueArenaSO, true, false, true);
            }));
        }
    }

    void OnContinueButtonClicked()
    {
        FTUE_Fighting.value = true;
        //SceneManager.LoadScene(SceneName.FTUE_LootBox, isPushToStack: false);
        GameEventHandler.Invoke(LogFTUEEventCode.EndControl);
        SceneManager.LoadScene(SceneName.MainScene, isPushToStack: false);
    }

    void HandleLevelStarted()
    {
        StartCoroutine(CR_HandleLevelStarted());
    }

    IEnumerator CR_HandleLevelStarted()
    {
        yield return 0;
        var carPhysics = ObjectFindCache<CarPhysics>.GetAll();
        foreach (var item in carPhysics)
        {
            item.IsPreview = true;
        }
    }

    IEnumerator CR_DisablePreview()
    {
        yield return 0;
        var carPhysics = ObjectFindCache<CarPhysics>.GetAll();
        foreach (var item in carPhysics)
        {
            item.IsPreview = false;
        }
    }
    private void HandleSoundAudienceCheer()
    {
        if (_countTimeLevelUpCheer > 0) return;
        _countTimeLevelUpCheer = 3;

        _timeCountDecreaseAudienceCheer = 10;
        _audienceCheerLevel++;
        if (_audienceCheerLevel >= _audienceCheerMaxLevel)
            _audienceCheerLevel = _audienceCheerMaxLevel;

        SoundManager.Instance.PlayLoopSFX(GetSFXlevel(_audienceCheerLevel), 1, true, false, this.gameObject);
    }
    private void OnLevelEnded(object[] parameters)
    {
        SoundManager.Instance.StopLoopSFX(this.gameObject);

        if (CountTimeLevelUpAudienceCheerCR != null)
            StopCoroutine(CountTimeLevelUpAudienceCheerCR);

        if (CountTimeAudienceCheerCR != null)
            StopCoroutine(CountTimeAudienceCheerCR);

        SoundManager.Instance.PlayLoopSFX(PBSFX.UIWinGuitar);
    }
    private SFX GetSFXlevel(int levelIndex)
    {
        if (levelIndex == 1)
            return SFX.Audience_Level1;
        else if (levelIndex == 2)
            return SFX.Audience_Level2;
        else
            return SFX.Audience_Level3;
    }
    private IEnumerator CountTimeAudienceCheer()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        int timeDecreaseEverySecond = 10;
        while (true)
        {
            if (_audienceCheerLevel > 1)
            {
                _timeCountDecreaseAudienceCheer--;
                if (_timeCountDecreaseAudienceCheer <= 0)
                {
                    _audienceCheerLevel--;
                    _timeCountDecreaseAudienceCheer = timeDecreaseEverySecond;

                    SoundManager.Instance.PlayLoopSFX(GetSFXlevel(_audienceCheerLevel), 1, true, false, this.gameObject);
                }
            }
            yield return waitForSeconds;
        }
    }
    private IEnumerator CountTimeConditionLevelUpAudienceCheer()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);
        while (true)
        {
            _countTimeLevelUpCheer--;
            yield return waitForSeconds;
        }
    }
}