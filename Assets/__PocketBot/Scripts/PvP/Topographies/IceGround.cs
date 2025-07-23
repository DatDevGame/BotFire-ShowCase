using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using UnityEngine;

public class IceGround : MonoBehaviour
{
    private const float MaxRayDistance = 3f;
    private const float IceCarTopSpeedMultiplier = 0.375f;
    private const float IceFrontTireGripMultiplier = 0.05f;
    private const float IceRearTireGripMultiplier = 0.1f;

    [SerializeField]
    private GameObject _iceEffectPrefab;
    [SerializeField]
    private OnTriggerCallback triggerCallback;
    [SerializeField]
    private Collider[] colliders;

    private HashSet<int> colliderIdHashSet;
    private Dictionary<PBRobot, bool> robotEnterStateDict = new();

    private void Awake()
    {
        var triggerCollider = triggerCallback.GetComponentInChildren<Collider>();
        var ignoreColliders = new List<Collider>(colliders);
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

    private void OnTriggerStayCallback(Collider collider)
    {
        var part = collider.GetComponent<PBPart>();
        if (part == null || part.RobotChassis == null)
            return;
        var chassis = part.RobotChassis;
        if (chassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID != default)
        {
            if (colliderIdHashSet.Contains(chassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID))
            {
                EnterIceGround(chassis);
            }
            else
            {
                ExitIceGround(chassis);
            }
        }
    }

    private void OnTriggerExitCallback(Collider collider)
    {
        var part = collider.GetComponent<PBPart>();
        if (part == null || part.RobotChassis == null)
            return;
        var chassis = part.RobotChassis;
        ExitIceGround(chassis);
    }

    private void EnterIceGround(PBChassis chassis)
    {
        if (chassis.CarPhysics.Drag != 0.5f)
        {
            chassis.CarPhysics.Drag = 0.5f;
        }
        if (robotEnterStateDict.Get(chassis.Robot))
            return;
        chassis.CarPhysics.Drag = 0.5f;
        chassis.CarPhysics.CarTopSpeedMultiplier *= IceCarTopSpeedMultiplier;
        chassis.CarPhysics.FrontTireGripMultiplier = IceFrontTireGripMultiplier;
        chassis.CarPhysics.RearTireGripMultiplier = IceRearTireGripMultiplier;
        robotEnterStateDict.Set(chassis.Robot, true);

        AddVFX(chassis);
    }

    private void ExitIceGround(PBChassis chassis)
    {
        if (!robotEnterStateDict.Get(chassis.Robot))
            return;
        chassis.CarPhysics.Drag = 2f;
        chassis.CarPhysics.CarTopSpeedMultiplier /= IceCarTopSpeedMultiplier;
        chassis.CarPhysics.FrontTireGripMultiplier = 1f;
        chassis.CarPhysics.RearTireGripMultiplier = 1f;
        robotEnterStateDict.Set(chassis.Robot, false);

        RemoveVFX(chassis);
    }

    private void AddVFX(PBChassis pBChassis)
    {
        for (int i = 0; i < pBChassis.PartContainers.Count; i++)
        {
            if (pBChassis.PartContainers[i].PartSlotType == PBPartSlot.Wheels_1 ||
                pBChassis.PartContainers[i].PartSlotType == PBPartSlot.Wheels_2 ||
                pBChassis.PartContainers[i].PartSlotType == PBPartSlot.Wheels_3)
            {
                List<Transform> wheels = new List<Transform>();
                wheels = pBChassis.PartContainers[i].Containers;
                for (int x = 0; x < wheels.Count; x++)
                {
                    var sandVFX = Instantiate(_iceEffectPrefab, pBChassis.transform);
                    sandVFX.transform.SetParent(wheels[x]);
                    sandVFX.transform.localPosition = new Vector3(0, -0.2f, 0.2f);

                    IceEffect sandScript = sandVFX.GetOrAddComponent<IceEffect>();
                    sandScript.SetUpEffect(pBChassis.RobotBaseBody);
                }

            }
        }
    }

    private void RemoveVFX(PBChassis pBChassis)
    {
        var listVFX = pBChassis.GetComponentsInChildren<IceEffect>();
        for (int i = 0; i < listVFX.Length; i++)
        {
            listVFX[i].transform.GetComponentInChildren<ParticleSystem>().loop = false;
            listVFX[i].transform.SetParent(pBChassis.transform.parent);
            Destroy(listVFX[i].gameObject, 1);
        }
    }
}