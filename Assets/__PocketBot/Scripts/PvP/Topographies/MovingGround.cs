using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using HyrphusQ.Helpers;
using HyrphusQ.Events;
using LatteGames;

[DefaultExecutionOrder(-5)]
public class MovingGround : MonoBehaviour, IDynamicGround
{
    private const float MaxRayDistance = 3f;
    [SerializeField, BoxGroup("Tweaks")]
    private float poleScaleOffset;
    [SerializeField, BoxGroup("Tweaks")]
    private bool isFlipDirection;
    [SerializeField, BoxGroup("No Tweaks")]
    private float stopDuration = 0.5f;
    [SerializeField, BoxGroup("No Tweaks")]
    private RangeFloatValue randomSpeedRange = new RangeFloatValue(1f, 2f) { minValue = 1f, maxValue = 2f };
    [SerializeField, BoxGroup("No Tweaks")]
    private Transform steelPole;
    [SerializeField, BoxGroup("No Tweaks")]
    private Transform movingRoot;
    [SerializeField, BoxGroup("No Tweaks")]
    private Rigidbody rb;
    [SerializeField, BoxGroup("No Tweaks")]
    private OnTriggerCallback triggerCallback;
    [SerializeField, BoxGroup("No Tweaks")]
    private Collider[] colliders;
    [SerializeField, BoxGroup("No Tweaks")] 
    private int m_StartDelay = 5;

    private bool m_IsDelayComplete = false;
    private float timeAbleToMove;
    private float speed;
    private float time = 0.5f;
    private Vector3 movingOriginPoint;
    private Vector3 movingDestinationPoint;
    private HashSet<int> colliderIdHashSet;

    public Transform MovingRoot => movingRoot;
    public bool IsAbleToMove
    {
        get => Time.fixedTime - timeAbleToMove >= 0;
    }
    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    [ShowInInspector, BoxGroup("Tweaks")]
    private float MovingRange
    {
        get
        {
            if (steelPole == null)
                return 1f;
            return steelPole.localScale.z;
        }
        set
        {
            if (steelPole == null)
                return;
            var localScale = steelPole.localScale;
            localScale.z = value;
            steelPole.localScale = localScale;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
    [ShowInInspector, BoxGroup("Tweaks"), PropertyRange(0, 1f)]
    private float OriginPos
    {
        get
        {
            var (movingOriginPoint, movingDestinationPoint) = CalcMovingPoint();
            return InverseLerp(rb.position, movingOriginPoint, movingDestinationPoint);
        }
        set
        {
            var (movingOriginPoint, movingDestinationPoint) = CalcMovingPoint();
            rb.transform.position = Vector3.Lerp(movingOriginPoint, movingDestinationPoint, value);
            movingRoot.position = rb.transform.position;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
    [ShowInInspector, BoxGroup("Debug")]
    private float MovingDistance
    {
        get
        {
            var (movingOriginPoint, movingDestinationPoint) = CalcMovingPoint();
            return Vector3.Distance(movingOriginPoint, movingDestinationPoint);
        }
    }

    private void Start()
    {
        StartCoroutine(CommonCoroutine.Delay(m_StartDelay, false, () => { m_IsDelayComplete = true;}));

        (movingOriginPoint, movingDestinationPoint) = CalcMovingPoint();
        steelPole.transform.localScale = new Vector3(steelPole.transform.localScale.x, steelPole.transform.localScale.y, steelPole.transform.localScale.z + poleScaleOffset);
        time = InverseLerp(rb.position, movingOriginPoint, movingDestinationPoint);
        triggerCallback.isFilterByTag = false;
        triggerCallback.onTriggerStay += OnTriggerStayCallback;
        triggerCallback.onTriggerExit += OnTriggerExitCallback;
        speed = Random.Range(randomSpeedRange.minValue, randomSpeedRange.maxValue);
        colliderIdHashSet = new HashSet<int>(colliders.Select(collider => collider.GetInstanceID()));
        var triggerCollider = triggerCallback.GetComponentInChildren<Collider>();
        foreach (var collider in colliders)
        {
            Physics.IgnoreCollision(triggerCollider, collider);
        }
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private void FixedUpdate()
    {
        if (!IsAbleToMove || !m_IsDelayComplete)
            return;
        var t = Mathf.PingPong(time, 1f);
        var currentTime = time + Time.fixedDeltaTime * Speed / Vector3.Distance(movingOriginPoint, movingDestinationPoint);
        if (Mathf.Ceil(time) == Mathf.Floor(currentTime))
            timeAbleToMove = Time.fixedTime + stopDuration;
        time = currentTime;
        rb.MovePosition(Vector3.Lerp(movingOriginPoint, movingDestinationPoint, t));
        movingRoot.position = rb.position;
    }

    private void OnDrawGizmosSelected()
    {
        if (steelPole != null)
        {
            Vector3 originPoint, destinationPoint;
            if (movingOriginPoint != default && movingDestinationPoint != default)
                (originPoint, destinationPoint) = (movingOriginPoint, movingDestinationPoint);
            else
                (originPoint, destinationPoint) = CalcMovingPoint();
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(originPoint, 0.5f);
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(destinationPoint, 0.5f);
        }
    }

    private void OnModelSpawned(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0 || parameters[0] is not PBRobot robot)
            return;
        var triggerCollider = triggerCallback.GetComponentInChildren<Collider>();
        var colliders = robot.GetComponentsInChildren<Collider>().Where(collider => collider.attachedRigidbody != robot.ChassisInstance.CarPhysics.CarRb);
        foreach (var collider in colliders)
        {
            Physics.IgnoreCollision(triggerCollider, collider);
        }
    }

    private float InverseLerp(Vector3 value, Vector3 min, Vector3 max)
    {
        var ba = value - min;
        var ca = max - min;
        return Mathf.Clamp01(Mathf.Sqrt(ba.sqrMagnitude / ca.sqrMagnitude));
    }

    private void OnTriggerStayCallback(Collider other)
    {
        var part = other.GetComponent<PBPart>();
        if (part == null || part.RobotChassis == null || part.RobotChassis.IsGripped)
            return;
        var chassis = part.RobotChassis;
        if (chassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID != default)
        {
            if (colliderIdHashSet.Contains(chassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID))
            {
                EnterMovingGround(chassis);
            }
            else
            {
                ExitMovingGround(chassis);
            }
        }
    }

    private void OnTriggerExitCallback(Collider other)
    {
        var part = other.GetComponent<PBPart>();
        if (part == null || part.RobotChassis == null || part.RobotChassis.IsGripped)
            return;
        var chassis = part.RobotChassis;
        ExitMovingGround(chassis);
    }

    private void EnterMovingGround(PBChassis chassis)
    {
        var robot = chassis.Robot;
        if (robot.ChassisInstanceTransform.parent != movingRoot)
        {
            robot.ChassisInstanceTransform.SetParent(movingRoot);
        }
    }

    private void ExitMovingGround(PBChassis chassis)
    {
        var robot = chassis.Robot;
        if (robot.ChassisInstanceTransform.parent == movingRoot)
        {
            robot.ChassisInstanceTransform.SetParent(robot.ChassisInstance.transform);
            robot.ChassisInstanceTransform.transform.localScale = Vector3.one;
        }
    }

    private (Vector3, Vector3) CalcMovingPoint()
    {
        var poleMesh = steelPole.GetComponent<MeshFilter>().sharedMesh;
        var poleLocalBounds = poleMesh.bounds;
        var extends = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, steelPole.localScale).MultiplyPoint(poleLocalBounds.extents);
        var movingOriginPoint = transform.TransformPoint(Vector3.left * extends.z);
        var movingDestinationPoint = transform.TransformPoint(Vector3.right * extends.z);
        return isFlipDirection ? (movingOriginPoint, movingDestinationPoint) : (movingDestinationPoint, movingOriginPoint);
    }
}