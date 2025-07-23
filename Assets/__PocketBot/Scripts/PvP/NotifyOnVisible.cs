using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;
public class NotifyOnVisible : MonoBehaviour
{
    void OnBecameVisible()
    {
        GameEventHandler.Invoke(RobotStatusEventCode.OnRobotVisibleOnScreen, true, this.gameObject.GetInstanceID());
    }

    void OnBecameInvisible()
    {
        GameEventHandler.Invoke(RobotStatusEventCode.OnRobotVisibleOnScreen, false, this.gameObject.GetInstanceID());
    }

}
