using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirborneEffect : CombatEffect<CombatEffectConfigSO>
{
    public AirborneEffect(float duration, bool resetVelocity, Vector3 force, ForceMode forceMode, Vector3 torque, ForceMode torqueMode, bool isAffectedByAbility, ICombatEntity sourceEntity) : base(duration, isAffectedByAbility, sourceEntity, false)
    {
        this.isExplosionForce = false;
        this.resetVelocity = resetVelocity;
        this.force = force;
        this.forceMode = forceMode;
        this.torque = torque;
        this.torqueMode = torqueMode;
    }

    public AirborneEffect(float duration, bool resetVelocity, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, ForceMode forceMode, bool isAffectedByAbility, ICombatEntity sourceEntity) : base(duration, isAffectedByAbility, sourceEntity, false)
    {
        this.isExplosionForce = true;
        this.resetVelocity = resetVelocity;
        this.explosionForce = explosionForce;
        this.explosionPosition = explosionPosition;
        this.explosionRadius = explosionRadius;
        this.upwardsModifier = upwardsModifier;
        this.forceMode = forceMode;
    }

    public override CombatEffectStatuses effectStatus => CombatEffectStatuses.Airborne;
    public override CombatEffectStatuses blockingEffectStatuses => CombatEffectStatuses.None;
    private bool resetVelocity { get; set; }
    private bool isExplosionForce { get; set; }
    private Vector3 force { get; set; }
    private ForceMode forceMode { get; set; }
    private Vector3 torque { get; set; }
    private ForceMode torqueMode { get; set; }
    private float explosionForce { get; set; }
    private Vector3 explosionPosition { get; set; }
    private float explosionRadius { get; set; }
    private float upwardsModifier { get; set; }

    public override void Apply()
    {
        base.Apply();
        if (resetVelocity)
        {
            affectedEntity.rigidbody.velocity = affectedEntity.rigidbody.angularVelocity = Vector3.zero;
        }
        if (isExplosionForce)
        {
            affectedEntity.rigidbody.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, upwardsModifier, forceMode);
        }
        else
        {
            affectedEntity.rigidbody.AddForce(force, forceMode);
            affectedEntity.rigidbody.AddTorque(torque, torqueMode);
        }
        affectedEntity.OnAirborneApplied();
        controller.Log($"{affectedEntity.name} is knocked airborne for {remainingDuration} seconds.");
    }

    public override void Remove()
    {
        base.Remove();
        affectedEntity.OnAirborneRemoved();
        controller.Log($"{affectedEntity.name} is no longer airborne");
    }
}