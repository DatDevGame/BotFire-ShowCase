using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum CombatEffectStatuses
{
    None = 0,
    Immobilized = 1 << 0,
    Stunned = 1 << 1,
    Invincible = 1 << 2,
    Slowed = 1 << 3 | Stunned,
    Disarmed = 1 << 4,
    Airborne = 1 << 5,
    // Status effect in the future
    // Silenced,
    // Vulnerable,
}