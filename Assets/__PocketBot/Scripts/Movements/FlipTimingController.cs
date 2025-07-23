using System.Collections;
using System.Collections.Generic;
using HightLightDebug;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using HyrphusQ.SerializedDataStructure;
using LatteGames;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Linq;
using static MultiplyRewardArc;
using System;

public enum FlipForce
{
    VeryLow,
    Low,
    Medium,
    High
}

public enum FlipTimingEvent
{
    Open,
    Close,
    Flip
}

[System.Serializable]
public class SegmentFlip
{
    public List<SegmentInfo> SegmentInfo;
    public float RunSpeed;
    public Sprite MultiplierSprite;
}

public class FlipTimingController : MultiplyRewardArc
{
    [SerializeField, BoxGroup("Property")] private float flipHighestValue;

    [SerializeField, BoxGroup("Ref")] private Image boardImage;
    [SerializeField, BoxGroup("Ref")] private Image hightLightFlipImage;
    [SerializeField, BoxGroup("Ref")] private Image timerClockWise;
    [SerializeField, BoxGroup("Ref")] private TMP_Text flipText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text tapToFlipText;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility mainCanvasGroup;
    [SerializeField, BoxGroup("Ref")] private SerializedDictionary<int, SegmentFlip> segmentFlip;

    [SerializeField, BoxGroup("Resource")] private SerializedDictionary<float, FlipForce> flipForceDic;

    [SerializeField, BoxGroup("Color")] private Color flipZoneColor;
    [SerializeField, BoxGroup("Color")] private Color missZoneColor;

    [SerializeField, BoxGroup("Data")] private CurrentHighestArenaVariable currentHighestArenaVariable;

    [ShowInInspector] private PBRobot playerRobot;
    private CarPhysics playerCarphysic => playerRobot.ChassisInstance.CarPhysics;

    private PBLevelController pbLevelController;

    private bool isBotImmobilized = false;
    private float timer;
    private bool isNotTapFlip = true;
    private IEnumerator HandleFlipTimingCR;
    private IEnumerator CountDownTimerFlipCR;
    private IEnumerator DelayCheckBotImmobilizedCR;

    private const float TIME_DEFAULT_TIMING_FLIP = 10;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(FlipTimingEvent.Open, OpenFlipTiming);
        GameEventHandler.AddActionEvent(FlipTimingEvent.Close, CloseFlipTiming);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, BotModelSpawned);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotImmobilized, BotImmobilized);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotRecoveredFromImmobilized, BotRecovered);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelDespawned, OnModelDespawned);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotEffectApplied, OnRobotEffectApplied);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotEffectRemoved, OnRobotEffectRemoved);
    }
    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(FlipTimingEvent.Open, OpenFlipTiming);
        GameEventHandler.RemoveActionEvent(FlipTimingEvent.Close, CloseFlipTiming);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, BotModelSpawned);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotImmobilized, BotImmobilized);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotRecoveredFromImmobilized, BotRecovered);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelDespawned, OnModelDespawned);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotEffectApplied, OnRobotEffectApplied);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotEffectRemoved, OnRobotEffectRemoved);
    }

    private void Start()
    {
        pbLevelController = ObjectFindCache<PBLevelController>.Get();
        // SetupFollowingArena();
    }

    public virtual void TapToFlip()
    {
        if (!isNotTapFlip) return;
        isNotTapFlip = false;

        tapToFlipText.gameObject.SetActive(false);

        if (HandleFlipTimingCR != null)
            StopCoroutine(HandleFlipTimingCR);
        HandleFlipTimingCR = HandleFlipTiming();
        StartCoroutineIfActive(HandleFlipTimingCR);
    }

    private void OnModelDespawned()
    {
        CloseFlipTiming();
    }

    public override void StartRun()
    {
        // runSpeed = 0.75f;
        flipText.transform.DOScale(Vector3.one, 0);
        hightLightFlipImage.transform.DOScale(Vector3.zero, 0);
        tapToFlipText.gameObject.SetActive(true);

        base.StartRun();
    }


    private void Update()
    {

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.J))
        {
            CheatFlipPlayer cheatFlipPlayer = FindObjectOfType<CheatFlipPlayer>();
            cheatFlipPlayer.Flip();
        }
#endif
    }


    private IEnumerator CountDownTimer()
    {
        timer = TIME_DEFAULT_TIMING_FLIP;
        bool isStopLoop = false;
        while (true)
        {
            if (isNotTapFlip)
            {
                timerClockWise.fillAmount = timer / 10;
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    CloseFlipTiming();
                    timer = 0;

                    if (playerRobot != null)
                        playerRobot.Health = 0;

                    isStopLoop = true;

                    #region Firebase Event
                    string statusFlip = "Failed";
                    bool isTimeElapsed = true;
                    GameEventHandler.Invoke(LogFirebaseEventCode.FlipTiming, statusFlip, isTimeElapsed);
                    #endregion
                }
            }

            if (isStopLoop)
                yield break;

            yield return null;
        }
    }

    private IEnumerator HandleFlipTiming()
    {
        StopRun();
        HandleColorTextFlip();
        FlipForce flipForce = flipForceDic[MultiplierResult];
        GameEventHandler.Invoke(FlipTimingEvent.Flip, flipForce);
        TweenCallback tweenCallback = null;
        if (flipForce != FlipForce.High)
        {
            #region Design Event
            string status = $"Fail";
            GameEventHandler.Invoke(DesignEvent.FlipTiming, status);
            #endregion

            #region Firebase Event
            string statusFlip = "Failed";
            bool isTimeElapsed = false;
            GameEventHandler.Invoke(LogFirebaseEventCode.FlipTiming, statusFlip, isTimeElapsed);
            #endregion

            tweenCallback = () =>
            {
                isNotTapFlip = true;
            };
            playerCarphysic.FlipTiming(flipForce);
            ActionFlipText(tweenCallback, 0.5f, true);

            yield return new WaitForSeconds(0.5f);
            StartRun();
            yield break;
        }

        tweenCallback = () =>
        {
            if (DelayCheckBotImmobilizedCR != null)
                StopCoroutine(DelayCheckBotImmobilizedCR);
            DelayCheckBotImmobilizedCR = DelayCheckBotImmobilized();
            StartCoroutineIfActive(DelayCheckBotImmobilizedCR);

            #region Design Event
            string status = $"Complete";
            GameEventHandler.Invoke(DesignEvent.FlipTiming, status);
            #endregion

            #region Firebase Event
            string statusFlip = "Complete";
            GameEventHandler.Invoke(LogFirebaseEventCode.FlipTiming, statusFlip);
            #endregion

            //Hide();
            playerCarphysic.FlipTiming(flipForce);
            isNotTapFlip = true;

            CloseFlipTiming();
        };
        ActionFlipText(tweenCallback, 0.5f, false);
    }

    private void ActionFlipText(TweenCallback tweenCallback, float timeDuration, bool isShake)
    {
        if (isShake)
        {
            int vibrato = 30;
            flipText.transform.DOPunchPosition(Vector3.right * 8, timeDuration, vibrato);
        }

        flipText.transform.DOScale(Vector3.one * 1.2f, timeDuration);
        hightLightFlipImage.transform
            .DOScale(Vector3.one * 2, timeDuration)
            .OnComplete(tweenCallback);
    }

    private void HandleColorTextFlip()
    {
        Color colorText = MultiplierResult != flipHighestValue ? missZoneColor : flipZoneColor;
        flipText.color = colorText;
        hightLightFlipImage.color = colorText;
    }

    private void OpenFlipTiming()
    {
        #region Design Event
        string status = $"Start";
        GameEventHandler.Invoke(DesignEvent.FlipTiming, status);
        #endregion

        #region Firebase Event
        string statusFlip = "Start";
        GameEventHandler.Invoke(LogFirebaseEventCode.FlipTiming, statusFlip);
        #endregion

        tapToFlipText.gameObject.SetActive(true);

        HandleColorTextFlip();
        ResetTimer();
        StartRun();
        Show();
    }

    private void CloseFlipTiming()
    {
        if (HandleFlipTimingCR != null)
            StopCoroutine(HandleFlipTimingCR);

        StopTimer();
        StopRun();
        Hide();
    }

    private void BotModelSpawned(params object[] parameters)
    {
        if (parameters[0] is not PBRobot pbBot) return;
        if (pbBot.PersonalInfo.isLocal == false) return;
        if (parameters[1] is not GameObject chassis) return;

        isBotImmobilized = false;
        playerRobot = pbBot;
        CloseFlipTiming();
    }

    private void ResetTimer()
    {
        if (CountDownTimerFlipCR != null)
            StopCoroutine(CountDownTimerFlipCR);
        CountDownTimerFlipCR = CountDownTimer();
        StartCoroutineIfActive(CountDownTimerFlipCR);
    }

    Coroutine StartCoroutineIfActive(IEnumerator routine)
    {
        if (gameObject.activeInHierarchy)
            return StartCoroutine(routine);
        else
            return null;
    }


    private void StopTimer()
    {
        if (CountDownTimerFlipCR != null)
            StopCoroutine(CountDownTimerFlipCR);
    }

    private void BotImmobilized(params object[] parameters)
    {
        if (parameters[0] is not PBRobot robot) return;

        if (robot.name.Contains("Player") && !robot.IsDead && (robot.CombatEffectStatuses & CombatEffectStatuses.Stunned) == 0)
        {
            isBotImmobilized = true;
            OpenFlipTiming();
        }

        if (robot.IsDead)
            CloseFlipTiming();
    }

    private void BotRecovered()
    {
        isBotImmobilized = false;
        CloseFlipTiming();
    }

    private void OnRobotEffectApplied(object[] parameters)
    {
        if (parameters[0] is not PBRobot robot) return;
        if (parameters[1] is not CombatEffect combatEffect) return;
        if ((combatEffect.effectStatus & CombatEffectStatuses.Stunned) != 0)
        {
            OnRobotBeingStunned(parameters);
        }
    }

    private void OnRobotEffectRemoved(object[] parameters)
    {
        if (parameters[0] is not PBRobot robot) return;
        if (parameters[1] is not CombatEffect combatEffect) return;
        if ((combatEffect.effectStatus & CombatEffectStatuses.Stunned) != 0)
        {
            OnRobotRecoveredFromStun(parameters);
        }
    }

    private void OnRobotBeingStunned(object[] parameters)
    {
        if (parameters[0] is not PBRobot robot) return;
        BotRecovered();
    }

    private void OnRobotRecoveredFromStun(object[] parameters)
    {
        if (parameters[0] is not PBRobot robot) return;

        if (robot.PersonalInfo.isLocal && !robot.IsDead && robot.ChassisInstance.CarPhysics.IsImmobilized)
        {
            isBotImmobilized = true;
            OpenFlipTiming();
        }
    }

    private IEnumerator DelayCheckBotImmobilized()
    {
        yield return new WaitForSeconds(3);
        if (isBotImmobilized && isNotTapFlip)
            OpenFlipTiming();
    }

    private void SetupFollowingArena()
    {
        SegmentFlip segmentHandle = segmentFlip[currentHighestArenaVariable.value.index + 1];
        if (segmentHandle != null)
        {
            Segments = segmentHandle.SegmentInfo;
            RunSpeed = segmentHandle.RunSpeed;
            boardImage.sprite = segmentHandle.MultiplierSprite;
        }
    }

    private void Show() => mainCanvasGroup.Show();
    private void Hide() => mainCanvasGroup.Hide();
}
