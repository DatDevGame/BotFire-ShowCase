using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using DG.Tweening;
using UnityEngine.AI;
using DigitalRuby.ThunderAndLightning;
using HyrphusQ.Events;

public class TeslaTuretBehavior : PartBehaviour
{
    [SerializeField, BoxGroup("Object")] private Transform _pointStart;
    [SerializeField, BoxGroup("Object")] private Transform _pointEnd;
    [SerializeField, BoxGroup("Object")] private SphereCollider _zoneCollider;
    [SerializeField, BoxGroup("Object")] private ParticleSystem _electricVfx;
    [SerializeField, BoxGroup("Object")] private MeshRenderer _lightningSphere;

    [SerializeField, BoxGroup("Property")] private float _nextAttackInSecond = 0.3f;
    [SerializeField, BoxGroup("Property")] private float _lengthZone = 5;
    [SerializeField, BoxGroup("Property")] private float _timeElectricExist = 3;
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

    [ShowInInspector, ReadOnly, BoxGroup("Preview")] private PBPart _pbpartAttack;
    [ShowInInspector, ReadOnly, BoxGroup("Preview")] private List<PBPart> _pbparts;
    [ShowInInspector, ReadOnly, BoxGroup("Preview")] private Dictionary<string, PBPart> _pbPartInZoneSaveDictionary;

    private IEnumerator _checkDistanceNearestOpponentInZoneCR;
    private IEnumerator _startBehaviorCR;
    private IEnumerator _checkTimeExistVfxCR;
    private IEnumerator _loopActiveCR;

    private float _timeElectricExistDelta = 0;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnCompleteBattle);
        _pbparts = new List<PBPart>();
        _pbPartInZoneSaveDictionary = new Dictionary<string, PBPart>();
        _zoneCollider.radius = _lengthZone;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnCompleteBattle);
        if (_checkDistanceNearestOpponentInZoneCR != null)
            StopCoroutine(_checkDistanceNearestOpponentInZoneCR);

        if (_startBehaviorCR != null)
            StopCoroutine(_startBehaviorCR);

        if (_checkTimeExistVfxCR != null)
            StopCoroutine(_checkTimeExistVfxCR);
    }

    private void OnCompleteBattle()
    {
        if (_startBehaviorCR != null)
            StopCoroutine(_startBehaviorCR);
    }

    private void Start()
    {
        if (_loopActiveCR != null)
            StopCoroutine(_loopActiveCR);
        _loopActiveCR = OnActiveAndInActiveElectic();
        StartCoroutine(_loopActiveCR);

        // if (_checkTimeExistVfxCR != null)
        //     StopCoroutine(_checkTimeExistVfxCR);
        // _checkTimeExistVfxCR = CheckTimeElectric();
        // StartCoroutine(_checkTimeExistVfxCR);

        _triggerInZoneCallback.isFilterByTag = false;
        _triggerInZoneCallback.onTriggerEnter += OnTriggerEnterCallback;
        _triggerInZoneCallback.onTriggerExit += OnTriggerExitCallback;
    }

    private IEnumerator OnActiveAndInActiveElectic()
    {
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

                yield return chargeInSecond;
            }

            if ((isFirstTimeRun && _firstElectricState == ElectricState.Active) || !isFirstTimeRun)
            {
                isFirstTimeRun = false;
                //Step Active
                _electricVfx.Play();

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
                if (_pbpartAttack != null)
                {
                    RaycastHit hit;
                    _pointStart.DOLookAt(_pbpartAttack.transform.position, 0);
                    float lengthRay = Vector3.Distance(_pointStart.position, _pointEnd.position);
                    if (!Physics.Raycast(_pointStart.position, _pointStart.forward, out hit, lengthRay, _ignoreMask, QueryTriggerInteraction.Ignore))
                    {
                        _pointEnd.position = _pbpartAttack.transform.position;

                        if (_audioSourceTurret != null)
                            _audioSourceTurret.Play();

                        _lightningBehavior.CallLightningAttack();
                        DamageTargetInZone(_pbpartAttack);

                        _timeElectricExistDelta = _timeElectricExist;
                    }
                }
            }
            yield return nextAttackInSecond;
        }
    }

    private IEnumerator CheckTimeElectric()
    {
        float timeMinus = 0.2f;
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);
        while (true)
        {
            _timeElectricExistDelta -= timeMinus;
            if (_timeElectricExistDelta <= 0)
            {
                if (_electricVfx.maxParticles != 1)
                    _electricVfx.maxParticles = 1;
            }
            else
            {
                if (_electricVfx.maxParticles != 5)
                    _electricVfx.maxParticles = 5;

            }
            yield return waitForSeconds;
        }
    }

    private void DamageTargetInZone(PBPart pbPart)
    {
        if (pbPart != null)
        {
            if (pbPart.GetComponent<Rigidbody>() == null) return;
            if (pbPart.GetComponent<IDamagable>() == null) return;

            var collisionInfo = new CollisionInfo
              (
                    pbPart.GetComponent<Rigidbody>(),
                    Vector3.zero,
                    pbPart.GetComponent<Collider>().ClosestPoint(transform.position)
                );

            GameEventHandler.Invoke(CollisionEventCode.OnPartCollide, PbPart, collisionInfo, 0);


            if (_vfxColliderPrefab != null)
            {
                var vfx = Instantiate(_vfxColliderPrefab, transform);
                vfx.transform.position = pbPart.transform.position;
                vfx.GetComponent<ParticleSystem>().Play();
                Destroy(vfx, 1.5f);
            }
        }
    }

    private IEnumerator CheckRandomOpponentInZone()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.3f);
        while (true)
        {
            if (_pbparts.Count > 0)
            {
                System.Random rng = new System.Random();

                _pbparts = _pbparts.Where(v => v != null && v.RobotChassis.Robot.Health > 0).ToList();
                PBPart target = _pbparts.OrderBy(v => rng.Next()).FirstOrDefault();

                if (target != null && target.enabled)
                    _pbpartAttack = target;
                else
                    _pbpartAttack = null;
            }
            else
                _pbpartAttack = null;

            yield return waitForSeconds;
        }
    }

    private void OnTriggerEnterCallback(Collider other)
    {
        PBPart pbpartTrigger = GetPBPart(other);
        if (pbpartTrigger != null)
        {
            var carPhysics = GetCarPhysics(other);
            if (!_pbPartInZoneSaveDictionary.ContainsKey(carPhysics.name))
            {
                _pbPartInZoneSaveDictionary.Add(carPhysics.name, pbpartTrigger);
                _pbparts.Add(pbpartTrigger);
                return;
            }
            _pbparts.Add(_pbPartInZoneSaveDictionary[carPhysics.name]);
        }
    }

    private void OnTriggerExitCallback(Collider other)
    {
        PBPart pbpartTrigger = GetPBPart(other);
        if (pbpartTrigger != null)
        {
            var carPhysics = GetCarPhysics(other);
            if (_pbPartInZoneSaveDictionary.ContainsKey(carPhysics.name))
            {
                _pbparts.Remove(_pbPartInZoneSaveDictionary[carPhysics.name]);
                return;
            }
            _pbparts.Remove(pbpartTrigger);
        }
    }
    private PBPart GetPBPart(Collider other)
    {
        PBPart pBPartTrigger = other.GetComponent<PBPart>();
        return pBPartTrigger;
    }

    private CarPhysics GetCarPhysics(Collider other)
    {
        return other.attachedRigidbody.GetComponent<CarPhysics>();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _lengthZone);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(_pointStart.position, _pointEnd.position);
    }
#endif
}
