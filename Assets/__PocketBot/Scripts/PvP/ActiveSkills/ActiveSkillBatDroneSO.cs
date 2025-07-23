using LatteGames.Template;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst.Intrinsics;
using UnityEngine;
using LatteGames;
using System.Runtime.ConstrainedExecution;

[CreateAssetMenu(fileName = "ActiveSkillBatDroneSO", menuName = "PocketBots/ActiveSkillSO/BatDroneSO")]
public class ActiveSkillBatDroneSO : ActiveSkillSO<ActiveSkillBatDroneSO, ActiveSkillScorpionBotBatDroneCaster>
{
    public float DroneAmount => m_DroneAmount;
    public float FollowHeightOffset => m_FollowHeightOffset;
    public float SeparationRadiusDrone => m_SeparationRadiusDrone;
    public float AvoidanceStrength => m_AvoidanceStrength;
    public float AttackZone => m_AttackZone;
    public float ZoneExplode => m_ZoneExplode;
    public float ExpiresDuration => m_ExpiresDuration;
    public float DroneRemainDuration => m_DroneRemainDuration;
    public float FireRate => m_FireRate;
    public float DamagePerHitRatio => m_DamagePerHitRatio;
    public float DamageLazerMaxHealthPercent => m_DamageLazerMaxHealthPercent;
    public float DamageExplosionMaxHealthPercent => m_DamageExplosionMaxHealthPercent;
    public float DetectRobotRadiusZoneAI => m_DetectRobotRadiusZoneAI;
    public LayerMask RobotLayermask => m_RobotLayermask;
    public BatDroneBehavior BatDroneBase => m_BatDroneBase;

    [SerializeField, BoxGroup("Config")] private float m_DroneAmount;
    [SerializeField, BoxGroup("Config")] private float m_FollowHeightOffset;
    [SerializeField, BoxGroup("Config")] private float m_SeparationRadiusDrone;
    [SerializeField, BoxGroup("Config")] private float m_AvoidanceStrength;
    [SerializeField, BoxGroup("Config")] private float m_AttackZone;
    [SerializeField, BoxGroup("Config")] private float m_ZoneExplode;
    [SerializeField, BoxGroup("Config")] private float m_ExpiresDuration;
    [SerializeField, BoxGroup("Config")] private float m_DroneRemainDuration;
    [SerializeField, BoxGroup("Config ATK")] private float m_FireRate;
    [SerializeField, BoxGroup("Config ATK")] private float m_DamagePerHitRatio;
    [SerializeField, BoxGroup("Config ATK")] private float m_DamageLazerMaxHealthPercent;
    [SerializeField, BoxGroup("Config ATK")] private float m_DamageExplosionMaxHealthPercent;
    [SerializeField, BoxGroup("Config AI")] private float m_DetectRobotRadiusZoneAI;
    [SerializeField, BoxGroup("Config")] private LayerMask m_RobotLayermask;
    [SerializeField, BoxGroup("Resource")] private BatDroneBehavior m_BatDroneBase;
}
public class ActiveSkillScorpionBotBatDroneCaster : ActiveSkillCaster<ActiveSkillBatDroneSO>
{
    private List<BatDroneBehavior> m_BatDroneBehaviors;
    private bool m_IsHasRobotInZone = false;

    protected override void Update()
    {
        base.Update();
        DetectRobot();
    }
    protected override void SetupRobot()
    {
        base.SetupRobot();
        m_BatDroneBehaviors = new List<BatDroneBehavior>();
    }
    public override void PerformSkill()
    {
        base.PerformSkill();
        ClearDrone();
        for (int i = 0; i < m_ActiveSkillSO.DroneAmount; i++)
        {
            BatDroneBehavior droneBehavior = Instantiate(m_ActiveSkillSO.BatDroneBase);
            m_BatDroneBehaviors.Add(droneBehavior);
        }
        Appear();
    }
    private void Appear()
    {
        for (int i = 0; i < m_BatDroneBehaviors.Count; i++)
        {
            CarPhysics carPhysics = m_Robot.ChassisInstance.CarPhysics;
            if(carPhysics != null)
                m_BatDroneBehaviors[i].transform.position = new Vector3(carPhysics.transform.position.x, carPhysics.transform.position.y + 50, carPhysics.transform.position.z);
            m_BatDroneBehaviors[i].Activate(m_Robot, m_ActiveSkillSO, i);
        }
        WaitingResetCoolDown();
    }

    private void WaitingResetCoolDown()
    {
        StartCoroutine(CommonCoroutine.Delay(m_ActiveSkillSO.ExpiresDuration, false, () =>
        {
            remainingCooldown = m_ActiveSkillSO.cooldown;
        }));
    }

    private void ClearDrone()
    {
        m_BatDroneBehaviors
            .Where(v => v != null).ToList()
            .ForEach(v => Destroy(v.gameObject));

        m_BatDroneBehaviors.Clear();
    }

    private void DetectRobot()
    {
        Collider[] colliders = Physics.OverlapSphere(m_Robot.ChassisInstance.CarPhysics.transform.position, m_ActiveSkillSO.DetectRobotRadiusZoneAI, m_ActiveSkillSO.RobotLayermask);
        m_IsHasRobotInZone = colliders.Any(collider => IsValidRobotTarget(collider));
    }

    private bool IsValidRobotTarget(Collider collider)
    {
        PBPart part = collider.GetComponent<PBPart>();
        return part?.RobotChassis?.CarPhysics != null && part.RobotChassis.Robot != m_Robot;
    }

    public override bool IsAbleToPerformSkillForAI()
    {
        return IsAbleToPerformSkill() && m_IsHasRobotInZone && m_Robot.ChassisInstance.CarPhysics.CanMove;
    }
}

