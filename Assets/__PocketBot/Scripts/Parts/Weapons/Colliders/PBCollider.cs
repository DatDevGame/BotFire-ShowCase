using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBCollider : MonoBehaviour
{
    [SerializeField] protected PartBehaviour partBehaviour;

    public Action<CollisionData> ApplyCollisionForceCallback { get; set; }

    protected void OnCollisionEnter(Collision collision)
    {
        if (!enabled) return;
        //LGDebug.Log($"Collision Enter {collision.rigidbody} {name} {GetInstanceID()}");
        CollisionEnterBehaviour(collision);
    }

    protected void OnCollisionStay(Collision collision)
    {
        if (!enabled) return;
        //LGDebug.Log($"Collision Stay {collision.rigidbody} {name} {GetInstanceID()}");
        CollisionStayBehaviour(collision);
    }

    protected void OnCollisionExit(Collision collision)
    {
        if (!enabled) return;
        //LGDebug.Log($"Collision Exit {collision.rigidbody} {name} {GetInstanceID()}");
        CollisionExitBehaviour(collision);
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        TriggerEnterBehaviour(other);
    }

    protected void OnTriggerStay(Collider other)
    {
        if (!enabled) return;
        TriggerStayBehaviour(other);
    }

    protected void OnTriggerExit(Collider other)
    {
        if (!enabled) return;
        TriggerExitBehaviour(other);
    }

    protected virtual void CollisionEnterBehaviour(Collision collision)
    {

    }

    protected virtual void CollisionStayBehaviour(Collision collision)
    {

    }

    protected virtual void CollisionExitBehaviour(Collision collision)
    {

    }

    protected virtual void TriggerEnterBehaviour(Collider other)
    {

    }

    protected virtual void TriggerStayBehaviour(Collider other)
    {

    }

    protected virtual void TriggerExitBehaviour(Collider other)
    {

    }
}
