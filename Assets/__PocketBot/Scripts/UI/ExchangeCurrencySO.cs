using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "ExchangeCurrencySO", menuName = "PocketBots/ExchangeCurrency/ExchangeCurrencySO")]
public class ExchangeCurrencySO : SerializedScriptableObject
{
    [SerializeField] private CurrentHighestArenaVariable m_CurrentHighestArenaVariable;
    [SerializeField] private SerializedDictionary<int, float> m_RatioFollowingArena;

    public float CalcRequiredAmountOfPremiumCurrencyFollowingArena(float exchangeAmountOfStandardCurrency)
    {
        float value = 0;
        if (m_RatioFollowingArena.ContainsKey(m_CurrentHighestArenaVariable.value.index + 1))
            value = m_RatioFollowingArena[m_CurrentHighestArenaVariable.value.index + 1];
        float calcValue = exchangeAmountOfStandardCurrency / value;
        return calcValue;
    }
}
