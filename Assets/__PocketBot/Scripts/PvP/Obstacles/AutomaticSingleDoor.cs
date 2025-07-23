using UnityEngine;
using DG.Tweening;
using System.Collections;
using Sirenix.OdinInspector;

public class AutomaticSingleDoor : AutomaticDoorBase
{
    [SerializeField, BoxGroup("Ref")] protected Transform m_Door;

    private Vector3 m_DoorInitialPos;
    private Vector3 m_DoorOpenPos;

    protected override void InitializeDoors()
    {
        m_DoorInitialPos = m_Door.localPosition;
        m_DoorOpenPos = m_DoorInitialPos + new Vector3(6, 0, 0);
    }

    protected override IEnumerator OpenDoors(bool isNow = false)
    {
        Tween doorTween = m_Door.DOLocalMove(m_DoorOpenPos, m_OpenDuration).SetEase(Ease.InOutQuad);
        yield return doorTween.WaitForCompletion();
    }

    protected override IEnumerator CloseDoors(bool isNow = false)
    {
        Tween doorTween = m_Door.DOLocalMove(m_DoorInitialPos, m_CloseDuration).SetEase(Ease.InOutQuad);
        yield return doorTween.WaitForCompletion();
    }
}
