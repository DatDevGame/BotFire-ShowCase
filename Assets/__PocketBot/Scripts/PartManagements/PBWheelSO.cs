using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "WheelPartSO", menuName = "PocketBots/PartManagement/WheelPartSO")]
public class PBWheelSO : PBPartSO
{
    [SerializeField, BoxGroup("Wheel Fields")] float speed;
    [SerializeField, BoxGroup("Wheel Fields")] TireConfigSO tireConfigSO;

    public float Speed => speed;
    public TireConfigSO TireConfigSO => tireConfigSO;
}
