using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterModelRobot : PBRobot
{
    protected PBChassisSO m_PBCharacterChassisSO;
    public void SetChassisSO(PBChassisSO chassisSO) => m_PBCharacterChassisSO = chassisSO;
    public override PBRobot BuildRobot(bool isInit)
    {
        if (m_PBCharacterChassisSO == null) return null;

        if (chassisInstance != null)
        {
            Destroy(chassisInstance.gameObject);
            foreach (var part in partInstances)
            {
                foreach (var instance in part.Parts)
                {
                    Destroy(instance.gameObject);
                }
            }
            partInstances.Clear();
        }
        transform.localScale = Vector3.one * 0.3f;
        var chassisPrefab = m_PBCharacterChassisSO.GetModelPrefab<PBChassis>();
        var partSlots = m_PBCharacterChassisSO.AllPartSlots;
        chassisInstance = Instantiate(chassisPrefab, transform);
        chassisInstance.gameObject.SetLayer(RobotLayer, true);
        chassisInstance.CarPhysics.CarTopSpeedMultiplier = m_PBCharacterChassisSO.Speed;
        chassisInstance.PartSO = m_PBCharacterChassisSO;
        chassisInstance.CarPhysics.IsPreview = isPreview;
        partInstances.Add(new Part(PBPartSlot.Body, new() { chassisInstance }));

        SpawnBotParts();
        return this;

        void SpawnBotParts()
        {
            for (int i = 0; i < partSlots.Count; i++)
            {
                if (partSlots[i].PartVariableSO == null) continue;
                if (partSlots[i].PartVariableSO.value == null) continue;
                partInstances.Add(new Part(partSlots[i].PartSlotType, new()));
                BuildPart(partSlots[i].PartSlotType);
            }
        }
    }

    public void RemoveRobot()
    {
        if (chassisInstance != null)
        {
            Destroy(chassisInstance.gameObject);
            foreach (var part in partInstances)
            {
                foreach (var instance in part.Parts)
                {
                    Destroy(instance.gameObject);
                }
            }
            partInstances.Clear();
        }
    }
}
