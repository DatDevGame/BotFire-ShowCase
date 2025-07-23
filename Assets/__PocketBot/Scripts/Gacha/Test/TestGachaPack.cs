using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GachaSystem.Core;
using UnityEngine;

[CreateAssetMenu(fileName = "TestGachaPack")]
public class TestGachaPack : GachaPack
{
    public List<ItemManagerSO> itemManagerSOs;

    public override List<GachaCard> GenerateCards()
    {
        var randomItemPool = new List<ItemSO>();
        foreach (var itemManagerSO in itemManagerSOs)
        {
            randomItemPool.AddRange(itemManagerSO.value.Where(item => item.GetMaxUpgradeLevel() > 1));
        }
        var cards = new List<GachaCard>()
        {
            cardTemplates.Generate<GachaCard_Currency>(500, CurrencyType.Standard),
            cardTemplates.Generate<GachaCard_Currency>(50, CurrencyType.Premium),
        };
        for (int i = 2; i < totalCardsCount; i++)
        {
            var item = randomItemPool.GetRandom();
            cards.Add(cardTemplates.Generate<GachaCard_Part>(item));
            randomItemPool.Remove(item);
        }
        return cards;
    }
}