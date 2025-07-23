using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LatteGames.PvP;
using LatteGames.Utils;
using UnityEngine;

public class PvPRobotSpawner : MonoBehaviour
{
    [SerializeField] PBRobot playerRobotPrefab;
    [SerializeField] PBRobot botRobotPrefab;
    [SerializeField] bool isRandomPoint = true;

    int spawnIndex = 1;
    List<PBRobot> robots = new List<PBRobot>();
    List<Transform> spawnPoints;

    List<Transform> SpawnPoints
    {
        get
        {
            if (spawnPoints == null)
            {
                Map.ShuffleSpawnPoint();
                spawnPoints = Map.PlayerSpawnPoints;
                if (isRandomPoint)
                {
                    //spawnPoints.Shuffle();
                }
            }
            return spawnPoints;
        }
    }

    void Start()
    {
        robots.Clear();
        SpawnPlayerRobot();
        SpawnAllBotRobots();
        CalcHighestOverallScore();
    }

    void SpawnPlayerRobot()
    {
        var spawnPoint = SpawnPoints[0];
        var instance = Instantiate(playerRobotPrefab, spawnPoint.position, spawnPoint.rotation, transform);
        instance.BuildRobot(true);
        instance.TeamId = 1;
        robots.Add(instance);
    }

    void SpawnAllBotRobots()
    {
        var matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
        if (matchManager == null)
            return;
        PBPvPMatch pvpMatch = matchManager.GetCurrentMatchOfPlayer() as PBPvPMatch;
        int botAmount = pvpMatch.Contestants.Count;
        for (int i = 0; i < botAmount; i++)
        {
            if (pvpMatch.Contestants[i].value.personalInfo.isLocal)
                continue;
            var botInfoVariable = pvpMatch.Contestants[i];
            var spawnPoint = SpawnPoints[i + 1];
            var instance = Instantiate(botRobotPrefab, spawnPoint.position, spawnPoint.rotation, transform);
            bool isPlayerTeam = i + 1 < botAmount / 2;
            instance.gameObject.layer = isPlayerTeam ? LayerMask.NameToLayer("PlayerPart") : LayerMask.NameToLayer("EnemyPart");
            instance.TeamId = isPlayerTeam ? 1 : 0;

            instance.SetInfo(botInfoVariable);
            instance.BuildRobot(true);
            robots.Add(instance);
        }
    }

    void CalcHighestOverallScore()
    {
        foreach (var robot in robots)
        {
            robot.CalcHighestOverallScore(robots);
        }
    }

    public void SpawnBotRobotFTUE(PlayerInfoVariable infoVariable, ItemSOVariable chassisSOVariable, int index)
    {
        var spawnPoint = SpawnPoints[index];
        var instance = Instantiate(botRobotPrefab, spawnPoint.position, spawnPoint.rotation, transform);
        instance.gameObject.layer = 31 - spawnIndex++;
        instance.SetInfo(infoVariable, chassisSOVariable);
        instance.BuildRobot(true);
        robots.Add(instance);
        CalcHighestOverallScore();
    }
}