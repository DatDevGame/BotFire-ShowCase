using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeStormSmasherBehavior : SmasherBehaviour
{
    [SerializeField, BoxGroup("Config")] private float m_WaitContinous = 1;
    [SerializeField, BoxGroup("Config")] private float m_BladeStormContinousDuration = 3;

    [SerializeField, BoxGroup("Ref")] private BladeStormImpactCollider m_BladeStormImpactCollider;
    [SerializeField, BoxGroup("Ref")] private BladeStormContinousCollider m_BladeStormContinousCollider;

    protected override IEnumerator StartBehaviour_CR()
    {
        while (true)
        {
            _isEnable = true;
            ChangeMomentumDirection();

            yield return new WaitForSeconds(m_WaitContinous);
            m_BladeStormContinousCollider.EnableContinous();
            yield return new WaitForSeconds(m_BladeStormContinousDuration);
            m_BladeStormContinousCollider.DisableContinous();
            yield return new WaitForSeconds(timeToLiftUp);

            _isEnable = torqueDownSpeed <= 0 && torqueUpSpeed <= 0 ? true : false;
            ChangeMomentumDirection();

            yield return CustomWaitForSeconds(attackCycleTime);
        }
    }
}    