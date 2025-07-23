using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Gun : MonoBehaviour, IAttackable
{
    [Header("Gun Settings")]
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float attackDamage = 5f;

    [Header("Auto Aim Settings")]
    [SerializeField] private bool autoAim = true;
    [SerializeField, ShowIf("autoAim")] private float autoAimRadius = 5f;
    [SerializeField, ShowIf("autoAim")] private float autoAimAngel = 360f;
    [SerializeField, ShowIf("autoAim")] private Transform gunTransform;
    [SerializeField, ShowIf("autoAim")] private RadarDetector radarDetector;

    [Header("Bullet Settings")]
    [SerializeField] private Transform bulletStartPoint;
    [SerializeField] private Bullet bulletPrefab;

    [Header("Cone Spread Settings")]
    [SerializeField] private bool useRandomConeDirection = true;
    [SerializeField, Range(0.1f, 10f)] private float coneLength = 2f;
    [SerializeField, Range(0f, 5f)] private float coneInnerRadius = 0f;
    [SerializeField, Range(0.1f, 5f)] private float coneOuterRadius = 1f;

    [Header("Layer")]
    [SerializeField] private LayerMask m_LayerObstacle;
    private Vector3 lastBulletDir;
    private Quaternion initialRotation;
    private bool hasValidTargetInAim = false;

    private PBRobot m_Robot;
    public PBRobot Robot => m_Robot;

    private void Start()
    {
        m_Robot = GetComponentInParent<PBRobot>();
        radarDetector.Initialize(m_Robot.AIBotController, autoAimRadius, autoAimAngel);
        if (gunTransform != null)
            initialRotation = gunTransform.localRotation;

        StartCoroutine(Fire_CR());
    }

    private void Update()
    {
        AutoAim();
    }

    private void AutoAim()
    {
        if (radarDetector.BotController == null)
            return;
        if (!radarDetector.TryScanOpponentRobotInDetectArea(out PBRobot otherRobot))
        {
            ResetGunAxis();
            return;
        }
        
        if (m_Robot == null || otherRobot == null || m_Robot.TeamId == otherRobot.TeamId || m_Robot.IsDead)
        {
            ResetGunAxis();
            return;
        }

        if (!IsPathClear(bulletStartPoint.position, otherRobot.ChassisInstance.CarPhysics.transform.position, m_LayerObstacle))
        {
            ResetGunAxis();
            return;
        }

        AimAtTarget(otherRobot);
    }
    private void AimAtTarget(PBRobot target)
    {
        Vector3 direction = target.GetTargetPoint() - gunTransform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        gunTransform.rotation = rotation;
        hasValidTargetInAim = true;
    }

    private void ResetGunAxis()
    {
        hasValidTargetInAim = false;

        if (gunTransform != null && gunTransform.localEulerAngles != Vector3.zero)
        {
            gunTransform.DOLocalRotate(Vector3.zero, 0.3f);
        }
    }


    private IEnumerator Fire_CR()
    {
        while (true)
        {
            yield return Yielders.Get(fireRate);
            if (!m_Robot.IsDead)
            {
                if (hasValidTargetInAim)
                    Fire();
            }
            else
                ResetGunAxis();
        }
    }

    public void Fire()
    {
        var bullet = Instantiate(bulletPrefab);
        bullet.transform.position = bulletStartPoint.position;

        Vector3 direction = bulletStartPoint.forward;
        if (useRandomConeDirection)
        {
            direction = RandomDirectionInCone(direction);
        }

        bullet.transform.rotation = Quaternion.LookRotation(direction);
        bullet.Fire(this, direction * speed);

        lastBulletDir = direction;
    }

    private Vector3 RandomDirectionInCone(Vector3 forward)
    {
        Quaternion baseRotation = Quaternion.LookRotation(forward);

        float radius = Random.Range(coneInnerRadius, coneOuterRadius);
        float angle = Random.Range(0f, 2f * Mathf.PI);

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * radius,
            Mathf.Sin(angle) * radius,
            coneLength
        );

        return baseRotation * offset.normalized;
    }

    public float GetDamage()
    {
        return attackDamage;
    }

    private float CalcSignedAngle(Transform target)
    {
        if (target == null || bulletStartPoint == null || gunTransform == null)
            return float.NegativeInfinity;

        Vector3 closestPoint = target.transform.position;
        return Vector3.SignedAngle(
            bulletStartPoint.position - gunTransform.position,
            closestPoint - gunTransform.position,
            bulletStartPoint.right
        );
    }
    private bool IsPathClear(Vector3 pointA, Vector3 pointB, LayerMask layerMask)
    {
        bool isClear = !Physics.Linecast(pointA, pointB, layerMask, QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
        Color rayColor = isClear ? Color.green : Color.red;
        Debug.DrawLine(pointA, pointB, rayColor);
#endif

        return isClear;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (bulletStartPoint == null) return;

        Vector3 origin = bulletStartPoint.position;
        Vector3 forward = bulletStartPoint.forward;
        Quaternion rotation = Quaternion.LookRotation(forward);

        int segments = 64;
        Vector3[] innerCircle = new Vector3[segments];
        Vector3[] outerCircle = new Vector3[segments];

        for (int i = 0; i < segments; i++)
        {
            float angle = (2 * Mathf.PI / segments) * i;
            Vector3 unitCircle = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);

            Vector3 innerPos = rotation * new Vector3(unitCircle.x * coneInnerRadius, unitCircle.y * coneInnerRadius, coneLength) + origin;
            Vector3 outerPos = rotation * new Vector3(unitCircle.x * coneOuterRadius, unitCircle.y * coneOuterRadius, coneLength) + origin;

            innerCircle[i] = innerPos;
            outerCircle[i] = outerPos;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, outerPos);
        }

        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(innerCircle[i], innerCircle[next]);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(outerCircle[i], outerCircle[next]);
        }

        if (Application.isPlaying && lastBulletDir != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(origin, lastBulletDir.normalized * coneLength);
        }
    }
#endif
}

