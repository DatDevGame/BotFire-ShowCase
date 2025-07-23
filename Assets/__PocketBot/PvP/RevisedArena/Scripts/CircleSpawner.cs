using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class CircleSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<GameObject> prefabList;
    public int spawnCount = 10;
    public float radius = 5f;

    [Header("Rotation Settings")]
    public bool randomYRotation = false;
    public bool lookAtCenter = true;

    private List<GameObject> spawnedPrefabs = new();

    [Button("Spawn Prefabs")]
    void SpawnPrefabsInCircle()
    {
        if (prefabList == null || prefabList.Count == 0)
        {
            Debug.LogWarning("Prefab list is empty!");
            return;
        }

        float angleStep = 360f / spawnCount;

        for (int i = 0; i < spawnCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 spawnPos = new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );

            GameObject prefab = prefabList[Random.Range(0, prefabList.Count)];
            Quaternion rotation;

            if (lookAtCenter)
            {
                Vector3 dirToCenter = (transform.position - (transform.position + spawnPos)).normalized;
                rotation = Quaternion.LookRotation(dirToCenter, Vector3.up);
            }
            else if (randomYRotation)
            {
                rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
            else
            {
                rotation = Quaternion.identity;
            }

            GameObject instance = Instantiate(prefab, transform.position + spawnPos, rotation, transform);
            instance.name = $"{instance.name}-{i}";
            instance.name = instance.name.Replace("(Clone)", "");
            spawnedPrefabs.Add(instance);
        }
    }

    [Button("Clear Spawned Prefabs")]
    void ClearSpawnedPrefabs()
    {
        foreach (GameObject go in spawnedPrefabs)
        {
            if (go != null) DestroyImmediate(go);
        }

        spawnedPrefabs.Clear();
    }

    // 🧠 Vẽ Gizmo vòng tròn trong Scene View
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = transform.position;
        const int segments = 100;
        float angleStep = 360f / segments;

        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
}
