using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ExchangeRateTableSO", menuName = "PocketBots/ExchangeRateTableSO")]
public class ExchangeRateTableSO : SingletonSO<ExchangeRateTableSO>
{
    [Flags]
    public enum ArenaFlags
    {
        Arena1 = 1 << 0,
        Arena2 = 1 << 1,
        Arena3 = 1 << 2,
        Arena4 = 1 << 3,
        All = Arena1 | Arena2 | Arena3 | Arena4,
    }
    public enum ItemType
    {
        Body,
        Front,
        Upper,
        RV,
        RVTicket,
        Coin,
        USD,
        Gem
    }
    public class TableRow
    {
        [SerializeField]
        private ItemType m_ItemType;
        [SerializeField, ShowIf("@this.m_ItemType == ItemType.Body || this.m_ItemType == ItemType.Front || this.m_ItemType == ItemType.Upper")]
        private RarityType m_RarityType;
        [SerializeField]
        private ArenaFlags m_ArenaFlags;
        [SerializeField]
        private float m_ConversionRateToGems;

        public ItemType itemType => m_ItemType;
        public RarityType rarityType => m_RarityType;
        public ArenaFlags arenaFlags => m_ArenaFlags;
        public float conversionRateToGems => m_ConversionRateToGems;
    }

    [SerializeField, TableList]
    private List<TableRow> m_ExchangeRateTableRows;

#if UNITY_EDITOR
    [Button, BoxGroup("EDITOR")]
    private void TestFunction(ItemType itemType, RarityType rarityType, ArenaFlags arenaFlags)
    {
        var exchangeRate = GetExchangeRate(itemType, rarityType, arenaFlags);
        Debug.Log($"{itemType}-{rarityType}-{arenaFlags}: {exchangeRate}");
    }
#endif

    public static float GetExchangeRate(ItemType itemType, RarityType rarityType, ArenaFlags arenaFlags)
    {
        var tableRow = Instance.m_ExchangeRateTableRows.Find(row => row.itemType == itemType && row.rarityType == rarityType && ((row.arenaFlags & arenaFlags) != 0));
        if (tableRow == null)
        {
            throw new Exception($"ConversionRate of {itemType}-{rarityType}-{arenaFlags} is not found");
        }
        return tableRow.conversionRateToGems;
    }

    public static float GetExchangeRateOfParts(ItemType itemType, RarityType rarityType, ArenaFlags arenaFlags = ArenaFlags.All)
    {
        return GetExchangeRate(itemType, rarityType, arenaFlags);
    }

    public static float GetExchangeRateOfOtherItems(ItemType itemType, ArenaFlags arenaFlags)
    {
        return GetExchangeRate(itemType, default, arenaFlags);
    }
}