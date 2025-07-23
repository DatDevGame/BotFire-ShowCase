using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class LazerCollider : PBCollider
{
    protected virtual float DamageCooldown => Const.CollideValue.ContinuousDamageCooldown;
    protected float lastInflictedDamageTime;
    protected override void TriggerStayBehaviour(Collider other)
    {
        if (partBehaviour == null) return;
        if (other.attachedRigidbody == null) return;
        if (other.attachedRigidbody.GetComponent<IDamagable>() == null) return;

        float damageCooldown = DamageCooldown;
        IBoostFireRate boostFireRate = partBehaviour.GetComponent<IBoostFireRate>();
        if (boostFireRate != null && boostFireRate.IsSpeedUp())
            damageCooldown = GetValueAtStack(boostFireRate.GetStackSpeedUp(), DamageCooldown);

        if (Time.time - lastInflictedDamageTime < damageCooldown) return;

        var collisionInfo = new CollisionInfo
          (
                other.attachedRigidbody,
                Vector3.zero,
                other.ClosestPoint(transform.position)
            );

        lastInflictedDamageTime = Time.time;
        GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, partBehaviour.PbPart, collisionInfo, 0);

    }
    private float GetValueAtStack(int stackIndex, float initialValue)
    {
        float currentValue = initialValue;
        for (int i = 0; i < stackIndex; i++)
            currentValue /= 2f;

        return currentValue;
    }
}
