using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    public float speed = 30.0f; // Rotation speed (degrees per second)
    public Vector3 rotationAxis = Vector3.up; // Axis of rotation (default: up)

    void Update()
    {
        transform.Rotate(rotationAxis * Time.unscaledDeltaTime * speed, Space.Self);
    }
}
