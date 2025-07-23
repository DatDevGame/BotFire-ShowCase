using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

[CreateAssetMenu(fileName = "PBGachaPackManagerSO", menuName = "PocketBots/Gacha/PBGachaPackManagerSO")]
public class PBGachaPackManagerSO : SerializedScriptableObject
{
    public CurrentHighestArenaVariable currentHighestArenaVariable;
    public List<PBPartCardsSO> partCardsSOList;
    [SerializeField]
    private Dictionary<GachaPackRarity, List<PBGachaPack>> rarityToGachaPackDictionary;

    [SerializeField, BoxGroup("Skill")] protected ActiveSkillManagerSO m_ActiveSkillManagerSO;
    [SerializeField, BoxGroup("Skill")] protected GachaCard_RandomActiveSkill m_GachaCardRandomActiveSkill;

    public GachaCard_RandomActiveSkill GachaCard_RandomActiveSkill => m_GachaCardRandomActiveSkill;

    public PBGachaPack GetGachaPackCurrentArena(GachaPackRarity gachaPackRarity)
    {
        List<PBGachaPack> pBGachaPacks = rarityToGachaPackDictionary.Get(gachaPackRarity);
        if (pBGachaPacks[currentHighestArenaVariable.value.index] == null)
            return pBGachaPacks[pBGachaPacks.Count - 1];
        else
            return pBGachaPacks[currentHighestArenaVariable.value.index];
    }

    public PBGachaPack GetGachaPackByArenaIndex(GachaPackRarity gachaPackRarity, int arenaIndex)
    {
        List<PBGachaPack> pBGachaPacks = rarityToGachaPackDictionary.Get(gachaPackRarity);
        return pBGachaPacks[arenaIndex];
    }

    public PBGachaPack GetRandomGachaPack()
    {
        List<PBGachaPack> pBGachaPacks = rarityToGachaPackDictionary.Get(GetRandomGachaPackRarity());
        if (pBGachaPacks[currentHighestArenaVariable.value.index] == null)
            return pBGachaPacks[pBGachaPacks.Count - 1];
        else
            return pBGachaPacks[currentHighestArenaVariable.value.index];
    }

    public GachaPackRarity GetGachaPackRarity(PBGachaPack gachaPack)
    {
        if (gachaPack == null)
            return default;
        foreach (var keyValuePair in rarityToGachaPackDictionary)
        {
            if (keyValuePair.Value.Contains(gachaPack))
                return keyValuePair.Key;
        }
        return default;
    }

    public PBPartCardsSO GetPartCardCurrentArena()
    {
        if (partCardsSOList[currentHighestArenaVariable.value.index] == null)
            return partCardsSOList[partCardsSOList.Count - 1];
        else
            return partCardsSOList[currentHighestArenaVariable.value.index];
    }

    public ActiveSkillSO GetSkillCard(string skillName = "")
    {
        if (skillName == "")
        {
            return m_ActiveSkillManagerSO.initialValue.GetRandom().Cast<ActiveSkillSO>();
        }
        else
        {
            return m_ActiveSkillManagerSO.GetSkill(skillName);
        }
    }

    private GachaPackRarity GetRandomGachaPackRarity()
    {
        System.Random random = new System.Random();
        Array values = Enum.GetValues(typeof(GachaPackRarity));
        return (GachaPackRarity)values.GetValue(random.Next(values.Length));
    }
}

public enum GachaPackRarity
{
    Classic,
    Great,
    Ultra,
    Fortune,
    Valor,
    Prime,
    Lucky,
    Grand,
    King
}