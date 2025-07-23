using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class GearUpgradeVFXReaction : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem m_UpgradeParticlePrefab;
    [SerializeField]
    private ItemSOVariable m_CurrentChassisInUse;

    private PBChassisSO currentChassisInUse => m_CurrentChassisInUse.value.Cast<PBChassisSO>();

    private GearTabButton _caheGearTabButton;
    private PBRobot _cachePbRobot;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUpgraded, OnPartUpgraded);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUpgraded, OnPartUpgraded);
    }

    private bool IsWheel(PBPartSlot partSlot)
    {
        return partSlot.GetPartTypeOfPartSlot() == PBPartType.Wheels;
    }
    private void OnPartUpgraded(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;

        if (_caheGearTabButton == null)
            _caheGearTabButton = FindObjectOfType<GearTabSelection>().SelectedGearTabButton;

        if (_cachePbRobot == null)
            _cachePbRobot = FindObjectOfType<PBRobot>();

        GearTabButton selectedTabSlot = _caheGearTabButton;
        LGDebug.Log($"PartType: {selectedTabSlot.PartType} - PartSlot: {selectedTabSlot.PartSlot}");
        var partSO = parameters[1] as PBPartSO;
        var pbRobot = _cachePbRobot;
        var isChassis = partSO.PartType != PBPartType.Body;
        var isSlotExist = currentChassisInUse.AllPartSlots.Exists(slot => slot.PartSlotType == selectedTabSlot.PartSlot);
        var particleSpawnPoint = pbRobot.ChassisInstance.transform;
        if (isChassis && isSlotExist)
        {
            var slot = currentChassisInUse.AllPartSlots.Find(slot => slot.PartSlotType == selectedTabSlot.PartSlot);
            var partContainer = pbRobot.ChassisInstance.PartContainers.Find(item => item.PartSlotType == slot.PartSlotType);
            particleSpawnPoint = IsWheel(slot.PartSlotType) ? partContainer.Containers.Find(item => item.name.Contains("FL") || item.name.Contains("BL")) : partContainer.Containers[0];
        }
        var upgradeParticle = Instantiate(m_UpgradeParticlePrefab);
        upgradeParticle.Play();
        upgradeParticle.transform.position = particleSpawnPoint.position;
        Destroy(upgradeParticle.gameObject, 1);
    }
}