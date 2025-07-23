using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private Rigidbody bulletRb;
    [SerializeField]
    private Collider bulletCollider;
    [SerializeField]
    private ParticleSystem hitParticle;

    [SerializeField]
    private Material m_PlayerMaterial;
    [SerializeField]
    private Material m_OppoentMaterial;
    [SerializeField]
    private MeshRenderer m_Meshrenderer;

    private IAttackable m_Attackable;

    private void Awake()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall"))
        {
            bulletRb.isKinematic = true;
            bulletCollider.gameObject.SetActive(false);
            Destroy(gameObject, 1f);
            return;
        }

        // PBPart part = other.GetComponent<PBPart>();
        if (other.isTrigger)
            return;
        if (other.TryGetComponent(out IDamagable damagableObject) || (other.attachedRigidbody != null && other.TryGetComponent(out damagableObject)))
        {
            damagableObject.ReceiveDamage(m_Attackable, 0f);
        }
        if (hitParticle != null)
        {
            hitParticle.transform.position = other.ClosestPoint(transform.position);
            hitParticle.Play();
        }
        bulletRb.isKinematic = true;
        bulletCollider.gameObject.SetActive(false);
        transform.Find("SmokeDarkSoftTrail").gameObject.SetActive(false);
        Destroy(gameObject, 1f);
    }

    public void Fire(Gun gun, Vector3 force)
    {
        Fire(gun.Robot.TeamId, gun.gameObject.layer, force, gun);
    }

    public void Fire(int teamId, int layer, Vector3 force, IAttackable attackable)
    {
        m_Attackable = attackable;
        m_Meshrenderer.material = teamId == 1 ? m_PlayerMaterial : m_OppoentMaterial;
        bulletRb.velocity = force;
        bulletCollider.excludeLayers |= 1 << layer;
        bulletCollider.enabled = true;
        Destroy(gameObject, 10f);
    }
}