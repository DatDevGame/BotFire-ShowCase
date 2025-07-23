using UnityEngine;
using LatteGames.Analytics;
using System.Collections.Generic;
using System;

public class PBAnalyticsManager : Singleton<PBAnalyticsManager>
{
    [SerializeField] private List<AnalyticsService> services = new List<AnalyticsService>();

    public void LogEvent<T>(Func<AnalyticsService, T> castFunc, Action<T> callbackWithRequiredService)
    {
        foreach (var service in services)
        {
            if (!service.EnableAnalyticsLogging)
                continue;
            var castedService = castFunc(service);
            if (castedService != null)
                callbackWithRequiredService?.Invoke(castedService);
        }
    }
}
