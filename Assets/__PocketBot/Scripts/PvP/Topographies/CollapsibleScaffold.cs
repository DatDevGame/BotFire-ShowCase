using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollapsibleScaffold : CollapsibleObject
{
    [SerializeField]
    private PulleyAnchor m_PulleyAnchor;

    public override Vector3 GetCollapsiblePoint()
    {
        return m_PulleyAnchor.transform.position;
    }

    public override bool IsCollapsed()
    {
        return m_PulleyAnchor.HasBroken;
    }
}