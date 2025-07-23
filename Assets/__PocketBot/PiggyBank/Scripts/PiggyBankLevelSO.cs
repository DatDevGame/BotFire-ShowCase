using LatteGames.Monetization;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PiggyBankLevelSO", menuName = "PocketBots/PiggyBank/PiggyBankLevelSO")]
public class PiggyBankLevelSO : SerializedScriptableObject
{
    [ReadOnly, PropertyOrder(0)]
    public string m_Key;

    [BoxGroup("Preview"), ShowInInspector, PropertyOrder(1), ReadOnly]
    public float SavedGems
    {
        get 
        {
            if (IAPProductSO != null && IAPProductSO.currencyItems != null)
            {
                return IAPProductSO.currencyItems[CurrencyType.Premium].value;
            }
            return 0;
        }
    }

    [BoxGroup("Config")] public int PerKill;
    [BoxGroup("Resource")] public Sprite Avatar;
    [BoxGroup("Resource")] public Sprite SmallAvatar;
    [BoxGroup("Data")] public IAPProductSO IAPProductSO;

#if UNITY_EDITOR
    [OnInspectorGUI]
    protected virtual void LoadEditor()
    {
        GenerateSaveKey();
    }

    protected virtual void GenerateSaveKey()
    {
        if (string.IsNullOrEmpty(m_Key) && !string.IsNullOrEmpty(name) && m_Key != name)
        {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
            m_Key = $"{name}_{guid}";
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}

