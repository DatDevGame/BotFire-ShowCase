using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingBullet : BulletBase
{
    [SerializeField] protected float movableDistanceMultiplier = 1.25f;
    [SerializeField] protected float homingRadius = 3f;
    [SerializeField] protected float homingConeAngle = 180f;
    [SerializeField] protected float rotateSpeed = 180f;

    protected AimController homingTarget;
    protected float movableDistance = 0;
    RaycastHit[] raycastHits = new RaycastHit[1];
    Vector3 previousPos = Vector3.zero;

    public override void Init(Action<BulletBase> returnPoolCallback, GunBase gun)
    {
        base.Init(returnPoolCallback, gun);
        movableDistance = gun.aimRange * movableDistanceMultiplier;
        previousPos = transform.position;
        homingTarget = null;
    }

    private void FixedUpdate()
    {
        if (homingTarget == null || homingTarget.Robot.Health <= 0)
        {
            homingTarget = AimController.FindATargetInRange(transform.position, homingRadius, gun.AimController.Robot.TeamId);
            if (homingTarget != null)
            {
                var targetDirection = homingTarget.transform.position - transform.position;
                if (Vector3.Angle(transform.forward, targetDirection) > homingConeAngle / 2)
                {
                    homingTarget = null;
                }
            }
        }
        if (homingTarget != null)
        {
            var targetDirection = homingTarget.transform.position - transform.position;
            var rotationStep = rotateSpeed * Time.fixedDeltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetDirection), rotationStep);
        }

        transform.position += transform.forward * gun.bulletSpeed * Time.fixedDeltaTime;

        var diff = transform.position - previousPos;
        var distance = diff.magnitude;

        movableDistance -= distance;
        if (movableDistance <= 0)
        {
            OnOutOfLifetime();
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, homingRadius);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(homingConeAngle / 2, Vector3.up) * transform.forward * homingRadius);
        Gizmos.DrawLine(transform.position, transform.position + Quaternion.AngleAxis(-homingConeAngle / 2, Vector3.up) * transform.forward * homingRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radiusAoE);
    }
}
