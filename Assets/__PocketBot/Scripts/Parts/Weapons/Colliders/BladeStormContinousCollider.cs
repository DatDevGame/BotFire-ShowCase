using DG.Tweening;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeStormContinousCollider : ContinuousCollider, IBoostFireRate, IPhaseTwoAttackable
{
    [SerializeField, BoxGroup("Config")] private float m_SpinningSpeed = 25f;
    [SerializeField, BoxGroup("Config")] private float m_PercentDamage = 10f;
    [SerializeField, BoxGroup("Ref")] private Transform m_Axis;
    [SerializeField, BoxGroup("Ref")] private ParticleSystem m_SparkVFX;
    private bool m_IsCanDamage = false;

    //Speed Up Handle
    private bool m_IsSpeedUp;
    private int m_StackSpeedUp;
    private float m_OriginalAttackSpeed;
    private float m_BoosterPercent;

    private void Start()
    {
        m_OriginalAttackSpeed = m_SpinningSpeed;
    }

    private void FixedUpdate()
    {
        if (m_IsCanDamage && m_Axis != null)
        {
            m_Axis.Rotate(Vector3.up * m_SpinningSpeed * Time.fixedDeltaTime, Space.Self);
        }
    }

    public void EnableContinous()
    {
        m_IsCanDamage = true;
    }

    public void DisableContinous()
    {
        m_IsCanDamage = false;

        if (m_SparkVFX != null)
            m_SparkVFX.Stop();
    }

    protected override void CollisionStayBehaviour(Collision collision)
    {
        if (!m_IsCanDamage)
            return;

        if (partBehaviour == null) return;
        if (collision.rigidbody == null) return;
        if (collision.rigidbody.GetComponent<IDamagable>() == null) return;

        if (m_IsSpeedUp)
        {
            damageCoolDownHandle = ApplyBooster(DamageCooldown, m_BoosterPercent);
            if (damageCoolDownHandle <= 0)
                damageCoolDownHandle = DamageCooldown;

            float ApplyBooster(float currentCooldown, float boosterPercent)
            {
                return currentCooldown / (1 + boosterPercent);
            }
        }
        else
            damageCoolDownHandle = DamageCooldown;

        if (Time.time - lastInflictedDamageTime < damageCoolDownHandle) return;

        //Match all condition
        lastInflictedDamageTime = Time.time;

        var contactPoint = transform.InverseTransformPoint(collision.GetContact(0).point);
        if (m_SparkVFX != null)
        {
            m_SparkVFX.transform.localPosition = new Vector3(0, contactPoint.y, contactPoint.z);
            m_SparkVFX.Play();
        }

        GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, partBehaviour.PbPart, collision, 0.1f, ApplyCollisionForceCallback);
    }

    protected override void CollisionExitBehaviour(Collision collision)
    {
        if (m_SparkVFX != null)
            m_SparkVFX.Stop();
        base.CollisionExitBehaviour(collision);
    }

    public void BoostSpeedUpFire(float boosterPercent)
    {
        m_StackSpeedUp++;
        m_IsSpeedUp = true;
        m_SpinningSpeed += m_OriginalAttackSpeed * boosterPercent;
        m_BoosterPercent += boosterPercent;
    }

    public void BoostSpeedUpStop(float boosterPercent)
    {
        m_StackSpeedUp--;
        if (m_StackSpeedUp <= 0)
        {
            m_IsSpeedUp = false;
            m_SpinningSpeed = m_OriginalAttackSpeed;
            m_BoosterPercent = 0;
        }
        else
        {
            m_BoosterPercent -= boosterPercent;
            m_SpinningSpeed -= m_OriginalAttackSpeed * boosterPercent;
        }
    }

    public float GetPercentSpeedUp()
    {
        return m_BoosterPercent;
    }

    public int GetStackSpeedUp()
    {
        return m_StackSpeedUp;
    }

    public bool IsSpeedUp()
    {
        return m_IsSpeedUp;
    }

    public float GetPercentDamage()
    {
        return m_PercentDamage / 100;
    }

    public bool IsActive() => m_IsCanDamage;
}
