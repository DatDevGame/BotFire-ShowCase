using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PyroGunNavMeshObstacleController : MonoBehaviour
{
    [SerializeField]
    private GunBehaviour gunBehaviour;
    [SerializeField]
    private NavMeshObstacle navMeshObstacle;

    private void Start()
    {
        gunBehaviour.onShootingStateChanged += EnableNavMeshObstacle;
    }

    private void OnDestroy()
    {
        gunBehaviour.onShootingStateChanged -= EnableNavMeshObstacle;
    }

    private void EnableNavMeshObstacle(bool isEnabled)
    {
        navMeshObstacle.enabled = isEnabled;
    }
}