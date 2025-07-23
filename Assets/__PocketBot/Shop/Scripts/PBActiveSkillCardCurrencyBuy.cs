using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBActiveSkillCardCurrencyBuyButton : PBCurrencyBuyButton
{
    public override ResourceLocationProvider GetSinkLocationProvider()
    {
        return new ResourceLocationProvider(ResourceLocation.Purchase, $"{shopProductSO.productName}");
    }
}
