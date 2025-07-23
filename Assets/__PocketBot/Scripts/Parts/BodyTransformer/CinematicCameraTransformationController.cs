using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class CinematicCameraTransformationController : MonoBehaviour
{
    [SerializeField]
    private CinemachineBrain m_CinemachineBrain;
    [SerializeField]
    private CinemachineVirtualCamera m_VirtualCamera;
    [SerializeField]
    private CinemachineVirtualCamera m_FollowCamera;
    [SerializeField]
    private BodyTransformerConfigSO m_ConfigSO;
    [SerializeField]
    private Vector3 m_FollowOffset = new Vector3(-8.500694f, 7.625252f);

    private bool m_IsLookAtPlayer;
    private int m_CinematicCameraFocusTimes;
    private Quaternion m_MainCamRotation;
    private PlayerController m_PlayerController;
    private HealthBarSpawner m_HealthBarSpawner;
    private Camera m_MainCamera;

    private void Start()
    {
        m_MainCamera = MainCameraFindCache.Get();
        m_CinematicCameraFocusTimes = m_ConfigSO.maxCinematicCameraFocusTimes;
        m_VirtualCamera.enabled = false;
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStart);
        GameEventHandler.AddActionEvent(BodyTransformerEventCode.OnTransformationStarted, OnTransformationStarted);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStart);
        GameEventHandler.RemoveActionEvent(BodyTransformerEventCode.OnTransformationStarted, OnTransformationStarted);
    }

    private void LateUpdate()
    {
        if (m_IsLookAtPlayer)
        {
            m_MainCamera.transform.rotation = m_MainCamRotation;
        }
    }

    private void OnLevelStart()
    {
        m_PlayerController = FindObjectOfType<PlayerController>();
        m_HealthBarSpawner = FindObjectOfType<HealthBarSpawner>();
    }

    private void OnTransformationStarted(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        BodyTransformer bodyTransformer = parameters[0] as BodyTransformer;
        float duration = (float)parameters[1];
        if (!bodyTransformer.robot.PersonalInfo.isLocal || bodyTransformer.robot.IsPreview || duration <= 0f || bodyTransformer.currentState != BodyTransformer.State.Attack || m_CinematicCameraFocusTimes-- <= 0)
            return;
        StartCoroutine(FocusOnTransformation_CR(bodyTransformer, duration));
    }

    private IEnumerator FocusOnTransformation_CR(BodyTransformer bodyTransformer, float duration)
    {
        m_PlayerController.gameObject.SetActive(false);
        m_HealthBarSpawner.gameObject.SetActive(false);
        StartCoroutine(CommonCoroutine.Delay(duration + AnimationDuration.SHORT, true, () =>
        {
            m_PlayerController.gameObject.SetActive(true);
            m_HealthBarSpawner.gameObject.SetActive(true);
        }));
        yield return new WaitForEndOfFrame();
        Transform followTransform = bodyTransformer.robot.ChassisInstance.RobotBaseBody.transform;
        CinemachineBlendDefinition previousBlendDef = m_CinemachineBrain.m_DefaultBlend;
        m_CinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.Cut, 0f);
        m_VirtualCamera.enabled = true;
        m_VirtualCamera.Follow = followTransform;
        yield return LerpUnscale_CR(duration + AnimationDuration.SSHORT, t =>
        {
            float angle = t * 180f;
            Vector3 offset = Quaternion.Euler(Vector3.up * angle) * m_FollowOffset;
            m_VirtualCamera.transform.position = followTransform.TransformPoint(offset);
            m_VirtualCamera.transform.LookAt(followTransform);
        });
        m_IsLookAtPlayer = true;
        m_CinemachineBrain.m_DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Style.EaseInOut, 0.5f);
        m_VirtualCamera.enabled = false;
        yield return LerpUnscale_CR(m_CinemachineBrain.m_DefaultBlend.m_Time, t =>
        {
            m_MainCamRotation = Quaternion.LookRotation(m_VirtualCamera.Follow.position - m_MainCamera.transform.position);
        });
        Quaternion originRotation = m_MainCamRotation;
        Quaternion destinationRotation = m_FollowCamera.transform.rotation;
        m_CinemachineBrain.m_DefaultBlend = previousBlendDef;
        yield return LerpUnscale_CR(AnimationDuration.SSHORT, t =>
        {
            m_MainCamRotation = Quaternion.Slerp(originRotation, destinationRotation, t);
        });
        m_IsLookAtPlayer = false;
    }

    private IEnumerator LerpUnscale_CR(float duration, Action<float> callback)
    {
        float t = 0.0f;
        callback(t / duration);
        while (t < duration)
        {
            yield return null;
            t += Time.unscaledDeltaTime;
            callback(t / duration);
        }
        callback(1);
    }
}