using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "AI_Profile", menuName = "PocketBots/PvP/AI Profile")]
public class PB_AIProfile : ScriptableObject, IBotProfile
{
    public DifficultyType bossType;
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

    #region Obsolete
    float movingSpeed = 0.4f;
    float steeringSpeed = 0.4f;
    float movingSpeedWhenAttack = 0.4f;
    float steeringSpeedWhenAttack = 0.4f;
    float collectBoosterProbability = 0.2f;
    float lowerOverallscoreCollectBoosterProbability = 0.35f;
    RangeFloatValue delayTimeRangeBeforeCastSkill;
    ChasingTargetState.Config chasingTargetConfig;
    AttackingTargetState.Config attackingTargetConfig;
    UpsideDownState.Config upsideDownConfig;
    ReversingState.Config reversingConfig;
    GettingOutDangerousZoneState.Config gettingOutDangerousZoneConfig;

    public DifficultyType BossType => bossType;
    public float MovingSpeed
    {
        get => movingSpeed;
        set => movingSpeed = value;
    }
    public float SteeringSpeed
    {
        get
        {
            return steeringSpeed;
        }
        set
        {
            steeringSpeed = value;
        }
    }
    public float MovingSpeedWhenAttack
    {
        get
        {
            return movingSpeedWhenAttack;
        }
        set
        {
            movingSpeedWhenAttack = value;
        }
    }
    public float SteeringSpeedWhenAttack
    {
        get
        {
            return steeringSpeedWhenAttack;
        }
        set
        {
            steeringSpeedWhenAttack = value;
        }
    }
    public float CollectBoosterProbability
    {
        get
        {
            return collectBoosterProbability;
        }
        set
        {
            collectBoosterProbability = value;
        }
    }
    public float LowerOverallscoreCollectBoosterProbability
    {
        get
        {
            return lowerOverallscoreCollectBoosterProbability;
        }
        set
        {
            lowerOverallscoreCollectBoosterProbability = value;
        }
    }
    public float CollectBoosterWhenLowHpProbability
    {
        get => attackingTargetConfig.chasingTargetTransitionConfig.collectBoosterWhenLowHpProbability;
        set => attackingTargetConfig.chasingTargetTransitionConfig.collectBoosterWhenLowHpProbability = value;
    }
    public float MinSkillCastDelay
    {
        get => delayTimeRangeBeforeCastSkill.minValue;
        set => delayTimeRangeBeforeCastSkill = new RangeFloatValue(value, delayTimeRangeBeforeCastSkill.maxValue);
    }
    public float MaxSkillCastDelay
    {
        get => delayTimeRangeBeforeCastSkill.maxValue;
        set => delayTimeRangeBeforeCastSkill = new RangeFloatValue(delayTimeRangeBeforeCastSkill.minValue, value);
    }
    public RangeFloatValue DelayTimeRangeBeforeCastSkill => delayTimeRangeBeforeCastSkill;
    public AttackingTargetState.Config AttackingTargetConfig => attackingTargetConfig;
    public UpsideDownState.Config UpsideDownConfig => upsideDownConfig;
    public ChasingTargetState.Config ChasingTargetConfig => chasingTargetConfig;
    public ReversingState.Config ReversingConfig => reversingConfig;
    public GettingOutDangerousZoneState.Config GettingOutDangerousZoneConfig => gettingOutDangerousZoneConfig;

    public static float GetProbabilityOfPointType(PointType pointType, bool isHighestOverallScore, PB_AIProfile aiProfile)
    {
        if (isHighestOverallScore)
        {
            switch (pointType)
            {
                case PointType.OpponentPoint:
                    return 0.6f;
                case PointType.CollectablePoint:
                    return aiProfile.collectBoosterProbability;
                case PointType.UtilityPoint:
                    return 0.15f;
                case PointType.NormalPoint:
                    return 0.05f;
                default:
                    return 0f;
            }
        }
        else
        {
            switch (pointType)
            {
                case PointType.OpponentPoint:
                    return 0.6f;
                case PointType.CollectablePoint:
                    return aiProfile.lowerOverallscoreCollectBoosterProbability;
                case PointType.UtilityPoint:
                    return 0.15f;
                case PointType.NormalPoint:
                    return 0.05f;
                default:
                    return 0f;
            }
        }
    }
    #endregion
}