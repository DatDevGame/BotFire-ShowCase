using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Helpers;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.AI.Navigation;
using UnityEngine;

public class CollapsibleBridge : CollapsibleObject
{
    [SerializeField]
    private ObstacleBreak[] m_BreakableObstacles = new ObstacleBreak[] { };
    [SerializeField]
    private Collider[] m_LowerGroundColliders = new Collider[] { };
    [SerializeField]
    private NavMeshSurface m_StageNavMeshSurface;
    [SerializeField]
    private NavMeshModifier m_NavMeshModifier;
    [SerializeField]
    private PulleyAnchor m_PulleyAnchor;
    [SerializeField]
    private Rigidbody m_Rigidbody;

    private bool m_IsCollapsed;

    private void Awake()
    {
        if (m_StageNavMeshSurface == null)
            m_StageNavMeshSurface = GetComponentInParent<NavMeshSurface>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (m_LowerGroundColliders.Contains(collision.collider) && !IsCollapsed())
        {
            m_Rigidbody.isKinematic = true;
            m_IsCollapsed = true;
            m_NavMeshModifier.area = 2;
            m_StageNavMeshSurface.BuildNavMesh();
            m_OnCollapsedEvent.Invoke();
        }
    }

    private ObstacleBreak[] GetIntactObstacles()
    {
        return m_BreakableObstacles.Where(item => !item.IsBroken()).ToArray();
    }

    private ObstacleBreak[] GetBrokenObstacles()
    {
        return m_BreakableObstacles.Where(item => item.IsBroken()).ToArray();
    }

    public override bool IsCollapsed()
    {
        return m_IsCollapsed;
    }

    public override Vector3 GetCollapsiblePoint()
    {
        if (m_PulleyAnchor != null)
            return m_PulleyAnchor.transform.position;
        else
        {
            ObstacleBreak[] intactObstacles = GetIntactObstacles();
            if (intactObstacles != null && intactObstacles.Length > 0)
            {
                return intactObstacles[0].transform.position;
            }
            return default;
        }
    }

    [Button]
    public void ForceCollapse()
    {
        GetIntactObstacles().ForEach(item => item.InvokeMethod("BreakObject", new object[] { null }));
    }
}