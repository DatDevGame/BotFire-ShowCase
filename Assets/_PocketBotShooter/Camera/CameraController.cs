using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float maxFOV = 90f;
    [SerializeField]
    private float visibilityThreshold = 0.5f; // Time threshold before changing FOV
    [SerializeField]
    private CinemachineVirtualCamera virtualCamera;
    private AimController aimController;

    private float defaultFOV = 60f;
    private float totalTimeVisible = 0f;
    private float totalTimeInvisible = 0f;
    private bool wasTargetVisible = true;

    private void Start()
    {
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private void LateUpdate()
    {
        bool isTargetVisible = true;
        var aimTargetTransform = GetAimTargetTransform();
        if (aimTargetTransform != null)
        {
            // Check if target is visible in camera frustum
            var targetScreenPoint = Camera.main.WorldToViewportPoint(aimTargetTransform.position);
            isTargetVisible = targetScreenPoint.x >= 0.1 && targetScreenPoint.x <= 0.9 &&
                                 targetScreenPoint.y >= 0 && targetScreenPoint.y <= 1 &&
                                 targetScreenPoint.z > 0;
        }

        // Reset accumulator when state changes
        if (isTargetVisible != wasTargetVisible)
        {
            if (isTargetVisible)
                totalTimeInvisible = 0f;
            else
                totalTimeVisible = 0f;

            wasTargetVisible = isTargetVisible;
        }

        // Accumulate time in current state
        if (isTargetVisible)
            totalTimeVisible += Time.deltaTime;
        else
            totalTimeInvisible += Time.deltaTime;

        // Adjust FOV based on accumulated visibility time
        if (isTargetVisible && totalTimeVisible >= visibilityThreshold)
        {
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, defaultFOV, Time.deltaTime * 0.5f);
            // LGDebug.Log($"Target is visible, reset to default FOV: {virtualCamera.m_Lens.FieldOfView}");
        }
        else if (!isTargetVisible && totalTimeInvisible >= visibilityThreshold && aimTargetTransform != null)
        {
            // Calculate distance to target
            float distanceToTarget = Vector3.Distance(transform.position, aimTargetTransform.position);
            float targetFOV = Mathf.Clamp(defaultFOV + distanceToTarget, defaultFOV, maxFOV);
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, targetFOV, Time.deltaTime * 1f);
            // LGDebug.Log($"Target is invisible, adjust FOV based on distance: {virtualCamera.m_Lens.FieldOfView}");
        }
    }

    private Transform GetAimTargetTransform()
    {
        if (aimController == null || aimController.aimTarget == null)
            return null;
        return aimController.aimTarget.transform;
    }

    [Button]
    private void LogLensData()
    {
        // if (virtualCamera == null)
        // {
        //     LGDebug.LogError("Virtual camera is not assigned!");
        //     return;
        // }

        // var lens = virtualCamera.m_Lens;
        // LGDebug.Log($"Camera Lens Data:\n" +
        //            $"Field of View: {lens.FieldOfView}\n" +
        //            $"Near Clip Plane: {lens.NearClipPlane}\n" +
        //            $"Far Clip Plane: {lens.FarClipPlane}\n" +
        //            $"Orthographic Size: {lens.OrthographicSize}\n" +
        //            $"Is Orthographic: {lens.Orthographic}\n" +
        //            $"Sensor Size: {lens.SensorSize}\n" +
        //            $"Gate Fit: {lens.GateFit}");
        Debug.Log("aimController: " + aimController);
    }

    private void OnModelSpawned(params object[] parameters)
    {
        var robot = parameters[0] as PBRobot;
        if (robot.PersonalInfo.isLocal)
        {
            if (defaultFOV == 0)
            {
                defaultFOV = virtualCamera.m_Lens.FieldOfView;
            }
            robot.OnHealthChanged -= OnHealthChanged;
            robot.OnHealthChanged += OnHealthChanged;
            aimController = robot.ChassisInstance.GetComponentInChildren<AimController>();
        }
    }

    private void OnHealthChanged(Competitor.HealthChangedEventData data)
    {
        if (data.CurrentHealth <= 0f)
        {
            aimController = null;
        }
    }
}