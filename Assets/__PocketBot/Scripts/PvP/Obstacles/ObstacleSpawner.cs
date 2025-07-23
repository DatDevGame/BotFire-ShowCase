using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> obstacles;
    [SerializeField] private Vector2 spawnTimeRange;
    [SerializeField] private float delay = 5;

    private int currentIndex = 0;
    private bool spawning = true;

    private void Start()
    {
        foreach (var obstacle in obstacles)
        {
            obstacle.SetActive(false);
        }
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(delay);

        while (spawning)
        {
            yield return new WaitForSeconds(Random.Range(spawnTimeRange.x, spawnTimeRange.y));
            SpawnObstacle();
        }
    }

    private void SpawnObstacle()
    {
        if (obstacles.Count == 0) return;

        GameObject obstacleToSpawn = obstacles[currentIndex];
        var obstacle = Instantiate(obstacleToSpawn, obstacleToSpawn.transform.position, obstacleToSpawn.transform.rotation);
        obstacle.SetActive(true);

        currentIndex = (currentIndex + 1) % obstacles.Count;
    }

    public void StopSpawning()
    {
        spawning = false;
    }
}
