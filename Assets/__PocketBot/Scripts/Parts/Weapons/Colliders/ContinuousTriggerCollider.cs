using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class ContinuousTriggerCollider : ContinuousCollider
{
    public bool CanInflictDamage { get; set; }

    protected override void CollisionStayBehaviour(Collision collision)
    {
        //Do nothing
    }

    protected override void TriggerStayBehaviour(Collider other)
    {
        if (other == null) return;

        if (CanInflictDamage == false) return;
        if (partBehaviour == null) return;
        if (other.attachedRigidbody == null) return;
        if (other.attachedRigidbody.GetComponent<IDamagable>() == null) return;
        if (other.attachedRigidbody.TryGetComponent(out PBPart part) && part == partBehaviour.PbPart) return; //self collided

        float damageCooldown = DamageCooldown;
        IBoostFireRate boostFireRate = partBehaviour.GetComponent<IBoostFireRate>();
        if (boostFireRate != null && boostFireRate.IsSpeedUp())
            damageCooldown = GetValueAtStack(boostFireRate.GetStackSpeedUp(), DamageCooldown);

        if (Time.time - lastInflictedDamageTime < damageCooldown) return;

        //Match all condition
        lastInflictedDamageTime = Time.time;
        var collisionInfo = new CollisionInfo
        (
            other.attachedRigidbody,
            Vector3.zero,
            other.ClosestPoint(transform.position)
        );
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
