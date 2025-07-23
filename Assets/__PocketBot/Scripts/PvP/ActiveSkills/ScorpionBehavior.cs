using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using FIMSpace.FProceduralAnimation;
using LatteGames.Template;
using DG.Tweening;

public class ScorpionBehavior : MonoBehaviour, IAttackable
{
    [SerializeField, BoxGroup("Config")] private LayerMask m_WallLayer;
    [SerializeField, BoxGroup("Config")] private LayerMask m_GroundLayer;
    [SerializeField, BoxGroup("Config")] private LayerMask m_RobotLayer;

    [SerializeField, BoxGroup("Ref")] private SkinnedMeshRenderer m_Model;
    [SerializeField, BoxGroup("Ref")] private Transform m_PointHead;
    [SerializeField, BoxGroup("Ref")] private LegsAnimator m_LegsAnimator;
    [SerializeField, BoxGroup("Ref")] private Rigidbody m_Rigidbody;
    [SerializeField, BoxGroup("Ref")] private Collider m_Collider;
    [SerializeField, BoxGroup("Ref")] private ParticleSystem m_ExplosionVFX;
    [SerializeField, BoxGroup("Ref")] private DOTweenAnimation m_WaringAnimation;

    private Vector3 m_TargetDirection;
    private float m_DamagePercent;
    private float m_MoveSpeed;
    private float m_LifeTime;
    private float m_RayDistanceWall;
    private float m_FromDampingTime;
    private float m_ToDampingTime;
    private float m_DetectionRadius;
    private float m_MinDictanceAttack;
    private float m_TimeWarning;
    private float m_JumpHeight;
    private float m_JumpDuration;
    private float m_DampingTimer;
    private float m_ZoneExplode;
    private bool m_IsDamping = false;
    private bool m_IsRobotDetected = false;
    private bool m_IsAttacking = false;
    private bool m_IsEnable = false;
    private bool m_IsExplode = false;
    private bool m_SoundCallOneTime = false;

    private CarPhysics m_NearestCarPhysics = null;
    private PBRobot m_MainRobot;
    private int m_ChassisIDReceiveDamage;
    private float m_TimeDelayStart = 0;
    private float m_AttackDelay = 0;
    private float m_AttackDelayTime = 0;

    public PBRobot MainRobot => m_MainRobot;

    private void Update()
    {
        if (!m_IsEnable)
            return;

        m_TimeDelayStart -= Time.deltaTime;
        if (m_TimeDelayStart > 0)
            return;

        if (m_IsAttacking)
            return;

        if (m_IsRobotDetected && m_NearestCarPhysics != null)
        {
            FollowingTarget();
            return;
        }

        DetectRobot();
        MoveForward();
        CheckWallAndReflect();
    }

    public void InitData(PBRobot robot, ActiveSkillScorpionBotsSO activeSkillScorpionBotsSO)
    {
        m_AttackDelayTime = m_AttackDelay;

        m_MainRobot = robot;
        m_DamagePercent = activeSkillScorpionBotsSO.DamagePercent;
        m_MoveSpeed = activeSkillScorpionBotsSO.MoveSpeed;
        m_LifeTime = activeSkillScorpionBotsSO.LifeTime;
        m_RayDistanceWall = activeSkillScorpionBotsSO.RayDistanceWall;
        m_FromDampingTime = activeSkillScorpionBotsSO.FromDampingTime;
        m_ToDampingTime = activeSkillScorpionBotsSO.ToDampingTime;
        m_DetectionRadius = activeSkillScorpionBotsSO.DetectionRadius;
        m_MinDictanceAttack = activeSkillScorpionBotsSO.MinDictanceAttack;
        m_TimeWarning = activeSkillScorpionBotsSO.TimeWarning;
        m_JumpHeight = activeSkillScorpionBotsSO.JumpHeight;
        m_JumpDuration = activeSkillScorpionBotsSO.JumpDuration;
        m_ZoneExplode = activeSkillScorpionBotsSO.ZoneExplode;
        this.gameObject.SetLayer(m_MainRobot.RobotLayer, true);
        m_TimeDelayStart = 0.5f;
    }

    public void EnableBehavior()
    {
        m_IsEnable = true;
        m_Rigidbody.useGravity = true;
        m_Collider.enabled = true;
        m_LegsAnimator.enabled = true;
        ActiveLifeTime();
    }

    public void DisableBehavior() 
    {
        m_IsEnable = false;
        m_Rigidbody.useGravity = false;
        m_Collider.enabled = false;
        m_LegsAnimator.enabled = false;
    }

    private void ActiveLifeTime()
    {
        DOVirtual.Float(m_LifeTime, 0, m_LifeTime, (value) =>
        {
            if(value <= 0.5f && !m_IsAttacking)
                
            if (!m_SoundCallOneTime && value <= m_TimeWarning && !m_IsAttacking)
            {
                m_SoundCallOneTime = true;
                SoundManager.Instance.PlaySFX(SFX.BombBotTicking);
                m_WaringAnimation.DOPlay();
            }
        })
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            if (!m_IsAttacking)
            {
                DisableBehavior();
                Explode();
            }
        });
    }

    private void FollowingTarget()
    {
        if (m_NearestCarPhysics == null) return;

        float distance = Vector3.Distance(transform.position, m_NearestCarPhysics.transform.position);

        if (distance > m_MinDictanceAttack)
        {
            Vector3 directionToTarget = (m_NearestCarPhysics.transform.position - transform.position).normalized;
            transform.position += directionToTarget * m_MoveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(directionToTarget);
            m_AttackDelayTime = m_AttackDelay;
        }
        else if (distance <= m_MinDictanceAttack)
        {
            m_AttackDelayTime -= Time.deltaTime;
            if(m_AttackDelayTime <= 0)
                StartCoroutine(AttackTarget());
        }
    }

    private IEnumerator AttackTarget()
    {
        m_Rigidbody.isKinematic = true;
        m_IsAttacking = true;
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;

        transform.DORotate(new Vector3(-45f, transform.eulerAngles.y, transform.eulerAngles.z), 0.2f);
        while (elapsedTime < m_JumpDuration)
        {
            Vector3 randomPositionNearestCarPhysics = new Vector3(m_NearestCarPhysics.transform.position.x + Random.RandomRange(-0.5f, 0.5f), m_NearestCarPhysics.transform.position.y, m_NearestCarPhysics.transform.position.z + Random.RandomRange(-0.5f, 0.5f));
            float t = elapsedTime / m_JumpDuration;
            float height = Mathf.Sin(t * Mathf.PI) * m_JumpHeight;
            transform.position = Vector3.Lerp(startPos, randomPositionNearestCarPhysics, t) + Vector3.up * height;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        m_WaringAnimation.DOPlay();
        SoundManager.Instance.PlaySFX(SFX.BombBotTicking);
        yield return new WaitForSeconds(m_TimeWarning);
        Explode();
    }

    private IEnumerator ExplodeDelay()
    {
        if(!m_IsAttacking)
            SoundManager.Instance.PlaySFX(SFX.BombBotTicking);
        float waringTime = 0.5f;
        yield return new WaitForSeconds(waringTime);
        Explode();
    }
    private void Explode()
    {
        if (m_IsExplode) return;
        m_IsExplode = true;
        m_ExplosionVFX.Play();

        if (PBSoundUtility.IsOnSound())
            SoundManager.Instance.PlaySFX(SFX.Landmine);

        Collider[] affectedObjects = Physics.OverlapSphere(transform.position, m_ZoneExplode);
        foreach (var obj in affectedObjects)
        {
            if (obj.TryGetComponent(out PBPart pbPart))
            {
                if (pbPart != null && pbPart.RobotChassis != null && m_ChassisIDReceiveDamage != pbPart.RobotChassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID && m_MainRobot != pbPart.RobotChassis.Robot)
                {
                    pbPart.RobotChassis.ReceiveDamage(this, Const.FloatValue.ZeroF);
                    m_ChassisIDReceiveDamage = pbPart.RobotChassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID;
                }
            }
        }
        m_Model.gameObject.SetActive(false);
    }

    private void MoveForward()
    {
        if (!IsCanMove()) return;
        transform.position += transform.forward * m_MoveSpeed * Time.deltaTime;
    }

    private void CheckWallAndReflect()
    {
        RaycastHit hit;
        if (Physics.Raycast(m_PointHead.position, transform.forward, out hit, m_RayDistanceWall, m_WallLayer, QueryTriggerInteraction.Ignore))
        {
            Reflect(hit);
            return;
        }
        if (!IsGroundAhead())
        {
            TurnAround();
        }

        void Reflect(RaycastHit hit)
        {
            Vector3 reflectDir = Vector3.Reflect(transform.forward, hit.normal);
            m_TargetDirection = reflectDir.normalized;
            m_DampingTimer = Random.Range(m_FromDampingTime, m_ToDampingTime);
            m_IsDamping = true;
            StartCoroutine(DampingRotation(m_DampingTimer));
        }

        bool IsGroundAhead()
        {
            RaycastHit groundHit;
            Vector3 groundCheckPos = m_PointHead.position + transform.forward;
            return Physics.Raycast(groundCheckPos, Vector3.down, out groundHit, 5, m_GroundLayer);
        }

        void TurnAround()
        {
            m_TargetDirection = -transform.forward;
            m_DampingTimer = Random.Range(m_FromDampingTime, m_ToDampingTime);
            m_IsDamping = true;
            StartCoroutine(DampingRotation(m_DampingTimer));
        }
    }


    private IEnumerator DampingRotation(float dampingTime)
    {
        float elapsedTime = 0f;
        Quaternion initialRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(m_TargetDirection);

        while (elapsedTime < dampingTime)
        {
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / dampingTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        m_IsDamping = false;
    }

    private bool IsCanMove()
    {
        return !m_IsDamping && !m_IsRobotDetected && !m_IsAttacking;
    }

    private void DetectRobot()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_DetectionRadius, m_RobotLayer);
        List<CarPhysics> carPhysicsList = new List<CarPhysics>();

        foreach (var collider in colliders)
        {
            PBPart part = collider.gameObject.GetComponent<PBPart>();
            if (part != null && part.RobotChassis != null && part.RobotChassis.CarPhysics != null && part.RobotChassis.Robot != m_MainRobot && !part.RobotChassis.Robot.IsDead)
            {
                if (!carPhysicsList.Contains(part.RobotChassis.CarPhysics))
                    carPhysicsList.Add(part.RobotChassis.CarPhysics);
            }
        }

        if (carPhysicsList.Count > 0)
            m_NearestCarPhysics = carPhysicsList.OrderBy(car => Vector3.Distance(transform.position, car.transform.position)).FirstOrDefault();
        else
            m_NearestCarPhysics = null;

        m_IsRobotDetected = carPhysicsList.Count > 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        PBPart part = collision.gameObject.GetComponent<PBPart>();
        if (part != null && m_IsAttacking)
        {
            if (part?.RobotChassis?.Robot != m_MainRobot)
            {
                DisableBehavior();
                m_LegsAnimator.enabled = false;
                transform.SetParent(m_NearestCarPhysics.transform);
                StartCoroutine(ExplodeDelay());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        MovingGround movingGround = other.GetComponentInParent<MovingGround>();
        if (movingGround != null)
        {
            transform.SetParent(movingGround.MovingRoot.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        MovingGround movingGround = other.GetComponentInParent<MovingGround>();
        if (movingGround != null)
        {
            transform.SetParent(movingGround.transform.parent);
        }
    }

    private void OnDrawGizmos()
    {
        if (m_PointHead == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_PointHead.position, m_PointHead.position + transform.forward * m_RayDistanceWall);

        RaycastHit hit;
        if (Physics.Raycast(m_PointHead.position, transform.forward, out hit, m_RayDistanceWall, m_WallLayer, QueryTriggerInteraction.Ignore))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hit.point, 0.1f);
            Vector3 reflectDir = Vector3.Reflect(transform.forward, hit.normal);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(hit.point, hit.point + reflectDir * 2f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, m_DetectionRadius);

        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.DrawSphere(transform.position, m_ZoneExplode);
    }

    public float GetDamage()
    {
        return m_DamagePercent;
    }
}