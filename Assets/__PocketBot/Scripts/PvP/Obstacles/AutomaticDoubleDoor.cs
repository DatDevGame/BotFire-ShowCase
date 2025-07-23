using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class AutomaticDoubleDoor : AutomaticDoorBase
{
    protected IEnumerator m_CloseDoorSlowCR;
    protected override IEnumerator Start()
    {
        m_IsRunning = true;
        yield return new WaitForSeconds(m_StartDelay);
        InitializeDoors();
        m_CurrentState = DoorState.Closed;
        m_DoorCycleCR = DoorCycle();
        StartCoroutine(m_DoorCycleCR);
    }
    protected virtual void Update()
    {
        if (IsObstacleDetected() && m_IsRunning)
        {
            m_IsRunning = false;
            if (m_CloseDoorSlowCR != null)
                StopCoroutine(m_CloseDoorSlowCR);
            m_CloseDoorSlowCR = CloseDoorsSlow();
            StartCoroutine(m_CloseDoorSlowCR);
        }
        else if(!IsObstacleDetected() && !m_IsRunning)
        {
            if (m_CloseDoorSlowCR != null)
                StopCoroutine(m_CloseDoorSlowCR);

            m_CurrentState = DoorState.Closing;
            if (m_DoorCycleCR != null)
                StopCoroutine(m_DoorCycleCR);
            m_DoorCycleCR = DoorCycle();
            StartCoroutine(m_DoorCycleCR);
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
        m_LeftDoorOpenPos = m_LeftDoorInitialPos + new Vector3(m_MaxOffetDoor, 0f, 0f);
        m_RightDoorOpenPos = m_RightDoorInitialPos + new Vector3(-m_MaxOffetDoor, 0f, 0f);
    }

    protected override IEnumerator OpenDoors(bool isNow = false)
    {
        m_LeftDoorTween.Kill();
        m_RightDoorTween.Kill();

        m_LeftDoorTween = m_LeftDoor.DOLocalMove(m_LeftDoorOpenPos, m_OpenDuration).SetEase(Ease.InOutQuad);
        m_RightDoorTween = m_RightDoor.DOLocalMove(m_RightDoorOpenPos, m_OpenDuration).SetEase(Ease.InOutQuad);
        yield return m_LeftDoorTween.WaitForCompletion();
        yield return m_RightDoorTween.WaitForCompletion();
        m_IsRunning = true;
    }

    protected override IEnumerator CloseDoors(bool isNow = false)
    {
        m_LeftDoorTween.Kill();
        m_RightDoorTween.Kill();

        m_LeftDoorTween = m_LeftDoor
            .DOLocalMove(m_LeftDoorInitialPos, m_CloseDuration)
            .SetEase(Ease.InOutQuad);

        m_RightDoorTween = m_RightDoor
            .DOLocalMove(m_RightDoorInitialPos, m_CloseDuration)
            .SetEase(Ease.InOutQuad);

        yield return m_LeftDoorTween.WaitForCompletion();
        yield return m_RightDoorTween.WaitForCompletion();
    }
    protected virtual IEnumerator CloseDoorsSlow()
    {
        if (m_DoorCycleCR != null)
            StopCoroutine(m_DoorCycleCR);

        m_LeftDoorTween.Kill();
        m_RightDoorTween.Kill();

        m_LeftDoorTween = m_LeftDoor
            .DOLocalMove(m_LeftDoorInitialPos, m_CloseDuration * 5)
            .SetEase(Ease.InOutQuad);

        m_RightDoorTween = m_RightDoor
            .DOLocalMove(m_RightDoorInitialPos, m_CloseDuration * 5)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                if (m_CloseDoorSlowCR != null)
                    StopCoroutine(m_CloseDoorSlowCR);

                m_CurrentState = DoorState.Closing;
                if (m_DoorCycleCR != null)
                    StopCoroutine(m_DoorCycleCR);
                StartCoroutine(m_DoorCycleCR);
            });

        yield return m_LeftDoorTween.WaitForCompletion();
        yield return m_RightDoorTween.WaitForCompletion();
    }
}
