using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatPiggyBank : MonoBehaviour
{
    public Button m_Button;
    public PiggyBankManagerSO m_PiggyBankManagerSO;
    private void Awake()
    {
        m_Button.onClick.AddListener(() =>
        {
            m_PiggyBankManagerSO.CurrentGem.value += 50;
            if(m_PiggyBankManagerSO.IsEnoughReward)
                m_PiggyBankManagerSO.CurrentGem.value = (int)m_PiggyBankManagerSO.GetPiggyBankCurrent().SavedGems;
        });
    }
}
