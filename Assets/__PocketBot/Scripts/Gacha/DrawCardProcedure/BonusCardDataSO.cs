using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BonusCardDataSO", menuName = "PocketBots/DrawCardProcedure/BonusCardDataSO")]
public class BonusCardDataSO : ES3ItemSOsVariable
{
    [Serializable]
    public class Config
    {
        [SerializeField]
        private int m_FirstShowAfterNBoxes = 4;
        [SerializeField]
        private int m_BonusCardUniqueInLastNBoxes = 2;
        [SerializeField]
        private float m_RerollPrice = 20f;
        [SerializeField]
        private float m_DelayTimeBeforeShowingButton = 0.5f;
        [SerializeField]
        private RangeFloatValue m_NumOfCardsRandomRange;

        public int firstShowAfterNBoxes => m_FirstShowAfterNBoxes;
        public int bonusCardUniqueInLastNBoxes => m_BonusCardUniqueInLastNBoxes;
        public float rerollPrice => m_RerollPrice;
        public float delayTimeBeforeShowingButton => m_DelayTimeBeforeShowingButton;
        public RangeFloatValue numberOfCardsRandomRange
        {
            get => m_NumOfCardsRandomRange;
            set => m_NumOfCardsRandomRange = value;
        }
    }

    [SerializeField]
    private Config m_Config;
    [SerializeField]
    private PPrefIntVariable m_NumOfOpenedBoxesVar;
    [SerializeField]
    private PPrefIntVariable m_NumOfClaimedBonusCardsVar;

    public Config config => m_Config;
    public PPrefIntVariable numOfOpenedBoxesVar => m_NumOfOpenedBoxesVar;
    public PPrefIntVariable numOfClaimedBonusCardVar => m_NumOfClaimedBonusCardsVar;

    public List<PBPartSO> GetLastBonusParts()
    {
        return GetItems<PBPartSO>();
    }

    public void AddLastBonusPart(PBPartSO partSO)
    {
        var lastBonusParts = GetLastBonusParts();
        if (lastBonusParts.Count >= config.bonusCardUniqueInLastNBoxes)
        {
            lastBonusParts.RemoveAt(0);
        }
        lastBonusParts.Add(partSO);
        value = lastBonusParts.Cast<ItemSO>().ToList();
    }

    public int GetNumOfClaimedBonusCards()
    {
        return m_NumOfClaimedBonusCardsVar.value;
    }

    public bool IsBonusCardClaimed()
    {
        return m_NumOfClaimedBonusCardsVar.value > 0;
    }

    public bool IsAbleToShowBonusCard()
    {
        return numOfOpenedBoxesVar.value >= config.firstShowAfterNBoxes;
    }
}