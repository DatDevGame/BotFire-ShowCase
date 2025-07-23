using System;
using System.Collections.Generic;
using System.Linq;
using GachaSystem.Core;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "LadderedOfferSO", menuName = "PocketBots/LadderedOffer/LadderedOfferSO")]
public class LadderedOfferSO : SerializableScriptableObject
{
    [SerializeField] private List<LadderedOfferRewardSet> _offerSets;
    [SerializeField] private int _firstSetIndex;
    [SerializeField] private int _firstShowTrophy;
    [SerializeField] private int _autoShowTrophy;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable _ladderStepIndex;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable _currentSetIndex;
    [SerializeField, BoxGroup("Data")] private PPrefFloatVariable _lastShowTrophy;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable _isFirstShow;
    [SerializeField, BoxGroup("Data")] private TimeBasedRewardSO _resetTimer;
    [SerializeField, BoxGroup("Data")] private HighestAchievedPPrefFloatTracker _highestAchievedMedal;
    [SerializeField, BoxGroup("Data")] private IntVariable m_RequiredTrophiesToUnlockActiveSkill;

    private string GetTheFirstTimeActiveSkill() => $"TheFirstTimeActiveSkill-LadderedOfferSO";

    public int ladderStepIndex => _ladderStepIndex.value;
    public bool isResetOffer => _resetTimer.canGetReward || _isFirstShow.value;
    public bool canAutoShow => _isFirstShow.value || _resetTimer.canGetReward || (_lastShowTrophy.value + 100 <= _highestAchievedMedal.value);
    public bool canShow
    {
        get
        {
            if (_highestAchievedMedal.value < _firstShowTrophy)
            {
                _isFirstShow.value = true;
                return false;
            }
            return true;
        }
    }

    public List<LadderedOfferReward> GetOfferSet()
    {
        if (isResetOffer)
        {
            return GetNewOffetSet();
        }
        else
        {
            return GetCurrentOfferSet();
        }
    }

    public string GetOfferRemainTime()
    {
        TimeSpan interval = DateTime.Now - _resetTimer.LastRewardTime;
        double totalWaitedSeconds = interval.TotalSeconds;
        if (totalWaitedSeconds < 0) //Time traveler detected. Reset timer.
        {
            _resetTimer.GetReward();
            totalWaitedSeconds = 0;
        }
        var remainingSeconds = _resetTimer.CoolDownInterval - totalWaitedSeconds;
        interval = TimeSpan.FromSeconds(remainingSeconds);
        if (interval.TotalHours < 1)
        {
            return string.Format("{0:00}M {1:00}S", interval.Minutes, interval.Seconds);
        }
        else
        {
            return string.Format("{0:00}H {1:00}M", interval.Hours + (interval.Days * 24f), interval.Minutes);
        }
    }

    public void SetLastAutoShowTrophy()
    {
        _lastShowTrophy.value = _highestAchievedMedal.value;
    }

    public void UnlockNextStep()
    {
        _ladderStepIndex.value++;
    }

    private List<LadderedOfferReward> GetCurrentOfferSet()
    {
        return _offerSets[Mathf.Clamp(_currentSetIndex.value, 0, _offerSets.Count - 1)].rewards;
    }

    private List<LadderedOfferReward> GetNewOffetSet()
    {
        _resetTimer.GetReward();
        SetLastAutoShowTrophy();
        _ladderStepIndex.value = 0;
        int setIndex;
        if (_isFirstShow.value)
        {
            setIndex = _firstSetIndex;
            _isFirstShow.value = false;
        }
        else
        {
            int setCount = _offerSets.Count;
            int currentSetIndex = _currentSetIndex.value;
            List<int> indices = new(setCount - 1);
            for (int i = 0; i < setCount; i++)
            {
                if (i != currentSetIndex)
                {
                    indices.Add(i);
                }
            }

            if (_highestAchievedMedal.value >= m_RequiredTrophiesToUnlockActiveSkill.value)
            {
                setIndex = indices.GetRandom();
                List<int> indicesSkill = new List<int>();
                if (!PlayerPrefs.HasKey(GetTheFirstTimeActiveSkill()))
                {
                    for (int x = 0; x < _offerSets.Count; x++)
                    {
                        for (int y = 0; y < _offerSets[x].rewards.Count; y++)
                        {
                            if (_offerSets[x].rewards[y].Rewards.Any(v => v.type == RewardType.SkillCard))
                            {
                                indicesSkill.Add(x);
                                break;
                            }
                        }
                    }
                    PlayerPrefs.SetInt(GetTheFirstTimeActiveSkill(), 1);
                    setIndex = indicesSkill.GetRandom();
                }
            }
            else
            {
                indices.RemoveAt(indices.Count - 1);
                setIndex = indices.GetRandom();
            }
        }
        _currentSetIndex.value = setIndex;

        for (int i = 0; i < _offerSets[setIndex].rewards.Count; i++)
            _offerSets[setIndex].rewards[i].ClearKeySkill();

        return _offerSets[setIndex].rewards;
    }
}

[Serializable]
public class LadderedOfferReward
{
    [SerializeField] private LadderedOfferRewardClaimType _rewardClaimType;
    [BoxGroup("Log Event")]
    [SerializeField] private string m_Set;
    [BoxGroup("Log Event")]
    [SerializeField] private string m_Pack;
    [HideIf(nameof(IsWatchRV))]
    [SerializeField] private IAPProductSO _iapProductSO = null;
    [SerializeField] private List<Reward> _rewards;
    [NonSerialized] private RewardGroupInfo _rewardGroupInfo = null;

    public string m_KeySkillName => $"SkillName-LadderOffer-{m_Set}-{m_Pack}";
    public string SkillName => PlayerPrefs.GetString(m_KeySkillName, "");
    public void ClearKeySkill() => PlayerPrefs.DeleteKey(m_KeySkillName);

    public LadderedOfferRewardClaimType rewardClaimType => _rewardClaimType;
    public List<Reward> Rewards => _rewards;
    public IAPProductSO iapProductSO => _iapProductSO;
    public string Set => m_Set;
    public string Pack => m_Pack;

    private bool IsWatchRV() => _rewardClaimType == LadderedOfferRewardClaimType.WatchRV;
    public void Claim()
    {

    }

    public RewardGroupInfo GetReward(PBGachaPackManagerSO gachaPackManagerSO)
    {
        if (_rewardGroupInfo == null)
        {
            _rewardGroupInfo = new RewardGroupInfo();
            for (int i = 0, length = _rewards.Count; i < length; ++i)
            {
                Reward reward = _rewards[i];
                switch (reward.type)
                {
                    case RewardType.Coins:
                        AddCurrency(CurrencyType.Standard, _rewardGroupInfo, reward);
                        break;
                    case RewardType.Gems:
                        AddCurrency(CurrencyType.Premium, _rewardGroupInfo, reward);
                        break;
                    case RewardType.Skip_Ads:
                        AddCurrency(CurrencyType.RVTicket, _rewardGroupInfo, reward);
                        break;
                    case RewardType.Part_Cards:
                        _rewardGroupInfo.generalItems ??= new Dictionary<ItemSO, ShopProductSO.DiscountableValue>();
                        _rewardGroupInfo.generalItems.Add(
                            gachaPackManagerSO.GetPartCardCurrentArena(),
                            new ShopProductSO.DiscountableValue()
                            {
                                value = reward.amount,
                                originalValue = reward.amount
                            }

                        );
                        break;
                    case RewardType.Classic_Boxes:
                        _rewardGroupInfo.generalItems ??= new Dictionary<ItemSO, ShopProductSO.DiscountableValue>();
                        _rewardGroupInfo.generalItems.Add(
                            gachaPackManagerSO.GetGachaPackCurrentArena(GachaPackRarity.Classic),
                            new ShopProductSO.DiscountableValue()
                            {
                                value = reward.amount,
                                originalValue = reward.amount
                            }

                        );
                        break;
                    case RewardType.Great_Boxes:
                        _rewardGroupInfo.generalItems ??= new Dictionary<ItemSO, ShopProductSO.DiscountableValue>();
                        _rewardGroupInfo.generalItems.Add(
                            gachaPackManagerSO.GetGachaPackCurrentArena(GachaPackRarity.Great),
                            new ShopProductSO.DiscountableValue()
                            {
                                value = reward.amount,
                                originalValue = reward.amount
                            }

                        );
                        break;
                    case RewardType.Ultra_Boxes:
                        _rewardGroupInfo.generalItems ??= new Dictionary<ItemSO, ShopProductSO.DiscountableValue>();
                        _rewardGroupInfo.generalItems.Add(
                            gachaPackManagerSO.GetGachaPackCurrentArena(GachaPackRarity.Ultra),
                            new ShopProductSO.DiscountableValue()
                            {
                                value = reward.amount,
                                originalValue = reward.amount
                            }

                        );
                        break;

                    case RewardType.SkillCard:
                        _rewardGroupInfo.generalItems ??= new Dictionary<ItemSO, ShopProductSO.DiscountableValue>();
                        _rewardGroupInfo.generalItems.Add(
                            gachaPackManagerSO.GachaCard_RandomActiveSkill, 
                            new ShopProductSO.DiscountableValue()
                        {
                            value = reward.amount,
                            originalValue = reward.amount
                        });
                        break;
                }
            }
        }

        return _rewardGroupInfo;

        void AddCurrency(CurrencyType currencyType, RewardGroupInfo rewardGroupInfo, Reward reward)
        {
            if (rewardGroupInfo.currencyItems == null)
            {
                rewardGroupInfo.currencyItems = new Dictionary<CurrencyType, ShopProductSO.DiscountableValue>();
            }
            rewardGroupInfo.currencyItems.Add(
                currencyType,
                new ShopProductSO.DiscountableValue()
                {
                    value = reward.amount,
                    originalValue = reward.amount
                });
        }
    }
}

[Serializable]
public class LadderedOfferRewardSet
{
    [SerializeField] private List<LadderedOfferReward> _rewards = new();

    public List<LadderedOfferReward> rewards => _rewards;
}

public enum LadderedOfferRewardClaimType
{
    IAP,
    Free,
    WatchRV,
}
