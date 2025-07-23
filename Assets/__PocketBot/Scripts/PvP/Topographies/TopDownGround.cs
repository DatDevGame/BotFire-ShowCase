using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using HyrphusQ.Helpers;
using UnityEngine.UIElements;
using HyrphusQ.Events;

[DefaultExecutionOrder(-5)]
public class TopDownGround : MonoBehaviour, IDynamicGround
{
    private const float MaxRayDistance = 3f;

    [SerializeField, BoxGroup("Tweaks")]
    private bool m_RunOnce = false;
    [SerializeField, BoxGroup("Tweaks")]
    private bool isFlipDirection;
    [SerializeField, BoxGroup("No Tweaks")]
    private float delayStart = 2f;
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

    private float timeAbleToMove;
    private float speed;
    private float time = 0.5f;
    private Vector3 movingOriginPoint;
    private Vector3 movingDestinationPoint;
    private HashSet<int> colliderIdHashSet;
    private bool m_HasRunOnce = false;
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
        (movingOriginPoint, movingDestinationPoint) = CalcMovingPoint();
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
        StartCoroutine(DelayStart());
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
    }

    private IEnumerator DelayStart()
    {
        while (delayStart > 0)
        {
            delayStart -= Time.deltaTime;
            yield return null;
        }
    }

    private void FixedUpdate()
    {
        if (delayStart > 0) return;

        if (!IsAbleToMove || m_HasRunOnce)
            return;

        var t = Mathf.PingPong(time, 1f);
        var currentTime = time + Time.fixedDeltaTime * Speed / Vector3.Distance(movingOriginPoint, movingDestinationPoint);

        if (Mathf.Ceil(time) == Mathf.Floor(currentTime))
        {
            timeAbleToMove = Time.fixedTime + stopDuration;

            if (m_RunOnce && t >= 0.99f)
            {
                m_HasRunOnce = true;
                return;
            }
        }

        time = currentTime;
        rb.MovePosition(Vector3.Lerp(movingOriginPoint, movingDestinationPoint, t));
        movingRoot.position = rb.position;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, OnModelSpawned);
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
            Gizmos.color = Color.green;
            Gizmos.DrawCube(originPoint, new Vector3(10, 0.5f, 10));
            Gizmos.color = Color.red;
            Gizmos.DrawCube(destinationPoint, new Vector3(10, 0.5f, 10));

            float distance = Vector3.Distance(originPoint, destinationPoint);
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(transform.position, new Vector3(1, distance, 1));
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
        if (part == null || part.RobotChassis == null)
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
        if (part == null || part.RobotChassis == null)
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
        var movingOriginPoint = transform.TransformPoint(Vector3.up * extends.z);
        var movingDestinationPoint = transform.TransformPoint(Vector3.down * extends.z);
        return isFlipDirection ? (movingOriginPoint, movingDestinationPoint) : (movingDestinationPoint, movingOriginPoint);
    }

#if UNITY_EDITOR

    public BoxCollider m_BoxCollider;
    public SkinnedMeshRenderer m_SkinnedMeshRenderer;

    [Button, ContextMenu("Update Box Collider")]
    private void UpdateBoxCollider()
    {
        float A = m_SkinnedMeshRenderer.bounds.extents.y;
        float B = m_SkinnedMeshRenderer.GetBlendShapeWeight(0);
        float newValue = A * (B / 100f);

        float sizeY = newValue <= 1 ? 1.8F : newValue * 2.2F;
        float centerY = -(sizeY / 2);
        m_BoxCollider.size = new Vector3(15 * rb.transform.localScale.x, sizeY, 15 * rb.transform.localScale.z);
        m_BoxCollider.center = new Vector3(0, centerY, 0);
    }
#endif
}