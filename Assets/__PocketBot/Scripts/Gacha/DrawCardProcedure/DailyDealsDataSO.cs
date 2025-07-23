using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.SerializedDataStructure;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using UnityEngine;
using static DrawCardProcedure;
using HyrphusQ.Helpers;

[Serializable]
public class DailyDealsProduct : DrawCardProcedure.ICardIAPProduct, IRandomizable
{
    [SerializeField]
    private IAPProductSO m_ProductSO;
    [SerializeField, ShowIf("itemType", ItemType.Part)]
    private PBPartType m_PartType;
    [SerializeField, Range(0, 1)]
    private float m_Probability = 0.5f;
    [SerializeField, ShowIf("itemType", ItemType.Part)]
    private float m_BonusRate = 1.5f;
    [SerializeField, ShowIf("itemType", ItemType.Part)]
    private float m_NewCardRate = 2f;

    public ItemType itemType => m_ProductSO.itemType;
    public PBPartType partType => m_PartType;
    public IAPProductSO productSO => m_ProductSO;

    public float Probability
    {
        get => m_Probability;
        set => m_Probability = value;
    }

    public float GetBonusRate()
    {
        return m_BonusRate;
    }

    public float GetNewCardRate()
    {
        return m_NewCardRate;
    }

    public float GetPrice()
    {
        return m_ProductSO.price;
    }
}
[CreateAssetMenu(fileName = "DailyDealsDataSO", menuName = "PocketBots/DrawCardProcedure/DailyDealsDataSO")]
public class DailyDealsDataSO : PartCardsDataStorageSO
{
    [Serializable]
    public class Config
    {
        [SerializeField]
        private int m_ShowAtTrophy = 200;
        [SerializeField]
        private List<ItemSet> m_ItemSets;
        [SerializeField]
        private List<DailyDealsProduct> m_DealProducts;
        [SerializeField]
        private SerializedDictionary<RarityType, Sprite> m_RarityToCardIconDictionary;

        public int showAtTrophy => m_ShowAtTrophy;
        public List<ItemSet> itemSets => m_ItemSets;
        public List<DailyDealsProduct> dealProducts => m_DealProducts;

        public Sprite GetCardIconByRarity(RarityType rarityType)
        {
            return m_RarityToCardIconDictionary.Get(rarityType);
        }
    }
    [Serializable]
    public class ItemSet : IRandomizable
    {
        [SerializeField, Range(0, 1)]
        private float m_Probability = 0.25f;
        [SerializeField]
        private List<ItemType> m_ItemTypes;

        public float Probability
        {
            get => m_Probability;
            set => m_Probability = value;
        }
        public List<ItemType> itemTypes => m_ItemTypes;
    }

    [SerializeField]
    private Config m_Config;
    [SerializeField]
    private PPrefFloatVariable m_HighestAchievedMedalVar;
    [SerializeField]
    private DayBasedRewardSO m_NextDayRewardSO;
    [SerializeField]
    private IAPProductSOContainer m_ProductSOContainer;
    [NonSerialized]
    private List<DailyDealsItem> m_DealItems;

    public Config config => m_Config;
    public List<DailyDealsItem> dealItems
    {
        get
        {
            if (m_DealItems == null)
            {
                ES3Settings settings = new ES3Settings(m_SaveFileName, m_EncryptionType, k_EncryptionPassword);
                m_DealItems = ES3.Load("DealsItems", new List<DailyDealsItem>(), settings);
                foreach (var dailyDealsItem in m_DealItems)
                {
                    if (dailyDealsItem == null)
                        continue;
                    dailyDealsItem.dailyDealsDataSO = this;
                    dailyDealsItem.onClaimed += SaveDailyDealsItem;
                }
            }
            return m_DealItems;
        }
        set
        {
            if (value == null)
                return;
            List<DailyDealsItem> dealItems = value;
            if (m_DealItems != value)
            {
                for (int i = 0; i < dealItems.Count; i++)
                {
                    dealItems[i].dailyDealsDataSO = this;
                    dealItems[i].onClaimed += SaveDailyDealsItem;
                }
            }
            m_DealItems = value;
            SaveDailyDealsItem();
        }
    }

    private void SaveDailyDealsItem()
    {
        ES3Settings settings = new ES3Settings(m_SaveFileName, m_EncryptionType, k_EncryptionPassword);
        ES3.Save("DealsItems", m_DealItems, settings);
    }

    public bool IsAbleToShow()
    {
        return m_HighestAchievedMedalVar.value >= config.showAtTrophy;
    }

    public void ResetDailyDeals()
    {
        List<ItemType> itemTypes = config.itemSets.GetRandomRedistribute().itemTypes;
        List<ItemType> currencyItems = itemTypes.FindAll(x => x == ItemType.Gem || x == ItemType.RVTicket);
        List<ItemType> partItems = itemTypes.FindAll(x => x == ItemType.Part);
        List<DailyDealsProduct> randomPartDealProducts = new List<DailyDealsProduct>();
        List<DailyDealsItem> dealItems = new List<DailyDealsItem>(new DailyDealsItem[itemTypes.Count]);
        List<PartCard> partCards = DrawCardProcedure.Instance.GenerateDailyDealsCards(GetRandomCardProduct);
        for (int i = 0; i < currencyItems.Count; i++)
        {
            dealItems[i] = new DailyDealsItem
            {
                itemType = currencyItems[i],
                isClaimed = false,
                partCardIndex = -1
            };
            var dealProducts = config.dealProducts;
            var randomDealProduct = dealProducts.Where(product => product.itemType == currencyItems[i]).ToList().GetRandomRedistribute();
            dealItems[i].dealProductIndex = dealProducts.IndexOf(randomDealProduct);
        }
        for (int i = currencyItems.Count; i < currencyItems.Count + partItems.Count; i++)
        {
            dealItems[i] = new DailyDealsItem
            {
                itemType = ItemType.Part,
                isClaimed = false,
                partCardIndex = i - currencyItems.Count
            };
            var dealProducts = config.dealProducts;
            var randomDealProduct = randomPartDealProducts.IsValidIndex(i - currencyItems.Count) ? randomPartDealProducts[i - currencyItems.Count] : null;
            dealItems[i].dealProductIndex = randomDealProduct == null ? -1 : dealProducts.IndexOf(randomDealProduct);
        }
        this.dealItems = dealItems;

        ICardIAPProduct GetRandomCardProduct(PBPartSO partSO)
        {
            var partType = partSO.PartType;
            var dealProducts = config.dealProducts.Where(cardProduct => cardProduct.itemType == ItemType.Part && cardProduct.partType == partType).ToList();
            var randomDealProduct = dealProducts.GetRandomRedistribute();
            randomPartDealProducts.Add(randomDealProduct);
            return randomDealProduct;
        }
        m_NextDayRewardSO.GetReward();
    }
}