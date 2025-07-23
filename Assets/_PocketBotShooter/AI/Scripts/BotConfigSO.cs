using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "BotConfigSO", menuName = "Shooter/Bot/BotConfigSO")]
public class BotConfigSO : ScriptableObject
{
    // Patrol
    [Title("Patrol")]
    public int maxPatrolPointsPerSession = 3;

    // Combat settings
    [Title("Combat settings")]
    public float enemyDetectionRadius = 20f;
    [Range(0f, 1f)]
    public float attackStayStillChance = 0.15f;
    public RangeFloatValue minMaxAttackDistanceMultiplier = new RangeFloatValue(0.7f, 0.95f);
    public RangeFloatValue attackStayStillTime = new RangeFloatValue(0.25f, 1f);
    public RangeFloatValue attackMoveRandomlyDistance = new RangeFloatValue(5f, 10f);
    public RangeFloatValue attackMoveRandomlyTime = new RangeFloatValue(0.85f, 1.25f);

    // Fleeing
    [Title("Fleeing")]
    [Range(0f, 1f), Tooltip("Percentage of max health at which the bot will start to flee")]
    public float lowHealthPercentage = 0.5f;
    [Range(0f, 1f)]
    public float fleeChance = 0.2f;
    [Range(0f, 1f)]
    public float firstFleeChance = 0.5f;

    // Human-like imperfections (reaction delay, etc.)
    [Title("Human-like imperfections")]
    public RangeFloatValue reactionDelay = new RangeFloatValue(0.1f, 0.4f);

    // Powerup
    [Title("Powerup")]
    public LayerMask powerupLayerMask;
    public float powerupDetectionRadius = 10f;
    [Range(0f, 1f)]
    public float powerupPickupChanceWhenSpotted = 0.75f;
    [Range(0f, 1f)]
    public float powerupPickupChanceAtLowHealth = 0.2f;
    [Range(0f, 1f)]
    public float firstPowerupPickupChanceAtLowHealth = 0.5f;
}