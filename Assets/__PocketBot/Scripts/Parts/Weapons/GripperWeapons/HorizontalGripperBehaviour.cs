using System.Linq;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using UnityEngine;
using System;
public class HorizontalGripperBehaviour : GripperBehaviour
{
    public Action<float, HorizontalGripperBehaviour> OnSetLastTime;
    [HideInInspector] public bool isRunnable;
    protected bool isFollowAnother;
    protected float lastTime;
    public float LastTime
    {
        set => lastTime = value;
    }
    protected override void Awake()
    {
        base.Awake();
    }
    protected override IEnumerator Start()
    {
        if (isModifyMassScale)
        {
            var configurableJoint = GetComponent<ConfigurableJoint>();
            // Dirty fix update mass scale depend on mass of chassis
            if (rb.mass > 1 && configurableJoint.massScale != 1f)
                configurableJoint.massScale = configurableJoint.connectedBody.mass / rb.mass;
        }
        enabled = false;
        yield return new WaitUntil(() => isRunnable);
        enabled = true;
        yield return StartBehaviour_CR();
    }
    public override void GripTarget()
    {
        base.GripTarget();
        liftTransform.rotation = rb.transform.rotation;
    }
    public virtual float RandomDelayTime()
    {
        return GetRandomDelayTime();
    }
    protected override IEnumerator StartBehaviour_CR()
    {
        lastTime = Time.time;
        var releaseConditions = new WaitUntil(() => (Time.time - gripTimeStamp >= holdDuration) || !isGripAnyone || PbPart.RobotChassis.IsGripped || PbPart.RobotChassis.Robot.IsDead || grippedRobot.ChassisInstance.Robot.IsDead);
        var changeMomentumDirCondition = new WaitUntil(ChangeMomentumDirCondition);
        var followAnotherCondition = new WaitUntil(() => !isFollowAnother);
        while (true)
        {
            lastTime = Time.time;
            OnSetLastTime?.Invoke(lastTime, this);
            ChangeMomentumDirection();
            yield return changeMomentumDirCondition;
            // Create joint and attach robot to joint
            if (isGripAnyone)
            {
                if (!grippedRobot.IsDead)
                {
                    GripTarget();
                    yield return releaseConditions;
                    ReleaseTarget();
                }
                else
                {
                    grippedRobot = null;
                    isGripAnyone = grippedRobot != null;
                    OnReleased?.Invoke(this);
                }
            }
            if (isFollowAnother)
            {
                yield return followAnotherCondition;
            }
            // Release joint and deattach robot from joint
            ChangeMomentumDirection();
            yield return new WaitForSeconds(attackCycleTime);
        }
        bool ChangeMomentumDirCondition()
        {
            return isGripAnyone || Time.time - lastTime >= (gripDuration + 1f) || isFollowAnother;
        }
    }
    public override bool IsAbleToDealDamage(Collision collision, out QueryResult queryResult)
    {
        if (isFollowAnother)
        {
            queryResult = QueryResult.Default;
            return false;
        }
        var result = base.IsAbleToDealDamage(collision, out QueryResult m_queryResult);
        queryResult = m_queryResult;
        return result;
    }
    protected override void FixedUpdate()
    {
        if (isFollowAnother)
        {
            rb.transform.rotation = liftTransform.rotation;
        }
        else
        {
            base.FixedUpdate();
        }
    }
    public virtual void HandleWhenAnotherGripped()
    {
        liftTransform.rotation = rb.transform.rotation;
        foreach (var item in colliders)
        {
            item.enabled = false;
        }
        isFollowAnother = true;
    }
    public virtual void HandleWhenAnotherRelease()
    {
        foreach (var item in colliders)
        {
            item.enabled = true;
        }
        isFollowAnother = false;
    }
}