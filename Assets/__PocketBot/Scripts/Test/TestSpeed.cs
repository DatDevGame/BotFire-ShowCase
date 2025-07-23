using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public struct PartSlot
{
    public PBPartSlot slotType;
    public PBPartSO partSO;
}
public class TestSpeed : MonoBehaviour
{
    [SerializeField]
    private PBRobot enemyRobot1, enemyRobot2;
    [SerializeField, BoxGroup("Chassis 1")]
    private Transform target1;
    [SerializeField, BoxGroup("Chassis 1")]
    private PBChassisSO chassisSO1;
    [SerializeField, BoxGroup("Chassis 1")]
    private List<PartSlot> chassisSlots1;
    [SerializeField, BoxGroup("Chassis 1")]
    private Transform target2;
    [SerializeField, BoxGroup("Chassis 2")]
    private PBChassisSO chassisSO2;
    [SerializeField, BoxGroup("Chassis 2")]
    private List<PartSlot> chassisSlots2;
    [SerializeField]
    private NationalFlagManagerSO nationalFlagManagerSO;

    private CarPhysics carPhysics1, carPhysics2;
    private float totalSpeed1, totalSpeed2;
    private ulong frameCount = 0;

    [ShowInInspector]
    private float MaxSpeed1 => totalSpeed1 / frameCount;
    [ShowInInspector]
    private float MaxSpeed2 => totalSpeed2 / frameCount;

    private void Awake()
    {
        CreateRobot();
    }

    private void FixedUpdate()
    {
        if (Time.fixedTime < 1f)
            return;
        if (carPhysics1 == null || carPhysics2 == null)
            return;
        totalSpeed1 += carPhysics1.CarRb.velocity.magnitude;
        totalSpeed2 += carPhysics2.CarRb.velocity.magnitude;
        frameCount++;
    }

    private void Update()
    {
        if (carPhysics1 == null || carPhysics2 == null)
            return;
        carPhysics1.AccelInput = 1f;
        carPhysics1.InputDir = target1.position - carPhysics1.CarRb.position;
        carPhysics1.RotationInput = Vector3.Dot(carPhysics1.transform.right, carPhysics1.InputDir.normalized);
        carPhysics2.AccelInput = 1f;
        carPhysics2.InputDir = target2.position - carPhysics2.CarRb.position;
        carPhysics2.RotationInput = Vector3.Dot(carPhysics2.transform.right, carPhysics2.InputDir.normalized);
    }

    private PlayerInfoVariable CreateCloneChassis(PBChassisSO chassisSO, List<PartSlot> chassisSlots, string name)
    {
        var cloneChassisSO = Instantiate(chassisSO);
        var currentChassisInUse = ScriptableObject.CreateInstance<ItemSOVariable>();
        currentChassisInUse.value = cloneChassisSO;
        var robotStatSO = ScriptableObject.CreateInstance<PBRobotStatsSO>();
        robotStatSO.chassisInUse = currentChassisInUse;
        if (chassisSlots.Count > 0)
        {
            cloneChassisSO.AllPartSlots.Clear();
            foreach (var slot in chassisSlots)
            {
                var itemSOVar = ScriptableObject.CreateInstance<ItemSOVariable>();
                itemSOVar.value = slot.partSO;
                cloneChassisSO.AllPartSlots.Add(new BotPartSlot(slot.slotType, itemSOVar));
            }
        }
        var playerInfoVar = ScriptableObject.CreateInstance<PlayerInfoVariable>();
        var playerInfo = new PBBotInfo(new AIBotInfo(name, null, nationalFlagManagerSO.GetRandomCountryInfo(), default, default), robotStatSO, null);
        playerInfoVar.value = playerInfo;
        return playerInfoVar;
    }

    [Button]
    private void CreateRobot()
    {
        var playerInfoVar1 = CreateCloneChassis(chassisSO1, chassisSlots1, "1");
        enemyRobot1.SetInfo(playerInfoVar1);
        enemyRobot1.BuildRobot(true);
        enemyRobot1.ChassisInstance.CarPhysics.CanMove = true;
        carPhysics1 = enemyRobot1.ChassisInstance.CarPhysics;
        carPhysics1.AccelInput = 1f;
        carPhysics1.RotationInput = 0f;
        carPhysics1.InputDir = Vector3.forward;
        var playerInfoVar2 = CreateCloneChassis(chassisSO2, chassisSlots2, "2");
        enemyRobot2.SetInfo(playerInfoVar2);
        enemyRobot2.BuildRobot(true);
        enemyRobot2.ChassisInstance.CarPhysics.CanMove = true;
        carPhysics2 = enemyRobot2.ChassisInstance.CarPhysics;
        carPhysics2.AccelInput = 1f;
        carPhysics2.RotationInput = 0f;
        carPhysics2.InputDir = Vector3.forward;
    }

    [SerializeField]
    private Rigidbody rb1, rb2;

    [Button]
    private void TestMassFormulaVelocityChange()
    {
        rb1.velocity = Vector3.forward * 10f;
        rb2.velocity = Vector3.forward * 10f;
    }
    [Button]
    private void TestMassFormulaAddForce()
    {
        rb1.AddForce(Vector3.forward * 10f, ForceMode.Impulse);
        rb2.AddForce(Vector3.forward * 10f, ForceMode.Impulse);
        Debug.Break();
    }
}