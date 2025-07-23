using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public void ReceiveDamage(IAttackable attacker, float forceTaken);
}
