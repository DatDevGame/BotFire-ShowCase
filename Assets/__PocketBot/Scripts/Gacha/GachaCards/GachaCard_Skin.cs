using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GachaSystem.Core;

[CreateAssetMenu(menuName = "PocketBots/Gacha/Card/Skin")]
public class GachaCard_Skin : GachaCard
{
    public virtual SkinSO SkinSO
    {
        get => TryGetModule<GachaSkinRewardModule>(out var gachaSkinRewardModule) ? gachaSkinRewardModule.SkinSO : null;
        set
        {
            if (TryGetModule<GachaSkinRewardModule>(out var gachaSkinRewardModule))
            {
                gachaSkinRewardModule.SkinSO = value;
            }
            if (TryGetModule<NameItemModule>(out var nameItemModule))
            {
                nameItemModule.displayName = value.GetDisplayName();
            }
            if (TryGetModule<MonoImageItemModule>(out var imageItemModule))
            {
                imageItemModule.SetThumbnailSprite(value.GetThumbnailImage());
            }
            // if (TryGetModule<RarityItemModule>(out var rarityItemModule))
            // {
            //     rarityItemModule.rarityType = ItemSO.GetRarityType();
            // }
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        // Must have GachaItemCardReward Module
        if (!TryGetModule<GachaSkinRewardModule>(out _))
        {
            AddModule(ItemModule.CreateModule<GachaSkinRewardModule>(this));
        }
        // Must have RarityItem Module
        // if (!TryGetModule<RarityItemModule>(out _))
        // {
        //     AddModule(ItemModule.CreateModule<RarityItemModule>(this));
        // }
    }
#endif
    public override GachaCard Clone(params object[] _params)
    {
        var instance = Instantiate(this);
        if (_params[0] is SkinSO skinSO)
        {
            instance.SkinSO = skinSO;
        }
        return instance;
    }

    public override string ToString()
    {
        return $"SkinCard_{SkinSO?.GetInternalName() ?? "Null"}";
    }

    public override bool Equals(object other)
    {
        if (other == null) return false;
        if (SkinSO == null) return false;
        if (other is GachaCard_Skin otherCard)
        {
            return SkinSO.GetInternalName() == otherCard.SkinSO.GetInternalName();
        }
        return false;
    }

    public override int GetHashCode()
    {
        if (SkinSO == null)
            return Const.IntValue.Invalid;
        return SkinSO.GetInternalName().GetHashCode();
    }
}