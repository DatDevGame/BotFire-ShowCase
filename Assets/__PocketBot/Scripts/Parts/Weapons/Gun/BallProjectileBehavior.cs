using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallProjectileBehavior : TurretProjectileBehaviorBase
{
    [SerializeField] ParticleSystem explodeFX;
    protected override void ActionTriggerObject(Collider other)
    {
        base.ActionTriggerObject(other);

        var explosionVfx = Instantiate(explodeFX.gameObject).GetComponent<ParticleSystem>();
        explosionVfx.transform.position = transform.position;
        Destroy(explosionVfx.gameObject, 1);
        explodeFX.Play();

        Destroy(this);
    }
}
