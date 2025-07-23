using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisarmEffect : CombatEffect
{
    public DisarmEffect(float duration, bool isAffectedByAbility, ICombatEntity sourceEntity, bool canStack = true) : base(duration, isAffectedByAbility, sourceEntity, canStack)
    {
    }

    public override CombatEffectStatuses effectStatus => CombatEffectStatuses.Disarmed;
    public override CombatEffectStatuses blockingEffectStatuses => CombatEffectStatuses.Invincible;

    public override void Apply()
    {
        base.Apply();
        affectedEntity.isDisarmed = true;
        affectedEntity.OnDisarmApplied();
        controller.Log($"{affectedEntity.name} is now no longer able to use any weapon for {remainingDuration} seconds.");
    }

    public override void Remove()
    {
        base.Remove();
        affectedEntity.isDisarmed = false;
        affectedEntity.OnDisarmRemoved();
        controller.Log($"{affectedEntity.name} is now able to use weapons.");
    }
}