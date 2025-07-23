using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ABTestFreeCoinsAndGems", menuName = "PocketBots/ABTest/v2.0.0/ABTestFreeCoinsAndGems")]
public class ABTestFreeCoinsAndGems : GroupBasedABTestSO
{
    [Serializable]
    public class Data
    {
        public int timeWaitingForCoinsReward;
        public int timeWaitingForGemsReward;
        public List<int> coinsArenaMultipliers;
        public List<float> gemsArenaMultipliers;

        public void InjectData(HotOffersFreeCoinsSO freeCoinsSO, HotOffersFreeGemsSO freeGemsSO)
        {
            freeCoinsSO.TimeWaitingForReward = timeWaitingForCoinsReward;
            freeCoinsSO.ArenaMultipliers = new List<int>(coinsArenaMultipliers);

            freeGemsSO.TimeWaitingForReward = timeWaitingForGemsReward;
            freeGemsSO.ArenaMultipliers = new List<float>(gemsArenaMultipliers);
        }
    }

    [SerializeField]
    private List<Data> m_DataGroups;
    [SerializeField]
    private HotOffersFreeCoinsSO m_FreeCoinsSO;
    [SerializeField]
    private HotOffersFreeGemsSO m_FreeGemsSO;

    public override void InjectData(int groupIndex)
    {
        m_DataGroups[groupIndex].InjectData(m_FreeCoinsSO, m_FreeGemsSO);
    }
}