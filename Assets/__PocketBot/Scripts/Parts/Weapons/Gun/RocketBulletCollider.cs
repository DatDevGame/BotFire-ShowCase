using System.Collections;
using System.Collections.Generic;

using HyrphusQ.Events;

using LatteGames;
using LatteGames.Template;

using UnityEngine;

public class RocketBulletCollider : PBCollider
{
    [SerializeField] float bulletLifeTime = 10;
    [SerializeField] float explodeRadius = 1.5f;
    [SerializeField] float explodeForce = 5f;
    [SerializeField] float launchMaxSpeed = 100f;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] List<PBAxis> movingDir;
    [SerializeField] AnimationCurve animationCurve;
    [SerializeField] ParticleSystem explodeFX;
    [SerializeField] ParticleSystem smokeFX;
    [SerializeField] PBPartSkin partSkin;

    private const float LAUCH_MAX_SPEED_DEFAULT = 100;

    public PartBehaviour PartBehaviour
    {
        set
        {
            partBehaviour = value;
            partSkin.part = partBehaviour.PbPart;
        }
    }

    float currentSpeed;
    Coroutine coroutine;

    public void Launch(float bodySpeed)
    {
        var rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        GetComponent<Collider>().enabled = true;
        coroutine = StartCoroutine(CR_Launch());
        smokeFX.Play();
    }

    protected override void TriggerEnterBehaviour(Collider other)
    {
        if (other.isTrigger)
        {
            if (other.GetComponent<RocketBulletCollider>() == null)
                return;
        }

        if (coroutine != null) StopCoroutine(coroutine);
        meshRenderer.enabled = false;
        var mySelfColliders = GetComponents<Collider>();
        SoundManager.Instance.PlaySFX(SFX.RocketExplosive, PBSoundUtility.IsOnSound() ? 0.6f : 0);
        foreach (var collider in mySelfColliders)
            collider.enabled = false;
        Collider[] colliders = Physics.OverlapSphere(explodeFX.transform.position, explodeRadius, -1 ^ (1 << gameObject.layer));
        foreach (var collider in colliders)
        {
            var damagable = collider.GetComponent<IDamagable>();
            if (damagable == null) continue;
            var closestPoint = collider.ClosestPoint(transform.position);
            var distance = Vector3.Distance(closestPoint, transform.position).Remap(0, explodeRadius, 0, 1);
            var collisionForce = Mathf.Lerp(explodeForce, 0, distance);
            var collisionInfo = new CollisionInfo
            (
                collider.GetComponent<Rigidbody>(),
                collider.transform.position - transform.position,
                closestPoint
            );

            GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, partBehaviour.PbPart, collisionInfo, collisionForce);
            break;
        }
        smokeFX.Stop();
        explodeFX.Play();
        StartCoroutine(CommonCoroutine.Delay(2f, false, () => Destroy(gameObject)));
    }

    IEnumerator CR_Launch()
    {
        float t = 0;
        while (true)
        {
            t += Time.deltaTime;
            if (t >= bulletLifeTime) Destroy(gameObject);
            currentSpeed = Mathf.Lerp(0, launchMaxSpeed, animationCurve.Evaluate(t.Remap(0, bulletLifeTime, 0, 1)));
            transform.position += currentSpeed * Time.deltaTime * PartBehaviour.GetAxis(movingDir, transform);
            yield return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (explodeFX == null) return;
        var color = Color.red;
        color.a = 0.3f;
        Gizmos.color = color;
        Gizmos.DrawSphere(explodeFX.transform.position, explodeRadius);
    }
}