using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleListSO", menuName = "PocketBots/PvP/ObstacleListSO")]
public class ObstacleListSO : ListVariable<PBObstaclePartSO>
{
    public PBObstaclePartSO GetRandomObstacle(int type)
    {
        return value.GetRandom(obstacle => obstacle.ObstacleType == type);
    }
}
