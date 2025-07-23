using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using LatteGames;
using Sirenix.OdinInspector;
using UnityEngine;
using DG.Tweening;
using LatteGames.Template;

public class RocketArtilleryBehaviour : PartBehaviour, IBoostFireRate
{
    [SerializeField]
    private RocketArtilleryConfigSO configSO;
    [SerializeField]
    private Transform bulletContainer;
    [SerializeField]
    private BoxObjectPlacer boxObjectPlacer;
    [SerializeField]
    private RocketArtilleryBullet bocketBulletPrefab;

    private PBRobot robot;
    private Transform rocketGunTransform;
    private WaitUntil waitUntilEnable;
    private WaitForSeconds waitTimePerFire;
    private Coroutine fireCoroutine;
    private BodyTransformer bodyTransformer;
    private List<Vector3> bulletSpawnPoints = new List<Vector3>();
    private List<RocketArtilleryBullet> rocketBullets = new List<RocketArtilleryBullet>();

    public int MaxBulletSize => bulletSpawnPoints.Count;
    public RocketArtilleryConfigSO ConfigSO => configSO;
    public BodyTransformer BodyTransformer => bodyTransformer;

    private bool m_IsSpeedUp = false;
    private int m_StackSpeedUp;
    private float m_ObjectTimeScale;
    private float m_TimeScaleOrginal => TimeManager.Instance.originalScaleTime;
    private float m_BoosterPercent;

    [SerializeField, BoxGroup("Visual")] private List<MeshRenderer> m_MeshRendererBooster;
    private void Awake()
    {
        m_ObjectTimeScale = m_TimeScaleOrginal;
        robot = pbPart.RobotChassis.Robot;
        bodyTransformer = robot.GetComponentInChildren<BodyTransformer>();
        rocketGunTransform = transform.parent.parent;
        boxObjectPlacer.IterateSpawnPoint((item1, item2) =>
        {
            bulletSpawnPoints.Add(item2);
        });
        bulletSpawnPoints = bulletSpawnPoints.OrderByDescending(item => item.y).ThenByDescending(item => item.x).ToList();
        waitUntilEnable = new WaitUntil(() => enabled);
        waitTimePerFire = new WaitForSeconds(configSO.FireRate);
    }

    protected override IEnumerator Start()
    {
        yield return base.Start();
        yield return Reload_CR();
        if (!robot.IsPreview)
            Fire();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.R))
            Reload();
        if (Input.GetKeyDown(KeyCode.S))
            Fire();
#endif
    }

    private Transform FindClosestTarget()
    {
        return null;
    }

    private void OnObjectHit(IDamagable damagableObject)
    {
        // SFX
        if (BodyTransformer.IsSoundEnabled())
            SoundManager.Instance.PlaySFX(SFX.TFM_MissleExplosion);
        if (damagableObject == null)
            return;
        damagableObject.ReceiveDamage(pbPart, 0f);
    }

    private void ReloadImmediately()
    {
        rocketBullets.Clear();
        for (int i = 0; i < MaxBulletSize; i++)
        {
            var rocketBullet = Instantiate(bocketBulletPrefab, bulletContainer);
            rocketBullet.transform.localPosition = bulletSpawnPoints[i];
            rocketBullet.Init(gameObject.layer, this);
            rocketBullet.name += $" [{i}]";
            rocketBullets.Add(rocketBullet);
        }
    }

    private IEnumerator Reload_CR()
    {
        ReloadImmediately();
        yield return CustomWaitForSeconds(configSO.ReloadTime);
    }

    [Button]
    private void Reload()
    {
        StartCoroutine(Reload_CR());
    }

    [Button]
    private void PlayRecoilAnim()
    {
        if (DOTween.IsTweening(rocketGunTransform))
            return;
        rocketGunTransform
            .DOPunchPosition(rocketGunTransform.InverseTransformDirection(-rocketGunTransform.forward) * configSO.RecoilStrength, configSO.RecoilAnimDuration, configSO.RecoilAnimVibrato, configSO.RecoilAnimElasticity)
            .SetEase(configSO.RecoilAnimEase)
            .SetUpdate(UpdateType.Late);
    }

    [Button]
    private void Fire(int index)
    {
        rocketBullets[index].Fire(target: FindClosestTarget(), onObjectHit: OnObjectHit);
        // Animation
        PlayRecoilAnim();
        // SFX & Haptic
        if (BodyTransformer.IsSoundEnabled())
        {
            SoundManager.Instance.PlaySFX(SFX.TFM_MissleShot);
            if (robot.PersonalInfo.isLocal)
                HapticManager.Instance.PlayFlashHaptic();
        }
    }

    public IEnumerator Fire_CR(bool isOneShot = false)
    {
        do
        {
            yield return waitUntilEnable;
            if (rocketBullets.Count <= 0)
                ReloadImmediately();
            for (int i = 0; i < MaxBulletSize; i++)
            {
                Fire(i);
                if (i < MaxBulletSize - 1)
                    yield return waitTimePerFire;
            }
            yield return Reload_CR();
        }
        while (!isOneShot);
        fireCoroutine = null;
    }

    [Button]
    public void Fire(bool isOneShot = false)
    {
        if (fireCoroutine != null)
            return;
        fireCoroutine = StartCoroutine(Fire_CR(isOneShot));
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