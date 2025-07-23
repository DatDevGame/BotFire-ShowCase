using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvincibleEffect : CombatEffect
{
    public InvincibleEffect(float duration, bool isAffectedByAbility, ICombatEntity sourceEntity, bool canStack = true) : base(duration, isAffectedByAbility, sourceEntity, canStack)
    {
    }

    public override CombatEffectStatuses effectStatus => CombatEffectStatuses.Invincible;
    public override CombatEffectStatuses blockingEffectStatuses => CombatEffectStatuses.None;

    public override void Apply()
    {
        base.Apply();
        affectedEntity.isInvincible = true;
        controller.ClearAllEffects(e => (e.blockingEffectStatuses & CombatEffectStatuses.Invincible) != 0);
        controller.Log($"{affectedEntity.name} is now invincible for {remainingDuration} seconds.");
    }

    public override void Remove()
    {
        base.Remove();
        affectedEntity.isInvincible = false;
        controller.Log($"{affectedEntity.name} is no longer invincible.");
    }
}