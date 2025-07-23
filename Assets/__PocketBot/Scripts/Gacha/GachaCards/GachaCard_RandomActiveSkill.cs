using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using UnityEngine;

[CreateAssetMenu(menuName = "PocketBots/Gacha/Card/RandomActiveSkill")]
public class GachaCard_RandomActiveSkill : GachaCard
{
    [SerializeField]
    private PBGachaPack gachaPack;

    public ActiveSkillSO GetRandomActiveSkillSO()
    {
        return gachaPack.GetRandomActiveSkillSO();
    }
}