using LatteGames.Template;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveSkillSO_ScorpionBots", menuName = "PocketBots/ActiveSkillSO/ScorpionBots")]
public class ActiveSkillScorpionBotsSO : ActiveSkillSO<ActiveSkillScorpionBotsSO, ActiveSkillScorpionBotsCaster>
{
    public float ScorpionAmout => m_ScorpionAmout;
    public float DamagePercent => m_DamagePercent;
    public float LifeTime => m_LifeTime;
    public float MoveSpeed => m_MoveSpeed;
    public float RayDistanceWall => m_RayDistanceWall;
    public float FromDampingTime => m_FromDampingTime;
    public float ToDampingTime => m_ToDampingTime;
    public float DetectionRadius => m_DetectionRadius;
    public float MinDictanceAttack => m_MinDictanceAttack;
    public float TimeWarning => m_TimeWarning;
    public float JumpHeight => m_JumpHeight;
    public float JumpDuration => m_JumpDuration;
    public float ZoneExplode => m_ZoneExplode;
    public float DetectRobotRadiusZoneAI => m_DetectRobotRadiusZoneAI;
    public LayerMask WallLayer => m_WallLayer;
    public LayerMask RobotLayer => m_RobotLayer;

    public ScorpionBehavior ScorpionBehaviorPrefab => m_ScorpionBehaviorPrefab;

    [SerializeField, BoxGroup("Config")] private float m_ScorpionAmout = 5f;
    [SerializeField, BoxGroup("Config")] private float m_DamagePercent = 5;
    [SerializeField, BoxGroup("Config")] private float m_LifeTime = 5;
    [SerializeField, BoxGroup("Config")] private float m_MoveSpeed = 3f;
    [SerializeField, BoxGroup("Config")] private float m_RayDistanceWall = 1f;
    [SerializeField, BoxGroup("Config")] private float m_FromDampingTime = 0.1f;
    [SerializeField, BoxGroup("Config")] private float m_ToDampingTime = 0.5f;
    [SerializeField, BoxGroup("Config")] private float m_DetectionRadius = 2f;
    [SerializeField, BoxGroup("Config")] private float m_MinDictanceAttack = 1f;
    [SerializeField, BoxGroup("Config")] private float m_TimeWarning = 2f;
    [SerializeField, BoxGroup("Config")] private float m_JumpHeight = 2f;
    [SerializeField, BoxGroup("Config")] private float m_JumpDuration = 0.5f;
    [SerializeField, BoxGroup("Config")] private float m_ZoneExplode = 5f;
    [SerializeField, BoxGroup("Config AI")] private float m_DetectRobotRadiusZoneAI = 5f;
    [SerializeField, BoxGroup("Config")] private LayerMask m_WallLayer;
    [SerializeField, BoxGroup("Config")] private LayerMask m_RobotLayer;

    [SerializeField, BoxGroup("Resource")] private ScorpionBehavior m_ScorpionBehaviorPrefab;
}

public class ActiveSkillScorpionBotsCaster : ActiveSkillCaster<ActiveSkillScorpionBotsSO>
{
    private List<ScorpionBehavior> m_Scorpions;
    private bool m_IsHasRobotInZone = false;

    protected override void Update()
    {
        base.Update();
        DetectRobot();
    }
    protected override void SetupRobot()
    {
        base.SetupRobot();
        m_Scorpions = new List<ScorpionBehavior>(); 
    }
    public override void PerformSkill()
    {
        base.PerformSkill();
        ClearScorpion();
        remainingCooldown = m_ActiveSkillSO.cooldown;
        for (int i = 0; i < m_ActiveSkillSO.ScorpionAmout; i++)
        {
            ScorpionBehavior scorpion = Instantiate(m_ActiveSkillSO.ScorpionBehaviorPrefab);
            scorpion.InitData(m_Robot, m_ActiveSkillSO);
            scorpion.DisableBehavior();
            scorpion.transform.position = m_Robot.ChassisInstance.CarPhysics.transform.position;
            scorpion.transform.localScale = Vector3.one * 0.01f;
            m_Scorpions.Add(scorpion);
        }
        SoundManager.Instance.PlaySFX(SFX.BombBotAppear);
        StartCoroutine(DeployScorpions());
    }

    private void DetectRobot()
    {
        Collider[] colliders = Physics.OverlapSphere(m_Robot.ChassisInstance.CarPhysics.transform.position, m_ActiveSkillSO.DetectRobotRadiusZoneAI, m_ActiveSkillSO.RobotLayer);
        m_IsHasRobotInZone = colliders.Any(collider => IsValidRobotTarget(collider));
    }

    private bool IsValidRobotTarget(Collider collider)
    {
        PBPart part = collider.GetComponent<PBPart>();
        return part?.RobotChassis?.CarPhysics != null && part.RobotChassis.Robot != m_Robot;
    }

    private IEnumerator DeployScorpions()
    {
        if (m_Scorpions.Count == 0) yield break;

        Transform chassisTransform = m_Robot.ChassisInstance.CarPhysics.transform;
        Vector3 forwardDir = chassisTransform.forward;
        Vector3 rightDir = chassisTransform.right;

        float maxDistance = 5f;
        float spacing = 1.5f;
        int count = m_Scorpions.Count;
        float totalWidth = (count - 1) * spacing;

        if (Physics.Raycast(chassisTransform.position, forwardDir, out RaycastHit hit, maxDistance, m_ActiveSkillSO.WallLayer))
            maxDistance = hit.distance - 0.5f;

        Dictionary<ScorpionBehavior, Vector3> DicData = new Dictionary<ScorpionBehavior, Vector3>();

        for (int i = 0; i < count; i++)
        {
            float offset = -totalWidth / 2f + i * spacing;
            Vector3 targetPos = chassisTransform.position + forwardDir * maxDistance + rightDir * offset;

            ScorpionBehavior scorpion = m_Scorpions[i];
            scorpion.transform.position = m_Robot.ChassisInstance.CarPhysics.transform.position;
            DicData.Add(scorpion, targetPos);
            yield return null;
        }

        for (int i = 0; i < count; i++)
            StartCoroutine(MoveScorpion(DicData.ElementAt(i).Key, DicData.ElementAt(i).Value, chassisTransform));
    }

    private IEnumerator MoveScorpion(ScorpionBehavior scorpion, Vector3 targetPos, Transform robotTransform)
    {
        float duration = 0.4f;
        float elapsedTime = 0f;

        scorpion.transform.rotation = Quaternion.LookRotation(robotTransform.forward);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float height = Mathf.Sin(t * Mathf.PI) * 2f;

            if(elapsedTime > duration / 50)
                scorpion.EnableBehavior();

            scorpion.transform.position = Vector3.Lerp(m_Robot.ChassisInstance.CarPhysics.transform.position, targetPos, t) + Vector3.up * height;
            scorpion.transform.localScale = Vector3.Lerp(scorpion.transform.localScale, Vector3.one, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        scorpion.transform.position = targetPos;
        scorpion.transform.localScale = Vector3.one;
    }

    private void ClearScorpion()
    {
        m_Scorpions.Where(v => v != null).ToList().ForEach(v => Destroy(v.gameObject));
        m_Scorpions.Clear();
    }

    public override bool IsAbleToPerformSkill()
    {
        if (!m_Robot.ChassisInstance.CarPhysics.CanMove)
            return false;
        return base.IsAbleToPerformSkill();
    }
    public override bool IsAbleToPerformSkillForAI()
    {
        return IsAbleToPerformSkill() && m_IsHasRobotInZone && m_Robot.ChassisInstance.CarPhysics.CanMove;
    }
}