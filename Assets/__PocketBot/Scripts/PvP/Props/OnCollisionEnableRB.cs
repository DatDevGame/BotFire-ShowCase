using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using HyrphusQ.Events;
using Unity.VisualScripting;
using System;

public class OnCollisionEnableRB : MonoBehaviour, IDamagable
{
    public Action OnGravity;
    [SerializeField] private Rigidbody m_Rigidbody;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(CollisionEventCode.OnPartCollide, HandlePartCollide);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(CollisionEventCode.OnPartCollide, HandlePartCollide);
    }

    private void HandlePartCollide(params object[] parameters)
    {
        if (parameters.Length <= 0) return;

        PBPart pBPart = null;
        Collision collision = null;
        Collider collider = null;
        CollisionInfo collisionInfo = null;

        if (parameters[0] is PBPart)
            pBPart = parameters[0] as PBPart;

        if (parameters[1] is Collision)
            collision = parameters[1] as Collision;

        if (parameters[1] is Collider)
            collider = parameters[1] as Collider;

        if (parameters[1] is CollisionInfo)
            collisionInfo = parameters[1] as CollisionInfo;


        if (pBPart == null) return;

        if (collisionInfo != null)
        {
            if (collisionInfo.body.gameObject == this.gameObject)
                ReceiveDamage(pBPart.GetComponent<IAttackable>(), Const.FloatValue.ZeroF);
        }
        if (collision != null)
        {
            if (collision.gameObject == this.gameObject)
                ReceiveDamage(pBPart.GetComponent<IAttackable>(), Const.FloatValue.ZeroF);
        }
        if (collider != null)
        {
            if (collider.gameObject == this.gameObject)
                ReceiveDamage(pBPart.GetComponent<IAttackable>(), Const.FloatValue.ZeroF);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnGravityHandle();
    }

    [Button]
    private void Load()
    {
        m_Rigidbody = gameObject.GetComponent<Rigidbody>();
    }
    private void OnGravityHandle()
    {
        m_Rigidbody.useGravity = true;

        transform
            .DOScale(Vector3.zero, 0.5f)
            .SetDelay(2f)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });

        OnGravity?.Invoke();
    }

    public void ReceiveDamage(IAttackable attacker, float forceTaken)
    {
        OnGravityHandle();
    }
}
