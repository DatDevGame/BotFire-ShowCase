using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ABTestBonusCard", menuName = "PocketBots/ABTest/v2.0.0/ABTestBonusCard")]
public class ABTestBonusCard : GroupBasedABTestSO
{
    [Serializable]
    public class Data
    {
        public RangeFloatValue m_NumOfCardsRandomRange = new RangeFloatValue();

        public void InjectData(BonusCardDataSO dataSO)
        {
            dataSO.config.numberOfCardsRandomRange = m_NumOfCardsRandomRange;
        }
    }

    [SerializeField]
    private List<Data> m_DataGroups;
    [SerializeField]
    private BonusCardDataSO m_DataSO;

    public override void InjectData(int groupIndex)
    {
        m_DataGroups[groupIndex].InjectData(m_DataSO);
    }
}