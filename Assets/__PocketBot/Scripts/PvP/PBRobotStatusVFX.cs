using System.Collections;
using System.Collections.Generic;
using LatteGames.Template;
using UnityEngine;

public class PBRobotStatusVFX : MonoBehaviour
{
    PBChassis m_Chassis;

    [SerializeField]
    ParticleSystem m_Smoke, m_Fire, m_Explosion;
    ParticleSystem m_SmokeInst, m_FireInst, m_ExplosionInst;

    ParticleSystem SmokeInst
    {
        get
        {
            if (m_SmokeInst == null)
            {
                m_SmokeInst = Instantiate(m_Smoke, PBChassis.RobotBaseBody.transform);
            }
            return m_SmokeInst;
        }
    }

    ParticleSystem FireInst
    {
        get
        {
            if (m_FireInst == null)
            {
                m_FireInst = Instantiate(m_Fire, PBChassis.RobotBaseBody.transform);
            }
            return m_FireInst;
        }
    }

    ParticleSystem ExplosionInst
    {
        get
        {
            if (m_ExplosionInst == null)
            {
                m_ExplosionInst = Instantiate(m_Explosion, PBChassis.RobotBaseBody.transform);
            }
            return m_ExplosionInst;
        }
    }

    PBChassis PBChassis
    {
        get
        {
            if (m_Chassis == null)
            {
                m_Chassis = GetComponent<PBChassis>();
            }
            return m_Chassis;
        }
    }

    private void Awake()
    {
        SmokeInst.gameObject.SetActive(false);
        FireInst.gameObject.SetActive(false);
        ExplosionInst.gameObject.SetActive(false);
    }

    public void EnableVFX(VFXEnum vfxenum)
    {
        switch (vfxenum)
        {
            case VFXEnum.Smoke:
                {
                    SmokeInst.gameObject.SetActive(true);
                    break;
                }
            case VFXEnum.Fire:
                {
                    FireInst.gameObject.SetActive(true);
                    break;
                }
            case VFXEnum.Explosion:
                {
                    ExplosionInst.gameObject.SetActive(true);
                    SoundManager.Instance.PlaySFX(SFX.Explosion);
                    break;
                }
        }
    }

    public enum VFXEnum
    {
        Smoke,
        Fire,
        Explosion
    }

}
