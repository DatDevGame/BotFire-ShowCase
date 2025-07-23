using System;
using System.Collections;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using LatteGames;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveSkillSO_Dash", menuName = "PocketBots/ActiveSkillSO/Dash")]
public class ActiveSkillDashSO : ActiveSkillSO<ActiveSkillDashSO, ActiveSkillDashCaster>
{
    [SerializeField, BoxGroup("Debug"), PropertyOrder(100)]
    private bool m_DrawGizmos;
    [SerializeField]
    private bool m_CanDashWithoutNearbyEnemy;
    [SerializeField]
    private float m_DashForce = 15f;
    [SerializeField]
    private float m_RangeToPerformSkill = 5f;
    [SerializeField, Range(0f, 1f)]
    private float m_DamagePercentage = 0.25f;
    [Header("Impact")]
    [SerializeField]
    private float m_UpwardModifier = 0.4f;
    [SerializeField]
    private float m_ImpactForce = 40;
    [SerializeField]
    private float m_ImpactTorque = 60f;
    [SerializeField]
    private float m_ImpactParticleScale = 1f;
    [SerializeField]
    private ParticleSystem m_ImpactParticlePrefab;
    [SerializeField]
    private ParticleSystem m_FrontDashParticlePrefab;
    [SerializeField]
    private ParticleSystem m_RearDashParticlePrefab;

    public bool drawGizmos => m_DrawGizmos;
    public bool canDashWithoutNearbyEnemy => m_CanDashWithoutNearbyEnemy;
    public float dashForce => m_DashForce;
    public float rangeToPerformSkill => m_RangeToPerformSkill;
    public float damagePercentage => m_DamagePercentage;
    public float upwardModifier => m_UpwardModifier;
    public float impactForce => m_ImpactForce;
    public float impactTorque => m_ImpactTorque;
    public float impactParticleScale => m_ImpactParticleScale;
    public ParticleSystem impactParticlePrefab => m_ImpactParticlePrefab;
    public ParticleSystem frontDashParticlePrefab => m_FrontDashParticlePrefab;
    public ParticleSystem rearDashParticlePrefab => m_RearDashParticlePrefab;
}
public class ActiveSkillDashCaster : ActiveSkillCaster<ActiveSkillDashSO>, IAttackable
{
    private PBRobot m_TargetingRobot;
    private PBRobot m_DetectedClosestRobot;
    private RadarDetector m_RadarDetector;
    private ParticleSystem m_ImpactParticle;
    private ParticleSystem m_DashFrontParticle;
    private ParticleSystem m_DashRearParticle;
    private Coroutine m_DashCoroutine;
    private OnTriggerCallback m_TriggerCallback;

    protected override void Update()
    {
        base.Update();
        if (m_RadarDetector != null && skillState != SkillState.Active && skillState != SkillState.OnCooldown)
        {
            m_RadarDetector.TryScanRobotInDetectArea(out m_DetectedClosestRobot);
        }
    }

    private void OnDrawGizmos()
    {
        m_RadarDetector.InvokeMethod("OnDrawGizmos");
    }

    private BoxCollider CalculateCombinedCollider(BoxCollider boxCollider, PBRobot robot)
    {
        Bounds combinedBounds = new Bounds();

        BoxCollider[] colliders = robot.ChassisInstance.CarPhysics.GetComponentsInChildren<BoxCollider>(true);
        foreach (var collider in colliders)
        {
            Bounds bounds = collider.bounds;
            bounds.center = boxCollider.transform.InverseTransformPoint(bounds.center);
            combinedBounds.Encapsulate(bounds);
        }

        PBChassis.PartContainer frontContainer = robot.ChassisInstance.PartContainers.Find(item => item.PartSlotType == PBPartSlot.Front_1);
        if (frontContainer.Containers != null && frontContainer.Containers[0] != null)
        {
            BoxCollider[] frontColliders = frontContainer.Containers[0].GetComponentsInChildren<BoxCollider>(true);
            foreach (var collider in frontColliders)
            {
                Bounds bounds = collider.bounds;
                bounds.center = boxCollider.transform.InverseTransformPoint(bounds.center);
                combinedBounds.Encapsulate(bounds);
            }
        }

        boxCollider.center = combinedBounds.center;
        boxCollider.size = combinedBounds.size;
        return boxCollider;
    }

    private Vector3 CalcDashDirection(PBRobot closestRobot)
    {
        return closestRobot == null ? m_Robot.ChassisInstanceTransform.forward : closestRobot.GetTargetPoint() - m_Robot.GetTargetPoint();
    }

    private void BeginDash(PBRobot targetingRobot)
    {
        //LGDebug.Log($"BEGIN DASH - {Time.time}");
        Vector3 direction = CalcDashDirection(targetingRobot);
        m_DashFrontParticle.gameObject.SetActive(true);
        m_DashFrontParticle.Play();
        m_DashRearParticle.Play();
        m_Robot.ChassisInstance.CarPhysics.CarRb.velocity = direction.normalized * m_ActiveSkillSO.dashForce;
        m_Robot.IgnoreCollision(targetingRobot, true);
        m_TriggerCallback.gameObject.SetActive(true);
        m_TriggerCallback.onTriggerStay += HandleDashHitObject;
        if (PBSoundUtility.IsOnSound())
        {
            SoundManager.Instance.PlaySFX(SFX.BlitzDash);
        }
    }

    private void EndDash(PBRobot targetingRobot)
    {
        //LGDebug.Log($"END DASH - {Time.time}");
        if (m_DashFrontParticle.gameObject.activeSelf)
        {
            m_DashFrontParticle.Stop(true);
            m_DashFrontParticle.gameObject.SetActive(false);
        }
        m_Robot.IgnoreCollision(targetingRobot, false);
        m_Robot.ChassisInstance.CarPhysics.CanMove = true;
        m_TriggerCallback.onTriggerStay -= HandleDashHitObject;
        m_TriggerCallback.gameObject.SetActive(false);
        remainingCooldown = GetActiveSkillSO().cooldown;
    }

    private IEnumerator Dash_CR(PBRobot targetingRobot)
    {
        m_TargetingRobot = targetingRobot;
        m_RemainingCooldown = m_ActiveSkillSO.cooldown;
        m_Robot.ChassisInstance.CarPhysics.CarRb.angularVelocity = Vector3.zero;
        m_Robot.ChassisInstance.CarPhysics.CanMove = false;
        if (targetingRobot != null)
        {
            Quaternion from = m_Robot.ChassisInstance.CarPhysics.CarRb.rotation;
            yield return CommonCoroutine.LerpFactor(AnimationDuration.TINY / 2f, t =>
            {
                Vector3 direction = targetingRobot.GetTargetPoint() - m_Robot.GetTargetPoint();
                Quaternion to = Quaternion.LookRotation(direction);
                m_Robot.ChassisInstance.CarPhysics.CarRb.MoveRotation(Quaternion.Slerp(from, to, t));
            });
        }
        BeginDash(targetingRobot);
        Vector3 direction = CalcDashDirection(targetingRobot);
        while (m_Robot.ChassisInstance.CarPhysics.CarRb.velocity.magnitude > 5f)
        {
            m_Robot.ChassisInstance.CarPhysics.CarRb.MoveRotation(Quaternion.LookRotation(direction));
            m_Robot.ChassisInstance.CarPhysics.CarRb.angularVelocity = Vector3.zero;
            yield return null;
        }
        EndDash(targetingRobot);
    }

    private void HandleDashHitObject(Collider collider)
    {
        if (collider.isTrigger)
            return;
        PBRobot attacker = m_Robot;
        PBRobot receiver = collider?.attachedRigidbody?.GetComponent<PBPart>()?.RobotChassis?.Robot;
        bool isHitTarget = attacker == m_Robot && receiver == m_TargetingRobot && m_TargetingRobot != null;
        bool isHitWall = collider.CompareTag("Wall");
        if (!isHitTarget && !isHitWall)
            return;
        m_DashFrontParticle.Stop(true);
        m_DashFrontParticle.gameObject.SetActive(false);
        m_ImpactParticle.transform.position = m_DashFrontParticle.transform.position;
        m_ImpactParticle.transform.forward = m_DashFrontParticle.transform.forward;
        m_ImpactParticle.Play();
        m_TriggerCallback.onTriggerStay -= HandleDashHitObject;
        LGDebug.Log($"DASH HIT - Collider: {collider} - {collider.attachedRigidbody} - {Time.time}", context: collider);

        if (isHitTarget)
        {
            #region Firebase Event
            try
            {
                if (receiver.PersonalInfo.isLocal && m_ActiveSkillSO != null)
                {
                    string skillName = "null";
                    skillName = m_ActiveSkillSO.GetDisplayName();
                    GameEventHandler.Invoke(LogFirebaseEventCode.AffectedByOpponentSkill, skillName);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion

            Vector3 direction = m_TargetingRobot.GetTargetPoint() - m_Robot.GetTargetPoint();
            Vector3 force = (direction.normalized + Vector3.up * m_ActiveSkillSO.upwardModifier).normalized * m_ActiveSkillSO.impactForce;
            Vector3 torque = Vector3.Cross(-direction, Vector3.up).normalized * m_ActiveSkillSO.impactTorque;
            AirborneEffect airborneEffect = new AirborneEffect(1f, true, force, ForceMode.VelocityChange, torque, ForceMode.VelocityChange, true, m_Robot);
            m_TargetingRobot.CombatEffectController.ApplyEffect(airborneEffect);
            m_TargetingRobot.ChassisInstance.ReceiveDamage(this, 0f);
            if (!(m_TargetingRobot.ActiveSkillCaster != null && m_TargetingRobot.ActiveSkillCaster.GetActiveSkillSO() == m_ActiveSkillSO && m_TargetingRobot.ActiveSkillCaster.skillState == SkillState.Active))
            {
                m_Robot.ChassisInstance.CarPhysics.CarRb.drag *= 2.5f;
                StartCoroutine(CommonCoroutine.Delay(0.5f, false, () => m_Robot.ChassisInstance.CarPhysics.CarRb.drag /= 2.5f));
            }
        }
        if (PBSoundUtility.IsOnSound())
        {
            SoundManager.Instance.PlaySFX(SFX.BlitzDashImpact);
        }
    }

    protected override void SetupRobot()
    {
        base.SetupRobot();
        BoxCollider triggerBoxCollider = new GameObject("ActiveSkill-Dash_TriggerCollider", typeof(BoxCollider), typeof(OnTriggerCallback)).GetComponent<BoxCollider>();
        triggerBoxCollider.transform.position = m_Robot.ChassisInstanceTransform.position;
        triggerBoxCollider.transform.forward = m_Robot.ChassisInstanceTransform.forward;
        triggerBoxCollider = CalculateCombinedCollider(triggerBoxCollider, m_Robot);
        triggerBoxCollider.isTrigger = true;
        triggerBoxCollider.gameObject.layer = Const.UnityLayerMask.Default;
        triggerBoxCollider.transform.SetParent(m_Robot.ChassisInstanceTransform);
        triggerBoxCollider.excludeLayers |= m_Robot.ChassisInstance.CarPhysics.RaycastMask | (1 << m_Robot.RobotLayer);
        OnTriggerCallback triggerCallback = triggerBoxCollider.GetComponent<OnTriggerCallback>();
        triggerCallback.isFilterByTag = false;
        foreach (var collider in m_Robot.ChassisInstance.Colliders)
        {
            Physics.IgnoreCollision(collider, triggerBoxCollider, true);
        }
        m_DashFrontParticle = Instantiate(m_ActiveSkillSO.frontDashParticlePrefab, m_Robot.ChassisInstance.CarPhysics.transform);
        m_DashFrontParticle.transform.localRotation = Quaternion.identity;
        m_DashFrontParticle.transform.localPosition = Vector3.forward * (triggerBoxCollider.size.z / 2f + 0.8f);
        m_DashRearParticle = Instantiate(m_ActiveSkillSO.rearDashParticlePrefab, m_Robot.ChassisInstance.CarPhysics.transform);
        m_DashRearParticle.transform.localRotation = m_ActiveSkillSO.rearDashParticlePrefab.transform.localRotation;
        m_DashRearParticle.transform.localPosition = -Vector3.forward * triggerBoxCollider.size.z / 2f;
        m_TriggerCallback = triggerCallback;
        m_TriggerCallback.gameObject.SetActive(false);
    }

    public override bool IsAbleToPerformSkillForAI()
    {
        return IsAbleToPerformSkill();
    }

    public override bool IsAbleToPerformSkill()
    {
        if (!base.IsAbleToPerformSkill())
            return false;
        // Check nearby enemy
        if (!m_ActiveSkillSO.canDashWithoutNearbyEnemy && m_DetectedClosestRobot == null)
            return false;
        return true;
    }

    public override void PerformSkill()
    {
        base.PerformSkill();
        if (m_DashCoroutine != null)
            StopCoroutine(m_DashCoroutine);
        m_DashCoroutine = StartCoroutine(Dash_CR(m_DetectedClosestRobot));
        m_DetectedClosestRobot = null;
    }

    public override void Initialize(ActiveSkillDashSO activeSkillSO, PBRobot robot)
    {
        base.Initialize(activeSkillSO, robot);
        m_ImpactParticle = Instantiate(m_ActiveSkillSO.impactParticlePrefab);
        m_ImpactParticle.transform.localScale = Vector3.one * m_ActiveSkillSO.impactParticleScale;
        m_RadarDetector = new RadarDetector();
        m_RadarDetector.Initialize(m_Robot.AIBotController, m_ActiveSkillSO.rangeToPerformSkill, 360f, m_ActiveSkillSO.drawGizmos);
    }

    [Button]
    public float GetDamage()
    {
        float rawAttack = m_ActiveSkillSO.damagePercentage * m_Robot.MaxHealth;
        float attackMultiplier = m_Robot.AtkMultiplier;
        return rawAttack * attackMultiplier;
    }
}