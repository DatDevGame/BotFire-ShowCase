using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using LatteGames;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;

public class PBChassis : PBPart
{
    [SerializeField] Rigidbody rb;
    [SerializeField] CarPhysics carPhysics;
    [SerializeField] PBRobotStatusVFX pBRobotStatusVFX;
    [SerializeField] List<PartContainer> partContainers;

    bool isGripped;
    float lastReceivedForceTime = 0f;
    PBRobot robot;
    Rigidbody[] rigidbodies;
    Collider[] colliders;
    PBAntiSlidingBox antiSlidingBox;
    Dictionary<PBPartSO, float> lastReceivedImpactDamageTimeDictionary = new Dictionary<PBPartSO, float>();
    CollisionData collisionData = new CollisionData();

    public override Rigidbody RobotBaseBody => rb;
    public override PBChassis RobotChassis => this;
    public CarPhysics CarPhysics => carPhysics;
    public PBRobotStatusVFX PbRobotStatusVFX => pBRobotStatusVFX;
    public PBChassisSO ChassisSO => PartSO != null ? PartSO.Cast<PBChassisSO>() : null;
    [ShowInInspector]
    public PBRobot Robot
    {
        get
        {
            if (robot == null)
                robot = GetComponentInParent<PBRobot>();
            return robot;
        }
    }
    public PBAntiSlidingBox AntiSlidingBox
    {
        get
        {
            if (antiSlidingBox == null)
                antiSlidingBox = GetComponentInChildren<PBAntiSlidingBox>();
            return antiSlidingBox;
        }
    }
    public bool IsGripped
    {
        get
        {
            return isGripped;
        }
        set
        {
            isGripped = value;
        }
    }
    public List<PartContainer> PartContainers => partContainers;
    public Rigidbody[] Rigidbodies
    {
        get
        {
            if (rigidbodies == null)
            {
                rigidbodies = GetComponentsInChildren<Rigidbody>(true);
            }
            return rigidbodies;
        }
    }
    public Collider[] Colliders
    {
        get
        {
            if (colliders == null)
            {
                colliders = GetComponentsInChildren<Collider>(true);
            }
            return colliders;
        }
    }

    void Awake()
    {
        SubscribeEvents();
    }

    void OnDestroy()
    {
        UnsubscribeEvents();
    }

    void HandlePartCollide(params object[] parameters)
    {
        CollisionInfo collisionInfo;
        // Can't self harm, if attacker is this robot
        if (parameters[0] is not PBPart attackerPart || attackerPart.RobotBaseBody.Equals(rb))
            return;
        if (parameters[1] is Collision collision)
        {
            collisionInfo = new(collision);
        }
        else if (parameters[1] is CollisionInfo _collisionInfo)
        {
            collisionInfo = _collisionInfo;
        }
        else
            return;
        if (collisionInfo.body == null)
            return;
        if (!collisionInfo.body.TryGetComponent<PBPart>(out var damageReceiverPart))
            return;

        if (damageReceiverPart == null || damageReceiverPart.RobotBaseBody == null || rb == null)
            return;
        // If damage receiver robot is not this robot
        if (damageReceiverPart.RobotBaseBody.Equals(rb) == false)
            return;

        PBChassis attackerChassis = attackerPart.RobotChassis;
        float attackerOverallScore = attackerChassis == null ? Robot.HighestOverallScore : attackerChassis.Robot.RobotStatsSO.value;
        float originManualForce = (float)Convert.ToDouble(parameters[2]);
        float forceMultiplier = Robot.ForceAdjustmentCurve.Evaluate(attackerOverallScore / Robot.HighestOverallScore) * MovementGlobalConfigs.forceScale;
        float manualForce = originManualForce * forceMultiplier;
        bool isImpactWeapon = originManualForce > 1;

        UpdateCollisionData();
        ApplyForce();
        ApplyDamage();

        void ApplyForce()
        {
            if (originManualForce <= 0 || Time.fixedTime - lastReceivedForceTime < Const.CollideValue.ReceiveCollisionForceCooldown)
                return;
            if (isImpactWeapon && Time.time - lastReceivedImpactDamageTimeDictionary.Get(attackerPart.PartSO) < Const.CollideValue.ImpactDamageCooldown)
                return;
            lastReceivedForceTime = Time.fixedTime;
            if (parameters.Length >= 4 && parameters[3] is Action<CollisionData> applyCollisionForceCallback)
                applyCollisionForceCallback?.Invoke(collisionData);
            else
                rb.AddForceAtPosition(manualForce * rb.mass * collisionInfo.relativeVelocity.normalized, collisionInfo.contactPoint, ForceMode.Impulse);
            StartCoroutine(WaitForNextFixedTimestep_CR());
        }

        void ApplyDamage()
        {
            float impactDamageCooldown = Const.CollideValue.ImpactDamageCooldown;
            if (isImpactWeapon) //Is impact weapon
            {
                IBoostFireRate boostFireRate = attackerPart.GetComponentInChildren<IBoostFireRate>();
                if (boostFireRate != null && boostFireRate.IsSpeedUp())
                    impactDamageCooldown -= Const.CollideValue.ImpactDamageCooldown * boostFireRate.GetPercentSpeedUp();

                var currentTime = Time.time;
                if (currentTime - lastReceivedImpactDamageTimeDictionary.Get(attackerPart.PartSO) < impactDamageCooldown) return; //Cooldown when inflict damaged
                lastReceivedImpactDamageTimeDictionary.Set(attackerPart.PartSO, currentTime);
                SoundManager.Instance.PlaySFX(SFX.BladeHit);
                //LGDebug.Log($"{name} received damage from {attackerPart} ({attackerPart.GetInstanceID()}): {Time.fixedTime} - {Time.time}");
            }
            // The collided part receive damage instead of chassis
            collisionInfo.body.GetComponent<PBPart>().ReceiveDamage(attackerPart, manualForce);
        }

        void UpdateCollisionData()
        {
            collisionData.chassisRb = rb;
            collisionData.collisionInfo = collisionInfo;
            collisionData.originManualForce = originManualForce;
            collisionData.forceMultiplier = forceMultiplier;
            collisionData.manualForce = manualForce;
        }

        IEnumerator WaitForNextFixedTimestep_CR()
        {
            yield return CommonCoroutine.FixedUpdate;
            yield return CommonCoroutine.FixedUpdate;
            var velocityLength = rb.velocity.magnitude;
            GameEventHandler.Invoke(RobotStatusEventCode.OnRobotReceiveForce, this, attackerChassis, velocityLength);
            //LGDebug.Log($"{name} received force from {attackerPart} ({Time.fixedTime}): {originManualForce} - {manualForce} - {velocityLength}", context: this);
        }
    }

    public void SubscribeEvents()
    {
        GameEventHandler.AddActionEvent(CollisionEventCode.OnPartCollide, HandlePartCollide);
    }

    public void UnsubscribeEvents()
    {
        GameEventHandler.RemoveActionEvent(CollisionEventCode.OnPartCollide, HandlePartCollide);
    }

    [Serializable]
    public struct PartContainer
    {
        public PBPartSlot PartSlotType;
        public List<Transform> Containers;
    }
}

public class CollisionInfo
{
    public Rigidbody body;
    public Vector3 relativeVelocity;
    public Vector3 contactPoint;

    #region Constructor
    public CollisionInfo(Collision collision)
    {
        body = collision.rigidbody;
        relativeVelocity = collision.relativeVelocity;
        contactPoint = collision.GetContact(0).point;
    }

    public CollisionInfo(Rigidbody body, Vector3 relativeVelocity, Vector3 contactPoint)
    {
        this.body = body;
        this.relativeVelocity = relativeVelocity;
        this.contactPoint = contactPoint;
    }
    #endregion
}

public class CollisionData
{
    public Rigidbody chassisRb;
    public CollisionInfo collisionInfo;
    public float originManualForce;
    public float forceMultiplier;
    public float manualForce;
}