using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SmasherNavMeshObstacleController : MonoBehaviour
{
    [SerializeField]
    private SmasherBehaviour smasherBehaviour;
    [SerializeField]
    private NavMeshObstacle[] smashDownNavMeshObstacles;
    [SerializeField]
    private NavMeshObstacle[] liftUpNavMeshObstacles;

    private int smashValue;

    private void Update()
    {
        int currentSmashValue = smasherBehaviour.GetSmashValue();
        if (currentSmashValue != smashValue)
        {
            smashValue = currentSmashValue;
            if (smashValue > 0)
            {
                SetEnableNavMeshObstacles(true);
            }
            else
            {
                SetEnableNavMeshObstacles(false);
            }
        }
    }

    private void SetEnableNavMeshObstacles(bool isSmashDown)
    {
        foreach (var smashDownNavMeshObstacle in smashDownNavMeshObstacles)
        {
            smashDownNavMeshObstacle.enabled = isSmashDown;
        }
        foreach (var liftUpNavMeshObstacle in liftUpNavMeshObstacles)
        {
            liftUpNavMeshObstacle.enabled = !isSmashDown;
        }
    }
}