using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HyrphusQ.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

public class DrawCardProcedure : Singleton<DrawCardProcedure>
{
    public enum GroupType
    {
        NewAvailable,
        Duplicate,
        InUsed
    }
    [Serializable]
    public class Group : IRandomizable
    {
        [SerializeField]
        private GroupType m_GroupType;
        [SerializeField]
        private float m_Probability;

        public GroupType GroupType { get => m_GroupType; set => m_GroupType = value; }
        public float Probability { get => m_Probability; set => m_Probability = value; }
        public RarityType HighestRarityType => PartSOs == null || PartSOs.Count <= 0 ? (RarityType)(-1) : PartSOs[0].GetRarityType();
        public List<PBPartSO> PartSOs { get; set; }

        public Group DeepClone()
        {
            var clonedGroup = new Group()
            {
                GroupType = GroupType,
                Probability = Probability,
                PartSOs = PartSOs == null ? null : new List<PBPartSO>(PartSOs),
            };
            return clonedGroup;
        }
    }
    [Serializable]
    public class PartCard
    {
        public PartCard(PBPartSO partSO, int numOfCards, GroupType groupType)
        {
            this.partSO = partSO;
            this.numOfCards = numOfCards;
            this.groupType = groupType;
        }

        [SerializeField]
        private int m_NumOfCards;
        [SerializeField]
        private string m_PartId;
        [SerializeField]
        private GroupType m_GroupType;
        [NonSerialized, ShowInInspector]
        private PBPartSO m_PartSO;

        public int numOfCards
        {
            get => m_NumOfCards;
            set => m_NumOfCards = value;
        }
        public string partId
        {
            get => m_PartId;
        }
        public GroupType groupType
        {
            get => m_GroupType;
            set => m_GroupType = value;
        }
        public PBPartSO partSO
        {
            get => m_PartSO;
            set
            {
                m_PartSO = value;
                m_PartId = value.guid;
            }
        }
    }
    public interface ICardIAPProduct
    {
        float GetPrice();
        float GetBonusRate();
        float GetNewCardRate();
    }

    [SerializeField]
    private DrawCardProcedureDataSO m_DrawCardProcedureDataSO;
    [SerializeField]
    private BonusCardDataSO m_BonusCardDataSO;
    [SerializeField]
    private HotOffersDataSO m_HotOffersDataSO;
    [SerializeField]
    private DailyDealsDataSO m_DailyDealsDataSO;

#if UNITY_EDITOR
    [FoldoutGroup("Info"), ShowInInspector]
    private List<PBPartSO> newAvailableGroup => !Application.isPlaying ? null : GetNewAvailableGroup();
    [FoldoutGroup("Info"), ShowInInspector]
    private List<PBPartSO> filteredNewAvailableGroup => !Application.isPlaying ? null : FilterParts(GetNewAvailableGroup(), currentUltraBox.FoundInArena);
    [FoldoutGroup("Info"), ShowInInspector]
    private List<PBPartSO> duplicateGroup => !Application.isPlaying ? null : GetDuplicateGroup();
    [FoldoutGroup("Info"), ShowInInspector]
    private List<PBPartSO> filteredDuplicateGroup => !Application.isPlaying ? null : FilterParts(GetDuplicateGroup(), currentUltraBox.FoundInArena);
    [FoldoutGroup("Info"), ShowInInspector]
    private List<PBPartSO> inUsedGroup => !Application.isPlaying ? null : GetInUsedGroup();
    [FoldoutGroup("Info"), ShowInInspector]
    private List<PBPartSO> filteredInUsedGroup => !Application.isPlaying ? null : FilterParts(GetInUsedGroup(), currentUltraBox.FoundInArena);
    [FoldoutGroup("Info"), ShowInInspector]
    private PBGachaPack currentUltraBox => m_DrawCardProcedureDataSO.currentUltraGachaBox;
    [FoldoutGroup("Info"), ShowInInspector, TableMatrix]
    private List<PBGachaPack.GearDropRate> gearDropRates => m_DrawCardProcedureDataSO.currentUltraGachaBox.GetGearDropRates();
#endif

    private string GetLogMessage(List<PBPartSO> partSOs)
    {
        var stringBuilder = new StringBuilder();
        foreach (var partSO in partSOs)
        {
            stringBuilder.Append($"{partSO.GetInternalName()} ");
        }
        return stringBuilder.ToString();
    }

    private void Log(string message, string tag, bool moreInfo = false, UnityEngine.Object context = null)
    {
        if (!GameDataSO.Instance.isDevMode)
            return;
        if (moreInfo)
        {
            var currentUltraBox = m_DrawCardProcedureDataSO.currentUltraGachaBox;
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(message);
            stringBuilder.AppendLine($"NewAvailableGroup: {GetLogMessage(GetNewAvailableGroup())}");
            stringBuilder.AppendLine($"\t*FilterNewAvailableGroup: {GetLogMessage(FilterParts(GetNewAvailableGroup(), currentUltraBox.FoundInArena))}");
            stringBuilder.AppendLine($"DuplicateGroup: {GetLogMessage(GetDuplicateGroup())}");
            stringBuilder.AppendLine($"\t*FilterDuplicateGroup: {GetLogMessage(FilterParts(GetDuplicateGroup(), currentUltraBox.FoundInArena))}");
            stringBuilder.AppendLine($"InUsedGroup: {GetLogMessage(GetInUsedGroup())}");
            stringBuilder.AppendLine($"\t*FilterInUsedGroup: {GetLogMessage(FilterParts(GetInUsedGroup(), currentUltraBox.FoundInArena))}");
            message = stringBuilder.ToString();
        }
        Debug.LogWarning(message, context);
    }

    private List<PBPartSO> GetNewAvailableGroup(List<PBPartSO> excludedParts = default, Predicate<PBPartSO> predicate = null)
    {
        var newAvailableGroup = new List<PBPartSO>();
        foreach (var partManagerSO in m_DrawCardProcedureDataSO.partManagerSOs)
        {
            var partSOs = partManagerSO.Parts;
            foreach (var partSO in partSOs)
            {
                if (!partSO.IsUnlocked() && partSO.IsAvailable() && (predicate?.Invoke(partSO) ?? true))
                    newAvailableGroup.Add(partSO);
            }
        }
        if (excludedParts != null)
            newAvailableGroup = newAvailableGroup.Except(excludedParts).ToList();
        return newAvailableGroup;
    }

    private List<PBPartSO> GetDuplicateGroup(List<PBPartSO> excludedParts = default, Predicate<PBPartSO> predicate = null)
    {
        var duplicateGroup = new List<PBPartSO>();
        foreach (var partManagerSO in m_DrawCardProcedureDataSO.partManagerSOs)
        {
            var partSOs = partManagerSO.Parts;
            foreach (var partSO in partSOs)
            {
                if (partSO.IsUnlocked() && (predicate?.Invoke(partSO) ?? true))
                    duplicateGroup.Add(partSO);
            }
        }
        if (excludedParts != null)
            duplicateGroup = duplicateGroup.Except(excludedParts).ToList();
        return duplicateGroup;
    }

    private List<PBPartSO> GetInUsedGroup(List<PBPartSO> excludedParts = default, Predicate<PBPartSO> predicate = null)
    {
        var inUsedGroup = new List<PBPartSO>();
        var inUsedChassisSO = m_DrawCardProcedureDataSO.partManagerSOs.Find(partManagerSO => partManagerSO.PartType == PBPartType.Body).CurrentPartInUse.Cast<PBChassisSO>();
        if (inUsedChassisSO.IsSpecial)
            return inUsedGroup;
        foreach (var partSlot in inUsedChassisSO.AllPartSlots)
        {
            var partSO = partSlot.PartVariableSO.value?.Cast<PBPartSO>();
            // Ignore Wheels only take Front & Upper into account
            if (partSlot.PartType != PBPartType.Wheels && partSO != null && (predicate?.Invoke(partSO) ?? true))
                inUsedGroup.Add(partSO);
        }
        if (predicate?.Invoke(inUsedChassisSO) ?? true)
            inUsedGroup.Add(inUsedChassisSO);
        if (excludedParts != null)
            inUsedGroup = inUsedGroup.Except(excludedParts).ToList();
        return inUsedGroup;
    }

    private List<PBPartSO> FilterParts(List<PBPartSO> partSOs, int foundInArena)
    {
        if (partSOs.Count <= 0)
            return partSOs;
        partSOs = partSOs.Where(partSO => partSO.foundInArena <= foundInArena).ToList();
        if (partSOs.Count <= 0)
            return partSOs;
        var highestRarityType = partSOs.Max(partSO => partSO.GetRarityType());
        return partSOs.Where(partSO => partSO.GetRarityType() == highestRarityType).ToList();
    }

    private void UpdateProbabilityOfGroups(List<Group> groups, Action<List<Group>> adjustProbabilityOfGroupsCallback)
    {
        if (adjustProbabilityOfGroupsCallback != null)
        {
            adjustProbabilityOfGroupsCallback.Invoke(groups);
        }
        else
        {
            var highestRarityGroup = groups.Max(group => group.HighestRarityType);
            foreach (var group in groups)
            {
                if (group.HighestRarityType < highestRarityGroup)
                {
                    group.Probability /= 2f;
                }
            }
        }
    }

    private List<PBPartSO> GetExcludedPartsFromPartCards(List<PartCard> partCards, int myIndex)
    {
        var excludedParts = new List<PBPartSO>();
        for (int i = 0, length = partCards.Count; i < length; i++)
        {
            if (i == myIndex || partCards[i] == null)
                continue;
            excludedParts.Add(partCards[i].partSO);
        }
        return excludedParts;
    }

    private PartCard GenerateHotOffersCard(PBGachaPack refGachaPack, List<PBPartSO> excludedParts, int priceType, int requiredRV, float requiredNumOfGems, float bonusRate, float newCardRate)
    {
        var randomPartSO = GenerateRandomPart(refGachaPack, out GroupType groupType, excludedParts);
        if (randomPartSO == null)
        {
            Log($"Generate HotOffers card failed", nameof(DrawCardProcedure), true);
            return null;
        }
        // Convert the price to the amount of gems (based on priceType)
        var numOfGems = CalculateNumOfGems();
        // Determine the number of cards based on the part type and the rarity
        var numOfCards = CalculateNumOfCards();
        Log($"Generate HotOffers successfully {randomPartSO}-{numOfCards} from {refGachaPack}", nameof(DrawCardProcedure), true, randomPartSO);
        return new PartCard(randomPartSO, numOfCards, groupType);

        float CalculateNumOfGems()
        {
            var rvExchangeRate = ExchangeRateTableSO.GetExchangeRateOfOtherItems(ExchangeRateTableSO.ItemType.RV, ExchangeRateTableSO.ArenaFlags.All);
            // 0 is RV
            // 1 is Gem
            if (priceType == 0)
                return requiredRV * rvExchangeRate * bonusRate;
            else
                return requiredNumOfGems * bonusRate;
        }
        int CalculateNumOfCards()
        {
            var partExchangeRate = ExchangeRateTableSO.GetExchangeRateOfParts(randomPartSO.PartType.ToExchangeItemType(), randomPartSO.GetRarityType());
            if (!randomPartSO.IsUnlocked())
                return Mathf.RoundToInt(numOfGems / (partExchangeRate * newCardRate));
            else
                return Mathf.RoundToInt(numOfGems / partExchangeRate);
        }
    }

    public PBPartSO GenerateRandomPart(PBGachaPack refGachaPack, out GroupType groupType, List<PBPartSO> excludedParts = default, Predicate<PBPartSO> preFilterPredicate = null, Action<List<Group>> adjustProbabilityOfGroupsCallback = null)
    {
        // Step 1: Determine 3 groups
        var newAvailableGroup = GetNewAvailableGroup(excludedParts, preFilterPredicate);
        var duplicateGroup = GetDuplicateGroup(excludedParts, preFilterPredicate);
        var inUsedGroup = GetInUsedGroup(excludedParts, preFilterPredicate);
        // Step 2: Filter parts
        var filteredNewAvaiableGroup = FilterParts(newAvailableGroup, refGachaPack.FoundInArena);
        var filteredDuplicateGroup = FilterParts(duplicateGroup, refGachaPack.FoundInArena);
        var filteredInUsedGroup = FilterParts(inUsedGroup, refGachaPack.FoundInArena);
        // Step 3: Update probabilites of groups
        var groups = m_DrawCardProcedureDataSO.config.CloneDefaultGroupsRngInfo(filteredNewAvaiableGroup, filteredDuplicateGroup, filteredInUsedGroup);
        UpdateProbabilityOfGroups(groups, adjustProbabilityOfGroupsCallback);
        // Step 4: Randomly select group
        var selectedGroup = groups.GetRandomRedistribute();
        groupType = selectedGroup.GroupType;
        Log($"Select group: {selectedGroup.GroupType}", nameof(DrawCardProcedure));
        // Step 5: Select a part by randomizing with the selected rarity based on their probability in the reference gacha box.
        var partDropRates = refGachaPack.GetGearDropRates().Where(gearDropRate => selectedGroup.PartSOs.Contains(gearDropRate.PartSO)).ToList();
        Log($"DropRates count: {partDropRates.Count} from {refGachaPack}", nameof(DrawCardProcedure));
        if (partDropRates.Count <= 0)
        {
            Log($"DropRates of {refGachaPack} does not contain {partDropRates.Count}", nameof(DrawCardProcedure));
            return null;
        }
        return partDropRates.GetRandomRedistribute().PartSO;
    }

    public PartCard GenerateBonusCard(PBGachaPack refGachaPack)
    {
        // The Bonus Card must be different from the Bonus Card in the previous N boxes
        var lastBonusParts = m_BonusCardDataSO.GetLastBonusParts();
        var bonusPart = GenerateRandomPart(refGachaPack, out GroupType groupType, lastBonusParts, adjustProbabilityOfGroupsCallback: UpdateProbabilityOfGroups);
        if (bonusPart == null)
        {
            Log($"Generate BonusCard failed", nameof(DrawCardProcedure));
            return null;
        }
        var config = m_BonusCardDataSO.config;
        var numOfCards = Mathf.RoundToInt(UnityEngine.Random.Range(config.numberOfCardsRandomRange.minValue, config.numberOfCardsRandomRange.maxValue) * refGachaPack.TotalCardsCount);
        Log($"Generate BonusCard successfully {bonusPart}-{numOfCards} from {refGachaPack}", nameof(DrawCardProcedure), bonusPart);
        return new PartCard(bonusPart, numOfCards, groupType);

        void UpdateProbabilityOfGroups(List<Group> groups)
        {
            var newAvaiableGroup = groups.Find(group => group.GroupType == GroupType.NewAvailable);
            if (!m_BonusCardDataSO.IsBonusCardClaimed() && newAvaiableGroup != null)
            {
                // If the New Available Group is not empty, user will draw a new card with 100% probability until the first bonus card is claimed
                foreach (var group in groups)
                {
                    group.Probability = group == newAvaiableGroup ? 1f : 0f;
                }
            }
            else
            {
                this.UpdateProbabilityOfGroups(groups, null);
            }
        }
    }

    public PartCard GetHotOffersPartCardRV()
    {
        var partCards = m_HotOffersDataSO.data.partCards;
        if (!partCards.IsValidIndex(0))
            return null;
        return partCards[0];
    }

    public PartCard GetHotOffersPartCardGem()
    {
        var partCards = m_HotOffersDataSO.data.partCards;
        if (!partCards.IsValidIndex(1))
            return null;
        return partCards[1];
    }

    public PartCard GenerateHotOffersCardRV(int requiredRV, float bonusRate, float newCardRate)
    {
        var refGachaPack = m_DrawCardProcedureDataSO.currentUltraGachaBox;
        var excludedParts = GetExcludedPartsFromPartCards(m_HotOffersDataSO.data.partCards, 0);
        var partCard = GenerateHotOffersCard(refGachaPack, excludedParts, 0, requiredRV, Const.IntValue.Invalid, bonusRate, newCardRate);
        m_HotOffersDataSO.data.partCards[0] = partCard;
        return partCard;
    }

    public PartCard GenerateHotOffersCardGem(float requiredNumOfGems, float bonusRate, float newCardRate)
    {
        var refGachaPack = m_DrawCardProcedureDataSO.currentUltraGachaBox;
        var excludedParts = GetExcludedPartsFromPartCards(m_HotOffersDataSO.data.partCards, 1);
        var partCard = GenerateHotOffersCard(refGachaPack, excludedParts, 1, Const.IntValue.Invalid, requiredNumOfGems, bonusRate, newCardRate);
        m_HotOffersDataSO.data.partCards[1] = partCard;
        return partCard;
    }

    public void ResetHotOffersCardRV()
    {
        m_HotOffersDataSO.data.partCards[0] = null;
    }

    public void ResetHotOffersCardGem()
    {
        m_HotOffersDataSO.data.partCards[1] = null;
    }

    public PartCard GenerateDailyDealsCard(int index, Func<PBPartSO, ICardIAPProduct> randomProductFunc)
    {
        // Step 3: Determine specific cards for each part item
        var partCards = m_DailyDealsDataSO.data.partCards;
        var isSamePartTypeExistsTwice = IsSamePartTypeExistsTwice(out PBPartType ignorePartType);
        var excludedParts = GetExcludedPartsFromPartCards(partCards, index);
        var refGachaPack = m_DrawCardProcedureDataSO.currentUltraGachaBox;
        var partSO = GenerateRandomPart(refGachaPack, out GroupType groupType, excludedParts, FilterSamePart);
        if (partSO == null)
        {
            Log($"Generate DailyDeals card at index {index} failed", nameof(DrawCardProcedure));
            partCards[index] = null;
            return null;
        }
        // Step 4: Randomly select a price from the list of available prices corresponding the part type in Product Table
        var product = randomProductFunc.Invoke(partSO);
        // Step 5: Determine the number of cards based on the part type and rarity
        var numOfGems = CalculateNumOfGems();
        var numOfCards = CalculateNumOfCards();
        partCards[index] = new PartCard(partSO, numOfCards, groupType);
        Log($"Generate DailyDeals card successfully {partSO}-{numOfCards} from {refGachaPack} at index {index}", nameof(DrawCardProcedure), partSO);
        return partCards[index];

        bool IsSamePartTypeExistsTwice(out PBPartType partType)
        {
            // Check whether there are two parts of the same part type.
            for (int i = 0; i < partCards.Count - 1; i++)
            {
                for (int j = i + 1; j < partCards.Count; j++)
                {
                    var cardA = partCards[i];
                    var cardB = partCards[j];
                    if (cardA != null && cardB != null && cardA.partSO.PartType == cardB.partSO.PartType)
                    {
                        partType = cardA.partSO.PartType;
                        return true;
                    }
                }
            }
            partType = (PBPartType)(-1);
            return false;
        }
        bool FilterSamePart(PBPartSO partSO)
        {
            if (isSamePartTypeExistsTwice)
                return partSO.PartType != ignorePartType;
            return true;
        }
        float CalculateNumOfGems()
        {
            var price = product.GetPrice();
            var bonusRate = product.GetBonusRate();
            var usdExchangeRate = ExchangeRateTableSO.GetExchangeRateOfOtherItems(ExchangeRateTableSO.ItemType.USD, ExchangeRateTableSO.ArenaFlags.All);
            return price * usdExchangeRate * bonusRate;
        }
        int CalculateNumOfCards()
        {
            var newCardRate = product.GetNewCardRate();
            var partExchangeRate = ExchangeRateTableSO.GetExchangeRateOfParts(partSO.PartType.ToExchangeItemType(), partSO.GetRarityType());
            if (!partSO.IsUnlocked())
                return Mathf.RoundToInt(numOfGems / (partExchangeRate * newCardRate));
            else
                return Mathf.RoundToInt(numOfGems / partExchangeRate);
        }
    }

    public List<PartCard> GenerateDailyDealsCards(Func<PBPartSO, ICardIAPProduct> randomProductFunc)
    {
        var partCards = m_DailyDealsDataSO.data.partCards;
        var nonNullCardIndex = 0;
        for (int i = 0; i < partCards.Count; i++)
        {
            // Reset part card to null
            partCards[i] = null;
            // Generate and assigns part card to data storage
            var partCard = GenerateDailyDealsCard(nonNullCardIndex, randomProductFunc);
            if (partCard != null)
            {
                nonNullCardIndex++;
            }
        }
        return partCards;
    }

    public List<PartCard> GetDailyDealsCards()
    {
        return m_DailyDealsDataSO.data.partCards;
    }
}