using System.Linq;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Events;
public class GripperBehaviour : SmasherBehaviour, IBoostFireRate
{
    public UnityEvent<PBRobot, GripperBehaviour> OnGripped;
    public UnityEvent<GripperBehaviour> OnReleased;
    protected const float MAX_VELOCITY = 30;
    protected const float MAX_ROTATION_DEGREE = 8;
    protected const float DROP_TARGET_VELOCITY = 33;
    protected const float ROTATE_UP = 30;
    protected const float GRIPPED_DRAG = 5.69f;
    protected const float GRIPPED_ANGULAR_DRAG = 30.69f;
    protected const float DROP_TARGET_DISTANCE_THRESHOLD = 1f;
    [SerializeField] protected float gripDuration;
    [SerializeField] protected float holdDuration = 50;
    [SerializeField] protected float liftY = 0.5f;
    [SerializeField] protected float pushZ = 0.5f;
    [SerializeField] protected BoxCollider validBoxCollider;
    [SerializeField] protected BoxCollider minimumBoxCollider;
    protected List<Collider> colliders;
    protected bool isGripAnyone;
    protected PBRobot grippedRobot;
    protected float originalMass;
    protected float originalDrag;
    protected float originalAngularDrag;
    protected Transform grippedTargetTransform;
    protected Transform liftTransform;
    protected CollisionDetectionMode originalTargetCollisionDetectionMode;
    protected SmoothingBoolBuffer dropTargetSmoothBuffer = new SmoothingBoolBuffer(5, false);
    protected float gripTimeStamp;
    protected Rigidbody carRB => PbPart.RobotChassis.RobotBaseBody;
    protected Rigidbody targetRB => grippedRobot.ChassisInstance.CarPhysics.CarRb;
    private float m_AttackCycleTimeOrginal;
    protected override void Awake()
    {
        m_AttackCycleTimeOrginal = attackCycleTime;
        base.Awake();
        CreateGrippedTargetTransform();
        CreateLiftTransform();
        colliders = new List<Collider>(GetComponentsInChildren<Collider>());
    }
    protected virtual void CreateGrippedTargetTransform()
    {
        grippedTargetTransform = new GameObject("GrippedTargetTransform").transform;
        grippedTargetTransform.SetParent(transform.parent);
        grippedTargetTransform.position = transform.position;
        grippedTargetTransform.rotation = transform.rotation;
    }
    protected virtual void CreateLiftTransform()
    {
        liftTransform = new GameObject("LiftTransform").transform;
        liftTransform.SetParent(transform.parent);
        liftTransform.position = transform.position;
    }
    protected override IEnumerator StartBehaviour_CR()
    {
        var lastTime = Time.time;
        var releaseConditions = new WaitUntil(() => (Time.time - gripTimeStamp >= holdDuration) || !isGripAnyone || PbPart.RobotChassis.IsGripped || PbPart.RobotChassis.Robot.IsDead || grippedRobot.ChassisInstance.Robot.IsDead);
        var changeMomentumDirCondition = new WaitUntil(ChangeMomentumDirCondition);
        while (true)
        {
            lastTime = Time.time;
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
                }
            }
            // Release joint and deattach robot from joint
            ChangeMomentumDirection();
            yield return new WaitForSeconds(attackCycleTime);
        }
        bool ChangeMomentumDirCondition()
        {
            return isGripAnyone || Time.time - lastTime >= (gripDuration + 1f);
        }
    }
    public override bool IsAbleToDealDamage(Collision collision, out QueryResult queryResult)
    {
        if (isGripAnyone)
        {
            queryResult = QueryResult.Default;
            return false;
        }
        var isDealDamage = base.IsAbleToDealDamage(collision, out queryResult);
        if (isDealDamage)
        {
            var chassis = collision?.rigidbody.GetComponent<PBPart>().RobotChassis;
            if (chassis == null || chassis.AntiSlidingBox == null)
                return false;
            var targetBounds = chassis.AntiSlidingBox.BoxCollider.bounds;
            var checkingBounds = new Bounds(targetBounds.center, new Vector3(targetBounds.size.x, collision.collider.bounds.size.y, targetBounds.size.z));
            if (validBoxCollider.bounds.Intersects(checkingBounds))
            {
                grippedRobot = chassis?.Robot;
                isGripAnyone = grippedRobot != null;
                grippedTargetTransform.transform.position = targetRB.transform.position + Vector3.up * liftY;
                if (minimumBoxCollider != null && minimumBoxCollider.bounds.Intersects(checkingBounds))
                {
                    grippedTargetTransform.transform.position += transform.parent.forward * pushZ;
                }
                grippedTargetTransform.transform.rotation = targetRB.transform.rotation;
                OnGripped?.Invoke(grippedRobot, this);
            }
        }
        return isDealDamage;
    }
    public virtual void GripTarget()
    {
        //Do for gripping
        if (grippedRobot.ChassisInstance.AntiSlidingBox != null)
        {
            grippedRobot.ChassisInstance.AntiSlidingBox.gameObject.SetActive(false);
        }
        grippedRobot.ChassisInstance.CarPhysics.enabled = false;
        originalTargetCollisionDetectionMode = targetRB.collisionDetectionMode;
        targetRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
        grippedRobot.EnabledAllParts(false);
        originalMass = carRB.mass;
        carRB.mass = targetRB.mass + originalMass;
        grippedRobot.ChassisInstance.IsGripped = true;
        gripTimeStamp = Time.time;
        liftTransform.rotation = transform.rotation;
        liftTransform.Rotate(Vector3.left * ROTATE_UP);
        foreach (var item in colliders)
        {
            item.enabled = false;
        }
    }
    protected virtual void ReleaseTarget()
    {
        dropTargetSmoothBuffer.Reset();
        if (!isGripAnyone || !grippedRobot.ChassisInstance.IsGripped)
        {
            return;
        }
        targetRB.collisionDetectionMode = originalTargetCollisionDetectionMode;
        grippedRobot.EnabledAllParts(true);
        if (!grippedRobot.ChassisInstance.Robot.IsDead)
        {
            if (grippedRobot.ChassisInstance.AntiSlidingBox != null)
            {
                grippedRobot.ChassisInstance.AntiSlidingBox.gameObject.SetActive(true);
            }
            targetRB.drag = originalDrag;
            targetRB.angularDrag = originalAngularDrag;
            grippedRobot.ChassisInstance.CarPhysics.enabled = true;
            targetRB.velocity = Vector3.zero;
        }
        foreach (var item in colliders)
        {
            item.enabled = true;
        }
        carRB.mass = originalMass;
        grippedRobot.ChassisInstance.IsGripped = false;
        grippedRobot = null;
        isGripAnyone = grippedRobot != null;
        OnReleased?.Invoke(this);
    }
    protected override void FixedUpdate()
    {
        if (isGripAnyone)
        {
            var grippedPos = grippedTargetTransform.transform.position;
            var grippedRot = grippedTargetTransform.rotation;
            var diff = grippedPos - targetRB.position;
            rb.transform.rotation = liftTransform.rotation;
            targetRB.MoveRotation(Quaternion.RotateTowards(targetRB.transform.rotation, grippedRot, MAX_ROTATION_DEGREE));
            targetRB.velocity = Vector3.ClampMagnitude(diff / Time.fixedDeltaTime, MAX_VELOCITY);
            if (targetRB.drag != GRIPPED_DRAG)
            {
                originalDrag = targetRB.drag;
                targetRB.drag = GRIPPED_DRAG;
            }
            if (targetRB.angularDrag != GRIPPED_ANGULAR_DRAG)
            {
                originalAngularDrag = targetRB.angularDrag;
                targetRB.angularDrag = GRIPPED_ANGULAR_DRAG;
            }
            //Recalculate diff after moving the target
            diff = grippedPos - targetRB.position;
            if (diff.magnitude > DROP_TARGET_DISTANCE_THRESHOLD || targetRB.velocity.magnitude > DROP_TARGET_VELOCITY || PbPart.RobotChassis.RobotBaseBody.velocity.magnitude > DROP_TARGET_VELOCITY)
            {
                dropTargetSmoothBuffer.Add(true);
                var trueValues = 0;
                foreach (var item in dropTargetSmoothBuffer.m_BoolBuffer)
                {
                    if (item == true)
                    {
                        trueValues++;
                    }
                }
                if (dropTargetSmoothBuffer.Get())
                {
                    ReleaseTarget();
                }
            }
            else
            {
                dropTargetSmoothBuffer.Add(false);
            }
        }
        else
        {
            rb.AddTorque(smashValue * Time.deltaTime * torqueValue * GetRigidbodyAxes(), ForceMode.VelocityChange);
        }
    }
    protected virtual void OnDestroy()
    {
        ReleaseTarget();
    }
    public override void BoostSpeedUpFire(float boosterPercent)
    {
        //Enable VFX
        if (m_MeshRendererBooster != null && m_MeshRendererBooster.Count > 0)
            PBBoosterVFXManager.Instance.PlayBoosterSpeedUpAttackWeapon(pbPart, m_MeshRendererBooster);
        m_StackSpeedUp++;
        m_IsSpeedUp = true;
        float minAttackCycleTime = 0.1F;
        attackCycleTime -= (m_AttackCycleTimeOrginal * boosterPercent);
        if (attackCycleTime <= 0)
            attackCycleTime = minAttackCycleTime;
    }
    public void BoostSpeedUpStop()
    {
        //Disable VFX
        PBBoosterVFXManager.Instance.StopBoosterSpeedUpAttackWeapon(pbPart);
        m_StackSpeedUp = 0;
        m_IsSpeedUp = false;
        attackCycleTime = m_AttackCycleTimeOrginal;
    }
    public override bool IsSpeedUp() => m_IsSpeedUp;
    public override int GetStackSpeedUp() => m_StackSpeedUp;
}