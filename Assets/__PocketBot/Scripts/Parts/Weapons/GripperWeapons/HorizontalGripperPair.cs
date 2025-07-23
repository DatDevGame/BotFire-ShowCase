using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalGripperPair : MonoBehaviour
{
    List<HorizontalGripperBehaviour> horizontalGripperBehaviours;

    protected virtual void Awake()
    {
        horizontalGripperBehaviours = new List<HorizontalGripperBehaviour>(GetComponentsInChildren<HorizontalGripperBehaviour>());
    }

    protected virtual IEnumerator Start()
    {
        yield return new WaitForSeconds(horizontalGripperBehaviours[0].RandomDelayTime());
        foreach (var item in horizontalGripperBehaviours)
        {
            item.isRunnable = true;
            item.OnGripped.AddListener(OnGripped);
            item.OnReleased.AddListener(OnReleased);
            item.OnSetLastTime += OnSetLastTime;
        }
    }

    private void OnDestroy()
    {
        foreach (var item in horizontalGripperBehaviours)
        {
            if (item != null)
            {
                item.OnGripped.RemoveListener(OnGripped);
                item.OnReleased.RemoveListener(OnReleased);
                item.OnSetLastTime -= OnSetLastTime;
            }
        }
    }

    protected virtual void OnGripped(PBRobot targetRobot, GripperBehaviour horizontalGripperBehaviour)
    {
        foreach (var item in horizontalGripperBehaviours)
        {
            if (item != (HorizontalGripperBehaviour)horizontalGripperBehaviour)
            {
                item.HandleWhenAnotherGripped();
            }
        }
    }

    protected virtual void OnReleased(GripperBehaviour horizontalGripperBehaviour)
    {
        foreach (var item in horizontalGripperBehaviours)
        {
            if (item != (HorizontalGripperBehaviour)horizontalGripperBehaviour)
            {
                item.HandleWhenAnotherRelease();
            }
        }
    }

    protected virtual void OnSetLastTime(float lastTime, HorizontalGripperBehaviour horizontalGripperBehaviour)
    {
        foreach (var item in horizontalGripperBehaviours)
        {
            if (item != horizontalGripperBehaviour)
            {
                item.LastTime = lastTime;
            }
        }
    }
}
