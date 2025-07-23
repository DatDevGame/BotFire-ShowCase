using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class TestPartAttackRange : MonoBehaviour
{
    [SerializeField]
    private bool measureAttackRange;
    [SerializeField]
    private int startIndex;
    [SerializeField]
    private PBRobot robot;
    [SerializeField]
    private PBChassisSO chassisSO1;
    [SerializeField]
    private PBWheelSO wheelSO1, wheelSO2;
    [SerializeField]
    private List<PBPartManagerSO> partManagerSOs;
    [SerializeField]
    private List<PBPartSO> ignoreParts;
    [SerializeField]
    private NationalFlagManagerSO nationalFlagManagerSO;

    private int currentIndex = -1;
    private NormalPoint debugPoint;
    private List<PartSlot> defaultPartSlots;
    private List<PBPartSO> measureAttackRangeParts = new List<PBPartSO>();

    private void Awake()
    {
        currentIndex = startIndex - 1;
        if (measureAttackRange)
            GameEventHandler.AddActionEvent(CollisionEventCode.OnPartCollide, HandlePartCollide);
    }

    private void OnDestroy()
    {
        measureAttackRangeParts.ForEach(part => AssetDatabase.SaveAssetIfDirty(part));
        if (measureAttackRange)
            GameEventHandler.RemoveActionEvent(CollisionEventCode.OnPartCollide, HandlePartCollide);
    }

    private void Start()
    {
        defaultPartSlots = new List<PartSlot>()
        {
            new PartSlot() { slotType = PBPartSlot.Wheels_1, partSO = wheelSO1 },
            new PartSlot() { slotType = PBPartSlot.Wheels_2, partSO = wheelSO2 },
        };
        debugPoint = new GameObject("", typeof(NormalPoint)).GetComponent<NormalPoint>();
        partManagerSOs.ForEach(partManagerSO => measureAttackRangeParts.AddRange(partManagerSO.Parts));
        measureAttackRangeParts = measureAttackRangeParts.Except(ignoreParts).ToList();
        CreateNextPart();
    }

    private void HandlePartCollide(object[] parameters)
    {
        CollisionInfo collideData;
        if (parameters[0] is not PBPart attackerPart)
            return;
        if (parameters[1] is Collision collision)
        {
            collideData = new(collision);
        }
        else if (parameters[1] is CollisionInfo collisionInfo)
        {
            collideData = collisionInfo;
        }
        else
            return;
        if (robot.ChassisInstance == null)
            return;
        var partSO = attackerPart.PartSO;
        var partContainerTransform = robot.ChassisInstance.PartContainers.FirstOrDefault(item => item.PartSlotType == PartTypeToPartSlot(partSO)).Containers[0];
        if (partContainerTransform == null)
            return;
        var point = collideData.contactPoint - partContainerTransform.transform.position;
        var attackRange = Vector3.Dot(point, Vector3.forward);
        if (!partSO.TryGetModule(out AttackRangeModule attackRangeModule))
        {
            attackRangeModule = ItemModule.CreateModule<AttackRangeModule>(partSO);
            partSO.AddModule(attackRangeModule);
        }
        attackRangeModule.SetFieldValue("m_ItemSO", partSO);
        attackRangeModule.SetFieldValue("attackRange", attackRange);
        EditorUtility.SetDirty(partSO);
        //AssetDatabase.SaveAssetIfDirty(partSO);
        debugPoint.transform.position = collideData.contactPoint;
        // Debug.Break();
        Debug.Log($"Hit at point: {point} - AttackRange: {attackRange} - Part: {partSO}");
        CreateNextPart();
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

    private PBPartSlot PartTypeToPartSlot(PBPartSO partSO)
    {
        switch (partSO.PartType)
        {
            case PBPartType.Upper:
                return PBPartSlot.Upper_1;
            case PBPartType.Front:
                return PBPartSlot.Front_1;
            default:
                return PBPartSlot.Upper_1;
        }
    }

    private void CreateRobot(PBPartSO partSO)
    {
        var slots = new List<PartSlot>(defaultPartSlots)
        {
            new() { slotType = PartTypeToPartSlot(partSO), partSO = partSO }
        };
        var playerInfoVar1 = CreateCloneChassis(chassisSO1, slots, "1");
        robot.SetInfo(playerInfoVar1);
        robot.BuildRobot(true);
        robot.ChassisInstance.CarPhysics.CanMove = false;
        robot.AIBotController.InitializeStateMachine();
    }

    [Button]
    private void CreateNextPart()
    {
        currentIndex++;
        if (currentIndex >= measureAttackRangeParts.Count)
        {
            robot.DestroyRobot();
            return;
        }
        CreateRobot(measureAttackRangeParts[currentIndex]);
    }

    [Button]
    private void Rebuild()
    {
        CreateRobot(measureAttackRangeParts[currentIndex]);
    }
}
#endif