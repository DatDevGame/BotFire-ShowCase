using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;

public class GunBehaviour : PartBehaviour, IBoostFireRate
{
    public event Action<bool> onShootingStateChanged = delegate { };

    [SerializeField] protected float m_StartDelay = 0;
    [SerializeField] protected float shootingTime = 2;
    [SerializeField] ParticleSystem particle;
    [SerializeField] ContinuousTriggerCollider continuousTriggerCollider;

    bool isShooting = false;
    protected bool IsShooting
    {
        set
        {
            isShooting = value;
            continuousTriggerCollider.CanInflictDamage = value;
            if (isShooting == true) particle.Play();
            else particle.Stop();
            onShootingStateChanged.Invoke(value);
        }
    }

    //Speed Up Handle
    private bool m_IsSpeedUp = false;
    private int m_StackSpeedUp;
    private float m_ObjectTimeScale;
    private float m_TimeScaleOrginal => TimeManager.Instance.originalScaleTime;
    private float m_BoosterPercent;

    [SerializeField, BoxGroup("Visual")] private List<MeshRenderer> m_MeshRendererBooster;
    private void Awake()
    {
        m_ObjectTimeScale = m_TimeScaleOrginal;
    }

    protected override IEnumerator Start()
    {
        enabled = false;
        yield return new WaitForSeconds(m_StartDelay);
        enabled = true;

        StartCoroutine(CR_Shoot());
    }

    void OnDestroy()
    {
        particle.Stop();
    }

    protected virtual IEnumerator CR_Shoot()
    {
        var waitUntilEnable = new WaitUntil(() => { return this.enabled; });
        while (true)
        {
            yield return waitUntilEnable;
            IsShooting = true;
            if (IsObstacle == false) SoundManager.Instance.PlayLoopSFX(SFX.Flamethrower, PBSoundUtility.IsOnSound() ? 0.25f : 0, true, true, gameObject);
            yield return new WaitForSeconds(shootingTime);
            IsShooting = false;
            if (IsObstacle == false) SoundManager.Instance.PauseLoopSFX(gameObject);
            yield return CustomWaitForSeconds(attackCycleTime);
        }
    }

    private void OnDisable()
    {
        IsShooting = false;
        if (IsObstacle == false) SoundManager.Instance?.PauseLoopSFX(gameObject);
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
    /// <summary>
    /// Custom wait method to respect the object's time scale.
    /// </summary>
    /// <param name="time">Time to wait in seconds.</param>
    /// <returns>Yield instruction with custom time scale.</returns>
    private IEnumerator CustomWaitForSeconds(float time)
    {
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime * m_ObjectTimeScale; // Apply custom time scale
            yield return null;
        }
    }
}
