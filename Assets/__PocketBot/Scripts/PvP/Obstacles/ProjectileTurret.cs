using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;
using LatteGames.Template;

public class ProjectileTurret : MonoBehaviour, IAttackable
{
    [SerializeField, LabelText("Damage")] private float _damage = 50f;
    [SerializeField, LabelText("Force Object")] private float _force = 100f;
    [SerializeField, LabelText("Life Time")] private float _lifeTime = 5f;
    [SerializeField, LabelText("Speed Projectile")] private float _speed = 10f;
    [SerializeField, LabelText("Explosion Prefab")] private ParticleSystem _explosionPrefab;
    [SerializeField, LabelText("Collider")] private Collider _colliderProjectile;

    private void Awake()
    {
        Destroy(gameObject, _lifeTime);
    }

    public void FireFoward(Vector3 targetPoint)
    {
        SoundManager.Instance.PlayLoopSFX(SFX.NukeProjectileFire, 0.2f, false, true, gameObject);
        float distanceTarget = Vector3.Distance(targetPoint, transform.position);
        transform
            .DOJump(targetPoint, 1, 1, distanceTarget / 40)
            .SetEase(Ease.Linear);
    }
    public void FireThrow(Vector3 targetPoint)
    {
        SoundManager.Instance.PlayLoopSFX(SFX.NukeProjectileFire, 0.2f, false, true, gameObject);
        float distanceTarget = Vector3.Distance(targetPoint, transform.position);
        float angleForce = distanceTarget / 3;
        transform
            .DOJump(targetPoint, angleForce, 1, distanceTarget / 20)
            .SetEase(Ease.Linear);
    }

    private void OnCollisionEnter(Collision hitInfo)
    {
        if (hitInfo.gameObject.TryGetComponent(out PBPart part))
        {
            part.ReceiveDamage(this, Const.FloatValue.ZeroF);
            ForceObject(part);
        }
        else
        {
            var iBreakObstacle = hitInfo.gameObject.GetComponent<IBreakObstacle>();
            if (iBreakObstacle != null)
            {
                ObstacleBreak obstacleBreak = (iBreakObstacle as MonoBehaviour).GetComponent<ObstacleBreak>();
                obstacleBreak.ReceiveDamage(this, Const.FloatValue.ZeroF);
            }
        }

        SoundManager.Instance.PlayLoopSFX(SFX.NukeExplosion, 0.1f, false, true, gameObject);
        _colliderProjectile.enabled = false;
        var explosionVfx = Instantiate(_explosionPrefab.gameObject).GetComponent<ParticleSystem>();
        explosionVfx.transform.position = transform.position;
        Destroy(this.gameObject);
        Destroy(explosionVfx.gameObject, 1);
    }
    private void ForceObject(PBPart part)
    {
        Rigidbody rigidbodyTarget = part.GetComponent<Rigidbody>();
        if (rigidbodyTarget == null) return;

        // Add force if the part's health is above the damage threshold
        if (part.RobotChassis.Robot.Health > _damage)
        {
            // Calculate direction from this object to the part
            Vector3 directionToPart = part.transform.position - transform.position;

            // Apply force in the direction away from the object
            rigidbodyTarget.AddForce(directionToPart * _force, ForceMode.Impulse);

            // Add an additional force to simulate the flipping effect (you may need to adjust this value)
            rigidbodyTarget.AddForce(Vector3.up * (_force / 5), ForceMode.Impulse);
        }
    }
    public float GetDamage()
    {
        return _damage;
    }
}
