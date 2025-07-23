using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using UnityEngine;
using static DrawCardProcedure;

[Serializable]
public class DailyDealsItem
{
    public event Action onClaimed = delegate { };

    [field: SerializeField]
    public ItemType itemType { get; set; }
    [field: SerializeField]
    public bool isClaimed { get; set; }
    [field: SerializeField]
    public int dealProductIndex { get; set; } = -1;
    [field: SerializeField]
    public int partCardIndex { get; set; } = -1;

    public DailyDealsDataSO dailyDealsDataSO { get; set; }
    [ShowInInspector]
    public IAPProductSO productSO
    {
        get
        {
            if (dailyDealsDataSO == null)
                return null;
            var dealProducts = dailyDealsDataSO.config.dealProducts;
            if (!dealProducts.IsValidIndex(dealProductIndex))
                return null;
            return dealProducts[dealProductIndex].productSO;
        }
    }
    [ShowInInspector, ShowIf("itemType", ItemType.Part)]
    public PartCard partCard => itemType == ItemType.Part ? DrawCardProcedure.Instance?.GetDailyDealsCards()[partCardIndex] : null;

    public void Claim()
    {
        if (isClaimed)
            return;
        isClaimed = true;
        if (itemType == ItemType.Part && partCard != null)
        {
            RewardGroupInfo rewardGroupInfo = new RewardGroupInfo();
            rewardGroupInfo.generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>();
            rewardGroupInfo.generalItems.Add(partCard.partSO, new ShopProductSO.DiscountableValue()
            {
                value = partCard.numOfCards,
                originalValue = partCard.numOfCards
            });
            var cards = (PBGachaCardGenerator.Instance as PBGachaCardGenerator).Generate(rewardGroupInfo);
            GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, cards, null, null);
        }
        onClaimed.Invoke();
    }
}
public class PBDailyDeals : MonoBehaviour
{
    [SerializeField]
    private DailyDealsDataSO m_DailyDealsDataSO;
    [SerializeField]
    private List<PBDailyDealsCell> m_DailyDealsCells;

    [ShowInInspector]
    public List<DailyDealsItem> dealItems => m_DailyDealsDataSO.dealItems;

    private void Awake()
    {
        gameObject.SetActive(m_DailyDealsDataSO.IsAbleToShow());
        var resetStrategies = GetComponents<IResetStrategy>();
        foreach (var resetStrategy in resetStrategies)
        {
            resetStrategy.onReset += OnResetDailyDeals;
        }
        Initialize();
    }

    [Button]
    private void OnResetDailyDeals()
    {
        var isAbleToShow = m_DailyDealsDataSO.IsAbleToShow();
        if (!isAbleToShow)
            return;
        m_DailyDealsDataSO.ResetDailyDeals();
        Initialize();
    }

    private void Start()
    {
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseItemCompleted);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseItemCompleted);
    }

    private void OnPurchaseItemCompleted(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        var iapBuyButton = parameters[0] as LG_IAPButton;
        // FIXME: Dirty fix call update view all item cells because LG_IAPButton auto set buy button isInteractive = true for consumable products
        foreach (var itemCell in m_DailyDealsCells)
        {
            if (itemCell.iapButton == iapBuyButton)
                itemCell.Claim();
            itemCell.UpdateView();
        }
    }

    private void Initialize()
    {
        for (int i = 0; i < m_DailyDealsCells.Count; i++)
        {
            var dealItem = m_DailyDealsDataSO.dealItems.IsValidIndex(i) ? m_DailyDealsDataSO.dealItems[i] : null;
            m_DailyDealsCells[i].Initialize(m_DailyDealsDataSO, dealItem);
        }
    }
}