using System;
using System.Collections;
using System.Collections.Generic;

using LatteGames.Template;

using Sirenix.OdinInspector;

using UnityEngine;

public abstract class DroneBehavior : MonoBehaviour, IAttackable
{
    [SerializeField, BoxGroup("Base")] private Rigidbody _rigidbody;
    [SerializeField, BoxGroup("Base")] private Transform _modelRoot;
    [SerializeField, BoxGroup("Base")] private float _tiltSpeed;
    [SerializeField, BoxGroup("Base"), Range(0, 90)] private float _tiltMax;
    [SerializeField, BoxGroup("Base")] private float _slowDownRange;
    [SerializeField, BoxGroup("Base")] private float _targetKeepDistance;
    [SerializeField, BoxGroup("Base")] private float _appearTime;
    [SerializeField, BoxGroup("Base")] private AnimationCurve _appearAnimCurve;
    [SerializeField, BoxGroup("Base")] private AnimationCurve _speedMaxCurve;
    [SerializeField, BoxGroup("Base")] private List<Renderer> _renderers;
    protected PBRobot _target;
    protected PBRobot _owner;
    protected float _hpPercentDamage;
    protected float _damagePerHitRatio;
    protected float _flyHeight;
    protected float _attackRange;
    protected float _speedMax;
    protected bool _isDoingAppearEffect;
    protected bool _isGunActivated;
    private float _randomOffset;

    #region Firebase Event
    protected HashSet<int> m_TargetIDs;
    protected ActiveSkillSO m_ActiveSkillSO;
    #endregion

    public PBRobot owner => _owner;

    public void AttackRobot(PBRobot robot)
    {
        _target = robot;
    }

    public void FollowOwner()
    {
        _target = null;
    }

    #region Firebase Event
    public virtual void InitTargetID(HashSet<int> targetIDs, ActiveSkillSO activeSkillSO)
    {
        m_TargetIDs = targetIDs;
        m_ActiveSkillSO = activeSkillSO;
    }
    #endregion

    public virtual void Init(PBRobot owner, Material material,
        float hpPercentDamage, float damagePerHitRatio,
        float speed, float attackRange, float flyHeight)
    {
        _randomOffset = UnityEngine.Random.Range(0f, 1f);
        _isGunActivated = false;
        _owner = owner;
        _hpPercentDamage = hpPercentDamage;
        _damagePerHitRatio = damagePerHitRatio;
        _speedMax = speed;
        _attackRange = attackRange;
        _flyHeight = flyHeight;
        foreach (var renderer in _renderers)
        {
            renderer.sharedMaterial = material;
        }
    }

    public virtual void Appear(Vector3 startOffset, Vector3 endOffset)
    {
        if (PBSoundUtility.IsOnSound())
        {
            SoundManager.Instance.PlaySFX(SFX.DroneAppear);
            SoundManager.Instance.PlayLoopSFX(SFX.DroneFlying, 1f, true, true, gameObject);
        }
        ActiveGun(false);
        _isDoingAppearEffect = true;
        StartCoroutine(AppearEffect_CR(
            startOffset, endOffset,
            (t) => _appearAnimCurve.Evaluate(t),
            null
        ));
    }

    public virtual void Disappear(Vector3 startOffset, Vector3 endOffset, Action<DroneBehavior> callback)
    {
        ActiveGun(false);
        _isDoingAppearEffect = true;
        callback += (droneBehavior) =>
        {
            SoundManager.Instance?.StopLoopSFX(droneBehavior.gameObject);
        };
        StartCoroutine(AppearEffect_CR(
            startOffset, endOffset,
            (t) => 1 - _appearAnimCurve.Evaluate(1 - t),
            callback
        ));
    }

    private IEnumerator AppearEffect_CR(Vector3 startOffset, Vector3 endOffset, Func<float, float> animFunc, Action<DroneBehavior> effectEndCB)
    {
        float timePass = 0;
        while (timePass < _appearTime)
        {
            transform.position = Vector3.Lerp(
                _owner.ChassisInstanceTransform.position + startOffset,
                _owner.ChassisInstanceTransform.position + endOffset,
                animFunc(timePass / _appearTime));
            yield return null;
            timePass += Time.deltaTime;
        }
        transform.position = _owner.ChassisInstanceTransform.position + endOffset;
        effectEndCB?.Invoke(this);
        _isDoingAppearEffect = false;
    }

    protected virtual void FixedUpdate()
    {
        UpdateMovement();
        UpdateGunAim();
    }

    protected virtual void OnDestroy()
    {
        SoundManager.Instance?.StopLoopSFX(gameObject);
    }

    private void UpdateMovement()
    {
        if (_isDoingAppearEffect)
        {
            return;
        }
        PBRobot followTarget = _target ? _target : _owner;
        Vector3 targetHoverPos = followTarget.ChassisInstanceTransform.position + _flyHeight * Vector3.up;
        Vector3 myPos = transform.position;
        Vector3 me2HoverPos = targetHoverPos - myPos;
        Vector3 me2HoverPosNor = me2HoverPos.normalized;
        Vector3 velocity = _rigidbody.velocity;

        Vector3 horizontalForward = me2HoverPos;
        horizontalForward.y = 0;
        horizontalForward = horizontalForward.normalized;
        float speetTiltPer = Mathf.Clamp01(velocity.magnitude / _speedMax);
        //Idle tilting for randomness
        Quaternion autoTilt = Quaternion.AngleAxis((1 - speetTiltPer) * (1 - speetTiltPer) * 45f *
            (Mathf.PerlinNoise1D(0.6f * Time.time + _randomOffset) - 0.5f), horizontalForward);
        Quaternion tiltRotation = Quaternion.AngleAxis(-speetTiltPer * _tiltMax, Vector3.Cross(velocity, Vector3.up));
        _modelRoot.rotation = Quaternion.RotateTowards(
            _modelRoot.rotation,
            Quaternion.LookRotation(autoTilt * tiltRotation * horizontalForward, autoTilt * tiltRotation * Vector3.up),
            _tiltSpeed * Time.deltaTime);


        //if (_target)
        {
            targetHoverPos -= (_target ? _targetKeepDistance : Mathf.Clamp(_targetKeepDistance * 0.5f, 1f, 3f)) * me2HoverPosNor;
            me2HoverPos = targetHoverPos - myPos;
            me2HoverPosNor = me2HoverPos.normalized;
        }

        float me2HoverPosDist = me2HoverPos.magnitude;
        _rigidbody.AddForce(_speedMax *
            (me2HoverPosDist <= _slowDownRange ? _speedMaxCurve.Evaluate(me2HoverPosDist / _slowDownRange) : 1) *
            me2HoverPosNor - velocity, ForceMode.VelocityChange);
    }

    protected void ActiveGun(bool isActive)
    {
        if (_isGunActivated == isActive)
        {
            return;
        }
        _isGunActivated = isActive;
        if (_isGunActivated)
        {
            OnActiveGun();
        }
        else
        {
            OnDeactiveGun();
        }
    }

    protected abstract void UpdateGunAim();
    protected abstract void OnActiveGun();
    protected abstract void OnDeactiveGun();

    public float GetDamage()
    {
        return _hpPercentDamage * _owner.MaxHealth * _damagePerHitRatio * _owner.AtkMultiplier;
    }
}