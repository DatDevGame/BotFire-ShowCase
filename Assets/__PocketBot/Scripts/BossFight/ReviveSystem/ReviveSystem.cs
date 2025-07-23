using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ReviveData
{
    public ReviveData()
    {

    }
    public ReviveData(PBRobot robot, Vector3 position, Quaternion rotation)
    {
        this.robot = robot;
        this.position = position;
        this.rotation = rotation;
    }

    public PBRobot robot;
    public Vector3 position;
    public Quaternion rotation;
}
public interface IReviveRobotStrategy
{
    void ReviveRobot(ReviveData reviveData, Action callback);
}
public class ReviveSystem : MonoBehaviour
{
    public event Action<bool> onReviveDecisionMade;

    [SerializeField]
    private ReviveDataSO m_ReviveDataSO;
    [SerializeField]
    private ReviveUI m_ReviveUI;

    private PBRobot m_PlayerRobot, m_EnemyRobot;
    private Collider[] m_OverlapCheckBoxColliders = new Collider[1];
    private GameObject m_DebugBoxesContainer;

    public bool isAbleToRevive => m_ReviveDataSO.isAbleToRevive;
    public ReviveDataSO dataSO => m_ReviveDataSO;

    private PBRobot playerRobot
    {
        get
        {
            if (m_PlayerRobot == null)
                m_PlayerRobot = PBRobot.allFightingRobots.FirstOrDefault(robot => robot.PersonalInfo.isLocal);
            return m_PlayerRobot;
        }
    }
    private PBRobot enemyRobot
    {
        get
        {
            if (m_EnemyRobot == null)
                m_EnemyRobot = PBRobot.allFightingRobots.FirstOrDefault(robot => !robot.PersonalInfo.isLocal);
            return m_EnemyRobot;
        }
    }
    private GameObject debugBoxesContainer
    {
        get
        {
            if (m_DebugBoxesContainer == null)
                m_DebugBoxesContainer = new GameObject("DebugBoxesContainer");
            return m_DebugBoxesContainer;
        }
    }

    private void Awake()
    {
        ObjectFindCache<ReviveSystem>.Add(this);
        m_ReviveDataSO.RefillReviveTimes();
    }

    private void OnDestroy()
    {
        ObjectFindCache<ReviveSystem>.Remove(this);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (debugBoxesContainer == null || debugBoxesContainer.transform.childCount <= 0)
            return;
        foreach (Transform debugBox in debugBoxesContainer.transform)
        {
            Handles.color = Color.black;
            Handles.Label(debugBox.position, debugBox.name[^2].ToString());
        }
    }
#endif

    private GameObject DrawDebugBox(Vector3 point, Vector3 halfExtends, Quaternion rotation, Color color, string name = "Cube")
    {
        if (debugBoxesContainer.transform.childCount >= 9)
            DestroyDebugBoxes();
        var numOfReachablePoints = NavMeshHelper.CalcNumOfReachablePoints(point, rotation, 30f);
        var debugBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugBox.name = $"{name} ({(color == Color.red ? "0" : numOfReachablePoints)})";
        debugBox.transform.SetPositionAndRotation(point, rotation);
        debugBox.transform.localScale = halfExtends * 2f;
        debugBox.transform.parent = debugBoxesContainer.transform;
        debugBox.GetComponent<Renderer>().material.color = color == Color.red ? Color.red : Color.Lerp(Color.red, color, numOfReachablePoints / 12f);
        DestroyImmediate(debugBox.GetComponent<Collider>());
        return debugBox;

        void DestroyDebugBoxes()
        {
            var childCount = debugBoxesContainer.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                DestroyImmediate(debugBoxesContainer.transform.GetChild(childCount - i - 1).gameObject);
            }
        }
    }

    private GameObject DrawDebugRay(Vector3 point, Vector3 direction, float distance, Quaternion rotation, Transform parent, Color color, string name = "Cube")
    {
        var debugRay = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugRay.name = name;
        debugRay.transform.SetPositionAndRotation(Vector3.Lerp(point, point + direction * distance, 0.5f), rotation);
        debugRay.transform.localScale = new Vector3(0.05f, distance, 0.05f);
        debugRay.transform.parent = parent;
        debugRay.GetComponent<Renderer>().material.color = color;
        DestroyImmediate(debugRay.GetComponent<Collider>());
        return debugRay;
    }

    private bool IsValidPoint(Vector3 point, Quaternion rotation, int index, bool isVisualize = false)
    {
        GameObject debugBox = null;
        var carPhysics = playerRobot.ChassisInstance.CarPhysics;
        var checkOverlayLayerMasks = Physics.DefaultRaycastLayers ^ (1 << playerRobot.RobotLayer);
        var checkOverlapBoxExtends = carPhysics.LocalBounds.extents;
        // Checks overlap any obstacles
        if (Physics.OverlapBoxNonAlloc(point, checkOverlapBoxExtends, m_OverlapCheckBoxColliders, rotation, checkOverlayLayerMasks, QueryTriggerInteraction.Ignore) > 0)
        {
            if (isVisualize)
                DrawDebugBox(point, checkOverlapBoxExtends, rotation, Color.red, $"CheckOverlapBox_{m_OverlapCheckBoxColliders[0]}_{index}");
            return false;
        }
        if (isVisualize)
            debugBox = DrawDebugBox(point, checkOverlapBoxExtends, rotation, Color.green, $"CheckOverlapBox_{index}");
        Vector3 bottomLeftCorner = rotation * new Vector3(-checkOverlapBoxExtends.x, -checkOverlapBoxExtends.y, -checkOverlapBoxExtends.z);
        Vector3 bottomRightCorner = rotation * new Vector3(checkOverlapBoxExtends.x, -checkOverlapBoxExtends.y, -checkOverlapBoxExtends.z);
        Vector3 topLeftCorner = rotation * new Vector3(-checkOverlapBoxExtends.x, -checkOverlapBoxExtends.y, checkOverlapBoxExtends.z);
        Vector3 topRightCorner = rotation * new Vector3(checkOverlapBoxExtends.x, -checkOverlapBoxExtends.y, checkOverlapBoxExtends.z);
        Vector3[] corners = new Vector3[4] { bottomLeftCorner + point, bottomRightCorner + point, topLeftCorner + point, topRightCorner + point };
        for (int i = 0; i < corners.Length; i++)
        {
            if (!Physics.Raycast(corners[i], Vector3.down, out RaycastHit hitInfo, float.MaxValue, carPhysics.RaycastMask))
            {
                if (isVisualize)
                {
                    debugBox.GetComponent<Renderer>().material.color = Color.red;
                    var sb = new StringBuilder(debugBox.name);
                    sb[^2] = '0';
                    debugBox.name = sb.ToString();
                    DrawDebugRay(corners[i], Vector3.down, 20f, rotation, debugBox.transform, Color.red, $"Ray_{i}");
                }
                return false;
            }
            if (isVisualize)
                DrawDebugRay(corners[i], Vector3.down, hitInfo.distance, rotation, debugBox.transform, Color.green, $"Ray_{i}");
        }
        return true;
    }

    private List<Vector3> GenerateSurroundingPoints(Vector3 centerPoint, Quaternion rotation, int segmentCount, float maxDistance)
    {
        var surroundingPoints = new List<Vector3>();
        for (int i = 0; i < segmentCount; i++)
        {
            var angles = 360f / segmentCount * i;
            var direction = rotation * Quaternion.Euler(0f, angles, 0f) * Vector3.forward;
            var point = centerPoint + direction.normalized * maxDistance;
            surroundingPoints.Add(UpdatePoint(point));
        }
        surroundingPoints.Insert(0, UpdatePoint(centerPoint));
        return surroundingPoints;

        Vector3 UpdatePoint(Vector3 point)
        {
            if (Physics.Raycast(point + Vector3.up * 10f, Vector3.down, out RaycastHit hitInfo, float.MaxValue, playerRobot.ChassisInstance.CarPhysics.RaycastMask, QueryTriggerInteraction.Ignore))
                point.y = hitInfo.point.y + 0.75f;
            return point;
        }
    }

    private bool TryFindValidPointBySurroundingPoints(Vector3 centerPoint, Quaternion rotation, out Vector3 validPoint)
    {
        validPoint = default;
        var isValidPointFound = false;
        var surroundingPoints = GenerateSurroundingPoints(centerPoint, rotation, 8, 10f);
        for (int i = 0; i < surroundingPoints.Count; i++)
        {
            var point = surroundingPoints[i];
            if (IsValidPoint(point, rotation, i) && !isValidPointFound)
            {
                isValidPointFound = true;
                validPoint = new Vector3(point.x, point.y > playerRobot.transform.position.y ? point.y : playerRobot.transform.position.y, point.z);
            }
        }
        return isValidPointFound;
    }

    public (Vector3, Quaternion) FindValidPointToRevive(PBRobot playerRobot, PBRobot bossRobot)
    {
        Vector3 position = Vector3.Scale(playerRobot.ChassisInstanceTransform.position, Vector3.forward + Vector3.right) + Vector3.up * bossRobot.ChassisInstanceTransform.position.y;
        Quaternion rotation = Quaternion.LookRotation((bossRobot.ChassisInstanceTransform.position - position).normalized);
        // Find a valid point in surrounding points of both player & boss
        Vector3[] centerPoints = new Vector3[2] { position, bossRobot.ChassisInstanceTransform.position };
        foreach (var centerPoint in centerPoints)
        {
            if (TryFindValidPointBySurroundingPoints(centerPoint, rotation, out Vector3 validPoint))
            {
                return (validPoint, Quaternion.LookRotation((bossRobot.ChassisInstanceTransform.position - validPoint).normalized));
            }
        }
        return (playerRobot.transform.position, playerRobot.transform.rotation);
    }

    public void ShowReviveUI(Action<bool> callback)
    {
        var playerController = FindObjectOfType<PlayerController>(true);
        var readyMatchCountdown = FindObjectOfType<ReadyMatchCountdown>();
        var cameraController = FindObjectOfType<CameraFollowTransformSetter>();
        var cinemachineBrain = ObjectFindCache<CinemachineBrain>.Get();
        var cinemachineUpdateMode = cinemachineBrain.m_UpdateMethod;
        var activeVirtualCamera = cinemachineBrain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
        var levelController = ObjectFindCache<PBLevelController>.Get();
        var cameraShake = cinemachineBrain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CameraShake>();
        var slowMotion = ObjectFindCache<PvPSlowMotion>.Get();
        if (!isAbleToRevive)
        {
            OnReviveCallback(false);
            return;
        }
        levelController.IsStopCountdownMatchTime = true;
        StartCoroutine(CommonCoroutine.Delay(AnimationDuration.MEDIUM, true, () =>
        {
            //slowMotion.StopSloMo();
            //Time.timeScale = 0f;
            //m_ReviveDataSO.SubtractReviveTimes(1);
            //m_ReviveUI.Show(OnReviveCallback);
        }));
        void OnReviveCallback(bool isRevive)
        {
            callback?.Invoke(isRevive);
            //onReviveDecisionMade?.Invoke(isRevive);
            if (isRevive)
            {
                //cameraController.IsAbleToModifyFoV = false;
                //activeVirtualCamera.m_Lens.FieldOfView *= 1.5f;
                //cameraShake.IsAbleToShakeCam = false;
                //cinemachineBrain.m_IgnoreTimeScale = true;
                //cinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
                //playerController.enabled = false;
                //levelController.Competitors.ForEach(competitor => (competitor as PBRobot).IsInvincible = true);
                ReviveLocalPlayer(() =>
                {
                    //activeVirtualCamera.m_Lens.FieldOfView /= 1.5f;
                    readyMatchCountdown.StartCountdownLevel(true, false, OnCountdownCompleted);
                });
            }
            else
            {
                Time.timeScale = 1f;
            }
        }

        void OnCountdownCompleted()
        {
            Time.timeScale = 1f;
            playerRobot.ChassisInstance.CarPhysics.enabled = true;
            //cameraController.IsAbleToModifyFoV = true;
            //cinemachineBrain.m_IgnoreTimeScale = false;
            //cinemachineBrain.m_UpdateMethod = cinemachineUpdateMode;
            //playerController.enabled = true;
            //levelController.IsStopCountdownMatchTime = false;
            //levelController.Competitors.ForEach(competitor => (competitor as PBRobot).IsInvincible = false);
            //levelController.Competitors.ForEach(competitor => (competitor as PBRobot).EnabledAllParts(false));

            StartCoroutine(CommonCoroutine.Delay(0, false, () =>
            {
                //cameraShake.IsAbleToShakeCam = true;
                //levelController.Competitors.ForEach(competitor => (competitor as PBRobot).EnabledAllParts(true));
            }));
        }
    }

    public void ReviveLocalPlayer(Action callback)
    {
        var (position, rotation) = FindValidPointToRevive(playerRobot, enemyRobot);
        playerRobot.Health = playerRobot.MaxHealth * dataSO.healthPercentageAfterRevive;
        GetComponent<IReviveRobotStrategy>().ReviveRobot(new ReviveData(playerRobot, position, rotation), callback);
    }

    public void ReviveBot(PBRobot robot, Action callback = null)
    {
        robot.Health = robot.MaxHealth * dataSO.healthPercentageAfterRevive;
        GetComponent<IReviveRobotStrategy>().ReviveRobot(
            new ReviveData(robot, robot.transform.position, robot.transform.rotation),
            callback
        );
    }
}