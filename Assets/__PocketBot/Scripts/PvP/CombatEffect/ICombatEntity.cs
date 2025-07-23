using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatEntity
{
    string name { get; set; }
    float maxHealth { get; set; }
    float currentHealth { get; set; }
    float damageMultiplier { get; set; }
    float damageTakenMultiplier { get; set; }
    float movementSpeedMultiplier { get; set; }
    float rotationSpeedMultiplier { get; set; }
    bool isStunned { get; set; }
    bool isDisarmed { get; set; }
    bool isInvincible { get; set; }
    Rigidbody rigidbody { get; }

    void OnSlowApplied();
    void OnSlowRemoved();
    void OnStunApplied();
    void OnStunRemoved();
    void OnDisarmApplied();
    void OnDisarmRemoved();
    void OnInvincibleApplied();
    void OnInvincibleRemoved();
    void OnAirborneApplied();
    void OnAirborneRemoved();
}