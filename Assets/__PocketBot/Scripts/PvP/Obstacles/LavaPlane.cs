using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaPlane : MonoBehaviour, IAttackable
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamagable damagable))
        {
            if (other.attachedRigidbody.TryGetComponent(out CarPhysics carPhysics))
            {
                damagable.ReceiveDamage(this, 0);
            }
        }
    }

    public float GetDamage()
    {
        return 999999f;
    }
}