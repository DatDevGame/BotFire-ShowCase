using HyrphusQ.Events;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ShieldBehaviour : PartBehaviour
{
    [SerializeField] float minVelocity;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.TryGetComponent<IDamagable>(out var _) == false) return;
        float collidePower = collision.relativeVelocity.magnitude;
        if (collidePower < minVelocity) return;
        GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, PbPart, collision, 0);
    }
}
