using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using UnityEngine;

public class StunEffect : CombatEffect<StunEffectConfigSO>
{
    public class DataContainer : MonoBehaviour
    {
        private StunEffectConfigSO m_ConfigSO;
        private CombatEffectController m_Controller;
        private ParticleSystem m_StunParticle;

        public bool isInitialized => m_ConfigSO != null;
        public ParticleSystem stunParticle
        {
            get
            {
                if (m_StunParticle == null)
                {
                    m_StunParticle = Instantiate(m_ConfigSO.stunParticlePrefab, transform);
                }
                return m_StunParticle;
            }
        }

        public void Initialize(StunEffect stunEffect)
        {
            if (isInitialized)
                return;
            m_ConfigSO = stunEffect.configSO;
            m_Controller = stunEffect.controller;
            ParticleSystem.ShapeModule stunParticleShape = stunParticle.shape;
            Renderer renderer = m_Controller.affectedEntity.rigidbody.GetComponentInChildren<Renderer>();
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                stunParticleShape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                stunParticleShape.skinnedMeshRenderer = skinnedMeshRenderer;
            }
            else if (renderer is MeshRenderer meshRenderer)
            {
                stunParticleShape.shapeType = ParticleSystemShapeType.MeshRenderer;
                stunParticleShape.meshRenderer = meshRenderer;
            }
        }
    }

    public StunEffect(float duration, bool isAffectedByAbility, ICombatEntity sourceEntity, bool canStack = true) : base(duration, isAffectedByAbility, sourceEntity, canStack)
    {
    }

    private DataContainer m_DataContainer;

    public override CombatEffectStatuses effectStatus => CombatEffectStatuses.Stunned;
    public override CombatEffectStatuses blockingEffectStatuses => CombatEffectStatuses.Invincible;
    private DataContainer dataContainer
    {
        get
        {
            if (m_DataContainer == null)
            {
                m_DataContainer = controller.GetOrAddComponent<DataContainer>();
                m_DataContainer.Initialize(this);
            }
            return m_DataContainer;
        }
    }

    public override void Apply()
    {
        base.Apply();
        affectedEntity.isStunned = true;
        affectedEntity.OnStunApplied();
        dataContainer.stunParticle.Play();
        controller.Log($"{affectedEntity.name} is now stunned for {remainingDuration} seconds.");
    }

    public override void Remove()
    {
        base.Remove();
        affectedEntity.isStunned = false;
        affectedEntity.OnStunRemoved();
        dataContainer.stunParticle.Stop();
        controller.Log($"{affectedEntity.name} is no longer stunned.");
    }
}