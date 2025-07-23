using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using UnityEngine;

public class PBDamagedVisualController : MonoBehaviour
{
    [Serializable]
    public class DamageLevel
    {
        [SerializeField, Range(0f, 1f)]
        private float m_HealthPercentage;
        [SerializeField]
        private string m_BlendshapeName;
        [SerializeField]
        private GameObject[] m_BreakingFragments;

        public float healthPercentage => m_HealthPercentage;
        public string blendShapeName => m_BlendshapeName;
        public GameObject[] breakingFragments => m_BreakingFragments;
    }

    [SerializeField]
    private AstroblastLazerGunConfigSO m_ConfigSO;
    [SerializeField]
    private PBChassis m_PBChassis;
    [SerializeField]
    private SkinnedMeshRenderer[] m_SkinMeshRenderers;
    [SerializeField]
    private List<DamageLevel> m_DamageLevels;

    private PBRobot m_Robot;

    private void Start()
    {
        m_Robot = m_PBChassis.Robot;
        m_Robot.OnHealthChanged += OnHealthChanged;
        //GameEventHandler.AddActionEvent(PBPvPEventCode.OnShakeCamera, OnShakeCamera);
    }

    private void OnDestroy()
    {
        if (m_Robot != null)
            m_Robot.OnHealthChanged -= OnHealthChanged;
        //GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnShakeCamera, OnShakeCamera);
    }

    private int CalcHealthPercentageLevelIndex(float health, float maxHealth)
    {
        float healthPercentage = health / maxHealth;
        for (int i = m_DamageLevels.Count - 1; i >= 0; i--)
        {
            if (healthPercentage > m_DamageLevels[i].healthPercentage)
                return i;
        }
        return -1;
    }

    private void OnHealthChanged(Competitor.HealthChangedEventData eventData)
    {
        int oldLevelIndex = CalcHealthPercentageLevelIndex(eventData.OldHealth, eventData.MaxHealth);
        int newLevelIndex = CalcHealthPercentageLevelIndex(eventData.CurrentHealth, eventData.MaxHealth);
        if (oldLevelIndex > newLevelIndex)
        {
            Break(newLevelIndex + 1);
        }
    }

    public void Break(int damageLevelIndex)
    {
        DamageLevel damageLevel = m_DamageLevels[damageLevelIndex];
        Debug.Log($"Break {damageLevelIndex}");
        for (int i = 0; i < m_SkinMeshRenderers.Length; i++)
        {
            for (int j = 0; j < m_SkinMeshRenderers[i].sharedMesh.blendShapeCount; j++)
            {
                m_SkinMeshRenderers[i].SetBlendShapeWeight(j, 0f);
            }
            m_SkinMeshRenderers[i].SetBlendShapeWeight(m_SkinMeshRenderers[i].sharedMesh.GetBlendShapeIndex(damageLevel.blendShapeName), 100f);
        }
        if (damageLevel.breakingFragments != null && damageLevel.breakingFragments.Length > 0)
        {
            foreach (var breakingFragment in damageLevel.breakingFragments)
            {
                var rb = breakingFragment.GetOrAddComponent<Rigidbody>();
                var collider = breakingFragment.GetOrAddComponent<BoxCollider>();
                rb.AddExplosionForce(m_ConfigSO.ExplosiveForce, m_PBChassis.CarPhysics.transform.position, m_ConfigSO.ExplosiveDamageRange, m_ConfigSO.ExplosiveUpwardsModifier, ForceMode.VelocityChange);
                var gravityPhysics = breakingFragment.GetOrAddComponent<GravityPhysics>();
                gravityPhysics.CarConfigSO = m_PBChassis.CarPhysics.CarConfigSO;
            }
        }
    }
}