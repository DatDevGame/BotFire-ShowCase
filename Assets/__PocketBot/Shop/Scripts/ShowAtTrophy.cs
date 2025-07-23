using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowAtTrophy : MonoBehaviour
{
    [SerializeField] GameObject go;
    [SerializeField] HighestAchievedPPrefFloatTracker highestAchievedMedal;

    private void Awake()
    {
        if (go != null)
            go.SetActive(highestAchievedMedal.value >= FullSkinPopup.SHOW_TROPHY_THRESHOLD);
    }
}
