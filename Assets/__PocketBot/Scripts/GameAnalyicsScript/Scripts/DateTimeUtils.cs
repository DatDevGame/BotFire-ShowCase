using UnityEngine;
using System;

public class DateTimeUtils : MonoBehaviour
{
    static DateTime appInstalledTime;
    public static int DaysSinceInstalled
    {
        get
        {
            var timeSpan = DateTime.Now - appInstalledTime;
            return timeSpan.Days;
        }
    }

    [SerializeField] PPrefDatetimeVariable pprefAppInstalledTime;

    private void Awake()
    {
        if (!pprefAppInstalledTime.hasKey)
        {
            pprefAppInstalledTime.value = DateTime.Now;
        }
        appInstalledTime = pprefAppInstalledTime.value;
    }
}
