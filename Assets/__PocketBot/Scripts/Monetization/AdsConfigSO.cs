using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AdsConfigSO", menuName = "PocketBots/AdsConfigSO")]
public class AdsConfigSO : ScriptableObject
{
    public bool is_enable;
    public int IS_Firstshow_Trophy;
    public List<string> IS_Locations;

    public bool IsEnable => is_enable;
    public int FirstShowTrophyThreshold => IS_Firstshow_Trophy;

    public bool IsLocationEnabled(AdsLocation adsLocation)
    {
        return IS_Locations.Contains(adsLocation.ToString());
    }
}