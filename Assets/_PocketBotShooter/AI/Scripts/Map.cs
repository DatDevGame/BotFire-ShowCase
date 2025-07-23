using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LatteGames.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.AI;

public class Map : Singleton<Map>
{
    [SerializeField]
    private List<Transform> playerSpawnPoints;
    [SerializeField]
    private List<Transform> patrolPoints = new List<Transform>();
    [SerializeField]
    private List<Vector3> coverPoints = new List<Vector3>();
    [SerializeField]
    private List<Collider> coverColliders = new List<Collider>();

    private List<INavigationPoint> powerupPoints = new List<INavigationPoint>();

    public static List<Transform> PlayerSpawnPoints => Instance.playerSpawnPoints;
    public static List<Transform> PlayerTeam1SpawnPoints => Instance.playerSpawnPoints.GetRange(0, Instance.playerSpawnPoints.Count / 2);
    public static List<Transform> PlayerTeam2SpawnPoints => Instance.playerSpawnPoints.GetRange(Instance.playerSpawnPoints.Count / 2, Instance.playerSpawnPoints.Count / 2);
    public static List<Transform> PatrolPoints => Instance.patrolPoints;
    public static List<Vector3> CoverPoints => Instance.coverPoints;
    public static List<INavigationPoint> PowerupPoints
    {
        get
        {
            if (Instance.powerupPoints.Count <= 0)
            {
                Instance.powerupPoints = Instance.GetComponentsInChildren<INavigationPoint>().ToList();
            }
            return Instance.powerupPoints;
        }
    }
    private void Start()
    {
        SetActiveSpawnPoint(false);
    }
    private static void SetActiveSpawnPoint(bool isActive)
    {
        PlayerSpawnPoints.ForEach(v => v.gameObject.SetActive(isActive));
    }
    public static void ShuffleSpawnPoint()
    {
        //Save Materials
        Material[] blueMaterial = Instance.playerSpawnPoints.First().GetComponent<MeshRenderer>().materials;
        Material[] redMaterial = Instance.playerSpawnPoints.Last().GetComponent<MeshRenderer>().materials;

        int halfCount = Instance.playerSpawnPoints.Count / 2;

        //Shuffle Team
        var team1 = Instance.playerSpawnPoints.GetRange(0, halfCount);
        var team2 = Instance.playerSpawnPoints.GetRange(halfCount, halfCount);

        //Shuffle Spawn Point In Team
        team1.Shuffle();
        team2.Shuffle();

        if (Random.value < 0.5f)
        {
            Instance.playerSpawnPoints = team2.Concat(team1).ToList();
        }
        else
        {
            Instance.playerSpawnPoints = team1.Concat(team2).ToList();
        }

        //Handle Material Spawn Point
        var theFirstTeam = Instance.playerSpawnPoints.GetRange(0, halfCount);
        var theSecondTeam = Instance.playerSpawnPoints.GetRange(halfCount, halfCount);

        List<MeshRenderer> theFirstTeamMaterial = theFirstTeam
            .Select(t => t.GetComponent<MeshRenderer>())
            .Where(r => r != null)
            .ToList();

        List<MeshRenderer> theSecondTeamMaterial = theSecondTeam
            .Select(t => t.GetComponent<MeshRenderer>())
            .Where(r => r != null)
            .ToList();

        theFirstTeamMaterial.ForEach(v => v.materials = blueMaterial);
        theSecondTeamMaterial.ForEach(v => v.materials = redMaterial);

        SetActiveSpawnPoint(true);
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 cPoint in coverPoints)
        {
            Gizmos.DrawSphere(cPoint, 0.2f);
        }
    }

    [Button]
    private void GenerateCoverPoints()
    {
        coverPoints.Clear();

        // Find all colliders in the scene
        foreach (Collider collider in coverColliders)
        {
            // Skip triggers and mesh colliders
            if (collider.isTrigger || collider is MeshCollider)
                continue;

            // Generate points around the collider bounds
            Bounds bounds = collider.bounds;
            int numPoints = 8; // Number of points to generate per collider

            for (int i = 0; i < numPoints; i++)
            {
                // Generate random point around the edge of the collider
                Vector3 randomPoint;
                float angle = (i / (float)numPoints) * 360f;
                float radius = Mathf.Max(bounds.extents.x, bounds.extents.z) * 1.5f; // Slightly outside the bounds

                randomPoint = new Vector3(
                    bounds.center.x + Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    0f,
                    bounds.center.z + Mathf.Sin(angle * Mathf.Deg2Rad) * radius
                );

                // Check if point is valid on navmesh
                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1f, NavMesh.AllAreas))
                {
                    coverPoints.Add(hit.position);
                }
            }
        }
        UnityEditor.EditorUtility.SetDirty(this);

        LGDebug.Log($"Generated {coverPoints.Count} cover points");
    }
#endif

    public static bool TryFindCoverPoint(BotController botController, out Vector3 coverPoint)
    {
        CoverPoints.Shuffle();
        foreach (Vector3 cPoint in CoverPoints)
        {
            if (!IsDangerousPoint(botController, cPoint))
            {
                coverPoint = cPoint;
                return true;
            }
        }
        coverPoint = default;
        return false;
    }

    public static bool IsDangerousPoint(BotController botController, Vector3 coverPoint)
    {
        float travelCost = botController.CalculateTravelCost(coverPoint);
        List<BotController> enemyBotControllers = botController.TeamId == 1 ? BotController.AllBotControllersTeam2 : BotController.AllBotControllersTeam1;
        foreach (BotController enemyBotController in enemyBotControllers)
        {
            if (enemyBotController.IsDead())
                continue;
            // Make sure enemy can't reach cover point faster than me
            float enemyTravelCost = enemyBotController.CalculateTravelCost(coverPoint);
            if (travelCost >= enemyTravelCost)
            {
                return true;
            }
            // Make sure enemy can't shoot me through cover point
            if (!Physics.Raycast(enemyBotController.transform.position, coverPoint - enemyBotController.transform.position, out RaycastHit hit, Vector3.Distance(coverPoint, enemyBotController.transform.position), Physics.DefaultRaycastLayers ^ Const.PBLayerMask.Ground ^ (1 << botController.gameObject.layer), QueryTriggerInteraction.Ignore))
            {
                Debug.DrawRay(enemyBotController.transform.position, coverPoint - enemyBotController.transform.position, Color.red, 0.5f);
                return true;
            }
            else
            {
                Debug.DrawRay(enemyBotController.transform.position, hit.point - enemyBotController.transform.position, Color.green, 0.5f);
            }
        }
        return false;
    }

    public static bool TryFindPowerupPoint(BotController botController, out INavigationPoint powerupPoint)
    {
        // Sort in ascending order by distance to bot
        PowerupPoints.Sort((a, b) => Vector3.Distance(botController.transform.position, a.GetTargetPoint()).CompareTo(Vector3.Distance(botController.transform.position, b.GetTargetPoint())));
        foreach (INavigationPoint pwrPoint in PowerupPoints)
        {
            if (pwrPoint.IsAvailable() && !IsDangerousPoint(botController, pwrPoint.GetTargetPoint()))
            {
                powerupPoint = pwrPoint;
                return true;
            }
        }
        powerupPoint = null;
        return false;
    }
}