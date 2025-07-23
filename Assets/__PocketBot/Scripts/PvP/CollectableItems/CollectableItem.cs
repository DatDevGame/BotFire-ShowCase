using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Template;
using UnityEngine;

public abstract class CollectableItem : MonoBehaviour, INavigationPoint
{
    public event Action<PBRobot, CollectableItem> onBeingCollected = delegate { };

    [SerializeField]
    protected Collider triggerCollider;

    protected LinkedList<PBRobot> enterNavPointRobots = new LinkedList<PBRobot>();

    protected virtual void OnTriggerEnter(Collider collider)
    {
        if (!enabled)
            return;
        var pbPart = GetPartFromCollider(collider);
        if (pbPart == null || pbPart.RobotChassis == null)
        {
            return;
        }
        AddEnterPointRobot(pbPart.RobotChassis.Robot);
        if (IsAbleToCollect())
            CollectItem(pbPart.RobotChassis.Robot);
    }

    protected virtual void OnTriggerExit(Collider collider)
    {
        if (!enabled)
            return;
        var pbPart = GetPartFromCollider(collider);
        if (pbPart == null || pbPart.RobotChassis == null)
        {
            return;
        }
        RemoveEnterPointRobot(pbPart.RobotChassis.Robot);
    }

    protected virtual PBPart GetPartFromCollider(Collider collider)
    {
        if (collider == null || collider.isTrigger)
            return null;
        if (collider.attachedRigidbody != null && collider.attachedRigidbody.TryGetComponent(out PBPart pbPart))
            return pbPart;
        return collider.GetComponent<PBPart>();
    }

    protected virtual void AddEnterPointRobot(PBRobot robot)
    {
        if (!enterNavPointRobots.Contains(robot))
            enterNavPointRobots.AddLast(robot);
    }

    protected virtual void RemoveEnterPointRobot(PBRobot robot)
    {
        enterNavPointRobots.Remove(robot);
    }

    protected virtual void NotifyEventBoosterBeingCollected(PBRobot robot)
    {
        SoundManager.Instance.PlaySFX_3D_Pitch(GeneralSFX.UICardFlip, transform.position, true);
        // Notify event
        onBeingCollected.Invoke(robot, this);
        // Tracking mission data
        if (robot.PersonalInfo.isLocal)
        {
            GameEventHandler.Invoke(MissionEventCode.OnPlayerPickedUpCollectable);
        }
    }

    public virtual bool IsAvailable()
    {
        return IsAbleToCollect();
    }

    public virtual bool IsRobotReached(PBRobot robot)
    {
        return enterNavPointRobots.Contains(robot);
    }

    public virtual PointType GetPointType()
    {
        return PointType.CollectablePoint;
    }

    public virtual Vector3 GetTargetPoint()
    {
        return transform.position;
    }

    public abstract bool IsAbleToCollect();
    public abstract void CollectItem(PBRobot robot);
}