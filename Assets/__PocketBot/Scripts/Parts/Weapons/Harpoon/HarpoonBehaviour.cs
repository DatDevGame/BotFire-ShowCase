using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Template;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
public class HarpoonBehaviour : PartBehaviour, IBoostFireRate
{
    [SerializeField] HarpoonBulletCollider harpoonBulletCollider;
    [SerializeField] private AimingLine _aimingLine;
    private PBChassis pbChassisBody;
    private IEnumerator _autoAimCR;
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
        yield return base.Start();
        StartCoroutine(CR_Shoot());
    }
    void OnDestroy()
    {
        if (harpoonBulletCollider != null && harpoonBulletCollider.gameObject.activeInHierarchy)
        {
            harpoonBulletCollider.Revoke();
        }
        if (_autoAimCR != null)
            StopCoroutine(_autoAimCR);
    }
    protected virtual IEnumerator CR_Shoot()
    {
        var waitUntilEnable = new WaitUntil(() => { return this.enabled; });
        var waitForBackingHome = new WaitUntil(() => harpoonBulletCollider.State == HarpoonBulletState.OnHome);
        var waitForAttackCycleTime = new WaitForSeconds(attackCycleTime);
        while (true)
        {
            yield return waitUntilEnable;
            float bodySpeed = 1;
            if (pbChassisBody != null)
                bodySpeed = pbChassisBody.RobotBaseBody.velocity.magnitude <= 1
                    ? 1
                    : pbChassisBody.RobotBaseBody.velocity.magnitude;
            harpoonBulletCollider.Launch(bodySpeed);
            _aimingLine.OffAimLine();
            yield return waitForBackingHome;
            _aimingLine.OnAimLine();
            yield return CustomWaitForSeconds(attackCycleTime);
        }
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
    /// Custom wait method to respect the object’s time scale.
    /// </summary>
    /// <param name=“time”>Time to wait in seconds.</param>
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