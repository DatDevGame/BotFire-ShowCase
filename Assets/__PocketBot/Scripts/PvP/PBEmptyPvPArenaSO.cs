using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using LatteGames.PvP;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "EmptyPvPArenaSO", menuName = "PocketBots/PvP/EmptyPvPArenaSO")]
public class PBEmptyPvPArenaSO : PvPArenaSO
{
    [SerializeField, TitleGroup("TrophyRoad"), PropertyOrder(4)]
    protected Color insideColor, outsideColor;
    [SerializeField, TitleGroup("TrophyRoad"), PropertyOrder(4)]
    protected Sprite patternSprite;

    public Color InsideColor => insideColor;
    public Color OutsideColor => outsideColor;
    public Sprite PatternSprite => patternSprite;
    public override int index => m_Tournament.arenas.Count;
    public override float wonNumOfPoints
    {
        get
        {
            if (TryGetReward(out CurrencyRewardModule currencyRewardModule, item => item.CurrencyType == CurrencyType.Medal))
                return currencyRewardModule.Amount;
            return Const.IntValue.Zero;
        }
    }
    public override float lostNumOfPoints
    {
        get
        {
            if (TryGetPunishment(out CurrencyPunishmentModule currencyPunishmentModule, item => item.currencyType == CurrencyType.Medal))
                return currencyPunishmentModule.amount;
            return Const.IntValue.Zero;
        }
    }
    public virtual int NumOfRounds
    {
        get => m_NumOfRounds;
        set => m_NumOfRounds = value;
    }
    public virtual int WonNumOfCoins
    {
        get
        {
            if (TryGetReward(out CurrencyRewardModule currencyRewardModule, item => item.CurrencyType == CurrencyType.Standard))
                return currencyRewardModule.Amount;
            return Const.IntValue.Zero;
        }
        set
        {
            if (TryGetReward(out CurrencyRewardModule currencyRewardModule, item => item.CurrencyType == CurrencyType.Standard))
                currencyRewardModule.Amount = value;
        }
    }
    public virtual int WonNumOfTrophies
    {
        get
        {
            if (TryGetReward(out CurrencyRewardModule currencyRewardModule, item => item.CurrencyType == CurrencyType.Medal))
                return currencyRewardModule.Amount;
            return Const.IntValue.Zero;
        }
        set
        {
            if (TryGetReward(out CurrencyRewardModule currencyRewardModule, item => item.CurrencyType == CurrencyType.Medal))
                currencyRewardModule.Amount = value;
        }
    }
    public virtual int LostNumOfTrophies
    {
        get
        {
            if (TryGetPunishment(out CurrencyPunishmentModule currencyPunishmentModule, item => item.currencyType == CurrencyType.Medal))
                return Mathf.RoundToInt(currencyPunishmentModule.amount);
            return Const.IntValue.Zero;
        }
        set
        {
            if (TryGetPunishment(out CurrencyPunishmentModule currencyPunishmentModule, item => item.currencyType == CurrencyType.Medal))
            {
                currencyPunishmentModule.amount = value;
            }
        }
    }
    public virtual int RequiredNumOfTrophiesToUnlock
    {
        get
        {
            if (this.TryGetUnlockRequirement(out Requirement_Currency requirement, item => item.currencyType == CurrencyType.Medal))
                return Mathf.RoundToInt(requirement.requiredAmountOfCurrency);
            return Const.IntValue.Zero;
        }
        set
        {
            if (this.TryGetUnlockRequirement(out Requirement_Currency requirement, item => item.currencyType == CurrencyType.Medal))
            {
                requirement.requiredAmountOfCurrency = value;
            }
        }
    }
}