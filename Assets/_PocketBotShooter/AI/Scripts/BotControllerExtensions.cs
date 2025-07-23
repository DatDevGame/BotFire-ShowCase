using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class BotControllerExtensions
{
    private static NavMeshPath path;
    private static NavMeshPath Path
    {
        get
        {
            if (path == null)
            {
                path = new NavMeshPath();
            }
            return path;
        }
    }

    public static float CalculateTravelCost(this BotController botController, Vector3 to)
    {
        bool isEnalbed = botController.Agent.enabled;
        if (!isEnalbed)
            botController.Agent.enabled = true;
        if (botController.Agent.CalculatePath(to, Path))
        {
            var corners = Path.corners;
            if (corners.Length < 2)
            {
                if (!isEnalbed)
                    botController.Agent.enabled = false;
                return float.MinValue;
            }
            var pathCost = 0.0f;
            for (int i = 1; i < corners.Length; i++)
            {
                pathCost += (corners[i] - corners[i - 1]).magnitude;
            }
            if (!isEnalbed)
                botController.Agent.enabled = false;
            return pathCost;
        }
        if (!isEnalbed)
            botController.Agent.enabled = false;
        return float.MaxValue;
    }
}