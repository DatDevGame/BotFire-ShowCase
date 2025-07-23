using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using LatteGames;
using LatteGames.Template;

public class ArrowCollider : PBCollider
{
    [SerializeField] float shootForce = 5f;
    [SerializeField] float launchForce = 5f;
    [SerializeField] List<PBAxis> movingDir;
    [SerializeField] ParticleSystem hitVFX;
    [SerializeField] SoundID hitSoundID;
    [SerializeField] Collider m_Collider;
    [SerializeField] float forwardBonusOffset = 0.15f;
    [SerializeField] PBPartSkin partSkin;

    public Queue<GameObject> attachedArrowQueue;
    public PartBehaviour PartBehaviour
    {
        set
        {
            partBehaviour = value;
            partSkin.part = partBehaviour.PbPart;
        }
    }

    Vector3 localHitVFXPos;
    bool hasCollided;
    Rigidbody rb;
    Coroutine coroutine;
    Vector3 beforePos;
    Quaternion beforeRot;

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        if (hitVFX != null)
            localHitVFXPos = hitVFX.transform.localPosition;
    }

    public void Reset()
    {
        rb.isKinematic = true;
        hasCollided = false;
        m_Collider.enabled = false;
    }

    public void Launch(float bodySpeed)
    {
        m_Collider.enabled = true;
        rb.isKinematic = false;
        rb.AddForce(PartBehaviour.GetAxis(movingDir, transform) * launchForce, ForceMode.VelocityChange);
        if (coroutine != null) StopCoroutine(coroutine);

        if(gameObject.activeSelf)
            coroutine = StartCoroutine(CR_Lauching());
    }

    protected override void TriggerEnterBehaviour(Collider other)
    {
        if (hasCollided) return;
        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.attachedRigidbody?.GetComponent<IDamagable>() != null)
        {
            if (hitSoundID.value != null)
            {
                SoundManager.Instance.PlaySFX(hitSoundID, PBSoundUtility.IsOnSound() ? 0.2f : 0);
            }
            hasCollided = true;
            hitVFX.transform.position = transform.TransformPoint(localHitVFXPos);
            hitVFX.transform.SetParent(transform.parent);
            hitVFX.Play();
            if (coroutine != null) StopCoroutine(coroutine);

            if (!other.CompareTag("Wall"))
            {
                if (attachedArrowQueue == null)
                {
                    return;
                }
                var meshOnly = attachedArrowQueue.Dequeue();
                if (meshOnly == null) return;
                attachedArrowQueue.Enqueue(meshOnly);
                meshOnly.gameObject.SetActive(true);
                meshOnly.transform.rotation = beforeRot;
                meshOnly.transform.position = beforePos;
                meshOnly.transform.SetParent(other.transform);
                if (other.attachedRigidbody?.GetComponent<IDamagable>() != null)
                {
                    meshOnly.transform.position += meshOnly.transform.forward * forwardBonusOffset;
                    var closestPoint = other.ClosestPoint(transform.position);
                    var collisionInfo = new CollisionInfo
                    (
                        other.attachedRigidbody,
                        other.transform.position - transform.position,
                        closestPoint
                    );
                    GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, partBehaviour.PbPart, collisionInfo, shootForce);
                }
            }
            gameObject.SetActive(false);
        }
    }

    IEnumerator CR_Lauching()
    {
        var waitForFixedUpdate = new WaitForFixedUpdate();
        while (!hasCollided)
        {
            beforePos = transform.position;
            beforeRot = transform.rotation;
            yield return waitForFixedUpdate;
        }
    }
}