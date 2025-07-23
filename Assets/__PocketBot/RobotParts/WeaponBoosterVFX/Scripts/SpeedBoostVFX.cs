using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoostVFX : BoosterVFX
{
    [SerializeField, BoxGroup("Object")] private ParticleSystem m_WindLineStormy;
    [SerializeField, BoxGroup("Object")] private ParticleSystem m_SmokeWhiteSoft;

    private Rigidbody m_Rigidbody;

    private void Update()
    {
        if (m_Rigidbody != null)
        {
            HandleParticleSystemWind(m_Rigidbody.velocity.magnitude);
        }
    }

    public void AddRigidbodyObject(Rigidbody rigidbody) => m_Rigidbody = rigidbody;

    private void HandleParticleSystemWind(float velocity)
    {
        velocity = velocity / 2;
        velocity = Mathf.Clamp(velocity, 0, 10);
        m_SmokeWhiteSoft.startLifetime = velocity * 0.03f;
        m_SmokeWhiteSoft.startSpeed = velocity * 7;
        m_SmokeWhiteSoft.startSize = velocity * 2;

        m_WindLineStormy.startLifetime = velocity * 0.05f;
        m_WindLineStormy.startSpeed = velocity * 3;
        m_WindLineStormy.startSize = velocity * 0.02f;
    }
}
