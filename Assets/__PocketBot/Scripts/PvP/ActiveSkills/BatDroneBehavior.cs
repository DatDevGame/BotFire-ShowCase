using DG.Tweening;
using LatteGames;
using LatteGames.Template;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static ActiveSkillCaster;

public class BatDroneBehavior : MonoBehaviour, IAttackable
{
    [SerializeField] private List<MeshRenderer> m_Models;
    [SerializeField] private LazerBeamBehavior m_LazerBeamBehavior;
    [SerializeField] private ParticleSystem m_DestructVFX;
    [SerializeField] private ParticleSystem m_Trail;

    private Transform m_Owner;
    private LayerMask m_ObstacleMask;
    private bool m_IsActive = false;
    private bool m_IsSelfDestructing = false;
    private bool m_HasExplosion = false;
    private bool m_IsAttacking = false;
    private float m_FollowHeightOffset;
    private float m_SeparationRadius;
    private float m_AvoidanceStrength;
    private float m_AttackZone;
    private float m_ZoneExplode;
    private float m_ExpiresDuratuon;
    private float m_FireRate;
    private float m_DamagePerHitRatio;
    private float m_DamageLazerMaxHealthPercent;
    private float m_DamageExplosionMaxHealthPercent;

    private float m_RandomWaveSpeed;
    private float m_RandomWaveStrengthX;
    private float m_RandomWaveStrengthY;
    private float m_RandomWaveStrengthZ;
    private float m_TimeResetMovePath;
    private float m_RandomWaveOffset;

    private int m_ChassisIDReceiveDamage;
    private int m_DroneIndex;
    private float m_DamageExplosion;
    private PBRobot m_OwnerRobot;
    private PBRobot m_OpponentNearestRobot;
    
    private LayerMask m_RobotLayerMask;
    private ActiveSkillBatDroneSO m_ActiveSkillBatDroneSO;
    private List<LazerBeamBehavior> m_LazerBeamPools = new List<LazerBeamBehavior>();

    private void Start()
    {
        UpdateRandomWaveParameters();
        StartCoroutine(UpdateWaveOffset());
        StartCoroutine(FireLasers());
        m_RandomWaveOffset = Random.Range(0f, 10f);
    }

    private void Update()
    {
        if (m_IsSelfDestructing || m_HasExplosion)
            return;

        DetectRobot();
        if (m_IsActive)
            FollowTarget(m_Owner);
    }

    public void Activate(PBRobot robot, ActiveSkillBatDroneSO activeSkillBatDroneSO, int index)
    {
        if (m_IsActive) return;
        m_IsActive = true;

        m_OwnerRobot = robot;
        m_ActiveSkillBatDroneSO = activeSkillBatDroneSO;

        this.m_Owner = robot.ChassisInstance.CarPhysics.transform;
        m_FollowHeightOffset = activeSkillBatDroneSO.FollowHeightOffset;
        m_SeparationRadius = activeSkillBatDroneSO.SeparationRadiusDrone;
        m_AvoidanceStrength = activeSkillBatDroneSO.AvoidanceStrength;
        m_AttackZone = activeSkillBatDroneSO.AttackZone;
        m_ZoneExplode = activeSkillBatDroneSO.ZoneExplode;
        m_RobotLayerMask = activeSkillBatDroneSO.RobotLayermask;
        m_ExpiresDuratuon = activeSkillBatDroneSO.ExpiresDuration;
        m_DroneIndex = index;
        m_FireRate = activeSkillBatDroneSO.FireRate;
        m_FireRate = activeSkillBatDroneSO.FireRate;
        m_DamageLazerMaxHealthPercent = activeSkillBatDroneSO.DamageLazerMaxHealthPercent;
        m_DamageExplosionMaxHealthPercent = activeSkillBatDroneSO.DamageExplosionMaxHealthPercent;
        m_DamagePerHitRatio = activeSkillBatDroneSO.DamagePerHitRatio;
        m_Trail.gameObject.SetActive(true);
        m_Trail.Play();
        StartCoroutine(ExpiresCR());
    }

    private void DetectRobot()
    {
        Vector3 myPosition = m_OwnerRobot.ChassisInstance.CarPhysics.transform.position;
        m_OpponentNearestRobot = Physics.OverlapSphere(myPosition, m_AttackZone, m_RobotLayerMask)
            .Where(collider => IsValidRobotTarget(collider))
            .Select(collider => collider.GetComponent<PBPart>()?.RobotChassis?.Robot)
            .Where(robot => robot != null && !robot.IsDead)
            .OrderBy(robot => Vector3.Distance(myPosition, robot.ChassisInstance.CarPhysics.transform.position))
            .FirstOrDefault();
    }

    private bool IsValidRobotTarget(Collider collider)
    {
        PBPart part = collider.GetComponent<PBPart>();
        return part?.RobotChassis?.CarPhysics != null && part.RobotChassis.Robot != m_OwnerRobot;
    }

    private void UpdateRandomWaveParameters()
    {
        m_RandomWaveSpeed = Random.Range(0.1f, 0.5f); 
        m_RandomWaveStrengthX = Random.Range(0.1f, 0.5f);
        m_RandomWaveStrengthY = Random.Range(0.1f, 0.5f);
        m_RandomWaveStrengthZ = Random.Range(0.1f, 0.5f);
    }

    private IEnumerator UpdateWaveOffset()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));
            m_RandomWaveOffset = Random.Range(0f, 10f);
        }
    }

    private void FollowTarget(Transform followTarget)
    {
        Vector3 targetPosition = followTarget.position + Vector3.up * m_FollowHeightOffset;

        if (IsObstacleInPath(targetPosition, out Vector3 avoidanceVector))
        {
            targetPosition += avoidanceVector * m_AvoidanceStrength;
        }

        Vector3 separationOffset = AvoidOtherDrones();
        targetPosition += separationOffset;

        float waveOffsetX = (Mathf.PerlinNoise((Time.time + m_RandomWaveOffset) * m_RandomWaveSpeed, 0f) - 0.5f) * m_RandomWaveStrengthX;
        float waveOffsetY = (Mathf.PerlinNoise(0f, (Time.time + m_RandomWaveOffset) * m_RandomWaveSpeed) - 0.5f) * m_RandomWaveStrengthY;
        float waveOffsetZ = (Mathf.PerlinNoise((Time.time + m_RandomWaveOffset) * m_RandomWaveSpeed, (Time.time + m_RandomWaveOffset) * 0.5f) - 0.5f) * m_RandomWaveStrengthZ;

        Vector3 offset = new Vector3(waveOffsetX, waveOffsetY, waveOffsetZ);
        targetPosition += offset;

        transform.DOKill();

        m_TimeResetMovePath -= Time.deltaTime;
        if (m_TimeResetMovePath <= 0)
        {
            UpdateRandomWaveParameters();
            m_TimeResetMovePath = Random.Range(0.1f, 5);
        }

        transform.DOBlendableMoveBy((targetPosition - transform.position) * 0.5f, 0.15f)
            .SetEase(Ease.Linear)
            .SetUpdate(UpdateType.Fixed);

        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero && m_OpponentNearestRobot == null)
        {
            transform.DORotateQuaternion(Quaternion.LookRotation(direction), 0.2f)
                .SetEase(Ease.OutSine)
                .SetUpdate(UpdateType.Fixed);
        }
    }

    private Vector3 AvoidOtherDrones()
    {
        float avoidanceForce = 1f; 
        Collider[] nearbyDrones = Physics.OverlapSphere(transform.position, m_SeparationRadius, LayerMask.GetMask("Drone"));

        Vector3 separationVector = Vector3.zero;
        foreach (var drone in nearbyDrones)
        {
            if (drone.transform != transform)
            {
                Vector3 awayDirection = (transform.position - drone.transform.position).normalized;
                separationVector += awayDirection;
            }
        }
        return separationVector.normalized * avoidanceForce;
    }

    protected bool IsObstacleInPath(Vector3 targetPosition, out Vector3 avoidanceVector)
    {
        RaycastHit hit;
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (Physics.Raycast(transform.position, direction, out hit, distance, m_ObstacleMask))
        {
            avoidanceVector = Vector3.Cross(direction, Vector3.up).normalized;
            return true;
        }

        avoidanceVector = Vector3.zero;
        return false;
    }

    private void FireLaser(PBRobot targetRobot)
    {
        if (targetRobot == null) return;
        Transform target = targetRobot.ChassisInstance.CarPhysics.transform;
        if (target == null)
            return;

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        transform
            .DOLookAt(target.position, 0)
            .OnComplete(Fire);

        void Fire()
        {
            SoundManager.Instance.PlaySFX(SFX.BlackKnight2_DroneLaserShot);
            LazerBeamBehavior lazer = null;
            lazer = m_LazerBeamPools.Where(v => !v.IsActive).FirstOrDefault();
            if (lazer == null)
            {
                lazer = Instantiate(m_LazerBeamBehavior);
                m_LazerBeamPools.Add(lazer);
            }

            float damage = 0;
            float shotsPerSecond = 1f / m_FireRate;
            float baseDamage = (m_DamageLazerMaxHealthPercent / 100) * m_OwnerRobot.MaxHealth;
            damage = baseDamage / shotsPerSecond;

            lazer.transform.position = transform.position;
            lazer.transform.rotation = transform.rotation;
            lazer.gameObject.SetActive(true);
            lazer.EnableMissile(m_OwnerRobot, targetRobot, damage);
        }
    }

    private IEnumerator FireLasers()
    {
        bool IsFireFollowingIndex = false;
        while (m_IsActive && !m_IsSelfDestructing && !m_HasExplosion)
        {
            if (m_OpponentNearestRobot != null)
            {
                if (!IsFireFollowingIndex)
                {
                    yield return new WaitForSeconds(m_DroneIndex * 0.2f);
                    IsFireFollowingIndex = true;
                }

                m_IsAttacking = true;
                FireLaser(m_OpponentNearestRobot);
                yield return new WaitForSeconds(m_FireRate);
            }
            else
            {
                m_IsAttacking = false;
                IsFireFollowingIndex = false;
            }

            yield return null;
        }
    }

    private IEnumerator ExpiresCR()
    {
        yield return new WaitForSeconds(m_ExpiresDuratuon);
        m_IsSelfDestructing = true;

        if (m_OpponentNearestRobot != null)
        {
            Vector3 retreatPosition = transform.position - transform.forward * 7f;
            transform
                .DOMove(retreatPosition, 0.15f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(m_DroneIndex * 0.2f, () =>
                    {
                        if (m_OpponentNearestRobot != null)
                        {
                            Vector3 startPos = transform.position;
                            Vector3 targetPos = m_OpponentNearestRobot.ChassisInstance.CarPhysics.transform.position;

                            float randomHeight1 = Random.Range(2f, 5f);
                            float randomHeight2 = Random.Range(1f, 4f);

                            float randomSide1 = Random.Range(-2f, 2f);
                            float randomSide2 = Random.Range(-1.5f, 1.5f);

                            Vector3 midPoint1 = Vector3.Lerp(startPos, targetPos, 0.3f) + new Vector3(randomSide1, randomHeight1, 0);
                            Vector3 midPoint2 = Vector3.Lerp(startPos, targetPos, 0.6f) + new Vector3(randomSide2, randomHeight2, 0);

                            Vector3[] path = new Vector3[] { midPoint1, midPoint2};

                            transform.DOLookAt(targetPos, 0.2f, AxisConstraint.Y);

                            transform.DOPath(path, 0.15f, PathType.CatmullRom)
                                .SetEase(Ease.InOutSine)
                                .OnComplete(() =>
                                {
                                    transform
                                        .DOMove(m_OpponentNearestRobot.ChassisInstance.CarPhysics.transform.position, 0.1f)
                                        .SetEase(Ease.InOutSine)
                                        .OnComplete(() =>
                                        {
                                            if (!m_HasExplosion)
                                                Explode();
                                        });
                                });
                        }
                    }, false);
                });
        }
        else
        {
            FlyUpAndDisappear();
        }
    }


    private void Explode()
    {
        if (m_HasExplosion)
            return;
        SoundManager.Instance.PlaySFX(SFX.BlackKnight2_DroneExplode);
        m_HasExplosion = true;
        m_Models.ForEach(v => v.enabled = false);
        m_DestructVFX.Play();
        m_LazerBeamBehavior.gameObject.SetActive(false);

        Collider[] affectedObjects = Physics.OverlapSphere(transform.position, m_ZoneExplode);
        foreach (var obj in affectedObjects)
        {
            if (obj.TryGetComponent(out PBPart pbPart))
            {
                if (pbPart != null && m_ChassisIDReceiveDamage != pbPart.RobotChassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID && pbPart.RobotChassis.Robot != m_OwnerRobot)
                {
                    m_DamageExplosion = m_OwnerRobot.MaxHealth * (m_DamageExplosionMaxHealthPercent / 100);
                    pbPart.RobotChassis.ReceiveDamage(this, Const.FloatValue.ZeroF);
                    m_ChassisIDReceiveDamage = pbPart.RobotChassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID;
                }
            }
        }
    }
    private void FlyUpAndDisappear()
    {
        m_LazerBeamPools
            .Where(v => v != null).ToList()
            .ForEach(v => Destroy(v.gameObject));

        float flyHeight = Random.Range(30f, 50f);
        float flyDuration = Random.Range(1f, 2f);
        float rotateSpeed = Random.Range(360f, 1080f);

        transform
            .DOMoveY(transform.position.y + flyHeight, flyDuration)
            .SetEase(Ease.InCubic)
            .OnComplete(() => Destroy(gameObject));

        transform
            .DORotate(new Vector3(0, rotateSpeed, 0), flyDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear);
    }

    private void OnTriggerEnter(Collider other)
    {
        PBPart part = other.gameObject.GetComponent<PBPart>();
        if (part != null && part.RobotChassis.Robot != m_OwnerRobot)
        {
            Explode();
        }
    }

    public float GetDamage()
    {
        return m_DamageExplosion;
    }
}
