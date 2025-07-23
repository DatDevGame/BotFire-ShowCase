using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ScaffoldBreakHandle : MonoBehaviour, IDamagable
{
    [SerializeField, BoxGroup("Ref")] private GameObject m_HidenPlane;
    [SerializeField, BoxGroup("Ref")] private GameObject m_ParrentDebris;
    [SerializeField, BoxGroup("Ref")] private Collider m_ColliderStaffold;
    [SerializeField, BoxGroup("Ref")] private NavMeshObstacle m_NavMeshObstacle;
    [SerializeField, BoxGroup("Ref")] private GameObject m_ColliderDefault;
    [SerializeField, BoxGroup("Ref")] private List<Renderer> m_ScaffoldDefaults;
    [SerializeField, BoxGroup("Ref")] private List<Rigidbody> m_DebrisRigidbodys;
    [SerializeField, BoxGroup("Settings")] private float m_ExplosionForce = 20f;
    [SerializeField, BoxGroup("Settings")] private float m_ExplosionRadius = 10f;
    [SerializeField, BoxGroup("Settings")] private float m_UpwardModifier = 2f;

    private void Break(Transform pointEx)
    {
        m_ColliderStaffold.enabled = false;
        m_HidenPlane.SetActive(false);
        m_ParrentDebris.SetActive(true);
        if(m_NavMeshObstacle != null)
            m_NavMeshObstacle.enabled = false;
        m_ScaffoldDefaults.ForEach(v => v.enabled = false);
        Destroy(m_ColliderDefault);

        foreach (var rb in m_DebrisRigidbodys)
        {
            rb.useGravity = true;
            rb.isKinematic = false;

            Vector3 explosionCenter = pointEx.position;
            rb.AddExplosionForce(m_ExplosionForce, explosionCenter, m_ExplosionRadius, m_UpwardModifier, ForceMode.Impulse);
            rb.transform
                .DOScale(Vector3.zero, 1f)
                .SetDelay(Random.Range(3, 4))
                .OnComplete(() => 
                {
                    Destroy(rb.gameObject);
                });
        }
    }

    public void ReceiveDamage(IAttackable attacker, float forceTaken)
    {
        if (attacker is IExplodable)
        {
            if (attacker is Component component)
            {
                Break(component.transform);
            }
        }
    }

}
