using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BoosterVFX : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> m_ParticleSystem;

    public virtual void SetMesh(MeshRenderer meshRenderer)
    {
        m_ParticleSystem.ForEach((v) =>
        {
            ParticleSystem.ShapeModule shapeModule = v.shape;
            shapeModule.shapeType = ParticleSystemShapeType.MeshRenderer;
            shapeModule.meshRenderer = meshRenderer;
        });
    }
    public virtual void SetMesh(SkinnedMeshRenderer skinedMesh)
    {
        m_ParticleSystem.ForEach((v) =>
        {
            ParticleSystem.ShapeModule shapeModule = v.shape;
            shapeModule.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
            shapeModule.skinnedMeshRenderer = skinedMesh;
        });
    }

    public virtual void OnVFX() 
    { 
        m_ParticleSystem
            .Where(x => !x.isPlaying)
            .ForEach(v => v.Play());
    }
    public virtual void OffVFX() { m_ParticleSystem.ForEach(v => v.Stop()); }
}
