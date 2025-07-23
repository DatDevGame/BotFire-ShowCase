using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;

public class BulletBase : MonoBehaviour
{
    [SerializeField] protected VFXReturnPool explodeFX;
    [SerializeField] public float radius = 0.5f;
    [SerializeField] protected bool isAoE;
    [SerializeField] bool isPlayHitSound = false;
    [SerializeField, ShowIf("isPlayHitSound")] SoundID hitSoundID;
    [SerializeField, ShowIf("isAoE")] protected float radiusAoE = 3f;

    protected RaycastHit[] raycastHits_AoE = new RaycastHit[10];
    protected Action<BulletBase> returnPoolCallback;
    protected GunBase gun;
    protected List<ParticleSystem> particleSystems;

    protected virtual void Awake()
    {
        particleSystems = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
    }

    public virtual void Init(Action<BulletBase> returnPoolCallback, GunBase gun)
    {
        this.gun = gun;
        this.returnPoolCallback = returnPoolCallback;
        foreach (var ps in particleSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }
    }

    public virtual void OnHit(RaycastHit hit)
    {
        if (isAoE)
        {
            int raycastHitsCount = Physics.SphereCastNonAlloc(transform.position, radiusAoE, Vector3.forward, raycastHits_AoE, radiusAoE, gun.hitLayerMask);
            for (int i = 0; i < raycastHitsCount; i++)
            {
                if (raycastHits_AoE[i].collider.TryGetComponent(out IDamagable damagable))
                {
                    damagable.ReceiveDamage(gun.PbPart, 0);
                }
            }
        }
        else
        {
            if (hit.collider.TryGetComponent(out IDamagable damagable))
            {
                damagable.ReceiveDamage(gun.PbPart, 0);
            }
        }
        var fx = BulletPoolManager.Instance.Get(explodeFX);
        if (hit.point != Vector3.zero)
        {
            fx.transform.position = hit.point;
        }
        else
        {
            fx.transform.position = transform.position;
        }
        fx.PlayThenReturn(explodeFX);
        if (isPlayHitSound)
            SoundManager.Instance.PlaySFX_3D_Pitch(hitSoundID, fx.transform.position, true);
        returnPoolCallback?.Invoke(this);
    }

    public virtual void OnOutOfLifetime()
    {
        if (isAoE)
        {
            int raycastHitsCount = Physics.SphereCastNonAlloc(transform.position, radiusAoE, Vector3.forward, raycastHits_AoE, radiusAoE, gun.hitLayerMask);
            for (int i = 0; i < raycastHitsCount; i++)
            {
                if (raycastHits_AoE[i].collider.TryGetComponent(out IDamagable damagable))
                {
                    damagable.ReceiveDamage(gun.PbPart, 0);
                }
            }
        }
        var fx = BulletPoolManager.Instance.Get(explodeFX);
        fx.transform.position = transform.position;
        fx.PlayThenReturn(explodeFX);
        if (isPlayHitSound)
            SoundManager.Instance.PlaySFX_3D_Pitch(hitSoundID, fx.transform.position, true);
        returnPoolCallback?.Invoke(this);
    }
}
