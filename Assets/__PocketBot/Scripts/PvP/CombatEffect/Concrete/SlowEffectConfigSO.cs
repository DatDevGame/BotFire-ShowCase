using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SlowEffectConfigSO", menuName = "PocketBots/CombatEffectConfigSO/Slow")]
public class SlowEffectConfigSO : CombatEffectConfigSO
{
    [SerializeField]
    private ParticleSystem m_SlowParticlePrefab;

    public override CombatEffectStatuses effectStatus => CombatEffectStatuses.Slowed;
    public ParticleSystem slowParticlePrefab => m_SlowParticlePrefab;
}