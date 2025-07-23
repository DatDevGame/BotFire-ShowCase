using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CombatEffectConfigSO : ScriptableObject
{
    public abstract CombatEffectStatuses effectStatus { get; }
}