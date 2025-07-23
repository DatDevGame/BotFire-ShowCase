using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaSkinRewardModule : RewardModule
{
    [SerializeField]
    protected SkinSO skinSO;

    public SkinSO SkinSO { get => skinSO; set => skinSO = value; }

    public override void GrantReward()
    {
        base.GrantReward();
        skinSO.TryUnlockIgnoreRequirement();
    }
}