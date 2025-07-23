using System.Collections.Generic;

using LatteGames.Template;

using Sirenix.OdinInspector;

using UnityEngine;

public class PyroDroneBehavior : DroneBehavior
{
    [SerializeField, BoxGroup("Pyro")] private float _gunRotateSpeed;
    [SerializeField, BoxGroup("Pyro")] private Transform _gunTrs;
    [SerializeField, BoxGroup("Pyro")] private OnTriggerCallback _onTriggerCallback;
    [SerializeField, BoxGroup("Pyro")] private ParticleSystem _activatedFire;
    [SerializeField, BoxGroup("Pyro")] private float _shootingCD;
    [SerializeField, BoxGroup("Pyro")] private float _shootingTime;
    private float _lastInflictedDamageTime;
    private float _nextTimeGunOff;
    private float _nextTimeGunOn;
    //private float _speed;
    private HashSet<PBRobot> _receivedDmgThisFrameRobots = new();

    public override void Init(PBRobot owner, Material material, float hpPercentDamage, float damagePerHitRatio, float speed, float attackRange, float flyHeight)
    {
        _nextTimeGunOff = _nextTimeGunOn = Time.time;
        base.Init(owner, material, hpPercentDamage, damagePerHitRatio, speed, attackRange, flyHeight);
    }

    protected override void FixedUpdate()
    {
        _receivedDmgThisFrameRobots.Clear();
        base.FixedUpdate();
    }

    private void HandleFireHitObject(Collider other)
    {
        float damageCooldown = Const.CollideValue.ContinuousDamageCooldown;
        if (Time.time - _lastInflictedDamageTime < damageCooldown) return;
        if (other == null) return;
        if (other.attachedRigidbody == null) return;
        if (!other.attachedRigidbody.TryGetComponent(out IDamagable damable)) return;
        if (damable is PBPart damablePart && damablePart.RobotChassis != null && damablePart.RobotChassis.Robot == _owner)
        {
            if (damablePart.RobotChassis.Robot == _owner || _receivedDmgThisFrameRobots.Contains(damablePart.RobotChassis.Robot))
            {
                return;
            }
            else
            {
                _receivedDmgThisFrameRobots.Add(damablePart.RobotChassis.Robot);
            }
        }
        //Match all condition
        _lastInflictedDamageTime = Time.time;
        damable.ReceiveDamage(this, 0);
    }

    protected override void UpdateGunAim()
    {
        if (!_target)
        {
            _gunTrs.localRotation = Quaternion.RotateTowards(_gunTrs.localRotation, Quaternion.Euler(0, 180, 0), _gunRotateSpeed * Time.deltaTime);
            ActiveGun(false);
        }
        else
        {
            Vector3 me2Target = transform.position - _target.ChassisInstanceTransform.position;
            ActiveGun(me2Target.magnitude <= _attackRange && (_nextTimeGunOn >= _nextTimeGunOff ? Time.time > _nextTimeGunOn : Time.time < _nextTimeGunOff));
            _gunTrs.rotation = Quaternion.RotateTowards(_gunTrs.rotation, Quaternion.LookRotation(me2Target, Vector3.up + transform.forward), _gunRotateSpeed * Time.deltaTime);
        }
    }

    protected override void OnActiveGun()
    {
        _activatedFire.Play();
        _onTriggerCallback.onTriggerStay += HandleFireHitObject;
        _nextTimeGunOff = Time.time + _shootingTime;
        if (PBSoundUtility.IsOnSound())
        {
            SoundManager.Instance.PlayLoopSFX(SFX.Flamethrower, 0.25f, true, true, _gunTrs.gameObject);
        }
    }

    protected override void OnDeactiveGun()
    {
        _activatedFire.Stop();
        _onTriggerCallback.onTriggerStay -= HandleFireHitObject;
        _nextTimeGunOff = _nextTimeGunOn = Time.time + _shootingCD;
        SoundManager.Instance?.StopLoopSFX(_gunTrs.gameObject);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        SoundManager.Instance?.StopLoopSFX(_gunTrs.gameObject);
    }
}