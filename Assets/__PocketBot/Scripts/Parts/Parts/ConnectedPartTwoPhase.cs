using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedPartTwoPhase : ConnectedPart
{
    private IPhaseTwoAttackable m_PhaseTwoAttackable;
    public IPhaseTwoAttackable GetIPhaseTwoAttackable()
    {
        IPhaseTwoAttackable IPhaseTwoAttackable = gameObject.GetComponentInChildren<IPhaseTwoAttackable>();
        if (m_PhaseTwoAttackable == null && IPhaseTwoAttackable != null)
            m_PhaseTwoAttackable = IPhaseTwoAttackable;

        return m_PhaseTwoAttackable;
    }
}

