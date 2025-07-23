using HyrphusQ.SerializedDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "RaritySpriteSO", menuName = "PocketBots/RaritySpriteSO")]
public class RaritySpriteSO : SerializableScriptableObject
{
    [SerializeField, BoxGroup("Currency")] private Sprite _defaultCurrencyRaritySprite;
    [SerializeField, BoxGroup("Currency")] private SerializedDictionary<CurrencyType, Sprite> _currencyRaritySprites;
    [SerializeField, BoxGroup("ItemSO")] private Sprite _defaultItemRaritySprite;
    [SerializeField, BoxGroup("ItemSO")] private SerializedDictionary<RarityType, Sprite> _itemRaritySprites;

    public Sprite GetRaritySprite(CurrencyType currencyType)
    {
        if (_currencyRaritySprites != null &&
            _currencyRaritySprites.TryGetValue(currencyType, out Sprite sprite))
        {
            return sprite;
        }
        return _defaultCurrencyRaritySprite;
    }

    public Sprite GetRaritySprite(RarityType rarityType)
    {
        if (_itemRaritySprites != null &&
            _itemRaritySprites.TryGetValue(rarityType, out Sprite sprite))
        {
            return sprite;
        }
        return _defaultItemRaritySprite;
    }
}
