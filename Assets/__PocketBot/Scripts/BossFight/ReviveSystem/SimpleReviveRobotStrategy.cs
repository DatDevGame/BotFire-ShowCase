using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames;
using UnityEngine;

public class SimpleReviveRobotStrategy : MonoBehaviour, IReviveRobotStrategy
{
    public void ReviveRobot(ReviveData data, Action callback)
    {
        PBRobot robot = data.robot;
        robot.BuildRobot(false);
        PBChassis chassisInstance = robot.ChassisInstance;
        List<Vector3> localPositions = new List<Vector3>();
        List<Quaternion> localRotations = new List<Quaternion>();
        foreach (var part in robot.PartInstances)
        {
            if (part.PartSlotType.GetPartTypeOfPartSlot() == PBPartType.Body)
                continue;
            if (part.PartSlotType.GetPartTypeOfPartSlot() == PBPartType.Wheels)
                continue;
            foreach (var pbPart in part.Parts)
            {
                localPositions.Add(chassisInstance.CarPhysics.transform.InverseTransformPoint(pbPart.transform.position));
                localRotations.Add(Quaternion.Inverse(chassisInstance.CarPhysics.transform.rotation) * pbPart.transform.rotation);
            }
        }
        //chassisInstance.CarPhysics.enabled = false;
        chassisInstance.CarPhysics.transform.SetPositionAndRotation(data.position, data.rotation);
        var index = 0;
        foreach (var part in robot.PartInstances)
        {
            if (part.PartSlotType.GetPartTypeOfPartSlot() == PBPartType.Body)
                continue;
            if (part.PartSlotType.GetPartTypeOfPartSlot() == PBPartType.Wheels)
                continue;
            foreach (var pbPart in part.Parts)
            {
                pbPart.transform.SetPositionAndRotation(chassisInstance.CarPhysics.transform.TransformPoint(localPositions[index]), chassisInstance.CarPhysics.transform.rotation * localRotations[index]);
                index++;
            }
        }
        StartCoroutine(CommonCoroutine.Wait(null, () => callback?.Invoke()));
    }
}