using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using HyrphusQ.Events;

public class RotatingGround : MonoBehaviour, IDynamicGround
{
    private const float MaxRayDistance = 3f;

    [SerializeField, BoxGroup("Tweaks")]
    private Vector3 rotateSpeedAngles = Vector3.up * 90f;
    [SerializeField, BoxGroup("No Tweaks")]
    private Transform rotatingRoot;
    [SerializeField, BoxGroup("No Tweaks")]
    private Rigidbody rb;
    [SerializeField, BoxGroup("No Tweaks")]
    private OnTriggerCallback triggerCallback;
    [SerializeField, BoxGroup("No Tweaks")]
    private Collider[] colliders;

    private HashSet<int> colliderIdHashSet;

    private void Start()
    {
        var triggerCollider = triggerCallback.GetComponentInChildren<Collider>();
        foreach (var collider in colliders)
        {
            Physics.IgnoreCollision(triggerCollider, collider);
        }
        triggerCallback.isFilterByTag = false;
        triggerCallback.onTriggerStay += OnTriggerStayCallback;
        triggerCallback.onTriggerExit += OnTriggerExitCallback;
        colliderIdHashSet = new HashSet<int>(colliders.Select(collider => collider.GetInstanceID()));
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private void FixedUpdate()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotateSpeedAngles * Time.fixedDeltaTime));
        rotatingRoot.rotation = rb.rotation;
    }

    private void OnModelSpawned(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0 || parameters[0] is not PBRobot robot)
            return;
        var triggerCollider = triggerCallback.GetComponentInChildren<Collider>();
        var colliders = robot.GetComponentsInChildren<Collider>().Where(collider => collider.attachedRigidbody != robot.ChassisInstance.CarPhysics.CarRb);
        foreach (var collider in colliders)
        {
            Physics.IgnoreCollision(triggerCollider, collider);
        }
    }

    private void OnTriggerStayCallback(Collider other)
    {
        var part = other.GetComponent<PBPart>();
        if (part == null || part.RobotChassis == null || part.RobotChassis.IsGripped)
            return;
        var chassis = part.RobotChassis;
        if (chassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID != default)
        {
            if (colliderIdHashSet.Contains(chassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID))
            {
                EnterRotatingGround(chassis);
            }
            else
            {
                ExitRotatingGround(chassis);
            }
        }
    }

    private void OnTriggerExitCallback(Collider other)
    {
        var part = other.GetComponent<PBPart>();
        if (part == null || part.RobotBaseBody == null || part.RobotChassis.IsGripped)
            return;
        var chassis = part.RobotChassis;
        ExitRotatingGround(chassis);
    }

    private void EnterRotatingGround(PBChassis chassis)
    {
        var robot = chassis.Robot;
        if (robot.ChassisInstanceTransform.parent != rotatingRoot)
        {
            robot.ChassisInstanceTransform.SetParent(rotatingRoot);
        }
    }

    private void ExitRotatingGround(PBChassis chassis)
    {
        var robot = chassis.Robot;
        if (robot.ChassisInstanceTransform.parent == rotatingRoot)
        {
            robot.ChassisInstanceTransform.SetParent(robot.ChassisInstance.transform);
            robot.ChassisInstanceTransform.transform.localScale = Vector3.one;
        }
    }
}