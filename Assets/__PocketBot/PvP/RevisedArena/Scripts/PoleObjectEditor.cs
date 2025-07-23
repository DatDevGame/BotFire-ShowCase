using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

public class PoleObjectEditor : MonoBehaviour
{
    [SerializeField] private GameObject m_RailBarPrefab;
    [SerializeField] private GameObject m_PolePrefab;
    [SerializeField] private int count = 5;
    [SerializeField] private float spacing = 2.0f;
    [SerializeField] private Vector3 startPoint;
    [SerializeField] private Vector3 endPoint;

    private List<Transform> m_ListPole = new List<Transform>();
    public Transform StartTest { get; private set; }
    public Transform EndTest { get; private set; }

    [Button("Generate Fence")]
    private void GenerateFence()
    {
        if (m_PolePrefab == null || m_RailBarPrefab == null)
        {
            Debug.LogError("Please assign both pole and rail bar prefabs!");
            return;
        }

        ClearFence();
        Vector3 direction = (endPoint - startPoint).normalized;
        float totalDistance = Vector3.Distance(startPoint, endPoint);
        int actualCount = Mathf.Max(1, Mathf.CeilToInt(totalDistance / spacing));
        Vector3 centerOffset = transform.position - startPoint;

        for (int i = 0; i < actualCount; i++)
        {
            Vector3 position = startPoint + direction * (i * spacing) + centerOffset;
            if (Vector3.Distance(position, startPoint + centerOffset) > totalDistance) break;

            GameObject obj = Instantiate(m_PolePrefab, position, Quaternion.identity, transform);
            obj.name = m_PolePrefab.name + "_" + i;
            m_ListPole.Add(obj.transform);
        }

        if (m_ListPole.Count > 1)
        {
            StartTest = m_ListPole.First();
            EndTest = m_ListPole.Last();
            CreateRailBar(StartTest, EndTest);
        }
    }

    private void CreateRailBar(Transform startPole, Transform endPole)
    {
        Transform railBar = Instantiate(m_RailBarPrefab, transform).transform;
        railBar.position = new Vector3(startPole.position.x, startPole.position.y + 1.5f, startPole.position.z);
        railBar.LookAt(endPole);
        railBar.eulerAngles += new Vector3(0, 90, 0);
        railBar.localScale = new Vector3(Vector3.Distance(startPole.position, endPole.position) / 10, railBar.localScale.y, railBar.localScale.z);
    }

    [Button("Clear Fence")]
    private void ClearFence()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        m_ListPole.Clear();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + startPoint, 0.2f);
        Gizmos.DrawSphere(transform.position + endPoint, 0.2f);
        Gizmos.DrawLine(transform.position + startPoint, transform.position + endPoint);
    }
}
