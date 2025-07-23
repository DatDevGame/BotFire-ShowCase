using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StraightBullet : BulletBase
{
    protected float movableDistance = 0;
    RaycastHit[] raycastHits = new RaycastHit[1];
    Vector3 previousPos = Vector3.zero;

    public override void Init(Action<BulletBase> returnPoolCallback, GunBase gun)
    {
        base.Init(returnPoolCallback, gun);
        movableDistance = gun.aimRange;
        previousPos = transform.position;
    }

    private void FixedUpdate()
    {
        transform.position += transform.forward * gun.bulletSpeed * Time.fixedDeltaTime;

        var diff = transform.position - previousPos;
        var distance = diff.magnitude;

        movableDistance -= distance;
        if (movableDistance <= 0)
        {
            returnPoolCallback?.Invoke(this);
        }

        if (distance > 0)
        {
            int raycastHitsCount = Physics.SphereCastNonAlloc(transform.position, radius, diff.normalized, raycastHits, distance, gun.hitLayerMask);
            if (raycastHitsCount > 0)
            {
                OnHit(raycastHits[0]);
            }
            previousPos = transform.position;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
