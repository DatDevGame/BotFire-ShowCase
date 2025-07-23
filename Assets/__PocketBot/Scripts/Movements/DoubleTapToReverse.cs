using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class DoubleTapToReverse : MonoBehaviour, IPointerDownHandler
{
    public Action OnStartReversing, OnEndReversing;

    [SerializeField] float reverseSpeed = 18;
    [SerializeField] float validDoubleTapTime = 0.5f;
    [SerializeField] float reverseDuration = 0.5f;
    [SerializeField] float cooldown = 0.5f;
    [SerializeField] float breakReverseThreshold = 0.2f;
    [SerializeField] PlayerController playerController;
    [SerializeField] Joystick joystick;
    float clickTimeStamp;
    float reverseTimeStamp;
    Coroutine reversingCR;
    CarPhysics player;
    bool isReversing;

    [SerializeField] private WheelTankHandle _wheelTankHandle;

    [SerializeField, BoxGroup("Data")] private Variable<int> countDoubleReverse;
    [SerializeField, BoxGroup("Data")] private Variable<Mode> m_CurrentMode;
    private void Awake()
    {
        countDoubleReverse.value = 0;
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, HandleBotModelSpawned);

    }

    private void OnDestroy()
    {
        countDoubleReverse.value = 0;
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, HandleBotModelSpawned);
    }

    public void CancelReversing()
    {
        if (isReversing)
        {
            if (reversingCR != null)
            {
                StopCoroutine(reversingCR);
            }
            EndReversing();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isReversing && Time.unscaledTime - reverseTimeStamp >= cooldown)
        {
            if (Time.unscaledTime - clickTimeStamp < validDoubleTapTime)
            {
                CancelReversing();
                reversingCR = StartCoroutine(CR_Reversing());
                reverseTimeStamp = Time.unscaledTime;
                clickTimeStamp = 0;
            }
            clickTimeStamp = Time.unscaledTime;
        }
    }

    private void Update()
    {
        if (isReversing && joystick.Direction.magnitude >= breakReverseThreshold)
        {
            CancelReversing();
        }
    }

    void EndReversing()
    {
        playerController.IsUsingBrake = false;
        player.CarTopSpeedMultiplier /= reverseSpeed;
        player.ResetAcceleration();
        if (_wheelTankHandle != null)
            _wheelTankHandle.StopWheel();
        isReversing = false;
        OnEndReversing?.Invoke();
    }

    IEnumerator CR_Reversing()
    {
        if (player == null) yield break;

        #region Count Reverse
        countDoubleReverse.value++;
        #endregion

        isReversing = true;
        player.CarTopSpeedMultiplier *= reverseSpeed;
        OnStartReversing?.Invoke();
        if (_wheelTankHandle != null)
            _wheelTankHandle.RunWheelBack(player.CarTopSpeedMultiplier);

        #region Firebase Event
        string matchType = m_CurrentMode.value switch
        {
            Mode.Normal => "Duel Match",
            Mode.Boss => "Boss Fight",
            Mode.Battle => "Battle Royale",
            _ => "null"
        };
        GameEventHandler.Invoke(LogFirebaseEventCode.BackwardsMove, matchType);
        #endregion

        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / reverseDuration;
            playerController.IsUsingBrake = true;
            player.AccelInput = -1;
            // if (reverseSpeed != player.CarTopSpeedMultiplier)
            // {
            //     originalPlayerSpeed = player.CarTopSpeedMultiplier;
            //     player.CarTopSpeedMultiplier = reverseSpeed;
            // }

            yield return null;
        }
        EndReversing();
    }

    void HandleBotModelSpawned(params object[] parameters)
    {
        if (parameters[0] is not PBRobot pbBot) return;
        if (pbBot.PersonalInfo.isLocal == false) return;
        if (parameters[1] is not GameObject chassis) return;

        player = chassis.GetComponent<CarPhysics>();

        var wheelTank = chassis.GetComponentInParent<WheelTankHandle>();
        if (wheelTank != null)
        {
            _wheelTankHandle = chassis.GetComponentInParent<WheelTankHandle>();
        }
    }
}
