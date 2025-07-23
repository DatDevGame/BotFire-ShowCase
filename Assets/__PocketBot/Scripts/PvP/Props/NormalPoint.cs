using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalPoint : MonoBehaviour, INavigationPoint
{
    public NormalPoint[] AdjacentPoints { get; set; } = new NormalPoint[0];

    private void OnDrawGizmos()
    {
        if (!enabled)
            return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    public PointType GetPointType()
    {
        return PointType.NormalPoint;
    }

    public Vector3 GetTargetPoint()
    {
        return transform.position;
    }

    public bool IsAvailable()
    {
        return true;
    }

    public bool IsRobotReached(PBRobot robot)
    {
        var pointA = GetTargetPoint();
        var pointB = robot.GetTargetPoint();
        return Vector3.Distance(pointA, pointB) <= 1f;
    }
}