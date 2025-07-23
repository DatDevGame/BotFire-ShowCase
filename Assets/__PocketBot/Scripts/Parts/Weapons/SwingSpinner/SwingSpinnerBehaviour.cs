using HyrphusQ.Events;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwingSpinnerBehaviour : PartBehaviour, IBoostFireRate
{
    [SerializeField] float minForcePower = 40f;
    [SerializeField] float topSpinningSpeed = 100f;
    [SerializeField] float torqueAmount = 10;
    [SerializeField] float pushForce = 10;
    [SerializeField] float switchDirectionCooldown = 1;
    [SerializeField] PBCollider pbCollider;

    protected float lastTimeDealDamage;
    float switchDirectionCounter = 0;
    bool isClockWise = true;

    private bool m_IsSpeedUp = false;
    private int m_StackSpeedUp;
    private float m_ObjectTimeScale;
    private float m_TimeScaleOrginal => TimeManager.Instance.originalScaleTime;
    private float m_BoosterPercent;

    [SerializeField, BoxGroup("Visual")] private List<MeshRenderer> m_MeshRendererBooster;
    void Awake()
    {
        m_ObjectTimeScale = m_TimeScaleOrginal;

        rb.maxAngularVelocity = topSpinningSpeed;
        pbCollider.ApplyCollisionForceCallback = HandleApplyCollisionForce;
    }

    void FixedUpdate()
    {
        float deltaTime = Time.deltaTime * m_ObjectTimeScale;
        switchDirectionCounter += deltaTime;
        if (switchDirectionCounter > switchDirectionCooldown)
        {
            switchDirectionCounter = 0;
            isClockWise = !isClockWise;
        }
        rb.AddTorque(new Vector3(0f, (isClockWise ? 1 : -1) * torqueAmount, 0f), ForceMode.Acceleration);
    }

    public override bool IsAbleToDealDamage(Collision collision, out QueryResult queryResult)
    {
        queryResult = QueryResult.Default;
        if (Time.time - lastTimeDealDamage < attackCycleTime)
            return false;
        lastTimeDealDamage = Time.time;
        var forcePower = Vector3.Dot(collision.relativeVelocity, -transform.forward);
        return forcePower >= minForcePower;
    }

    void HandleApplyCollisionForce(CollisionData collisionData)
    {
        var rb = collisionData.chassisRb;
        var forceDirection = transform.forward + rb.velocity.normalized;
        rb.AddForce(collisionData.forceMultiplier * pushForce * forceDirection, ForceMode.VelocityChange);
    }
    public void BoostSpeedUpFire(float boosterPercent)
    {
        m_BoosterPercent += boosterPercent;

        //Enable VFX
        if (m_MeshRendererBooster != null && m_MeshRendererBooster.Count > 0)
            PBBoosterVFXManager.Instance.PlayBoosterSpeedUpAttackWeapon(pbPart, m_MeshRendererBooster);

        m_StackSpeedUp++;
        m_IsSpeedUp = true;
        m_ObjectTimeScale += m_TimeScaleOrginal * boosterPercent;
    }
    public void BoostSpeedUpStop(float boosterPercent)
    {
        m_StackSpeedUp--;
        if (m_StackSpeedUp <= 0)
        {
            //Disable VFX
            PBBoosterVFXManager.Instance.StopBoosterSpeedUpAttackWeapon(pbPart);

            m_IsSpeedUp = false;
            m_ObjectTimeScale = m_TimeScaleOrginal;
            m_BoosterPercent = 0;
        }
        else
        {
            m_BoosterPercent -= boosterPercent;
            m_ObjectTimeScale -= m_TimeScaleOrginal * boosterPercent;
        }
    }
    public bool IsSpeedUp() => m_IsSpeedUp;
    public int GetStackSpeedUp() => m_StackSpeedUp;
    public float GetPercentSpeedUp() => m_BoosterPercent;
}