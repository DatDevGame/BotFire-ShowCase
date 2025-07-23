using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using HyrphusQ.Events;
using System.Linq;
using DG.Tweening;

public class InfoWeaponBooster
{
    public InfoWeaponBooster(BoosterVFX boosterVFX, Mesh mesh)
    {
        BoosterVFX = boosterVFX;
        Mesh = mesh;
    }
    public BoosterVFX BoosterVFX;
    public Mesh Mesh;
}

public class PBBoosterVFXManager : Singleton<PBBoosterVFXManager>
{
    [SerializeField] private Transform m_Holder;
    [SerializeField] private SpeedUpAttackVFX m_SpeedUpVFXPrefab;
    [SerializeField] private SpeedBoostVFX m_SpeedBoostVFXPrefab;
    [ShowInInspector] private Dictionary<PBPart, List<SpeedUpAttackVFX>> m_SpeedUpAttackVFXs;
    [ShowInInspector] private Dictionary<PBChassis, SpeedBoostVFX> m_SpeedBoosterVFXs;

    protected override void Awake()
    {
        base.Awake();
        m_SpeedUpAttackVFXs = new Dictionary<PBPart, List<SpeedUpAttackVFX>>();
        m_SpeedBoosterVFXs = new Dictionary<PBChassis, SpeedBoostVFX>();

        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnResetBots, OnResetBots);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnResetBots, OnResetBots);
    }

    private void OnMatchStarted()
    {
        ClearVFX();
    }
    private void OnFinalRoundCompleted()
    {
        ClearVFX();
    }
    private void OnResetBots()
    {
        ClearVFX();
    }
    private void ClearVFX()
    {
        for (int i = 0; i < m_SpeedUpAttackVFXs.Count; i++)
            m_SpeedUpAttackVFXs.ElementAt(i).Value.Where(x => x != null).ToList().ForEach(v => Destroy(v.gameObject));
        m_SpeedUpAttackVFXs.Clear();
    }

    public void PlayBoosterSpeedUpAttackWeapon(PBPart pbPart, List<MeshRenderer> meshRenderer)
    {
        if (!m_SpeedUpAttackVFXs.ContainsKey(pbPart))
        {
            List<SpeedUpAttackVFX> speedUpAttackVFXs = new List<SpeedUpAttackVFX>();
            for (int i = 0; i < meshRenderer.Count; i++)
            {
                var particle = Instantiate(m_SpeedUpVFXPrefab, m_Holder);
                particle.name = $"{pbPart.PartSO.GetDisplayName()} - SpeedUpAttackVFX - MeshRenderer";
                particle.SetMesh(meshRenderer[i]);
                speedUpAttackVFXs.Add(particle);
            }
            m_SpeedUpAttackVFXs.Add(pbPart, speedUpAttackVFXs);
        }
        m_SpeedUpAttackVFXs[pbPart].ForEach(v => v.OnVFX());
    }

    public void PlayBoosterSpeedUpAttackWeapon(PBPart pbPart, List<SkinnedMeshRenderer> skinnedMeshRenderer)
    {
        if (!m_SpeedUpAttackVFXs.ContainsKey(pbPart))
        {
            List<SpeedUpAttackVFX> speedUpAttackVFXs = new List<SpeedUpAttackVFX>();
            for (int i = 0; i < skinnedMeshRenderer.Count; i++)
            {
                var particle = Instantiate(m_SpeedUpVFXPrefab, m_Holder);
                particle.name = $"{pbPart.PartSO.GetDisplayName()} - SpeedUpAttackVFX - SkinnedMeshRenderer";
                particle.SetMesh(skinnedMeshRenderer[i]);
                speedUpAttackVFXs.Add(particle);
            }
            m_SpeedUpAttackVFXs.Add(pbPart, speedUpAttackVFXs);
        }
        m_SpeedUpAttackVFXs[pbPart].ForEach(v => v.OnVFX());
    }

    public void PlaySpeedBooster(PBChassis pbChassis)
    {
        if (pbChassis == null)
            return;
        if (pbChassis.CarPhysics == null)
            return;

        if (!m_SpeedBoosterVFXs.ContainsKey(pbChassis))
        {
            var particle = Instantiate(m_SpeedBoostVFXPrefab, m_Holder);
            particle.transform.SetParent(pbChassis.CarPhysics.transform);
            particle.transform.localPosition = Vector3.zero;
            particle.transform.localEulerAngles = new Vector3(0, 90, 0);
            particle.name = $"{pbChassis.PartSO.GetDisplayName()} - SpeedBoosterVFX";
            m_SpeedBoosterVFXs.Add(pbChassis, particle);
        }

        m_SpeedBoosterVFXs[pbChassis].AddRigidbodyObject(pbChassis.CarPhysics.GetComponent<Rigidbody>());
        m_SpeedBoosterVFXs[pbChassis].OnVFX();
    }
    public void StopSpeedBooster(PBChassis pbChassis)
    {
        if (pbChassis == null)
            return;
        m_SpeedBoosterVFXs[pbChassis].OnVFX();
    }

    public void StopBoosterSpeedUpAttackWeapon(PBPart pbPart)
    {
        if (m_SpeedUpAttackVFXs.ContainsKey(pbPart))
            m_SpeedUpAttackVFXs[pbPart].ForEach(v => v.OffVFX());
    }
}
