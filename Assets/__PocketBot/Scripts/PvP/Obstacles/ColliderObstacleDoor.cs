using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColliderObstacleDoor : MonoBehaviour
{
    public bool IsObstacleDected
    {
        get 
        {
            if (m_IsForce)
                return m_IsObstacleDected;

            return m_IsObstacleDected;
        }
    }
    public Transform TriggerPoint => m_TriggerPoint;

    [SerializeField, BoxGroup("Config")] protected Vector3 m_OffsetCheckObstacle;
    [SerializeField, BoxGroup("Config")] protected float m_Height;
    [SerializeField, BoxGroup("Config")] protected float m_Width;
    [SerializeField, BoxGroup("Config")] protected float m_PushForce = 5f;
    [SerializeField, BoxGroup("Config")] protected bool m_IsForce;
    [SerializeField, BoxGroup("Config")] protected Transform m_TriggerPoint;
    [SerializeField, BoxGroup("Config")] protected LayerMask m_ObstacleLayer;
    [SerializeField, BoxGroup("Config")] protected List<Transform> m_ForceLists;

    private bool m_IsObstacleDected;
    private void Update()
    {
        m_IsObstacleDected = IsObstacleDetected();
        if (m_IsObstacleDected && m_IsForce)
            PushObstacles();
    }
    private bool IsObstacleDetected()
    {
        float doorWidth = 1f;
        return Physics.CheckBox(m_TriggerPoint.position, new Vector3(doorWidth / 2, m_Height, m_Width), Quaternion.identity, m_ObstacleLayer, QueryTriggerInteraction.Ignore);
    }
    private void PushObstacles()
    {
        // Get all colliders in the area
        Collider[] colliders = Physics.OverlapBox(
            m_TriggerPoint.position,
            new Vector3(m_Width / 2, m_Height, m_Width),
            Quaternion.identity,
            m_ObstacleLayer,
            QueryTriggerInteraction.Ignore
        );

        foreach (var collider in colliders)
        {
            Rigidbody rb = collider.attachedRigidbody;
            if (rb != null)
            {
                Vector3 nearestPosition = m_ForceLists
                    .OrderBy(v => Vector3.Distance(v.transform.position, rb.transform.position))
                    .First().transform.position;

                Vector3 directionNearest = (nearestPosition - rb.transform.position).normalized;

                rb.AddForce(directionNearest * m_PushForce, ForceMode.Impulse);
            }
        }
    }
    private void OnDrawGizmos()
    {
        Vector3 midPoint = m_TriggerPoint.position;
        float doorWidth = 1f;
        Vector3 halfExtents = new Vector3(doorWidth / 2, m_Height, m_Width);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(midPoint, halfExtents * 2);
    }


}
