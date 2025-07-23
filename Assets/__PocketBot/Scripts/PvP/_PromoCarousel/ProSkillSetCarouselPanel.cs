using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Monetization;
using UnityEngine;

public class ProSkillSetCarouselPanel : CarouselPanel
{
    [SerializeField]
    private ProSkillSet proSkillSet;
    public override bool isAvailable => !proSkillSet.ProSkillSetIAPProduct.IsPurchased() && proSkillSet.IsUnlocked();

    private void Awake()
    {
        if (!proSkillSet.ProSkillSetIAPProduct.IsPurchased())
            proSkillSet.OnItemPurchased += OnItemPurchased;
    }

    private void OnDestroy()
    {
        proSkillSet.OnItemPurchased -= OnItemPurchased;
    }

    private void OnItemPurchased()
    {
        OnPurchased?.Invoke(this, index);
    }
}