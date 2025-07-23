using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using LatteGames;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;

public class AstroblastLazerGunBehaviour : PartBehaviour, IBoostFireRate
{
    public class LaserExplosion : MonoBehaviour
    {
        private AstroblastLazerGunConfigSO configSO;
        private ParticleSystem explosionVFX;
        private AstroblastLazerGunBehaviour laserGun;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, configSO.ExplosiveDamageRange);
        }

        private void HandleDealDamage(AstroblastLazerMissile missile, Vector3 point, Collider directHitCollider)
        {
            PBRobot directHitRobot = GetRobot(directHitCollider);
            HashSet<PBRobot> robotHashSet = new HashSet<PBRobot>();
            Collider[] detectedColliders = Physics.OverlapSphere(transform.position, configSO.ExplosiveDamageRange, Physics.DefaultRaycastLayers ^ (1 << laserGun.gameObject.layer), QueryTriggerInteraction.Ignore);
            Array.Sort(detectedColliders, (x, y) => Vector3.Distance(x.transform.position, transform.position).CompareTo(Vector3.Distance(y.transform.position, transform.position)));
            for (int i = 0; i < detectedColliders.Length; i++)
            {
                IDamagable hitTarget = detectedColliders[i].GetComponent<IDamagable>();
                if (hitTarget == null && detectedColliders[i].attachedRigidbody != null)
                {
                    hitTarget = detectedColliders[i].attachedRigidbody.GetComponent<IDamagable>();
                }
                if (hitTarget != null)
                {
                    PBRobot hitRobot = null;
                    Rigidbody hitRigidbody = detectedColliders[i].attachedRigidbody;
                    if (hitTarget is PBPart part && part.RobotChassis != null && part.RobotChassis.Robot != null)
                    {
                        if (robotHashSet.Contains(part.RobotChassis.Robot))
                            continue;
                        hitRobot = part.RobotChassis.Robot;
                        hitRigidbody = part.RobotBaseBody;
                        robotHashSet.Add(part.RobotChassis.Robot);
                    }

                    // Receive damage
                    hitTarget.ReceiveDamage(laserGun.pbPart, 0f);
                    LGDebug.Log($"{hitTarget} [{(hitTarget as PBPart)?.RobotChassis?.Robot}] ({Time.time}): receive laser explosion damage - {laserGun.PbPart.GetDamage()}");

                    // Receive force
                    if ((hitRobot != null && hitRobot.IsDead) || hitRigidbody == null)
                        continue;
                    if (laserGun.ConfigSO.IsExplosionForce)
                    {
                        hitRigidbody?.AddExplosionForce(laserGun.ConfigSO.ExplosiveForce, transform.position, laserGun.ConfigSO.ExplosiveDamageRange, laserGun.ConfigSO.ExplosiveUpwardsModifier, ForceMode.VelocityChange);
                    }
                    else
                    {
                        Vector3 forceDirection = hitRobot == directHitRobot ? missile.transform.forward : hitRigidbody.position - transform.position;
                        forceDirection.y = 0f;
                        Vector3 force = laserGun.ConfigSO.KnockBackForce * forceDirection.normalized;
                        if (hitRobot != null)
                        {
                            hitRobot.CombatEffectController.ApplyEffect(new AirborneEffect(0.1f, false, force, ForceMode.VelocityChange, Vector3.zero, ForceMode.VelocityChange, false, laserGun.PbPart.RobotChassis.Robot));
                        }
                        else
                        {
                            hitRigidbody?.AddForce(force, ForceMode.VelocityChange);
                        }
                        LGDebug.Log($"{hitRigidbody} [{(hitTarget as PBPart)?.RobotChassis?.Robot}] ({Time.time}): receive knockback force");
                    }
                }
            }

            PBRobot GetRobot(Collider collider)
            {
                if (collider != null && collider.attachedRigidbody != null && collider.attachedRigidbody.TryGetComponent(out PBPart part) && part.RobotChassis != null && part.RobotChassis.Robot != null)
                {
                    return part.RobotChassis.Robot;
                }
                return null;
            }
        }

        public void Initialize(AstroblastLazerGunConfigSO configSO, ParticleSystem explosionVFX, AstroblastLazerGunBehaviour laserGun)
        {
            this.configSO = configSO;
            this.explosionVFX = explosionVFX;
            this.laserGun = laserGun;
        }

        public void Explode(AstroblastLazerMissile missile, Vector3 point, Collider directHitCollider)
        {
            if (transform == null)
                return;
            transform.position = point;
            // VFX & SFX
            explosionVFX.Play();
            if (laserGun.BodyTransformer.IsSoundEnabled())
                SoundManager.Instance.PlaySFX(SFX.TFM_LaserGunExplosion);
            // Deal damage
            HandleDealDamage(missile, point, directHitCollider);
        }
    }

    [SerializeField]
    private AstroblastLazerGunConfigSO configSO;
    [SerializeField]
    private Transform firePoint;
    [SerializeField]
    private ParticleSystem chargingSmallEnergyVFX;
    [SerializeField]
    private ParticleSystem chargingBigEnergyVFX;
    [SerializeField]
    private AstroblastLazerMissile laserMissilePrefab;
    [SerializeField]
    private ParticleSystem explosionVFXPrefab;
    [SerializeField]
    private CinemachineImpulseSource cinemachineImpulseSource;

    private PBRobot robot;
    private WaitUntil waitUntilEnable;
    private WaitForSeconds waitTimePerFire;
    private Coroutine fireCoroutine;
    private LaserExplosion explosion;
    private Transform lazerGunTransform;
    private AstroblastLazerGunAutoAim gunAutoAim;
    private BodyTransformer bodyTransformer;

    public AstroblastLazerGunConfigSO ConfigSO => configSO;
    public LaserExplosion Explosion => explosion;
    public Transform LazerGunTransform => lazerGunTransform;
    public Transform FirePointTransform => firePoint;
    public BodyTransformer BodyTransformer => bodyTransformer;

    private bool m_IsSpeedUp = false;
    private int m_StackSpeedUp;
    private float m_ObjectTimeScale;
    private float m_TimeScaleOrginal => TimeManager.Instance.originalScaleTime;
    private float m_BoosterPercent;

    [SerializeField, BoxGroup("Visual")] private List<MeshRenderer> m_MeshRendererBooster;
    [SerializeField, BoxGroup("Visual")] private Transform m_FakeMeshVisualBooster;
    private void Awake()
    {
        m_ObjectTimeScale = m_TimeScaleOrginal;
        robot = pbPart.RobotChassis.Robot;
        waitUntilEnable = new WaitUntil(() => enabled);
        waitTimePerFire = new WaitForSeconds(configSO.ReloadTime - (configSO.ChargingEnergyPhase1Time + configSO.ChargingEnergyPhase2Time));
        lazerGunTransform = pbPart.RobotChassis?.transform.FindRecursive("gun_1");
        bodyTransformer = robot.GetComponentInChildren<BodyTransformer>();
        if (configSO.IsEnableAutoAim)
        {
            gunAutoAim = lazerGunTransform.gameObject.GetOrAddComponent<AstroblastLazerGunAutoAim>();
            gunAutoAim.Initialize(this);
        }
    }

    protected override IEnumerator Start()
    {
        var explosionVFX = Instantiate(explosionVFXPrefab);
        explosion = explosionVFX.gameObject.GetOrAddComponent<LaserExplosion>();
        explosion.Initialize(configSO, explosionVFX, this);
        yield return base.Start();
        if (!pbPart.RobotChassis.Robot.IsPreview)
            Fire();
    }

    private void OnEnable()
    {
        if (gunAutoAim != null)
            gunAutoAim.enabled = true;
    }

    private void OnDisable()
    {
        if (gunAutoAim != null)
            gunAutoAim.enabled = false;
    }

    private void OnDestroy()
    {
        if (explosion != null)
        {
            Destroy(explosion.gameObject);
        }
    }

    private IEnumerator LauchMissile_CR()
    {
        yield return CommonCoroutine.EndOfFrame;
        AstroblastLazerMissile laserMissileInstance = Instantiate(laserMissilePrefab);
        laserMissileInstance.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
        laserMissileInstance.Lanch(this);
        chargingBigEnergyVFX.transform
            .DOScale(0f, configSO.ScaleDownEnergyOrbDuration)
            .SetEase(configSO.ScaleDownEnergyOrbEase)
            .OnComplete(() => chargingBigEnergyVFX.Stop());
        // Anim
        PlayRecoilAnim();
        // SFX & Haptic
        if (BodyTransformer.IsSoundEnabled())
        {
            SoundManager.Instance.PlaySFX(SFX.TFM_LaserGunShot);
            if (robot.PersonalInfo.isLocal)
                HapticManager.Instance.PlayFlashHaptic(HapticTypes.HeavyImpact);
        }
    }

    [Button]
    private void PlayRecoilAnim()
    {
        if (lazerGunTransform == null)
            return;
        if (DOTween.IsTweening(lazerGunTransform))
            return;
        lazerGunTransform
            .DOPunchPosition(lazerGunTransform.InverseTransformDirection(-lazerGunTransform.forward) * configSO.RecoilStrength, configSO.RecoilAnimDuration, configSO.RecoilAnimVibrato, configSO.RecoilAnimElasticity)
            .SetEase(configSO.RecoilAnimEase)
            .SetUpdate(UpdateType.Late);
        if (configSO.IsEnableCamImpulse)
        {
            cinemachineImpulseSource.m_ImpulseDefinition.m_ImpulseDuration = configSO.CamImpulseDuration;
            cinemachineImpulseSource.GenerateImpulse(-firePoint.forward * configSO.CamImpulseStrength);
        }
    }

    public IEnumerator Fire_CR(bool isOneShot = false)
    {
        do
        {
            yield return waitUntilEnable;
            if (BodyTransformer.IsSoundEnabled())
                SoundManager.Instance.PlaySFX(SFX.TFM_LaserGunCharge);
            chargingSmallEnergyVFX.Play();
            chargingSmallEnergyVFX.transform.localScale = configSO.OriginEnergyOrbScale * Vector3.one;
            chargingSmallEnergyVFX.transform
                .DOScale(configSO.TargetEnergyOrbScale, configSO.ScaleUpEnergyOrbDuration)
                .SetEase(configSO.ScaleUpEnergyOrbEase);
            //WaitTimeToChargeEnergyPhase1
            yield return CustomWaitForSeconds(configSO.ChargingEnergyPhase1Time);
            chargingSmallEnergyVFX.Stop();
            chargingBigEnergyVFX.transform.localScale = Vector3.one;
            chargingBigEnergyVFX.Play();
            //WaitTimeToChargeEnergyPhase2
            yield return CustomWaitForSeconds(configSO.ChargingEnergyPhase2Time);
            yield return LauchMissile_CR();
            yield return waitTimePerFire;
        }
        while (!isOneShot);
        fireCoroutine = null;
    }

    public void Fire(bool isOneShot = false)
    {
        if (fireCoroutine != null)
            return;
        fireCoroutine = StartCoroutine(Fire_CR(isOneShot));
    }

    public void HandleFakeVisualBooster()
    {
        if (m_FakeMeshVisualBooster != null && bodyTransformer != null)
        {
            m_FakeMeshVisualBooster.localPosition = bodyTransformer.currentState == BodyTransformer.State.Idle ? Vector3.zero : new Vector3(0, 0, -2.5f);
            m_FakeMeshVisualBooster.localScale = bodyTransformer.currentState == BodyTransformer.State.Idle ? Vector3.one : new Vector3(1, 1, 4.5f);
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