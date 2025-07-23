using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine.Utility;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using LatteGames;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarPhysics : MonoBehaviour
{

    const float FLIP_FORCE = 15f;
    const float SPEED_STANDSTILL_THRESHOLD = 1f;
    const float MIN_TURNING_SPEED = 0f;
    const float MIN_STEERING_SPEED = 0f;
    public const float MAX_SPEED = 8f * 2f / 3f;

    public event Action OnCarImmobilized = delegate { };
    public event Action OnCarRecoveredFromImmobilized = delegate { };

    //Tweak
    [SerializeField] float turning = 50;
    [SerializeField] LayerMask raycastMask;

    //Config
    [SerializeField] CarConfigSO carConfigSO;

    Vector3 originCenterOfMass;
    Vector3 inputDir;
    Bounds localBounds;
    float accelInput;
    float rotationInput;

    bool isImmobilizedState = false;

    Vector3 currentRotation;
    float currentAcceleration;
    StackedBool canMove;
    bool isPreview = true;
    float lockBrakeTime;
    float lastTimeCastRay;
    float steeringSpeed;
    RaycastHit currentRaycastHitTarget;
    RigidbodyState lastTickState;
    PBLevelController levelController;
    PBRobot robot;
    Rigidbody carRb;
    Coroutine brakingCoroutine;
    BodyTransformer bodyTransformer;
    // Speed (length of vector velocity) in the last 15 frames
    SmoothingFloatBuffer speedBuffer = new SmoothingFloatBuffer(15, 10);
    // Position in the last 60 frames
    PositionBuffer positionBuffer = new PositionBuffer(60, Vector3.zero);
    SmoothingBoolBuffer immobilizedSmoothBuffer = new SmoothingBoolBuffer(5, false);

    public bool CanMove
    {
        get => canMove;
        set => canMove.value = value;
    }
    public Vector3 InputDir { get => CanMove ? inputDir : Vector3.zero; set => inputDir = value; }
    public float AccelInput { get => CanMove ? accelInput : 0; set => accelInput = value; }
    public float RotationInput { get => CanMove ? rotationInput : 0; set => rotationInput = value; }

    public CarConfigSO CarConfigSO { get => carConfigSO; set => carConfigSO = value; }

    public bool IsImmobilized => immobilizedSmoothBuffer.Get();
    public float Drag { get => carRb.drag; set => carRb.drag = value; }
    public float FrontTireGripMultiplier { get; set; } = 1f;
    public float RearTireGripMultiplier { get; set; } = 1f;
    public float TireGripFactor { get; set; } = 1f;
    public float CarTopSpeedMultiplier { get; set; } = 1f;
    public float Turning { get => Mathf.Max(turning * TurningMultiplier * robot.RotationSpeedMultiplier, MIN_TURNING_SPEED); set => turning = value; }
    public float TurningMultiplier { get; set; } = 1f;
    public float SteeringSpeed { get => Mathf.Max(steeringSpeed * SteeringSpeedMultiplier * robot.RotationSpeedMultiplier, MIN_STEERING_SPEED); set => steeringSpeed = value; }
    public float SteeringSpeedMultiplier { get; set; } = 1f;
    public bool IsAbleToBrake
    {
        get
        {
            return Time.time - lockBrakeTime > 0f;
        }
    }
    public float LockBrakeTime
    {
        get
        {
            return lockBrakeTime;
        }
        set
        {
            if (lockBrakeTime < Time.time)
                lockBrakeTime = Time.time + value;
            else
                lockBrakeTime += value;
            if (!IsAbleToBrake)
            {
                if (brakingCoroutine != null)
                    StopCoroutine(brakingCoroutine);
                brakingCoroutine = null;
            }
        }
    }
    public bool IsPreview { set => isPreview = value; }
    public bool CanAutoBalance { get; set; } = true;
    public Vector3 PreviousOneSecPosition => positionBuffer.GetBuffer().First.Value;
    public Bounds LocalBounds => localBounds;
    public Bounds WorldBounds => new Bounds(transform.TransformPoint(LocalBounds.center), transform.TransformVector(LocalBounds.size));
    public LayerMask RaycastMask => raycastMask;
    public RaycastHit CurrentRaycastHitTarget
    {
        get
        {
            var currentTime = Time.fixedTime;
            if (lastTimeCastRay < currentTime)
            {
                lastTimeCastRay = currentTime;
                Physics.Raycast(CarRb.position, Vector3.down, out currentRaycastHitTarget, 3f, RaycastMask, QueryTriggerInteraction.Ignore);
            }
            return currentRaycastHitTarget;
        }
    }
    public RigidbodyState LastTickState => lastTickState;
    public SmoothingFloatBuffer SpeedBuffer => speedBuffer;
    public Rigidbody CarRb
    {
        get
        {
            if (carRb == null)
            {
                carRb = GetComponent<Rigidbody>();
            }
            return carRb;
        }
    }

    void Awake()
    {
        ObjectFindCache<CarPhysics>.Add(this);
    }

    void Start()
    {
        robot = GetComponentInParent<PBRobot>();
        if (!robot.IsPreview)
            levelController = ObjectFindCache<PBLevelController>.Get();
        bodyTransformer = robot.GetComponentInChildren<BodyTransformer>();
        originCenterOfMass = CarRb.centerOfMass;
        Transform meshTransform = null;
        var meshFilter = GetComponentInChildren<MeshFilter>();
        var meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (meshFilter == null)
        {
            meshTransform = meshRenderer.transform;
        }
        else
        {
            meshTransform = meshFilter.transform;
        }
        var localMeshMatrix = Matrix4x4.TRS(meshTransform.localPosition, meshTransform.transform.localRotation, meshTransform.transform.localScale);
        localBounds = meshFilter == null ? meshRenderer.sharedMesh.bounds : meshFilter.sharedMesh.bounds;
        localBounds.center = localMeshMatrix.MultiplyPoint(localBounds.center);
        localBounds.size = meshTransform.transform.localRotation * localBounds.size;

        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, HandleLevelStart);
        if (levelController != null)
        {
            levelController.OnRemoveAliveCompetitor += OnAnyPlayerDied;
        }
    }

    void OnDestroy()
    {
        ObjectFindCache<CarPhysics>.Remove(this);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, HandleLevelStart);
        if (levelController != null)
        {
            levelController.OnRemoveAliveCompetitor -= OnAnyPlayerDied;
        }
    }

    void FixedUpdate()
    {
        if (InputDir != Vector3.zero)
        {
            currentRotation = InputDir;
            CarRb.MovePosition(CarRb.position + MAX_SPEED * robot.ChassisInstance.ChassisSO.Speed * Time.fixedDeltaTime * InputDir.normalized);
            CarRb.MoveRotation(Quaternion.RotateTowards(CarRb.rotation, Quaternion.LookRotation(currentRotation), 10f));
        }
        else
        {
            CarRb.velocity = Vector3.up * CarRb.velocity.y;
            if (currentRotation != Vector3.zero)
            {
                CarRb.MoveRotation(Quaternion.RotateTowards(CarRb.rotation, Quaternion.LookRotation(currentRotation), 10f));
            }
        }
    }

    void HandleLevelStart()
    {
        isPreview = false;
        CanMove = true;
    }

    void OnAnyPlayerDied()
    {
        if (levelController != null && levelController.AliveCompetitors.Count <= 1)
        {
            CanMove = false;
        }
    }

    public void Brake(float duration = 0.125f)
    {
        if (IsImmobilized || brakingCoroutine != null || carRb == null || !IsAbleToBrake)
            return;
        var currentMagnitude = carRb.velocity.magnitude;
        brakingCoroutine = StartCoroutine(CommonCoroutine.LerpFactor(duration, t =>
        {
            if (!IsImmobilized)
            {
                var value = carConfigSO.brakingCurve.Evaluate(1f - t);
                carRb.velocity = currentMagnitude * value * carRb.velocity.normalized;
            }
            if (t == 1f)
            {
                brakingCoroutine = null;
            }
        }));
    }

    public void Flip()
    {
        var dir = (-transform.up + -transform.forward).normalized;
        var frontOfBody = carRb.transform.TransformPoint(localBounds.center + Vector3.forward * localBounds.extents.z - Vector3.up * localBounds.extents.y);
        carRb.velocity = Vector3.zero;
        carRb.AddForceAtPosition(dir * FLIP_FORCE, frontOfBody + transform.forward, ForceMode.VelocityChange);
    }
    public void FlipTiming(FlipForce flipForce)
    {
        float forceValue = flipForce switch
        {
            FlipForce.VeryLow => 1,
            FlipForce.Low => 2,
            FlipForce.Medium => 3,
            FlipForce.High => FLIP_FORCE,
            _ => 1
        };

        var dir = (-transform.up + -transform.forward).normalized;
        var frontOfBody = carRb.transform.TransformPoint(localBounds.center + Vector3.forward * localBounds.extents.z - Vector3.up * localBounds.extents.y);
        carRb.velocity = Vector3.zero;
        carRb.AddForceAtPosition(dir * forceValue, frontOfBody + transform.forward, ForceMode.VelocityChange);

    }

    public bool IsStandstill()
    {
        return Mathf.Abs(AccelInput) >= 0.2f
        && speedBuffer.GetAvg() <= SPEED_STANDSTILL_THRESHOLD;
    }

    public bool IsUpsideDown()
    {
        return isImmobilizedState;
    }

    public float CalcDiagonalOfBounds()
    {
        return Mathf.Sqrt(Mathf.Pow(localBounds.size.x, 2f) + Mathf.Pow(localBounds.size.z, 2f));
    }

    public void ResetAcceleration()
    {
        currentAcceleration = 0;
    }
}

public class SmoothingBoolBuffer
{
    bool initialValue;
    public SmoothingBoolBuffer(int bufferSize, bool initialValue, float threshold = 0.8f)
    {
        m_BufferIndex = 0;
        m_Threshold = threshold;
        m_BoolBuffer = new bool[bufferSize];
        this.initialValue = initialValue;
        for (int i = 0; i < m_BoolBuffer.Length; i++)
        {
            m_BoolBuffer[i] = this.initialValue;
        }
    }

    private float m_Threshold;
    private int m_BufferIndex;
    public bool[] m_BoolBuffer;

    public bool Get()
    {
        var total = 0f;
        for (int i = 0; i < m_BoolBuffer.Length; i++)
        {
            if (m_BoolBuffer[i])
                total++;
        }
        return (total / m_BoolBuffer.Length) >= m_Threshold;
    }

    public void Add(bool value)
    {
        m_BoolBuffer[m_BufferIndex] = value;
        m_BufferIndex = (m_BufferIndex + 1) % m_BoolBuffer.Length;
    }

    public void Reset()
    {
        for (int i = 0; i < m_BoolBuffer.Length; i++)
        {
            m_BoolBuffer[i] = initialValue;
        }
    }
}

public abstract class SmoothingBuffer<T>
{
    public SmoothingBuffer(int bufferSize, T initialValue)
    {
        m_BufferIndex = 0;
        m_Buffer = new T[bufferSize];
        for (int i = 0; i < m_Buffer.Length; i++)
        {
            m_Buffer[i] = initialValue;
        }
    }

    protected int m_BufferIndex;
    protected T[] m_Buffer;

    public virtual T[] GetBuffer()
    {
        return m_Buffer;
    }

    public virtual void Add(T value)
    {
        m_Buffer[m_BufferIndex] = value;
        m_BufferIndex = (m_BufferIndex + 1) % m_Buffer.Length;
    }

    public abstract T GetAvg();
}

public class SmoothingFloatBuffer : SmoothingBuffer<float>
{
    public SmoothingFloatBuffer(int bufferSize, float initialValue) : base(bufferSize, initialValue)
    {
    }

    public override float GetAvg()
    {
        var total = 0f;
        for (int i = 0; i < m_Buffer.Length; i++)
        {
            total += m_Buffer[i];
        }
        return total / m_Buffer.Length;
    }
}

public class PositionBuffer
{
    public PositionBuffer(int bufferSize, Vector3 initialValue)
    {
        m_Buffer = new LinkedList<Vector3>(new Vector3[bufferSize].FillAll(initialValue));
    }

    private LinkedList<Vector3> m_Buffer;

    public virtual LinkedList<Vector3> GetBuffer()
    {
        return m_Buffer;
    }

    public virtual void Add(Vector3 point)
    {
        var head = m_Buffer.First;
        head.Value = point;
        m_Buffer.RemoveFirst();
        m_Buffer.AddLast(head);
    }
}

public struct StackedBool
{
    public StackedBool(bool value = false)
    {
        m_StackCount = value ? 1 : 0;
    }

    private int m_StackCount;

    public bool value
    {
        get => m_StackCount > 0;
        set
        {
            if (value)
                m_StackCount++;
            else
                m_StackCount--;
        }
    }

    public static implicit operator bool(StackedBool stackedBool)
    {
        return stackedBool.value;
    }
}