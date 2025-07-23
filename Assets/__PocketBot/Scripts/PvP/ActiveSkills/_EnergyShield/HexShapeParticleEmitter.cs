using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

public class HexShapeParticleEmitter : MonoBehaviour
{
    [System.Serializable]
    public class ParticleCluster
    {
        public float m_DelayTime = 0.05f;
        public float m_StartSize = 1;
        public float m_StartLifetime = 1f;
        public Color m_StartColor = Color.white;
    }

    [SerializeField]
    private bool m_OverrideHexSize;
    [SerializeField]
    private ParticleSystem m_HexParticle;
    [SerializeField]
    private HexWaveGenerator m_HexGenerator;
    [SerializeField]
    private List<ParticleCluster> m_ParticleClusters;

    private float m_OriginHexSize;

    private void Awake()
    {
        m_OriginHexSize = m_HexGenerator.hexSize;
    }

    private IEnumerator PlayFX_CR()
    {
        m_HexParticle.Stop();
        m_HexParticle.Play();
        if (m_OriginHexSize > 0f && m_OverrideHexSize)
            m_HexGenerator.hexSize = m_OriginHexSize * transform.localScale.x;
        for (int i = 0; i < m_ParticleClusters.Count; i++)
        {
            if (!Mathf.Approximately(m_ParticleClusters[i].m_DelayTime, 0f))
                yield return new WaitForSeconds(m_ParticleClusters[i].m_DelayTime);
            Vector3 particleStartSize3D = m_HexParticle.main.startSize3D ? new Vector3(m_HexParticle.main.startSizeX.constant, m_HexParticle.main.startSizeY.constant, m_HexParticle.main.startSizeZ.constant) * m_ParticleClusters[i].m_StartSize : Vector3.one * m_ParticleClusters[i].m_StartSize * m_HexParticle.main.startSize.constant;
            float particleStartLifetime = m_ParticleClusters[i].m_StartLifetime;
            Color particleStartColor = (m_ParticleClusters[i].m_StartColor.ToFloat4() * m_HexParticle.main.startColor.color.ToFloat4()).ToColor();
            HexCoords hexCoords = HexCoords.FromWorldPosition(transform.position, m_HexGenerator.hexSize);
            List<HexCoords> ringCoords = m_HexGenerator.GetHexRing(hexCoords, i);
            for (int j = 0; j < ringCoords.Count; j++)
            {
                Vector3 worldPoint = ringCoords[j].ToWorldPosition(m_HexGenerator.hexSize);
                worldPoint.y = transform.position.y;
                ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
                emitParams.position = worldPoint;
                emitParams.startSize3D = particleStartSize3D;
                emitParams.startLifetime = particleStartLifetime;
                emitParams.startColor = particleStartColor;
                m_HexParticle.Emit(emitParams, 1);
            }
        }
    }

    [Button]
    public void PlayFX()
    {
        StopAllCoroutines();
        StartCoroutine(PlayFX_CR());
        GetComponentsInChildren<HexShapeParticleEmitter>().Except(new HexShapeParticleEmitter[] { this }).ForEach(item => item.PlayFX());
    }
}