using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : Singleton<TimeManager>
{
    [HideInInspector] public float originalScaleTime;

    private void Start()
    {
        originalScaleTime = Time.timeScale;
    }
}
