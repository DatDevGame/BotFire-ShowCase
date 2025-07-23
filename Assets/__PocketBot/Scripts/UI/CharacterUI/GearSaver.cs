using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using Unity.Burst.Intrinsics;
using LatteGames;
using LatteGames.UnpackAnimation;
using System;

public class GearSaver : Singleton<GearSaver>
{
    [Header("Item Saving Reference")]
    public PPrefItemSOVariable specialSO;
    public PPrefItemSOVariable chassisSO;
    public PPrefItemSOVariable wheels_1SO;
    public PPrefItemSOVariable wheels_2SO;
    public PPrefItemSOVariable wheels_3SO;
    public PPrefItemSOVariable frontSO;
    public PPrefItemSOVariable upper_1;
    public PPrefItemSOVariable upper_2;

    public PBPartManagerSO SpecialManager => _specialManager;

    [SerializeField, BoxGroup("Data")] private PBPartManagerSO m_BodyManager;
    [SerializeField, BoxGroup("Data")] private PBPartManagerSO m_FrontManager;
    [SerializeField, BoxGroup("Data")] private PBPartManagerSO m_UpperManager;

    [SerializeField] private PBPartManagerSO _specialManager;
    [SerializeField] List<PBPartManagerSO> partManager;

    void OnEnable()
    {
        InitData();
    }

    public void InitData()
    {
        ClearData();
        SetData(chassisSO);
        SetData(wheels_1SO);
        SetData(wheels_2SO);
        SetData(wheels_3SO);
        SetData(frontSO);
        SetData(upper_1);
        SetData(upper_2);
    }

    void SetData(PPrefItemSOVariable pp)
    {
        var so = (PBPartSO)pp.value;
        if (so != null)
            so.IsEquipped = true;
    }

    void ClearData()
    {
        foreach (var item in partManager)
        {
            foreach (var _item in item.value)
            {
                var i = (PBPartSO)_item;
                i.IsEquipped = false;
            }
        }
    }

    public PBPartSO GetCurrentPartSO(PBPartSlot pBPartSlot)
    {
        switch (pBPartSlot)
        {
            case PBPartSlot.Body:
                return (PBPartSO)chassisSO.value;
            case PBPartSlot.Wheels_1:
                return (PBPartSO)wheels_1SO.value;
            case PBPartSlot.Wheels_2:
                return (PBPartSO)wheels_2SO.value;
            case PBPartSlot.Wheels_3:
                return (PBPartSO)wheels_3SO.value;
            case PBPartSlot.Front_1:
                return (PBPartSO)frontSO.value;
            case PBPartSlot.Upper_1:
                return (PBPartSO)upper_1.value;
            case PBPartSlot.Upper_2:
                return (PBPartSO)upper_2.value;
            default:
                return (PBPartSO)specialSO.value;
        }
    }

    public void EquipGear(PBPartSO partSO, PBPartSlot pBPartSlot)
    {
        #region Firebase Event
        GameEventHandler.Invoke(LogFirebaseEventCode.ItemEquip, partSO, pBPartSlot);
        #endregion

        PBPartSO oldPart;
        List<PBPartSO> equippedWheels = new();

        switch (pBPartSlot)
        {
            case PBPartSlot.Body:
                if (partSO == null)
                {
                    break;
                }

                oldPart = (PBPartSO)chassisSO.value;
                if (oldPart != null)
                {
                    oldPart.IsEquipped = false;
                    ClearData();
                }

                var _tempChassis = (PBChassisSO)chassisSO;
                for (int i = 0; i < _tempChassis.AllPartSlots.Count; i++)
                {
                    PBPartSO item = (PBPartSO)_tempChassis.AllPartSlots[i].PartVariableSO.value;
                    if (item != null)
                        item.IsEquipped = false;
                }
                chassisSO.value = partSO;
                var _chassis = (PBChassisSO)chassisSO;

                frontSO.Clear();
                upper_1.Clear();
                upper_2.Clear();

                // for (int i = 0; i < _chassis.AllPartSlots.Count; i++)
                // {
                //     BotPartSlot item = _chassis.AllPartSlots[i];
                //     if (item.PartType == PBPartType.Wheels)
                //     {
                //         if (!_chassis.IsSpecial)
                //         {
                //             PBPartSO highestWheel = GetHighestAvailableWheel();
                //             ((PPrefItemSOVariable)item.PartVariableSO).value = highestWheel;
                //             if (highestWheel != null)
                //             {
                //                 highestWheel.IsEquipped = true;
                //                 equippedWheels.Add(highestWheel);
                //             }
                //         }
                //     }
                // }
                AttachWheelToChassis();
                void AttachWheelToChassis()
                {
                    var wheelSlots = _chassis.AllPartSlots.FindAll(x => x.PartType == PBPartType.Wheels);
                    for (int i = 0; i < wheelSlots.Count; i++)
                    {
                        var wheel = wheelSlots[i];
                        wheel.PartVariableSO.value = i < _chassis.AttachedWheels.Count ? _chassis.AttachedWheels[i] : _chassis.AttachedWheels.Last();
                    }
                }
                break;
            case PBPartSlot.Wheels_1:
                if (partSO == null)
                {
                    wheels_1SO.Clear();
                    break;
                }

                oldPart = (PBPartSO)wheels_1SO.value;
                if (oldPart != null)
                    oldPart.IsEquipped = false;

                wheels_1SO.value = partSO;
                break;
            case PBPartSlot.Wheels_2:
                if (partSO == null)
                {
                    wheels_2SO.Clear();
                    break;
                }

                oldPart = (PBPartSO)wheels_2SO.value;
                if (oldPart != null)
                    oldPart.IsEquipped = false;

                wheels_2SO.value = partSO;
                break;
            case PBPartSlot.Wheels_3:
                if (partSO == null)
                {
                    wheels_3SO.Clear();
                    break;
                }

                oldPart = (PBPartSO)wheels_3SO.value;
                if (oldPart != null)
                    oldPart.IsEquipped = false;

                wheels_3SO.value = partSO;
                break;
            case PBPartSlot.Front_1:
                if (partSO == null)
                {
                    frontSO.Clear();
                    break;
                }

                oldPart = (PBPartSO)frontSO.value;
                if (oldPart != null)
                    oldPart.IsEquipped = false;

                frontSO.value = partSO;
                break;
            case PBPartSlot.Upper_1:
                if (partSO == null)
                {
                    upper_1.Clear();
                    break;
                }

                oldPart = (PBPartSO)upper_1.value;
                if (oldPart != null)
                    oldPart.IsEquipped = false;

                upper_1.value = partSO;
                break;
            case PBPartSlot.Upper_2:
                if (partSO == null)
                {
                    upper_2.Clear();
                    break;
                }

                oldPart = (PBPartSO)upper_2.value;
                if (oldPart != null)
                    oldPart.IsEquipped = false;

                upper_2.value = partSO;
                break;
            default:
                break;
        }
        PBPartSO GetHighestAvailableWheel()
        {
            var wheelManagerSO = partManager.Find(manager => manager.PartType == PBPartType.Wheels);

            if (chassisSO.value.Cast<PBChassisSO>().IsSpecial)
            {
                PBChassisSO pbSpecialEquiped = specialSO.value.Cast<PBChassisSO>();
                wheelManagerSO = pbSpecialEquiped.AllPartSlots.Find(v => v.PartSlotType == PBPartSlot.Wheels_1).PartVariableSO.value.Cast<PBPartSO>().ManagerSO;
            }

            List<PBPartSO> availableWheels = new List<PBPartSO>(wheelManagerSO.Parts);
            foreach (var wheelSO in equippedWheels)
            {
                availableWheels.Remove(wheelSO);
            }
            PBPartSO highestWheelSO = availableWheels.Find(wheelSO => wheelSO.IsOwned() == true);
            foreach (var wheelSO in availableWheels)
            {
                if (wheelSO.IsOwned() == false) continue;
                if (highestWheelSO.CalCurrentHp() < wheelSO.CalCurrentHp()) highestWheelSO = wheelSO;
            }
            return highestWheelSO;
        }

        #region Firebase Event
        GameEventHandler.Invoke(LogFirebaseEventCode.ItemsAvailableUpgradeChange);
        #endregion
    }
}
