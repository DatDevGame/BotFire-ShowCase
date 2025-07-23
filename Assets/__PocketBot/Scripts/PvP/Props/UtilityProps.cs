using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityProps : MonoBehaviour, INavigationPoint
{
    protected LinkedList<PBRobot> enterNavPointRobots = new LinkedList<PBRobot>();

    protected virtual void AddEnterPointRobot(PBRobot robot)
    {
        if (!enterNavPointRobots.Contains(robot))
            enterNavPointRobots.AddLast(robot);
    }

    protected virtual void RemoveEnterPointRobot(PBRobot robot)
    {
        enterNavPointRobots.Remove(robot);
    }

    public virtual bool IsAvailable()
    {
        return true;
    }

    public virtual bool IsRobotReached(PBRobot robot)
    {
        return enterNavPointRobots.Contains(robot);
    }

    public virtual PointType GetPointType()
    {
        return PointType.UtilityPoint;
    }

    public virtual Vector3 GetTargetPoint()
    {
        return transform.position;
    }
}