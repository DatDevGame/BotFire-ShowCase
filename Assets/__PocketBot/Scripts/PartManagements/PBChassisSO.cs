using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using HyrphusQ.Events;
using System;

[CreateAssetMenu(fileName = "ChassisPartSO", menuName = "PocketBots/PartManagement/ChassisPartSO")]
public class PBChassisSO : PBPartSO
{
    [SerializeField, BoxGroup("Chassis Fields")] float speed = 0.5f;
    [SerializeField, BoxGroup("Chassis Fields")] float mass = 100f;
    [SerializeField, BoxGroup("Chassis Fields")] float steeringSpeed = 50f;
    [SerializeField, BoxGroup("Chassis Fields")] List<BotPartSlot> slots;
    [SerializeField, BoxGroup("Chassis Fields")] List<PBPartSO> attachedWheels;

    [SerializeField, BoxGroup("Special Check")] public bool IsSpecial;
    [SerializeField, BoxGroup("Special Check"), ShowIf("IsSpecial")] public bool IsIgnoreClaimRV;
    [SerializeField, BoxGroup("Special Check"), ShowIf("IsSpecial")] public string FoundInName;
    [SerializeField, BoxGroup("Special Check"), ShowIf("IsSpecial")] public PBChassis SpecialPreviewChassisPrefab;
    [SerializeField, BoxGroup("Special Check"), ShowIf("IsSpecial")] public PPrefItemSOVariable CurrentInUse;

    public float Mass => mass;
    public float Speed => GetSpeed();
    public float Turning => GetTurning().value;
    public float SteeringSpeed => steeringSpeed;
    public List<BotPartSlot> AllPartSlots => slots;
    public List<PBPartSO> AttachedWheels => attachedWheels;

    private string IsClaimedRVKey => this.GetDisplayName();
    private string IsClaimedRVPPrefKey => $"{IsClaimedRVKey}_IsClaimedRV";

    private void OnEnable()
    {
        if (IsSpecial)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                slot.PartVariableSO.value.Cast<PBPartSO>().PartType = slot.PartType;
            }
        }
    }

    private float GetSpeed()
    {
        List<PBWheelSO> wheelSOs = new();
        foreach (var partSlot in slots) //Get all wheels exclude duplicated
        {
            if (partSlot.PartVariableSO == null) continue;
            if (partSlot.PartVariableSO.value == null) continue;
            if (partSlot.PartVariableSO.value.Cast<PBPartSO>().PartType.Equals(PBPartType.Wheels) == false) continue;
            else wheelSOs.Add(partSlot.PartVariableSO.value.Cast<PBWheelSO>());
        }

        float speed = this.speed;

        foreach (var wheel in wheelSOs) //Sum speed of all wheels
        {
            speed += wheel.Speed;
        }

        return speed;
    }

    public PBChassisSO CloneChassisSO()
    {
        var chassisSOInstance = Instantiate(this);

        for (int i = 0; i < chassisSOInstance.AllPartSlots.Count; i++)
        {
            var slotInstance = new BotPartSlot(chassisSOInstance.AllPartSlots[i].PartSlotType, CreateInstance<ItemSOVariable>());
            slotInstance.PartVariableSO.value = chassisSOInstance.AllPartSlots[i].PartVariableSO.value;
            if (chassisSOInstance.IsSpecial)
                slotInstance.PartVariableSO.onValueChanged += OnValueChanged;
            chassisSOInstance.AllPartSlots[i] = slotInstance;
        }
        return chassisSOInstance;

        void OnValueChanged(ValueDataChanged<ItemSO> eventData)
        {
            eventData.newValue.Cast<PBPartSO>().PartType = eventData.oldValue.Cast<PBPartSO>().PartType;
        }
    }

    public int GetWheelAmount()
    {
        var result = 0;
        foreach (var slot in slots)
        {
            if (slot.PartType.Equals(PBPartType.Wheels)) result++;
        }
        return result * 2;
    }

    public int GetUpperAndFrontSlotAmount()
    {
        int upperAndUpperSlotAmount = 0;
        foreach (var slot in AllPartSlots)
        {
            if (slot.PartType.Equals(PBPartType.Front) || slot.PartType.Equals(PBPartType.Upper)) upperAndUpperSlotAmount++;
        }
        return upperAndUpperSlotAmount;
    }

    Dictionary<PBPartSlot, IPartStats> GetRobotPartStats()
    {
        Dictionary<PBPartSlot, IPartStats> stats = new()
        {
            { PBPartSlot.Body, this }
        };
        foreach (var slot in AllPartSlots)
        {
            var partVariableSO = slot.PartVariableSO;
            if (partVariableSO.value == null) continue;
            stats.Add(slot.PartSlotType, partVariableSO.value.Cast<PBPartSO>());
        }
        return stats;
    }

    public float GetTotalStatsScore()
    {
        var robotStats = new CompoundRobotStats(GetRobotPartStats());
        return robotStats.GetStatsScore().value;
    }

    public bool IsClaimedRV
    {
        get
        {
            if (IsIgnoreClaimRV)
                return true;
            else
                return PlayerPrefs.GetInt(IsClaimedRVPPrefKey, 0) == 1 ? true : false;
        }
        set
        {
            PlayerPrefs.SetInt(IsClaimedRVPPrefKey, value == true ? 1 : 0);
        }
    }
}

[System.Serializable]
public struct BotPartSlot
{
    #region Constructor
    public BotPartSlot(PBPartSlot slotType, ItemSOVariable partVariableSO)
    {
        PartSlotType = slotType;
        PartVariableSO = partVariableSO;
    }
    #endregion
    public PBPartSlot PartSlotType;
    public ItemSOVariable PartVariableSO;
    public PBPartType PartType => PartSlotType.GetPartTypeOfPartSlot();
}
