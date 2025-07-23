using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using HyrphusQ.Events;
using LatteGames.Template;
using Sirenix.OdinInspector;

public class CameraShake : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera vCam;
    [SerializeField] float minForceToShakeCamera = 30;
    [SerializeField] float shakeCameraTime;

    bool isAbleToShakeCam = true;
    Coroutine shakeCameraCoroutine;
    public bool IsAbleToShakeCam
    {
        get => isAbleToShakeCam;
        set
        {
            isAbleToShakeCam = value;
            if (!isAbleToShakeCam)
            {
                StopShakeCamera();
                var noise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                noise.m_FrequencyGain = 0;
            }
        }
    }

    private void Awake()
    {
        ObjectFindCache<CameraShake>.Add(this);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnRobotReceiveForce, HandleBotReceiveForce);
    }

    private void OnDestroy()
    {
        ObjectFindCache<CameraShake>.Remove(this);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnRobotReceiveForce, HandleBotReceiveForce);
    }

    void HandleBotReceiveForce(object[] parameters)
    {
        if (parameters[0] is not PBChassis receiverChassis) return;
        if (parameters[1] is not PBChassis attackerChassis) return;
        if (parameters[2] is not float force) return;
        var receiverRobot = receiverChassis.Robot;
        var attackerRobot = attackerChassis.Robot;
        if (!receiverRobot.PlayerInfoVariable.value.isLocal && !attackerRobot.PlayerInfoVariable.value.isLocal)
            return;
        if (force >= minForceToShakeCamera)
        {
            ShakeCamera(parameters[0], parameters[1], parameters[2], false);
        }
    }

    IEnumerator CR_ShakeCamera()
    {
        StartCoroutine(ShakeHandle());
        yield return null;
    }
    IEnumerator CR_ShakeCamera(float frequencyGainValue)
    {
        StartCoroutine(ShakeHandle(frequencyGainValue));
        yield return null;
    }

    private IEnumerator ShakeHandle(float frequencyGainValue = 1)
    {
        var noise = vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_FrequencyGain = frequencyGainValue;
        yield return new WaitForSeconds(shakeCameraTime);
        noise.m_FrequencyGain = 0;
        yield return new WaitForSeconds(5f);
        shakeCameraCoroutine = null;
    }

    void ShakeCamera(params object[] parameters)
    {
        if (!IsAbleToShakeCam) return;
        if (shakeCameraCoroutine != null) return;
        shakeCameraCoroutine = StartCoroutine(CR_ShakeCamera());
        SoundManager.Instance.PlaySFX(SFX.CrowdCheer);
        GameEventHandler.Invoke(PBPvPEventCode.OnShakeCamera, parameters);
    }

    public void StopShakeCamera()
    {
        if (shakeCameraCoroutine != null)
        {
            StopCoroutine(shakeCameraCoroutine);
            shakeCameraCoroutine = null;
        }
    }

    public void ShakeCameraIgnoreCondition(params object[] parameters)
    {
        if (parameters[0] is not PBChassis receiverChassis) return;
        if (parameters[1] is not PBChassis attackerChassis) return;
        if (!receiverChassis.Robot.IsDead && !attackerChassis.Robot.IsDead)
            return;
        StopShakeCamera();
        ShakeCamera(parameters);
    }

    public void ExplosionShake()
    {
        StopShakeCamera();
        StartCoroutine(CR_ShakeCamera(0.2f));
    }
}