using System.Collections;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine;

public class CommonGun : GunBase
{
    [Header("Gun Configs")]
    [SerializeField] float fireRate = 1;
    [SerializeField] float spreadCount = 1;
    [SerializeField] float spreadAngle = 30f;
    [SerializeField] float burstCount = 2f;
    [SerializeField] float burstInterval = 0.05f;
    [SerializeField] float randomAngle = 2f;
    [Header("Lock On Configs")]
    [SerializeField] bool useLockOnFeature;
    [SerializeField, ShowIf("useLockOnFeature")] float lockOnDuration = 1f;
    [SerializeField, ShowIf("useLockOnFeature")] float warningDuration = 0.2f;
    [SerializeField, ShowIf("useLockOnFeature")] LineRenderer lineRenderer;
    [Header("Gun Refs")]
    [SerializeField] BulletBase bulletPrefab;
    [SerializeField] ParticleSystem muzzleVFX;
    [SerializeField] EZAnimBase burstAnim;

    protected RaycastHit[] raycastHits = new RaycastHit[1];
    protected Coroutine shootCoroutine;

    public float SpreadCount => spreadCount;
    public float BurstCount => burstCount;

    protected override void Awake()
    {
        base.Awake();
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (shootCoroutine != null) StopCoroutine(shootCoroutine);
        shootCoroutine = StartCoroutine(CR_Shoot());
    }

    IEnumerator CR_Shoot()
    {
        while (true)
        {
            yield return Yielders.Get(reload);
            if (aimController == null || aimController.aimTarget == null || aimController.aimTarget.Robot.IsDead)
            {
                continue;
            }
            yield return new WaitUntil(() => CanShootTarget(aimController.aimTarget));
            if (useLockOnFeature)
            {
                lineRenderer.enabled = true;
                lineRenderer.startColor = Color.yellow;
                lineRenderer.endColor = Color.yellow;
                lineRenderer.SetPosition(0, Vector3.zero);
                var t = 0f;
                while (t <= lockOnDuration + warningDuration)
                {
                    if (aimController == null || aimController.aimTarget == null || aimController.aimTarget.Robot.IsDead)
                    {
                        lineRenderer.enabled = false;
                        break;
                    }
                    if (t < lockOnDuration && t + Time.fixedDeltaTime >= lockOnDuration)
                    {
                        lineRenderer.startColor = Color.red;
                        lineRenderer.endColor = Color.red;
                    }
                    t += Time.fixedDeltaTime;

                    var diff = aimController.aimTarget.transform.position - shootingPoint.position;
                    var distance = diff.magnitude;
                    int raycastHitsCount = Physics.SphereCastNonAlloc(shootingPoint.position, bulletPrefab.radius, diff.normalized, raycastHits, distance, hitLayerMask);
                    if (raycastHitsCount > 0)
                    {
                        if (raycastHits[0].point == Vector3.zero)
                        {
                            lineRenderer.SetPosition(1, Vector3.zero);
                        }
                        else
                        {
                            lineRenderer.SetPosition(1, shootingPoint.InverseTransformPoint(raycastHits[0].point));
                        }
                    }
                    else
                    {
                        lineRenderer.SetPosition(1, shootingPoint.InverseTransformPoint(aimController.aimTarget.transform.position));
                    }
                    yield return Yielders.FixedUpdate;
                }
                lineRenderer.enabled = false;
            }
            for (int i = 0; i < burstCount; i++)
            {
                var shootDir = Quaternion.AngleAxis(Random.Range(-randomAngle, randomAngle), Vector3.up) * shootingPoint.transform.forward;
                for (int j = 0; j < spreadCount; j++)
                {
                    var bullet = BulletPoolManager.Instance.Get(bulletPrefab);
                    bullet.transform.position = shootingPoint.transform.position;
                    bullet.gameObject.layer = gameObject.layer;

                    float angleStep = spreadAngle / (spreadCount - 1);
                    float angle = -spreadAngle / 2 + j * angleStep;
                    Vector3 dir = spreadCount > 1 ? Quaternion.AngleAxis(angle, Vector3.up) * shootDir : shootDir;
                    bullet.transform.rotation = Quaternion.LookRotation(dir);

                    bullet.Init((x) => BulletPoolManager.Instance.Release(bulletPrefab, x), this);
                }
                PlayMuzzleSound();
                muzzleVFX?.Play();
                burstAnim?.Play();
                yield return Yielders.Get(burstInterval);
            }
        }
    }
}
