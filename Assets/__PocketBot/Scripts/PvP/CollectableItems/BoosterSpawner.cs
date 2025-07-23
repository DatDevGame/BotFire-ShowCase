using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.SerializedDataStructure;
using System.Linq;
using System;

public class BoosterSpawner : MonoBehaviour
{
    public Action OnSpawnBooster;
    public bool IsHasBooster => m_Booster != null;

    [SerializeField, BoxGroup("Config")] private List<PvPBoosterType> m_ScriptedBooster;
    [SerializeField, BoxGroup("Config")] private SerializedDictionary<PvPBoosterType, Booster> m_Boosters;
    [SerializeField, BoxGroup("Config")] private Transform m_SpawnPoint;
    [SerializeField, BoxGroup("Config")] private float m_SpawnInterval = 3f;

    private int m_ScriptedIndex = 0;
    private bool m_UseRandom = false;
    private Booster m_Booster;
    [ShowInInspector] private SerializedDictionary<PvPBoosterType, Booster> m_BoosterSaves;

    private void Start()
    {
        m_BoosterSaves = new SerializedDictionary<PvPBoosterType, Booster> ();

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(m_SpawnInterval);
            SpawnBooster();
            yield return new WaitUntil(() => m_Booster == null);
        }
    }

    private void SpawnBooster()
    {
        Booster boosterToSpawn = null;

        if (!m_UseRandom && m_ScriptedIndex < m_ScriptedBooster.Count)
        {
            boosterToSpawn = m_Boosters[m_ScriptedBooster[m_ScriptedIndex]];
            m_ScriptedIndex++;
        }
        else
        {
            m_UseRandom = true;
            boosterToSpawn = GetRandomBooster();
        }

        if (boosterToSpawn != null)
        {
            if (m_BoosterSaves.ContainsKey(boosterToSpawn.GetBoosterType()))
            {
                //m_Booster.OnCollected -= OnItemCollected;
                m_Booster = m_BoosterSaves[boosterToSpawn.GetBoosterType()];
                m_Booster.EnableBooster();
            }
            else
            {
                m_Booster = Instantiate(boosterToSpawn, m_SpawnPoint.position, Quaternion.identity, transform);
                if (!m_BoosterSaves.ContainsKey(m_Booster.GetBoosterType()))
                    m_BoosterSaves.Add(m_Booster.GetBoosterType(), m_Booster);
                m_Booster.OnCollected += OnItemCollected;
            }
        }
        OnSpawnBooster?.Invoke();
    }

    private void OnItemCollected()
    {
        m_Booster = null;
    }

    private Booster GetRandomBooster()
    {
        if (m_Boosters.Count == 0) return null;

        List<Booster> boosterList = new List<Booster>(m_Boosters.Values);
        return boosterList[UnityEngine.Random.Range(0, boosterList.Count)];
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        for (int i = 0; i < 360; i += 10)
        {
            float rad = i * Mathf.Deg2Rad;
            float nextRad = (i + 10) * Mathf.Deg2Rad;
            Vector3 point1 = m_SpawnPoint.position + new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * 0.5f;
            Vector3 point2 = m_SpawnPoint.position + new Vector3(Mathf.Cos(nextRad), 0, Mathf.Sin(nextRad)) * 0.5f;
            Gizmos.DrawLine(point1, point2);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(m_SpawnPoint.position, 0.2f);
    }
#endif
}
