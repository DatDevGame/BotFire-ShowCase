using HyrphusQ.Events;
using LatteGames;
using LatteGames.Template;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PendulumHammerImpactCollider : ImpactCollider
{
    [SerializeField] protected float m_Interval = 1;
    protected HashSet<int> m_ColliderIdHashSet;

    private void Awake()
    {
        m_ColliderIdHashSet = new HashSet<int>();
    }

    protected override void CollisionEnterBehaviour(Collision collision)
    {
        if (collision == null) return;

        if (collision.collider.CompareTag("Ground") || partBehaviour == null) return;
        if (collision.rigidbody == null) return;
        if (collision.rigidbody.GetComponent<IDamagable>() == null) return;
        if (!partBehaviour.IsAbleToDealDamage(collision, out var queryResult)) return;

        //Interval Deal Dame Handler
        bool isInterval = true;
        var part = collision.gameObject.GetComponent<PBPart>();
        if (part == null || part.RobotChassis == null || part.RobotChassis.IsGripped)
            return;
        var chassis = part.RobotChassis;
        if (chassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID != default)
        {
            int chassisID = chassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID;
            if (m_ColliderIdHashSet.Contains(chassisID))
            {
                isInterval = false;
            }
            else
            {
                isInterval = true;
                m_ColliderIdHashSet.Add(chassisID);
                StartCoroutine(CommonCoroutine.Delay(m_Interval, false, () => 
                {
                    m_ColliderIdHashSet.Remove(chassisID);
                }));
            }
        }
        if (!isInterval)
            return;

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
