using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Color enabledColor, disabledColor;
    [SerializeField] Joystick joystick;
    [SerializeField] HoldButton brakeButton;
    [SerializeField] CanvasGroupVisibility canvasGroupVisibility;
    [SerializeField] WheelTankHandle _wheelTankHandle;
    [SerializeField] Image[] joystickBGImages;

    bool isUsingBrake;
    float horizontal;
    float vertical;
    Vector3 joyStickdir;
    CarPhysics player;
    bool isActive = true;

    public bool IsUsingBrake { get => isUsingBrake; set => isUsingBrake = value; }

    private void Awake()
    {
        if (brakeButton != null)
        {
            brakeButton.OnButtonHold += HandleBrakeButtonHold;
            brakeButton.OnButtonRelease += HandleBrakeButtonRelease;
        }
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, HandleBotModelSpawned);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnResetBots, OnCarRecoveredFromImmobilized);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
        ObjectFindCache<PlayerController>.Add(this);
    }

    private void OnEnable()
    {
        foreach (var joystickBGImage in joystickBGImages)
        {
            joystickBGImage.color = enabledColor;
        }
    }

    private void OnDisable()
    {
        foreach (var joystickBGImage in joystickBGImages)
        {
            joystickBGImage.color = disabledColor;
        }
        joystick.OnPointerUp(null);
    }

    private void OnDestroy()
    {
        if (brakeButton != null)
        {
            brakeButton.OnButtonHold -= HandleBrakeButtonHold;
            brakeButton.OnButtonRelease -= HandleBrakeButtonRelease;
        }
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, HandleBotModelSpawned);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnResetBots, OnCarRecoveredFromImmobilized);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
        ObjectFindCache<PlayerController>.Remove(this);
    }

    void Update()
    {
        if (!isActive) return;
        if (player == null) return;
        horizontal = joystick.Horizontal;
        vertical = joystick.Vertical;
        joyStickdir = new Vector3(horizontal, 0, vertical);
        joyStickdir = MainCameraFindCache.Get().transform.TransformDirection(joyStickdir);
        joyStickdir = Vector3.ProjectOnPlane(joyStickdir, Vector3.up);

        Vector3 carRight = player.transform.right.normalized;

        if (IsUsingBrake == false) player.AccelInput = joyStickdir.normalized.magnitude;

        var steeringDir = Vector3.Dot(carRight, joyStickdir.normalized);
        player.RotationInput = steeringDir;
        player.InputDir = joyStickdir;

        if (_wheelTankHandle != null)
            _wheelTankHandle.RunWheelJoyStick(steeringDir);
    }

    private void OnFinalRoundCompleted()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    private void OnAnyPlayerDied(params object[] parameters)
    {
        if (parameters.Length <= 0) return;

        PBRobot pbRobot = (PBRobot)parameters[0];
        if (pbRobot != null)
        {
            if (pbRobot.PersonalInfo.isLocal)
                SetActive(false);
        }
    }
    void HandleBrakeButtonHold()
    {
        if (player == null) return;
        IsUsingBrake = true;
        player.AccelInput = -1;

        if (_wheelTankHandle != null)
            _wheelTankHandle.RunWheelBack();
    }

    void HandleBrakeButtonRelease()
    {
        if (player == null) return;
        IsUsingBrake = false;
        player.AccelInput = 0;

        if (_wheelTankHandle != null)
            _wheelTankHandle.StopWheel();
    }

    void HandleBotModelSpawned(params object[] parameters)
    {
        if (parameters[0] is not PBRobot pbBot) return;
        if (pbBot.PersonalInfo.isLocal == false) return;
        if (parameters[1] is not GameObject chassis) return;

        SetActive(true);
        gameObject.SetActive(true);
        if (player != null)
        {
            player.OnCarImmobilized -= OnCarImmobilized;
            player.OnCarRecoveredFromImmobilized -= OnCarRecoveredFromImmobilized;
        }

        player = chassis.GetComponent<CarPhysics>();

        var wheelTank = chassis.GetComponentInParent<WheelTankHandle>();
        if (wheelTank != null)
        {
            _wheelTankHandle = chassis.GetComponentInParent<WheelTankHandle>();
        }

        player.OnCarImmobilized += OnCarImmobilized;
        player.OnCarRecoveredFromImmobilized += OnCarRecoveredFromImmobilized;
    }

    void OnCarRecoveredFromImmobilized()
    {
        gameObject.SetActive(true);
    }

    void OnCarImmobilized()
    {
        gameObject.SetActive(false);
    }

    public void SetActive(bool isActive)
    {
        this.isActive = isActive;
        if (isActive)
        {
            canvasGroupVisibility.Show();
        }
        else
        {
            canvasGroupVisibility.Hide();
        }
    }
}
