using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockItem : MonoBehaviour, IDamagable
{
    public void ReceiveDamage(IAttackable attacker, float forceTaken) { }
}
