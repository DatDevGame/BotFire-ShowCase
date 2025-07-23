using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames;
using LatteGames.Template;
using UnityEngine;

public class AcceleratingPad : UtilityProps
{
    private const float TweentyFrame = 20 / 60f;
    private const float DurationDisableSteeringForce = 0.35f;

    [SerializeField]
    private Vector3 forceVector = Vector3.forward + Vector3.up;
    [SerializeField]
    private MeshRenderer padMeshRenderer;

    private Vector2 offsetOverTime;
    private Material boostSignMaterial;
    private Dictionary<Rigidbody, float> lastTimeBoostSpeedDict = new Dictionary<Rigidbody, float>();

    private void Start()
    {
        offsetOverTime = new Vector2(0f, 1f / 3f);
        var sharedMaterials = padMeshRenderer.sharedMaterials;
        sharedMaterials[0] = Instantiate(sharedMaterials[0]);
        padMeshRenderer.sharedMaterials = sharedMaterials;
        boostSignMaterial = sharedMaterials[0];
    }

    private void OnTriggerStay(Collider collider)
    {
        var pbPart = collider.GetComponent<PBPart>();
        if (collider == null || pbPart == null || pbPart.RobotBaseBody != collider.attachedRigidbody)
        {
            return;
        }
        var contactPoint = collider.ClosestPoint(transform.position);
        BoostSpeedTarget(pbPart.RobotChassis.Robot, contactPoint);
    }

    private void OnTriggerExit(Collider collider)
    {
        var pbPart = collider.GetComponent<PBPart>();
        if (collider == null || pbPart == null || pbPart.RobotBaseBody != collider.attachedRigidbody)
        {
            return;
        }
        RemoveEnterPointRobot(pbPart.RobotChassis.Robot);
    }

    private void Update()
    {
        boostSignMaterial.mainTextureOffset -= offsetOverTime * Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, GetForceVector());
    }

    public Vector3 GetForceVector()
    {
        return transform.TransformDirection(forceVector.normalized) * forceVector.magnitude;
    }

    public Vector3 GetForceDirection()
    {
        return transform.TransformDirection(forceVector.normalized);
    }

    public bool IsAbleToBoost(Rigidbody targetRb)
    {
        return Time.time - lastTimeBoostSpeedDict.Get(targetRb) > TweentyFrame;
    }

    public void BoostSpeedTarget(PBRobot robot, Vector3 contactPoint)
    {
        var targetRb = robot.ChassisInstance.RobotBaseBody;
        if (!IsAbleToBoost(targetRb))
            return;
        lastTimeBoostSpeedDict.Set(targetRb, Time.time);
        var localPos = transform.InverseTransformPoint(contactPoint);
        var forceDir = GetForceDirection();
        var forceMag = forceVector.magnitude - localPos.z;
        // Assign velocity directly instead of AddForce
        targetRb.velocity = forceMag * forceDir;
        // Get car physics & disable steering force in periods
        var carPhysics = robot.ChassisInstance.CarPhysics;
        carPhysics.TireGripFactor = 0f;
        carPhysics.LockBrakeTime = DurationDisableSteeringForce;
        StartCoroutine(CommonCoroutine.Delay(DurationDisableSteeringForce, false, () =>
        {
            carPhysics.TireGripFactor = 1f;
        }));
        AddEnterPointRobot(robot);
        SoundManager.Instance.PlaySFX(GeneralSFX.UIDropBox);
        //Debug.Log($"Boost speed {targetRb} - {localPos} - {forceMag} - {Time.time} - {Time.fixedTime}");
    }
}