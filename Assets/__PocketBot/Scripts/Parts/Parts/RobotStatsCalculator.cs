using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RobotStatsCalculator
{
    #region Methods
    // Hp
    public static float CalHp(float hpMultiplier, float baseHp = Const.UpgradeValue.BaseHp) => baseHp * hpMultiplier;
    public static float CalHpMultiplier(int hpStep, float additionValue = 0) => additionValue + hpStep * Const.UpgradeValue.HpStepPercent;
    // Attack
    public static float CalAttack(float attackMultiplier, float baseAttack = Const.UpgradeValue.BaseAttack) => baseAttack * attackMultiplier;
    public static float CalAttackMultiplier(int attackStep, float additionValue = 0) => additionValue + attackStep * Const.UpgradeValue.AttackStepPercent;

    // Stats Score
    public static float CalStatsScore(float hp, float attack)
    {
        var totalScore = (attack + Const.UpgradeValue.Virtual_Atk) * hp * Const.UpgradeValue.StatsFactor;
        return Mathf.Round(totalScore);
    }

    public static float CalCombinationStatsScore(bool isMaxReachableUpgradeLevel, PBChassisSO chassisSO, params PBPartSO[] partSOs)
    {
        var upgradeLevel_Chassis = isMaxReachableUpgradeLevel ? chassisSO.CalMaxReachableUpgradeLevel() : chassisSO.GetCurrentUpgradeLevel();
        var hp = chassisSO.CalHpByLevel(upgradeLevel_Chassis);
        var atk = chassisSO.CalAttackByLevel(upgradeLevel_Chassis);

        foreach (var gearSO in partSOs)
        {
            var upgradeLevel_Gear = isMaxReachableUpgradeLevel ? gearSO.CalMaxReachableUpgradeLevel() : gearSO.GetCurrentUpgradeLevel();
            hp += gearSO.CalHpByLevel(upgradeLevel_Gear);
            atk += gearSO.CalAttackByLevel(upgradeLevel_Gear);
        }

        return CalStatsScore(hp, atk);
    }
    #endregion
}
