using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterManagerSO", menuName = "PocketBots/Characters/CharacterManagerSO")]
public class CharacterManagerSO : ItemListSO
{
    public Variable<Mode> CurrentMode => m_CurrentMode;
    public PPrefCharacterSOVariable PlayerCharacterSO => m_PlayerCharacterSO;
    public List<PPrefCharacterSOVariable> OpponentCharacterSOs => m_OpponentCharacterSOs;
    public CharacterSO BossCharacterSO => m_BossCharacterSO;

    [SerializeField, BoxGroup("Config")] private Dictionary<Mode, int> m_OppoentCounts;
    [SerializeField, BoxGroup("Data")] private Variable<Mode> m_CurrentMode;
    [SerializeField, BoxGroup("Data")] private PPrefCharacterSOVariable m_PlayerCharacterSO;
    [SerializeField, BoxGroup("Data")] private CharacterSO m_BossCharacterSO;
    [SerializeField, BoxGroup("Data")] private List<CharacterSO> m_TempOpponentSO;
    [SerializeField, BoxGroup("Data")] private List<PPrefCharacterSOVariable> m_OpponentCharacterSOs;

    public int GetOppoentCount()
    {
        if (m_CurrentMode == null || m_OppoentCounts == null || m_OppoentCounts.Count <= 0) return 0;
        return m_OppoentCounts[m_CurrentMode];
    }

    public CharacterSO GetRandomOpponentTemps()
    {
        return m_TempOpponentSO.GetRandom();
    }
}
