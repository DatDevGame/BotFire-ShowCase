using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Template;
using UnityEngine;

public class SpinnerImpactCollider : ImpactCollider
{
    [SerializeField] Rigidbody rb;
    [SerializeField] ParticleSystem sparkVFX;

    SpinnerBehaviour spinnerBehaviour => (SpinnerBehaviour)partBehaviour;

    protected override void CollisionEnterBehaviour(Collision collision)
    {
        if (collision.rigidbody == null) return;
        if (collision.rigidbody.GetComponent<IDamagable>() == null) return;
        if (!partBehaviour.IsAbleToDealDamage(collision, out var queryResult)) return;
        partBehaviour.PbPart.DamageMultiplier = queryResult.damageMultiplier;
        // Match all condition
        if (sparkVFX != null)
        {
            sparkVFX.transform.SetPositionAndRotation(collision.GetContact(0).point, Quaternion.Euler(-collision.relativeVelocity));
            sparkVFX.Play();
            SoundManager.Instance.PlayLoopSFX(SFX.SawHit, PBSoundUtility.IsOnSound() ? 0.8f : 0, true, true, gameObject);
        }
        if (impactVFX != null)
        {
            impactVFX.transform.position = collision.GetContact(0).point;
            impactVFX.Play();
        }
        float power = Mathf.Lerp(0, collisionForce, Mathf.Max(spinnerBehaviour.CalcForcePower(), 0.2f));
        if (spinnerBehaviour.IsEnabledManualForceApply())
            GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, partBehaviour.PbPart, collision, power, ApplyCollisionForceCallback);
        else
            GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, partBehaviour.PbPart, collision, power);
    }

    protected override void CollisionExitBehaviour(Collision collision)
    {
        base.CollisionExitBehaviour(collision);
        if (SoundManager.Instance != null) SoundManager.Instance.PauseLoopSFX(gameObject);
        if (sparkVFX != null) sparkVFX.Stop();
    }

    private void OnDestroy()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.StopLoopSFX(gameObject);
    }

    private void OnDisable()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.StopLoopSFX(gameObject);
    }
}
