using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatHammerSmasherBehavior : SmasherBehaviour
{
    [SerializeField, BoxGroup("Config")] protected float m_StartDelay = 3;
    [SerializeField, BoxGroup("Config")] protected DOTweenAnimation m_WarningColorAnimation;

    protected override IEnumerator Start()
    {
        if (isModifyMassScale)
        {
            var configurableJoint = GetComponent<ConfigurableJoint>();
            if (rb.mass > 1 && configurableJoint.massScale != 1f)
                configurableJoint.massScale = configurableJoint.connectedBody.mass / rb.mass;
        }
        if (smashValue <= 0)
        {
            torqueValue = torqueUpSpeed;
        }
        else
        {
            torqueValue = torqueDownSpeed;
        }

        rb.isKinematic = true;
        enabled = false;
        yield return new WaitForSeconds(m_StartDelay);
        rb.isKinematic = false;
        enabled = true;
        m_WarningColorAnimation.DOPlay();
        yield return StartBehaviour_CR();
    }

    protected override IEnumerator StartBehaviour_CR()
    {
        bool isTheFirstTiem = false;
        while (true)
        {
            if (!isTheFirstTiem)
            {
                isTheFirstTiem = true;
                m_WarningColorAnimation.DOPlay();
                yield return new WaitForSeconds(0.5f);

            }

            _isEnable = true;
            ChangeMomentumDirection();
            yield return new WaitForSeconds(timeToLiftUp);
            m_WarningColorAnimation.DORestart();
            m_WarningColorAnimation.DOPause();

            _isEnable = torqueDownSpeed <= 0 && torqueUpSpeed <= 0;
            ChangeMomentumDirection();

            yield return new WaitForSeconds(attackCycleTime * 0.5F);
            m_WarningColorAnimation.DOPlay();
            yield return new WaitForSeconds(attackCycleTime * 0.5F);
            m_WarningColorAnimation.DORestart();
            m_WarningColorAnimation.DOPause();
        }
    }

}
