using System;
using System.Collections.Generic;
using System.Linq;
using GachaSystem.Core;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "SeasonPassSO", menuName = "PocketBots/SeasonPass/SeasonPassSO")]
public class SeasonPassSO : SavedDataSO<SeasonPassSO.SeasonPassSavedData>, IResourceLocationProvider
{
    [Serializable]
    public class SeasonPassSavedData : SavedData
    {
        public List<MilestoneState> MilestoneStates = new();
        public SeasonPassState state;
        [SerializeField]
        private DateTime firstDayOfSeason;
        [SerializeField]
        private DateTime _passedDay;
        public DateTime passedWeek;
        public DateTime passedHalfSeason;
        public bool isPurchasedPass;
        public int todayRefreshRound = 1;
        public float seasonTokenUI;
        public int seasonTreeIndex = -1;
        public bool firstTimeEarnReward = false;
        public bool isNewUser = true;

        public DateTime FirstDayOfSeason
        {
            get => firstDayOfSeason;
            set
            {
                firstDayOfSeason = passedDay = passedWeek = passedHalfSeason = value;
                GameEventHandler.Invoke(SeasonPassEventCode.OnSetNewSeasonFirstDay);
            }
        }

        public DateTime passedDay
        {
            get => _passedDay;
            set
            {
                _passedDay = value;
                todayRefreshRound = 1;
            }
        }
        [Serializable]
        public class MilestoneState
        {
            public float requiredToken;
            public string itemGUID_Free;
            public string itemGUID_Premium;
            public Reward rewardFree;
            public Reward rewardPremium;
            public bool ClaimedFree = false;
            public bool ClaimedPremium = false;
        }
    }
    [Serializable]
    public class RewardTreeInputData
    {
        public List<MilestoneInputData> milestoneInputDataList = new();
    }
    [Serializable]
    public struct MilestoneInputData
    {
        public float requiredToken;
        public Reward rewardFree;
        public Reward rewardPremium;
    }
    [Serializable]
    public class Milestone
    {
        public static event Action<Milestone> OnClaimedAny = delegate { };
        public static event Action<Milestone> OnUnlockedAny = delegate { };

        public event Action OnUnlocked = delegate { };
        public event Action OnClaimedFree = delegate { };
        public event Action OnClaimedPremium = delegate { };
        // Props
        [NonSerialized]
        bool unlocked = false;
        public bool Unlocked
        {
            get => unlocked;
            set
            {
                unlocked = value;
                if (unlocked) OnUnlocked();
                OnUnlockedAny?.Invoke(this);
            }
        }
        [NonSerialized]
        bool claimedFree = false;
        [NonSerialized]
        bool claimedPremium = false;
        public bool ClaimedFree
        {
            get => claimedFree;
            set
            {
                claimedFree = value;
                if (claimedFree) OnClaimedFree();
                OnClaimedAny?.Invoke(this);
            }
        }
        public bool ClaimedPremium
        {
            get => claimedPremium;
            set
            {
                claimedPremium = value;
                if (claimedPremium) OnClaimedPremium();
                OnClaimedAny?.Invoke(this);
            }
        }
        public float requiredAmount;
        public Func<RewardGroupInfo> RewardFree;
        public Func<RewardGroupInfo> RewardPremium;
        public Reward freeReward;
        public Reward premiumReward;
        public IResourceLocationProvider ResourceLocationProvider { get; set; }
        // Methods
        public bool IsUnlockable(float currentAmount)
        {
            // if (ClaimedFree || ClaimedPremium) return false;
            if (Unlocked) return false;
            return currentAmount >= requiredAmount;
        }
        public bool TryUnlock(float currentAmount, bool notify = true)
        {
            // if (ClaimedFree || ClaimedPremium) return false;
            if (Unlocked) return false;
            var value = currentAmount >= requiredAmount;
            if (notify)
            {
                Unlocked = value;
            }
            else
            {
                unlocked = value;
            }
            return Unlocked;
        }
        public bool TryClaim(bool isPremium)
        {
            var cards = GetCards(isPremium);
            return TryClaimCards(cards, isPremium);
        }
        public bool TryClaimCards(List<GachaCard> cards, bool isPremium)
        {
            if (!Unlocked) return false;
            if ((isPremium && claimedPremium) || (!isPremium && claimedFree)) return false;
            foreach (var card in cards)
            {
                if (card is GachaCard_Currency gachaCard_Currency)
                {
                    //TODO: Fix ResourceLocation type (wait for design)
                    string itemID = isPremium ? "Season_PremiumRewards" : "Season_FreeRewards";
                    gachaCard_Currency.ResourceLocationProvider = new ResourceLocationProvider(ResourceLocation.Season, itemID);
                }

                if (card is GachaCard_Skin gachaCard_Skin)
                {
                    #region Progression Event
                    try
                    {
                        if (gachaCard_Skin != null && gachaCard_Skin.SkinSO != null)
                        {
                            string input = $"{gachaCard_Skin.SkinSO.name}";
                            string[] parts = input.Split('_');
                            if (parts.Length > 1 && int.TryParse(parts[1], out int skinID))
                            {
                                string status = ProgressionEventStatus.Start;
                                string partname_SkinID = $"{gachaCard_Skin.SkinSO.partSO.GetDisplayName()}-{skinID}";
                                string currentcyType = "Season";
                                GameEventHandler.Invoke(ProgressionEvent.BuySkin, status, partname_SkinID, currentcyType);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
                    }
                    #endregion
                }
            }

            #region Log GA
            try
            {
                List<GachaPack> LogGachaPack = GetPacks(isPremium);
                if (LogGachaPack != null && LogGachaPack.Count > 0)
                {
                    GetPacks(isPremium).ForEach(pack =>
                    {
                        if (pack != null)
                        {
                            #region DesignEvent
                            string openStatus = "NoTimer";
                            string location = "Season";
                            GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
                            #endregion

                            #region Firebase Event
                            GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, pack, "free");
                            #endregion
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion

            GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, cards, GetPacks(isPremium), null);
            if (isPremium)
            {
                ClaimedPremium = true;
            }
            else
            {
                ClaimedFree = true;
            }
            return true;
        }
        public List<GachaCard> GetCards(bool isPremium)
        {
            var reward = isPremium ? RewardPremium() : RewardFree();
            if (reward == null)
            {
                return null;
            }
            return (GachaCardGenerator.Instance as PBGachaCardGenerator).Generate(reward);
        }

        public List<GachaPack> GetPacks(bool isPremium)
        {
            var reward = isPremium ? RewardPremium() : RewardFree();
            if (reward == null || reward.generalItems == null)
            {
                return null;
            }

            foreach (var item in reward.generalItems)
            {
                if (item.Key is GachaPack gachaPackKey)
                {
                    return new List<GachaPack> { gachaPackKey };
                }
            }
            return null;
        }
    }

    [SerializeField] private CurrencySO _seasonCurrency;
    [Header("Resource")]
    [SerializeField] protected ResourceLocation resourceLocation;
    [SerializeField] protected string resourceItemId;
    [SerializeField, BoxGroup("Rewards")] protected PBPartManagerSO chassisManagerSO;
    [SerializeField, BoxGroup("Rewards")] protected PBPartManagerSO upperManagerSO;
    [SerializeField, BoxGroup("Rewards")] protected PBPartManagerSO frontManagerSO;
    [SerializeField, BoxGroup("Rewards")] protected PBPartManagerSO specialBotManagerSO;
    [SerializeField, BoxGroup("Rewards")] protected PBGachaPackManagerSO gachaPackManagerSO;
    [SerializeField, BoxGroup("Rewards")] protected ItemListSO rewardSpecialBots;
    [SerializeField, BoxGroup("Rewards")] protected List<SkinRngInfo> skinRngInfos_Free = new();
    [SerializeField, BoxGroup("Rewards")] protected List<SkinRngInfo> skinRngInfos_Premium = new();
    [SerializeField, BoxGroup("Rewards")] protected int freeSkinReplacePartCardAmount = 5;
    [SerializeField, BoxGroup("Rewards")] protected int premiumSkinReplacePartCardAmount = 10;
    [SerializeField, BoxGroup("Rewards")] protected int freePreBuildBotReplaceUltraBoxAmount = 1;
    [SerializeField, BoxGroup("Rewards")] protected int premiumPreBuildBotReplaceUltraBoxAmount = 2;

    [ReadOnly] private List<Milestone> _milestones = new();

    public List<RewardTreeInputData> rewardTreeInputDataList = new();

    public List<Milestone> milestones => _milestones;
    public CurrencySO seasonCurrency => _seasonCurrency;
    public bool isPurchased => data.isPurchasedPass;
    public override SeasonPassSavedData defaultData
    {
        get
        {
            var result = new SeasonPassSavedData();
            return result;
        }
    }

    public override void Load()
    {
        base.Load();
        SyncMilestoneData();
    }

    void SyncMilestoneData()
    {
        milestones.Clear();
        Milestone milestone;
        for (int i = 0, length = data.MilestoneStates.Count; i < length; i++)
        {
            milestone = new Milestone();
            var mileStoneState = data.MilestoneStates[i];
            milestone.requiredAmount = mileStoneState.requiredToken;
            milestone.freeReward = mileStoneState.rewardFree;
            milestone.premiumReward = mileStoneState.rewardPremium;
            milestone.Unlocked = false;
            milestone.TryUnlock(_seasonCurrency.value, false);
            if (data.MilestoneStates.Count <= i)
                data.MilestoneStates.Add(new());
            var savedState = data.MilestoneStates[i];
            milestone.ClaimedFree = savedState.ClaimedFree;
            milestone.ClaimedPremium = savedState.ClaimedPremium;
            milestone.ResourceLocationProvider = this;
            milestone.OnClaimedFree += () => savedState.ClaimedFree = true;
            milestone.OnClaimedPremium += () => savedState.ClaimedPremium = true;
            CreateRewardGroupInfo(true);
            CreateRewardGroupInfo(false);
            void CreateRewardGroupInfo(bool isFree)
            {
                Func<RewardGroupInfo> rewardGroupInfoFunc = null;
                var rewardGroupInfo = new RewardGroupInfo();
                var reward = isFree ? mileStoneState.rewardFree : mileStoneState.rewardPremium;
                if (reward.type == RewardType.Coins)
                {
                    AddCurrency(CurrencyType.Standard);
                }
                else if (reward.type == RewardType.Gems)
                {
                    AddCurrency(CurrencyType.Premium);
                }
                else if (reward.type == RewardType.Skip_Ads)
                {
                    AddCurrency(CurrencyType.RVTicket);
                }
                else if (reward.type == RewardType.Part_Cards)
                {
                    rewardGroupInfoFunc = () =>
                    {
                        var rewardGroupInfo = new RewardGroupInfo();
                        rewardGroupInfo.generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>
                        {
                            {
                                gachaPackManagerSO.GetPartCardCurrentArena(),
                                new ShopProductSO.DiscountableValue()
                                {
                                    value = reward.amount,
                                    originalValue = reward.amount
                                }
                            }
                        };
                        return rewardGroupInfo;
                    };
                }
                else if (reward.type == RewardType.Classic_Boxes)
                {
                    rewardGroupInfoFunc = () =>
                    {
                        var rewardGroupInfo = new RewardGroupInfo();
                        rewardGroupInfo.generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>
                        {
                            {
                                gachaPackManagerSO.GetGachaPackCurrentArena(GachaPackRarity.Classic),
                                new ShopProductSO.DiscountableValue()
                                {
                                    value = reward.amount,
                                    originalValue = reward.amount
                                }
                            }
                        };
                        return rewardGroupInfo;
                    };
                }
                else if (reward.type == RewardType.Great_Boxes)
                {
                    rewardGroupInfoFunc = () =>
                    {
                        var rewardGroupInfo = new RewardGroupInfo();
                        rewardGroupInfo.generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>
                        {
                            {
                                gachaPackManagerSO.GetGachaPackCurrentArena(GachaPackRarity.Great),
                                new ShopProductSO.DiscountableValue()
                                {
                                    value = reward.amount,
                                    originalValue = reward.amount
                                }
                            }
                        };
                        return rewardGroupInfo;
                    };
                }
                else if (reward.type == RewardType.Ultra_Boxes)
                {
                    rewardGroupInfoFunc = () =>
                    {
                        var rewardGroupInfo = new RewardGroupInfo();
                        rewardGroupInfo.generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>
                        {
                            {
                                gachaPackManagerSO.GetGachaPackCurrentArena(GachaPackRarity.Ultra),
                                new ShopProductSO.DiscountableValue()
                                {
                                    value = reward.amount,
                                    originalValue = reward.amount
                                }
                            }
                        };
                        return rewardGroupInfo;
                    };
                }
                else if (reward.type == RewardType.Skins)
                {
                    AddSkin();
                }
                else if (reward.type == RewardType.Prebuilt_bots)
                {
                    AddPreBuildBot();
                }

                if (isFree)
                {
                    milestone.RewardFree = rewardGroupInfoFunc;
                }
                else
                {
                    milestone.RewardPremium = rewardGroupInfoFunc;
                }

                void AddCurrency(CurrencyType currencyType)
                {
                    rewardGroupInfo.currencyItems = new Dictionary<CurrencyType, ShopProductSO.DiscountableValue>
                    {
                        {
                            currencyType,
                            new ShopProductSO.DiscountableValue()
                            {
                                value = reward.amount,
                                originalValue = reward.amount
                            }
                        }
                    };
                    rewardGroupInfoFunc = () => rewardGroupInfo;
                }

                void AddSkin()
                {
                    var parts = new List<ItemSO>(chassisManagerSO.value);
                    parts.AddRange(upperManagerSO.value);
                    parts.AddRange(frontManagerSO.value);
                    var skins = new List<SkinSO>();
                    foreach (var part in parts)
                    {
                        if (part.TryGetModule(out SkinItemModule skinModule))
                        {
                            skins.AddRange(skinModule.skins);
                        }
                    }
                    var skin = skins.Find(x => x.guid == (isFree ? mileStoneState.itemGUID_Free : mileStoneState.itemGUID_Premium));

                    rewardGroupInfo.generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>
                    {
                        {
                            skin,
                            new ShopProductSO.DiscountableValue()
                            {
                                value = reward.amount,
                                originalValue = reward.amount
                            }
                        }
                    };
                    rewardGroupInfoFunc = () => rewardGroupInfo;
                }

                void AddPreBuildBot()
                {
                    var bot = rewardSpecialBots.Find(x => x.guid == (isFree ? mileStoneState.itemGUID_Free : mileStoneState.itemGUID_Premium));

                    rewardGroupInfo.generalItems = new Dictionary<ItemSO, ShopProductSO.DiscountableValue>
                    {
                        {
                            bot,
                            new ShopProductSO.DiscountableValue()
                            {
                                value = reward.amount,
                                originalValue = reward.amount
                            }
                        }
                    };
                    rewardGroupInfoFunc = () => rewardGroupInfo;
                }
            }

            milestones.Add(milestone);
        }
    }

    public SeasonPassSavedData testData;
    [Button]
    public void InitRewardTree()
    {
        var rewardTree = rewardTreeInputDataList.GetRandom(x => rewardTreeInputDataList.IndexOf(x) != data.seasonTreeIndex);
        data.seasonTreeIndex = rewardTreeInputDataList.IndexOf(rewardTree);
        data.MilestoneStates.Clear();
        var random = new System.Random();

        //Get available skins for the reward tree
        bool IsValidPart(ItemSO x, RarityType rarityType)
        {
            return x.GetRarityType() == rarityType;
        }
        var commonParts = chassisManagerSO.value.FindAll(x => IsValidPart(x, RarityType.Common));
        commonParts.AddRange(upperManagerSO.value.FindAll(x => IsValidPart(x, RarityType.Common)));
        commonParts.AddRange(frontManagerSO.value.FindAll(x => IsValidPart(x, RarityType.Common)));
        var epicParts = chassisManagerSO.value.FindAll(x => IsValidPart(x, RarityType.Epic));
        epicParts.AddRange(upperManagerSO.value.FindAll(x => IsValidPart(x, RarityType.Epic)));
        epicParts.AddRange(frontManagerSO.value.FindAll(x => IsValidPart(x, RarityType.Epic)));
        var legendParts = chassisManagerSO.value.FindAll(x => IsValidPart(x, RarityType.Legendary));
        legendParts.AddRange(upperManagerSO.value.FindAll(x => IsValidPart(x, RarityType.Legendary)));
        legendParts.AddRange(frontManagerSO.value.FindAll(x => IsValidPart(x, RarityType.Legendary)));

        var commonSkins = new List<SkinSO>();
        foreach (var part in commonParts)
        {
            if (part.TryGetModule(out SkinItemModule skinModule))
            {
                commonSkins.AddRange(skinModule.skins.FindAll(s => !s.IsUnlocked()));
            }
        }
        commonSkins = commonSkins.GroupBy(x => x.GetRequiredRVCount())
            .OrderByDescending(group => group.Key)
            .SelectMany(group => group.OrderBy(_ => random.Next()))
            .ToList();

        var epicSkins = new List<SkinSO>();
        foreach (var part in epicParts)
        {
            if (part.TryGetModule(out SkinItemModule skinModule))
            {
                epicSkins.AddRange(skinModule.skins.FindAll(s => !s.IsUnlocked()));
            }
        }
        epicSkins = epicSkins.GroupBy(x => x.GetRequiredRVCount())
            .OrderByDescending(group => group.Key)
            .SelectMany(group => group.OrderBy(_ => random.Next()))
            .ToList();

        var legendSkins = new List<SkinSO>();
        foreach (var part in legendParts)
        {
            if (part.TryGetModule(out SkinItemModule skinModule))
            {
                legendSkins.AddRange(skinModule.skins.FindAll(s => !s.IsUnlocked()));
            }
        }
        legendSkins = legendSkins.GroupBy(x => x.GetRequiredRVCount())
            .OrderByDescending(group => group.Key)
            .SelectMany(group => group.OrderBy(_ => random.Next()))
            .ToList();

        //Get available pre-build bots for the reward tree
        var preBuildBots = rewardSpecialBots.value.FindAll(x => !x.IsUnlocked());

        for (int i = 0; i < rewardTree.milestoneInputDataList.Count; i++)
        {
            var milestoneInputData = rewardTree.milestoneInputDataList[i];
            var milestoneState = new SeasonPassSavedData.MilestoneState();
            milestoneState.requiredToken = milestoneInputData.requiredToken;
            milestoneState.rewardFree = milestoneInputData.rewardFree;
            milestoneState.rewardPremium = milestoneInputData.rewardPremium;
            PreGenerateSkinRewards(true);
            PreGenerateSkinRewards(false);
            PreGeneratePrebuildBotRewards(true);
            PreGeneratePrebuildBotRewards(false);
            data.MilestoneStates.Add(milestoneState);

            void PreGenerateSkinRewards(bool isFree)
            {
                var type = isFree ? milestoneState.rewardFree.type : milestoneState.rewardPremium.type;
                if (type == RewardType.Skins)
                {
                    var selectedSkinRngInfos = isFree ? skinRngInfos_Free : skinRngInfos_Premium;
                    if (commonSkins.Count <= 0)
                    {
                        selectedSkinRngInfos.Remove(selectedSkinRngInfos.Find(x => x.rarity == RarityType.Common));
                    }
                    if (epicSkins.Count <= 0)
                    {
                        selectedSkinRngInfos.Remove(selectedSkinRngInfos.Find(x => x.rarity == RarityType.Epic));
                    }
                    if (legendSkins.Count <= 0)
                    {
                        selectedSkinRngInfos.Remove(selectedSkinRngInfos.Find(x => x.rarity == RarityType.Legendary));
                    }

                    var skinRngInfo = selectedSkinRngInfos.GetRandomRedistribute();
                    if (skinRngInfo != null)
                    {
                        SkinSO skin = null;
                        if (skinRngInfo.rarity == RarityType.Common)
                        {
                            skin = commonSkins.First();
                            commonSkins.Remove(skin);
                        }
                        else if (skinRngInfo.rarity == RarityType.Epic)
                        {
                            skin = epicSkins.First();
                            epicSkins.Remove(skin);
                        }
                        else if (skinRngInfo.rarity == RarityType.Legendary)
                        {
                            skin = legendSkins.First();
                            legendSkins.Remove(skin);
                        }

                        if (skin != null)
                        {
                            if (isFree)
                            {
                                milestoneState.itemGUID_Free = skin.guid;
                            }
                            else
                            {
                                milestoneState.itemGUID_Premium = skin.guid;
                            }
                            return;
                        }
                    }

                    if (isFree)
                    {
                        milestoneState.rewardFree.type = RewardType.Part_Cards;
                        milestoneState.rewardFree.amount = freeSkinReplacePartCardAmount;
                    }
                    else
                    {
                        milestoneState.rewardPremium.type = RewardType.Part_Cards;
                        milestoneState.rewardPremium.amount = premiumSkinReplacePartCardAmount;
                    }
                }
            }
            void PreGeneratePrebuildBotRewards(bool isFree)
            {
                var type = isFree ? milestoneState.rewardFree.type : milestoneState.rewardPremium.type;
                if (type == RewardType.Prebuilt_bots)
                {
                    ItemSO itemSO = null;
                    itemSO = preBuildBots.GetRandom();
                    preBuildBots.Remove(itemSO);

                    if (itemSO != null)
                    {
                        if (isFree)
                        {
                            milestoneState.itemGUID_Free = itemSO.guid;
                        }
                        else
                        {
                            milestoneState.itemGUID_Premium = itemSO.guid;
                        }
                        return;
                    }

                    if (isFree)
                    {
                        milestoneState.rewardFree.type = RewardType.Ultra_Boxes;
                        milestoneState.rewardFree.amount = freePreBuildBotReplaceUltraBoxAmount;
                    }
                    else
                    {
                        milestoneState.rewardPremium.type = RewardType.Ultra_Boxes;
                        milestoneState.rewardPremium.amount = premiumPreBuildBotReplaceUltraBoxAmount;
                    }
                }
            }
        }
        testData = data;
        SyncMilestoneData();
    }

    public bool TryUnlockMilestones(bool notify = true)
    {
        var existUnlocked = false;
        foreach (var milestone in _milestones)
        {
            if (milestone.TryUnlock(_seasonCurrency.value, notify))
            {
                existUnlocked = true;
            }
        }
        return existUnlocked;
    }

    public void PurchaseSeasonPass()
    {
        data.isPurchasedPass = true;
        GameEventHandler.Invoke(SeasonPassEventCode.OnPurchaseSeasonPass);
    }

    public string GetItemId() => resourceItemId;

    public ResourceLocation GetLocation() => resourceLocation;

    public DateTime GetStartPreSeasonDay()
    {
        return data.FirstDayOfSeason.AddDays(-SeasonPassManager.PRESEASON_DAY_AMOUNT);
    }

    public DateTime GetLastDay()
    {
        return data.FirstDayOfSeason.AddMonths(1).AddDays(-SeasonPassManager.PRESEASON_DAY_AMOUNT);
    }

    public DateTime GetNextMissionDay()
    {
        var result = data.passedDay;
        return result.AddDays(1);
    }

    public DateTime GetNextMissionWeek()
    {
        var result = data.passedWeek;
        return result.AddDays(7);
    }

    public int GetSeasonIndex()
    {
        return data.FirstDayOfSeason.Month;
    }

    public DateTime GetNextMissionHalfSeason()
    {
        if (data.passedHalfSeason == data.FirstDayOfSeason)
        {
            var result = data.FirstDayOfSeason;
            var daysInMonth = DateTime.DaysInMonth(result.Year, result.Month) - SeasonPassManager.PRESEASON_DAY_AMOUNT;
            return result.AddDays(Mathf.RoundToInt(daysInMonth / 2f));
        }
        else
        {
            return GetLastDay();
        }
    }

    [Serializable]
    protected class SkinRngInfo : IRandomizable
    {
        public RarityType rarity;
        [Range(0f, 1f)] public float probability;
        public float Probability { get => probability; set => probability = value; }

        public SkinRngInfo(RarityType rarity, float prob)
        {
            this.rarity = rarity;
            this.probability = prob;
        }
    }
}

public enum SeasonPassState : byte
{
    None,
    PreludeSeason,
    InSeason,
    PreSeason,
    EndSeason,
    StartSeason
}
