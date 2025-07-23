using System;
using System.Collections.Generic;
using UnityEngine;

//default
public class MissionTargetCalculator
{
    public virtual object[] additionalDatas => null;

    public virtual bool IsHaveValidTarget()
    {
        return true;
    }

    public virtual float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        return amountFactor * amountMultiplier;
    }
}

//MissionTargetType.WeaponMastery_Count_Equip,
//MissionTargetType.WeaponMastery_Count_Win,
//MissionTargetType.WeaponMastery_Count_Kill,
public class WeaponMastery_TargetCalculator : MissionTargetCalculator
{
    private PBPartManagerSO _frontPartManagerSO;
    private PBPartManagerSO _upperPartManagerSO;
    private List<PBPartSO> _allWeaponPart = new();
    private Dictionary<string, RarityType> _rarityCache = new();
    private object[] _additionalDatas = null;

    public override object[] additionalDatas => _additionalDatas;

    public WeaponMastery_TargetCalculator(PBPartManagerSO frontPartManagerSO, PBPartManagerSO upperPartManagerSO)
    {
        _frontPartManagerSO = frontPartManagerSO;
        _upperPartManagerSO = upperPartManagerSO;
        foreach(PBPartSO partSO in _frontPartManagerSO.Parts)
        {
            _rarityCache[partSO.guid] = partSO.GetRarityType();
            _allWeaponPart.Add(partSO);
        }
        foreach(PBPartSO partSO in _upperPartManagerSO.Parts)
        {
            _rarityCache[partSO.guid] = partSO.GetRarityType();
            _allWeaponPart.Add(partSO);
        }
        _allWeaponPart.Sort(PartComparison);
    }
    
    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        RarityType rarity = RarityType.Mythic;
        List<PBPartSO> unlockedParts = _allWeaponPart.FindAll((part) => part.IsUnlocked());
        while (!TryFindRandomEquip(rarity, unlockedParts))
        {
            if (rarity == RarityType.Common) //this should not happen
                break;
            rarity--;
        }
        return base.CalculateTarget(amountFactor, amountMultiplier);
    }

    private bool TryFindRandomEquip(RarityType rarity, List<PBPartSO> unlockedParts)
    {
        List<PBPartSO> partPoolByRarity = unlockedParts.FindAll((part) => _rarityCache[part.guid] == rarity);
        if (partPoolByRarity.Count == 0)
        {
            _additionalDatas = null;
            return false;
        }
        else
        {
            _additionalDatas = new object[]{partPoolByRarity.GetRandom()};
            return true;
        }
    }

    private int PartComparison(PBPartSO part1, PBPartSO part2)
    {
        return _rarityCache[part2.guid] - _rarityCache[part1.guid];
    }
}

//MissionTargetType.EarnCurrency_Count_Trophies
public class EarnCurrency_Count_Trophies_TargetCalculator : MissionTargetCalculator
{
    private CurrentHighestArenaVariable _highestArena;

    public EarnCurrency_Count_Trophies_TargetCalculator(CurrentHighestArenaVariable highestArena)
    {
        _highestArena = highestArena;
    }

    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        float trophyPrize = _highestArena.value.TryGetReward(out CurrencyRewardModule medalReward, item => item.CurrencyType == CurrencyType.Medal) ?
            medalReward.Amount * PBPvPGameOverUI.GetRewardMultiplier(Mode.Normal, CurrencyType.Medal, 1) : 1f;
 
        return base.CalculateTarget(amountFactor, amountMultiplier) * trophyPrize;
    }
}

//MissionTargetType.EarnCurrency_Count_Coins
public class EarnCurrency_Count_Coins_TargetCalculator : MissionTargetCalculator
{
    private CurrentHighestArenaVariable _highestArena;

    public EarnCurrency_Count_Coins_TargetCalculator(CurrentHighestArenaVariable highestArena)
    {
        _highestArena = highestArena;
    }

    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        float coinPrize = _highestArena.value.TryGetReward(out CurrencyRewardModule moneyReward, item => item.CurrencyType == CurrencyType.Standard) ?
            moneyReward.Amount * PBPvPGameOverUI.GetRewardMultiplier(Mode.Normal, CurrencyType.Standard, 1) : 1f;
        return base.CalculateTarget(amountFactor, amountMultiplier) * coinPrize;
    }
}

//MissionTargetType.EarnCurrency_Count_Gems
public class EarnCurrency_Count_Gems_TargetCalculator : MissionTargetCalculator
{
    private CurrentHighestArenaVariable _highestArena;

    public EarnCurrency_Count_Gems_TargetCalculator(CurrentHighestArenaVariable highestArena)
    {
        _highestArena = highestArena;
    }

    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {        
        int arenaIndex = _highestArena.value.index;
        return base.CalculateTarget(amountFactor, amountMultiplier) * Mathf.Pow(1.1f, arenaIndex);
    }
}

//MissionTargetType.StreakMastery_Reach_Streak
public class StreakMastery_Reach_Streak_TargetCalculator : MissionTargetCalculator
{
    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        return amountFactor;
    }
}

//MissionTargetType.BossDefeats_Count
public class BossDefeats_Count_TargetCalculator : MissionTargetCalculator
{
    private int _allNextChapterUndefeatedBoss;
    private int _currentChapterUndefeatedBoss;
    public override bool IsHaveValidTarget()
    {
        int bossCount = BossFightManager.Instance.bossMapSO.chapterList[BossFightManager.Instance.bossMapSO.chapterIndex.value].bossCount;
        int bossIndex = BossFightManager.Instance.bossMapSO.chapterList[BossFightManager.Instance.bossMapSO.chapterIndex.value].bossIndex.value;
        _currentChapterUndefeatedBoss = bossCount - bossIndex;
        int chapterCount = BossFightManager.Instance.bossMapSO.chapterList.Count;
        _allNextChapterUndefeatedBoss = 0;
        for (int i = BossFightManager.Instance.bossMapSO.chapterIndex.value + 1; i < chapterCount; i++)
        {
            _allNextChapterUndefeatedBoss += BossFightManager.Instance.bossMapSO.chapterList[i].bossCount;
        }
        return _allNextChapterUndefeatedBoss + _currentChapterUndefeatedBoss > 0;
    }

    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        return Mathf.Min(amountFactor, _currentChapterUndefeatedBoss + _allNextChapterUndefeatedBoss);
    }
}

//MissionTargetType.BossDefeats_Specific
public class BossDefeats_Specific_TargetCalculator : MissionTargetCalculator
{
    private object[] _additionalDatas = null;

    public override object[] additionalDatas => _additionalDatas;

    public override bool IsHaveValidTarget()
    {
        int chapterIndex = BossFightManager.Instance.bossMapSO.chapterIndex.value;
        BossChapterSO currentChapter = BossFightManager.Instance.bossMapSO.chapterList[chapterIndex];
        if (currentChapter.currentBossSO != null)
        {
            _additionalDatas = new object[]{ currentChapter.currentBossSO };
            return true;
        }
        int chapterCount = BossFightManager.Instance.bossMapSO.chapterList.Count;
        for (int i = BossFightManager.Instance.bossMapSO.chapterIndex.value + 1; i < chapterCount; i++)
        {
            if (BossFightManager.Instance.bossMapSO.chapterList[i].bossCount <= 0)
                continue;
            _additionalDatas = new object[]{ BossFightManager.Instance.bossMapSO.chapterList[i].bossList[0] };
            return true;
        }
        _additionalDatas = null;
        return false;
    }

    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        return 1f;
    }
}

//MissionTargetType.CardCollection_Any
public class CardCollection_Any_TargetCalculator : MissionTargetCalculator
{
    private PBGachaPackManagerSO _gachaPackManagerSO;

    public CardCollection_Any_TargetCalculator(PBGachaPackManagerSO gachaPackManagerSO)
    {
        _gachaPackManagerSO = gachaPackManagerSO;
    }

    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        return base.CalculateTarget(amountFactor, amountMultiplier) * _gachaPackManagerSO.GetGachaPackCurrentArena(GachaPackRarity.Classic).TotalCardsCount;
    }
}

//MissionTargetType.CardCollection_Specific
public class CardCollection_Specific_TargetCalculator : MissionTargetCalculator
{
    private PBGachaPackManagerSO _gachaPackManagerSO;
    private object[] _additionalDatas = null;

    public override object[] additionalDatas => _additionalDatas;
    
    public CardCollection_Specific_TargetCalculator(PBGachaPackManagerSO gachaPackManagerSO)
    {
        _gachaPackManagerSO = gachaPackManagerSO;
    }

    public override bool IsHaveValidTarget()
    {
        PBGachaPack gachaPack = _gachaPackManagerSO.GetGachaPackCurrentArena(GachaPackRarity.Great);
        List<PBPartSO> pool = gachaPack.GetAllUnlockablePartByRarity(gachaPack.highestDroppableRarity);
        if (pool.Count <= 0)
        {
            _additionalDatas = null;
            return false;
        }
        else
        {
            _additionalDatas = new object[]{ pool.GetRandom() };
            return true;
        }
    }

    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        return 1f;
    }
}

//MissionTargetType.ArenaUnlocks_Specific
public class ArenaUnlocks_Specific_TargetCalculator : MissionTargetCalculator
{
    private CurrentHighestArenaVariable _highestArena;
    private PBPvPTournamentSO _tournament;
    private object[] _additionalDatas = null;

    public override object[] additionalDatas => _additionalDatas;

    public ArenaUnlocks_Specific_TargetCalculator(CurrentHighestArenaVariable highestArena, PBPvPTournamentSO tournament)
    {
        _highestArena = highestArena;
        _tournament = tournament;
    }

    public override bool IsHaveValidTarget()
    {
        if (_tournament.arenas.Count <= _highestArena.value.index + 1)
        {
            _additionalDatas = null;
            return false;
        }
        else
        {
            _additionalDatas = new object[]{ _tournament.arenas[_highestArena.value.index + 1] };
            return true;
        }
    }

    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        return 1f;
    }
}

//MissionTargetType.SpendCurrency_Count_Coins
//MissionTargetType.SpendCurrency_Count_Gems
public class SpendCurrency_TargetCalculator : MissionTargetCalculator
{
    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        float rvTicket2GemExchangeRate = ExchangeRateTableSO.GetExchangeRateOfOtherItems(ExchangeRateTableSO.ItemType.RV, ExchangeRateTableSO.ArenaFlags.All);
        return base.CalculateTarget(amountFactor, amountMultiplier) * rvTicket2GemExchangeRate;
    }
}

//MissionTargetType.BossClaim_Count
public class BossClaim_Count_TargetCalculator : MissionTargetCalculator
{
    private int _unclaimedBossCount = 0;
    public override bool IsHaveValidTarget()
    {
        _unclaimedBossCount = 0;
        foreach(BossChapterSO bossChapter in BossFightManager.Instance.bossMapSO.chapterList)
        {
            foreach(BossSO bossSO in bossChapter.bossList)
            {
                if (bossSO.chassisSO.IsClaimedRV)
                    continue;
                _unclaimedBossCount++;
            }
        }
        return _unclaimedBossCount > 0;
    }

    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        return Mathf.Min(amountFactor, _unclaimedBossCount);
    }
}

//MissionTargetType.BossClaim_Specific
public class BossClaim_Specific_TargetCalculator : MissionTargetCalculator
{
    private List<BossSO> _unclaimedBosses = new();
    private object[] _additionalDatas = null;

    public override object[] additionalDatas => _additionalDatas;

    public override bool IsHaveValidTarget()
    {
        _unclaimedBosses.Clear();
        foreach(BossChapterSO bossChapter in BossFightManager.Instance.bossMapSO.chapterList)
        {
            foreach(BossSO bossSO in bossChapter.bossList)
            {
                if (bossSO.chassisSO.IsClaimedRV)
                    continue;
                _unclaimedBosses.Add(bossSO);
            }
        }
        if (_unclaimedBosses.Count > 0)
        {
            _additionalDatas = new object[]{ _unclaimedBosses.GetRandom() };
            return true;
        }
        else
        {
            _additionalDatas = null;
            return false;
        }
    }

    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        return 1;
    }
}

//MissionTargetType.CardUpgrades_UptoLevel_Specific,
public class CardUpgrades_UptoLevel_Specific_TargetCalculator : MissionTargetCalculator
{
    private PBPartManagerSO _frontPartManagerSO;
    private PBPartManagerSO _upperPartManagerSO;
    private PBPartManagerSO _bodyPartManagerSO;
    private List<PBPartSO> _allWeaponPart = new();
    private Dictionary<string, RarityType> _rarityCache = new();
    private PBPartSO _selectedWeapon;
    private object[] _additionalDatas = null;

    public override object[] additionalDatas => _additionalDatas;

    public CardUpgrades_UptoLevel_Specific_TargetCalculator(PBPartManagerSO frontPartManagerSO, PBPartManagerSO upperPartManagerSO, PBPartManagerSO bodyPartManagerSO)
    {
        _frontPartManagerSO = frontPartManagerSO;
        _upperPartManagerSO = upperPartManagerSO;
        _bodyPartManagerSO = bodyPartManagerSO;
        foreach(PBPartSO partSO in _frontPartManagerSO.Parts)
        {
            if (partSO.IsMaxUpgradeLevel())
                continue;
            _rarityCache[partSO.guid] = partSO.GetRarityType();
            _allWeaponPart.Add(partSO);
        }
        foreach(PBPartSO partSO in _upperPartManagerSO.Parts)
        {
            if (partSO.IsMaxUpgradeLevel())
                continue;
            _rarityCache[partSO.guid] = partSO.GetRarityType();
            _allWeaponPart.Add(partSO);
        }
        foreach(PBPartSO partSO in _bodyPartManagerSO.Parts)
        {
            if (partSO.IsMaxUpgradeLevel())
                continue;
            _rarityCache[partSO.guid] = partSO.GetRarityType();
            _allWeaponPart.Add(partSO);
        }
        _allWeaponPart.Sort(PartComparison);
    }

    public override bool IsHaveValidTarget()
    {        
        RarityType rarity = RarityType.Mythic;
        List<PBPartSO> unlockedParts = _allWeaponPart.FindAll((part) => part.IsUnlocked() && part.GetCurrentUpgradeLevel() < part.GetMaxUpgradeLevel());
        while (!TryFindRandomEquip(rarity, unlockedParts))
        {
            if (rarity == RarityType.Common)
            {
                _additionalDatas = null;
                return false;
            }
            rarity--;
        }
        return true;
    }

    public override float CalculateTarget(float amountFactor, float amountMultiplier)
    {
        _additionalDatas = new object[] {_selectedWeapon, Mathf.Min(_selectedWeapon.GetMaxUpgradeLevel(), (int) (_selectedWeapon.GetCurrentUpgradeLevel() + amountFactor))};
        return 1f;
    }

    private bool TryFindRandomEquip(RarityType rarity, List<PBPartSO> unlockedParts)
    {
        List<PBPartSO> partPoolByRarity = unlockedParts.FindAll((part) => _rarityCache[part.guid] == rarity);
        if (partPoolByRarity.Count == 0)
        {
            _selectedWeapon = null;
            return false;
        }
        else
        {
            _selectedWeapon = partPoolByRarity.GetRandom();
            return true;
        }
    }

    private int PartComparison(PBPartSO part1, PBPartSO part2)
    {
        return _rarityCache[part2.guid] - _rarityCache[part1.guid];
    }
}
