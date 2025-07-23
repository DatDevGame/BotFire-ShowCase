using UnityEngine;
using HyrphusQ.Events;
using LatteGames.Template;

public class ImpactCollider : PBCollider
{
    [SerializeField] protected float collisionForce = 5;
    [SerializeField] protected ParticleSystem impactVFX;

    protected override void CollisionEnterBehaviour(Collision collision)
    {
        if (collision == null) return;

        if (collision.collider.CompareTag("Ground") || partBehaviour == null) return;
        if (collision.rigidbody == null) return;
        if (collision.rigidbody.GetComponent<IDamagable>() == null) return;
        if (!partBehaviour.IsAbleToDealDamage(collision, out var queryResult)) return;
        partBehaviour.PbPart.DamageMultiplier = queryResult.damageMultiplier;

        // Match all condition
        if (impactVFX != null)
        {
            impactVFX.transform.position = collision.GetContact(0).point;
            impactVFX.Play();
        }
        SoundManager.Instance.PlaySFX(SFX.BladeHit, PBSoundUtility.IsOnSound() ? 1 : 0);
        GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, partBehaviour.PbPart, collision, collisionForce, ApplyCollisionForceCallback);
    }
}