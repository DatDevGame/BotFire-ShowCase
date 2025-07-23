using System;
using System.Collections.Generic;
using System.Linq;
using GachaSystem.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "PocketBots/Gacha/PBGachaPack")]
public class PBGachaPack : BaseGachaPack
{
    [BoxGroup("Pack Info"), SerializeField]
    protected int maxCardToSplitGroup = 4;
    [BoxGroup("Pack Info"), SerializeField]
    protected RangeIntValue splitGroupRandomRange = new RangeIntValue(2, 3);
    [BoxGroup("Pack Info"), SerializeField]
    protected Sprite miniThumbnailImage;

    //Active skill Cards
    [TabGroup("NewGroup", "Active Skill Card"), SerializeField, Range(0f, 1f)]
    protected float activeSkillCardProbability;
    [TabGroup("NewGroup", "Active Skill Card"), SerializeField]
    protected ActiveSkillManagerSO activeSkillManagerSO;
    [TabGroup("NewGroup", "Active Skill Card"), SerializeField]
    protected List<ActiveSkillCardQuantityDropRate> activeSkillCardQuantityDropRates = new();
    [TabGroup("NewGroup", "Active Skill Card"), SerializeField]
    protected IntVariable requiredTrophiesToUnlockActiveSkillVar;
    [TabGroup("NewGroup", "Active Skill Card"), SerializeField]
    protected FloatVariable highestAchievedTrophiesVar;

    public int FoundInArena => foundInArena;
    public Vector2 moneyAmountRange => new Vector2(moneyCardRngInfos[0].min, moneyCardRngInfos[0].max);
    public Vector2 gemAmountRange => new Vector2(gemCardRngInfos[0].min, gemCardRngInfos[0].max);
    public RarityType highestDroppableRarity
    {
        get
        {
            RarityType rarity = RarityType.Common;
            foreach (ItemCardRngInfo rngInfo in gearCardRngInfos)
            {
                if (rngInfo.Probability > 0 && rngInfo.rarity > rarity)
                {
                    rarity = rngInfo.rarity;
                }
            }
            return rarity;
        }
    }

    public override int UnlockedDuration
    {
        get => unlockedDuration;
        set => unlockedDuration = value;
    }

    public List<PBPartSO> GetAllUnlockablePartByRarity(RarityType rarity)
    {
        List<GearTypeDropRate.GearDropRate> gearDropRates = new List<GearTypeDropRate.GearDropRate>();
        foreach (var table in gearTypeDropRateTables)
        {
            gearDropRates.AddRange(
                table.dropRates.SelectMany(dropTable => dropTable.gearDropRates).
                Where(dropTable => dropTable.ItemSO.GetRarityType() == rarity && !dropTable.ItemSO.IsUnlocked() && dropTable.ItemSO.Cast<PBPartSO>().IsAvailable()));
        }
        List<PBPartSO> result = new List<PBPartSO>();
        foreach (var gearDropRate in gearDropRates)
        {
            result.Add(gearDropRate.ItemSO.Cast<PBPartSO>());
        }
        return result;
    }

    protected override List<GearTypeDropRate.GearDropRate> GetGearDropRateTableByRarity(RarityType rarity)
    {
        return adjustedGearTypeDropRateTables.
            SelectMany(table => table.dropRates).
            SelectMany(table => table.gearDropRates).
            Where(table => table.ItemSO.GetRarityType() == rarity && table.ItemSO.Cast<PBPartSO>().IsAvailable()).
            ToList();
    }

    protected override List<GachaItemSO> GetAllAvailableGearSO()
    {
        List<GachaItemSO> result = new();
        foreach (var table in gearTypeDropRateTables)
        {
            foreach (var drop in table.dropRates)
            {
                foreach (var gear in drop.gearDropRates)
                {
                    if (gear.ItemSO.foundInArena <= foundInArena && gear.ItemSO.Cast<PBPartSO>().IsAvailable())
                    {
                        result.Add(gear.ItemSO);
                    }
                }
            }
        }
        return result;
    }

    protected override List<GachaCard_GachaItem> GenerateGearCardByRarity(RarityType rarity, int cardCount)
    {
        var result = new List<GachaCard_GachaItem>();
        if (cardCount <= 0) return result;
        // Split into groups
        int groupCount = cardCount > maxCardToSplitGroup ? Random.Range(splitGroupRandomRange.minValue, splitGroupRandomRange.maxValue + 1) : 1;
        int[] groups = new int[groupCount];
        while (groupCount > 0)
        {
            var offset = groupCount > 1 ? Random.Range(0.5f, 1.5f) : 1f;
            var curGroup = Mathf.FloorToInt((float)cardCount / groupCount * offset);
            groups[groupCount - 1] = curGroup;
            cardCount -= curGroup;
            groupCount--;
        }
        // Determine which gear for each group, which means the group will only have cards of that gear
        foreach (var group in groups)
        {
            var droppedGear = GetGearDropRateTableByRarity(rarity).GetRandomRedistribute();
            var gearCard = cardTemplates.Generate<GachaCard_GachaItem>(droppedGear.ItemSO);
            for (int i = 0; i < group; i++)
            {
                result.Add(gearCard);
            }
        }
        return result;
    }

    public List<GearDropRate> GetGearDropRates()
    {
        var gearDropRates = new List<GearDropRate>();
        foreach (var dropRateTable in gearTypeDropRateTables)
        {
            foreach (var gearTypeDropRate in dropRateTable.dropRates)
            {
                foreach (var dropRate in gearTypeDropRate.gearDropRates)
                {
                    gearDropRates.Add(new GearDropRate(dropRate.ItemSO.Cast<PBPartSO>(), dropRate.Probability));
                }
            }
        }
        return gearDropRates;
    }

    public override List<GachaCard> GenerateCards()
    {
        return GenerateCards(TotalCardsCount);
    }

    public List<GachaCard> GenerateCards(int totalCardsCount, bool isRandomMoney = true, bool isRandomGem = true, bool isRandomBooster = true, bool isRandomPart = true, bool isRandomActiveSkill = true)
    {
        var result = new List<GachaCard>();
        var itemCardAmountByRarity = new Dictionary<ItemType, ItemCardCount>
        {
            { ItemType.Character, new ItemCardCount() },
            { ItemType.Gear, new ItemCardCount() }
        };
        int remainAmount = totalCardsCount;

        // Money cards
        if (isRandomMoney)
            RandomMoneyCards();

        // Gem cards
        if (isRandomGem)
            RandomGemCards();

        // Booster cards
        if (isRandomBooster)
            RandomBoosterCards();

        // Priority part cards
        if (isRandomPart)
            RandomPriorityPartCards();

        // Get gear/character cards count
        if (isRandomPart)
            CalculateItemCardAmount();

        // Gear cards
        if (isRandomPart)
            RandomGearCards();

        // Character cards
        if (isRandomPart)
            RandomCharacterCards();

        // Active skill cards
        if (isRandomActiveSkill)
            RandomActiveSkillCards();

        return result;

        void RandomMoneyCards()
        {
            if (remainAmount <= 0) return;
            var moneyCardRngInfo = moneyCardRngInfos.GetRandomRedistribute();
            if (moneyCardRngInfo != null)
            {
                result.Add(cardTemplates.Generate<GachaCard_Currency>(moneyCardRngInfo.amount, CurrencyType.Standard, ResourceLocationProvider));
                remainAmount--;
            }
        }

        void RandomGemCards()
        {
            if (remainAmount <= 0) return;
            var gemCardRngInfo = gemCardRngInfos.GetRandomRedistribute();
            if (gemCardRngInfo != null)
            {
                var randomAmount = gemCardRngInfo.amount;
                if (randomAmount <= 0)
                    return;
                result.Add(cardTemplates.Generate<GachaCard_Currency>(randomAmount, CurrencyType.Premium, ResourceLocationProvider));
                remainAmount--;
            }
        }

        void RandomBoosterCards()
        {
            if (remainAmount <= 0) return;
            var willGenerateBoosterCards = Random.value > noBoosterProb;
            if (willGenerateBoosterCards)
            {
                var boosterCardCount = Random.Range(minBoosterCardsCount, maxBoosterCardsCount + 1);
                for (int i = 0; i < boosterCardCount; i++)
                {
                    var boosterCardRngInfo = boosterCardRngInfos.GetRandomRedistribute();
                    if (boosterCardRngInfo != null)
                    {
                        result.Add(cardTemplates.Generate<GachaCard_Booster>(boosterCardRngInfo.boosterType));
                        remainAmount--;
                    }
                }
            }
        }

        void RandomPriorityPartCards()
        {
            if (remainAmount <= 0) return;
            if (!GachaPoolManager.TryDequeueAvailablePriorityPart(out PBPartSO priorityPartSO)) return;
            var numOfCards = Mathf.Min(remainAmount, Mathf.RoundToInt(Random.Range(0.2f, 0.35f) * totalCardsCount));
            remainAmount -= numOfCards;
            var priorityPartCard = cardTemplates.Generate<GachaCard_GachaItem>(priorityPartSO);
            for (int i = 0; i < numOfCards; i++)
            {
                result.Add(priorityPartCard);
            }
        }

        void CalculateItemCardAmount()
        {
            var cardRemain = remainAmount;
            var guaranteeEpicCount = GetGuaranteedCardsCount(RarityType.Epic);
            var guaranteeLegendaryCount = GetGuaranteedCardsCount(RarityType.Legendary);
            List<GachaItemSO> availableCharacterSOList = new();
            List<GachaItemSO> availableGearList = new();
            List<ItemTypeRandom> typeRngList = new()
            {
                new ItemTypeRandom() // Gear
                {
                    ItemType = ItemType.Gear,
                    Probability = gearCardProbability,
                    ItemCardRngInfos = gearCardRngInfos
                },
                new ItemTypeRandom() // Character
                {
                    ItemType = ItemType.Character,
                    Probability = characterCardProbability,
                    ItemCardRngInfos = characterCardRngInfos
                },
            };
            availableCharacterSOList = GetAllAvailableCharacterSO();

            availableGearList = GetAllAvailableGearSO();

            while (cardRemain > 0)
            {
                var rngInfos = typeRngList.GetRandomRedistribute();
                if (rngInfos != null)
                {
                    RarityType rarityType = RarityType.Common;
                    if (guaranteeLegendaryCount > 0)
                    {
                        rarityType = RarityType.Legendary;
                        guaranteeLegendaryCount--;
                    }
                    else if (guaranteeEpicCount > 0)
                    {
                        rarityType = RarityType.Epic;
                        guaranteeEpicCount--;
                    }
                    else
                    {
                        rarityType = rngInfos.ItemCardRngInfos.GetRandomRedistribute().rarity;
                    }
                    switch (rngInfos.ItemType)
                    {
                        case ItemType.Character:
                            AddCharacterCard(rarityType);
                            break;
                        case ItemType.Gear:
                            AddGearCard(rarityType);
                            break;
                    }
                    cardRemain--;

                    void AddCharacterCard(RarityType rarity)
                    {
                        while (true)
                        {
                            if (IsRarityAvailableInCharacter(rarity))
                            {
                                itemCardAmountByRarity[ItemType.Character].AddOne(rarity);
                                return;
                            }
                            else if (IsRarityAvailableInGearType(rarity))
                            {
                                itemCardAmountByRarity[ItemType.Gear].AddOne(rarity);
                                return;
                            }
                            else
                            {
                                if (rarity == RarityType.Legendary) rarity = RarityType.Epic;
                                else if (rarity == RarityType.Epic) rarity = RarityType.Common;
                                else return;
                            }
                        }
                    }

                    void AddGearCard(RarityType rarity)
                    {
                        while (true)
                        {
                            if (IsRarityAvailableInGearType(rarity))
                            {
                                itemCardAmountByRarity[ItemType.Gear].AddOne(rarity);
                                return;
                            }
                            else if (IsRarityAvailableInCharacter(rarity))
                            {
                                itemCardAmountByRarity[ItemType.Character].AddOne(rarity);
                                return;
                            }
                            else
                            {
                                if (rarity == RarityType.Legendary) rarity = RarityType.Epic;
                                else if (rarity == RarityType.Epic) rarity = RarityType.Common;
                                else return;
                            }
                        }
                    }

                    bool IsRarityAvailableInCharacter(RarityType rarityType)
                    {
                        return availableCharacterSOList.Any(character => character.GetRarityType() == rarityType);
                    }

                    bool IsRarityAvailableInGearType(RarityType rarityType)
                    {
                        return availableGearList.Any(gear => gear.GetRarityType() == rarityType);
                    }
                }
                else
                {
                    Debug.LogError("RngInfos is null");
                }
            }
        }

        void RandomGearCards()
        {
            int commonCount = itemCardAmountByRarity[ItemType.Gear].commonCount;
            int epicCount = itemCardAmountByRarity[ItemType.Gear].epicCount;
            int legendaryCount = itemCardAmountByRarity[ItemType.Gear].legendaryCount;

            // Determine which gear for each gear card
            AdjustGearDropRateTables();
            result.AddRange(GenerateGearCardByRarity(RarityType.Common, commonCount));
            result.AddRange(GenerateGearCardByRarity(RarityType.Epic, epicCount));
            result.AddRange(GenerateGearCardByRarity(RarityType.Legendary, legendaryCount));
        }

        void RandomCharacterCards()
        {
            int commonCount = itemCardAmountByRarity[ItemType.Character].commonCount;
            int epicCount = itemCardAmountByRarity[ItemType.Character].epicCount;
            int legendaryCount = itemCardAmountByRarity[ItemType.Character].legendaryCount;

            // Determine which character for each character card
            AdjustCharacterDropRateTables();
            result.AddRange(GenerateCharacterCardByRarity(RarityType.Common, commonCount));
            result.AddRange(GenerateCharacterCardByRarity(RarityType.Epic, epicCount));
            result.AddRange(GenerateCharacterCardByRarity(RarityType.Legendary, legendaryCount));
        }

        void RandomActiveSkillCards()
        {
            var willGenerateActiveSkillCards = activeSkillCardProbability > 0f && Random.value <= activeSkillCardProbability && highestAchievedTrophiesVar >= requiredTrophiesToUnlockActiveSkillVar;
            if (willGenerateActiveSkillCards)
            {
                int cardQuantity = activeSkillCardQuantityDropRates.GetRandomRedistribute().CardQuantity;
                for (int i = 0; i < cardQuantity; i++)
                {
                    result.Add(cardTemplates.Generate<GachaCard_ActiveSkill>(GetRandomActiveSkillSO()));
                }
            }
        }
    }

    public float GetDropRateByRarity(RarityType rarityType)
    {
        return gearCardRngInfos.Find(item => item.rarity == rarityType).Probability;
    }

    public Sprite GetMiniThumbnailImage()
    {
        return miniThumbnailImage;
    }

    public void SetSkillDropRate(float skillDropRate, List<int> skillCardAmounts, List<float> skillAmountDropRates)
    {
        activeSkillCardProbability = skillDropRate;
        activeSkillCardQuantityDropRates.Clear();
        for (int i = 0; i < skillCardAmounts.Count; i++)
        {
            activeSkillCardQuantityDropRates.Add(new ActiveSkillCardQuantityDropRate()
            {
                CardQuantity = skillCardAmounts[i],
                Probability = skillAmountDropRates[i]
            });
        }
    }

    public ActiveSkillSO GetRandomActiveSkillSO()
    {
        return activeSkillManagerSO.GetRandomItem().Cast<ActiveSkillSO>();
    }

    public class GearDropRate : IRandomizable
    {
        public GearDropRate(PBPartSO partSO, float probability)
        {
            this.partSO = partSO.Cast<PBPartSO>();
            this.probability = probability;
        }

        private PBPartSO partSO;
        private float probability;

        [ShowInInspector]
        public PBPartSO PartSO => partSO;
        [ShowInInspector]
        public float Probability { get => probability; set => probability = value; }
    }

    [Serializable]
    public class ActiveSkillCardQuantityDropRate : IRandomizable
    {
        [SerializeField]
        private int cardQuantity;
        [SerializeField, Range(0, 1f)]
        private float probability;

        public int CardQuantity
        {
            get => cardQuantity;
            set => cardQuantity = value;
        }
        public float Probability
        {
            get => probability;
            set => probability = value;
        }
    }
}