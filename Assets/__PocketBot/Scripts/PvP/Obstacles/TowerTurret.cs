using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using DG.Tweening;
using UnityEditor;

public class TowerTurret : MonoBehaviour
{
    [SerializeField, BoxGroup("Property")] private float _timeNextNewBehaviorSecond = 3;
    [SerializeField, BoxGroup("Property")] private float _timeNextPointSecond = 1;
    [SerializeField, BoxGroup("Property"), Title("Fire Action")] private float _bulletsPerShot = 3;
    [SerializeField, BoxGroup("Property")] private float _timePerShot = 0.5f;
    [SerializeField, BoxGroup("Property")] private bool _isFireThrow = true;
    [SerializeField, BoxGroup("Property")] private bool _isFireRandom = true;

    [SerializeField, BoxGroup("Object Turret")] private Transform _pointFire;
    [SerializeField, BoxGroup("Object Turret")] private Transform _axisGunBody;
    [SerializeField, BoxGroup("Object Turret")] private Transform _axisGunHead;
    [SerializeField, BoxGroup("Object Turret")] private Transform _targetPointHolder;
    [SerializeField, BoxGroup("Object Turret")] private GameObject _crossHairPrefab;
    [SerializeField, BoxGroup("Object Turret")] private Material _instanceMaterial;
    [SerializeField, PropertyOrder(1), BoxGroup("Fire The Path")] private List<Transform> _targetPoints;

    [SerializeField, BoxGroup("Object Turret")] private GameObject _projectTilePrefab;

    private bool _isNextBehavior = true;
    private void Awake()
    {
        _targetPoints.ForEach((v) =>
        {
            v.GetComponent<MeshRenderer>().material.DOFade(0, 0);
        });
    }

    private void Start()
    {
        StartCoroutine(StartTurret());
    }

    private IEnumerator StartTurret()
    {
        float timeNextBehavior = _isFireRandom ? 0 : _timeNextNewBehaviorSecond;
        WaitForSeconds nextStep = new WaitForSeconds(timeNextBehavior);
        while (true)
        {
            for (int i = 0; i < _targetPoints.Count; i++)
            {
                _isNextBehavior = false;
                if (_isFireRandom)
                {
                    Transform targetRandom = _targetPoints[Random.Range(0, _targetPoints.Count)];
                    Material crossHairFade = targetRandom.GetComponent<MeshRenderer>().material;
                    crossHairFade
                        .DOFade(1, _timeNextPointSecond)
                        .OnComplete(() =>
                        {
                            StartFireBehaviorTurret(targetRandom.position, crossHairFade);
                        });
                }
                else
                {
                    Material crossHairFade = _targetPoints[i].GetComponent<MeshRenderer>().material;
                    crossHairFade
                        .DOFade(1, _timeNextPointSecond)
                        .OnComplete(() =>
                        {
                            StartFireBehaviorTurret(_targetPoints[i].position, crossHairFade);
                        });
                }
                yield return new WaitUntil(() => _isNextBehavior);
            }
            yield return nextStep;
        }
    }
    private void StartFireBehaviorTurret(Vector3 areaFire, Material crossHairFade)
    {
        if(areaFire != Vector3.zero)
        {
            if (_isFireThrow)
            {
                _axisGunBody
                    .DOLookAt(areaFire, 0.5f);

                float distanceTarget = Vector3.Distance(_pointFire.position, areaFire);
                _axisGunHead.DOLocalRotate(new Vector3(-distanceTarget * 3, 0, 0), 1)
                .OnComplete(() =>
                {
                    StartCoroutine(Fire(areaFire, crossHairFade));
                });

                return;
            }

            _axisGunBody
                .DOLookAt(areaFire, 1)
                .OnComplete(() =>
                {
                    StartCoroutine(Fire(areaFire, crossHairFade));
                });
        }
    }
    private IEnumerator Fire(Vector3 areaFire, Material crossHairFade)
    {
        WaitForSeconds waitingTime = new WaitForSeconds(0.2f);
        WaitForSeconds timeEveryFireSecond = new WaitForSeconds(_timePerShot);

        yield return waitingTime;

        for (int i = 0; i < _bulletsPerShot; i++)
        {
            var projectile = Instantiate(_projectTilePrefab);

            int layerIndex = gameObject.layer;
            string layerName = LayerMask.LayerToName(layerIndex);
            projectile.layer = LayerMask.NameToLayer(layerName);

            projectile.transform.position = _pointFire.position;
            projectile.transform.eulerAngles = _pointFire.eulerAngles;

            ProjectileTurret projectileTurret = projectile.GetComponent<ProjectileTurret>();
            if (_isFireThrow)
                projectileTurret.FireThrow(areaFire);
            else
                projectileTurret.FireFoward(areaFire);

            yield return timeEveryFireSecond;
        }
        crossHairFade.DOFade(0, 0f);
        _isNextBehavior = true;
    }

#if UNITY_EDITOR
    [Button("Add Point", ButtonSizes.Medium), PropertyOrder(0), BoxGroup("Fire The Path")]
    private void AddPoint()
    {
        GameObject test = PrefabUtility.InstantiatePrefab(_crossHairPrefab, _targetPointHolder.transform) as UnityEngine.GameObject;
        Transform point = test.transform;
        point.transform.position = new Vector3(point.transform.position.x, point.transform.position.y, point.transform.position.z + _targetPoints.Count + 1);
        point.name = $"Point - {_targetPoints.Count + 1}";

        if (point != null)
            _targetPoints.Add(point.transform);
    }
    [Button("Remove Point", ButtonSizes.Medium), PropertyOrder(0), BoxGroup("Fire The Path")]
    private void RemovePoint()
    {
        var point = _targetPoints.LastOrDefault();
        if (point != null)
        {
            for (int i = 0; i < _targetPointHolder.childCount; i++)
            {
                if(_targetPointHolder.GetChild(i) == point.transform)
                {
                    _targetPoints.Remove(point.transform);
                    DestroyImmediate(_targetPointHolder.GetChild(i).gameObject);
                }
            }
        }

    }
    private void OnDrawGizmosSelected()
    {
        if (_targetPoints.Count > 0)
        {
            for (int i = 0; i < _targetPoints.Count; i++)
            {
                if (_targetPoints[i] != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(_targetPoints[i].position, _pointFire.position);
                }
            }
        }
    }

#endif
}
