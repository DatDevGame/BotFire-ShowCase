using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public enum HiddenRewardType
{
    Empty,
    Health,
    Speed,
    Coin,
    Value
}

[System.Serializable]
public class RewardChanceDictionary : SerializedDictionary<HiddenRewardType, float> { }

[System.Serializable]
public class RewardPrefabDictionary : SerializedDictionary<HiddenRewardType, GameObject> { }

public class HiddenReward : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] private bool m_IsActiveGif;
    [SerializeField, BoxGroup("Config")] private int m_CoinFrom, m_CoinTo;

    [SerializeField, BoxGroup("Chances"), Tooltip("Spawn probability for each reward type (total must always be 100%)")]
    private RewardChanceDictionary m_RewardChances = new RewardChanceDictionary();

    [SerializeField, BoxGroup("Prefabs"), Tooltip("Prefab associated with each reward type")]
    private RewardPrefabDictionary m_RewardPrefabs = new RewardPrefabDictionary();

    private void Awake()
    {
        AutoUpdateEnum();
    }

    public void Spawn(Vector3 spawnPosition, IAttackable attacker)
    {
        if(!m_IsActiveGif)
            return;

        SpawnRandomReward(spawnPosition, attacker);
    }

    private void SpawnReward(Vector3 spawnPosition, HiddenRewardType rewardType)
    {
        if (m_RewardPrefabs.TryGetValue(rewardType, out GameObject prefab) && prefab != null)
        {
            Instantiate(prefab, spawnPosition, Quaternion.identity);
        }
    }

    private void SpawnRandomReward(Vector3 spawnPosition, IAttackable attacker)
    {
        HiddenRewardType rewardType = GetRandomRewardType();
        if (m_RewardPrefabs.TryGetValue(rewardType, out GameObject prefab) && prefab != null)
        {
            var rewardItem = Instantiate(prefab, spawnPosition, Quaternion.identity);
            if (rewardType == HiddenRewardType.Coin && attacker != null && attacker is PBPart part)
            {
                if (part != null)
                {
                    if (part.RobotChassis != null)
                    {
                        int randomCoin = Random.Range(m_CoinFrom, m_CoinTo + 1);
                        CoinRewardPVP coinRewardPVP = rewardItem.GetComponent<CoinRewardPVP>();
                        if (coinRewardPVP != null)
                            coinRewardPVP.Spawn(attacker, randomCoin);
                    }
                }
            }
            else
            {
                IPhysicsHandler physicsHandler = rewardItem.GetComponent<IPhysicsHandler>();
                if (physicsHandler != null)
                    physicsHandler.EnablePhysics();
            }
        }
    }

    private HiddenRewardType GetRandomRewardType()
    {
        float randomValue = Random.Range(0f, 100f);
        float currentSum = 0f;

        foreach (var kvp in m_RewardChances)
        {
            currentSum += kvp.Value;
            if (randomValue <= currentSum)
                return kvp.Key;
        }

        return HiddenRewardType.Empty;
    }

    [Button("Normalize Chances")]
    private void NormalizeChances()
    {
        float total = 0;

        if (m_RewardChances.Count == 0)
        {
            m_RewardChances[HiddenRewardType.Health] = 100f;
            return;
        }

        foreach (var kvp in m_RewardChances)
        {
            total += kvp.Value;
        }

        if (total == 0)
        {
            float equalChance = 100f / m_RewardChances.Count;
            List<HiddenRewardType> keys = new List<HiddenRewardType>(m_RewardChances.Keys);
            foreach (var key in keys)
            {
                m_RewardChances[key] = equalChance;
            }
            return;
        }

        List<HiddenRewardType> keysList = new List<HiddenRewardType>(m_RewardChances.Keys);
        foreach (var key in keysList)
        {
            m_RewardChances[key] = (m_RewardChances[key] / total) * 100f;
        }
    }

    [Button("Auto Update Enum")]
    private void AutoUpdateEnum()
    {
        HiddenRewardType[] allTypes = (HiddenRewardType[])System.Enum.GetValues(typeof(HiddenRewardType));

        foreach (var type in allTypes)
        {
            if (!m_RewardChances.ContainsKey(type))
                m_RewardChances[type] = 0f;

            if (!m_RewardPrefabs.ContainsKey(type))
                m_RewardPrefabs[type] = null;
        }

        List<HiddenRewardType> existingKeys = new List<HiddenRewardType>(m_RewardChances.Keys);
        foreach (var key in existingKeys)
        {
            if (System.Array.IndexOf(allTypes, key) == -1)
            {
                m_RewardChances.Remove(key);
                m_RewardPrefabs.Remove(key);
            }
        }
        m_RewardPrefabs.Remove(HiddenRewardType.Empty);
        NormalizeChances();
    }
}
