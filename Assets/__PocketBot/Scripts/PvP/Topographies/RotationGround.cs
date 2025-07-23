using HyrphusQ.Events;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RotationGround : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private OnTriggerCallback m_GroundOnTriggerCallBack;
    [SerializeField, BoxGroup("Ref")] private Rigidbody m_Rigidbody;
    [SerializeField, BoxGroup("Ref")] private Collider[] m_Colliders;

    private HashSet<int> m_ColliderIdHashSet;

    private void Awake()
    {
        m_GroundOnTriggerCallBack.isFilterByTag = false;
        m_GroundOnTriggerCallBack.onTriggerStay += OnTriggerStayCallback;
        m_GroundOnTriggerCallBack.onTriggerExit += OnTriggerExitCallback;
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private void OnDestroy()
    {
        m_GroundOnTriggerCallBack.onTriggerStay -= OnTriggerStayCallback;
        m_GroundOnTriggerCallBack.onTriggerExit -= OnTriggerExitCallback;
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private void Start()
    {
        m_ColliderIdHashSet = new HashSet<int>(m_Colliders.Select(collider => collider.GetInstanceID()));
        var triggerCollider = m_GroundOnTriggerCallBack.GetComponentInChildren<Collider>();
        foreach (var collider in m_Colliders)
            Physics.IgnoreCollision(triggerCollider, collider);
    }
    private void OnModelSpawned(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0 || parameters[0] is not PBRobot robot)
            return;
        var triggerCollider = m_GroundOnTriggerCallBack.GetComponentInChildren<Collider>();
        var colliders = robot.GetComponentsInChildren<Collider>().Where(collider => collider.attachedRigidbody != robot.ChassisInstance.CarPhysics.CarRb);
        foreach (var collider in colliders)
        {
            Physics.IgnoreCollision(triggerCollider, collider);
        }
    }
    private void OnTriggerStayCallback(Collider collider)
    {
        IBreakObstacle breakObstacle = collider.GetComponent<IBreakObstacle>();
        if (breakObstacle != null)
        {
            var obj = breakObstacle as Component;
            if (obj != null && obj.gameObject.transform.parent != transform)
            {
                obj.gameObject.transform.SetParent(transform);
            }
        }

        var part = collider.GetComponent<PBPart>();
        if (part == null || part.RobotChassis == null || part.RobotChassis.IsGripped)
            return;
        var chassis = part.RobotChassis;
        if (chassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID != default)
        {
            if (m_ColliderIdHashSet.Contains(chassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID))
            {
                EnterMovingGround(chassis);
            }
            else
            {
                ExitMovingGround(chassis);
            }
        }
    }
    private void OnTriggerExitCallback(Collider collider)
    {
        IBreakObstacle breakObstacle = collider.GetComponent<IBreakObstacle>();
        if (breakObstacle != null)
        {
            var obj = breakObstacle as Component;
            if (obj != null)
            {
                obj.gameObject.transform.SetParent(transform.parent);
            }
            return;
        }

        var part = collider.GetComponent<PBPart>();
        if (part == null || part.RobotChassis == null || part.RobotChassis.IsGripped)
            return;
        var chassis = part.RobotChassis;
        ExitMovingGround(chassis);
    }

    private void EnterMovingGround(PBChassis chassis)
    {
        var robot = chassis.Robot;
        if (robot.ChassisInstanceTransform.parent != transform)
        {
            robot.ChassisInstanceTransform.SetParent(transform);
        }
    }

    private void ExitMovingGround(PBChassis chassis)
    {
        var robot = chassis.Robot;
        if (robot.ChassisInstanceTransform.parent == transform)
        {
            robot.ChassisInstanceTransform.SetParent(robot.ChassisInstance.transform);
            robot.ChassisInstanceTransform.transform.localScale = Vector3.one;
        }
    }
}
