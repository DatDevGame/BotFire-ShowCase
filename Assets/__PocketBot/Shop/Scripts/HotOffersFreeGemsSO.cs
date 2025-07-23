using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "HotOffersFreeGemsSO", menuName = "PocketBots/PackReward/HotOffersFreeGemsSO")]
public class HotOffersFreeGemsSO : HotOffersCurrencyPackSO
{
    [SerializeField, BoxGroup("Config")] protected List<float> m_MultipWinArenas;
    [SerializeField, BoxGroup("Config")] protected int m_TheExchangeRateFromRVToGem;

    public List<float> ArenaMultipliers
    {
        get => m_MultipWinArenas;
        set => m_MultipWinArenas = value;
    }

    public override int GetReward()
    {
        int reward = (int)(m_TheExchangeRateFromRVToGem * m_MultipWinArenas[m_CurrentHighestArenaVariable.value.index]);
        if (m_MultipWinArenas.Count == m_PBPvPTournamentSO.arenas.Count)
            return reward;

        Debug.LogWarning("Warning!");
        return 1;
    }

#if UNITY_EDITOR
    [ShowInInspector, ReadOnly, BoxGroup("Config"), LabelText("Total Free Gems Reward: ")]
    protected int m_TotalRewardFreeCoinFollowingArena => GetReward();

    [OnInspectorGUI]
    protected void LoadMultipArenas()
    {
        if (m_PBPvPTournamentSO == null) return;
        if (m_MultipWinArenas == null)
            m_MultipWinArenas = new List<float>();

        for (int i = 0; i < m_PBPvPTournamentSO.arenas.Count; i++)
        {
            if (m_MultipWinArenas.Count <= m_PBPvPTournamentSO.arenas.Count)
            {
                m_MultipWinArenas.Add(0);
            }
        }
        if (m_MultipWinArenas.Count > m_PBPvPTournamentSO.arenas.Count)
            m_MultipWinArenas.RemoveAt(m_MultipWinArenas.Count - 1);
    }

#endif

}
