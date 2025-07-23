using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "GarageManagerSO", menuName = "PocketBots/GarageSystem/GarageManagerSO")]

public class GarageManagerSO : SerializedScriptableObject
{

    public bool IsDisplayed => m_HighestTrophyroad.value >= m_Displayed;
    public List<GarageSO> GarageSOs => m_GarageSOs;
    public Dictionary<BuyType, Sprite> BannerSpecial => m_BannerSpecial;

    [SerializeField, BoxGroup("Config")] private int m_Displayed;

    [SerializeField, BoxGroup("Data")] private List<GarageSO> m_GarageSOs;
    [SerializeField, BoxGroup("Data")] private HighestAchievedPPrefFloatTracker m_HighestTrophyroad;
    [SerializeField, BoxGroup("Data")] private Dictionary<BuyType, Sprite> m_BannerSpecial;

    public bool IsDisplayedGarage(GarageSO garageSO) => m_HighestTrophyroad.value >= m_GarageSOs.Find(v => v == garageSO).IsTrophyRoadDisplayed;

}
