using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-1)]
public class LinkingNavMeshSurface : MonoBehaviour
{
    [SerializeField]
    private bool isDistanceLimit;
    [SerializeField]
    private bool isAutoUpdate = true;
    [SerializeField]
    private Transform startPoint;
    [SerializeField]
    private Transform endPoint;
    [SerializeField]
    private NavMeshLink navMeshLink;

    private float maxDistance;

    private void Start()
    {
        maxDistance = Vector3.Distance(startPoint.position, endPoint.position);
        UpdateNavMeshLinkPoint();
        if (!isAutoUpdate)
            enabled = false;
    }

    private void Update()
    {
        // Update position nav mesh link start point and end point
        UpdateNavMeshLinkPoint();
    }

    public void UpdateNavMeshLinkPoint()
    {
        if (startPoint != null && endPoint != null && navMeshLink != null)
        {
            navMeshLink.startPoint = navMeshLink.transform.InverseTransformPoint(startPoint.position);
            navMeshLink.endPoint = isDistanceLimit
            ? navMeshLink.transform.InverseTransformPoint(startPoint.position + Vector3.ClampMagnitude(endPoint.position - startPoint.position, maxDistance))
            : navMeshLink.transform.InverseTransformPoint(endPoint.position);
        }
    }
}