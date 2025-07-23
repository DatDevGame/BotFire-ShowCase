using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;

public class StagePhysics3 : MonoBehaviour
{
    // [SerializeField]
    // private Transform center;

    // private Dictionary<CarPhysics, float> lastTimeManualForceGoUpDict = new Dictionary<CarPhysics, float>();
    // private Dictionary<CarPhysics, float> lastTimeManualForceGoDownDict = new Dictionary<CarPhysics, float>();

    // private void Awake()
    // {
    //     GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
    // }

    // private void OnDestroy()
    // {
    //     GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
    // }

    // private void OnMatchStarted(object[] parameters)
    // {
    //     if (parameters[0] is not PBPvPMatch)
    //         return;
    //     StartCoroutine(CommonCoroutine.Delay(0.5f, false, () =>
    //     {
    //         var levelController = FindObjectOfType<LevelController>(true);
    //         var robots = levelController.GetComponentsInChildren<PBRobot>(true);
    //         foreach (var robot in robots)
    //         {
    //             robot.onBotSpawned += OnBotSpawned;
    //             OnBotSpawned(robot.ChassisInstance);
    //         }
    //     }));
    // }

    // private void OnBotSpawned(PBChassis chassis)
    // {
    //     var carPhysics = chassis.CarPhysics;
    //     carPhysics.OnCalcSlopeRotation += OnCalcSlopeRotation;

    //     void OnCalcSlopeRotation(CarPhysics.SlopeRotateData eventData)
    //     {
    //         var frontPos = eventData.frontPos;
    //         var frontHit = eventData.frontHit;
    //         var rearPos = eventData.rearPos;
    //         var rearHit = eventData.rearHit;
    //         var carRb = eventData.carRb;
    //         var movingDir = eventData.movingDir;
    //         var slopeAngle = eventData.slopeAngle;

    //         var frontAngle = Vector3.Angle(frontHit.normal, Vector3.up);
    //         var rearAngle = Vector3.Angle(rearHit.normal, Vector3.up);
    //         var dir = (frontPos - rearPos).normalized;
    //         var currentTime = Time.time;
    //         if (frontAngle <= Mathf.Epsilon && rearAngle > Mathf.Epsilon)
    //         {
    //             // Go up (move forward) or down (move backward)
    //             var centerPoint = new Vector3(center.position.x, carRb.position.y, center.position.z);
    //             var playerToCenter = carRb.position - centerPoint;
    //             var dotProduct = Vector3.Dot(playerToCenter.normalized, carRb.velocity.normalized);
    //             if (carRb.velocity.magnitude >= 10f && currentTime - lastTimeManualForceGoUpDict.Get(carPhysics) > 0.5f && dotProduct > 0.5f && movingDir < 0f)
    //             {
    //                 lastTimeManualForceGoUpDict.Set(carPhysics, currentTime);
    //                 carRb.AddForce(dir * carRb.velocity.magnitude * movingDir, ForceMode.VelocityChange);
    //                 Debug.Log($"Go up force: {dotProduct} - {carRb.velocity.magnitude} - {currentTime - lastTimeManualForceGoUpDict.Get(carPhysics)} - {slopeAngle}");
    //             }
    //             else
    //             {
    //                 Debug.Log($"Go up force failed: {dotProduct} - {carRb.velocity.magnitude} - {currentTime - lastTimeManualForceGoUpDict.Get(carPhysics)}");
    //             }
    //         }
    //         else if (frontAngle > Mathf.Epsilon && rearAngle <= Mathf.Epsilon)
    //         {
    //             // Go down (move forward) or up (move backward)
    //             var centerPoint = new Vector3(center.position.x, carRb.position.y, center.position.z);
    //             var playerToCenter = centerPoint - carRb.position;
    //             var dotProduct = Vector3.Dot(playerToCenter.normalized, carRb.velocity.normalized);
    //             if (carRb.velocity.magnitude >= 10f && currentTime - lastTimeManualForceGoDownDict.Get(carPhysics) > 0.5f && dotProduct > 0.5f && movingDir > 0f)
    //             {
    //                 lastTimeManualForceGoDownDict.Set(carPhysics, currentTime);
    //                 carRb.AddForce(dir * carRb.velocity.magnitude * movingDir, ForceMode.VelocityChange);
    //                 Debug.Log($"Go down force: {dotProduct} - {carRb.velocity.magnitude} - {currentTime - lastTimeManualForceGoDownDict.Get(carPhysics)} - {slopeAngle}");
    //             }
    //             else
    //             {
    //                 Debug.Log($"Go down force failed: {dotProduct} - {carRb.velocity.magnitude} - {currentTime - lastTimeManualForceGoDownDict.Get(carPhysics)}");
    //             }
    //         }
    //     }
    // }
}