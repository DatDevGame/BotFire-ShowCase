using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class TaserCollider : PBCollider
{
    protected float lastInflictedDamageTime;
    protected override void TriggerStayBehaviour(Collider other)
    {
        if (partBehaviour == null) return;
        if (other.attachedRigidbody == null) return;
        if (other.attachedRigidbody.GetComponent<IDamagable>() == null) return;
        if (Time.time - lastInflictedDamageTime < Const.CollideValue.ContinuousDamageCooldown) return;

        var collisionInfo = new CollisionInfo
          (
                other.attachedRigidbody,
                Vector3.zero,
                other.ClosestPoint(transform.position)
            );

        lastInflictedDamageTime = Time.time;
        GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, partBehaviour.PbPart, collisionInfo, 0);
    }
}
