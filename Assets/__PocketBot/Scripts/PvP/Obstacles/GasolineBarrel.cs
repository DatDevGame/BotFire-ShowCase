using System.Collections;
using System.Collections.Generic;
using System.Linq;

using HyrphusQ.Events;

using Sirenix.OdinInspector;

using UnityEngine;

public class GasolineBarrel : MonoBehaviour, IAttackable, IBreakObstacle, IDamagable, IExplodable
{
    [SerializeField, LabelText("Break"), BoxGroup("Breakable")] private bool m_IsCollisionBreak;
    [SerializeField, LabelText("Current HP"), ReadOnly, BoxGroup("Breakable")] private float _healPoint;
    [SerializeField, LabelText("Max HP"), BoxGroup("Breakable")] private float _maxHealPoint = 100;
    [SerializeField, LabelText("Life Time"), BoxGroup("Breakable")] private float _lifeTime = 2;
    [SerializeField, BoxGroup("Breakable")] private Collider _mainCollider;
    [SerializeField, BoxGroup("Breakable")] private Rigidbody _mainrig;
    [SerializeField, BoxGroup("Breakable")] private MeshRenderer _mainMeshObject;
    [SerializeField, BoxGroup("Breakable")] private GameObject _objectBreak;
    [SerializeField, BoxGroup("Breakable")] private GameObject _healbarObstaclePrefab;
    [SerializeField, BoxGroup("Breakable")] private float _splinterForce = 1000;
    [SerializeField, BoxGroup("Breakable")] private float _breakForceMagnitudeThreshold = 10;

    [SerializeField, BoxGroup("Gravity")] private bool m_IsActiveGravity = false;
    [SerializeField, BoxGroup("Gravity")] private float m_FallSpeed = 1;

    [SerializeField, BoxGroup("Explode")] private float m_ZoneExplode = 3;
    [SerializeField, BoxGroup("Explode")] private float _damage = 100;
    [SerializeField, BoxGroup("Explode")] private float _force = 1000;
    [SerializeField, BoxGroup("Explode")] private float _stunCarPhysicsDuration = 0.1f;
    [SerializeField, BoxGroup("Explode")] private ParticleSystem _explosionEffect;

    [SerializeField, BoxGroup("DoT")] private GameObject fireDoTField;
    [SerializeField, BoxGroup("DoT")] private ParticleSystem fireDoTFieldVFX;
    [SerializeField, BoxGroup("DoT")] private float _causeDamageSpeedPerSec = 0.2f;
    [SerializeField, BoxGroup("DoT")] private float _damagePerWave = 6f;
    [SerializeField, BoxGroup("DoT")] private float _duration = 3f;

    private List<Rigidbody> _splinters;
    private HealthBarObstacle _healthBarObstacle;
    private Dictionary<PBChassis, int> m_ChassicDamage = new();
    private Dictionary<IDamagable, int> m_ObjectIDamage = new();
    private List<PBPart> _pbPartsTrigger = new();
    private bool hasExploded;
    private bool hasDestroyedAllSplinters;
    private bool hasDestroyedFireDoTField;

    private void Awake()
    {
        _splinters = _objectBreak.GetComponentsInChildren<Rigidbody>(true).ToList();
        GameEventHandler.AddActionEvent(CollisionEventCode.OnPartCollide, HandlePartCollide);

        _healPoint = _maxHealPoint;

        if (_healbarObstaclePrefab != null)
        {
            _healthBarObstacle = Instantiate(_healbarObstaclePrefab, transform).GetComponentInChildren<HealthBarObstacle>();
            if (_healthBarObstacle != null)
                _healthBarObstacle.SetUp(_objectBreak.transform);
        }
    }
    private void Update()
    {
        if (m_IsActiveGravity)
        {
            _mainrig.AddForce(Vector3.down * m_FallSpeed, ForceMode.Acceleration);
        }
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(CollisionEventCode.OnPartCollide, HandlePartCollide);
    }
    public void Explode()
    {
        _explosionEffect.gameObject.SetActive(true);
        _explosionEffect.transform.rotation = Quaternion.identity;
        _explosionEffect.Play();

        // Damage logic (e.g., apply damage to nearby objects)
        Collider[] affectedObjects = Physics.OverlapSphere(transform.position, m_ZoneExplode); // Explosion radius
        foreach (var obj in affectedObjects)
        {
            if (obj.TryGetComponent(out PBPart pbPart))
            {
                if (pbPart != null && pbPart.RobotChassis != null && !m_ChassicDamage.ContainsKey(pbPart.RobotChassis))
                {
                    m_ChassicDamage.Add(pbPart.RobotChassis, 1);
                    pbPart.RobotChassis.ReceiveDamage(this, Const.FloatValue.ZeroF);
                    ForceObject(pbPart.RobotChassis);
                }
            }
            else if (obj.TryGetComponent(out IDamagable damagable))
            {
                if (damagable is IExplodable)
                    continue;

                if (!m_ObjectIDamage.ContainsKey(damagable))
                {
                    m_ObjectIDamage.Add(damagable, 1);
                    damagable.ReceiveDamage(this, _damage);
                }
            }
        }
        ForceSplinter();
        hasExploded = true;
    }


    private void ForceSplinter()
    {
        foreach (var item in _splinters)
        {
            Vector3 directionToPart = item.transform.position - transform.position;

            // Apply force in the direction away from the object
            item.AddForce((directionToPart.normalized + Vector3.up * 0.5f) * _splinterForce, ForceMode.VelocityChange);
        }
    }

    private void ForceObject(PBPart part)
    {
        Rigidbody rigidbodyTarget = part.RobotBaseBody;
        if (rigidbodyTarget == null) return;
        var carPhysic = part.RobotChassis.CarPhysics;
        StartCoroutine(CR_DisableCarPhysics(carPhysic));
        // Add force if the part's health is above the damage threshold
        if (part != null && part.RobotChassis != null && part.RobotChassis.Robot.Health > _damage)
        {
            // Calculate direction from this object to the part
            Vector3 directionToPart = part.transform.position - transform.position;

            // Apply force in the direction away from the object
            rigidbodyTarget.AddForce(directionToPart.normalized * _force, ForceMode.VelocityChange);

            // Add an additional force to simulate the flipping effect (you may need to adjust this value)
            rigidbodyTarget.AddForce(Vector3.up * (_force / 5), ForceMode.VelocityChange);
        }
    }

    IEnumerator CR_DisableCarPhysics(CarPhysics carPhysic)
    {
        carPhysic.CanMove = false;
        yield return new WaitForSeconds(_stunCarPhysicsDuration);
        carPhysic.CanMove = true;
    }

    private void OnTriggerEnter(Collider hitInfo)
    {
        if (hitInfo.TryGetComponent(out PBPart part))
        {
            if (part.GetComponent<CarPhysics>() != null)
            {
                _pbPartsTrigger.Add(part);
            }
        }
    }
    private void OnTriggerExit(Collider outInfo)
    {
        if (outInfo.TryGetComponent(out PBPart part))
        {
            _pbPartsTrigger.Remove(part);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (m_IsCollisionBreak)
        {
            PBPart part = collision.gameObject.GetComponent<PBPart>();
            PartBehaviour partBehaviour = collision.gameObject.GetComponent<PartBehaviour>();
            if (part != null)
            {
                #region Design Event
                try
                {
                    if (PBFightingStage.Instance != null)
                    {
                        if (part.RobotChassis?.Robot?.PersonalInfo?.isLocal == true)
                        {
                            string statgeID = ($"{PBFightingStage.Instance.name}").Replace(" Variant(Clone)", "");
                            string objectType = $"GasolineBarrel";
                            GameEventHandler.Invoke(DesignEvent.StageInteraction, statgeID, objectType);
                            Debug.Log($"Key - statgeID: {statgeID} | objectType: {objectType}");

                        }
                    }
                }
                catch
                {}
                #endregion

                Break();
                return;
            }
            if (partBehaviour != null)
                Break();
        }

        if (!hasExploded)
        {
            if (collision.impulse.magnitude > _breakForceMagnitudeThreshold)
            {
                Break();
            }
        }
    }

    [Button]
    private void Break()
    {
        if (hasExploded)
            return;

        if (_healthBarObstacle != null)
            Destroy(_healthBarObstacle.gameObject);

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, m_ZoneExplode, LayerMask.GetMask("Ground")))
        {
            fireDoTField.transform.position = hit.point;
            fireDoTField.transform.rotation = Quaternion.identity;
            fireDoTField.SetActive(true);
            StartCoroutine(OnTakeDamageEverySecond());
        }
        _mainrig.isKinematic = true;
        _mainCollider.enabled = false;
        _mainMeshObject.enabled = false;
        _objectBreak.SetActive(true);

        Explode();
        StartCoroutine(LifeTimeSplinter());
    }

    private IEnumerator LifeTimeSplinter()
    {
        WaitForSeconds waitForSecondsDelay = new WaitForSeconds(_lifeTime);
        WaitForSeconds waitForSecondsEverySplinterDisable = new WaitForSeconds(0.1f);
        yield return waitForSecondsDelay;

        if (_splinters != null)
        {
            for (int i = 0; i < _splinters.Count; i++)
            {
                _splinters[i].gameObject.SetActive(false);
                yield return waitForSecondsEverySplinterDisable;
            }
        }

        hasDestroyedAllSplinters = true;
        if (hasDestroyedAllSplinters && hasDestroyedFireDoTField)
        {
            Destroy(transform.parent.gameObject);
        }
    }
    public void ReceiveDamage(IAttackable attacker, float forceTaken)
    {
        #region Design Event
        try
        {
            if (PBFightingStage.Instance != null)
            {
                if (attacker is PBPart attackerPart && attackerPart.RobotChassis?.Robot?.PersonalInfo?.isLocal == true)
                {
                    string statgeID = ($"{PBFightingStage.Instance.name}").Replace(" Variant(Clone)", "");
                    string objectType = $"GasolineBarrel";
                    GameEventHandler.Invoke(DesignEvent.StageInteraction, statgeID, objectType);
                    Debug.Log($"Key - statgeID: {statgeID} | objectType: {objectType}");

                }
            }
        }
        catch
        {}
        #endregion

        Break();
        return;

        // if (_maxHealPoint < 0)
        //     return;

        // _healPoint -= attacker.GetDamage();
        // if (_healPoint <= 0)
        //     Break();

        // var value = _healPoint / _maxHealPoint;
        // if (_healthBarObstacle != null)
        //     _healthBarObstacle.SetFilledAmount(value);

        // _mainMeshObject.SetBlendShapeWeight(0, value);
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

    private IEnumerator OnTakeDamageEverySecond()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(_causeDamageSpeedPerSec);
        var startTime = Time.time;
        while (Time.time - startTime < _duration)
        {
            _pbPartsTrigger.ForEach((v) =>
            {
                if (v == null) return;

                if (v.RobotChassis.Robot.Health > 0)
                {
                    v.ReceiveDamage(this, Const.FloatValue.ZeroF);
                    return;
                }
            });
            yield return waitForSeconds;
        }
        fireDoTFieldVFX.Stop();
        hasDestroyedFireDoTField = true;
        if (hasDestroyedAllSplinters && hasDestroyedFireDoTField)
        {
            Destroy(transform.parent.gameObject);
        }
    }

    public float GetDamage() => hasExploded ? _damagePerWave : _damage;
    public bool IsBroken() => hasExploded;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Set Gizmo color
        Gizmos.color = new Color(1f, 0f, 0f, 0.15f); // Red with transparency

        // Draw the explosion radius sphere
        Gizmos.DrawSphere(transform.position, m_ZoneExplode);
    }
#endif
}

public interface IExplodable
{
    void Explode();
}
