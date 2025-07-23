using UnityEngine;
using Sirenix.OdinInspector;
using HyrphusQ.Events;
using System.Collections.Generic;
using System;

public class ContinuousCollider : PBCollider
{
    [SerializeField] protected float collideForce = 2;
    protected float lastInflictedDamageTime;

    protected virtual float DamageCooldown => Const.CollideValue.ContinuousDamageCooldown;
    protected float damageCoolDownHandle;

    protected override void CollisionStayBehaviour(Collision collision)
    {
        if (partBehaviour == null) return;
        if (collision.rigidbody == null) return;
        if (collision.rigidbody.GetComponent<IDamagable>() == null) return;

        if (damageCoolDownHandle <= 0)
            damageCoolDownHandle = DamageCooldown;

        if (Time.time - lastInflictedDamageTime < damageCoolDownHandle) return;

        //Match all condition
        lastInflictedDamageTime = Time.time;
        GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, partBehaviour.PbPart, collision, 0.1f, ApplyCollisionForceCallback);
    }

    protected virtual void Start()
    {
        ApplyCollisionForceCallback = HandleApplyCollisionForce;

    }

    protected bool isLeft;
    protected virtual void HandleApplyCollisionForce(CollisionData collisionData)
    {
        var rb = collisionData.chassisRb;
        rb.AddForceAtPosition(collideForce * collisionData.forceMultiplier * Vector3.up, rb.position + (isLeft ? -transform.right : transform.right), ForceMode.VelocityChange);
        isLeft = !isLeft;
    }
}
