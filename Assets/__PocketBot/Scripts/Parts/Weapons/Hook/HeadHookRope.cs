using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class HeadHookRope : MonoBehaviour
{
    [SerializeField] private HingeJoint _hingeJoint;
    [SerializeField] private LayerMask _layerMaskEnemy;

    private Action _fireAction;
    private Action _goodFire;
    private Action _badFire;

    public void SetAction(Action fireAction, Action goodFire, Action badFire)
    {
        _fireAction = fireAction;
        _goodFire = goodFire;
        _badFire = badFire;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
            Fire();

        RaycastCheck();
    }
    private void RaycastCheck()
    {
        // Check for input or any condition that triggers the raycast
        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = new Ray(transform.position, transform.forward);
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 5, Color.red);

            float maxRaycastDistance = 5;

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxRaycastDistance, _layerMaskEnemy))
            {
                //Debug.Log("Hit object: " + hit.collider.gameObject.name);

                Vector3 hitPoint = hit.point;
                //Debug.Log("Hit point position: " + hitPoint);

                Vector3 hitNormal = hit.normal;
                //Debug.Log("Surface normal at hit point: " + hitNormal);
                Fire();
            }
            else
            {
                //Debug.Log("No object hit.");
            }
        }
    }
    private void Fire()
    {
        _fireAction?.Invoke();

        transform.DOMove(transform.forward * 10, 1f)
            .SetEase(Ease.OutQuad)
            .SetDelay(0.1f)
            .OnComplete(() =>
            {
                if (_hingeJoint.connectedBody == null)
                    _badFire?.Invoke();
                else
                    _goodFire?.Invoke();
            });
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("EnemyPart1"))
        {
            if (collision.gameObject.GetComponent<Rigidbody>() != null)
            {
                _hingeJoint.connectedBody = collision.gameObject.GetComponent<Rigidbody>();
                DOTween.KillAll();
            }

        }
    }
}
