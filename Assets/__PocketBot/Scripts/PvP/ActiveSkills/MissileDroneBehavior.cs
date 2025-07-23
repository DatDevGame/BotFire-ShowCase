using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using LatteGames.Template;

using Sirenix.OdinInspector;

using UnityEngine;

public class MissileDroneBehavior : DroneBehavior
{
    [SerializeField, BoxGroup("Missile")] private RocketArtilleryBullet _rocketTemplate;
    [SerializeField, BoxGroup("Missile")] private BoxObjectPlacer _leftRocketRack;
    [SerializeField, BoxGroup("Missile")] private BoxObjectPlacer _rightRocketRack;
    [SerializeField, BoxGroup("Missile")] private RocketArtilleryConfigSO _rocketConfigSO;
    private float _nextTimeGunOn;
    private List<RocketArtilleryBullet> _rocketBullets = new List<RocketArtilleryBullet>();
    private List<Vector3> _leftRocketSpawmPoints = new List<Vector3>();
    private List<Vector3> _rightRocketSpawmPoints = new List<Vector3>();
    private WaitForSeconds _delayPerRockerFire;
    private Coroutine _fireCR;

    private void Awake()
    {
        _leftRocketRack.IterateSpawnPoint((item1, item2) =>
        {
            _leftRocketSpawmPoints.Add(item2);
        });
        _rightRocketRack.IterateSpawnPoint((item1, item2) =>
        {
            _rightRocketSpawmPoints.Add(item2);
        });
        _delayPerRockerFire = new WaitForSeconds(_rocketConfigSO.FireRate);
    }

    public override void Init(PBRobot owner, Material material, float hpPercentDamage, float damagePerHitRatio, float speed, float attackRange, float flyHeight)
    {
        _nextTimeGunOn = Time.time;
        base.Init(owner, material, hpPercentDamage, damagePerHitRatio, speed, attackRange, flyHeight);
    }

    public override void Appear(Vector3 startOffset, Vector3 endOffset)
    {
        base.Appear(startOffset, endOffset);
        _nextTimeGunOn = Time.time;
    }

    public override void Disappear(Vector3 startOffset, Vector3 endOffset, Action<DroneBehavior> callback)
    {
        base.Disappear(startOffset, endOffset, callback);
        foreach (var rocket in _rocketBullets)
        {
            if (rocket == null)
            {
                continue;
            }
            Destroy(rocket.gameObject);
        }
        _rocketBullets.Clear();
    }

    protected override void UpdateGunAim()
    {
        if (_isDoingAppearEffect)
        {
            return;
        }
        if (!_isGunActivated && Time.time > _nextTimeGunOn && _target &&
            (transform.position - _target.ChassisInstanceTransform.position).magnitude <= _attackRange)
        {
            ActiveGun(true);
            return;
        }

        if (_isGunActivated && _fireCR == null)
        {
            ActiveGun(false);
            return;
        }
    }

    protected override void OnActiveGun()
    {
        if (_fireCR != null)
        {
            return;
        }
        _fireCR = StartCoroutine(Fire_CR());
    }

    protected override void OnDeactiveGun()
    {
        if (_fireCR != null)
        {
            StopCoroutine(_fireCR);
            _fireCR = null;
        }
        _nextTimeGunOn = Time.time + _rocketConfigSO.ReloadTime;
    }

    private void OnObjectHit(IDamagable damagableObject)
    {
        if (PBSoundUtility.IsOnSound())
            SoundManager.Instance.PlaySFX(SFX.DroneMissleExplosive);
        if (damagableObject == null)
            return;
        damagableObject.ReceiveDamage(this, 0);

        #region Firebase Event
        try
        {
            if (damagableObject is PBPart part)
            {
                int chassisID = part.RobotChassis.CarPhysics.CurrentRaycastHitTarget.colliderInstanceID;
                if (part.RobotChassis.Robot.PersonalInfo.isLocal && m_ActiveSkillSO != null && !m_TargetIDs.Contains(chassisID))
                {
                    m_TargetIDs.Add(chassisID);

                    string skillName = "null";
                    skillName = m_ActiveSkillSO.GetDisplayName();
                    GameEventHandler.Invoke(LogFirebaseEventCode.AffectedByOpponentSkill, skillName);
                }
            }

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    private void ReloadImmediately()
    {
        _rocketBullets.Clear();
        SpawnRocket(_leftRocketSpawmPoints, _leftRocketRack.transform);
        SpawnRocket(_rightRocketSpawmPoints, _rightRocketRack.transform);

        void SpawnRocket(List<Vector3> spawnPoints, Transform rocketRack)
        {
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                var rocketBullet = Instantiate(_rocketTemplate, rocketRack);
                rocketBullet.transform.localPosition = spawnPoints[i];
                rocketBullet.Init(_owner.gameObject.layer, _rocketConfigSO);
                rocketBullet.name += " " + i;
                _rocketBullets.Add(rocketBullet);
            }
        }
    }

    private void Fire(int index)
    {
        if (_rocketBullets[index] == null)
        {
            return;
        }
        if (PBSoundUtility.IsOnSound())
        {
            SoundManager.Instance.PlaySFX(SFX.TFM_MissleShot);
        }
        _rocketBullets[index].Fire(_target ? _target.ChassisInstanceTransform : null, OnObjectHit);
        _rocketBullets[index] = null;
    }

    public IEnumerator Fire_CR()
    {
        if (_rocketBullets.Count <= 0)
            ReloadImmediately();
        for (int i = 0; i < _rocketBullets.Count; i++)
        {
            Fire(i);
            if (i < _rocketBullets.Count - 1)
                yield return _delayPerRockerFire;
        }
        ReloadImmediately();
        _fireCR = null;
    }
}