using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SmasherBehaviour : PartBehaviour, IBoostFireRate
{
    public bool IsEnable => _isEnable;

    [SerializeField] protected bool isModifyMassScale = true;
    [SerializeField] protected float minForcePower = 20f;
    [SerializeField] protected float torqueDownSpeed = 1500f;
    [SerializeField] protected float torqueUpSpeed = 300f;
    [SerializeField] protected float timeToLiftUp = 0.5f;

    protected int smashValue = -1;
    protected float torqueValue;
    protected float lastTimeDealDamage;

    protected bool _isEnable = false;

    //Speed Up Handle
    protected bool m_IsSpeedUp = false;
    protected int m_StackSpeedUp;
    protected float m_ObjectTimeScale;
    protected float m_TimeScaleOrginal => TimeManager.Instance?.originalScaleTime ?? 1f;
    protected float m_BoosterPercent;

    [SerializeField, BoxGroup("Visual")] protected List<MeshRenderer> m_MeshRendererBooster;
    [SerializeField, BoxGroup("Visual")] protected List<SkinnedMeshRenderer> m_SkinnedMeshRenderer;
    protected virtual void Awake()
    {
        m_ObjectTimeScale = m_TimeScaleOrginal;

        torqueValue = torqueUpSpeed;
        rb.maxAngularVelocity = 100f;
    }

    protected override IEnumerator Start()
    {
        if (isModifyMassScale)
        {
            var configurableJoint = GetComponent<ConfigurableJoint>();
            if (rb.mass > 1 && configurableJoint.massScale != 1f)
                configurableJoint.massScale = configurableJoint.connectedBody.mass / rb.mass;
        }
        if (smashValue <= 0)
        {
            torqueValue = torqueUpSpeed;
        }
        else
        {
            torqueValue = torqueDownSpeed;
        }
        yield return base.Start();
        yield return StartBehaviour_CR();
    }

    protected virtual void FixedUpdate()
    {
        rb.AddTorque(smashValue * Time.deltaTime * torqueValue * GetRigidbodyAxes(), ForceMode.VelocityChange);
    }

    protected virtual IEnumerator StartBehaviour_CR()
    {
        while (true)
        {
            // Smash down
            _isEnable = true;
            ChangeMomentumDirection();

            yield return new WaitForSeconds(timeToLiftUp);
            // Lift up
            _isEnable = torqueDownSpeed <= 0 && torqueUpSpeed <= 0 ? true : false;
            ChangeMomentumDirection();

            yield return CustomWaitForSeconds(attackCycleTime);
        }
    }

    protected virtual void ChangeMomentumDirection()
    {
        smashValue = -smashValue;
        if (smashValue <= 0)
        {
            torqueValue = torqueUpSpeed;
        }
        else
        {
            torqueValue = m_IsSpeedUp ? torqueDownSpeed * m_ObjectTimeScale : torqueDownSpeed;
        }
    }

    public override bool IsAbleToDealDamage(Collision collision, out QueryResult queryResult)
    {
        queryResult = QueryResult.Default;
        if (smashValue <= 0)
        {
            return false;
        }
        float attackCycleTimeHandle = m_IsSpeedUp ? 0 : attackCycleTime;
        if (Time.time - lastTimeDealDamage < attackCycleTimeHandle)
        {
            return false;
        }

        lastTimeDealDamage = Time.time;
        var forcePower = Vector3.Dot(collision.relativeVelocity, -transform.forward);
        return forcePower >= minForcePower;
    }

    public virtual void BoostSpeedUpFire(float boosterPercent)
    {
        m_BoosterPercent += boosterPercent;

        //Enable VFX
        if (m_MeshRendererBooster != null && m_MeshRendererBooster.Count > 0)
            PBBoosterVFXManager.Instance.PlayBoosterSpeedUpAttackWeapon(pbPart, m_MeshRendererBooster);
        if (m_SkinnedMeshRenderer != null && m_SkinnedMeshRenderer.Count > 0)
            PBBoosterVFXManager.Instance.PlayBoosterSpeedUpAttackWeapon(pbPart, m_SkinnedMeshRenderer);

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
    public virtual bool IsSpeedUp() => m_IsSpeedUp;
    public virtual int GetStackSpeedUp() => m_StackSpeedUp;
    public float GetPercentSpeedUp() => m_BoosterPercent;
    public int GetSmashValue() => smashValue;

    /// <summary>
    /// Custom wait method to respect the object's time scale.
    /// </summary>
    /// <param name="time">Time to wait in seconds.</param>
    /// <returns>Yield instruction with custom time scale.</returns>
    protected virtual IEnumerator CustomWaitForSeconds(float time)
    {
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime * m_ObjectTimeScale; // Apply custom time scale
            yield return null;
        }
    }
}