using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class CollapsibleObject : MonoBehaviour
{
    [SerializeField]
    protected UnityEvent m_OnCollapsedEvent;

    public virtual UnityEvent onCollapsed => m_OnCollapsedEvent;

    public abstract bool IsCollapsed();
    public abstract Vector3 GetCollapsiblePoint();
}