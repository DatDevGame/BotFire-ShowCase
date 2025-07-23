using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuyType
{
    Coin,
    Gem,
    Ads,
    Transformer_King
}

[CreateAssetMenu(fileName = "GarageSO", menuName = "PocketBots/GarageSystem/GarageSO")]
public class GarageSO : ItemSO
{
    [HideInInspector] public Action OnOwn = delegate { };

    [ReadOnly]
    public string m_Key;

    public BuyType BuyType => m_BuyType;
    public bool IsOwned => GetModule<UnlockableItemModule>().isUnlocked;
    public bool IsSelected => PlayerPrefs.HasKey(m_KeySelected);
    public bool IsEnoughAds => PlayerPrefs.GetInt(m_AdsCount, 0) >= m_Price;
    public int AdsWatchedCount => PlayerPrefs.GetInt(m_AdsCount);
    public int PriceCondition => m_Price;
    public int IsTrophyRoadDisplayed => m_TrophyRoadDisplayed;

    public Sprite Avatar => m_Avatar;
    public GarageModelHandle Room => m_Room;
    public string NameGarage => m_NameGarage;

    protected string m_KeyOwned => $"Owned-{m_Key}";
    protected string m_KeySelected => $"Selected-{m_Key}";
    protected string m_AdsCount => $"Ads-{m_Key}";

    [SerializeField, BoxGroup("Property")] private BuyType m_BuyType;
    [SerializeField, BoxGroup("Property")] private int m_Price;
    [SerializeField, BoxGroup("Property")] private int m_TrophyRoadDisplayed;
    [SerializeField, BoxGroup("Property")] private string m_NameGarage;
    [SerializeField, BoxGroup("Property")] private Sprite m_Avatar;
    [SerializeField, BoxGroup("Property")] private GarageModelHandle m_Room;

    public void WatchedAds(int count = 1)
    {
        int currentCount = PlayerPrefs.GetInt(m_AdsCount);
        PlayerPrefs.SetInt(m_AdsCount, currentCount + count);
    }

    [Button]
    public void Own()
    {
        GetModule<UnlockableItemModule>().TryUnlockIgnoreRequirement();
        OnOwn?.Invoke();
    }

    public void Select()
    {
        PlayerPrefs.SetInt(m_KeySelected, 1);
    }

    [Button]
    public void UnSelect()
    {
        PlayerPrefs.DeleteKey(m_KeySelected);
    }

#if UNITY_EDITOR
    [OnInspectorGUI]
    protected virtual void OnInspectorGUI()
    {
        GenerateSaveKey();
    }

    protected virtual void GenerateSaveKey()
    {
        if (string.IsNullOrEmpty(m_Key) && !string.IsNullOrEmpty(name))
        {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
            m_Key = $"{name}_{guid}";
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    [Button]
    public void ClearAllKey()
    {
        PlayerPrefs.DeleteKey(m_KeyOwned);
        PlayerPrefs.DeleteKey(m_KeySelected);
        PlayerPrefs.DeleteKey(m_AdsCount);
        GetModule<UnlockableItemModule>().ResetUnlockToDefault();
    }

    [Button]
    public void DebugPro()
    {
        Debug.Log($"Key - {IsOwned}");
    }
#endif
}
