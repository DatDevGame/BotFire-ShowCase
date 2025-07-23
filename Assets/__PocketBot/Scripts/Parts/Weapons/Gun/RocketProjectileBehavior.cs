using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketProjectileBehavior : TurretProjectileBehaviorBase
{
    protected override void ActionTriggerObject(Collider other)
    {
        base.ActionTriggerObject(other);
        Destroy(this.gameObject, 1);
    }
}
