using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using Sirenix.Serialization;

// [Serializable]
// public class NormalPointGenerator
// {
//     [SerializeField]
//     private float maxDistance = 0.5f;
//     [SerializeField]
//     private GridLayoutGroup3D layoutGroup3D;
//     [SerializeField]
//     private Transform pointsContainer;

//     public Transform PointContainer => pointsContainer;
//     public NormalPoint RootPoint => Points[0];
//     public List<NormalPoint> Points { get; set; } = new List<NormalPoint>();

//     private void CreatePoint(Vector3 position, NormalPoint[] adjacentPoints = null)
//     {
//         var normalPoint = new GameObject("NormalPoint").AddComponent<NormalPoint>();
//         normalPoint.transform.parent = pointsContainer;
//         normalPoint.transform.position = position;
//         if (adjacentPoints != null)
//             normalPoint.AdjacentPoints = adjacentPoints;
//         Points.Add(normalPoint);
//     }

//     public void GenerateNormalPoints()
//     {
//         var cells = layoutGroup3D.GetLayoutDataAsList();
//         var groundLayerIndex = LayerMask.NameToLayer("Ground");
//         foreach (var transformData in cells)
//         {
//             if (Physics.Raycast(transformData.position, Vector3.down, out RaycastHit hitInfo, float.MaxValue, Physics.DefaultRaycastLayers, queryTriggerInteraction: QueryTriggerInteraction.Ignore))
//             {
//                 if (hitInfo.collider.gameObject.layer != groundLayerIndex)
//                     continue;
//                 if (hitInfo.collider.GetComponentInParent<IDynamicGround>() != null)
//                     continue;
//                 if (NavMesh.SamplePosition(hitInfo.point, out var hit, maxDistance, NavMesh.AllAreas))
//                 {
//                     var posA = new Vector3(hit.position.x, 0f, hit.position.z);
//                     var posB = new Vector3(hitInfo.point.x, 0f, hitInfo.point.z);
//                     var sqrLength = (posA - posB).sqrMagnitude;
//                     if (Mathf.Approximately(sqrLength, 0f))
//                         CreatePoint(hit.position);
//                 }
//             }
//         }
//     }
// }
public class PBFightingStage : Singleton<PBFightingStage>
{
    // [SerializeField]
    // private Sprite thumbnail;
    // [SerializeField]
    // private Transform[] contestantSpawnPoints;
    // [SerializeField]
    // private NormalPointGenerator normalPointGenerator;

    // private Dictionary<PointType, List<INavigationPoint>> navigationPointDictionary;
    // private ObjectPool<NormalPoint> normalPointPool;

    public bool isFightingReady {get; private set;} = false;

    private IEnumerator Start()
    {
        // Cache original parent & local transform data of all robots in scene
        isFightingReady = false;
        yield return new WaitUntil(WaitForFightingReady);
    }

    private bool WaitForFightingReady()
    {
        var robots = PBRobot.allFightingRobots;
        if (robots.Count <= 0)
            return false;
        for (int i = 0, length = robots.Count; i < length; ++i)
            if (robots[i].RobotStatsSO == null)
                return false;
        isFightingReady = true;
        return true;
    }

    // private void CreatePool()
    // {
    //     normalPointPool = new ObjectPool<NormalPoint>(InstantiatePoint, actionOnDestroy: DestroyPoint, actionOnGet: GetPoint, actionOnRelease: ReleasePoint);

    //     NormalPoint InstantiatePoint()
    //     {
    //         var normalPoint = new GameObject("NormalPoint").AddComponent<NormalPoint>();
    //         normalPoint.transform.parent = normalPointGenerator.PointContainer;
    //         return normalPoint;
    //     }

    //     void DestroyPoint(NormalPoint point)
    //     {
    //         Destroy(point);
    //     }

    //     void GetPoint(NormalPoint point)
    //     {
    //         point.gameObject.name = "NormalPoint_Enabled";
    //         point.gameObject.SetActive(true);
    //     }

    //     void ReleasePoint(NormalPoint point)
    //     {
    //         point.gameObject.name = "NormalPoint_Disabled";
    //         point.gameObject.SetActive(false);
    //     }
    // }

    public ObjectPool<NormalPoint> GetNormalPointPool()
    {
        // if (normalPointPool == null)
        // {
        //     CreatePool();
        // }
        // return normalPointPool;
        return null;
    }

    public Sprite GetThumbnail()
    {
        // return thumbnail;
        return null;
    }

    public Transform[] GetContestantSpawnPoints()
    {
        // return contestantSpawnPoints;
        return null;
    }

    public Dictionary<PointType, List<INavigationPoint>> GetAllNavigationPoints()
    {
        // if (navigationPointDictionary == null)
        // {
        //     navigationPointDictionary = new Dictionary<PointType, List<INavigationPoint>>();
        //     var rootGameObjects = gameObject.scene.GetRootGameObjects();
        //     for (int i = 0; i < rootGameObjects.Length; i++)
        //     {
        //         var rootGameObject = rootGameObjects[i];
        //         var navigationPoints = rootGameObject.GetComponentsInChildren<INavigationPoint>();
        //         foreach (var navigationPoint in navigationPoints)
        //         {
        //             if (!navigationPointDictionary.ContainsKey(navigationPoint.GetPointType()))
        //                 navigationPointDictionary.Add(navigationPoint.GetPointType(), new List<INavigationPoint>());
        //             navigationPointDictionary[navigationPoint.GetPointType()].Add(navigationPoint);
        //         }
        //     }
        // }
        // return navigationPointDictionary;
        return null;
    }
}