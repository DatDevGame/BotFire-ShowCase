using System.Collections;
using System.Collections.Generic;
using LatteGames.Template;
using LatteGames.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

public class FlameGun : ContinuousGunBase
{
    [Header("Gun Configs")]
    [SerializeField] float fireRate = 1;
    [SerializeField] float throwAngle = 30;
    [SerializeField] ParticleSystem flameVFX;

    protected RaycastHit[] raycastHits = new RaycastHit[1];
    protected Collider[] colliders = new Collider[10];
    protected Coroutine shootCoroutine;
    protected HashSet<Rigidbody> damageRbHashSet = new HashSet<Rigidbody>();

    protected override void OnEnable()
    {
        base.OnEnable();
        if (shootCoroutine != null) StopCoroutine(shootCoroutine);
        shootCoroutine = StartCoroutine(CR_Shoot());
    }

    protected void OnDisable()
    {
        if (shootCoroutine != null) StopCoroutine(shootCoroutine);
        shootCoroutine = null;
        flameVFX.Stop();
    }

    IEnumerator CR_Shoot()
    {
        while (true)
        {
            yield return Yielders.Get(reload);
            if (aimController == null || aimController.aimTarget == null || aimController.aimTarget.Robot.IsDead)
            {
                flameVFX.Stop();
                continue;
            }
            yield return new WaitUntil(() => CanShootTarget(aimController.aimTarget));
            flameVFX.Play();
            PlayMuzzleSound();
            var t = 0f;
            var hitTimeCounter = 0f;
            damageRbHashSet.Clear();
            while (t <= playDuration)
            {
                if (aimController == null || aimController.aimTarget == null || aimController.aimTarget.Robot.IsDead)
                {
                    flameVFX.Stop();
                    break;
                }
                t += Time.fixedDeltaTime;
                hitTimeCounter += Time.fixedDeltaTime;
                if (hitTimeCounter >= secondsPerHit)
                {
                    hitTimeCounter = 0;
                    int colliderCount = Physics.OverlapSphereNonAlloc(shootingPoint.position, aimRange, colliders, hitLayerMaskPartOnly);
                    if (colliderCount > 0)
                    {
                        for (int i = 0; i < colliderCount; i++)
                        {
                            var col = colliders[i];
                            if (col == null || !col.TryGetComponent(out IDamagable damagable) || damageRbHashSet.Contains(col.attachedRigidbody))
                            {
                                continue;
                            }
                            Vector3 closest = col.ClosestPoint(shootingPoint.position);
                            Vector3 dir = closest - shootingPoint.position;
                            float distance = dir.magnitude;

                            if (distance == 0) continue;

                            dir.Normalize();
                            float dot = Vector3.Dot(transform.forward, dir);

                            if (dot >= Mathf.Cos(throwAngle * Mathf.Deg2Rad) && distance <= aimRange)
                            {
                                var hitCount = Physics.RaycastNonAlloc(shootingPoint.position, shootingPoint.forward, raycastHits, distance, hitLayerMaskWallOnly);
                                if (hitCount <= 0)
                                {
                                    damageRbHashSet.Add(col.attachedRigidbody);
                                    damagable.ReceiveDamage(PbPart, 0);
                                }
                            }
                        }
                    }
                }
                damageRbHashSet.Clear();
                yield return Yielders.FixedUpdate;
            }
            flameVFX.Stop();
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        if (shootingPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(shootingPoint.position, shootingPoint.position + Quaternion.AngleAxis(throwAngle / 2, Vector3.up) * shootingPoint.forward * aimRange);
            Gizmos.DrawLine(shootingPoint.position, shootingPoint.position + Quaternion.AngleAxis(-throwAngle / 2, Vector3.up) * shootingPoint.forward * aimRange);
        }
    }
}
