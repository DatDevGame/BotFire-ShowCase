using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using LatteGames.PvP;
using UnityEngine;

public class PBBoxOffers : MonoBehaviour
{
    public bool IsShop => m_isShop;

    [SerializeField]
    private PBGachaPackManagerSO m_GachaPackManagerSO;
    [SerializeField]
    protected List<GachaPackRarity> m_BoxRarities;
    [SerializeField]
    protected List<ShopProductSO> m_ProductSOs;

    protected List<ShopBuyButton> m_BuyButtons;

    [SerializeField]
    protected bool m_isShop;

    private void Awake()
    {
        m_GachaPackManagerSO.currentHighestArenaVariable.onValueChanged += OnNewArenaUnlocked;
        m_BuyButtons = GetComponentsInChildren<ShopBuyButton>().ToList();
    }

    private void Start()
    {
        UpdateProductData(m_GachaPackManagerSO.currentHighestArenaVariable.value.index);
    }

    private void OnDestroy()
    {
        m_GachaPackManagerSO.currentHighestArenaVariable.onValueChanged -= OnNewArenaUnlocked;
        ClearProductData();
    }

    private void OnNewArenaUnlocked(ValueDataChanged<PvPArenaSO> data)
    {
        UpdateProductData(m_GachaPackManagerSO.currentHighestArenaVariable.value.index);
    }

    private void UpdateProductData(int arenaIndex)
    {
        for (int i = 0; i < m_ProductSOs.Count; i++)
        {
            var boxSO = m_GachaPackManagerSO.GetGachaPackByArenaIndex(m_BoxRarities[i], arenaIndex);
            m_ProductSOs[i].generalItems.Clear();
            m_ProductSOs[i].generalItems.Set(boxSO, new ShopProductSO.DiscountableValue() { value = 1, originalValue = 1 });
            // Dirty fix null currencyProductSO when try to get currencySO
            m_BuyButtons[i].shopProductSO = m_ProductSOs[i];
            m_BuyButtons[i].OverrideSetup(m_ProductSOs[i]);
        }
    }

    private void ClearProductData()
    {
        for (int i = 0; i < m_ProductSOs.Count; i++)
        {
            m_ProductSOs[i].generalItems.Clear();
        }
    }
}