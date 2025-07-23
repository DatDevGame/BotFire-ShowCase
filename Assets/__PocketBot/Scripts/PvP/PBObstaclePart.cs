using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBObstaclePart : PBPart
{
    [SerializeField] PBPartSO obstaclePartSO;
    [SerializeField] Rigidbody selfRigidbody;

    public override PBPartSO PartSO { get => obstaclePartSO; set => obstaclePartSO = value; }
    public override Rigidbody RobotBaseBody => selfRigidbody;
}