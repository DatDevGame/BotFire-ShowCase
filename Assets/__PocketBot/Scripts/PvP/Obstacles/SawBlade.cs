using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using UnityEngine.UIElements;

public class SawBlade : MonoBehaviour
{
    const float SAW_WIDTH = 1.1f;
    [SerializeField, BoxGroup("No Tweaks")]
    private Transform sawRail;
    [SerializeField, BoxGroup("No Tweaks")]
    private Rigidbody rb;
    [SerializeField, BoxGroup("No Tweaks")]
    private float stopDuration = 0.5f;

    [SerializeField, BoxGroup("No Tweaks")]
    private float speed;
    [SerializeField, BoxGroup("Tweaks")]
    private bool isFlipDirection;
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
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    [ShowInInspector, BoxGroup("Tweaks")]
    private float MovingRange
    {
        get
        {
            if (sawRail == null)
                return 1f;
            return sawRail.localScale.z;
        }
        set
        {
            if (sawRail == null)
                return;
            var localScale = sawRail.localScale;
            localScale.z = value;
            sawRail.localScale = localScale;
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

    public bool IsAbleToMove
    {
        get => Time.fixedTime - timeAbleToMove >= 0;
    }

    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    private float timeAbleToMove;
    private float time = 0.5f;
    private Vector3 movingOriginPoint;
    private Vector3 movingDestinationPoint;

    private void Start()
    {
        (movingOriginPoint, movingDestinationPoint) = CalcMovingPoint();
        time = InverseLerp(rb.position, movingOriginPoint, movingDestinationPoint);
    }
    private void FixedUpdate()
    {
        if (!IsAbleToMove)
            return;
        var t = Mathf.PingPong(time, 1f);
        var currentTime = time + Time.fixedDeltaTime * Speed / Vector3.Distance(movingOriginPoint, movingDestinationPoint);
        if (Mathf.Ceil(time) == Mathf.Floor(currentTime))
            timeAbleToMove = Time.fixedTime + stopDuration;
        time = currentTime;
        rb.transform.position = Vector3.Lerp(movingOriginPoint, movingDestinationPoint, t);
    }

    private (Vector3, Vector3) CalcMovingPoint()
    {
        var poleMesh = sawRail.GetComponent<MeshFilter>().sharedMesh;
        var poleLocalBounds = poleMesh.bounds;
        var extends = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, sawRail.localScale).MultiplyPoint(poleLocalBounds.extents);
        var movingOriginPoint = transform.TransformPoint(Vector3.left * (extends.z - SAW_WIDTH));
        var movingDestinationPoint = transform.TransformPoint(Vector3.right * (extends.z - SAW_WIDTH));
        return isFlipDirection ? (movingOriginPoint, movingDestinationPoint) : (movingDestinationPoint, movingOriginPoint);
    }

    private float InverseLerp(Vector3 value, Vector3 min, Vector3 max)
    {
        var ba = value - min;
        var ca = max - min;
        return Mathf.Clamp01(Mathf.Sqrt(ba.sqrMagnitude / ca.sqrMagnitude));
    }

    private void OnDrawGizmosSelected()
    {
        if (sawRail != null)
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
}
