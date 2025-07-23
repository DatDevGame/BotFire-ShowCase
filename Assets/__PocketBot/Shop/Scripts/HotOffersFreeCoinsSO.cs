using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "HotOffersFreeCoinsSO", menuName = "PocketBots/PackReward/HotOffersFreeCoinsSO")]
public class HotOffersFreeCoinsSO : HotOffersCurrencyPackSO
{
    [SerializeField, BoxGroup("Config")] protected List<int> m_MultipWinArenas;

    public List<int> ArenaMultipliers
    {
        get => m_MultipWinArenas;
        set => m_MultipWinArenas = value;
    }

    public override int GetReward()
    {
        if (m_PBPvPTournamentSO != null)
        {
            if (m_MultipWinArenas.Count == m_PBPvPTournamentSO.arenas.Count)
            {
                return (m_CurrentHighestArenaVariable.value as PBPvPArenaSO).WonNumOfCoins * m_MultipWinArenas[m_CurrentHighestArenaVariable.value.index];
            }
        }
        return 1;
    }

#if UNITY_EDITOR
    [ShowInInspector, ReadOnly, BoxGroup("Config"), LabelText("Arena Coins Won")]
    protected string m_CoinWinArena => $"Arena {m_CurrentHighestArenaVariable.value.index + 1}: {(m_CurrentHighestArenaVariable.value as PBPvPArenaSO).WonNumOfCoins}";

    [ShowInInspector, ReadOnly, BoxGroup("Config"), LabelText("Total Free Coin Reward Following Arena")]


    protected int m_TotalRewardFreeCoinFollowingArena => GetReward();

    [OnInspectorGUI]
    protected void LoadMultipArenas()
    {
        if (m_PBPvPTournamentSO == null) return;
        if (m_MultipWinArenas == null)
            m_MultipWinArenas = new List<int>();

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
