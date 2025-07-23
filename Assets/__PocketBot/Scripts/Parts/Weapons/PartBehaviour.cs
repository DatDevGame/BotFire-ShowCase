using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PBAxis
{
    Right, Up, Forward, Left, Down, Backward
}
public struct QueryResult
{
    public float damageMultiplier;

    public static readonly QueryResult Default = new QueryResult()
    {
        damageMultiplier = 1f,
    };
}
public struct RigidbodyState
{
    public Rigidbody rigidbody;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public void UpdateState()
    {
        UpdateState(rigidbody);
    }

    public void UpdateState(Rigidbody rigidbody)
    {
        position = rigidbody.position;
        rotation = rigidbody.rotation;
        velocity = rigidbody.velocity;
        angularVelocity = rigidbody.angularVelocity;
    }
}
public class PartBehaviour : MonoBehaviour
{
    protected readonly static RangeFloatValue DelayRandomRange = new RangeFloatValue(0f, 3f) { minValue = 0f, maxValue = 3f };

    [SerializeField] protected float attackCycleTime = 1;
    [SerializeField] protected PBPart pbPart;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected List<PBAxis> forceAxis = new();

    public bool IsObstacle => pbPart is PBObstaclePart;
    public float AttackCycleTime => attackCycleTime;
    public PBPart PbPart { get => pbPart; set => pbPart = value; }
    public Rigidbody Rb => rb;

    #region Monobehaviour Methods
    protected virtual IEnumerator Start()
    {
        enabled = false;
        yield return new WaitForSeconds(GetRandomDelayTime());
        enabled = true;
    }

    protected virtual void OnValidate()
    {
        if (pbPart == null)
        {
            pbPart = GetComponent<PBPart>();
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
    }
    #endregion

    protected virtual Vector3 GetRigidbodyAxes()
    {
        return GetAxis(forceAxis, rb.transform);
    }

    protected virtual float GetRandomDelayTime()
    {
        var randomFactor = Random.Range(DelayRandomRange.minValue, DelayRandomRange.maxValue);
        return randomFactor;
    }

    public virtual bool IsAbleToDealDamage(Collision collision, out QueryResult queryResult)
    {
        queryResult = QueryResult.Default;
        return true;
    }

    public static Vector3 GetAxis(List<PBAxis> axes, Transform transform)
    {
        Vector3 forceAxis = Vector3.zero;
        foreach (var force in axes)
        {
            SetForce(force);
        }
        return forceAxis;

        Vector3 SetForce(PBAxis axis)
        {
            Vector3 additionalForce = Vector3.zero;
            switch (axis)
            {
                case PBAxis.Right:
                    additionalForce = transform.right;
                    break;
                case PBAxis.Left:
                    additionalForce = -transform.right;
                    break;
                case PBAxis.Up:
                    additionalForce = transform.up;
                    break;
                case PBAxis.Down:
                    additionalForce = -transform.up;
                    break;
                case PBAxis.Forward:
                    additionalForce = transform.forward;
                    break;
                case PBAxis.Backward:
                    additionalForce = -transform.forward;
                    break;
                default:
                    break;
            }
            if (forceAxis == Vector3.zero) forceAxis = additionalForce;
            else forceAxis += additionalForce;
            return forceAxis;
        }
    }
}