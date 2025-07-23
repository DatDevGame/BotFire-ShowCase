using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GachaSystem.Core;
using UnityEngine;

[CreateAssetMenu(fileName = "PBManualGachaPack", menuName = "PocketBots/Gacha/PBManualGachaPack")]
public class PBManualGachaPack : ManualGachaPack
{
    [SerializeField]
    PBGachaPack simulationFromGachaPack;
    [SerializeField]
    protected bool isSpeedUpPack;
    public PBScriptedGachaPacks scriptedGachaPacks;

    protected ShopProductSO currentShopProductSO;

    public PBGachaPack SimulationFromGachaPack => simulationFromGachaPack;
    public ShopProductSO ShopProductSO => m_ShopProductSO;
    public override int UnlockedDuration
    {
        get => (unlockedDuration <= Const.IntValue.Invalid && simulationFromGachaPack != null) ? simulationFromGachaPack.UnlockedDuration : unlockedDuration;
        set => unlockedDuration = value;
    }
    public bool IsSpeedUpPack => isSpeedUpPack;

    private bool TryFindDuplicatePartWithHighestNumOfCards(out PBPartSO duplicatePartSO)
    {
        duplicatePartSO = null;
        float highestNumOfCards = int.MinValue;
        foreach (var item in currentShopProductSO.generalItems)
        {
            if (item.Key is PBPartSO partSO && partSO.IsUnlocked() && item.Value.value > highestNumOfCards)
            {
                highestNumOfCards = item.Value.value;
                duplicatePartSO = partSO;
            }
        }
        return duplicatePartSO != null;
    }

    private PBPartSO FindUnavailablePartWithHighestNumOfCards()
    {
        float highestNumOfCards = int.MinValue;
        PBPartSO highestNumOfCardsPartSO = null;
        foreach (var item in currentShopProductSO.generalItems)
        {
            if (item.Key is PBPartSO partSO && !partSO.IsAvailable() && item.Value.value > highestNumOfCards)
            {
                highestNumOfCards = item.Value.value;
                highestNumOfCardsPartSO = partSO;
            }
        }
        return highestNumOfCardsPartSO;
    }

    private Dictionary<ItemSO, ShopProductSO.DiscountableValue> ReplaceParts()
    {
        var replacePartTuples = new List<(PBPartSO, PBPartSO)>();
        var results = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>(currentShopProductSO.generalItems);
        // Checks whether any parts are unavailable
        if (currentShopProductSO.generalItems.Keys.Any(itemSO => itemSO is PBPartSO partSO && !partSO.IsAvailable()))
        {
            PBPartSO replacedPartSO = null;
            // At least 1 part in Queue Pool
            if (GachaPoolManager.TryDequeueAvailablePriorityPart(out replacedPartSO))
            {
                var unvailablePartSO = FindUnavailablePartWithHighestNumOfCards();
                replacePartTuples.Add((unvailablePartSO, replacedPartSO));
                // Enqueue priority part due to unvailable status
                GachaPoolManager.EnqueuePriorityPart(unvailablePartSO);
            }
            foreach (var item in results)
            {
                if (item.Key is PBPartSO unvailablePartSO &&
                    !replacePartTuples.Exists(tuple => tuple.Item1 == unvailablePartSO) &&
                    !unvailablePartSO.IsAvailable())
                {
                    RarityType rarityType = unvailablePartSO.GetRarityType();
                    replacedPartSO = null;
                    do
                    {
                        replacedPartSO = GachaPoolManager.GetRandomDuplicatePartByRarity(rarityType);
                        rarityType--;
                    } while (replacedPartSO == null);
                    replacePartTuples.Add((unvailablePartSO, replacedPartSO));
                    // Enqueue priority part due to unvailable status
                    GachaPoolManager.EnqueuePriorityPart(unvailablePartSO);
                }
            }
        }
        else
        {
            // At least 1 part in Queue Pool & 1 duplicate part in the box
            if (GachaPoolManager.TryDequeueAvailablePriorityPart(out PBPartSO replacedPartSO) &&
                TryFindDuplicatePartWithHighestNumOfCards(out PBPartSO duplicatePartSO))
            {
                replacePartTuples.Add((duplicatePartSO, replacedPartSO));
            }
        }
        foreach (var tuple in replacePartTuples)
        {
            var unvailableOrDuplicatePartSO = tuple.Item1;
            var availableOrNewPartSO = tuple.Item2;
            if (!results.ContainsKey(availableOrNewPartSO))
                results.Add(availableOrNewPartSO, results[unvailableOrDuplicatePartSO]);
            else
                results[availableOrNewPartSO] =
                    new ShopProductSO.DiscountableValue()
                    {
                        value = results[availableOrNewPartSO].value +
                                                                    results[unvailableOrDuplicatePartSO].value
                    };
            results.Remove(unvailableOrDuplicatePartSO);
            LGDebug.Log($"Replace {unvailableOrDuplicatePartSO} by {availableOrNewPartSO}");
        }
        return results;
    }

    public override List<GachaCard> GenerateCards()
    {
        currentShopProductSO = m_ShopProductSO;
        if (scriptedGachaPacks != null)
        {
            currentShopProductSO = scriptedGachaPacks.GetCurrentShopProductSO(this);
            scriptedGachaPacks.IncreasePackTypeCurrentIndex(this);
        }
        if (currentShopProductSO.generalItems == null)
            return GachaCardGenerator.Instance.Generate(currentShopProductSO);

        var rewardGroupInfo =
            new RewardGroupInfo()
            {
                currencyItems = currentShopProductSO.currencyItems,
                generalItems = ReplaceParts(),
                consumableItems = currentShopProductSO.consumableItems
            };
        return (PBGachaCardGenerator.Instance as PBGachaCardGenerator).Generate(rewardGroupInfo);
    }
}