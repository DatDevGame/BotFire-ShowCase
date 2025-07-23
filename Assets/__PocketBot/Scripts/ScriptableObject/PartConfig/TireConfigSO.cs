using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TireConfigSO", menuName = "PocketBots/PartConfig/TireConfigSO")]
public class TireConfigSO : ScriptableObject
{
    public float SuspensionRestDist = 0.6f;
    public float TireMass = 1f;
    public float TireHeight = 0.5f;
}
