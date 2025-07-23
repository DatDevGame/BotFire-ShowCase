using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarInfoTeleport
{
    public DateTime DateTime;
    public List<Collider> Colliders;
    public List<MeshRenderer> MeshRenderers;

    public CarInfoTeleport(DateTime dateTime, List<Collider> colliders, List<MeshRenderer> meshRenderers)
    {
        DateTime = dateTime;
        Colliders = colliders;
        MeshRenderers = meshRenderers;
    }
}
public class PortalGate : MonoBehaviour
{
    public Dictionary<PBChassis, CarInfoTeleport> InfoCarTeleport => m_InfoCarTeleport;
    public ParticleSystem TeleVFX => m_TeleVFX;

    [SerializeField, BoxGroup("Ref")] private PortalLink m_PortalA1, m_PortalA2;
    [SerializeField, BoxGroup("Resource")] private ParticleSystem m_TeleVFX;
    [ShowInInspector] private Dictionary<PBChassis, CarInfoTeleport> m_InfoCarTeleport = new Dictionary<PBChassis, CarInfoTeleport>();

    public bool IsAceptTeleport(PBChassis chassis)
    {
        if (chassis == null)
            return false;

        bool result = false;
        if (m_InfoCarTeleport.ContainsKey(chassis))
        {
            result = DateTime.Now >= m_InfoCarTeleport[chassis].DateTime.AddSeconds(1.5f);
            if (result)
                m_InfoCarTeleport[chassis].DateTime = DateTime.Now;
        }
        else
        {
            List<Collider> colliders = new List<Collider>();
            colliders = chassis.Robot.GetComponentsInChildren<Collider>().ToList();

            List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
            meshRenderers = chassis.Robot.GetComponentsInChildren<MeshRenderer>().ToList();


            m_InfoCarTeleport.Add(chassis, new CarInfoTeleport(DateTime.Now, colliders, meshRenderers));
            result = true;
        }

        return result;
    }


    public void EnableCard(PBChassis chassis)
    {
        if (chassis != null)
        {
            m_InfoCarTeleport[chassis].Colliders.ForEach(v => { v.enabled = true; });
            m_InfoCarTeleport[chassis].MeshRenderers.ForEach(v => { v.enabled = true; });
        }

    }
    public void DisableCar(PBChassis chassis)
    {
        if (chassis != null)
        {
            m_InfoCarTeleport[chassis].Colliders.ForEach(v => { v.enabled = false; });
            m_InfoCarTeleport[chassis].MeshRenderers.ForEach(v => { v.enabled = false; });
        }

    }
}
