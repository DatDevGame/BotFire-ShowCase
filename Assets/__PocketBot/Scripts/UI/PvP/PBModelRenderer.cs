using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PBModelRenderer : PBRobot
{
    public Camera renderCamera;

    public override PBRobot BuildRobot(bool isInit)
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
        var pbChassisSO = currentChassisInUse.value.Cast<PBChassisSO>();
        var chassisPrefab = pbChassisSO.GetModelPrefab<PBChassis>();
        var partSlots = pbChassisSO.AllPartSlots;
        chassisInstance = Instantiate(chassisPrefab, transform);
        chassisInstance.RobotChassis.transform.localPosition = new Vector3(-1, -3.5f, -9.5f);
        chassisInstance.CarPhysics.transform.localPosition = Vector3.zero;
        chassisInstance.gameObject.SetLayer(RobotLayer, true);
        chassisInstance.CarPhysics.CarTopSpeedMultiplier = pbChassisSO.Speed;
        chassisInstance.PartSO = pbChassisSO;
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
}
