using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Sirenix.OdinInspector;
using UnityEditor;
using HyrphusQ.Helpers;
using GachaSystem.Core;

[Serializable]
[CreateAssetMenu(fileName = "PBPartCardsSO", menuName = "PocketBots/Gacha/Card/PBPartCardsSO")]
public class PBPartCardsSO : ItemSO
{
    [SerializeField] PBGachaPack gachaPack;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        // Must have GachaItemCardReward Module
        if (!TryGetModule<MonoImageItemModule>(out _))
        {
            AddModule(ItemModule.CreateModule<MonoImageItemModule>(this));
        }
    }
#endif
    public List<GachaCard> GenerateCards(int totalCardsCount)
    {
        return gachaPack.GenerateCards(totalCardsCount, false, false, false, true, false);
    }
}