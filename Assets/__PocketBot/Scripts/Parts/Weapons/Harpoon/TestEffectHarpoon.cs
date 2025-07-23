using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEffectHarpoon : MonoBehaviour
{
    [SerializeField] Rigidbody targetRB;
    public float pullForce; // Adjust this value to control the pulling strength
    public float maxDistance;

    void FixedUpdate()
    {
        // Check if a target Rigidbody is assigned
        if (targetRB == null)
        {
            return;
        }

        // Get the direction towards the object
        Vector3 direction = transform.position - targetRB.position;

        // Check if the object is within the maximum pull distance
        if (direction.magnitude <= maxDistance)
        {
            // Calculate the pulling force based on distance
            float pullStrength = pullForce * (1 - direction.magnitude / maxDistance);

            // Apply force in the direction towards the object
            targetRB.AddForce(direction.normalized * pullStrength, ForceMode.VelocityChange);
        }
    }
}
