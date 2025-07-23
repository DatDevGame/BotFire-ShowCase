using System.Collections;
using System.Collections.Generic;

using LatteGames.Template;

using Sirenix.OdinInspector;

using UnityEngine;

public class RocketBehaviour : PartBehaviour, IBoostFireRate
{
    [SerializeField] RocketBulletCollider rocketPrefab;
    [SerializeField] Transform rocketContainer;
    [SerializeField] private AimingLine _aimingLine;

    GameObject rocketWorldSpaceContainer;

    private PBChassis pbChassisBody;

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
        pbChassisBody = gameObject.GetComponentInParent<PBChassis>();
        rocketPrefab.PartBehaviour = this;
        yield return base.Start();
        yield return new WaitForEndOfFrame();
        rocketWorldSpaceContainer = new GameObject($"{PbPart.RobotBaseBody.name}_RocketBulletContainer");
        StartCoroutine(CR_Shoot());

        if (_aimingLine == null)
            _aimingLine = gameObject.GetComponentInChildren<AimingLine>();
    }

    private void OnDestroy()
    {
        Destroy(rocketWorldSpaceContainer);
    }

    protected virtual IEnumerator CR_Shoot()
    {
        var waitUntilEnable = new WaitUntil(() => { return this.enabled; });
        while (true)
        {
            yield return waitUntilEnable;
            float bodySpeed = 1;
            if (pbChassisBody != null)
                bodySpeed = (pbChassisBody.RobotBaseBody.velocity.magnitude <= 1
                    ? 1
                    : pbChassisBody.RobotBaseBody.velocity.magnitude);

            yield return CustomWaitForSeconds(AttackCycleTime / 2);

            if (_aimingLine != null)
                _aimingLine.OnAimLine();

            var instance = Instantiate(rocketPrefab, rocketContainer);
            instance.enabled = true;
            instance.gameObject.SetActive(true);
            instance.gameObject.layer = gameObject.layer;
            instance.PartBehaviour = this;
            if (instance != null)
                rocketPrefab.gameObject.SetActive(true);

            yield return CustomWaitForSeconds(AttackCycleTime / 2);
            rocketPrefab.gameObject.SetActive(false);

            if (_aimingLine != null)
                _aimingLine.OffAimLine();

            instance.transform.SetParent(rocketWorldSpaceContainer.transform);
            instance.Launch(bodySpeed);
            SoundManager.Instance.PlaySFX(SFX.RocketShooting, PBSoundUtility.IsOnSound() ? 0.3f : 0);
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