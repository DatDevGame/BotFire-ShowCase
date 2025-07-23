using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullSkinShopPackBG : MonoBehaviour
{
    [SerializeField] GameObject go;
    [SerializeField] HighestAchievedPPrefFloatTracker highestAchievedMedal;

    private void Awake()
    {
        go?.SetActive(highestAchievedMedal.value >= FullSkinPopup.SHOW_TROPHY_THRESHOLD);
    }
}
