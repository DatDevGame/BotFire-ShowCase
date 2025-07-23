using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;

public class TurretProjectileBehaviorBase : PBCollider
{
    [SerializeField, LabelText("Damage")] protected float damage = 50f;
    [SerializeField, LabelText("Force Foward")] protected float forceFoward = 20f;
    [SerializeField, LabelText("Force Up")] protected float forceUp = 15;
    [SerializeField, LabelText("Gravity")] protected float gravityForce = 50f;
    [SerializeField, LabelText("Life Time")] protected float lifeTime = 5f;
    [SerializeField, LabelText("Rigidbody")] protected Rigidbody rigidbody;
    [SerializeField] protected float explodeRadius = 1.5f;
    [SerializeField] protected float explodeForce = 5f;
    [SerializeField] protected List<PBAxis> movingDir;
    [SerializeField] protected AnimationCurve animationCurve;

    [HideInInspector]
    public PartBehaviour PartBehaviour
    {
        set => partBehaviour = value;
    }

    private void Awake()
    {
        Destroy(gameObject, lifeTime);
    }

    public virtual void Fire(PartBehaviour partBehaviour, Vector3 point, float jumpPower, float timeDuration)
    {
        this.partBehaviour = partBehaviour;

        var mySelfColliders = GetComponents<Collider>();
        foreach (var collider in mySelfColliders)
            collider.enabled = true;

        transform
            .DOJump(point, jumpPower, 1, 0.3f)
            .SetEase(Ease.Linear);
    }

    protected virtual void ActionTriggerObject(Collider other) { }

    protected override void TriggerEnterBehaviour(Collider other)
    {
        if (other.isTrigger)
        {
            if (other.GetComponent<TurretProjectileBehaviorBase>() == null)
                return;
        }

        if (other.GetComponent<IBreakObstacle>() != null)
        {
            GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, partBehaviour.PbPart, other, explodeForce, ApplyCollisionForceCallback);
            ActionTriggerObject(other);
            StartCoroutine(CommonCoroutine.Delay(0, false, () => Destroy(gameObject)));
            return;
        }

        var mySelfColliders = GetComponents<Collider>();
        SoundManager.Instance.PlaySFX(SFX.MissileExplode, 0.2f);
        //foreach (var collider in mySelfColliders)
        //    collider.enabled = false;
        Collider[] colliders = Physics.OverlapSphere(transform.position, explodeRadius, -1 ^ (1 << gameObject.layer));
        foreach (var collider in colliders)
        {
            var damagable = collider.GetComponent<IDamagable>();
            if (damagable == null) continue;
            var closestPoint = collider.ClosestPoint(transform.position);
            var distance = Vector3.Distance(closestPoint, transform.position).Remap(0, explodeRadius, 0, 1);
            var collisionForce = Mathf.Lerp(explodeForce, 0, distance);
            var collisionInfo = new CollisionInfo
            (
                collider.GetComponent<Rigidbody>(),
                collider.transform.position - transform.position,
                closestPoint
            );
            GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, partBehaviour.PbPart, collisionInfo, collisionForce);
            break;
        }

        ActionTriggerObject(other);
    }
}
