using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerToLockCarPhysics : MonoBehaviour
{
    CarPhysics carPhysics;
    private void OnTriggerEnter(Collider other)
    {
        carPhysics = other.GetComponent<CarPhysics>();
        if (carPhysics != null)
        {
            carPhysics.enabled = false;
            GetComponent<BoxCollider>().enabled = false;
        }
    }

    public void UnlockCarPhysics()
    {
        if (carPhysics != null)
        {
            carPhysics.enabled = true;
        }
    }
}
