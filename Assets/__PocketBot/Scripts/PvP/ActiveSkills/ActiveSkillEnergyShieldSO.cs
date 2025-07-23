using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using HyrphusQ.SerializedDataStructure;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveSkillSO_EnergyShield", menuName = "PocketBots/ActiveSkillSO/EnergyShield")]
public class ActiveSkillEnergyShieldSO : ActiveSkillSO<ActiveSkillEnergyShieldSO, ActiveSkillEnergyShieldCaster>
{
    [Serializable]
    public struct ShieldConfig
    {
        public float m_Radius;
        public Vector3 m_Offset;
    }

    [SerializeField, BoxGroup("Debug"), PropertyOrder(100)]
    private bool m_DrawGizmos;
    [SerializeField]
    private float m_InvincibleDuration = 3f;
    [SerializeField]
    private float m_AirborneEffectDuration = 2f;
    [SerializeField]
    private float m_DamageExplosionForce = 30f;
    [SerializeField]
    private float m_DamageExplosionRadius = 4f;
    [SerializeField]
    private float m_ExplosionForceUpwardsModifier = 2f;
    [SerializeField, Range(0f, 1f)]
    private float m_DamagePercentage = 0.05f;
    [SerializeField]
    private SerializedDictionary<PBChassisSO, ShieldConfig> m_OverrideShieldConfigDictionary;
    [SerializeField]
    private EnergyShield m_EnergyShieldPrefab;

    public bool drawGizmos => m_DrawGizmos;
    public float invincibleDuration => m_InvincibleDuration;
    public float airborneEffectDuration => m_AirborneEffectDuration;
    public float damageExplosionForce => m_DamageExplosionForce;
    public float damageExplosionRadius => m_DamageExplosionRadius;
    public float explosionForceUpwardsModifier => m_ExplosionForceUpwardsModifier;
    public float damagePercentage => m_DamagePercentage;
    public override float activeDuration => invincibleDuration;
    public EnergyShield energyShieldPrefab => m_EnergyShieldPrefab;

    public bool TryGetOverrideShieldConfig(PBChassisSO chassisSO, out ShieldConfig config)
    {
        return m_OverrideShieldConfigDictionary.TryGetValue(chassisSO, out config);
    }
}
public class ActiveSkillEnergyShieldCaster : ActiveSkillCaster<ActiveSkillEnergyShieldSO>, IAttackable
{
    private float m_TimeSinceCastSkill;
    // private SphereCollider m_ShieldCollider;
    private ActiveSkillEnergyShieldSO.ShieldConfig m_ShieldConfig;
    private RadarDetector m_RadarDetector;
    private EnergyShield m_EnergyShield;

    public override float remainingActiveTime => Time.time - m_TimeSinceCastSkill;

    private void Start()
    {
        if (!m_ActiveSkillSO.TryGetOverrideShieldConfig(mRobot.ChassisInstance.ChassisSO, out m_ShieldConfig))
        {
            Bounds worldBounds = mRobot.ChassisInstance.CarPhysics.WorldBounds;
            float radius = Vector3.Distance(worldBounds.min, worldBounds.max);
            m_ShieldConfig = new ActiveSkillEnergyShieldSO.ShieldConfig()
            {
                m_Radius = radius,
                m_Offset = Vector3.zero,
            };
            //LGDebug.Log($"Radius: {radius} - {Vector3.Distance(worldBounds.min, worldBounds.max)} - {worldBounds.min} - {worldBounds.max}");
        }
        m_EnergyShield.transform.localScale = m_ShieldConfig.m_Radius / 3f * Vector3.one;
    }

    protected override void Update()
    {
        base.Update();
        m_EnergyShield.transform.position = mRobot.ChassisInstanceTransform.TransformPoint(mRobot.ChassisInstanceTransform.InverseTransformPoint(mRobot.ChassisInstance.CarPhysics.WorldBounds.center) + m_ShieldConfig.m_Offset);
    }

    private void OnDrawGizmos()
    {
        m_RadarDetector.InvokeMethod("OnDrawGizmos");
    }

    private LayerMask GetDamagableObjectLayers()
    {
        int layers = Physics.DefaultRaycastLayers ^ LayerMask.GetMask("Ground");
        foreach (var robot in PBRobot.allFightingRobots)
        {
            layers ^= 1 << robot.RobotLayer;
        }
        return layers;
    }

    private IEnumerator CastShieldSkill_CR()
    {
        SoundManager.Instance.PlaySFX(SFX.ShieldActive);
        m_TimeSinceCastSkill = Time.time;
        m_Robot.CombatEffectController.ApplyEffect(new InvincibleEffect(m_ActiveSkillSO.invincibleDuration, true, null));
        // m_ShieldCollider.gameObject.SetActive(true);
        m_EnergyShield.CreateShield();
        yield return Yielders.Get(m_ActiveSkillSO.invincibleDuration);
        SoundManager.Instance.PlaySFX(SFX.ShieldBlast);
        List<(PBRobot, float)> robotDistancePairs = new List<(PBRobot, float)>(m_RadarDetector.ScanAllRobotsInDetectArea());
        List<(IDamagable, float)> damagableObjectDistancePairs = m_RadarDetector.ScanAllObjectsInDetectArea<IDamagable>(GetDamagableObjectLayers(), predicate: item => item is not PBPart);
        for (int i = 0; i < robotDistancePairs.Count; i++)
        {
            PBRobot robot = robotDistancePairs[i].Item1;
            robot.ChassisInstance.ReceiveDamage(this, 0f);
            robot.CombatEffectController.ApplyEffect(new AirborneEffect(m_ActiveSkillSO.airborneEffectDuration, true, m_ActiveSkillSO.damageExplosionForce, mRobot.GetTargetPoint(), m_ActiveSkillSO.damageExplosionRadius, m_ActiveSkillSO.explosionForceUpwardsModifier, ForceMode.VelocityChange, true, mRobot));
        }
        for (int i = 0; i < damagableObjectDistancePairs.Count; i++)
        {
            damagableObjectDistancePairs[i].Item1.ReceiveDamage(this, 0f);
        }
        m_EnergyShield.Explode(m_Robot.ChassisInstance.CarPhysics.WorldBounds.center - m_Robot.ChassisInstance.CarPhysics.WorldBounds.extents.y * Vector3.up * 0.5f);
        // m_ShieldCollider.gameObject.SetActive(false);
        remainingCooldown = m_ActiveSkillSO.cooldown;
    }

    public override void PerformSkill()
    {
        base.PerformSkill();
        StartCoroutine(CastShieldSkill_CR());
    }

    public override bool IsAbleToPerformSkillForAI()
    {
        return false;
    }

    public override void Initialize(ActiveSkillEnergyShieldSO activeSkillSO, PBRobot robot)
    {
        base.Initialize(activeSkillSO, robot);
        m_RadarDetector = new RadarDetector();
        m_RadarDetector.Initialize(m_Robot.AIBotController, m_ActiveSkillSO.damageExplosionRadius, 360f, m_ActiveSkillSO.drawGizmos);
        // m_ShieldCollider = new GameObject("EnergyShield_Collider").AddComponent<SphereCollider>();
        // m_ShieldCollider.transform.parent = m_Robot.ChassisInstance.AntiSlidingBox.transform;
        // m_ShieldCollider.transform.position = m_Robot.ChassisInstance.CarPhysics.CarRb.worldCenterOfMass;
        // m_ShieldCollider.radius = m_ActiveSkillSO.shieldRadius;
        // m_ShieldCollider.gameObject.SetActive(false);
        // m_ShieldCollider.gameObject.layer = m_Robot.RobotLayer;
        // m_ShieldCollider.gameObject.tag = m_Robot.gameObject.tag;
        m_EnergyShield = Instantiate(m_ActiveSkillSO.energyShieldPrefab, mRobot.transform);
        m_EnergyShield.Initialize(m_ActiveSkillSO.damageExplosionRadius);
        m_EnergyShield.transform.ResetTransform();
    }

    public float GetDamage()
    {
        float rawAttack = m_ActiveSkillSO.damagePercentage * m_Robot.MaxHealth;
        float attackMultiplier = m_Robot.AtkMultiplier;
        return rawAttack * attackMultiplier;
    }
}