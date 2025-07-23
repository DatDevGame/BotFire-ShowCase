using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockRotationBooster : MonoBehaviour
{
    public bool IsLock;
    private Quaternion lockedRotation;
    void Start()
    {
        lockedRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if(IsLock)
            transform.rotation = lockedRotation;
    }
}
