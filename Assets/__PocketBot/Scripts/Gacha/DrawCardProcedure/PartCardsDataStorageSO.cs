using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "PartCardsDataStorageSO", menuName = "PocketBots/DrawCardProcedure/PartCardsDataStorageSO")]
public class PartCardsDataStorageSO : SavedDataSO<PartCardsDataStorageSO.PersistentData>
{
    [Serializable]
    public class PersistentData : SavedData
    {
        [SerializeField]
        private List<DrawCardProcedure.PartCard> m_PartCards;

        public List<DrawCardProcedure.PartCard> partCards
        {
            get => m_PartCards;
            set => m_PartCards = value;
        }
    }

    protected static Dictionary<string, PBPartSO> s_IdPartSODictionary = new Dictionary<string, PBPartSO>();

    [SerializeField]
    protected int m_DefaultPartCardArrLength = 2;
    [SerializeField]
    protected List<PBPartManagerSO> m_PartManagerSOs;

#if UNITY_EDITOR
    [ShowInInspector, BoxGroup("EDITOR")]
    private PersistentData editorData => m_Data;
#endif

    public override PersistentData defaultData
    {
        get
        {
            var persistentData = new PersistentData()
            {
                partCards = new List<DrawCardProcedure.PartCard>(new DrawCardProcedure.PartCard[m_DefaultPartCardArrLength])
            };
            return persistentData;
        }
    }

    public override void Load()
    {
        base.Load();
        foreach (var partManagerSO in m_PartManagerSOs)
        {
            var partSOs = partManagerSO.Parts;
            foreach (var partSO in partSOs)
            {
                if (!s_IdPartSODictionary.ContainsKey(partSO.guid))
                    s_IdPartSODictionary.Add(partSO.guid, partSO);
            }
        }
        if (m_Data.partCards != null && m_Data.partCards.Count > 0)
        {
            var partCards = new List<DrawCardProcedure.PartCard>(m_Data.partCards);
            for (int i = 0; i < partCards.Count; i++)
            {
                var partCard = partCards[i];
                if (partCard != null)
                {
                    partCard.partSO = s_IdPartSODictionary.Get(partCard.partId);
                    // Handle rare case which this itemSO was removed out of game by design
                    if (partCard.partSO == null)
                    {
                        m_Data.partCards[i] = null;
                    }
                }
            }
        }
    }
}