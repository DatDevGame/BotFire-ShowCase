using System.Collections;
using UnityEngine;
using DG.Tweening;
using LatteGames.Template;
using Sirenix.OdinInspector;
using UnityEngine.Animations;
using UnityEngine.UI;
using System.Collections.Generic;
using HightLightDebug;

public class TimeBomb : MonoBehaviour, IAttackable, IDamagable, IExplodable
{
    [SerializeField, BoxGroup("Config")] private float m_ZoneExplode = 3;
    [SerializeField, BoxGroup("Config")] private float m_Damage = 100;
    [SerializeField, BoxGroup("Config")] private float m_Force = 1000;
    [SerializeField, BoxGroup("Config")] private float m_CountdownTime = 5f;
    [SerializeField, BoxGroup("Config")] private float m_MinForceActiveBomb = 1;

    [SerializeField, BoxGroup("Ref")] private MeshRenderer m_BombMeshRenderer;
    [SerializeField, BoxGroup("Ref")] private ParticleSystem m_ExplosionEffect;
    [SerializeField, BoxGroup("Ref")] private ParticleSystem m_WarningEffect;
    [SerializeField, BoxGroup("Ref")] private Image m_TimerImage;
    [SerializeField, BoxGroup("Ref")] private LookAtConstraint m_LookAtConstraint;

    private Collider m_BombCollider;
    private bool _isCountingDown = false;
    private bool m_IsExplode = false;
    private Dictionary<PBChassis, int> m_ChassicDamage;
    private Dictionary<IDamagable, int> m_ObjectIDamage;
    private void Awake()
    {
        m_ChassicDamage = new Dictionary<PBChassis, int>();
        m_ObjectIDamage = new Dictionary<IDamagable, int>();
        m_BombCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        if (m_LookAtConstraint != null)
        {
            ConstraintSource constraintSource = new ConstraintSource();
            constraintSource.sourceTransform = MainCameraFindCache.Get().transform;
            constraintSource.weight = 1.0f;
            m_LookAtConstraint.AddSource(constraintSource);
            m_LookAtConstraint.constraintActive = true;
        }

        m_IsExplode = false;
        m_TimerImage.gameObject.SetActive(false);
        m_WarningEffect.gameObject.SetActive(true);
    }
    private void LateUpdate()
    {
        if(m_LookAtConstraint != null)
            m_LookAtConstraint.transform.position = new Vector3(transform.position.x, transform.position.y + 1.05f, transform.position.z);
    }
    public float GetDamage() => m_Damage;

    private void ForceObject(PBPart part)
    {
        Rigidbody rigidbodyTarget = part.GetComponent<Rigidbody>();
        if (rigidbodyTarget == null) return;

        // Calculate direction from this object to the part
        Vector3 directionToPart = part.transform.position - transform.position;

        // Apply force in the direction away from the object
        rigidbodyTarget.AddForce(directionToPart * m_Force, ForceMode.Impulse);

        // Add an upward force to simulate flipping
        rigidbodyTarget.AddForce(Vector3.up * (m_Force / 5), ForceMode.Impulse);
    }

    private void StartCountdown()
    {
        if (_isCountingDown) return;
        _isCountingDown = true;
        m_TimerImage.gameObject.SetActive(true);

        TweenCallback<float> tweenCallback = (value) =>
        {
            m_TimerImage.fillAmount = value;
            if (value <= 0)
            {
                _isCountingDown = false;
                Explode();
            }
        };
        DOVirtual.Float(1, 0, m_CountdownTime, tweenCallback).SetEase(Ease.Linear);
    }

    private void Explode()
    {
        if (m_IsExplode) return;
        m_IsExplode = true;

        m_TimerImage.gameObject.SetActive(false);
        m_BombCollider.enabled = false;
        m_BombMeshRenderer.enabled = false;
        m_WarningEffect.Stop();
        m_WarningEffect.gameObject.SetActive(false);
        m_ExplosionEffect.transform.SetParent(transform.parent, true);
        m_ExplosionEffect.gameObject.SetActive(true);
        m_ExplosionEffect.Play();
        m_ExplosionEffect.transform.position = transform.position;

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
                if (!m_ObjectIDamage.ContainsKey(damagable))
                {
                    m_ObjectIDamage.Add(damagable, 1);
                    damagable.ReceiveDamage(this, m_Damage);
                }
            }
        }


        SoundManager.Instance.PlayLoopSFX(SFX.NukeExplosion, 0.5f, false, true, gameObject);

        // Destroy the bomb after the explosion effect
        Destroy(transform.parent.gameObject, m_ExplosionEffect.main.duration);
    }

    private void OnCollisionEnter(Collision collision)
    {
        PBPart part = collision.gameObject.GetComponent<PBPart>();
        IAttackable attackable = collision.gameObject.GetComponent<IAttackable>();
        if (part != null || attackable != null)
        {
            if (part.RobotChassis.RobotBaseBody.velocity.magnitude >= m_MinForceActiveBomb)
            {
                StartCountdown();
            }
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
    void IExplodable.Explode()
    {
        throw new System.NotImplementedException();
    }


}
