using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "PocketBots/Gacha/Card/RandomPart", fileName = "GachaCard_RandomPart")]
public class GachaCard_RandomPart : GachaCard_Part
{
    [SerializeField] List<PBPartManagerSO> partManagerSOs = new();
    [SerializeField] int foundInArena = -1; // -1 means any arena

    public override PBPartSO PartSO => GetRandom();

    public virtual RarityType Rarity => TryGetModule<RarityItemModule>(out var rarityItemModule) ? rarityItemModule.rarityType : RarityType.Common;

    private PBPartSO GetRandom()
    {
        var founded = new List<PBPartSO>();
        foreach (var partManagerSO in partManagerSOs)
        {
            founded.AddRange(partManagerSO.Parts.FindAll(Matched));
        }
        var count = founded.Count;
        if (count <= 0) return null;
        return founded[Random.Range(0, count)];
    }

    private bool Matched(PBPartSO partSO)
    {
        if (partSO.GetRarityType() != Rarity) return false;
        if (foundInArena >= 0 && partSO.foundInArena != foundInArena) return false;
        return true;
    }

    public override bool Equals(object other)
    {
        return other is GachaCard_RandomPart otherCard &&
            foundInArena == otherCard.foundInArena &&
            Rarity == otherCard.Rarity;
    }

    public override int GetHashCode()
    {
        return $"{Rarity}{foundInArena}".GetHashCode();
    }
}
