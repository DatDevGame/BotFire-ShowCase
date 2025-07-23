using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

public class AstroblastLazerMissile : MonoBehaviour
{
    [SerializeField]
    private Rigidbody missileRb;
    [SerializeField]
    private Collider missileCollider;
    [SerializeField]
    private Renderer[] missileRenderers;

    private Action<IDamagable> onObjectHit;
    private AstroblastLazerGunBehaviour laserGun;

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }
        IDamagable damagableObject;
        if (other.TryGetComponent(out damagableObject))
        {
            onObjectHit?.Invoke(damagableObject);
        }
        else if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out damagableObject))
        {
            onObjectHit?.Invoke(damagableObject);
        }
        missileCollider.enabled = false;
        missileRb.isKinematic = true;
        missileRenderers.ForEach(item => item.enabled = false);
        laserGun.Explosion.Explode(this, transform.position, other);
        // TODO: Consider using ObjectPool, and return to pool instead
        Destroy(gameObject, 1f);
    }

    public void Lanch(AstroblastLazerGunBehaviour laserGun, Action<IDamagable> onObjectHit = null)
    {
        this.laserGun = laserGun;
        this.onObjectHit = onObjectHit;
        missileCollider.enabled = true;
        missileRb.isKinematic = false;
        missileRb.AddForce(missileRb.transform.forward * laserGun.ConfigSO.MissileForceSpeed, ForceMode.Impulse);
        gameObject.SetLayer(laserGun.gameObject.layer, true);
        Destroy(gameObject, laserGun.ConfigSO.MissileLifetime);
    }
}