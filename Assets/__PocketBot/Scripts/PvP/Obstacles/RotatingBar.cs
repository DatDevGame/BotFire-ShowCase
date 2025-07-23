using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class RotatingBar : MonoBehaviour
{
    [SerializeField, BoxGroup("Tweaks")]
    private Vector3 rotateSpeedAngles = Vector3.up * 90f;
    [SerializeField, BoxGroup("No Tweaks")]
    private Rigidbody rb;

    private Collider[] m_Colliders;
    private Collider[] m_AttachedColliders;

    private void Start()
    {
        rb.maxAngularVelocity = rotateSpeedAngles.magnitude;
        m_Colliders = transform.Find("Model").gameObject.GetComponentsInChildren<Collider>();
        m_AttachedColliders = gameObject.GetComponentsInChildren<Collider>().ToList().Except(m_Colliders).ToArray();
        foreach (var collider in m_Colliders)
        {
            foreach (var attachedCollider in m_AttachedColliders)
            {
                Physics.IgnoreCollision(collider, attachedCollider);
            }
        }
    }

    private void FixedUpdate()
    {
        rb.AddTorque(rotateSpeedAngles, ForceMode.Acceleration);
    }
}