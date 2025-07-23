using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePuncherBehavior : PuncherBehaviour
{
    protected override void LateUpdate()
    {
        if (puncherJoint == null) return;

        if (m_DidPhysicUpdate)
        {
            m_ValidTargetLastFrame = m_ValidTargetThisFrame;
            m_ValidTargetThisFrame = false;
            m_DidPhysicUpdate = false;
        }
        puncherPipe.localScale = new Vector3(originLocalScale.x, originLocalScale.y, (pbCollider.transform.localPosition.z * 3) * originLocalScale.z);
    }

    protected override IEnumerator DOPunch_CR()
    {
        var waitUntilEnable = new WaitUntil(() => { return enabled; });
        var waitUntilHaveTarget = new WaitUntil(() => m_ValidTargetLastFrame && Time.time - m_ValidTargetEnterTime > validTargetTimeToAttack);
        var softJointLimit = puncherJoint.linearLimit;

        while (true)
        {
            if (puncherJoint == null)
                break;

            pbCollider.enabled = true;

            Vector3 scaleFactor = transform.localScale;
            float scaledDistance = Vector3.Distance(originPoint.position, destinationPoint.position) * scaleFactor.z;

            softJointLimit.limit = scaledDistance;

            m_IsPunching = true;
            puncherJoint.linearLimit = softJointLimit;

            Vector3 localDestination = originPoint.InverseTransformPoint(destinationPoint.position);
            puncherJoint.targetPosition = new Vector3(0, 0, -localDestination.z * transform.localScale.z);

            puncherJoint.zDrive = new JointDrive()
            {
                positionSpring = forwardJointDrive.positionSpring,
                positionDamper = forwardJointDrive.positionDamper,
                maximumForce = forwardJointDrive.maximumForce
            };

            yield return new WaitForSeconds(punchForwardDelayTime);

            pbCollider.enabled = false;

            Vector3 localOrigin = originPoint.InverseTransformPoint(originPoint.position);
            puncherJoint.targetPosition = new Vector3(0, 0, -localOrigin.z);
            m_IsPunching = false;
            puncherJoint.zDrive = new JointDrive()
            {
                positionSpring = backwardJointDrive.positionSpring,
                positionDamper = backwardJointDrive.positionDamper,
                maximumForce = backwardJointDrive.maximumForce
            };

            yield return new WaitForSeconds(goBackwardDelayTime);

            softJointLimit.limit = 0;
            puncherJoint.linearLimit = softJointLimit;

            yield return CustomWaitForSeconds(attackCycleTime - goBackwardDelayTime);
        }
    }
}
