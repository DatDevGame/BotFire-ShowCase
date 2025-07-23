using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NukeProjectileBehavior : TurretProjectileBehaviorBase
{
    [SerializeField] ParticleSystem explodeFX;

    protected override void ActionTriggerObject(Collider other)
    {
        base.ActionTriggerObject(other);

        var mySelfColliders = GetComponent<Collider>();
        mySelfColliders.enabled = false;

        var explosionVfx = Instantiate(explodeFX.gameObject).GetComponent<ParticleSystem>();
        explosionVfx.transform.position = transform.position;
        Destroy(explosionVfx.gameObject, 1);
        Destroy(gameObject);
        explodeFX.Play();
    }
}
