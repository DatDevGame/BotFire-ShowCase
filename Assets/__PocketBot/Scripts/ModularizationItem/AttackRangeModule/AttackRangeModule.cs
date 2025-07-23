using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackRangeModule : ItemModule
{
    [SerializeField]
    private bool isInfinityAttackRange;
    [SerializeField]
    private float attackRange;
    [SerializeField]
    private float attackAngles;

    public float AttackRange => attackRange;
    public float AttackAngles => attackAngles;
    public bool IsInfinityAttackRange => isInfinityAttackRange;
}