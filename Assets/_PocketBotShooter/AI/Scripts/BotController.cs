using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using HyrphusQ.Helpers;
using UnityEngine.Animations;
using HyrphusQ.Events;

public class BotController : MonoBehaviour
{
    // Bot states
    public enum BotState
    {
        Idle,
        Patrol,
        Seek,
        Attack,
        Flee,
        Collect,
    }

    // Bot configuration
    [SerializeField]
    private PB_AIProfile botConfigSO;
    [SerializeField]
    private PBRobot robot;

    private BotState currentState = BotState.Idle;
    private float attackRange;
    private int currentPatrolIndex = -1;
    private NavMeshAgent agent;
    private AimController aimController;
    private Coroutine stateMachineCoroutine;
    [ShowInInspector, ReadOnly]
    private BotController nearestTarget;
    private INavigationPoint nearestPowerup;
    private INavigationPoint pickupPowerup;
    private Dictionary<BotState, int> stateCountDictionary = new Dictionary<BotState, int>();

    [ShowInInspector, ReadOnly, Title("DEBUG")]
    public static readonly List<BotController> AllBotControllers = new List<BotController>();
    [ShowInInspector, ReadOnly]
    public static readonly List<BotController> AllBotControllersTeam1 = new List<BotController>();
    [ShowInInspector, ReadOnly]
    public static readonly List<BotController> AllBotControllersTeam2 = new List<BotController>();
    [ShowInInspector, ReadOnly]
    private float DistanceToNearestTarget => nearestTarget == null ? 0f : Vector3.Distance(GetTargetPoint(), nearestTarget.GetTargetPoint());
    public int TeamId => gameObject.layer == 31 ? 1 : 2;
    public float CurrentHealth => robot.Health;
    public float MaxHealth => robot.MaxHealth;
    public float AttackRange => attackRange;
    public float FleeChance => stateCountDictionary.Get(BotState.Flee) <= 0 ? botConfigSO.firstFleeChance : botConfigSO.fleeChance;
    public float PowerupPickupChanceWhenSpotted => botConfigSO.powerupPickupChanceWhenSpotted;
    public float PowerupPickupChanceAtLowHealth => stateCountDictionary.Get(BotState.Collect) <= 0 ? botConfigSO.firstPowerupPickupChanceAtLowHealth : botConfigSO.powerupPickupChanceAtLowHealth;
    public PB_AIProfile BotConfigSO => botConfigSO;
    [ShowInInspector, ReadOnly]
    public BotState CurrentState
    {
        get => currentState;
        set
        {
            BotState previousState = currentState;
            currentState = value;
            stateCountDictionary.Set(value, stateCountDictionary.Get(value) + 1);
            Log($"Bot State Transition: {previousState} -> {value}");
        }
    }
    public NavMeshAgent Agent
    {
        get
        {
            if (agent == null)
            {
                agent = robot.ChassisInstance.GetComponentInChildren<NavMeshAgent>();
                agent.enabled = !robot.PersonalInfo.isLocal;
                agent.stoppingDistance = 0.5f;
                agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
                if (!robot.PersonalInfo.isLocal)
                {
                    agent.speed = CarPhysics.MAX_SPEED * robot.ChassisInstance.ChassisSO.Speed;
                    robot.ChassisInstance.CarPhysics.enabled = false;
                }
            }
            return agent;
        }
    }
    public AimController AimController
    {
        get
        {
            if (aimController == null)
            {
                aimController = robot.ChassisInstance.GetComponentInChildren<AimController>();
            }
            return aimController;
        }
    }

    private void Awake()
    {
        if (robot.IsPreview)
            return;
        if (robot.PersonalInfo.isLocal)
        {
            AllBotControllers.Add(this);
            AllBotControllersTeam1.Add(this);
        }
    }

    private void Start()
    {
        if (robot.IsPreview)
            return;
        robot.OnHealthChanged += OnHealthChanged;
        AllBotControllers.Add(this);
        if (TeamId == 1)
            AllBotControllersTeam1.Add(this);
        else
            AllBotControllersTeam2.Add(this);
        if (!robot.PersonalInfo.isLocal)
        {
            botConfigSO = robot.PlayerInfoVariable.value.Cast<PBBotInfo>().aiProfile;
        }
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStart);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);
        InvokeRepeating(nameof(UpdateRandomAttackRange), 0f, 10f);
    }

    private void Update()
    {
        // Update nearest powerup
        UpdateNearestPowerup();

        // Update target (e.g., nearest enemy)
        UpdateNearestTarget();
    }

    private void OnDestroy()
    {
        robot.OnHealthChanged -= OnHealthChanged;
        AllBotControllers.Remove(this);
        if (TeamId == 1)
            AllBotControllersTeam1.Remove(this);
        else
            AllBotControllersTeam2.Remove(this);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStart);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, OnLevelEnded);
    }

    private void OnLevelStart()
    {
        StartStateMachine();
    }

    private void OnLevelEnded()
    {
        StopStateMachine();
        enabled = false;
        Agent.isStopped = true;
    }

    private void UpdateRandomAttackRange()
    {
        attackRange = AimController.Guns.Count > 0 ? AimController.Guns.GetRandom().aimRange : 10f;
    }

    private void Log(string message)
    {
        // if (gameObject.layer == 31)
        //     LGDebug.Log($"{message} [Time: {Time.time} - Frame: {Time.frameCount}]", context: this);
    }

    private IEnumerator StateMachine_CR()
    {
        while (true)
        {
            switch (CurrentState)
            {
                case BotState.Idle:
                    yield return IdleState_CR();
                    break;
                case BotState.Patrol:
                    yield return PatrolState_CR();
                    break;
                case BotState.Seek:
                    yield return SeekState_CR();
                    break;
                case BotState.Attack:
                    yield return AttackState_CR();
                    break;
                case BotState.Flee:
                    yield return FleeState_CR();
                    break;
                case BotState.Collect:
                    yield return CollectState_CR();
                    break;
            }
        }
    }

    private IEnumerator IdleState_CR()
    {
        Log("Idle State");
        Agent.isStopped = true;
        // Add a small delay between state checks
        yield return new WaitForSeconds(Random.Range(botConfigSO.reactionDelay.minValue, botConfigSO.reactionDelay.maxValue));

        if (nearestTarget != null && !nearestTarget.IsDead())
        {
            float distanceToTarget = Vector3.Distance(GetTargetPoint(), nearestTarget.GetTargetPoint());
            if (distanceToTarget <= AttackRange)
            {
                CurrentState = BotState.Attack;
            }
            else
            {
                CurrentState = BotState.Seek;
            }
        }
        else
        {
            if (stateCountDictionary.Get(BotState.Patrol) < botConfigSO.maxPatrolPointsPerSession)
                CurrentState = BotState.Patrol;
            else
            {
                // Find neareat alive enemy on map
                float closestDistance = float.MaxValue;
                BotController closestTarget = null;
                foreach (BotController botController in AllBotControllers)
                {
                    if (botController.IsSameTeam(this) || botController.IsDead() || botController == this)
                        continue;

                    float distance = Vector3.Distance(GetTargetPoint(), botController.GetTargetPoint());
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = botController;
                    }
                }
                nearestTarget = closestTarget;
                CurrentState = BotState.Seek;
            }
        }
    }

    private IEnumerator PatrolState_CR()
    {
        Log("Patrol State");
        if (Map.PatrolPoints.Count == 0)
        {
            CurrentState = BotState.Idle;
            yield break;
        }

        int patrolIndex = RandomHelper.RandomRange(0, Map.PatrolPoints.Count, index => index != currentPatrolIndex);
        currentPatrolIndex = patrolIndex;
        Agent.isStopped = false;
        Agent.SetDestination(Map.PatrolPoints[patrolIndex].position);

        while (!Agent.hasPath || Agent.pathPending || Agent.remainingDistance > Agent.stoppingDistance)
        {
            // If we find a target change state
            if (nearestTarget != null && !nearestTarget.IsDead())
            {
                CurrentState = BotState.Idle;
                yield break;
            }
            yield return null;
        }
        Log($"Patrol State: Reached destination. Remaining distance: {Agent.remainingDistance}");

        CurrentState = BotState.Idle;
    }

    private IEnumerator SeekState_CR()
    {
        Log("Seek State");
        if (nearestTarget == null || nearestTarget.IsDead())
        {
            CurrentState = BotState.Idle;
            yield break;
        }

        Agent.isStopped = false;
        Agent.SetDestination(nearestTarget.GetTargetPoint());

        // Periodically update destination to follow moving targets
        float updateInterval = Random.Range(0.3f, 0.6f);
        float timeSinceLastUpdate = 0f;

        while (Agent.pathPending || Agent.remainingDistance > Agent.stoppingDistance)
        {
            timeSinceLastUpdate += Time.deltaTime;

            // Update path to target periodically
            if (timeSinceLastUpdate >= updateInterval && nearestTarget != null)
            {
                Agent.SetDestination(nearestTarget.GetTargetPoint());
                timeSinceLastUpdate = 0f;
            }

            // If we're in attack range, switch to attack
            if (nearestTarget != null && Vector3.Distance(GetTargetPoint(), nearestTarget.GetTargetPoint()) <= AttackRange)
            {
                CurrentState = BotState.Attack;
                yield break;
            }

            if (nearestTarget == null || nearestTarget.IsDead())
            {
                CurrentState = BotState.Idle;
                yield break;
            }

            yield return null;
        }

        CurrentState = BotState.Attack;
    }

    private IEnumerator AttackState_CR()
    {
        Log("Attack State");
        if (nearestTarget == null || nearestTarget.IsDead())
        {
            CurrentState = BotState.Idle;
            yield break;
        }

        while (nearestTarget != null && !nearestTarget.IsDead() && Vector3.Distance(GetTargetPoint(), nearestTarget.GetTargetPoint()) <= AttackRange * 1.3f)
        {
            float randomDelay;
            // Decide whether to continue stay still to attack or move randomly
            if (botConfigSO.attackStayStillChance > 0f && Random.value <= botConfigSO.attackStayStillChance)
            {
                // Stay still and attack
                Agent.isStopped = true;
                randomDelay = Random.Range(botConfigSO.attackStayStillTime.minValue, botConfigSO.attackStayStillTime.maxValue);
            }
            else
            {
                // Move randomly
                Agent.isStopped = false;
                float randomDistance = Random.Range(botConfigSO.attackMoveRandomlyDistance.minValue, botConfigSO.attackMoveRandomlyDistance.maxValue);
                Vector3 randomPoint = RandomHelper.RandomDirection((System.Predicate<Vector3>)(point =>
                {
                    Vector3 targetPosition = GetTargetPoint() + point.normalized * randomDistance;
                    float distance = Vector3.Distance(targetPosition, nearestTarget.GetTargetPoint());
                    // Make sure not too close to target
                    if (distance < AttackRange * botConfigSO.minMaxAttackDistanceMultiplier.minValue)
                        return false;
                    // Make sure not too far from target
                    if (distance > AttackRange * botConfigSO.minMaxAttackDistanceMultiplier.maxValue)
                        return false;
                    // Make sure not blocked by obstacle
                    if (this.Agent.Raycast(targetPosition, out NavMeshHit hit))
                        return false;
                    return true;
                }), Axis.X | Axis.Z).normalized * randomDistance;
                Agent.SetDestination(GetTargetPoint() + randomPoint);
                randomDelay = Random.Range(botConfigSO.attackMoveRandomlyTime.minValue, botConfigSO.attackMoveRandomlyTime.maxValue) * randomPoint.magnitude / Agent.speed;
            }
            yield return new WaitForSeconds(randomDelay);
            if (!Agent.isStopped)
            {
                Log($"Remaining distance: {Agent.remainingDistance} - Stopping distance: {Agent.stoppingDistance} - ReachDest: {Agent.pathStatus == NavMeshPathStatus.PathComplete}");
            }
        }

        if (nearestTarget == null || nearestTarget.IsDead())
        {
            CurrentState = BotState.Idle;
            yield break;
        }

        CurrentState = BotState.Seek;
    }

    private IEnumerator FleeState_CR()
    {
        Log("Flee State");

        // If found a nearest flee point, go there
        if (Map.TryFindCoverPoint(this, out Vector3 nearestFleePoint))
        {
            Agent.isStopped = false;
            Agent.SetDestination(nearestFleePoint);

            while (Agent.pathPending || Agent.remainingDistance > Agent.stoppingDistance)
            {
                yield return null;
            }

            if (IsLowHealth())
            {
                float randomValue = Random.value;
                if (PowerupPickupChanceAtLowHealth > 0f && randomValue <= PowerupPickupChanceAtLowHealth && Map.TryFindPowerupPoint(this, out pickupPowerup))
                {
                    CurrentState = BotState.Collect;
                    yield break;
                }
                else if (FleeChance > 0f && randomValue <= (PowerupPickupChanceAtLowHealth + FleeChance) && Map.TryFindCoverPoint(this, out _))
                {
                    CurrentState = BotState.Flee;
                    yield break;
                }
            }
        }

        if (nearestTarget != null && !nearestTarget.IsDead())
        {
            if (Vector3.Distance(GetTargetPoint(), nearestTarget.GetTargetPoint()) > AttackRange)
                CurrentState = BotState.Seek;
            else
                CurrentState = BotState.Attack;
        }
        else
        {
            CurrentState = BotState.Idle;
        }
    }

    private IEnumerator CollectState_CR()
    {
        Log("Collect State");
        if (pickupPowerup == null || !pickupPowerup.IsAvailable())
        {
            CurrentState = BotState.Idle;
            yield break;
        }

        Agent.isStopped = false;
        Agent.SetDestination(pickupPowerup.GetTargetPoint());

        while (Agent.pathPending || Agent.remainingDistance > Agent.stoppingDistance)
        {
            // If powerup disappeared, break
            if (pickupPowerup == null || !pickupPowerup.IsAvailable())
            {
                yield break;
            }
            yield return null;
        }

        if (nearestTarget != null && !nearestTarget.IsDead())
        {
            if (Vector3.Distance(GetTargetPoint(), nearestTarget.GetTargetPoint()) > AttackRange)
                CurrentState = BotState.Seek;
            else
                CurrentState = BotState.Attack;
        }
        else
        {
            CurrentState = BotState.Idle;
        }
    }

    private void UpdateNearestPowerup()
    {
        Collider[] colliders = Physics.OverlapSphere(GetTargetPoint(), botConfigSO.powerupDetectionRadius, botConfigSO.powerupLayerMask, QueryTriggerInteraction.Collide);
        float closestDistance = float.MaxValue;
        INavigationPoint closestPowerup = null;

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent(out INavigationPoint powerup) && powerup.IsAvailable())
            {
                float distance = Vector3.Distance(GetTargetPoint(), collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPowerup = powerup;
                }
            }
        }

        // If we found a powerup and we're not in combat or fleeing, go collect it
        if (nearestPowerup != closestPowerup)
        {
            nearestPowerup = closestPowerup;
            if (nearestPowerup != null && CurrentHealth < MaxHealth && PowerupPickupChanceWhenSpotted > 0f && Random.value <= PowerupPickupChanceWhenSpotted)
            {
                pickupPowerup = nearestPowerup;
                CurrentState = BotState.Collect;
                StartStateMachine();
            }
        }
    }

    private void UpdateNearestTarget()
    {
        float closestDistance = float.MaxValue;
        BotController closestTarget = null;

        foreach (BotController botController in AllBotControllers)
        {
            if (botController.IsSameTeam(this) || botController.IsDead() || botController == this)
                continue;

            float distance = Vector3.Distance(GetTargetPoint(), botController.GetTargetPoint());
            if (distance <= botConfigSO.enemyDetectionRadius && distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = botController;
            }
        }

        if (closestTarget != null || (nearestTarget != null && nearestTarget.IsDead()))
        {
            // Normal targeting behavior
            nearestTarget = closestTarget;
        }
    }

    private void OnHealthChanged(Competitor.HealthChangedEventData data)
    {
        if (IsDead())
        {
            CurrentState = BotState.Idle;
            Agent.isStopped = true;
            agent = null;
            StopStateMachine();
            return;
        }
        if (data.CurrentHealth < data.OldHealth)
        {
            if (nearestTarget != null)
            {
                // If we're collecting or fleeing, don't change state
                if (CurrentState == BotState.Collect || CurrentState == BotState.Flee)
                {
                    return;
                }

                if (IsLowHealth())
                {
                    float randomValue = Random.value;
                    if (PowerupPickupChanceAtLowHealth > 0f && randomValue <= PowerupPickupChanceAtLowHealth && Map.TryFindPowerupPoint(this, out pickupPowerup))
                    {
                        CurrentState = BotState.Collect;
                        StartStateMachine();
                    }
                    else if (FleeChance > 0f && randomValue <= (PowerupPickupChanceAtLowHealth + FleeChance) && Map.TryFindCoverPoint(this, out _))
                    {
                        CurrentState = BotState.Flee;
                        StartStateMachine();
                    }
                }
            }
        }
    }

    public bool IsLowHealth()
    {
        if (nearestTarget == null || nearestTarget.IsDead())
            return false;
        return CurrentHealth < MaxHealth * botConfigSO.lowHealthPercentage && CurrentHealth < nearestTarget.CurrentHealth;
    }

    public bool IsSameTeam(BotController botController)
    {
        return botController.TeamId == TeamId;
    }

    public bool IsDead()
    {
        return CurrentHealth <= 0;
    }

    public void Die()
    {
        gameObject.SetActive(false);
    }

    public float GetDamage()
    {
        return 10f;
    }

    public Vector3 GetTargetPoint()
    {
        return robot.ChassisInstanceTransform.position;
    }

    public void StartStateMachine()
    {
        if (!enabled)
            return;
        if (robot.PersonalInfo.isLocal)
            return;
        if (stateMachineCoroutine != null)
        {
            StopCoroutine(stateMachineCoroutine);
        }
        stateMachineCoroutine = StartCoroutine(StateMachine_CR());
    }

    public void StopStateMachine()
    {
        if (robot.PersonalInfo.isLocal)
            return;
        if (stateMachineCoroutine != null)
        {
            StopCoroutine(stateMachineCoroutine);
        }
    }
}