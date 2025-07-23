using System.Collections;
using System.Collections.Generic;

using LatteGames.Template;

using Sirenix.OdinInspector;

using UnityEngine;

public class TankMineTrap : MonoBehaviour, IAttackable, IDamagable, IExplodable
{
    [SerializeField, BoxGroup("Config")] private float m_ZoneExplode = 3;
    [SerializeField, BoxGroup("Config")] private float _damage = 100;
    [SerializeField, BoxGroup("Config")] private float _force = 1000;

    [SerializeField, BoxGroup("Ref")] private MeshRenderer _tankMineMeshRender;
    [SerializeField, BoxGroup("Ref")] private ParticleSystem _explosionEffect;
    [SerializeField, BoxGroup("Ref")] private ParticleSystem _warningEffect;

    private Collider _colliderTankMine;
    private bool m_IsExplode = false;
    private Dictionary<PBChassis, int> m_ChassicDamage;
    private Dictionary<IDamagable, int> m_ObjectIDamage;

    private void Awake()
    {
        m_ChassicDamage = new Dictionary<PBChassis, int>();
        m_ObjectIDamage = new Dictionary<IDamagable, int>();
        _colliderTankMine = gameObject.GetComponent<Collider>();
    }

    private IEnumerator Start()
    {
        float randomShowWarningEffect = Random.Range(0.05f, 2f);
        yield return new WaitForSeconds(randomShowWarningEffect);
        _warningEffect.gameObject.SetActive(true);
    }

    public float GetDamage() => _damage;

    private void ForceObject(PBPart part)
    {
        Rigidbody rigidbodyTarget = part.GetComponent<Rigidbody>();
        if (rigidbodyTarget == null) return;

        // Add force if the part's health is above the damage threshold
        if (part != null && part.RobotChassis != null && part.RobotChassis.Robot.Health > _damage)
        {
            // Calculate direction from this object to the part
            Vector3 directionToPart = part.transform.position - transform.position;

            // Apply force in the direction away from the object
            rigidbodyTarget.AddForce(directionToPart * _force, ForceMode.Impulse);

            // Add an additional force to simulate the flipping effect (you may need to adjust this value)
            rigidbodyTarget.AddForce(Vector3.up * (_force / 5), ForceMode.Impulse);
        }
    }

    public void Explode()
    {
        if (m_IsExplode) return;
        m_IsExplode = true;

        _colliderTankMine.enabled = false;
        _tankMineMeshRender.enabled = false;
        _warningEffect.gameObject.SetActive(false);
        _explosionEffect.gameObject.SetActive(true);
        _explosionEffect.Play();
        if (PBSoundUtility.IsOnSound())
            SoundManager.Instance.PlaySFX(SFX.Landmine);

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
                    ForceObject(pbPart);
                }
            }
            else if (obj.TryGetComponent<IDamagable>(out IDamagable damagable))
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

        Destroy(gameObject, _explosionEffect.main.duration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PBPart>() != null || other.GetComponent<IAttackable>() != null)
        {
            StartCoroutine(DelayExplode());
        }
    }

    public void ReceiveDamage(IAttackable attacker, float forceTaken)
    {
        StartCoroutine(DelayExplode());
    }
    private IEnumerator DelayExplode()
    {
        yield return new WaitForSeconds(0.1f);
        Explode();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Set Gizmo color
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f); // Red with transparency

        // Draw the explosion radius sphere
        Gizmos.DrawSphere(transform.position, m_ZoneExplode);
    }
#endif
}
