using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HighGround : MonoBehaviour
{
    [SerializeField]
    private CollapsibleObject[] m_CollapsibleBridges;

    public bool IsReachable()
    {
        return m_CollapsibleBridges.All(bridge => bridge.IsCollapsed());
    }

    public Vector3 GetAlternativeTargetPoint()
    {
        for (int i = 0; i < m_CollapsibleBridges.Length; i++)
        {
            if (!m_CollapsibleBridges[i].IsCollapsed())
            {
                return m_CollapsibleBridges[i].GetCollapsiblePoint();
            }
        }
        return default;
    }
}