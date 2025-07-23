using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using UnityEngine;

public class MudGround : MonoBehaviour
{
    private const float MaxRayDistance = 1.5f;

    [SerializeField] private GameObject _mudEffectPrefab;
    [SerializeField] private GameObject _mudSkidMarkPrefab;

    [SerializeField]
    private OnTriggerCallback triggerCallback;
    [SerializeField]
    private Collider[] colliders;

    private HashSet<int> colliderIdHashSet;
    private Dictionary<PBRobot, bool> robotEnterStateDict = new();

    private void Awake()
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
                EnterMudGround(chassis);
            }
            else
            {
                ExitMudGround(chassis);
            }
        }
    }

    private void AddVFX(PBChassis pBChassis)
    {
        //Add Skin Mark
        GameObject mudSkinMark = Instantiate(_mudSkidMarkPrefab, pBChassis.CarPhysics.transform);
        mudSkinMark.transform.localPosition = new Vector3(0, -0.5f, -1);
        mudSkinMark.transform.localEulerAngles = Vector3.zero;
        var midSkiMark = mudSkinMark.GetComponentInChildren<ParticleSystem>();
        midSkiMark.Play();

        //Add Mud Effect
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
                    var sandVFX = Instantiate(_mudEffectPrefab, pBChassis.transform);
                    sandVFX.transform.SetParent(wheels[x]);
                    sandVFX.transform.localPosition = new Vector3(0, -0.3f, -0.6f);
                    sandVFX.transform.localEulerAngles = Vector3.zero;

                    MudEffect sandScript = sandVFX.GetOrAddComponent<MudEffect>();
                    sandScript.SetUpEffect(pBChassis.RobotBaseBody);
                }

            }
        }
    }
    private void RemoveVFX(PBChassis pBChassis)
    {
        var mudSkinMark = pBChassis.CarPhysics.GetComponentInChildren<MudSkinMark>();
        if (mudSkinMark != null)
        {
            mudSkinMark.transform.SetParent(pBChassis.transform.parent);
            Destroy(mudSkinMark.gameObject, 3f);
        }

        var listVFX = pBChassis.GetComponentsInChildren<MudEffect>();
        for (int i = 0; i < listVFX.Length; i++)
        {
            listVFX[i].transform.GetComponentInChildren<ParticleSystem>().loop = false;
            listVFX[i].transform.SetParent(pBChassis.transform.parent);
            Destroy(listVFX[i].gameObject, 1);
        }
    }

    private void OnTriggerExitCallback(Collider collider)
    {
        var part = collider.GetComponent<PBPart>();
        if (part == null || part.RobotChassis == null)
            return;
        var chassis = part.RobotChassis;
        ExitMudGround(chassis);
    }

    private void EnterMudGround(PBChassis chassis)
    {
        if (robotEnterStateDict.Get(chassis.Robot))
            return;

        chassis.CarPhysics.CarTopSpeedMultiplier *= 0.5f;
        robotEnterStateDict.Set(chassis.Robot, true);

        // Debug.Log($"Enter Mud Ground CarTopSpeedMultiplier -> {chassis.CarPhysics.CarTopSpeedMultiplier}");
        AddVFX(chassis);
    }

    private void ExitMudGround(PBChassis chassis)
    {
        if (!robotEnterStateDict.Get(chassis.Robot))
            return;

        chassis.CarPhysics.CarTopSpeedMultiplier /= 0.5f;
        robotEnterStateDict.Set(chassis.Robot, false);


        // Debug.Log($"Exit Mud Ground CarTopSpeedMultiplier -> {chassis.CarPhysics.CarTopSpeedMultiplier}");
        RemoveVFX(chassis);
    }
}
