using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "PBCurrencyProductSO", menuName = "LatteGames/Economy/PBCurrencyProductSO")]
public class PBCurrencyProductSO : CurrencyProductSO
{
    [SerializeField, TitleGroup("Price"), PropertyOrder(101)]
    private List<float> arenaMultipliers = new List<float>();

    public float GetBonusValue()
    {
        return originalPrice / price;
    }
    public float GetArenaMultiplier(int index)
    {
        if (arenaMultipliers == null || arenaMultipliers.Count <= 0)
            return Const.FloatValue.OneF;
        return arenaMultipliers[index];
    }
}