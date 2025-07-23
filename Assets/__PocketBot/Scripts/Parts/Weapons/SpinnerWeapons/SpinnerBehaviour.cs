using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpinnerBehaviour : PartBehaviour, IBoostFireRate
{
    [SerializeField] bool isEnabledManualForceApply;
    [SerializeField] float minForcePower = 40f;
    [SerializeField] float accelerateSpeed;
    [SerializeField] float topSpinningSpeed = 100f;

    RigidbodyState lastTickState;

    float AcceleratePower => accelerateSpeed * rb.mass;
    Vector3 RotateAxis => GetRigidbodyAxes();

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
        SpinnerImpactCollider spinnerImpactCollider = GetComponent<SpinnerImpactCollider>();
        spinnerImpactCollider.ApplyCollisionForceCallback = HandleApplyCollisionForce;
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        float AcceleratePowerHandle = m_IsSpeedUp ? AcceleratePower * m_ObjectTimeScale : AcceleratePower;
        rb.AddTorque(AcceleratePowerHandle * Time.fixedDeltaTime * RotateAxis, ForceMode.Acceleration);
        lastTickState.UpdateState(rb);
    }
    void OnDisable()
    {
        rb.angularVelocity = Vector3.zero;
    }

    void HandleApplyCollisionForce(CollisionData collisionData)
    {
        var rb = collisionData.chassisRb;
        var direction = Vector3.Cross(lastTickState.angularVelocity, collisionData.collisionInfo.contactPoint - lastTickState.position).normalized;
        rb.AddForceAtPosition(collisionData.manualForce * rb.mass * direction, collisionData.collisionInfo.contactPoint, ForceMode.Impulse);
    }

    public override bool IsAbleToDealDamage(Collision collision, out QueryResult queryResult)
    {
        queryResult = QueryResult.Default;
        var forcePower = Vector3.Dot(lastTickState.angularVelocity, Vector3.up);
        if (forcePower < minForcePower)
            return false;
        return true;
    }

    public bool IsEnabledManualForceApply()
    {
        return isEnabledManualForceApply;
    }

    public float CalcForcePower()
    {
        return (IsEnabledManualForceApply() ? lastTickState.angularVelocity.magnitude : rb.angularVelocity.magnitude) / topSpinningSpeed;
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