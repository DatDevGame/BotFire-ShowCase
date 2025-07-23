using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "BossSO", menuName = "PocketBots/BossFight/BossSO")]
public class BossSO : SerializedScriptableObject
{
    [SerializeField] string m_Key;
    public AIBotInfo botInfo;
    public PBPvPStage stage;
    public PBChassisSO chassisSO;
    public float unlockedTrophyAmount = 0;
    public int claimByRVAmount = 1;
    public PB_AIProfile AIProfile;
    [TitleGroup("Product Values"), PropertyOrder(200)]
    public RewardGroupInfo gameOverRewardGroupInfo, bossMapRewardGroupInfo;

    string IsClaimedPPrefKey => $"{m_Key}_IsClaimed";
    string IsFirstTimeFightingPPrefKey => $"{m_Key}_IsFirstTimeFighting";
    public bool IsClaimed
    {
        get
        {
            return PlayerPrefs.GetInt(IsClaimedPPrefKey, 0) == 1 ? true : false;
        }
        set
        {
            PlayerPrefs.SetInt(IsClaimedPPrefKey, value == true ? 1 : 0);
        }
    }
    public bool IsFirstTimeFighting
    {
        get
        {
            return PlayerPrefs.GetInt(IsFirstTimeFightingPPrefKey, 0) == 1 ? true : false;
        }
        set
        {
            PlayerPrefs.SetInt(IsFirstTimeFightingPPrefKey, value == true ? 1 : 0);
        }
    }
    public string ClaimByRVAmountKey => $"{m_Key}_ClaimByRVAmount";
    public string Key => m_Key;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        GenerateSaveKey();
    }
    protected virtual void GenerateSaveKey()
    {
        if (string.IsNullOrEmpty(m_Key) && !string.IsNullOrEmpty(name))
        {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath((Object)this);
            var guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
            m_Key = $"{name}_{guid}";
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }

    [Button]
    public void ClearData()
    {
        PlayerPrefs.DeleteKey(IsClaimedPPrefKey);
        chassisSO.IsClaimedRV = false;
    }

    [Button]
    public void SetBossPart()
    {
        foreach (var item in gameOverRewardGroupInfo.generalItems)
        {
            if (item.Key is PBPartSO)
            {
                ((PBPartSO)item.Key).IsBossPart = true;
                UnityEditor.EditorUtility.SetDirty(item.Key);
            }
        }
    }
#endif
}

public class RewardGroupInfo
{
    public Dictionary<CurrencyType, ShopProductSO.DiscountableValue> currencyItems;
    public Dictionary<ItemSO, ShopProductSO.DiscountableValue> generalItems;
    public Dictionary<PPrefIntVariable, ShopProductSO.DiscountableValue> consumableItems;
}
