using UnityEngine;
using DG.Tweening;
using System.Collections;
using Sirenix.OdinInspector;
using LatteGames.Template;

public class LazerBeamBehavior : MonoBehaviour, IAttackable
{
    [SerializeField, BoxGroup("Config")] private float m_Speed = 20f;
    [SerializeField, BoxGroup("Ref")] private Collider m_Collider;
    [SerializeField, BoxGroup("Ref")] private ParticleSystem m_LazerBeamVFX;
    [SerializeField, BoxGroup("Ref")] private ParticleSystem m_ExplosionVFX;

    private float m_Damage;

    public bool IsActive => m_IsActive;

    private PBRobot m_OwnerRobot;
    private IEnumerator m_LifeTimeCR;
    private bool m_IsActive = false;
    
    private void Update()
    {
        if (m_IsActive)
            transform.position += transform.forward * m_Speed * Time.deltaTime;
    }

    public void EnableMissile(PBRobot ownerRobot, PBRobot targetRobot, float damage)
    {
        if (m_IsActive)
            return;
        m_Damage = damage;
        if (m_LifeTimeCR != null)
            StopCoroutine(m_LifeTimeCR);
        m_LifeTimeCR = LifeTimeCR();
        StartCoroutine(m_LifeTimeCR);

        m_Collider.enabled = true;
        m_LazerBeamVFX.Play();
        m_LazerBeamVFX.gameObject.SetActive(true);
        m_OwnerRobot = ownerRobot;
        m_IsActive = true;
    }

    public void DisableMissile()
    {
        if (!m_IsActive)
            return;

        if (m_LifeTimeCR != null)
            StopCoroutine(m_LifeTimeCR);

        m_Collider.enabled = false;
        m_LazerBeamVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        m_LazerBeamVFX.gameObject.SetActive(false);
        transform.DOKill();
        m_IsActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        PBPart part = other.gameObject.GetComponent<PBPart>();
        if (part != null && part.RobotChassis.Robot != m_OwnerRobot)
        {
            SoundManager.Instance.PlaySFX(SFX.BlackKnight2_DroneLaserHit);
            part.ReceiveDamage(this, Const.FloatValue.ZeroF);
            m_ExplosionVFX.Play();
            DisableMissile();
        }
    }

    private IEnumerator LifeTimeCR()
    {
        yield return new WaitForSeconds(3);
        m_IsActive = false;
    }

    public float GetDamage()
    {
        return m_Damage;
    }
}
