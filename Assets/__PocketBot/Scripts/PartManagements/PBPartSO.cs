using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[WindowMenuItem("PartSO", assetFolderPath: "Assets/__PocketBot/RobotParts/ScriptableObjects", mode: WindowMenuItemAttribute.Mode.Multiple, sortByName: true)]
[CreateAssetMenu(fileName = "PartSO", menuName = "PocketBots/PartManagement/PartSO")]
public class PBPartSO : GachaItemSO, IPartStats
{
    [Serializable]
    public class PBStats
    {
        [SerializeField]
        private float power;
        [SerializeField, Range(0, 1)]
        private float resistance;
        [SerializeField]
        private float turning;
        [SerializeField]
        private float damagePerHitRatio;

        public float Power
        {
            get => power;
            set => power = value;
        }
        public float Resistance
        {
            get => resistance;
            set => resistance = value;
        }
        public float Turning
        {
            get => turning;
            set => turning = value;
        }
        public float DamagePerHitRatio
        {
            get => damagePerHitRatio;
            set => damagePerHitRatio = value;
        }
    }

    #region Fields
    [SerializeField, BoxGroup("Part Fields")]
    protected bool isBossPart;
    [SerializeField, BoxGroup("Part Fields")]
    protected int trophyThreshold;
    [SerializeField, BoxGroup("Part Fields")]
    protected PBPartManagerSO partManagerSO;
    [SerializeField, BoxGroup("Part Fields")]
    protected PBStats stats;
    [SerializeField, BoxGroup("Part Fields"), PropertyOrder(10)]
    protected BaseUpgradePath upgradePath;
    protected PBPartType partType;
    [SerializeField, BoxGroup("TransformBot")]
    protected bool isTransformBot;
    [SerializeField, BoxGroup("TransformBot"), ShowIf("isTransformBot")]
    protected int transformBotID;
    #endregion

    #region Properties
    public bool IsEquipped { get; set; }
    public bool IsBossPart
    {
        get => isBossPart;
        set => isBossPart = value;
    }
    public int TrophyThreshold
    {
        get => trophyThreshold;
        set => trophyThreshold = value;
    }
    public bool IsTransformBot
    {
        get => isTransformBot;
        set => isTransformBot = value;
    }
    public PBPartType PartType
    {
        get
        {
            return ManagerSO?.PartType ?? partType;
        }
        set
        {
            partType = value;
        }
    }
    public int TransformBotID => transformBotID;
    public PBPartManagerSO ManagerSO => partManagerSO;
    public PBStats Stats { get => stats; set => stats = value; }
    public BaseUpgradePath UpgradePath => upgradePath;

#if UNITY_EDITOR
    // Show in Editor only for debug purpose
    [ShowInInspector, FoldoutGroup("Info")]
    private string IsAvailableInfo => GachaPoolManager.Instance == null ? "Null" : (GachaPoolManager.IsAvailable(this) ? "Available" : "NOT Available");
    [ShowInInspector, FoldoutGroup("Info")]
    private bool IsUnlocked => this.IsUnlocked();
    [ShowInInspector, FoldoutGroup("Info")]
    private bool IsNew => this.IsNew();
    [ShowInInspector, FoldoutGroup("Info")]
    private int NumOfCards => this.GetNumOfCards();
    [ShowInInspector, FoldoutGroup("Info/Stats")]
    private float Health => GetHealth().value;
    [ShowInInspector, FoldoutGroup("Info/Stats")]
    private float Attack => GetAttack().value;
    [ShowInInspector, FoldoutGroup("Info/Stats")]
    private float Power => GetPower().value;
    [ShowInInspector, FoldoutGroup("Info/Stats")]
    private float Resistance => GetResistance().value;
    [ShowInInspector, FoldoutGroup("Info/Stats")]
    private float Turning => GetTurning().value;
    [ShowInInspector, FoldoutGroup("Info/Upgrade")]
    private int CurrentUpgradeLevel => this.GetCurrentUpgradeLevel();
    [ShowInInspector, FoldoutGroup("Info/Upgrade")]
    private int MaxUpgradeLevel => this.GetMaxUpgradeLevel();
    [ShowInInspector, FoldoutGroup("Info/Upgrade")]
    private int MaxReachableUpgradeLevel => CalMaxReachableUpgradeLevel();
    [ShowInInspector, FoldoutGroup("Info/Upgrade")]
    private float CoinsToReachMaxReachableUpgradeLevel => CalcTotalCoinsToMaxReachableUpgradeLevel();

    [ButtonGroup("2"), Button(SdfIconType.Plus, "Reset Cards"), PropertyOrder(-1)]
    private void ResetCards()
    {
        this.UpdateNumOfCards(0);
    }
    [ButtonGroup("2"), Button(SdfIconType.Plus, "Reset Upgrade"), PropertyOrder(-1)]
    private void ResetUpgrade()
    {
        this.ResetUpgradeLevelToDefault();
    }
    [ButtonGroup("2"), Button(SdfIconType.Plus, "Reset Unlock"), PropertyOrder(-1)]
    private void ResetUnlock()
    {
        this.ResetUnlockToDefault();
    }
    [ButtonGroup("2"), Button(SdfIconType.Plus, "Reset New"), PropertyOrder(-1)]
    private void ResetNew()
    {
        this.SetNewItem(false);
    }
#endif
    #endregion

    // Stats Methods
    public float GetStat(PBStatID statID)
    {
        return statID switch
        {
            PBStatID.Health => GetHealth().value,
            PBStatID.Attack => GetAttack().value,
            PBStatID.Power => GetPower().value,
            PBStatID.Resistance => GetResistance().value,
            PBStatID.Turning => GetTurning().value,
            _ => 0
        };
    }

    public IStat<PBStatID, float> GetAttack()
    {
        return new PBStat<float>(PBStatID.Attack, CalCurrentAttack());
    }

    public IStat<PBStatID, float> GetHealth()
    {
        return new PBStat<float>(PBStatID.Health, CalCurrentHp());
    }

    public IStat<PBStatID, float> GetPower()
    {
        return new PBStat<float>(PBStatID.Power, Stats.Power);
    }

    public IStat<PBStatID, float> GetResistance()
    {
        return new PBStat<float>(PBStatID.Resistance, Stats.Resistance);
    }

    public IStat<PBStatID, float> GetTurning()
    {
        return new PBStat<float>(PBStatID.Turning, stats.Turning);
    }

    public IStat<PBStatID, float> GetStatsScore()
    {
        return new PBStat<float>(PBStatID.StatsScore, CalCurrentStatsScore());
    }

    public float CalCurrentHp() => CalHpByLevel(this.GetCurrentUpgradeLevel());
    public float CalCurrentHpMultiplier() => CalHpMultiplierByLevel(this.GetCurrentUpgradeLevel());
    public float CalCurrentAttack() => CalAttackByLevel(this.GetCurrentUpgradeLevel());
    public float CalCurrentAttackMultiplier() => CalAttackMultiplierByLevel(this.GetCurrentUpgradeLevel());
    public float CalCurrentStatsScore() => CalStatsScoreByLevel(this.GetCurrentUpgradeLevel());

    public float CalHpByLevel(int upgradeLevel) => UpgradePath.CalHpByLevel(upgradeLevel - 1);
    public float CalHpMultiplierByLevel(int upgradeLevel) => UpgradePath.CalHpMultiplierByLevel(upgradeLevel - 1);
    public float CalAttackByLevel(int upgradeLevel) => UpgradePath.CalAttackByLevel(upgradeLevel - 1);
    public float CalAttackMultiplierByLevel(int upgradeLevel) => UpgradePath.CalAttackMultiplierByLevel(upgradeLevel - 1);
    public float CalStatsScoreByLevel(int upgradeLevel) => UpgradePath.CalStatsScoreByLevel(upgradeLevel - 1);

    // Others Methods
    public T GetModelPrefab<T>() where T : PBPart
    {
        if (TryGetModule(out ModelPrefabItemModule prefabBasedItemModule))
        {
            return prefabBasedItemModule.GetModelPrefab<T>();
        }
        return null;
    }

    public int CalMaxReachableUpgradeLevel()
    {
        var maxUpgradeLevel = this.GetMaxUpgradeLevel();
        var currentUpgradeLevel = this.GetCurrentUpgradeLevel();
        if (currentUpgradeLevel >= maxUpgradeLevel)
            return maxUpgradeLevel;
        int reachableUpgradeLevel = currentUpgradeLevel;
        int totalNumOfCards = this.GetNumOfCards();
        for (int i = reachableUpgradeLevel + 1; i <= maxUpgradeLevel; i++)
        {
            if (this.TryGetUpgradeRequirementOfLevel(i, out Requirement_GachaCard cardRequirement) && totalNumOfCards >= cardRequirement.requiredNumOfCards)
            {
                totalNumOfCards -= cardRequirement.requiredNumOfCards;
                reachableUpgradeLevel++;
            }
        }
        return reachableUpgradeLevel;
    }

    public int CalcTotalCoinsToMaxReachableUpgradeLevel()
    {
        int totalCoins = 0;
        int currentUpgradeLevel = this.GetCurrentUpgradeLevel();
        int maxUpgradeLevel = this.GetMaxUpgradeLevel();
        if (currentUpgradeLevel >= maxUpgradeLevel)
            return totalCoins;
        int maxReachableUpgradeLevel = CalMaxReachableUpgradeLevel();
        for (int i = currentUpgradeLevel + 1; i <= maxReachableUpgradeLevel; i++)
        {
            if (this.TryGetUpgradeRequirementOfLevel(i, out Requirement_Currency requirement))
                totalCoins += (int)requirement.requiredAmountOfCurrency;
        }
        return totalCoins;
    }

    public bool IsAvailable()
    {
        return GachaPoolManager.IsAvailable(this);
    }
}