using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using PBAnalyticsEvents;
using System.Linq;
using LatteGames.PvP.TrophyRoad;
using static LatteGames.PvP.TrophyRoad.TrophyRoadSO;
using Sirenix.OdinInspector;
using static PBRobot;
using System;

public class PBGachaItemAnalyticsEventEmitter : MonoBehaviour
{
    [SerializeField]
    private List<PBPartManagerSO> m_PartInventorySOs;

    [SerializeField, BoxGroup("Data")] PPrefItemSOVariable chassisSO;
    [SerializeField, BoxGroup("Data")] PPrefItemSOVariable frontSO;
    [SerializeField, BoxGroup("Data")] PPrefItemSOVariable upper_1;
    [SerializeField, BoxGroup("Data")] PPrefItemSOVariable upper_2;
    [SerializeField, BoxGroup("Data")] private PBPartManagerSO m_BodyManager;
    [SerializeField, BoxGroup("Data")] private PBPartManagerSO m_FrontManager;
    [SerializeField, BoxGroup("Data")] private PBPartManagerSO m_UpperManager;
    [SerializeField, BoxGroup("Data")] private TrophyRoadSO m_TrophyRoadSO;
    [SerializeField, BoxGroup("Data")] private CurrentHighestArenaVariable m_CurrentHighestArenaVariable;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUnlocked, OnGearUnlocked);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUpgraded, OnGearUpgraded);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnPartCardChanged);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUpgraded, OnPartCardChanged);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUnlocked, OnGearUnlocked);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUpgraded, OnGearUpgraded);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnPartCardChanged);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUpgraded, OnPartCardChanged);
    }

    private void Start()
    {
        OnInitCardOnChanged();
    }

    private void OnPartCardChanged(params object[] parrameters)
    {
        #region Firebase Event
        int bodyAvailable = AllItemAvailableUpgrade(m_BodyManager);
        int frontAvailable = AllItemAvailableUpgrade(m_FrontManager);
        int upperAvailable = AllItemAvailableUpgrade(m_UpperManager);
        int bodyCurrentAvailable = CurrentItemAvailableUpgrade(chassisSO.value);
        int frontCurrentAvailable = CurrentItemAvailableUpgrade(frontSO.value);
        int upper1CurrentAvailable = CurrentItemAvailableUpgrade(upper_1.value);
        int upper2CurrentAvailable = CurrentItemAvailableUpgrade(upper_2.value);

        GameEventHandler.Invoke(LogFirebaseEventCode.ItemsAvailableUpgradeChange, bodyAvailable, frontAvailable, upperAvailable, bodyCurrentAvailable, frontCurrentAvailable, upper1CurrentAvailable, upper2CurrentAvailable);

        int AllItemAvailableUpgrade(PBPartManagerSO pbPartManagerSO)
        {
            int totalValue = 0;
            foreach (var item in pbPartManagerSO.initialValue)
            {
                if (item is PBChassisSO chassisSO && chassisSO.IsUnlocked() && !chassisSO.IsSpecial ||
                    item is PBPartSO partSOs && partSOs.IsUnlocked())
                {
                    if (item.TryGetModule<UpgradableItemModule>(out var upgradableItemModule) && upgradableItemModule.isAbleToUpgrade)
                    {
                        if (upgradableItemModule.TryGetCurrentUpgradeRequirement<Requirement_Currency>(out var requirement))
                        {
                            totalValue += (int)requirement.requiredAmountOfCurrency;
                        }
                    }
                }
            }
            return totalValue;
        }
        int CurrentItemAvailableUpgrade(ItemSO itemSO)
        {
            if (itemSO is not PBPartSO && itemSO is not PBChassisSO)
                return 0;
            if (!itemSO.TryGetModule<UpgradableItemModule>(out var upgradableItemModule))
                return 0;
            if (!upgradableItemModule.isAbleToUpgrade)
                return -1;
            if (upgradableItemModule.TryGetCurrentUpgradeRequirement<Requirement_Currency>(out var requirement))
                return (int)requirement.requiredAmountOfCurrency;
            return 0;
        }
        #endregion
    }

    private void OnInitCardOnChanged()
    {
        for (int i = 0; i < m_PartInventorySOs.Count; i++)
        {
            for(int x = 0; x < m_PartInventorySOs[i].initialValue.Count; x++)
            {
                if (m_PartInventorySOs[i].initialValue[x].TryGetModule<CardItemModule>(out var cardItemModule))
                {
                    cardItemModule.onNumOfCardsChanged += CardItemModule_OnNumOfCardsChanged;
                }
            }
        }
    }

    private void CardItemModule_OnNumOfCardsChanged(CardItemModule cardItemModule, int numOfCards)
    {
        if (cardItemModule.itemSO is PBPartSO partSOHandle)
        {
            PBUpgradableItemModule upgradableItemModule = partSOHandle.GetModule<PBUpgradableItemModule>();
            if (upgradableItemModule != null)
            {
                if (partSOHandle.IsEnoughCardToUpgrade())
                {
                    for (int i = 0; i < partSOHandle.CalMaxReachableUpgradeLevel() + 1; i++)
                    {
                        if (i > (upgradableItemModule.defaultUpgradeLevel) && i <= (upgradableItemModule.maxUpgradeLevel))
                        {
                            string key = $"LogCalcUpgrade{partSOHandle.GetModule<NameItemModule>().displayName} - {i}";
                            if (!PlayerPrefs.HasKey(key))
                            {
                                string name = partSOHandle.GetInternalName();
                                int upgradeStepIndex = i;
                                PBAnalyticsManager.Instance.LogCalcUpgrade(name, upgradeStepIndex, m_CurrentHighestArenaVariable.value.index + 1);
                                PlayerPrefs.SetInt(key, 1);
                            }
                        }
                    }
                }
            }
        }
    }

    private int CalTotalNumOfUnlockedGears()
    {
        int total = 0;
        foreach (var gearManagerSO in m_PartInventorySOs)
        {
            total += gearManagerSO.Where(part => part.IsUnlocked() &&part.TryGetModule<UnlockableItemModule>(out var module) && !module.defaultIsUnlocked).Count();
        }
        return total;
    }

    private void OnGearUnlocked(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;

        PBPartSO partSO = parameters[1] as PBPartSO;

        bool isCall = true;
        if (partSO.Cast<PBChassisSO>() != null)
        {
            if (partSO.Cast<PBChassisSO>().IsSpecial)
                isCall = false;
        }
        if (!isCall) return;

        PBAnalyticsManager.Instance.Owned($"{partSO.GetInternalName()}", "Unlocked", CalTotalNumOfUnlockedGears(), GetMissionIDCurrent(), m_CurrentHighestArenaVariable.value.index + 1);
    }

    private int CalTotalNumOfMaxUpgradedGears()
    {
        int total = 0;
        foreach (var gearManagerSO in m_PartInventorySOs)
        {
            total += gearManagerSO.Where(part => part.IsMaxUpgradeLevel()).Count();
        }
        return total;
    }

    private void OnGearUpgraded(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        var partSO = parameters[1] as PBPartSO;

        if (!PlayerPrefs.HasKey((partSO.name + partSO.GetCurrentUpgradeLevel()).ToString()))
        {
            int currentArena = m_CurrentHighestArenaVariable.value.index + 1;
            PBAnalyticsManager.Instance.Upgrade($"{partSO.GetRarityType()}", $"{partSO.GetInternalName()}", partSO.GetCurrentFormIndex(), partSO.GetCurrentUpgradeLevel() - 1, currentArena);
            PlayerPrefs.SetString((partSO.name + partSO.GetCurrentUpgradeLevel()).ToString(), "");
        }


        if (partSO.IsMaxUpgradeLevel())
        {
            //PBAnalyticsManager.Instance.Owned($"{partSO.name}", "Max Upgrade", CalTotalNumOfMaxUpgradedGears(), GetCurrentMilestone());
        }
    }

    private int GetCurrentMilestone()
    {
        List<Milestone> milestones = m_TrophyRoadSO.ArenaSections
        .SelectMany(section => section.milestones)
        .ToList();

        if (!milestones.First().Unlocked)
            return 0;

        if (milestones.Last().Unlocked)
            return (int)milestones.Last().requiredAmount;

        var previousUnlockedMilestone = milestones
            .TakeWhile(milestone => milestone.Unlocked)
            .LastOrDefault();

        return previousUnlockedMilestone != null ? (int)previousUnlockedMilestone.requiredAmount : 0;
    }

    private int GetMissionIDCurrent()
    {
        if (!PlayerPrefs.HasKey(FireBaseEvent.GetLastMissionStartKey()))
            PlayerPrefs.SetInt(FireBaseEvent.GetLastMissionStartKey(), 1);
        return PlayerPrefs.GetInt(FireBaseEvent.GetLastMissionStartKey(), 1);
    }
}