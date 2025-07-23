using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using HyrphusQ.Helpers;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RocketArtilleryBullet : MonoBehaviour
{
    public class HomingPositionGenerator : MonoBehaviour
    {
        [ShowInInspector]
        private bool isDrawGizmos = true;
        [ShowInInspector]
        private List<Vector3> randomLocalPoints;

        [ShowInInspector]
        private bool IsInitialized => randomLocalPoints != null;

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!isDrawGizmos || !IsInitialized)
                return;
            for (int i = 0; i < randomLocalPoints.Count; i++)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.TransformPoint(randomLocalPoints[i]), 0.25f);
                Handles.Label(transform.TransformPoint(randomLocalPoints[i]), i.ToString());
            }
        }
#endif

        private void Initialize(float minDistance = 0.8f, int count = 10)
        {
            if (transform == null || IsInitialized)
                return;
            randomLocalPoints = new List<Vector3>();
            Rigidbody attachedRb = GetComponent<Rigidbody>();
            BoxCollider[] colliders = GetComponentsInChildren<BoxCollider>().Where(collider => !collider.isTrigger && collider.attachedRigidbody == attachedRb).ToArray();
            int maxAttempt = 100;
            int i = -1;
            do
            {
                i++;
                var collider = colliders[i % colliders.Length];
                var localBounds = new Bounds(collider.center, collider.size);
                var point = RandonPositionByBounds(localBounds, point =>
                {
                    var localPoint = transform.InverseTransformPoint(collider.transform.TransformPoint(point));
                    if (randomLocalPoints.Any(item => Vector3.Distance(item, localPoint) < minDistance))
                        return false;
                    return true;
                });
                if (!point.Equals(Vector3.negativeInfinity))
                {
                    randomLocalPoints.Add(transform.InverseTransformPoint(collider.transform.TransformPoint(point)));
                }
            }
            while (!(randomLocalPoints.Count >= count || i >= maxAttempt));

            Vector3 RandonPositionByBounds(Bounds boundingBox, Predicate<Vector3> predicate = null)
            {
                int attempt = Const.IntValue.One;
                while (attempt <= maxAttempt)
                {
                    var randomNormalizedDirection = new Vector3(
                        RandomHelper.RandomOpposite(boundingBox.extents.x),
                        RandomHelper.RandomOpposite(boundingBox.extents.y),
                        RandomHelper.RandomOpposite(boundingBox.extents.z));

                    var randomPoint = boundingBox.center + randomNormalizedDirection;
                    if (predicate?.Invoke(randomPoint) ?? true)
                        return randomPoint;
                    attempt++;
                }
                return Vector3.negativeInfinity;
            }
        }

        public Vector3 GetRandomPoint()
        {
            Initialize();
            return randomLocalPoints.GetRandom();
        }
    }

    [SerializeField]
    private ParticleSystem smokeFX, explosionFX;
    [SerializeField]
    private Rigidbody bulletRb;
    [SerializeField]
    private Collider bulletCollider;
    [SerializeField]
    private PBPartSkin partSkin;
    [SerializeField]
    private Renderer[] renderers;

    private Vector3 localOffset;
    private LayerMask targetLayerMask;
    private Transform target;
    private RocketArtilleryConfigSO configSO;
    private RocketArtilleryBehaviour rocketGun;
    private Action<IDamagable> onObjectHit;

    [ShowInInspector]
    public LayerMask TargetLayerMask
    {
        get => targetLayerMask;
        set => targetLayerMask = value;
    }
    [ShowInInspector]
    public LayerMask OtherLayerMask
    {
        get => Physics.DefaultRaycastLayers ^ TargetLayerMask ^ (1 << gameObject.layer);
        set
        {

        }
    }
    public Rigidbody BulletRb
    {
        get => bulletRb;
    }

    private void FixedUpdate()
    {
        if (bulletRb == null || bulletRb.isKinematic)
            return;
        Vector3 currentPosition = transform.position;
        if (target == null)
        {
            bulletRb.AddForce(Physics.gravity * configSO.GravityScale, ForceMode.Acceleration);
        }
        else
        {
            Vector3 delta = GetTargetPosition() - currentPosition;
            Vector3 direction = delta.normalized * configSO.DistanceToSpeedCurve.Evaluate(delta.magnitude);
            bulletRb.velocity = Vector3.ClampMagnitude(bulletRb.velocity + configSO.ChasingSpeed * Time.fixedDeltaTime * direction, configSO.MaxSpeed);
        }

        if (bulletRb.velocity != Vector3.zero)
        {
            transform.LookAt(currentPosition + bulletRb.velocity.normalized);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            return;
        }
        IDamagable damagableObject;
        if (other.TryGetComponent(out damagableObject))
        {
            onObjectHit?.Invoke(damagableObject);
        }
        else if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out damagableObject))
        {
            onObjectHit?.Invoke(damagableObject);
        }
        bulletCollider.enabled = false;
        bulletRb.isKinematic = true;
        smokeFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        explosionFX.Play();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }
        // TODO: Consider using ObjectPool, and return to pool instead
        Destroy(gameObject, 1f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (configSO == null || bulletRb == null)
            return;
        if (enabled && configSO.DrawGizmos)
        {
            Handles.color = Color.red;
            Handles.DrawLine(transform.position, transform.position + bulletRb.velocity.normalized * 5f);

            // Draw the search zone
            if (configSO.DrawSearchZone)
            {
                Handles.color = configSO.SearchZoneColor;
                Handles.DrawSolidArc(transform.position, Quaternion.AngleAxis(90, -transform.right) * transform.forward, Quaternion.AngleAxis(-configSO.SearchAngle / 2, transform.up) * transform.forward, configSO.SearchAngle, configSO.SearchRange);
                Handles.DrawSolidArc(transform.position, transform.forward, Quaternion.AngleAxis(-configSO.SearchAngle / 2, transform.up) * transform.forward, 360, configSO.SearchRange);
            }

            // Draw a line to the target
            if (target != null)
            {
                Handles.color = configSO.LineColor;
                Handles.DrawLine(GetTargetPosition(), transform.position);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (configSO == null)
            return;
        if (enabled && configSO.DrawGizmos)
        {
            if (target != null)
            {
                Gizmos.color = configSO.LineColor;
                Gizmos.DrawWireSphere(GetTargetPosition(), 0.15f);
            }
        }
    }
#endif

    private bool IsWithinRange(Transform target)
    {
        var distance = Vector3.Distance(target.position, transform.position);
        var angle = Vector3.Angle(target.position - transform.position, transform.forward);
        if (distance <= configSO.SearchRange && angle <= configSO.SearchAngle / 2f)
        {
            return true;
        }
        return false;
    }

    private Transform FindClosestTarget()
    {
        Vector3 currentPosition = transform.position;
        Collider[] detectedColliders = Physics.OverlapSphere(currentPosition, configSO.SearchRange, targetLayerMask, QueryTriggerInteraction.Ignore);
        Array.Sort(detectedColliders, (x, y) => Vector3.Distance(x.transform.position, currentPosition).CompareTo(Vector3.Distance(y.transform.position, currentPosition)));
        if (detectedColliders != null && detectedColliders.Length > 0)
        {
            for (int i = 0; i < detectedColliders.Length; i++)
            {
                Collider detectedCollider = detectedColliders[i];
                if (detectedCollider.attachedRigidbody != null && detectedCollider.attachedRigidbody.TryGetComponent(out PBPart pbPart) && pbPart.RobotChassis != null && !pbPart.RobotChassis.Robot.IsDead && pbPart.RobotChassis.CarPhysics != null && IsWithinRange(detectedCollider.attachedRigidbody.transform))
                {
                    return pbPart.RobotChassis.CarPhysics.transform;
                }
            }
        }
        return null;
    }

    private Vector3 GetTargetPosition()
    {
        return target.TransformPoint(localOffset);
    }

    private Vector3 CalcRandomLocalOffset()
    {
        Vector3 randomLocalOffset = default;
        if (target != null)
        {
            HomingPositionGenerator generator = target.GetOrAddComponent<HomingPositionGenerator>();
            return generator.GetRandomPoint();
        }
        return randomLocalOffset;
    }

    public void Init(int mLayerIndex, RocketArtilleryBehaviour rocketGun)
    {
        this.rocketGun = rocketGun;
        configSO = rocketGun.ConfigSO;
        partSkin.part = rocketGun.PbPart;
        bulletRb.detectCollisions = false;
        bulletRb.isKinematic = true;
        gameObject.SetLayer(mLayerIndex, true);
        for (int i = 31; i > (31 - 6); i--)
        {
            targetLayerMask |= 1 << i;
        }
        // Ignore myself
        targetLayerMask ^= 1 << mLayerIndex;
    }

    //Init from skill. Dont have PartBehavior, PBPart or PBPartSkin
    public void Init(int mOwnerLayerIndex, RocketArtilleryConfigSO configSO)
    {
        this.rocketGun = null;
        this.configSO = configSO;
        bulletRb.detectCollisions = false;
        bulletRb.isKinematic = true;
        gameObject.SetLayer(mOwnerLayerIndex, true);
        for (int i = 31; i > (31 - 6); i--)
        {
            targetLayerMask |= 1 << i;
        }
        //Ignore my owner
        targetLayerMask ^= 1 << mOwnerLayerIndex;
    }

    [Button]
    public void Fire(Transform target = null, Action<IDamagable> onObjectHit = null)
    {
        if (!this)
        {
            return;
        }
        if (target == null)
        {
            target = FindClosestTarget();
        }
        this.target = target;
        this.localOffset = CalcRandomLocalOffset();
        this.onObjectHit = onObjectHit;
        bulletRb.transform.SetParent(null);
        bulletRb.detectCollisions = true;
        bulletRb.isKinematic = false;
        bulletRb.AddForce(bulletRb.transform.forward * configSO.LaunchForce, ForceMode.VelocityChange);
        smokeFX.Play();
        // TODO: Consider using ObjectPool, and return to pool instead
        Destroy(gameObject, configSO.RocketLifetime);
        // Loop through all robots and ignore all dead one for performance wise
        List<PBRobot> robots = PBRobot.allFightingRobots;
        for (int i = 0; robots != null && i < robots.Count; i++)
        {
            if (rocketGun && robots[i] == rocketGun.PbPart.RobotChassis.Robot)
                continue;
            if (robots[i].IsDead)
            {
                targetLayerMask ^= 1 << robots[i].RobotLayer;
            }
        }
    }
}