using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Linq;

public class PortalLink : MonoBehaviour
{
    public PortalLink LinkedPortal;
    [SerializeField] private PortalGate m_PortalGate;

    [ShowInInspector] private Dictionary<ParticleSystem, bool> m_TeleportVFXs = new Dictionary<ParticleSystem, bool>();
    private void OnTriggerEnter(Collider other)
    {
        PBPart part = other.GetComponent<PBPart>();
        if (part != null && LinkedPortal != null && part.RobotChassis != null)
        {
            if (m_PortalGate.IsAceptTeleport(part.RobotChassis))
            {
                StartCoroutine(TeleportSmoothly(part.RobotChassis));
            }
        }
    }

    private IEnumerator TeleportSmoothly(PBChassis chassis)
    {
        ParticleSystem teleParticle = null;
        List<ParticleSystem> adjustVFX = new List<ParticleSystem>();

        Rigidbody rb = chassis.RobotBaseBody;
        if (rb != null)
        {
            m_PortalGate.DisableCar(chassis);

            if (m_TeleportVFXs == null || m_TeleportVFXs.Count <= 0)
            {
                teleParticle = Instantiate(m_PortalGate.TeleVFX, transform);
                m_TeleportVFXs.Add(teleParticle, false);
                adjustVFX = teleParticle.GetComponentsInChildren<ParticleSystem>().ToList();
            }
            else
            {
                for (int i = 0; i < m_TeleportVFXs.Count; i++)
                {
                    if (!m_TeleportVFXs.ElementAt(i).Value)
                    {
                        teleParticle = m_TeleportVFXs.ElementAt(i).Key;
                    }
                }
                if (teleParticle == null)
                {
                    teleParticle = Instantiate(m_PortalGate.TeleVFX, transform);
                    m_TeleportVFXs.Add(teleParticle, false);
                    adjustVFX = teleParticle.GetComponentsInChildren<ParticleSystem>().ToList();
                }
            }

            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            chassis.CarPhysics.transform.position = transform.position;
            m_TeleportVFXs[teleParticle] = true;

            adjustVFX.ForEach(v => { v.loop = true; });
            teleParticle.Play();

            chassis.CarPhysics.transform.DOMove(LinkedPortal.transform.position, 0.5f)
                .SetEase(Ease.InOutFlash)
                .OnUpdate(() => 
                {
                    teleParticle.transform.position = chassis.CarPhysics.transform.position;
                })
                .OnComplete(() => 
                {
                    rb.isKinematic = false;
                    m_PortalGate.EnableCard(chassis);
                    adjustVFX.ForEach(v => { v.loop = false;});
                    m_TeleportVFXs[teleParticle] = false;
                });

            yield return null;
        }
    }
}
