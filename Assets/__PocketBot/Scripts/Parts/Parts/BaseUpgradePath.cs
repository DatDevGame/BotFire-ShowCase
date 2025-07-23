using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class BaseUpgradePath
{
    [LabelText("ATK"), HorizontalGroup]
    public List<int> attackUpgradeSteps = new();
    [LabelText("HP"), HorizontalGroup]
    public List<int> hpUpgradeSteps = new();

    public virtual void Clear()
    {
        attackUpgradeSteps.Clear();
        hpUpgradeSteps.Clear();
    }

    // Hp
    public virtual int GetUpgradeStepByLevel(int levelIndex)
    {
        levelIndex = Mathf.Clamp(levelIndex, 0, hpUpgradeSteps.Count - 1);
        return hpUpgradeSteps[levelIndex];
    }

    public virtual float CalHpByLevel(int levelIndex)
    {
        return RobotStatsCalculator.CalHp(CalHpMultiplierByLevel(levelIndex));
    }

    public virtual float CalHpMultiplierByLevel(int levelIndex)
    {
        return RobotStatsCalculator.CalHpMultiplier(GetUpgradeStepByLevel(levelIndex));
    }

    // Attack
    public virtual int GetAttackStepByLevel(int levelIndex)
    {
        levelIndex = Mathf.Clamp(levelIndex, 0, attackUpgradeSteps.Count - 1);
        return attackUpgradeSteps[levelIndex];
    }

    public virtual float CalAttackByLevel(int levelIndex)
    {
        return RobotStatsCalculator.CalAttack(CalAttackMultiplierByLevel(levelIndex));
    }

    public virtual float CalAttackMultiplierByLevel(int levelIndex)
    {
        return RobotStatsCalculator.CalAttackMultiplier(GetAttackStepByLevel(levelIndex));
    }

    // Stats score
    public virtual float CalStatsScoreByLevel(int levelIndex)
    {
        var hp = CalHpByLevel(levelIndex);
        var attack = CalAttackByLevel(levelIndex);
        return RobotStatsCalculator.CalStatsScore(hp, attack);
    }
}
