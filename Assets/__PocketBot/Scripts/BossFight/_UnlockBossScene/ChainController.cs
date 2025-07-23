using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ChainController : MonoBehaviour
{
    [SerializeField] List<SpriteRenderer> chainNodeTypePrefabs;
    [SerializeField] int resolution;
    [SerializeField] float spacing;
    [SerializeField] int orderSorting;
    [SerializeField] Transform startHandlePoint, midHandlePoint, endHandlePoint;
    [SerializeField] List<SpriteRenderer> chainNodes = new();

    [Button]
    public void UpdateChain()
    {
        PlaceChainNodes();
    }

    private void PlaceChainNodes()
    {
        // Calculate total length of the curve
        float totalLength = CalculateCurveLength();

        // Position each chain node at fixed intervals along the curve
        float distance = 0f;
        float curveParameter = 0f;
        for (int i = 0; i < chainNodes.Count; i++)
        {
            // Find the parameter t on the curve that corresponds to the current distance
            curveParameter = FindCurveParameterAtDistance(distance / totalLength);
            Vector3 position = GetInterpolatedPosition(curveParameter);
            chainNodes[i].transform.position = position;

            if (i > 0)
            {
                // Adjust rotation to face the next node
                Vector3 direction = chainNodes[i].transform.position - chainNodes[i - 1].transform.position;
                if (direction.magnitude > 0)
                {
                    chainNodes[i - 1].transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Cross(direction, transform.forward));
                }
            }

            distance += spacing;
        }

        // Ensure the last node is correctly oriented
        if (chainNodes.Count > 1)
        {
            Vector3 lastDirection = chainNodes[chainNodes.Count - 1].transform.position - chainNodes[chainNodes.Count - 2].transform.position;
            if (lastDirection.magnitude > 0)
            {
                chainNodes[chainNodes.Count - 1].transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Cross(lastDirection, transform.forward));
            }
        }
    }

    private float CalculateCurveLength(int sampleCount = 100)
    {
        float length = 0f;
        Vector3 previousPoint = startHandlePoint.position;

        for (int i = 1; i <= sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            Vector3 currentPoint = GetInterpolatedPosition(t);
            length += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }

        return length;
    }

    private Vector3 GetPositionAtDistance(float t)
    {
        return GetInterpolatedPosition(t);
    }

    private Vector3 GetInterpolatedPosition(float t)
    {
        // Quadratic Bezier curve formula: (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
        Vector3 p0 = startHandlePoint.position;
        Vector3 p1 = midHandlePoint.position;
        Vector3 p2 = endHandlePoint.position;

        Vector3 position = Mathf.Pow(1 - t, 2) * p0 +
                           2 * (1 - t) * t * p1 +
                           Mathf.Pow(t, 2) * p2;

        return position;
    }

    private float FindCurveParameterAtDistance(float targetDistance)
    {
        const int sampleCount = 1000; // Higher sample count for better accuracy
        float totalLength = CalculateCurveLength(sampleCount);
        float length = 0f;
        Vector3 previousPoint = startHandlePoint.position;

        for (int i = 1; i <= sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            Vector3 currentPoint = GetInterpolatedPosition(t);
            length += Vector3.Distance(previousPoint, currentPoint);

            if (length >= targetDistance * totalLength)
            {
                return t;
            }

            previousPoint = currentPoint;
        }

        return 1f; // Return the end of the curve if not found
    }
#if UNITY_EDITOR
    [Button]
    void GenerateChainNodes()
    {
        foreach (var node in chainNodes)
        {
            if (node != null)
            {
                DestroyImmediate(node.gameObject);
            }
        }
        chainNodes.Clear();
        int chainNodeTypePrefabIndex = 0;
        for (var i = 0; i < resolution; i++)
        {
            var chainNode = Instantiate(chainNodeTypePrefabs[chainNodeTypePrefabIndex]);
            chainNode.transform.SetParent(transform);
            chainNode.gameObject.layer = gameObject.layer;
            chainNode.sortingOrder = orderSorting + i;
            chainNodes.Add(chainNode);
            chainNodeTypePrefabIndex++;
            chainNodeTypePrefabIndex = chainNodeTypePrefabIndex % chainNodeTypePrefabs.Count;
        }
        UpdateChain();
    }

    [Button]
    private void AddRigidbodiesAndJoints()
    {
        foreach (var chainNode in chainNodes)
        {
            if (chainNode.gameObject.TryGetComponent(out HingeJoint hingeJoint))
            {
                DestroyImmediate(hingeJoint);
            }
            if (chainNode.gameObject.TryGetComponent(out Rigidbody rigidbody))
            {
                DestroyImmediate(rigidbody);
            }
            if (chainNode.gameObject.TryGetComponent(out HingeJoint2D hingeJoint2D))
            {
                DestroyImmediate(hingeJoint2D);
            }
            if (chainNode.gameObject.TryGetComponent(out Rigidbody2D rigidbody2D))
            {
                DestroyImmediate(rigidbody2D);
            }
        }
        for (int i = 0; i < chainNodes.Count; i++)
        {
            Rigidbody rb = chainNodes[i].gameObject.AddComponent<Rigidbody>();
            rb.mass = 1f;

            if (i > 0)
            {
                HingeJoint joint = chainNodes[i].gameObject.AddComponent<HingeJoint>();
                joint.connectedBody = chainNodes[i - 1].GetComponent<Rigidbody>();
                joint.anchor = Vector3.zero;
                joint.axis = Vector3.forward;

                // Adjust joint settings
                joint.useLimits = true;
                JointLimits limits = joint.limits;
                limits.min = -45f;
                limits.max = 45f;
                joint.limits = limits;

                joint.useSpring = true;
                JointSpring spring = joint.spring;
                spring.spring = 100f;  // Adjust the spring force as needed
                spring.damper = 5f;    // Adjust the damping as needed
                joint.spring = spring;
            }
        }
    }
#endif
}
