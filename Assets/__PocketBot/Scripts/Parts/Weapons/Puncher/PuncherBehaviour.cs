using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public struct SerializedJointDrive
{
    [SerializeField]
    private float m_PositionSpring;
    [SerializeField]
    private float m_PositionDamper;
    [SerializeField]
    private float m_MaximumForce;

    //
    // Summary:
    //     Strength of a rubber-band pull toward the defined direction. Only used if mode
    //     includes Position.
    public float positionSpring
    {
        get
        {
            return m_PositionSpring;
        }
        set
        {
            m_PositionSpring = value;
        }
    }

    //
    // Summary:
    //     Resistance strength against the Position Spring. Only used if mode includes Position.
    public float positionDamper
    {
        get
        {
            return m_PositionDamper;
        }
        set
        {
            m_PositionDamper = value;
        }
    }

    //
    // Summary:
    //     Amount of force applied to push the object toward the defined direction.
    public float maximumForce
    {
        get
        {
            return m_MaximumForce;
        }
        set
        {
            m_MaximumForce = value;
        }
    }

    public static SerializedJointDrive Default => new SerializedJointDrive()
    {
        m_PositionSpring = 0f,
        m_PositionDamper = 0f,
        m_MaximumForce = float.MaxValue,
    };
}

public class PuncherBehaviour : PartBehaviour, IBoostFireRate
{
    [SerializeField] protected float m_LenghtPush = 2;
    [SerializeField] protected float pushForce = 30f;
    [SerializeField] protected float punchForwardDelayTime = 0.2f;
    [SerializeField] protected float goBackwardDelayTime = 0.5f;
    [SerializeField] protected Transform originPoint;
    [SerializeField] protected Transform destinationPoint;
    [SerializeField] protected Transform puncherPipe;
    [SerializeField] protected PBCollider pbCollider;
    [SerializeField] protected ConfigurableJoint puncherJoint;
    [SerializeField] protected Rigidbody puncherRb;
    [SerializeField] protected SerializedJointDrive forwardJointDrive = SerializedJointDrive.Default;
    [SerializeField] protected SerializedJointDrive backwardJointDrive = SerializedJointDrive.Default;
    [SerializeField] protected OnTriggerCallback onTriggerCallback;
    [SerializeField] protected float validTargetTimeToAttack;

    protected Vector3 originLocalScale;
    protected Vector3 originPos;
    protected Vector3 destinationPos;
    protected Vector3 originPuncherPos;

    protected bool m_IsSpeedUp = false;
    protected bool m_ValidTargetLastFrame = false;
    protected bool m_ValidTargetThisFrame = false;
    protected bool m_DidPhysicUpdate = false;
    protected bool m_IsPunching = false;
    protected int m_StackSpeedUp;
    protected float m_ObjectTimeScale;
    protected float m_TimeScaleOrginal => TimeManager.Instance.originalScaleTime;
    protected float m_BoosterPercent;
    protected float m_ValidTargetEnterTime;


    [SerializeField, BoxGroup("Visual")] protected List<MeshRenderer> m_MeshRendererBooster;
    [SerializeField, BoxGroup("Visual")] protected List<SkinnedMeshRenderer> m_SkinnedMeshRendererBooster;

    protected override IEnumerator Start()
    {
        m_ObjectTimeScale = m_TimeScaleOrginal;

        pbCollider.ApplyCollisionForceCallback = HandleApplyCollisionForce;
        originPos = originPoint.localPosition;
        destinationPos = destinationPoint.localPosition;
        originPuncherPos = puncherJoint.transform.localPosition;
        originLocalScale = puncherPipe.localScale;
        yield return base.Start();
        StartCoroutine(DOPunch_CR());
    }

    protected virtual void LateUpdate()
    {
        if (puncherJoint == null) return;
        puncherPipe.localScale = new Vector3(originLocalScale.x, originLocalScale.y, (pbCollider.transform.localPosition.z * 3) * (originLocalScale.z / 3f));
    }

    void HandleApplyCollisionForce(CollisionData collisionData)
    {
        pbCollider.enabled = false;
        m_IsPunching = false;
        var rb = collisionData.chassisRb;
        rb.AddForce(collisionData.forceMultiplier * pushForce * puncherJoint.transform.forward, ForceMode.VelocityChange);
    }

    public override bool IsAbleToDealDamage(Collision collision, out QueryResult queryResult)
    {
        queryResult = QueryResult.Default;
        return m_IsPunching;
    }

    protected virtual IEnumerator DOPunch_CR()
    {
        var waitUntilEnable = new WaitUntil(() => { return enabled; });
        var softJointLimit = puncherJoint.linearLimit;

        while (true)
        {
            if (puncherJoint == null)
                break;

            yield return waitUntilEnable;

            pbCollider.enabled = true;

            // Scale thực tế
            Vector3 scaleFactor = transform.localScale;
            float scaledDistance = Vector3.Distance(originPoint.position, destinationPoint.position) * scaleFactor.z;

            // Cập nhật Linear Limit
            softJointLimit.limit = scaledDistance;

            m_IsPunching = true;
            puncherJoint.linearLimit = softJointLimit;

            // Chuyển về tọa độ local
            Vector3 localDestination = originPoint.InverseTransformPoint(destinationPoint.position);
            puncherJoint.targetPosition = new Vector3(0, 0, -localDestination.z * transform.localScale.z);

            // Áp dụng lực
            puncherJoint.zDrive = new JointDrive()
            {
                positionSpring = forwardJointDrive.positionSpring,
                positionDamper = forwardJointDrive.positionDamper,
                maximumForce = forwardJointDrive.maximumForce
            };

            yield return new WaitForSeconds(punchForwardDelayTime);

            pbCollider.enabled = false;

            // Quay về gốc
            Vector3 localOrigin = originPoint.InverseTransformPoint(originPoint.position);
            puncherJoint.targetPosition = new Vector3(0, 0, -localOrigin.z);
            m_IsPunching = false;
            puncherJoint.zDrive = new JointDrive()
            {
                positionSpring = backwardJointDrive.positionSpring,
                positionDamper = backwardJointDrive.positionDamper,
                maximumForce = backwardJointDrive.maximumForce
            };

            yield return new WaitForSeconds(goBackwardDelayTime);

            softJointLimit.limit = 0;
            puncherJoint.linearLimit = softJointLimit;

            yield return CustomWaitForSeconds(attackCycleTime - goBackwardDelayTime);
        }
    }



    /// <summary>
    /// Custom wait method to respect the object's time scale.
    /// </summary>
    /// <param name="time">Time to wait in seconds.</param>
    /// <returns>Yield instruction with custom time scale.</returns>
    protected IEnumerator CustomWaitForSeconds(float time)
    {
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime * m_ObjectTimeScale; // Apply custom time scale
            yield return null;
        }
    }
    public void BoostSpeedUpFire(float boosterPercent)
    {
        m_BoosterPercent += boosterPercent;

        //Enable VFX
        if (m_SkinnedMeshRendererBooster != null && m_SkinnedMeshRendererBooster.Count > 0)
            PBBoosterVFXManager.Instance.PlayBoosterSpeedUpAttackWeapon(pbPart, m_SkinnedMeshRendererBooster);
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        destinationPoint.localPosition = new Vector3(0, 0, m_LenghtPush);
        Gizmos.DrawLine(transform.position, destinationPoint.localPosition);
        DrawSphere(destinationPoint.position, transform.forward, Color.cyan);
    }
    private void DrawSphere(Vector3 position, Vector3 direction, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawSphere(position, transform.localScale.x / 2);
    }
#endif
}