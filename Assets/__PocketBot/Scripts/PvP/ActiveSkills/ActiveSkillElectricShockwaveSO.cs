using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveSkillSO_ElectricShockwave", menuName = "PocketBots/ActiveSkillSO/ElectricShockwave")]
public class ActiveSkillElectricShockwaveSO : ActiveSkillSO<ActiveSkillElectricShockwaveSO, ActiveSkillElectricShockwaveCaster>
{
    [SerializeField, BoxGroup("Debug"), PropertyOrder(100)]
    private bool m_DrawGizmos;
    [SerializeField, Range(0f, 1f)]
    private float m_DamagePercentage = 0.1f;
    [SerializeField, Range(0f, 1f)]
    private float m_MovementSlowPercentage = 0.65f;
    [SerializeField, Range(0f, 1f)]
    private float m_RotationSlowPercentage = 0.95f;
    [SerializeField]
    private float m_SlowDuration = 3f;
    [SerializeField]
    private float m_ShockwaveRadius = 10f;
    [SerializeField]
    private ParticleSystem m_ElectricShockwaveParticlePrefab;

    public bool drawGizmos => m_DrawGizmos;
    public float damagePercentage => m_DamagePercentage;
    public float movementSlowPercentage => m_MovementSlowPercentage;
    public float rotationSlowPercentage => m_RotationSlowPercentage;
    public float slowDuration => m_SlowDuration;
    public float shockwaveRadius => m_ShockwaveRadius;
    public ParticleSystem electricShockwaveParticlePrefab => m_ElectricShockwaveParticlePrefab;
}
public class ActiveSkillElectricShockwaveCaster : ActiveSkillCaster<ActiveSkillElectricShockwaveSO>, IAttackable
{
    private RadarDetector m_RadarDetector;
    private ParticleSystem m_ElectricShockwaveParticle;

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

    private IEnumerator ReleaseElectricShockwave_CR()
    {
        m_ElectricShockwaveParticle.transform.position = m_Robot.GetTargetPoint() - Vector3.up * 0.25f;
        m_ElectricShockwaveParticle.transform.localScale = Vector3.one * m_ActiveSkillSO.shockwaveRadius;
        m_ElectricShockwaveParticle.Play();
        if (PBSoundUtility.IsOnSound())
            SoundManager.Instance.PlaySFX(SFX.VoltBurst);
        yield return Yielders.Get(0.1f);
        List<(PBRobot, float)> robotDistancePairs = new List<(PBRobot, float)>(m_RadarDetector.ScanAllRobotsInDetectArea());
        List<(IDamagable, float)> damagableObjectDistancePairs = m_RadarDetector.ScanAllObjectsInDetectArea<IDamagable>(GetDamagableObjectLayers(), predicate: item => item is not PBPart);
        for (int i = 0; i < robotDistancePairs.Count; i++)
        {
            PBRobot robot = robotDistancePairs[i].Item1;
            robot.CombatEffectController.ApplyEffect(new DisarmEffect(m_ActiveSkillSO.slowDuration, true, m_Robot));
            robot.CombatEffectController.ApplyEffect(new SlowEffect(m_ActiveSkillSO.slowDuration, m_ActiveSkillSO.movementSlowPercentage, m_ActiveSkillSO.rotationSlowPercentage, true, m_Robot));
            robot.ChassisInstance.ReceiveDamage(this, 0f);

            #region Firebase Event
            try
            {
                if (robot != null)
                {
                    if (robot.PersonalInfo.isLocal)
                    {
                        var test = this;
                        GameEventHandler.Invoke(LogFirebaseEventCode.BotStunned);

                        if (m_ActiveSkillSO != null)
                        {
                            string skillName = "null";
                            skillName = m_ActiveSkillSO.GetDisplayName();
                            GameEventHandler.Invoke(LogFirebaseEventCode.AffectedByOpponentSkill, skillName);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion
        }
        for (int i = 0; i < damagableObjectDistancePairs.Count; i++)
        {
            damagableObjectDistancePairs[i].Item1.ReceiveDamage(this, 0f);
        }
        remainingCooldown = m_ActiveSkillSO.cooldown;
    }

    public override bool IsAbleToPerformSkillForAI()
    {
        if (!IsAbleToPerformSkill())
            return false;
        if (m_RadarDetector.ScanAllRobotsInDetectArea().Count <= 0)
            return false;
        return true;
    }

    public override void PerformSkill()
    {
        base.PerformSkill();
        StartCoroutine(ReleaseElectricShockwave_CR());
    }

    public override void Initialize(ActiveSkillElectricShockwaveSO activeSkillSO, PBRobot mRobot)
    {
        base.Initialize(activeSkillSO, mRobot);
        m_RadarDetector = new RadarDetector();
        m_RadarDetector.Initialize(m_Robot.AIBotController, m_ActiveSkillSO.shockwaveRadius, 360f, m_ActiveSkillSO.drawGizmos);
        m_ElectricShockwaveParticle = Instantiate(m_ActiveSkillSO.electricShockwaveParticlePrefab, transform);
    }

    public float GetDamage()
    {
        float rawAttack = m_ActiveSkillSO.damagePercentage * m_Robot.MaxHealth;
        float attackMultiplier = m_Robot.AtkMultiplier;
        return rawAttack * attackMultiplier;
    }
}