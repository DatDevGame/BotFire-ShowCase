using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public struct DoorTransformConfig
{
    [BoxGroup("Position")] public Vector3 OpenPosition;
    [BoxGroup("Position")] public Vector3 ClosePosition;

    [BoxGroup("Scale")] public Vector3 OpenScale;
    [BoxGroup("Scale")] public Vector3 CloseScale;
}
public class TwoWingDoorObstacleResponsive : AutomaticDoorBase
{
    [SerializeField, BoxGroup("Ref")] protected Transform m_RootLeft;
    [SerializeField, BoxGroup("Ref")] protected Transform m_RootRight;

    [SerializeField, BoxGroup("Config")] protected float m_DoorOpenDurationOnObstacle;
    [SerializeField, BoxGroup("Config")] protected bool m_IsOpenTheFirstTime;
    [SerializeField, BoxGroup("Transform Config")] protected DoorTransformConfig m_DoorTransformConfig;
    [SerializeField, BoxGroup("Test")] protected DoorState DoorStateTest;

    protected Sequence m_LeftDoorSequence;
    protected Sequence m_RightDoorSequence;
    protected IEnumerator m_DoorOpenDurationOnObstacleCR;

    protected override IEnumerator Start()
    {
        m_RootLeft.position = m_ColliderObstacleDoorLeft.TriggerPoint.position;
        m_RootRight.position = m_ColliderObstacleDoorRight.TriggerPoint.position;

        if (m_IsOpenTheFirstTime)
            StartCoroutine(OpenDoors(true));
        else
            StartCoroutine(CloseDoors(true));

        yield return new WaitForSeconds(m_StartDelay);
        InitializeDoors();

        m_CurrentState = m_IsOpenTheFirstTime ? DoorState.Opening : DoorState.Closed;
        m_DoorCycleCR = DoorCycle();
        StartCoroutine(m_DoorCycleCR);
        m_IsRunning = true;
    }
    protected virtual void Update()
    {
        if (IsObstacleDetected() && m_IsRunning && m_CurrentState == DoorState.Closing)
        {
            m_IsRunning = false;
            if (m_DoorOpenDurationOnObstacleCR != null)
                StopCoroutine(m_DoorOpenDurationOnObstacleCR);
            m_DoorOpenDurationOnObstacleCR = DoorOpenDurationOnObstacle();
            StartCoroutine(m_DoorOpenDurationOnObstacleCR);
        }
    }

    protected override IEnumerator DoorCycle()
    {
        while (true)
        {
            switch (m_CurrentState)
            {
                case DoorState.Closed:
                    yield return new WaitForSeconds(m_CloseCooldown);
                    m_CurrentState = DoorState.Opening;
                    break;

                case DoorState.Opening:
                    yield return OpenDoors();
                    m_CurrentState = DoorState.OpenCooldown;
                    break;

                case DoorState.OpenCooldown:
                    yield return new WaitForSeconds(m_OpenCooldown);
                    m_CurrentState = DoorState.Closing;
                    break;

                case DoorState.Closing:
                    yield return CloseDoors();
                    m_CurrentState = DoorState.CloseCooldown;
                    break;

                case DoorState.CloseCooldown:
                    yield return new WaitForSeconds(m_CloseCooldown);
                    m_CurrentState = DoorState.Closed;
                    break;
            }
        }
    }

    protected virtual bool IsObstacleDetected()
    {
        return m_ColliderObstacleDoorLeft.IsObstacleDected && m_ColliderObstacleDoorRight.IsObstacleDected;
    }


    protected override void InitializeDoors()
    {
        m_LeftDoorInitialPos = m_LeftDoor.localPosition;
        m_RightDoorInitialPos = m_RightDoor.localPosition;
    }

    protected override IEnumerator OpenDoors(bool isNow = false)
    {
        SetupDoorSequences(
            m_DoorTransformConfig.OpenPosition,
            m_DoorTransformConfig.OpenScale,
            isNow ? 0 : m_OpenDuration,
            Ease.InCubic
        );

        yield return PlayDoorSequences();
        m_IsRunning = true;
    }

    protected override IEnumerator CloseDoors(bool isNow = false)
    {
        SetupDoorSequences(
            m_DoorTransformConfig.ClosePosition,
            m_DoorTransformConfig.CloseScale,
            isNow ? 0 : m_CloseDuration,
            Ease.OutCubic
        );

        yield return PlayDoorSequences();
    }

    private void SetupDoorSequences(Vector3 position, Vector3 scale, float duration, Ease ease)
    {
        m_LeftDoorSequence?.Kill();
        m_LeftDoorSequence = DOTween.Sequence();

        m_RightDoorSequence?.Kill();
        m_RightDoorSequence = DOTween.Sequence();

        m_LeftDoorSequence
            .Append(m_LeftDoor.DOLocalMove(position, duration).SetEase(ease))
            .Join(m_LeftDoor.DOScale(scale, duration).SetEase(ease))
            .OnUpdate(() => 
            {
                m_RootLeft.position = new Vector3(m_LeftDoor.position.x - m_LeftDoor.localScale.x / 2, m_LeftDoor.position.y, m_LeftDoor.position.z);
            });

        m_RightDoorSequence
            .Append(m_RightDoor.DOLocalMove(-position, duration).SetEase(ease))
            .Join(m_RightDoor.DOScale(scale, duration).SetEase(ease)
            .OnUpdate(() => 
            {
                m_RootRight.position = new Vector3(m_RightDoor.position.x + m_RightDoor.localScale.x / 2, m_RightDoor.position.y, m_RightDoor.position.z);
            }));
    }

    private IEnumerator PlayDoorSequences()
    {
        m_LeftDoorSequence.Play();
        m_RightDoorSequence.Play();

        yield return m_LeftDoorSequence.WaitForCompletion();
        yield return m_RightDoorSequence.WaitForCompletion();
    }


    protected virtual IEnumerator DoorOpenDurationOnObstacle()
    {
        if (m_LeftDoorSequence != null)
            m_LeftDoorSequence?.Kill();
        if (m_RightDoorSequence != null)
            m_RightDoorSequence?.Kill();
        if (m_DoorCycleCR != null)
            StopCoroutine(m_DoorCycleCR);

        yield return new WaitForSeconds(m_DoorOpenDurationOnObstacle);
        m_CurrentState = DoorState.Opening;
        m_DoorCycleCR = DoorCycle();
        StartCoroutine(m_DoorCycleCR);
    }
}
