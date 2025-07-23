using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using UnityEngine;

public class SlowEffect : CombatEffect<SlowEffectConfigSO>
{
    public class DataContainer : MonoBehaviour
    {
        private SlowEffectConfigSO m_ConfigSO;
        private CombatEffectController m_Controller;
        private ParticleSystem m_SlowParticle;

        public bool isInitialized => m_ConfigSO != null;
        public ParticleSystem slowParticle
        {
            get
            {
                if (m_SlowParticle == null)
                {
                    m_SlowParticle = Instantiate(m_ConfigSO.slowParticlePrefab, transform);
                }
                return m_SlowParticle;
            }
        }

        public void Initialize(SlowEffect slowEffect)
        {
            if (isInitialized)
                return;
            m_ConfigSO = slowEffect.configSO;
            m_Controller = slowEffect.controller;
            ParticleSystem.ShapeModule particleShape = slowParticle.shape;
            Renderer renderer = m_Controller.affectedEntity.rigidbody.GetComponentInChildren<Renderer>();
            if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
            {
                particleShape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                particleShape.skinnedMeshRenderer = skinnedMeshRenderer;
            }
            else if (renderer is MeshRenderer meshRenderer)
            {
                particleShape.shapeType = ParticleSystemShapeType.MeshRenderer;
                particleShape.meshRenderer = meshRenderer;
            }
        }
    }

    public SlowEffect(float duration, float movementSlowPercentage, float rotationSlowPercentage, bool isAffectedByAbility, ICombatEntity sourceEntity) : base(duration, isAffectedByAbility, sourceEntity, false)
    {
        this.movementSlowPercentage = movementSlowPercentage;
        this.rotationSlowPercentage = rotationSlowPercentage;
    }

    private DataContainer m_DataContainer;

    public override CombatEffectStatuses effectStatus => CombatEffectStatuses.Slowed;
    public override CombatEffectStatuses blockingEffectStatuses => CombatEffectStatuses.Invincible;
    private float movementSlowPercentage { get; set; }
    private float rotationSlowPercentage { get; set; }
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
        affectedEntity.movementSpeedMultiplier *= 1f - movementSlowPercentage;
        affectedEntity.rotationSpeedMultiplier *= 1f - rotationSlowPercentage;
        affectedEntity.OnSlowApplied();
        dataContainer.slowParticle.Play();
        controller.Log($"{affectedEntity.name} is slowed by {movementSlowPercentage * 100}% - {rotationSlowPercentage * 100}% - {affectedEntity.movementSpeedMultiplier * 100}% - {affectedEntity.rotationSpeedMultiplier * 100}% for {remainingDuration} seconds.");
    }


    public override void Remove()
    {
        base.Remove();
        affectedEntity.movementSpeedMultiplier /= 1f - movementSlowPercentage;
        affectedEntity.rotationSpeedMultiplier /= 1f - rotationSlowPercentage;
        affectedEntity.OnSlowRemoved();
        dataContainer.slowParticle.Stop();
        controller.Log($"{affectedEntity.name} is no longer slowed.");
    }
}