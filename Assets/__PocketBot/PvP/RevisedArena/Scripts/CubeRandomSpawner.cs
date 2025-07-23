using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeRandomSpawner : MonoBehaviour
{
    [Header("Prefabs to spawn")]
    public List<GameObject> objectsToSpawn;

    [Header("Number of objects to spawn")]
    public int numberToSpawn = 10;

    [Header("Spawn area size (X-Z)")]
    public Vector2 spawnAreaSize = new Vector2(10f, 10f);

    [Header("Minimum distance between spawned objects")]
    public float minDistanceBetweenObjects = 1.5f;

    [Header("Vertical offset (Y) if needed")]
    public float yOffset = 0f;

    [Header("Enable random rotation?")]
    public bool randomRotation = false;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    [Button("Spawn Now")]
    public void SpawnRandomObjects()
    {
        ClearSpawnedObjects(); // Clear existing objects first to avoid overlap
        if (objectsToSpawn == null || objectsToSpawn.Count == 0)
        {
            Debug.LogWarning("Prefab list is empty!");
            return;
        }

        List<Vector3> usedPositions = new List<Vector3>();
        int attempts = 0;
        int maxAttempts = numberToSpawn * 10;

        while (spawnedObjects.Count < numberToSpawn && attempts < maxAttempts)
        {
            Vector3 spawnPos = GetRandomPosition();
            if (IsPositionValid(spawnPos, usedPositions))
            {
                GameObject prefab = objectsToSpawn[Random.Range(0, objectsToSpawn.Count)];
                Quaternion rotation = randomRotation ? Quaternion.Euler(0, Random.Range(0f, 360f), 0) : Quaternion.identity;
                GameObject spawned = Instantiate(prefab, spawnPos, rotation, transform);
                spawnedObjects.Add(spawned);
                usedPositions.Add(spawnPos);
            }
            attempts++;
        }

        if (spawnedObjects.Count < numberToSpawn)
        {
            Debug.LogWarning($"Only {spawnedObjects.Count}/{numberToSpawn} objects spawned due to limited space.");
        }
    }

    private bool IsPositionValid(Vector3 newPos, List<Vector3> existing)
    {
        foreach (Vector3 pos in existing)
        {
            if (Vector3.Distance(pos, newPos) < minDistanceBetweenObjects)
                return false;
        }
        return true;
    }

    private Vector3 GetRandomPosition()
    {
        float halfWidth = spawnAreaSize.x / 2f;
        float halfDepth = spawnAreaSize.y / 2f;

        float x = Random.Range(-halfWidth, halfWidth);
        float z = Random.Range(-halfDepth, halfDepth);
        return transform.position + new Vector3(x, yOffset, z);
    }

    [Button("Clear All")]
    public void ClearSpawnedObjects()
    {
        for (int i = 0; i < transform.childCount; i++)
            DestroyImmediate(transform.GetChild(i).gameObject);
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) DestroyImmediate(obj);
        }
        spawnedObjects.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position + new Vector3(0, yOffset, 0), new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(0, yOffset, 0), new Vector3(spawnAreaSize.x, 0.1f, spawnAreaSize.y));
    }
}
