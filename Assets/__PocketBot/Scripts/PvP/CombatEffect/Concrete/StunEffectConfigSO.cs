using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StunEffectConfigSO", menuName = "PocketBots/CombatEffectConfigSO/Stun")]
public class StunEffectConfigSO : CombatEffectConfigSO
{
    [SerializeField]
    private ParticleSystem m_StunParticlePrefab;

    public override CombatEffectStatuses effectStatus => CombatEffectStatuses.Stunned;
    public ParticleSystem stunParticlePrefab => m_StunParticlePrefab;
}