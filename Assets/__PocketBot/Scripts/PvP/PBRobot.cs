using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;
using System.Linq;
using UnityEngine.Animations;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using HyrphusQ.Helpers;
using LatteGames;
using FIMSpace.FProceduralAnimation;

[EventCode]
public enum RobotStatusEventCode
{
    /// <summary>
    /// Raised when bot model has been build (spawned)
    /// <para><typeparamref name="PBRobot"/>: PBRobot script</para>
    /// <para><typeparamref name="GameObject"/>: Bot Chassis GameObject</para>
    /// <para><typeparamref name="isInit"/>: Bot Chassis GameObject</para>
    /// </summary>
    OnModelSpawned,
    /// <summary>
    /// Raised when bot model got destroy (despawned)
    /// <para><typeparamref name="PBRobot"/>: PBRobot script</para>
    /// </summary>
    OnModelDespawned,
    /// <summary>
    /// <para><typeparamref name="PBChassis"/>: Chassis is taken damage (Receiver)</para>
    /// <para><typeparamref name="PBChassis"/>: Chassis caused damage (Attacker)</para>
    /// <para><typeparamref name="Float"/>: Force taken amount</para>
    /// </summary>
    OnRobotReceiveForce,
    /// <summary>
    /// Raised when a robot taken damaged
    /// <para><typeparamref name="PBRobot"/>: Taken damaged robot</para>
    /// <para><typeparamref name="Float"/>: Damage taken amount</para>
    /// <para><typeparamref name="Float"/>: Force taken amount</para>
    /// <para><typeparamref name="IAttackable"/>: Attacker</para>
    /// </summary>
    OnRobotDamaged,
    /// <summary>
    /// Raised when a robot can't move
    /// <para><typeparamref name="PBRobot"/>: Immobilized robot</para>
    /// </summary>
    OnRobotImmobilized,
    /// <summary>
    /// Raised when a robot recovered from immobilized
    /// <para><typeparamref name="PBRobot"/>: Robot that recovered from immobilized</para>
    /// </summary>
    OnRobotRecoveredFromImmobilized,
    /// <summary>
    /// Raised when a robot recovered from immobilized
    /// <para><typeparamref name="bool"/>: Is visible on screen</para>
    /// <para><typeparamref name="int"/>: Object ID</para>
    /// </summary>
    OnRobotVisibleOnScreen,
    /// <summary>
    /// Raised when an combat effect is applied to a robot
    /// <para><typeparamref name="PBRobot"/>: Robot</para>
    /// <para><typeparamref name="CombatEffect"/>: Combat effect</para>
    /// </summary>
    OnRobotEffectApplied,
    /// <summary>
    /// Raised when a combat effect is removed from a robot
    /// <para><typeparamref name="PBRobot"/>: Robot </para>
    /// <para><typeparamref name="CombatEffect"/>: Combat effect</para>
    /// </summary>
    OnRobotEffectRemoved
}

public class PBRobot : Competitor, INavigationPoint, ICombatEntity
{
    public static List<PBRobot> allRobots { get; private set; } = new List<PBRobot>();
    public static List<PBRobot> allFightingRobots { get; private set; } = new List<PBRobot>();

    [SerializeField] protected bool buildOnStart = false;
    [SerializeField] protected bool isPreview = false;
    [SerializeField] protected float explodeForce = 100f;
    [SerializeField] protected CarConfigSO carConfigSO;
    [SerializeField] protected ItemSOVariable currentChassisInUse;
    [SerializeField] protected PBRobotStatsSO robotStats;
    [SerializeField] protected AnimationCurveVariable forceAdjustmentAnimCurve;

    protected float m_AssistWindowSeconds = 4f;
    protected int m_TeamId = 0;
    protected bool isMatchCompleted = false;
    protected StackedBool isDisarmed = new StackedBool(false);
    protected float atkMultiplier = 1f;
    protected float movementSpeedMultiplier = 1f;
    protected float rotationSpeedMultiplier = 1f;
    protected float highestOverallScore;
    protected PBChassis chassisInstance;
    protected PBRobotStatusVFX robotStatusVFX;
    protected PBLevelController levelController;
    protected AIBotController aiBotController;
    protected BotController botController;
    protected ActiveSkillCaster activeSkillCaster;
    protected CombatEffectController combatEffectController;
    protected List<Part> partInstances = new();
    protected Dictionary<PvPBoosterType, int> consumedBoosterDictionary = new Dictionary<PvPBoosterType, int>();
    [ShowInInspector] protected PlayerKDA m_PlayerKDA;
    [ShowInInspector] protected Dictionary<PlayerKDA, DateTime> m_RecentDamagers = new Dictionary<PlayerKDA, DateTime>();
    #region ICombatEntity
    float ICombatEntity.maxHealth { get => MaxHealth; set => MaxHealth = value; }
    float ICombatEntity.currentHealth { get => Health; set => Health = value; }
    float ICombatEntity.damageMultiplier { get => AtkMultiplier; set => AtkMultiplier = value; }
    float ICombatEntity.damageTakenMultiplier { get => DamageTakenMultiplier; set => DamageTakenMultiplier = value; }
    float ICombatEntity.movementSpeedMultiplier { get => MovementSpeedMultiplier; set => MovementSpeedMultiplier = value; }
    float ICombatEntity.rotationSpeedMultiplier { get => RotationSpeedMultiplier; set => RotationSpeedMultiplier = value; }
    bool ICombatEntity.isStunned { get => !ChassisInstance.CarPhysics.CanMove; set => ChassisInstance.CarPhysics.CanMove = !value; }
    bool ICombatEntity.isDisarmed { get => IsDisarmed; set => this.EnabledAllParts(!value); }
    bool ICombatEntity.isInvincible { get => IsInvincible; set => IsInvincible = value; }
    Rigidbody ICombatEntity.rigidbody { get => ChassisInstance.CarPhysics.CarRb; }
    #endregion
    public PlayerKDA PlayerKDA => m_PlayerKDA;
    [ShowInInspector, ReadOnly]
    public int TeamId
    {
        get => m_TeamId;
        set => m_TeamId = value;
    }
    public bool IsPreview => isPreview;
    public bool IsDisarmed
    {
        get
        {
            return isDisarmed;
        }
        set
        {
            isDisarmed.value = value;
        }
    }
    public bool IsInvincible { get; set; } = false;
    public float LastTimeCauseDamage { get; set; }
    public float LastTimeReceiveDamage { get; set; }
    public float AtkMultiplier
    {
        get => atkMultiplier;
        set
        {
            atkMultiplier = value;
        }
    }
    public float MovementSpeedMultiplier
    {
        get => movementSpeedMultiplier;
        set
        {
            movementSpeedMultiplier = value;
        }
    }
    public float RotationSpeedMultiplier
    {
        get => rotationSpeedMultiplier;
        set
        {
            rotationSpeedMultiplier = value;
        }
    }
    public float DamageTakenMultiplier { get; set; } = 1f;
    public float SizeMultiplier { get; set; } = 1f;
    public float HighestOverallScore => highestOverallScore;
    public int RobotLayer => gameObject.layer;
    public float ExplodeForce => explodeForce;
    public CombatEffectStatuses CombatEffectStatuses
    {
        get
        {
            CombatEffectStatuses combatEffectStatuses = CombatEffectController.combatEffectStatuses;
            if (ChassisInstance.CarPhysics.IsImmobilized)
                combatEffectStatuses |= CombatEffectStatuses.Immobilized;
            return combatEffectStatuses;
        }
    }
    public PBRobot LastRobotCauseDamageToMe { get; set; }
    public PBRobotStatsSO RobotStatsSO => robotStats;
    public PBChassis ChassisInstance => chassisInstance;
    public Transform ChassisInstanceTransform => chassisInstance != null ? chassisInstance.RobotBaseBody.transform : null;
    public AnimationCurve ForceAdjustmentCurve => forceAdjustmentAnimCurve;
    public AIBotController AIBotController
    {
        get
        {
            if (aiBotController == null)
            {
                aiBotController = GetComponent<AIBotController>();
            }
            return aiBotController;
        }
    }
    public BotController BotController
    {
        get
        {
            if (botController == null)
            {
                botController = GetComponent<BotController>();
            }
            return botController;
        }
    }
    public ActiveSkillCaster ActiveSkillCaster => activeSkillCaster;
    public CombatEffectController CombatEffectController
    {
        get
        {
            if (combatEffectController == null)
                combatEffectController = GetComponent<CombatEffectController>();
            return combatEffectController;
        }
    }
    public List<Part> PartInstances => partInstances;

    protected virtual void Awake()
    {
        allRobots.Add(this);
        if (!isPreview)
        {
            levelController = ObjectFindCache<PBLevelController>.Get(isCallFromAwake: true);
            allFightingRobots.Add(this);
            CombatEffectController.onEffectApplied += HandleEffectApplied;
            CombatEffectController.onEffectRemoved += HandleEffectRemoved;
        }
        if (levelController != null)
        {
            levelController.OnRemoveAliveCompetitor += HandleRoundComplete;
        }
    }

    protected virtual void Start()
    {
        OnHealthChanged += HandleHealthChanged;
        if (buildOnStart == true)
            BuildRobot();

        if (m_PlayerKDA == null)
            m_PlayerKDA = new PlayerKDA(this.name);

        if (m_RecentDamagers == null)
            m_RecentDamagers = new Dictionary<PlayerKDA, DateTime>();
    }

    protected virtual void OnDestroy()
    {
        allRobots.Remove(this);
        allFightingRobots.Remove(this);
        DestroyRobot();
        OnHealthChanged -= HandleHealthChanged;
        UnsubcribePartEvents();
        if (levelController != null)
        {
            levelController.OnRemoveAliveCompetitor -= HandleRoundComplete;
        }
        if (!isPreview)
        {
            CombatEffectController.onEffectApplied -= HandleEffectApplied;
            CombatEffectController.onEffectRemoved -= HandleEffectRemoved;
        }
    }

    protected void HandleEffectApplied(CombatEffect combatEffect)
    {
        GameEventHandler.Invoke(RobotStatusEventCode.OnRobotEffectApplied, this, combatEffect);
    }

    protected void HandleEffectRemoved(CombatEffect combatEffect)
    {
        GameEventHandler.Invoke(RobotStatusEventCode.OnRobotEffectRemoved, this, combatEffect);
    }

    protected void HandleRoundComplete()
    {
        if (levelController != null && levelController.AliveCompetitors.Count <= 1)
        {
            isMatchCompleted = true;
        }
    }

    protected void BuildPart(PBPartSlot slotType)
    {
        if (chassisInstance == null) return;
        if (partInstances.Any(part => part.PartSlotType.Equals(slotType)) == false) return;
        var partInstance = partInstances.Find(part => part.PartSlotType.Equals(slotType));
        var partSO = chassisInstance.PartSO.Cast<PBChassisSO>().AllPartSlots.Find(slot => slot.PartSlotType.Equals(slotType)).PartVariableSO.value.Cast<PBPartSO>();
        var partPrefab = partSO.GetModelPrefab<PBPart>();
        var containers = chassisInstance.PartContainers.Find(container => container.PartSlotType.Equals(slotType)).Containers;

        foreach (var instance in partInstance.Parts)
        {
            Destroy(instance.gameObject);
        }
        partInstance.Parts.Clear();

        foreach (var container in containers)
        {
            var pbPart = Instantiate(partPrefab, container);
            pbPart.transform.localPosition = Vector3.zero;
            // if (pbPart.TryGetComponentInChildren(out Joint joint))
            // {
            //     joint.connectedBody = chassisInstance.RobotBaseBody;
            //     joint.autoConfigureConnectedAnchor = false;
            //     joint.connectedAnchor = container.localPosition;
            //     if (isPreview) joint.connectedMassScale = 0.001f;
            // }
            var partComponents = pbPart.GetComponentsInChildren<PBPart>();
            foreach (var component in partComponents)
            {
                component.PartSO = partSO;
                if (robotStats != null && PersonalInfo.isLocal == false && robotStats.statsOfRobot.ContainsKey(slotType)) component.ManualRobotStats = robotStats.statsOfRobot[slotType];
                component.Resistance = partSO.GetStat(PBStatID.Resistance);
                component.OnPartReceiveDamage += HandlePartReceiveDamage;
                component.OnPartCauseDamage += HandlePartCauseDamage;
            }
            pbPart.gameObject.SetLayer(RobotLayer, true);
            partInstance.Parts.Add(pbPart);
            var colliders = pbPart.GetComponentsInChildren<Collider>();
            if (isPreview)
            {
                foreach (var collider in colliders)
                {
                    collider.enabled = false;
                }
            }
        }

        // if (slotType.GetPartTypeOfPartSlot().Equals(PBPartType.Wheels))
        // {
        //     chassisInstance.CarPhysics.SetTireConfig(slotType, partSO.Cast<PBWheelSO>().TireConfigSO);
        //     if (chassisInstance.ChassisSO.GetWheelAmount() == 2 && slotType.Equals(PBPartSlot.Wheels_1))
        //     {
        //         chassisInstance.CarPhysics.SetTireConfig(PBPartSlot.Wheels_2, partSO.Cast<PBWheelSO>().TireConfigSO);
        //         chassisInstance.CarPhysics.SetTireConfig(PBPartSlot.Wheels_3, partSO.Cast<PBWheelSO>().TireConfigSO);
        //     }
        // }
    }

    protected void HandlePartReceiveDamage(PBPart.PartDamagedEventData data)
    {
        if (isMatchCompleted || IsDead || IsInvincible)
            return;
        PBPart attackerPart = null;
        if (data.attacker is PBPart part && part.RobotChassis != null)
        {
            LastRobotCauseDamageToMe = part.RobotChassis.Robot;
            attackerPart = part;
        }
        else if (data.attacker is ActiveSkillCaster activeSkillCaster)
        {
            LastRobotCauseDamageToMe = activeSkillCaster.mRobot;
        }
        LastTimeReceiveDamage = Time.time;
        Health -= data.damageTaken;
        if (Health <= 0 && attackerPart != null)
        {
            var killerStats = attackerPart.RobotChassis.Robot.PlayerKDA;
            killerStats.AddKill();

            HandleAssist(killerStats);
        }
        else
        {
            if (attackerPart != null)
            {
                var attackerStats = attackerPart.RobotChassis.Robot.PlayerKDA;
                if (m_RecentDamagers.ContainsKey(attackerStats))
                {
                    m_RecentDamagers[attackerStats] = DateTime.Now;
                }
                else
                {
                    m_RecentDamagers.Add(attackerStats, DateTime.Now);
                }
            }
        }

        NotifyOnRobotDamaged(data.damageTaken, data.forceTaken, data.attacker);

        void HandleAssist(PlayerKDA killer)
        {
            var now = DateTime.Now;
            foreach (var entry in m_RecentDamagers)
            {
                PlayerKDA assister = entry.Key;
                DateTime damageTime = entry.Value;

                if (assister == killer) continue;

                TimeSpan timeSinceDamage = now - damageTime;
                if (timeSinceDamage.TotalSeconds <= m_AssistWindowSeconds)
                    assister.AddAssist();
            }

            m_RecentDamagers.Clear();
        }
    }

    protected void HandlePartCauseDamage(PBPart.PartDamagedEventData data)
    {
        if (isMatchCompleted)
            return;
        LastTimeCauseDamage = Time.time;
    }

    protected void NotifyOnRobotDamaged(float damage, float force, IAttackable attacker)
    {
        GameEventHandler.Invoke(RobotStatusEventCode.OnRobotDamaged, this, damage, force, attacker);
    }

    protected void NotifyOnRobotImmobilized()
    {
        GameEventHandler.Invoke(RobotStatusEventCode.OnRobotImmobilized, this);
    }

    protected void NotifyOnRobotRecoveredFromImmobilized()
    {
        GameEventHandler.Invoke(RobotStatusEventCode.OnRobotRecoveredFromImmobilized, this);
    }

    protected void HandleHealthChanged(HealthChangedEventData data)
    {
        if (!IsDead)
        {
            if (robotStatusVFX == null)
                return;
            if (health < (66f * maxHealth / 100f))
            {
                robotStatusVFX.EnableVFX(PBRobotStatusVFX.VFXEnum.Smoke);
            }
            if (health <= (33f * maxHealth / 100f))
            {
                robotStatusVFX.EnableVFX(PBRobotStatusVFX.VFXEnum.Fire);
            }
        }
        else
        {
            if (robotStatusVFX != null)
                robotStatusVFX.EnableVFX(PBRobotStatusVFX.VFXEnum.Explosion);
            chassisInstance.CarPhysics.enabled = false;
            chassisInstance.RobotBaseBody.mass = 1;
            chassisInstance.RobotBaseBody.drag = 1;
            var randomForceDir = new Vector3(Random.Range(-1, 1), 1, Random.Range(-1, 1));
            chassisInstance.RobotBaseBody.AddForce(randomForceDir * explodeForce, ForceMode.Impulse);
            UnsubcribePartEvents();
            SetActiveConstraints();
            var reviveSystem = ObjectFindCache<ReviveSystem>.Get();
            if (reviveSystem != null && reviveSystem.isAbleToRevive)
            {
                reviveSystem.onReviveDecisionMade += OnReviveDecisionMade;

                void OnReviveDecisionMade(bool isRevive)
                {
                    reviveSystem.onReviveDecisionMade -= OnReviveDecisionMade;

                    if (!isRevive)
                        GameEventHandler.Invoke(PBPvPEventCode.OnAnyPlayerDied, this);
                }
            }
            else
            {
                GameEventHandler.Invoke(PBPvPEventCode.OnAnyPlayerDied, this);
            }

            if (m_PlayerKDA != null)
                m_PlayerKDA.AddDeath();
        }
    }

    protected void SetActiveConstraints(bool isActive = false)
    {
        var constraints = GetComponentsInChildren<ParentConstraint>();
        foreach (var constraint in constraints)
        {
            constraint.enabled = isActive;
        }
    }

    protected void UnsubcribePartEvents()
    {
        if (chassisInstance != null)
        {
            chassisInstance.CarPhysics.OnCarImmobilized -= NotifyOnRobotImmobilized;
            chassisInstance.CarPhysics.OnCarRecoveredFromImmobilized -= NotifyOnRobotRecoveredFromImmobilized;
            chassisInstance.UnsubscribeEvents();
        }
    }

    [Button]
    public virtual PBRobot BuildRobot(bool isInit = true)
    {
        if (chassisInstance != null)
        {
            DestroyRobot();
            GameEventHandler.Invoke(RobotStatusEventCode.OnModelDespawned, this);
        }
        if (m_HealCoroutine != null)
            StopCoroutine(m_HealCoroutine);

        PBChassisSO pbChassisSO = currentChassisInUse.value.Cast<PBChassisSO>();
        PBChassis chassisPrefab = pbChassisSO.GetModelPrefab<PBChassis>();
        List<BotPartSlot> partSlots = pbChassisSO.AllPartSlots;

        if (SceneManager.GetActiveScene().name == SceneName.MainScene.ToString() && pbChassisSO.IsSpecial && pbChassisSO.SpecialPreviewChassisPrefab != null)
            chassisPrefab = pbChassisSO.SpecialPreviewChassisPrefab;

        SpawnChassis();
        SpawnBotParts();

        if (isInit)
        {
            SetupSkill();
            SetBotHealth();
        }

        GameEventHandler.Invoke(RobotStatusEventCode.OnModelSpawned, this, chassisInstance.RobotBaseBody.gameObject, isInit);

        NotifyCompetitorJoined();

        void SpawnChassis()
        {
            chassisInstance = Instantiate(chassisPrefab, transform);
            robotStatusVFX = chassisInstance.PbRobotStatusVFX;
            chassisInstance.RobotBaseBody.name = name;
            chassisInstance.gameObject.SetLayer(RobotLayer, true);
            chassisInstance.CarPhysics.CarRb.mass = pbChassisSO.Mass;
            chassisInstance.CarPhysics.CarTopSpeedMultiplier = pbChassisSO.Speed;
            chassisInstance.CarPhysics.Turning = pbChassisSO.Turning;
            chassisInstance.CarPhysics.SteeringSpeed = pbChassisSO.SteeringSpeed;
            chassisInstance.PartSO = pbChassisSO;
            chassisInstance.Resistance = pbChassisSO.GetStat(PBStatID.Resistance);
            chassisInstance.OnPartReceiveDamage += HandlePartReceiveDamage;
            chassisInstance.OnPartCauseDamage += HandlePartCauseDamage;
            chassisInstance.GetComponentInChildren<ConnectedPart>().OnPartReceiveDamage += HandlePartReceiveDamage;
            chassisInstance.GetComponentInChildren<ConnectedPart>().OnPartCauseDamage += HandlePartCauseDamage;
            chassisInstance.CarPhysics.CarConfigSO = carConfigSO;
            chassisInstance.CarPhysics.IsPreview = isPreview;
            if (isPreview == false)
            {
                chassisInstance.CarPhysics.OnCarImmobilized += NotifyOnRobotImmobilized;
                chassisInstance.CarPhysics.OnCarRecoveredFromImmobilized += NotifyOnRobotRecoveredFromImmobilized;
            }
            if (isInit == false)
            {
                chassisInstance.CarPhysics.CanMove = true;
                HandleHealthChanged(null); //Check bot health again
            }
            partInstances.Add(new Part(PBPartSlot.Body, new() { chassisInstance }));
        }

        void SpawnBotParts()
        {
            for (int i = 0; i < partSlots.Count; i++)
            {
                if (partSlots[i].PartVariableSO == null) continue;
                if (partSlots[i].PartVariableSO.value == null) continue;
                Part part = new Part(partSlots[i].PartSlotType, new());
                partInstances.Add(part);
                BuildPart(partSlots[i].PartSlotType);
                if (pbChassisSO.IsSpecial)
                {
                    LegsAnimator legsAnimator = GetComponentInChildren<LegsAnimator>();
                    if (legsAnimator != null && chassisInstance.GetComponent<BodyTransformer>() == null)
                    {
                        if (partSlots[i].PartSlotType == PBPartSlot.Wheels_1)
                        {
                            part.Parts.Clear();
                            for (int j = 0; j < 2; j++)
                            {
                                part.Parts.Add(legsAnimator.Legs[j].BoneStart.GetOrAddComponent<PBPart>());
                            }
                        }
                        else if (partSlots[i].PartSlotType == PBPartSlot.Wheels_2)
                        {
                            part.Parts.Clear();
                            for (int j = 2; j < 4; j++)
                            {
                                part.Parts.Add(legsAnimator.Legs[j].BoneStart.GetOrAddComponent<PBPart>());
                            }
                        }
                    }
                }
            }
        }

        void SetBotHealth()
        {
            if (robotStats == null) return;
            float totalHealth = robotStats.stats.GetHealth().value;
            MaxHealth = totalHealth;
        }

        void SetupSkill()
        {
            if (!IsPreview && robotStats.skillInUse != null && robotStats.skillInUse.value != null)
            {
                activeSkillCaster = robotStats.skillInUse.value.Cast<ActiveSkillSO>().SetupSkill(this);
            }
        }

        return this;
    }

    public virtual void DestroyRobot()
    {
        if (chassisInstance == null)
            return;
        chassisInstance.CarPhysics.OnCarImmobilized -= NotifyOnRobotImmobilized;
        chassisInstance.CarPhysics.OnCarRecoveredFromImmobilized -= NotifyOnRobotRecoveredFromImmobilized;
        if (chassisInstance.CarPhysics != null)
            Destroy(chassisInstance.CarPhysics.gameObject);
        Destroy(chassisInstance.gameObject);
        foreach (var part in partInstances)
        {
            foreach (var instance in part.Parts)
            {
                if (instance != null)
                    Destroy(instance.gameObject);
            }
        }
        partInstances.Clear();
    }

    public void RebuildPart(PBPartSlot slotType)
    {
        if (chassisInstance == null) return;
        if (slotType == PBPartSlot.Body)
        {
            BuildRobot();
            return;
        }
        if (chassisInstance.PartContainers.Any(part => part.PartSlotType.Equals(slotType)))
        {
            BuildPart(slotType);
        }
        else
        {
            Debug.LogError($"There's no slot {slotType} on the chassis");
        }
    }

    #region Healing Action
    private Coroutine m_HealCoroutine;
    private GameObject m_SpanerPrefab;
    private EZAnimVector3 m_SpanerAnimEZ;
    private ParticleSystem m_HealingParticlePrefabs;
    private List<ParticleSystem> m_HealingParticles;
    private float m_PendingHealAmount = 0;

    public void StartHealing(ParticleSystem healingParticle, GameObject spanerPrefab, float totalHealPercent, float healRatePercent, float interval)
    {
        if (m_HealingParticlePrefabs == null && healingParticle != null)
            m_HealingParticlePrefabs = Instantiate(healingParticle, chassisInstance.CarPhysics.transform);
        if (m_SpanerPrefab == null && spanerPrefab != null)
        {
            m_SpanerPrefab = Instantiate(spanerPrefab, chassisInstance.CarPhysics.transform);
            m_SpanerPrefab.SetActive(false);

            EZAnimVector3 animVector3 = m_SpanerPrefab.GetComponent<EZAnimVector3>();
            if (animVector3 != null)
                m_SpanerAnimEZ = animVector3;
        }

        float additionalHeal = totalHealPercent * MaxHealth;
        m_PendingHealAmount += additionalHeal;

        if (m_HealCoroutine == null)
            m_HealCoroutine = StartCoroutine(HealOverTime(healRatePercent, interval));
    }

    private IEnumerator HealOverTime(float healRatePercent, float interval)
    {
        while (m_PendingHealAmount > 0)
        {
            if (IsDead)
            {
                m_HealCoroutine = null;
                yield break;
            }
            float healThisTick = healRatePercent * MaxHealth;
            float healAmount = Mathf.Min(healThisTick, m_PendingHealAmount);

            float newHealth = Mathf.Min(Health + healAmount, MaxHealth);
            float actualHealed = newHealth - Health;

            Health = newHealth;
            m_PendingHealAmount -= healAmount;

            if (m_PendingHealAmount <= 0)
                break;
            ActiveHealingVFX();
            yield return new WaitForSeconds(interval);
        }

        m_HealCoroutine = null;
    }


    #endregion

    private void ActiveHealingVFX()
    {
        if (m_HealingParticles == null)
        {
            m_HealingParticles = new List<ParticleSystem>();
        }

        ParticleSystem healingParticle = null;
        m_HealingParticles = m_HealingParticles.Where(v => v != null).ToList();
        for (int i = 0; i < m_HealingParticles.Count; i++)
        {
            if (!m_HealingParticles[i].isPlaying)
            {
                healingParticle = m_HealingParticles[i];
                break;
            }
        }

        if (healingParticle == null)
        {
            healingParticle = Instantiate(m_HealingParticlePrefabs, chassisInstance.CarPhysics.transform);
            m_HealingParticles.Add(healingParticle);
        }

        PlayHealingVFX(healingParticle);

        void PlayHealingVFX(ParticleSystem healingParticle)
        {
            Transform pointFix = null;

            var parts = partInstances
                .Where(v => v.PartSlotType.GetPartTypeOfPartSlot() != PBPartType.Body &&
                            v.PartSlotType.GetPartTypeOfPartSlot() != PBPartType.Wheels)
                .SelectMany(v => v.Parts)
                .ToList();

            pointFix = parts.Any() ? parts.GetRandom().transform : partInstances
                .Where(v => v.PartSlotType.GetPartTypeOfPartSlot() == PBPartType.Wheels)
                .SelectMany(v => v.Parts)
                .ToList()
                .GetRandom()
                .transform;


            m_SpanerPrefab.transform.position = pointFix.position;
            m_SpanerPrefab.transform.position = new Vector3(m_SpanerPrefab.transform.position.x, m_SpanerPrefab.transform.position.y + 0.3f, m_SpanerPrefab.transform.position.z);

            m_SpanerPrefab.SetActive(true);
            healingParticle.transform.localPosition = Vector3.zero;
            healingParticle.Play();

            m_SpanerAnimEZ.Play(() =>
            {
                m_SpanerAnimEZ.InversePlay(() =>
                {
                    if (m_SpanerPrefab != null)
                        m_SpanerPrefab.SetActive(false);
                });
            });
        }

    }

    public float GetTotalHealth(PBChassisSO chassisSO)
    {
        float totalHealth = chassisSO.GetStat(PBStatID.Health);
        foreach (var part in chassisSO.AllPartSlots)
        {
            if (part.PartVariableSO == null) continue;
            if (part.PartVariableSO.value == null) continue;
            totalHealth += part.PartVariableSO.value.Cast<PBPartSO>().GetStat(PBStatID.Health);
        }
        return totalHealth;
    }

    public float GetTotalATK(PBChassisSO chassisSO)
    {
        float totalAtk = chassisSO.GetStat(PBStatID.Attack);
        foreach (var part in chassisSO.AllPartSlots)
        {
            if (part.PartVariableSO == null) continue;
            if (part.PartVariableSO.value == null) continue;
            totalAtk += part.PartVariableSO.value.Cast<PBPartSO>().GetStat(PBStatID.Attack);
        }
        return totalAtk;
    }

    public float GetAllPartPower(PBChassisSO chassisSO)
    {
        float totalPower = 0f;
        foreach (var part in chassisSO.AllPartSlots)
        {
            if (part.PartVariableSO == null) continue;
            if (part.PartVariableSO.value == null) continue;
            totalPower += part.PartVariableSO.value.Cast<PBPartSO>().GetStat(PBStatID.Power);
        }
        return totalPower;
    }

    public float GetChassisPower(PBChassisSO chassisSO)
    {
        float chassisPower = chassisSO.GetStat(PBStatID.Power);
        return chassisPower;

    }

    public void SetInfo(PlayerInfoVariable playerInfoVariable)
    {
        this.playerInfoVariable = playerInfoVariable;
        name = $"Robot_{playerInfoVariable.value.personalInfo.name}";
        robotStats = playerInfoVariable.value.Cast<PBPlayerInfo>().robotStatsSO;
        currentChassisInUse = robotStats.chassisInUse;
    }

    public void SetInfo(PlayerInfoVariable playerInfoVariable, ItemSOVariable chassisSOVariable)
    {
        this.playerInfoVariable = playerInfoVariable;
        robotStats = playerInfoVariable.value.Cast<PBPlayerInfo>().robotStatsSO;
        currentChassisInUse = chassisSOVariable;
    }

    public int GetTotalNumOfConsumedBoosters()
    {
        int total = 0;
        foreach (var item in consumedBoosterDictionary)
        {
            total += item.Value;
        }
        return total;
    }

    public int GetNumOfConsumedBoosters(PvPBoosterType boosterType)
    {
        return consumedBoosterDictionary.Get(boosterType);
    }

    public void ConsumeBooster(PvPBoosterType boosterType)
    {
        consumedBoosterDictionary.Set(boosterType, consumedBoosterDictionary.Get(boosterType) + 1);
    }

    public bool IsAvailable()
    {
        return !IsDead;
    }

    public bool IsRobotReached(PBRobot robot)
    {
        return robot.AIBotController.RadarDetector.IsRobotInDetectArea(this);
    }

    public PointType GetPointType()
    {
        return PointType.OpponentPoint;
    }

    public Vector3 GetTargetPoint()
    {
        return ChassisInstanceTransform.position;
    }

    public void CalcHighestOverallScore(List<PBRobot> robots)
    {
        if (RobotStatsSO == null)
            return;
        highestOverallScore = robots.Max(x => x.RobotStatsSO.value);
    }

    public void IgnoreCollision(PBRobot otherRobot, bool ignore)
    {
        if (otherRobot == null)
            return;
        Physics.IgnoreLayerCollision(RobotLayer, otherRobot.RobotLayer, ignore);
    }

    #region ICombatEntity
    void ICombatEntity.OnSlowApplied()
    {
        ChassisInstance.CarPhysics.ResetAcceleration();
    }

    void ICombatEntity.OnSlowRemoved()
    {

    }

    void ICombatEntity.OnStunApplied()
    {
        ChassisInstance.CarPhysics.ResetAcceleration();
        if (PersonalInfo.isLocal)
        {
            PlayerController playerController = ObjectFindCache<PlayerController>.Get();
            playerController.enabled = false;
        }
    }

    void ICombatEntity.OnStunRemoved()
    {
        if (PersonalInfo.isLocal)
        {
            PlayerController playerController = ObjectFindCache<PlayerController>.Get();
            playerController.enabled = true;
        }
    }

    void ICombatEntity.OnDisarmApplied()
    {
        ChassisInstance.CarPhysics.ResetAcceleration();
    }

    void ICombatEntity.OnDisarmRemoved()
    {

    }

    void ICombatEntity.OnInvincibleApplied()
    {

    }

    void ICombatEntity.OnInvincibleRemoved()
    {

    }

    void ICombatEntity.OnAirborneApplied()
    {
        ChassisInstance.CarPhysics.CanAutoBalance = false;
    }

    void ICombatEntity.OnAirborneRemoved()
    {
        ChassisInstance.CarPhysics.CanAutoBalance = true;
    }
    #endregion

    [Serializable]
    public struct Part
    {
        #region Constructor
        public Part(PBPartSlot slotType, List<PBPart> instances)
        {
            PartSlotType = slotType;
            Parts = instances;
        }
        #endregion

        public PBPartSlot PartSlotType;
        public List<PBPart> Parts;
    }
}