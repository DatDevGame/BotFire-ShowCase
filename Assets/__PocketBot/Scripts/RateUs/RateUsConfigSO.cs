using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RateUsConfigSO", menuName = "PocketBots/RateUsConfigSO")]
public class RateUsConfigSO : ScriptableObject
{
    public float RU_Firstshow_Trophy = 100;
    public float RU_Firstshow_PlayTime = 600;

    public float FirstShowTrophyThreshold => RU_Firstshow_Trophy;
    public float FirstShowTimeThreshold => RU_Firstshow_PlayTime;
}