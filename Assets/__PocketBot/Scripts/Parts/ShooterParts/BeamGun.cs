using System.Collections;
using System.Collections.Generic;
using LatteGames.Template;
using LatteGames.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;

public class BeamGun : ContinuousGunBase
{
    [Header("Gun Configs")]
    [SerializeField] float fireRate = 1;
    [SerializeField] float beamRadius = 0.5f;
    [Header("Lock On Configs")]
    [SerializeField] bool useLockOnFeature;
    [SerializeField, ShowIf("useLockOnFeature")] float lockOnDuration = 1f;
    [SerializeField, ShowIf("useLockOnFeature")] float warningDuration = 0.2f;
    [SerializeField, ShowIf("useLockOnFeature")] LineRenderer lineRenderer;
    [Header("Gun Refs")]
    [SerializeField] ParticleSystem muzzleVFX;
    [SerializeField] ParticleSystem collideVFX;
    [SerializeField] LineRenderer laserRenderer;

    protected RaycastHit[] raycastHits = new RaycastHit[1];
    protected Coroutine shootCoroutine;

    protected override void Awake()
    {
        base.Awake();
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
        if (laserRenderer != null)
        {
            laserRenderer.enabled = false;
        }
        if (muzzleVFX != null)
        {
            muzzleVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        if (collideVFX != null)
        {
            collideVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

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
        laserRenderer.enabled = false;
        lineRenderer.enabled = false;
        if (muzzleVFX != null)
        {
            muzzleVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        if (collideVFX != null)
        {
            collideVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    IEnumerator CR_Shoot()
    {
        while (true)
        {
            yield return Yielders.Get(reload);
            if (aimController == null || aimController.aimTarget == null || aimController.aimTarget.Robot.IsDead)
            {
                laserRenderer.enabled = false;
                lineRenderer.enabled = false;
                continue;
            }
            yield return new WaitUntil(() => CanShootTarget(aimController.aimTarget));
            if (useLockOnFeature)
            {
                lineRenderer.enabled = true;
                lineRenderer.startColor = Color.yellow;
                lineRenderer.endColor = Color.yellow;
                lineRenderer.SetPosition(0, Vector3.zero);
                var time = 0f;
                while (time <= lockOnDuration + warningDuration)
                {
                    if (aimController == null || aimController.aimTarget == null || aimController.aimTarget.Robot.IsDead)
                    {
                        break;
                    }
                    if (time < lockOnDuration && time + Time.fixedDeltaTime >= lockOnDuration)
                    {
                        lineRenderer.startColor = Color.red;
                        lineRenderer.endColor = Color.red;
                    }
                    time += Time.fixedDeltaTime;

                    var diff = aimController.aimTarget.transform.position - shootingPoint.position;
                    var distance = diff.magnitude;
                    int raycastHitsCount = Physics.SphereCastNonAlloc(shootingPoint.position, beamRadius, diff.normalized, raycastHits, distance, hitLayerMask);
                    if (raycastHitsCount > 0)
                    {
                        if (raycastHits[0].point == Vector3.zero)
                        {
                            lineRenderer.SetPosition(1, Vector3.zero);
                        }
                        else
                        {
                            lineRenderer.SetPosition(1, shootingPoint.InverseTransformPoint(raycastHits[0].point));
                        }
                    }
                    else
                    {
                        lineRenderer.SetPosition(1, shootingPoint.InverseTransformPoint(aimController.aimTarget.transform.position));
                    }
                    yield return Yielders.FixedUpdate;
                }
                lineRenderer.enabled = false;
            }
            if (muzzleVFX != null)
            {
                muzzleVFX.Play();
            }
            if (collideVFX != null)
            {
                collideVFX.Play();
            }
            PlayMuzzleSound();
            laserRenderer.enabled = true;
            laserRenderer.SetPosition(0, Vector3.zero);
            var t = 0f;
            var hitTimeCounter = 0f;
            while (t <= playDuration)
            {
                if (aimController == null || aimController.aimTarget == null || aimController.aimTarget.Robot.IsDead)
                {
                    break;
                }
                t += Time.fixedDeltaTime;
                hitTimeCounter += Time.fixedDeltaTime;
                bool canHit = false;
                if (hitTimeCounter >= secondsPerHit)
                {
                    hitTimeCounter = 0;
                    canHit = true;
                }

                var diff = aimController.aimTarget.transform.position - shootingPoint.position;
                var distance = diff.magnitude;
                int raycastHitsCount = Physics.SphereCastNonAlloc(shootingPoint.position, beamRadius, diff.normalized, raycastHits, distance, hitLayerMask);
                if (raycastHitsCount > 0)
                {
                    if (canHit && raycastHits[0].collider.TryGetComponent(out IDamagable damagable))
                    {
                        damagable.ReceiveDamage(PbPart, 0);
                    }
                    if (raycastHits[0].point == Vector3.zero)
                    {
                        laserRenderer.SetPosition(1, Vector3.zero);
                        collideVFX.transform.position = shootingPoint.position;
                    }
                    else
                    {
                        laserRenderer.SetPosition(1, shootingPoint.InverseTransformPoint(raycastHits[0].point));
                        collideVFX.transform.position = raycastHits[0].point;
                    }
                }
                else
                {
                    laserRenderer.SetPosition(1, shootingPoint.InverseTransformPoint(aimController.aimTarget.transform.position));
                    collideVFX.transform.position = aimController.aimTarget.transform.position;
                }
                yield return Yielders.FixedUpdate;
            }
            laserRenderer.enabled = false;
            if (muzzleVFX != null)
            {
                muzzleVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (collideVFX != null)
            {
                collideVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
