using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class AirVent : MonoBehaviour
{
    enum AirVentState
    {
        Inactive,
        Charge,
        Active
    }

    [SerializeField, BoxGroup("Object")] private ParticleSystem _windLineStormy;
    [SerializeField, BoxGroup("Object")] private ParticleSystem _smokeWhiteSoft;


    [SerializeField, BoxGroup("State")] private AirVentState _firstAirVentState;

    [SerializeField, BoxGroup("Property")] private float _startDelay = 0f;
    [SerializeField, BoxGroup("Property")] private float _inactiveInSecond = 5f;
    [SerializeField, BoxGroup("Property")] private float _chargeInSecond = 1f;
    [SerializeField, BoxGroup("Property")] private float _activeInSecond = 5f;
    [SerializeField, BoxGroup("Property")] private float _force = 100;
    [SerializeField, BoxGroup("Property")] private float _lenghtForce = 10;

    [SerializeField, BoxGroup("Component")] private BoxCollider _zoneForce;
    [SerializeField, BoxGroup("Component")] private BoxCollider _zoneForcePreview;

    [ShowInInspector, ReadOnly, BoxGroup("Preview")] private List<Rigidbody> _rigidbodysInZone;
    [ShowInInspector, ReadOnly, BoxGroup("Preview")] private Dictionary<string, CarPhysics> _carphysicInZoneSaveDictionary;

    private float _lenghtZone = 0;
    private bool _isActive = false;
    private IEnumerator _onStartBehaviorCR;

    private void Awake()
    {
        _rigidbodysInZone = new List<Rigidbody>();
        _carphysicInZoneSaveDictionary = new Dictionary<string, CarPhysics>();
    }

    private void Start()
    {
        if (_onStartBehaviorCR != null)
            StopCoroutine(_onStartBehaviorCR);
        _onStartBehaviorCR = OnStartBehavior();
        StartCoroutine(_onStartBehaviorCR);
    }

    private IEnumerator OnStartBehavior()
    {
        WaitForSeconds inactiveInSecond = new WaitForSeconds(_inactiveInSecond);
        WaitForSeconds chargeInSecond = new WaitForSeconds(_chargeInSecond);
        WaitForSeconds activeInSecond = new WaitForSeconds(_activeInSecond);

        yield return new WaitForSeconds(_startDelay);

        bool isFirstTimeRun = true;

        while (true)
        {
            if ((isFirstTimeRun && _firstAirVentState == AirVentState.Inactive) || !isFirstTimeRun)
            {
                isFirstTimeRun = false;
                bool isCompleteInactive = false;
                _isActive = false;

                DOTween.To(() => _lenghtZone, x => _lenghtZone = x, 0f, _inactiveInSecond)
               .OnUpdate(() =>
               {
                   HandleParticleSystemWind();
               })
               .OnComplete(() =>
               {
                   isCompleteInactive = true;
               });
                yield return new WaitUntil(() => isCompleteInactive);
            }

            if ((isFirstTimeRun && _firstAirVentState == AirVentState.Charge) || !isFirstTimeRun)
            {
                isFirstTimeRun = false;
                bool isCompleteCharge = false;

                DOTween.To(() => _lenghtZone, x => _lenghtZone = x, _lenghtForce / 2, _chargeInSecond)
                .OnUpdate(() =>
                {
                   HandleParticleSystemWind();
                })
                .OnComplete(() =>
                {
                    isCompleteCharge = true;
                });

                yield return new WaitUntil(() => isCompleteCharge);
            }

            if ((isFirstTimeRun && _firstAirVentState == AirVentState.Active) || !isFirstTimeRun)
            {
                isFirstTimeRun = false;
                _isActive = true;
                _windLineStormy.Play();
                _smokeWhiteSoft.Play();
                DOTween.To(() => _lenghtZone, x => _lenghtZone = x, _lenghtForce, 1)
                .OnUpdate(() =>
                {
                    HandleParticleSystemWind();
                });
                yield return activeInSecond;
            }
        }
    }

    private void HandleParticleSystemWind()
    {
        _smokeWhiteSoft.startLifetime = _lenghtZone * 0.03f;
        _smokeWhiteSoft.startSpeed = _lenghtZone * 7;
        _smokeWhiteSoft.startSize = _lenghtZone * 2;

        _windLineStormy.startLifetime = _lenghtZone * 0.05f;
        _windLineStormy.startSpeed = _lenghtZone * 3;
        _windLineStormy.startSize = _lenghtZone * 0.02f;

        Vector3 cubeSize = new Vector3(0.5f, _lenghtZone, 0.5f);
        if (_zoneForce != null)
        {
            _zoneForce.size = new Vector3(2.5f, cubeSize.y, 2.5f);
            _zoneForce.center = new Vector3(0, _lenghtZone / 2, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CarPhysics carPhysicsInZone = GetCarPhysics(other);
        if (carPhysicsInZone != null)
        {
            if (!_carphysicInZoneSaveDictionary.ContainsKey(carPhysicsInZone.name))
            {
                Rigidbody rigTarget_1 = carPhysicsInZone.CarRb;
                if (rigTarget_1 != null)
                    _rigidbodysInZone.Add(rigTarget_1);

                _carphysicInZoneSaveDictionary.Add(carPhysicsInZone.name, carPhysicsInZone);
                return;
            }
            Rigidbody rigTarget_2 = _carphysicInZoneSaveDictionary[carPhysicsInZone.name].GetComponent<Rigidbody>();
            _rigidbodysInZone.Add(rigTarget_2);
        }

    }

    private void OnTriggerExit(Collider other)
    {
        CarPhysics carPhysicsInZone = GetCarPhysics(other);
        if (carPhysicsInZone != null)
        {
            if (_carphysicInZoneSaveDictionary.ContainsKey(carPhysicsInZone.name))
                _rigidbodysInZone.Remove(_carphysicInZoneSaveDictionary[carPhysicsInZone.name].GetComponent<Rigidbody>());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_isActive) return;
        for (int i = 0; i < _rigidbodysInZone.Count; i++)
        { 
            Vector3 randomDic = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f));
            _rigidbodysInZone[i].AddForce((transform.up + randomDic) * _force * Time.deltaTime, ForceMode.Impulse);
        }

    }

    private CarPhysics GetCarPhysics(Collider other)
    {
        CarPhysics carPhysicsTrigger = other.GetComponentInParent<CarPhysics>();
        if (carPhysicsTrigger == null)
            carPhysicsTrigger = other.GetComponent<CarPhysics>();
        return carPhysicsTrigger;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 cubeSize = new Vector3(0.5f, _lenghtForce, 0.5f);
        if (_zoneForcePreview != null)
        {
            _zoneForcePreview.size = new Vector3(_zoneForce.size.x, cubeSize.y, _zoneForce.size.z);
            _zoneForcePreview.center = new Vector3(0, _lenghtForce / 2, 0);
        }
        _lenghtForce = cubeSize.y;
    }
#endif
}
