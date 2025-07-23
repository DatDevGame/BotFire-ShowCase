using System.Collections;
using System.Collections.Generic;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;

public class ContinuousGunBase : GunBase
{
    [Header("Base Gun Configs")]

    public float secondsPerHit = 0.5f;
    public float playDuration = 2f;
}
