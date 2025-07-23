using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PBObstaclePartSO", menuName = "PocketBots/PvP/PBObstaclePartSO")]
public class PBObstaclePartSO : PBPartSO
{
    [SerializeField] int obstacleType;
    public int ObstacleType => obstacleType;
}
