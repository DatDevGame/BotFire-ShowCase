using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GachaSystem.Core;
using HyrphusQ.SerializedDataStructure;
using UnityEngine;

[CreateAssetMenu(fileName = "BattleRoyaleLeaderboardDataSO", menuName = "PocketBots/PvP/BattleRoyaleLeaderboardDataSO")]
public class BattleRoyaleLeaderboardDataSO : ScriptableObject
{
    [Serializable]
    public class Config
    {
        [SerializeField]
        private Color m_WinTrophyTextColor = Color.black;
        [SerializeField]
        private Color m_LoseTrophyTextColor = Color.red;
        [SerializeField]
        private SerializedDictionary<int, Sprite> m_RankingToIconDictionary;
        [SerializeField]
        private SerializedDictionary<int, Color> m_RankingToTextColorDictionary;
        [SerializeField]
        private SerializedDictionary<GachaPackRarity, Sprite> m_RarityToBoxIconDictionary;

        public Color winTrophyTextColor => m_WinTrophyTextColor;
        public Color loseTrophyTextColor => m_LoseTrophyTextColor;
        public SerializedDictionary<int, Sprite> rankingToIconDictionary => m_RankingToIconDictionary;
        public SerializedDictionary<int, Color> rankingToTextColorDictionary => m_RankingToTextColorDictionary;

        public Sprite GetBoxIcon(GachaPack gachaPack)
        {
            var originGachaPack = gachaPack;
            if (gachaPack is PBManualGachaPack manualGachaPack)
            {
                originGachaPack = manualGachaPack.SimulationFromGachaPack;
            }
            return m_RarityToBoxIconDictionary.Get(GetRarityByPack(originGachaPack));

            GachaPackRarity GetRarityByPack(GachaPack gachaPack)
            {
                var packName = gachaPack.name.ToLower();
                if (packName.Contains("classic"))
                    return GachaPackRarity.Classic;
                else if (packName.Contains("great"))
                    return GachaPackRarity.Great;
                else
                    return GachaPackRarity.Ultra;
            }
        }

        public Sprite GetRandomBoxIcon()
        {
            return m_RarityToBoxIconDictionary.Values.ToList().GetRandom();
        }
    }
    [SerializeField]
    private Config m_Config;

    public Config config => m_Config;
}