using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
using HyrphusQ.Events;
using System.IO;
using UnityEditor;

public class TurretBehavior : PartBehaviour
{
    [SerializeField, BoxGroup("Property")] private LayerMask _layerMask;
    [SerializeField, BoxGroup("Property")] private float _timeNextNewBehaviorSecond = 3;
    [SerializeField, BoxGroup("Property")] private float _timeNextPointSecond = 1;
    [SerializeField, BoxGroup("Property")] private float _forceForward = 12;
    [SerializeField, BoxGroup("Property"), Title("Fire Action")] private int _quantityBulletShot = 3;
    [SerializeField, BoxGroup("Property")] private float _bulletsPerShot = 3;
    [SerializeField, BoxGroup("Property")] private float _timePerShot = 0.5f;
    [SerializeField, BoxGroup("Property")] private float _jumpPower = 5;
    [SerializeField, BoxGroup("Property")] private float _timeDurationJump = 0.5f;

    [SerializeField, BoxGroup("Object Turret")] private Transform _pointFire;
    [SerializeField, BoxGroup("Object Turret")] private Transform _targetPointHolder;
    [SerializeField, BoxGroup("Object Turret")] private GameObject _crossHairPrefab;
    [SerializeField, BoxGroup("Object Turret")] private Material _instanceMaterial;
    [SerializeField, BoxGroup("Object Turret")] private GameObject _projectTilePrefab;
    [SerializeField, BoxGroup("Object Turret")] private GameObject _pointBase;
    [SerializeField, BoxGroup("Object Turret")] private List<Transform> _points;

    private bool _isNextBehavior = true;
    private IEnumerator _onStartBehaviorCR;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, OnStartBehavior);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelEnded, OnEndBehavior);
    }
    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, OnStartBehavior);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelEnded, OnEndBehavior);

        if (_onStartBehaviorCR != null)
            StopCoroutine(_onStartBehaviorCR);
    }

    private void OnStartBehavior()
    {
        if (_onStartBehaviorCR != null)
            StopCoroutine(_onStartBehaviorCR);
        _onStartBehaviorCR = StartTurret();
        StartCoroutine(_onStartBehaviorCR);
    }
    private void OnEndBehavior()
    {
        if (_onStartBehaviorCR != null)
            StopCoroutine(_onStartBehaviorCR);
    }

    private IEnumerator StartTurret()
    {
        WaitForSeconds nextStep = new WaitForSeconds(_timeNextNewBehaviorSecond);
        while (true)
        {
            for (int i = 0; i < _quantityBulletShot; i++)
            {
                if (_points[i] != null)
                {
                    _isNextBehavior = false;
                    StartFireBehaviorTurret(_points[i]);
                    yield return new WaitUntil(() => _isNextBehavior);
                }
            }
            yield return nextStep;
        }
    }
    private void StartFireBehaviorTurret(Transform attackPoint)
    {
        StartCoroutine(Fire(attackPoint));
    }
    private IEnumerator Fire(Transform attackPoint)
    {
        WaitForSeconds waitingTime = new WaitForSeconds(0.2f);
        WaitForSeconds timeEveryFireSecond = new WaitForSeconds(_timePerShot);

        yield return waitingTime;

        for (int i = 0; i < _bulletsPerShot; i++)
        {
            var projectile = Instantiate(_projectTilePrefab, pbPart.RobotChassis.transform);

            int layerIndex = gameObject.layer;
            string layerName = LayerMask.LayerToName(layerIndex);
            projectile.layer = LayerMask.NameToLayer(layerName);

            projectile.transform.position = _pointFire.position;
            projectile.transform.eulerAngles = _pointFire.eulerAngles;

            GameObject pointEnd = null;
            RaycastHit hit;
            if (Physics.Raycast(attackPoint.position, -attackPoint.transform.up, out hit, 10, _layerMask, QueryTriggerInteraction.Ignore))
            {
                pointEnd = Instantiate(_pointBase, pbPart.RobotChassis.transform);
                pointEnd.transform.position = new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z);
                pointEnd.transform.eulerAngles = attackPoint.eulerAngles;
                pointEnd.SetActive(true);
                Destroy(pointEnd, 2);
            }

            if (pointEnd != null)
            {
                TurretProjectileBehaviorBase turretProjectileBehavior = projectile.GetComponent<TurretProjectileBehaviorBase>();
                turretProjectileBehavior.Fire(this, pointEnd.transform.position, _jumpPower, _timeDurationJump);
            }
            yield return timeEveryFireSecond;
        }
        _isNextBehavior = true;
    }


    [BoxGroup("Editor")] public Mesh AtomicMesh;
    [BoxGroup("Editor")] public Quaternion QuaternionAtomic;
    [BoxGroup("Editor")] public List<Color> ColorsEditor;

    [BoxGroup("Editor")] public float peakPositionFloat = 10;
    [BoxGroup("Editor")] public float peakPositionFloat_2 = 2;
    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < _points.Count; i++)
        {
            if (i == 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(_points[i].position, _pointFire.position);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(_points[i].position, _points[i - 1].position);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_points[i].position, 0.3f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(_points[i].position, -_points[i].transform.up * _points[i].position.y);

            Gizmos.color = Color.blue;
            Gizmos.DrawCube(new Vector3(_points[i].position.x, 0, _points[i].position.z) , new Vector3(1, 0.2f, 1));

            Gizmos.color = Color.red;
            if (AtomicMesh != null)
                Gizmos.DrawMesh(AtomicMesh, new Vector3(_points[i].position.x, _points[i].position.y / 2, _points[i].position.z), QuaternionAtomic);
        }
    }
}
