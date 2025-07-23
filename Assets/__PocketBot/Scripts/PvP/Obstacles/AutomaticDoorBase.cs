using UnityEngine;
using DG.Tweening;
using System.Collections;
using Sirenix.OdinInspector;

public abstract class AutomaticDoorBase : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] protected float m_StartDelay;
    [SerializeField, BoxGroup("Config")] protected float m_MaxOffetDoor;
    [SerializeField, BoxGroup("Config")] protected float m_OpenCooldown = 2f;
    [SerializeField, BoxGroup("Config")] protected float m_OpenDuration = 3f;
    [SerializeField, BoxGroup("Config")] protected float m_CloseCooldown = 1f;
    [SerializeField, BoxGroup("Config")] protected float m_CloseDuration = 1.5f;
    [SerializeField, BoxGroup("Config")] protected LayerMask obstacleLayer;

    [SerializeField, BoxGroup("Ref")] protected Transform m_LeftDoor;
    [SerializeField, BoxGroup("Ref")] protected Transform m_RightDoor;
    [SerializeField, BoxGroup("Ref")] protected ColliderObstacleDoor m_ColliderObstacleDoorLeft;
    [SerializeField, BoxGroup("Ref")] protected ColliderObstacleDoor m_ColliderObstacleDoorRight;

    protected Vector3 m_LeftDoorInitialPos;
    protected Vector3 m_RightDoorInitialPos;
    protected Vector3 m_LeftDoorOpenPos;
    protected Vector3 m_RightDoorOpenPos;
    protected Tween m_LeftDoorTween;
    protected Tween m_RightDoorTween;
    protected bool m_IsRunning = false;

    protected enum DoorState { Closed, Opening, OpenCooldown, Closing, CloseCooldown }
    protected DoorState m_CurrentState;
    protected IEnumerator m_DoorCycleCR;
    protected abstract void InitializeDoors();
    protected abstract IEnumerator OpenDoors(bool isNow = false);
    protected abstract IEnumerator CloseDoors(bool isNow = false);

    protected virtual IEnumerator DoorCycle()
    {
        yield return new WaitForSeconds(m_StartDelay);
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

    protected virtual IEnumerator Start()
    {
        yield return new WaitForSeconds(m_StartDelay);
        InitializeDoors();
        m_CurrentState = DoorState.Closed;
        m_DoorCycleCR = DoorCycle();
        StartCoroutine(m_DoorCycleCR);
    }
}
