using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
using DigitalRuby.ThunderAndLightning;
using HyrphusQ.Events;
using DG.Tweening;
using UnityEngine.AI;
using UnityEditor;
using Unity.AI.Navigation;

public class ObstacleTeslaTurret : MonoBehaviour, IAttackable
{
    [SerializeField, BoxGroup("No Tweaks")] private NavMeshModifier navMeshModifier;
    [SerializeField, BoxGroup("No Tweaks")] private NavMeshObstacle navMeshObstacle;
    [SerializeField, BoxGroup("Object")] private Transform _pointStart;
    [SerializeField, BoxGroup("Object")] private Transform _pointEnd;
    [SerializeField, BoxGroup("Object")] private SphereCollider _zoneCollider;
    [SerializeField, BoxGroup("Object")] private ParticleSystem _electricVfx;
    [SerializeField, BoxGroup("Object")] private MeshRenderer _lightningSphere;
    [SerializeField, BoxGroup("Object")] private MeshRenderer _visualRangeSphere;
    [SerializeField, BoxGroup("Object")] private Transform _visualRangeCircle;

    [SerializeField, BoxGroup("Property")] private float _damage = 50f;
    [SerializeField, BoxGroup("Property")] private float _nextAttackInSecond = 0.3f;
    [SerializeField, BoxGroup("Property")] private float _lengthZone = 5;
    [SerializeField, BoxGroup("Property"), ColorUsage(true, true)] private Color activeSphereColor;

    [SerializeField, BoxGroup("3 Looped States Electrical")] private ElectricState _firstElectricState;
    [SerializeField, BoxGroup("3 Looped States Electrical")] private float _startDelay = 0f;
    [SerializeField, BoxGroup("3 Looped States Electrical")] private float _inactiveInSecond = 2f;
    [SerializeField, BoxGroup("3 Looped States Electrical")] private float _chargeInSecond = 1f;
    [SerializeField, BoxGroup("3 Looped States Electrical")] private float _activeInSecond = 3f;

    [SerializeField, BoxGroup("Component")] private LayerMask _ignoreMask;
    [SerializeField, BoxGroup("Component")] private AudioSource _audioSourceTurret;
    [SerializeField, BoxGroup("Component")] private GameObject _vfxColliderPrefab;
    [SerializeField, BoxGroup("Component")] private LightningBoltPrefabScript _lightningBehavior;
    [SerializeField, BoxGroup("Component")] private OnTriggerCallback _triggerInZoneCallback;

    [ShowInInspector, ReadOnly, BoxGroup("Preview")] private CarPhysics _opponentAttack;
    [ShowInInspector, ReadOnly, BoxGroup("Preview")] private List<CarPhysics> _carPhysics;
    [ShowInInspector, ReadOnly, BoxGroup("Preview")] private Dictionary<string, CarPhysics> _carphysicInZoneSaveDictionary;

    private IEnumerator _checkDistanceNearestOpponentInZoneCR;
    private IEnumerator _startBehaviorCR;
    private IEnumerator _loopActiveCR;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, OnStartLevel);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnCompleteBattle);
        _visualRangeSphere.enabled = false;
        _carPhysics = new List<CarPhysics>();
        _carphysicInZoneSaveDictionary = new Dictionary<string, CarPhysics>();
        _zoneCollider.radius = _lengthZone;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, OnStartLevel);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnCompleteBattle);
        if (_checkDistanceNearestOpponentInZoneCR != null)
            StopCoroutine(_checkDistanceNearestOpponentInZoneCR);

        if (_startBehaviorCR != null)
            StopCoroutine(_startBehaviorCR);
    }

    private void OnStartLevel()
    {
        if (_loopActiveCR != null)
            StopCoroutine(_loopActiveCR);
        _loopActiveCR = OnActiveAndInActiveElectic();
        StartCoroutine(_loopActiveCR);
    }

    private void OnCompleteBattle()
    {
        if (_startBehaviorCR != null)
            StopCoroutine(_startBehaviorCR);
    }

    private void Start()
    {
        _triggerInZoneCallback.isFilterByTag = false;
        _triggerInZoneCallback.onTriggerEnter += OnTriggerEnterCallback;
        _triggerInZoneCallback.onTriggerExit += OnTriggerExitCallback;
    }

    private IEnumerator OnActiveAndInActiveElectic()
    {
        _electricVfx.Stop();
        navMeshObstacle.enabled = false;
        _lightningSphere.material.SetColor("_EmissionColor", Color.black);
        WaitForSeconds inactiveInSecond = new WaitForSeconds(_inactiveInSecond);
        WaitForSeconds chargeInSecond = new WaitForSeconds(_chargeInSecond);
        WaitForSeconds activeInSecond = new WaitForSeconds(_activeInSecond);

        yield return new WaitForSeconds(_startDelay);

        bool isFirstTimeRun = true;

        while (true)
        {
            if ((isFirstTimeRun && _firstElectricState == ElectricState.Inactive) || !isFirstTimeRun)
            {
                isFirstTimeRun = false;
                //Step Inactive
                _electricVfx.Stop();
                _lightningSphere.material.DOColor(Color.black, "_EmissionColor", AnimationDuration.TINY);
                navMeshObstacle.enabled = false;
                _visualRangeCircle.DOScale(Vector3.zero, AnimationDuration.TINY);

                if (_checkDistanceNearestOpponentInZoneCR != null)
                    StopCoroutine(_checkDistanceNearestOpponentInZoneCR);
                if (_startBehaviorCR != null)
                    StopCoroutine(_startBehaviorCR);
                yield return inactiveInSecond;
            }

            if ((isFirstTimeRun && _firstElectricState == ElectricState.Charge) || !isFirstTimeRun)
            {
                isFirstTimeRun = false;
                //Step Charge
                _lightningSphere.material.DOColor(activeSphereColor, "_EmissionColor", _chargeInSecond);
                _visualRangeCircle.DOScale(Vector3.one * _lengthZone * 2, _chargeInSecond);

                yield return chargeInSecond;
            }

            if ((isFirstTimeRun && _firstElectricState == ElectricState.Active) || !isFirstTimeRun)
            {
                isFirstTimeRun = false;
                //Step Active
                _electricVfx.Play();
                _lightningSphere.material.DOColor(activeSphereColor, "_EmissionColor", AnimationDuration.TINY);
                navMeshObstacle.enabled = true;
                _visualRangeCircle.DOScale(Vector3.one * _lengthZone * 2, AnimationDuration.TINY);

                if (_checkDistanceNearestOpponentInZoneCR != null)
                    StopCoroutine(_checkDistanceNearestOpponentInZoneCR);
                _checkDistanceNearestOpponentInZoneCR = CheckRandomOpponentInZone();
                StartCoroutine(_checkDistanceNearestOpponentInZoneCR);

                if (_startBehaviorCR != null)
                    StopCoroutine(_startBehaviorCR);
                _startBehaviorCR = StartBehavior();
                StartCoroutine(_startBehaviorCR);
                yield return activeInSecond;
            }
        }
    }

    private IEnumerator StartBehavior()
    {
        WaitForSeconds nextAttackInSecond = new WaitForSeconds(_nextAttackInSecond);
        while (true)
        {
            if (_lightningBehavior != null)
            {
                if (_opponentAttack != null)
                {
                    RaycastHit hit;
                    float lengthRay = Vector3.Distance(_pointStart.position, _pointEnd.position);
                    _pointStart.DOLookAt(_opponentAttack.transform.position, 0);
                    if (!Physics.Raycast(_pointStart.position, _pointStart.transform.forward, out hit, lengthRay, _ignoreMask, QueryTriggerInteraction.Ignore))
                    {
                        _pointEnd.position = _opponentAttack.transform.position;

                        if (_audioSourceTurret != null)
                            _audioSourceTurret.Play();

                        _lightningBehavior.CallLightningAttack();
                        DamageTargetInZone(_opponentAttack);
                    }
                }
            }
            yield return nextAttackInSecond;
        }
    }
    private IEnumerator CheckRandomOpponentInZone()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.3f);
        while (true)
        {
            if (_carPhysics.Count > 0)
            {
                System.Random rng = new System.Random();

                _carPhysics = _carPhysics.Where(v => v != null && v.enabled).ToList();
                CarPhysics target = _carPhysics.OrderBy(v => rng.Next()).FirstOrDefault();

                if (target != null && target.enabled)
                    _opponentAttack = target;
                else
                    _opponentAttack = null;
            }
            else
                _opponentAttack = null;

            yield return waitForSeconds;
        }
    }

    private void DamageTargetInZone(CarPhysics carPhysics)
    {
        if (carPhysics != null)
        {
            PBPart pBPartTarget = carPhysics.GetComponent<PBPart>();
            if (pBPartTarget == null) return;
            if (pBPartTarget.RobotChassis.Robot.Health <= 0) return;

            pBPartTarget.ReceiveDamage(this, 0);

            if (_vfxColliderPrefab != null)
            {
                var vfx = Instantiate(_vfxColliderPrefab, transform);
                vfx.transform.position = carPhysics.transform.position;
                vfx.GetComponent<ParticleSystem>().Play();
                Destroy(vfx, 1.5f);
            }
        }
    }

    private void OnTriggerEnterCallback(Collider other)
    {
        Rigidbody rigidbody = other.attachedRigidbody;
        if (rigidbody != null)
        {
            CarPhysics carPhysicsTrigger = rigidbody.GetComponent<CarPhysics>();
            if (carPhysicsTrigger != null)
            {
                if (!_carphysicInZoneSaveDictionary.ContainsKey(carPhysicsTrigger.name))
                {
                    _carphysicInZoneSaveDictionary.Add(carPhysicsTrigger.name, carPhysicsTrigger);
                    _carPhysics.Add(carPhysicsTrigger);
                    return;
                }
                _carPhysics.Add(_carphysicInZoneSaveDictionary[carPhysicsTrigger.name]);
            }
        }

    }

    private void OnTriggerExitCallback(Collider other)
    {
        Rigidbody rigidbody = other.attachedRigidbody;
        if (rigidbody != null)
        {
            CarPhysics carPhysicsTrigger = rigidbody.GetComponent<CarPhysics>();
            if (carPhysicsTrigger != null)
            {
                if (_carphysicInZoneSaveDictionary.ContainsKey(carPhysicsTrigger.name))
                {
                    _carPhysics.Remove(_carphysicInZoneSaveDictionary[carPhysicsTrigger.name]);
                    return;
                }
                _carPhysics.Remove(carPhysicsTrigger);
            }
        }
    }

    public float GetDamage()
    {
        return _damage;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _lengthZone);

        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(_pointStart.position, _pointEnd.position);
    }

    protected virtual void OnValidate()
    {
        navMeshObstacle.radius = _lengthZone;
        _visualRangeSphere.transform.localScale = Vector3.one * _lengthZone * 2;
        EditorUtility.SetDirty(this);
    }
#endif

}
