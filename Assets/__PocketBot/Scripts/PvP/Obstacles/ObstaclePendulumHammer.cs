using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePendulumHammer : PartBehaviour
{
    [SerializeField, BoxGroup("Config")] private float m_MinAngle;
    [SerializeField, BoxGroup("Config")] private float m_MaxAngle;
    [SerializeField, BoxGroup("Config")] private float m_PushForce = 10f;
    [SerializeField, BoxGroup("Config")] private float m_Speed = 1f;
    [SerializeField, BoxGroup("Config")] protected float m_MinForcePower = 20f;
    [SerializeField, BoxGroup("Config")] private AnimationCurve m_CurveX;

    [SerializeField, BoxGroup("Ref")] private ImpactCollider m_ImpactCollider;
    [SerializeField, BoxGroup("Ref")] private Transform m_LeftHammer;
    [SerializeField, BoxGroup("Ref")] private Transform m_RightHammer;
    [SerializeField, BoxGroup("Ref")] private Rigidbody m_Rigidbody;

    private float m_Time;

    private float m_LastTimeDealDamage;


    private void Start()
    {
        if(m_ImpactCollider != null)
            m_ImpactCollider.ApplyCollisionForceCallback = HandleApplyCollisionForce;
    }

    private void FixedUpdate()
    {
        m_Time += Time.fixedDeltaTime * m_Speed * Mathf.PI;

        float sinValue = Mathf.Sin(m_Time);

        float angle = Mathf.LerpUnclamped(m_MinAngle, m_MaxAngle, (sinValue + 1f) / 2f);

        Quaternion localRotation = Quaternion.Euler(angle, 0, 0);
        Quaternion worldRotation = transform.parent ? transform.parent.rotation * localRotation : localRotation;

        m_Rigidbody.MoveRotation(worldRotation);
    }


    private void HandleApplyCollisionForce(CollisionData collisionData)
    {
        if (collisionData.chassisRb == null) return;

        Rigidbody rb = collisionData.chassisRb;
        Vector3 hammerVelocity = m_Rigidbody.angularVelocity;
        bool isSwingingLeft = hammerVelocity.y > 0;
        Vector3 forceDirection = isSwingingLeft ? m_LeftHammer.forward : m_RightHammer.forward;
        forceDirection = (forceDirection + Vector3.up * 0.5f + Vector3.one);
        rb.AddForce(collisionData.forceMultiplier * m_PushForce * forceDirection, ForceMode.VelocityChange);
    }


    public override bool IsAbleToDealDamage(Collision collision, out QueryResult queryResult)
    {
        queryResult = QueryResult.Default;
        if (Time.time - m_LastTimeDealDamage < 0.1f)
            return false;

        m_LastTimeDealDamage = Time.time;
        return true;
    }
}
