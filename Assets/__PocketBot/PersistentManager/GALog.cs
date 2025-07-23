using HyrphusQ.Events;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PBRobot;


public class GALog : MonoBehaviour
{
    [SerializeField, BoxGroup("Data")] private List<PBPartManagerSO> m_PartInventorySOs;
    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUnlocked, OnGearUnlocked);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUpgraded, OnPartUpgraded);
    }
    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUnlocked, OnGearUnlocked);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUpgraded, OnPartUpgraded);
    }

    private void Start()
    {
        OnCheckDefaultPart();
        OnInitCardOnChanged();
    }
    private void OnCheckDefaultPart()
    {
        for (int i = 0; i < m_PartInventorySOs.Count; i++)
        {
            for (int x = 0; x < m_PartInventorySOs[i].initialValue.Count; x++)
            {
                if (m_PartInventorySOs[i].initialValue[x].TryGetModule<UnlockableItemModule>(out var unlockableItemModule))
                {
                    if (m_PartInventorySOs[i].initialValue[x] is PBPartSO partSO)
                    {
                        if (unlockableItemModule.defaultIsUnlocked)
                        {
                            int upgradeStepIndex = 1;
                            string keyStart = $"Start-LogCalcUpgrade{partSO.GetModule<NameItemModule>().displayName} - {upgradeStepIndex}";
                            if (!PlayerPrefs.HasKey(keyStart))
                            {
                                GameEventHandler.Invoke(PSDesignEvent.Upgrade, "Start", partSO, upgradeStepIndex);
                                PlayerPrefs.SetInt(keyStart, 1);
                            }

                            string keyComplete = $"Complete-LogCalcUpgrade{partSO.GetModule<NameItemModule>().displayName} - {upgradeStepIndex}";
                            if (!PlayerPrefs.HasKey(keyComplete))
                            {
                                GameEventHandler.Invoke(PSDesignEvent.Upgrade, "Complete", partSO, upgradeStepIndex);
                                PlayerPrefs.SetInt(keyComplete, 1);
                            }
                        }
                    }
                }
            }
        }
    }
    private void OnInitCardOnChanged()
    {
        for (int i = 0; i < m_PartInventorySOs.Count; i++)
        {
            for (int x = 0; x < m_PartInventorySOs[i].initialValue.Count; x++)
            {
                if (m_PartInventorySOs[i].initialValue[x].TryGetModule<CardItemModule>(out var cardItemModule))
                {
                    cardItemModule.onNumOfCardsChanged += CardItemModule_OnNumOfCardsChanged;
                }
            }
        }
    }

    private void CardItemModule_OnNumOfCardsChanged(CardItemModule data, int someInt)
    {
        if (data.itemSO is PBPartSO partSOHandle)
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
                            string status = "Start";
                            string key = $"{status}-LogCalcUpgrade{ partSOHandle.GetModule<NameItemModule>().displayName} - {i}";
                            if (!PlayerPrefs.HasKey(key))
                            {
                                int upgradeStepIndex = i;
                                GameEventHandler.Invoke(PSDesignEvent.Upgrade, status, partSOHandle, upgradeStepIndex);
                                PlayerPrefs.SetInt(key, 1);
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnMatchStarted(params object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete)
            return;
        if (matchOfPlayer.GetLocalPlayerInfo() is not PBPlayerInfo player)
            return;

        PBChassisSO pBChassisSO = player.robotStatsSO.chassisInUse.value.Cast<PBChassisSO>();

        int emptySlot = 0;
        int allSlot = pBChassisSO.AllPartSlots.Count;
        for (int i = 0; i < allSlot; i++)
        {
            if (pBChassisSO.AllPartSlots[i].PartVariableSO.value == null)
                emptySlot++;
        }

        GameEventHandler.Invoke(PSDesignEvent.PlayWithUnequipped, emptySlot);
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete)
            return;
    }

    private void OnGearUnlocked(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        PBPartSO partSO = parameters[1] as PBPartSO;

        GameEventHandler.Invoke(PSDesignEvent.UnlockNewItem, partSO);
        int upgradeStepIndex = 1;
        GameEventHandler.Invoke(PSDesignEvent.Upgrade, "Start", partSO, upgradeStepIndex);
        GameEventHandler.Invoke(PSDesignEvent.Upgrade, "Complete", partSO, upgradeStepIndex);

    }

    private void OnPartUpgraded(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        var partSO = parameters[1] as PBPartSO;

        if (partSO != null)
        {
            string status = "Complete";
            int upgradeStepIndex = partSO.GetCurrentUpgradeLevel();

            string key = $"{status}-LogCalcUpgrade{partSO.GetModule<NameItemModule>().displayName} - {upgradeStepIndex}";
            if (!PlayerPrefs.HasKey(key))
            {
                GameEventHandler.Invoke(PSDesignEvent.Upgrade, status, partSO, upgradeStepIndex);
                PlayerPrefs.SetInt(key, 1);
            }
        }
    }
}
