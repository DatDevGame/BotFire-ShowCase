using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PvPStageObstacleSpawner : MonoBehaviour
{
    [SerializeField] int noObstacleMatchesAmount = 2;
    [SerializeField] IntVariable totalPlayedMatches;
    [SerializeField] ObstacleListSO obstacleListSO;
    [SerializeField] ObstaclePatternListSO obstaclePatternListSO;
    [SerializeField] List<Transform> obstaclePositions;

    List<GameObject> obstacleInstances = new();

    private void Start()
    {
        if (totalPlayedMatches < noObstacleMatchesAmount) return;
        if (obstacleListSO == null) return;
        if (obstaclePatternListSO == null) return;
        SpawnObstacles();
    }

    void SpawnObstacles()
    {
        var pattern = obstaclePatternListSO.GetRandomPattern();
        foreach (var obstacle in pattern.ObstaclePositions)
        {
            var obstacleSO = obstacleListSO.GetRandomObstacle(obstacle.Type);
            var prefab = obstacleSO.GetModelPrefab<PBPart>();
            var container = obstaclePositions[obstacle.PositionIndex];
            var instance = Instantiate(prefab, container);
            instance.PartSO = obstacleSO;
            obstacleInstances.Add(instance.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (obstaclePositions == null) return;
        foreach (var pos in obstaclePositions)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(pos.position, Vector3.one * 0.5f);
        }
    }
}
