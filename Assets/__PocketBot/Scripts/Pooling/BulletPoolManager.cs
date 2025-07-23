using System.Collections.Generic;
using UnityEngine;

public class BulletPoolManager : Singleton<BulletPoolManager>
{
    private Dictionary<Component, Queue<Component>> poolDict = new();

    public T Get<T>(T prefab) where T : Component
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab is null when calling PoolManager.Get.");
            return null;
        }

        if (!poolDict.TryGetValue(prefab, out var pool))
        {
            pool = new Queue<Component>();
            poolDict[prefab] = pool;
        }

        T instance;
        if (pool.Count > 0)
        {
            instance = pool.Dequeue() as T;
            instance.gameObject.SetActive(true);
        }
        else
        {
            instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }

        return instance;
    }

    public void Release<T>(T prefab, T instance) where T : Component
    {
        if (prefab == null || instance == null)
        {
            Debug.LogWarning("Prefab or instance is null when calling PoolManager.Release.");
            return;
        }

        instance.transform.SetParent(transform);
        instance.gameObject.SetActive(false);

        if (!poolDict.TryGetValue(prefab, out var pool))
        {
            pool = new Queue<Component>();
            poolDict[prefab] = pool;
        }

        pool.Enqueue(instance);
    }
}
