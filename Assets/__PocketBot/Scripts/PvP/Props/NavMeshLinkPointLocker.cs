using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshLinkPointLocker : MonoBehaviour
{
    [SerializeField]
    private bool m_LockStartPoint = false;
    [SerializeField]
    private bool m_LockEndPoint = false;
    [SerializeField]
    private NavMeshLink m_NavMeshLink;

    private Vector3 m_WorldStartPoint;
    private Vector3 m_WorldEndPoint;
    
    private void Awake()
    {
        Matrix4x4 l2wMatrix = Matrix4x4.TRS(m_NavMeshLink.transform.position, m_NavMeshLink.transform.rotation, Vector3.one);
        m_WorldStartPoint = l2wMatrix.MultiplyPoint3x4(m_NavMeshLink.startPoint);
        m_WorldEndPoint = l2wMatrix.MultiplyPoint3x4(m_NavMeshLink.endPoint);
    }

    private void Update()
    {
        Matrix4x4 w2lMatrix = Matrix4x4.TRS(m_NavMeshLink.transform.position, m_NavMeshLink.transform.rotation, Vector3.one).inverse;
        if (m_LockStartPoint)
        {
            m_NavMeshLink.startPoint = w2lMatrix.MultiplyPoint3x4(m_WorldStartPoint);
        }
        if (m_LockEndPoint)
        {
            m_NavMeshLink.endPoint = w2lMatrix.MultiplyPoint3x4(m_WorldEndPoint);
        }
        
    }
}