using GachaSystem.Core;
using HightLightDebug;
using HyrphusQ.Events;
using LatteGames.Monetization;
using LatteGames.PvP;
using LatteGames.PvP.TrophyRoad;
using PBAnalyticsEvents;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using static LatteGames.PvP.TrophyRoad.TrophyRoadSO;
using static PBRobot;
using static SeasonPassSO;
using static UnityEngine.Rendering.DebugUI.Table;

public class FirebaseAnalyticsEmitter : MonoBehaviour
{
    [SerializeField, BoxGroup("Data")] private CurrentHighestArenaVariable m_CurrentHighestArenaVariable;
    [SerializeField, BoxGroup("Data")] private PBPvPTournamentSO m_PBPvPTournamentSO;
    [SerializeField, BoxGroup("Data")] private CurrencySO m_TrophyRoadPoint;
    [SerializeField, BoxGroup("Data")] private TrophyRoadSO m_TrophyRoadSO;
    [SerializeField, BoxGroup("Data")] private ModeVariable m_CurrentChosenModeVariable;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable m_ActiveIgnorePreludeMission;

    [SerializeField, BoxGroup("Data")] private PBPartManagerSO m_BodyManager;
    [SerializeField, BoxGroup("Data")] private PBPartManagerSO m_FrontManager;
    [SerializeField, BoxGroup("Data")] private PBPartManagerSO m_UpperManager;
    [SerializeField, BoxGroup("Data")] private PPrefItemSOVariable m_CurrentBodySO;
    [SerializeField, BoxGroup("Data")] private PPrefItemSOVariable m_CurrentFrontSO;
    [SerializeField, BoxGroup("Data")] private PPrefItemSOVariable m_CurrentUpper1SO;
    [SerializeField, BoxGroup("Data")] private PPrefItemSOVariable m_CurrentUpper2SO;

    private PBAnalyticsManager AnalyticsManager => PBAnalyticsManager.Instance;
    private void Awake()
    {
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.ItemEquip, FirebaseEvent_ItemEquip);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.ItemUpgrade, FirebaseEvent_ItemUpgrade);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.Tutorials, FirebaseEvent_Tutorials);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.BossFightMenu, FirebaseEvent_BossFightMenu);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.BossFight, FirebaseEvent_BossFight);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.BoxAvailable, FirebaseEvent_BoxAvailable);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.BoxOpen, FirebaseEvent_BoxOpen);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.BattleRoyale, FirebaseEvent_BattleRoyale);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.CurrencyTransaction, FirebaseEvent_CurrencyTransaction);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.FlipTiming, FirebaseEvent_FlipTiming);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.UpgradeNowShown, FirebaseEvent_UpgradeNowShown);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.UpgradeNowClicked, FirebaseEvent_UpgradeNowClicked);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.PopupAction, FirebaseEvent_PopupAction);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.IAPLocationPuchased, FirebaseEvent_IAPLocationPuchased);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.TrophyChange, FirebaseEvent_TrophyChange);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.PlayScreenReached, FirebaseEvent_PlayScreenReached);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.BackwardsMove, FirebaseEvent_BackwardsMove);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.ShopMenuReached, FirebaseEvent_ShopMenuReached);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.ItemsAvailableUpgradeChange, FirebaseEvent_ItemsAvailableUpgradeChange);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.SeasonMenuReached, FirebaseEvent_SeasonMenuReached);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.ClaimMissionReward, FirebaseEvent_ClaimMissionReward);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.ClaimSeasonReward, FirebaseEvent_ClaimSeasonReward);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.AvailableMissionReward, FirebaseEvent_AvailableMissionReward);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.AvailableSeasonReward, FirebaseEvent_AvailableSeasonReward);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.DriversMenuReached, FirebaseEvent_DriversMenuReached);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.NewDriverSelected, FirebaseEvent_NewDriverSelected);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.RequestBonusMissionsShown, FirebaseEvent_RequestBonusMissionsShown);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.RequestBonusMissionsClicked, FirebaseEvent_RequestBonusMissionsClicked);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.RefreshMissionClicked, FirebaseEvent_RefreshMissionClicked);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.LeagueStarted, FirebaseEvent_LeagueStarted);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.LeagueMenuReached, FirebaseEvent_LeagueMenuReached);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.InfoLeagueButtonClicked, FirebaseEvent_InfoLeagueButtonClicked);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.StartOfDivisionPopUp, FirebaseEvent_StartOfDivisionPopUp);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.EndOfDivisionPopUp, FirebaseEvent_EndOfDivisionPopUp);
        GameEventHandler.AddActionEvent(LogFirebaseEventCode.EndOfLeaguePopUp, FirebaseEvent_EndOfLeaguePopUp);

        m_TrophyRoadPoint.onValueChanged += TrophyRoadPoint_OnValueChanged;
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnNewArenaUnlocked, HandleNewArenaUnlocked);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, FirebaseEvent_ItemsAvailableUpgradeChange);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUpgraded, OnPartUpgraded);

        OnInitFireBaseEvent();
        OnMilestoneInitialized();
    }

    private void TrophyRoadPoint_OnValueChanged(ValueDataChanged<float> data)
    {
        float amount = data.newValue - data.oldValue;
        float balance = data.newValue;
        GameEventHandler.Invoke(LogFirebaseEventCode.TrophyChange, (int)amount, (int)balance);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.ItemEquip, FirebaseEvent_ItemEquip);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.ItemUpgrade, FirebaseEvent_ItemUpgrade);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.Tutorials, FirebaseEvent_Tutorials);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.BossFightMenu, FirebaseEvent_BossFightMenu);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.BossFight, FirebaseEvent_BossFight);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.BoxAvailable, FirebaseEvent_BoxAvailable);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.BoxOpen, FirebaseEvent_BoxOpen);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.BattleRoyale, FirebaseEvent_BattleRoyale);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.CurrencyTransaction, FirebaseEvent_CurrencyTransaction);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.FlipTiming, FirebaseEvent_FlipTiming);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.UpgradeNowShown, FirebaseEvent_UpgradeNowShown);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.UpgradeNowClicked, FirebaseEvent_UpgradeNowClicked);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.PopupAction, FirebaseEvent_PopupAction);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.IAPLocationPuchased, FirebaseEvent_IAPLocationPuchased);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.TrophyChange, FirebaseEvent_TrophyChange);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.PlayScreenReached, FirebaseEvent_PlayScreenReached);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.BackwardsMove, FirebaseEvent_BackwardsMove);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.ShopMenuReached, FirebaseEvent_ShopMenuReached);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.ItemsAvailableUpgradeChange, FirebaseEvent_ItemsAvailableUpgradeChange);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.SeasonMenuReached, FirebaseEvent_SeasonMenuReached);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.ClaimMissionReward, FirebaseEvent_ClaimMissionReward);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.ClaimSeasonReward, FirebaseEvent_ClaimSeasonReward);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.AvailableMissionReward, FirebaseEvent_AvailableMissionReward);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.AvailableSeasonReward, FirebaseEvent_AvailableSeasonReward);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.DriversMenuReached, FirebaseEvent_DriversMenuReached);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.NewDriverSelected, FirebaseEvent_NewDriverSelected);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.RequestBonusMissionsShown, FirebaseEvent_RequestBonusMissionsShown);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.RequestBonusMissionsClicked, FirebaseEvent_RequestBonusMissionsClicked);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.RefreshMissionClicked, FirebaseEvent_RefreshMissionClicked);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.LeagueStarted, FirebaseEvent_LeagueStarted);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.LeagueMenuReached, FirebaseEvent_LeagueMenuReached);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.InfoLeagueButtonClicked, FirebaseEvent_InfoLeagueButtonClicked);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.StartOfDivisionPopUp, FirebaseEvent_StartOfDivisionPopUp);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.EndOfDivisionPopUp, FirebaseEvent_EndOfDivisionPopUp);
        GameEventHandler.RemoveActionEvent(LogFirebaseEventCode.EndOfLeaguePopUp, FirebaseEvent_EndOfLeaguePopUp);

        m_TrophyRoadPoint.onValueChanged -= TrophyRoadPoint_OnValueChanged;
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnNewArenaUnlocked, HandleNewArenaUnlocked);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, FirebaseEvent_ItemsAvailableUpgradeChange);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUpgraded, OnPartUpgraded);
    }

    private void OnInitFireBaseEvent()
    {
        if (!PlayerPrefs.HasKey(FireBaseEvent.GetFirstMissionStartKey()))
        {
            PlayerPrefs.SetInt(FireBaseEvent.GetFirstMissionStartKey(), 1);
            int trophyPoints = PlayerPrefs.GetInt(FireBaseEvent.GetTrophyPointCurrentKey(), 0);

            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"currentArena", $"{GetCurrentArenaIndex()}"},
                {"trophyPoints", trophyPoints}
            };
            AnalyticsManager.SendEventMissionStarted(1, parameters);

            #region Log The First Time Arena
            string currentArenaName = m_CurrentHighestArenaVariable.value.GetModule<NameItemModule>().displayName;
            string previousArenaName = m_CurrentHighestArenaVariable.value.index == 0
                ? "null"
                : m_PBPvPTournamentSO.arenas
                    .Select((arena, index) => new { arena, index })
                    .Where(a => a.arena == m_CurrentHighestArenaVariable.value && a.index > 0)
                    .Select(a => m_PBPvPTournamentSO.arenas[a.index - 1].GetModule<NameItemModule>().displayName)
                    .FirstOrDefault() ?? "null";

            Dictionary<string, object> arenaParameters = new Dictionary<string, object>
            {
                {"missionID", GetMissionIDCurrent()},
                {"currentArena", currentArenaName},
                {"previousArena", previousArenaName},
                {"trophyPoints", (int)m_TrophyRoadPoint.value}
            };
            AnalyticsManager.LogArenaStarted(arenaParameters);
            #endregion
        }
        else
        {
            int missionID = PlayerPrefs.GetInt(FireBaseEvent.GetLastMissionStartKey());
            int trophyPoints = PlayerPrefs.GetInt(FireBaseEvent.GetTrophyPointCurrentKey());
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"currentArena", $"{GetCurrentArenaIndex()}"},
                {"trophyPoints", trophyPoints}
            };
            AnalyticsManager.SendEventMissionStarted(missionID, parameters);
        }
    }

    private void OnMilestoneInitialized()
    {
        if (m_TrophyRoadSO == null || m_TrophyRoadSO.ArenaSections == null)
            return;
        foreach (var arenaSection in m_TrophyRoadSO.ArenaSections)
        {
            foreach (var milestone in arenaSection.milestones)
            {
                milestone.OnUnlocked += OnMilestoneUnlocked;

                void OnMilestoneUnlocked()
                {
                    milestone.OnUnlocked -= OnMilestoneUnlocked;
                    MissionEvents(milestone.requiredAmount.RoundToInt());
                    ArenaEvents(milestone.requiredAmount.RoundToInt());
                }
            }
        }
    }

    private void MissionEvents(int milestone)
    {
        int missionIDOId = PlayerPrefs.GetInt(FireBaseEvent.GetLastMissionStartKey(), 1);
        int trophyPointsOld = PlayerPrefs.GetInt(FireBaseEvent.GetTrophyPointCurrentKey(), 0);
        Dictionary<string, object> missionCompletedParrams = new Dictionary<string, object>
                    {
                        {"missionID", missionIDOId},
                        {"currentArena", $"{GetCurrentArenaIndex()}"},
                        {"trophyPoints", milestone}
                    };

        var milestoneIndex = FindMilestoneIndex();
        var missionID = milestoneIndex + 1;
        int trophyPointsNew = milestone;
        Dictionary<string, object> missionStartedParrams = new Dictionary<string, object>
                    {
                        {"currentArena", $"{GetCurrentArenaIndex()}"},
                        {"trophyPoints", trophyPointsNew}
                    };

        AnalyticsManager.SendEventMissionComplete(missionCompletedParrams);
        AnalyticsManager.SendEventMissionStarted(missionID, missionStartedParrams);

        PlayerPrefs.SetInt(FireBaseEvent.GetMiniMissionKey(), 0);
        PlayerPrefs.SetInt(FireBaseEvent.GetTrophyPointCurrentKey(), milestone);
        PlayerPrefs.SetInt(FireBaseEvent.GetLastMissionStartKey(), missionID);

        int FindMilestoneIndex()
        {
            var milestoneIndex = 1;
            foreach (var section in m_TrophyRoadSO.ArenaSections)
            {
                for (int i = 0; i < section.milestones.Count; i++)
                {
                    if (section.milestones[i].requiredAmount == milestone)
                        return milestoneIndex;
                    else
                        milestoneIndex++;
                }
            }
            return milestoneIndex;
        }
    }

    private void ArenaEvents(int milestone)
    {
        int milestoneValue = milestone;
        string currentArenaName = m_CurrentHighestArenaVariable.value.GetModule<NameItemModule>().displayName;
        string previousArenaName = m_CurrentHighestArenaVariable.value.index == 0
            ? "null"
            : m_PBPvPTournamentSO.arenas
                .Select((arena, index) => new { arena, index })
                .Where(a => a.arena == m_CurrentHighestArenaVariable.value && a.index > 0)
                .Select(a => m_PBPvPTournamentSO.arenas[a.index - 1].GetModule<NameItemModule>().displayName)
                .FirstOrDefault() ?? "null";

        //if (CheckFirstMilestone())
        //{
        //    Dictionary<string, object> parameters = new Dictionary<string, object>
        //    {
        //        {"missionID", GetMissionIDCurrent()},
        //        {"currentArena", currentArenaName},
        //        {"previousArena", previousArenaName},
        //        {"trophyPoints", (int)m_TrophyRoadPoint.value}
        //    };
        //    AnalyticsManager.LogArenaStarted(parameters);
        //}

        if (CheckLastMilestone())
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                {"missionID", GetMissionIDCurrent()},
                {"currentArena", currentArenaName},
                {"previousArena", previousArenaName},
                {"trophyPoints", (int)m_TrophyRoadPoint.value}
            };
            AnalyticsManager.LogArenaCompleted(parameters);
        }

        //bool CheckFirstMilestone()
        //{
        //    foreach (var section in m_TrophyRoadSO.ArenaSections)
        //    {
        //        float firstTrophyPoints = section.milestones.FirstOrDefault().requiredAmount;
        //        DebugPro.CyanBold("FirstTrophyPoints" + firstTrophyPoints);
        //        if (milestone == firstTrophyPoints)
        //            return true;
        //    }
        //    return false;
        //}

        bool CheckLastMilestone()
        {
            foreach (var section in m_TrophyRoadSO.ArenaSections)
            {
                float lastTrophyPoints = section.milestones.LastOrDefault().requiredAmount;
                DebugPro.CyanBold("LastTrophyPoints" + lastTrophyPoints);
                if (milestone == lastTrophyPoints)
                    return true;
            }
            return false;
        }
    }

    private void FirebaseEvent_ItemEquip(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;

        PBPartSO pbPartSO = (PBPartSO)parameters[0];
        PBPartSlot pbPartSlot = (PBPartSlot)parameters[1];

        if (pbPartSO == null)
        {
            Debug.LogWarning("Missing Event FirebaseEvent_ItemEquip");
            return;
        }

        int slot = pbPartSlot switch
        {
            PBPartSlot.Upper_1 => 1,
            PBPartSlot.Upper_2 => 2,
            _ => 0
        };
        string rarity = GetRarityType(pbPartSO);

        string itemGroup = pbPartSlot.GetPartTypeOfPartSlot() switch
        {
            PBPartType.Body => "body",
            PBPartType.Upper => "upper",
            PBPartType.Front => "front",
            _ => "null"
        };

        if (pbPartSO is PBChassisSO chassisSO)
            if (chassisSO.IsSpecial)
                itemGroup = "special";

        Dictionary<string, object> itemEquipParrams = new Dictionary<string, object>
        {
            {"itemName", pbPartSO.GetModule<NameItemModule>().displayName},
            {"missionID", GetMissionIDCurrent()},
            {"itemGroup", itemGroup},
            {"slot", slot},
            {"itemLevel", pbPartSO.GetModule <UpgradableItemModule>().upgradeLevel},
            {"overalPower", pbPartSO.GetPower().value},
            {"currentArena", $"{GetCurrentArenaIndex()}"},
            {"type", $"{rarity}"},
        };

        AnalyticsManager.LogItemEquip(itemEquipParrams);
    }

    private void FirebaseEvent_ItemUpgrade(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;
        PBPartSO pbPartSO = (PBPartSO)parameters[0];
        int coinSpend = (int)parameters[1];

        if (pbPartSO == null)
        {
            Debug.LogWarning("Missing Event FirebaseEvent_ItemUpgrade");
            return;
        }
        string rarity = GetRarityType(pbPartSO);

        string itemGroup = pbPartSO.PartType switch
        {
            PBPartType.Body => "body",
            PBPartType.Upper => "upper",
            PBPartType.Front => "front",
            _ => "null"
        };

        if (pbPartSO is PBChassisSO chassisSO)
            if (chassisSO.IsSpecial)
                itemGroup = "special";
        int cardsAvailable = 0;
        int cardsNeeded = 0;
        if (pbPartSO.GetModule<PBUpgradableItemModule>().TryGetCurrentUpgradeRequirement(out Requirement_GachaCard requirement_GachaCard))
        {
            cardsAvailable = requirement_GachaCard.currentNumOfCards;
            cardsNeeded = requirement_GachaCard.requiredNumOfCards;
        }

        Dictionary<string, object> itemUpgradeParrams = new Dictionary<string, object>
        {
            {"itemName", pbPartSO.GetModule<NameItemModule>().displayName },
            {"missionID", GetMissionIDCurrent()},
            {"itemGroup", itemGroup},
            {"itemPreviousLevel", pbPartSO.GetModule<UpgradableItemModule>().upgradeLevel},
            {"itemNewLevel", pbPartSO.GetModule<UpgradableItemModule>().upgradeLevel + 1},
            {"overalPower", pbPartSO.GetPower().value},
            {"coinsSpent", coinSpend},
            {"currentArena", $"{GetCurrentArenaIndex()}"},
            {"type", $"{rarity}"},
            {"cardsAvailable", cardsAvailable},
            {"cardsNeeded", cardsNeeded}
        };

        AnalyticsManager.LogItemUpgrade(itemUpgradeParrams);
    }

    private void FirebaseEvent_Tutorials(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null || parameters[2] == null) return;

        int status = (int)parameters[0];
        string type = (string)parameters[1];
        string step = (string)parameters[2];
        Dictionary<string, object> tutorialsParrams = new Dictionary<string, object>
        {
            {"missionID",  GetMissionIDCurrent()},
            {"tutorialName", type},
            {"stepID", int.Parse(step)},
            {"stepType", "mandatory"},
            {"stepName", $"{type}_{step}"},
            {"currentArena", $"{GetCurrentArenaIndex()}"},
            {"stepStatus", status == 0 ? "started" : "completed"}
        };

        AnalyticsManager.LogTutorial(tutorialsParrams);
    }

    private void FirebaseEvent_BossFightMenu(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null)
        {
            Dictionary<string, object> bossFightMenuExitParrams = new Dictionary<string, object>
            {
                {"missionID",  GetMissionIDCurrent()},
                {"currentArena", $"{GetCurrentArenaIndex()}"}
            };

            AnalyticsManager.LogBossMenu("bossFightMenuExit", bossFightMenuExitParrams);
            return;
        }

        bool isMatchPlayed = (bool)parameters[0];
        bool isClaimedBoss = (bool)parameters[1];
        Dictionary<string, object> bossFightMenuEnterParrams = new Dictionary<string, object>
        {
            {"missionID",  GetMissionIDCurrent()},
            {"currentArena", $"{GetCurrentArenaIndex()}"},
            {"matchPlayed", isMatchPlayed},
            {"claimedBoss", isClaimedBoss}
        };

        AnalyticsManager.LogBossMenu("bossFightMenuEnter", bossFightMenuEnterParrams);
    }

    private void FirebaseEvent_BossFight(params object[] parameters)
    {
        if (parameters.Length <= 0) return;

        string status = (string)parameters[0];
        string bossName = (string)parameters[1];
        string bossDifficulty = (string)parameters[2];
        string driver = (string)parameters[3];
        int botStrength = (int)parameters[4];

        Dictionary<string, object> bossFightParrams = new Dictionary<string, object>
        {
            {"missionID",  GetMissionIDCurrent()},
            {"currentArena", $"{GetCurrentArenaIndex()}"},
            {"bossName", bossName},
            {"bossDifficulty", bossDifficulty},
            {"driver", driver},
            {"botStrength", botStrength}
        };
        string eventName = $"bossFight{status}";
        AnalyticsManager.LogBossFight(eventName, bossFightParrams);
    }

    private void FirebaseEvent_BoxAvailable(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;

        GachaPack gachaPack = (GachaPack)parameters[0];
        bool isSlotFilled = !(bool)parameters[1];

        Dictionary<string, object> BoxAvailableParrams = new Dictionary<string, object>
        {
            {"missionID",  GetMissionIDCurrent()},
            {"currentArena", $"{GetCurrentArenaIndex()}"},
            {"boxName", $"{gachaPack.GetModule<NameItemModule>().displayName}"},
            {"timeToOpen", gachaPack.UnlockedDuration},
            {"slotFilled", isSlotFilled}
        };

        AnalyticsManager.LogBoxAvailable(BoxAvailableParrams);
    }

    private void FirebaseEvent_BoxOpen(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;

        GachaPack gachaPack = (GachaPack)parameters[0];
        string openType = (string)parameters[1];

        Dictionary<string, object> BoxOpenParrams = new Dictionary<string, object>
        {
            {"missionID",  GetMissionIDCurrent()},
            {"currentArena", $"{GetCurrentArenaIndex()}"},
            {"boxName", $"{gachaPack.GetModule<NameItemModule>().displayName}"},
            {"timeToOpen", gachaPack.UnlockedDuration},
            {"openType", openType}
        };

        AnalyticsManager.LogBoxOpen(BoxOpenParrams);
    }

    private void FirebaseEvent_BattleRoyale(params object[] parameters)
    {
        if (parameters.Length <= 0) return;

        bool isCompleted = (bool)parameters[0];
        int totalMatch = (int)parameters[1];

        string driver = "";
        int botStrength = 0;

        if (isCompleted)
        {
            driver = (string)parameters[3];
            botStrength = (int)parameters[4];
            int rank = (int)parameters[2];
            Dictionary<string, object> BattleRoyaleCompletedParrams = new Dictionary<string, object>
            {
                {"missionID",  GetMissionIDCurrent()},
                {"currentArena", $"{GetCurrentArenaIndex()}"},
                {"place", rank},
                {"number", totalMatch},
                {"driver", driver},
                {"botStrength", botStrength}
            };

            AnalyticsManager.LogBattleRoyale("battleRoyaleCompleted", BattleRoyaleCompletedParrams);
            return;
        }

        driver = (string)parameters[2];
        botStrength = (int)parameters[3];
        Dictionary<string, object> BattleRoyaleStartedParrams = new Dictionary<string, object>
        {
            {"missionID",  GetMissionIDCurrent()},
            {"currentArena", $"{GetCurrentArenaIndex()}"},
            {"number", totalMatch},
            {"driver", driver},
            {"botStrength", botStrength}
        };

        AnalyticsManager.LogBattleRoyale("battleRoyaleStarted", BattleRoyaleStartedParrams);
    }

    private void FirebaseEvent_CurrencyTransaction(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null || parameters[2] == null || parameters[3] == null) return;
        int amount = (int)parameters[0];
        int balance = (int)parameters[1];
        string source = (string)parameters[2];
        string currency = (string)parameters[3];

        Dictionary<string, object> CurrencyTransactionParrams = new Dictionary<string, object>
        {
            {"missionID",  GetMissionIDCurrent()},
            {"amount", amount},
            {"balance", balance},
            {"source",  source},
            {"currentArena", $"{GetCurrentArenaIndex()}"},
            {"currency", currency}
        };

        AnalyticsManager.LogCurrencyTransaction(CurrencyTransactionParrams);
    }

    private void FirebaseEvent_FlipTiming(params object[] parameters)
    {
        int miniMissionID = PlayerPrefs.GetInt(FireBaseEvent.GetMiniMissionKey());
        string matchType = GetMatchType();

        if (parameters.Length <= 1)
        {
            if (parameters[0] != null)
            {
                //Flip Started and Completed
                string status = (string)parameters[0];
                Dictionary<string, object> FlipStartedOrCompletedParrams = new Dictionary<string, object>
                {
                    {"missionID",  GetMissionIDCurrent()},
                    {"miniMissionID", miniMissionID},
                    {"currentArena", $"{GetCurrentArenaIndex()}"},
                    {"matchType", $"{matchType}"}
                };

                string eventName = status switch
                {
                    "Start" => "flipStarted",
                    "Complete" => "flipCompleted",
                    _ => "flipStarted"
                };

                AnalyticsManager.LogFlip(eventName, FlipStartedOrCompletedParrams);
            }
            return;
        }

        //Flip Failed
        if (parameters[0] != null && parameters[1] != null)
        {
            string eventName = "flipFailed";
            bool isTimeElapsed = (bool)parameters[1];

            Dictionary<string, object> FlipFailedParrams = new Dictionary<string, object>
            {
                {"missionID",  GetMissionIDCurrent()},
                {"miniMissionID", miniMissionID},
                {"currentArena", $"{GetCurrentArenaIndex()}"},
                {"matchType", $"{matchType}"},
                {"timeElapsed", isTimeElapsed}
            };

            AnalyticsManager.LogFlip(eventName, FlipFailedParrams);
        }
    }

    private void FirebaseEvent_UpgradeNowShown()
    {
        int miniMissionID = PlayerPrefs.GetInt(FireBaseEvent.GetMiniMissionKey());
        string matchType = GetMatchType();

        Dictionary<string, object> UpgradeNowShownParrams = new Dictionary<string, object>
        {
            {"missionID",  GetMissionIDCurrent()},
            {"miniMissionID", miniMissionID},
            {"currentArena", $"{GetCurrentArenaIndex()}"},
            {"matchType", $"{matchType}"},
        };

        AnalyticsManager.LogUpgradeNowShown(UpgradeNowShownParrams);
    }

    private void FirebaseEvent_UpgradeNowClicked()
    {
        int miniMissionID = PlayerPrefs.GetInt(FireBaseEvent.GetMiniMissionKey());
        string matchType = GetMatchType();

        Dictionary<string, object> UpgradeNowShownParrams = new Dictionary<string, object>
        {
            {"missionID",  GetMissionIDCurrent()},
            {"miniMissionID", miniMissionID},
            {"currentArena", $"{GetCurrentArenaIndex()}"},
            {"matchType", $"{matchType}"},
        };

        AnalyticsManager.LogUpgradeNowClicked(UpgradeNowShownParrams);
    }

    private void FirebaseEvent_PopupAction(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null || parameters[2] == null) return;

        string popUpName = (string)parameters[0];
        string buttonClicked = (string)parameters[1];
        string openType = (string)parameters[2];

        Dictionary<string, object> popupActionParrams = new Dictionary<string, object>
        {
            {"missionID",  GetMissionIDCurrent()},
            {"currentArena", $"{GetCurrentArenaIndex()}"},
            {"popUpName", popUpName},
            {"buttonClicked", buttonClicked},
            {"openType", openType}
        };

        AnalyticsManager.LogPopupAction(popupActionParrams);
    }

    private void FirebaseEvent_IAPLocationPuchased(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;

        IAPProductSO iapProductSO = (IAPProductSO)parameters[0];
        if (iapProductSO != null)
        {
            string iapName = iapProductSO.productName;
            string location = (string)parameters[1];
            int saleTimer = iapProductSO.PurchasedTime;
            int saleTimerInMinutes = saleTimer / 60;

            Dictionary<string, object> IAPLocationPuchasedParrams = new Dictionary<string, object>
            {
                {"missionID",  GetMissionIDCurrent()},
                {"currentArena", $"{GetCurrentArenaIndex()}"},
                {"iapName", iapName},
                {"location", location},
                {"saleTimer", saleTimerInMinutes}
            };

            AnalyticsManager.LogIAPLocationPurchased(IAPLocationPuchasedParrams);
        }
    }

    private void FirebaseEvent_TrophyChange(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        int amount = (int)parameters[0];
        int balance = (int)parameters[1];
        Dictionary<string, object> TrophyChangeParrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"amount", amount},
           {"balance", balance},
           {"currentArena", $"{GetCurrentArenaIndex()}"}
        };

        AnalyticsManager.LogTrophyChange(TrophyChangeParrams);
    }

    private void FirebaseEvent_PlayScreenReached(params object[] parameters)
    {
        if (parameters.Length <= 0) return;

        int duelMatchStatus = (int)parameters[0];
        string bossFightStatus = (string)parameters[1];
        string bossDifficulty = (string)parameters[2];
        string battleRoyaleStatus = (string)parameters[3];
        int battleRoyaleRequirement = (int)parameters[4];

        string eventName = "playScreenReached";
        Dictionary<string, object> playScreenReachedParrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"duelMatchStatus", duelMatchStatus},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"bossFightStatus", bossFightStatus},
           {"bossDifficulty", bossDifficulty},
           {"battleRoyaleStatus", battleRoyaleStatus},
           {"battleRoyaleRequirement", battleRoyaleRequirement},
        };

        AnalyticsManager.CustomLogEvent(eventName, playScreenReachedParrams);
    }

    private void FirebaseEvent_BackwardsMove(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        int miniMissionID = PlayerPrefs.GetInt(FireBaseEvent.GetMiniMissionKey());
        string matchType = (string)parameters[0];

        string eventName = "backwardsMove";
        Dictionary<string, object> playScreenReachedParrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"miniMissionID", miniMissionID},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"matchType", matchType}
        };

        AnalyticsManager.CustomLogEvent(eventName, playScreenReachedParrams);
    }

    private void FirebaseEvent_ShopMenuReached(params object[] parameters)
    {
        string eventName = "shopMenuReached";
        Dictionary<string, object> playScreenReachedParrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
        };
        AnalyticsManager.CustomLogEvent(eventName, playScreenReachedParrams);
    }    
    

    private void FirebaseEvent_ItemsAvailableUpgradeChange()
    {
        int bodyAvailable = AllItemAvailableUpgrade(m_BodyManager);
        int frontAvailable = AllItemAvailableUpgrade(m_FrontManager);
        int upperAvailable = AllItemAvailableUpgrade(m_UpperManager);
        int bodyCurrentAvailable = CurrentItemAvailableUpgrade(m_CurrentBodySO.value);
        int frontCurrentAvailable = CurrentItemAvailableUpgrade(m_CurrentFrontSO.value);
        int upper1CurrentAvailable = CurrentItemAvailableUpgrade(m_CurrentUpper1SO.value);
        int upper2CurrentAvailable = CurrentItemAvailableUpgrade(m_CurrentUpper2SO.value);

        int AllItemAvailableUpgrade(PBPartManagerSO pbPartManagerSO)
        {
            int totalValue = 0;
            foreach (var item in pbPartManagerSO.initialValue)
            {
                if (item is PBChassisSO chassisSO && chassisSO.IsUnlocked() && !chassisSO.IsSpecial ||
                    item is PBPartSO partSOs && partSOs.IsUnlocked())
                {
                    totalValue += item.Cast<PBPartSO>().CalcTotalCoinsToMaxReachableUpgradeLevel();
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

        string eventName = "itemsAvailableUpgradeChange";
        Dictionary<string, object> itemsAvailableUpgradeChangeParrams = new Dictionary<string, object>
        {
           {"bodyAvailable", bodyAvailable},
           {"frontAvailable", frontAvailable},
           {"upperAvailable", upperAvailable},
           {"bodyCurrentAvailable", bodyCurrentAvailable},
           {"frontCurrentAvailable", frontCurrentAvailable},
           {"upper1CurrentAvailable", upper1CurrentAvailable},
           {"upper2CurrentAvailable", upper2CurrentAvailable},
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
        };
        AnalyticsManager.CustomLogEvent(eventName, itemsAvailableUpgradeChangeParrams);
    }
    private void OnPartUpgraded(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null) return;
        PBPartSO partSO = (PBPartSO)parameters[1];

        if (partSO != null)
        {
            #region Firebase Event
            string keyLogItemsAvailableUpgradeChangeKey = $"ItemsAvailableUpgradeChangeKey-Upgrade-{partSO.GetDisplayName()}-{partSO.GetModule<UpgradableItemModule>().upgradeLevel}";
            if (!PlayerPrefs.HasKey(keyLogItemsAvailableUpgradeChangeKey))
            {
                PlayerPrefs.SetInt(keyLogItemsAvailableUpgradeChangeKey, 1);
                FirebaseEvent_ItemsAvailableUpgradeChange();
            }
            #endregion
        }
    }

    private void FirebaseEvent_SeasonMenuReached(params object[] parameters)
    {
        if (parameters.Length <= 0) return;

        string seasonType = (string)parameters[0];
        int todayCompleted = (int)parameters[1];
        int weeklyCompleted = (int)parameters[2];
        int seasonCompleted = (int)parameters[3];
        int todayAvailable = (int)parameters[4];
        int weeklyAvailable = (int)parameters[5];
        int seasonAvailable = (int)parameters[6];
        int missionsCompleted = (int)parameters[7];
        int totalMissions = (int)parameters[8];
        string tabOpened = (string)parameters[9];
        int seasonID = (int)parameters[10];

        string eventName = "seasonMenuReached";
        Dictionary<string, object> seasonMenuReachedParrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"seasonType", seasonType},
           {"todayCompleted", todayCompleted},
           {"weeklyCompleted", weeklyCompleted},
           {"seasonCompleted", seasonCompleted},
           {"todayAvailable", todayAvailable},
           {"weeklyAvailable", weeklyAvailable},
           {"seasonAvailable", seasonAvailable},
           {"missionsCompleted", missionsCompleted},
           {"totalMissions", totalMissions},
           {"tabOpened", tabOpened},
           {"seasonID", seasonID},
        };
        AnalyticsManager.CustomLogEvent(eventName, seasonMenuReachedParrams);
    }

    private void FirebaseEvent_ClaimMissionReward(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        string seasonType = (string)parameters[0];
        int missionsCompleted = (int)parameters[1];
        int totalMissions = (int)parameters[2];
        int missionNumber = (int)parameters[3];
        string missionName = (string)parameters[4];
        string missionType = (string)parameters[5];
        int seasonID = (int)parameters[6];

        string eventName = "claimMissionReward";
        Dictionary<string, object> claimMissionRewardParrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"seasonType", seasonType},
           {"missionsCompleted", missionsCompleted},
           {"totalMissions", totalMissions},
           {"missionNumber", missionNumber},
           {"missionName", missionName},
           {"missionType", missionType},
           {"seasonID", seasonID}
        };
        AnalyticsManager.CustomLogEvent(eventName, claimMissionRewardParrams);
    }
    private void FirebaseEvent_ClaimSeasonReward(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        string seasonType = (string)parameters[0];
        int missionsCompleted = (int)parameters[1];
        int rewardID = (int)parameters[2];
        int totalRewards = (int)parameters[3];
        int seasonID = (int)parameters[4];
        string typeOfReward = (string)parameters[5];
        int todayCompleted = (int)parameters[6];
        int weeklyCompleted = (int)parameters[7];
        int seasonCompleted = (int)parameters[8];
        int todayAvailable = (int)parameters[9];
        int weeklyAvailable = (int)parameters[10];
        int seasonAvailable = (int)parameters[11];

        string eventName = "claimSeasonReward";
        Dictionary<string, object> claimSeasonRewardParrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"seasonType", seasonType},
           {"missionsCompleted", missionsCompleted},
           {"rewardID", rewardID},
           {"totalRewards", totalRewards},
           {"seasonID", seasonID},
           {"typeOfReward", typeOfReward},
           {"todayCompleted", todayCompleted},
           {"weeklyCompleted", weeklyCompleted},
           {"seasonCompleted", seasonCompleted},
           {"todayAvailable", todayAvailable},
           {"weeklyAvailable", weeklyAvailable},
           {"seasonAvailable", seasonAvailable}
        };
        AnalyticsManager.CustomLogEvent(eventName, claimSeasonRewardParrams);
    }
    private void FirebaseEvent_AvailableMissionReward(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        string seasonType = (string)parameters[0];
        int missionsCompleted = (int)parameters[1];
        int totalMissions = (int)parameters[2];
        int missionNumber = (int)parameters[3];
        string missionName = (string)parameters[4];
        string missionType = (string)parameters[5];
        int seasonID = (int)parameters[6];

        string eventName = "availableMissionReward";
        Dictionary<string, object> availableMissionRewardParrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"seasonType", seasonType},
           {"missionsCompleted", missionsCompleted},
           {"totalMissions", totalMissions},
           {"missionNumber", missionNumber},
           {"missionName", missionName},
           {"missionType", missionType},
           {"seasonID", seasonID},
        };
        AnalyticsManager.CustomLogEvent(eventName, availableMissionRewardParrams);
    }
    
    private void FirebaseEvent_AvailableSeasonReward(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        string seasonType = (string)parameters[0];
        int missionsCompleted = (int)parameters[1];
        int rewardID = (int)parameters[2];
        int totalRewards = (int)parameters[3];
        int seasonID = (int)parameters[4];
        string typeOfReward = (string)parameters[5];
        int todayCompleted = (int)parameters[6];
        int weeklyCompleted = (int)parameters[7];
        int seasonCompleted = (int)parameters[8];
        int todayAvailable = (int)parameters[9];
        int weeklyAvailable = (int)parameters[10];
        int seasonAvailable = (int)parameters[11];

        string eventName = "availableSeasonReward";
        Dictionary<string, object> availableSeasonRewardParrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"seasonType", seasonType},
           {"missionsCompleted", missionsCompleted},
           {"rewardID", rewardID},
           {"totalRewards", totalRewards},
           {"seasonID", seasonID},
           {"typeOfReward", typeOfReward},
           {"todayCompleted", todayCompleted},
           {"weeklyCompleted", weeklyCompleted},
           {"seasonCompleted", seasonCompleted},
           {"todayAvailable", todayAvailable},
           {"weeklyAvailable", weeklyAvailable},
           {"seasonAvailable", seasonAvailable},
        };
        AnalyticsManager.CustomLogEvent(eventName, availableSeasonRewardParrams);
    }
    private void FirebaseEvent_DriversMenuReached(params object[] parameters)
    {
        string eventName = "driversMenuReached";
        Dictionary<string, object> parrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"}
        };
        AnalyticsManager.CustomLogEvent(eventName, parrams);
    }
    private void FirebaseEvent_NewDriverSelected(params object[] parameters)
    {
        string eventName = "newDriverSelected";
        Dictionary<string, object> parrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"}
        };
        AnalyticsManager.CustomLogEvent(eventName, parrams);
    }
    private void FirebaseEvent_RequestBonusMissionsShown(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        int seasonID = (int)parameters[0];
        string eventName = "requestBonusMissionsShown";
        Dictionary<string, object> parrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"seasonID", seasonID}
        };
        AnalyticsManager.CustomLogEvent(eventName, parrams);
    }   
    private void FirebaseEvent_RequestBonusMissionsClicked(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        int seasonID = (int)parameters[0];
        string eventName = "requestBonusMissionsClicked";
        Dictionary<string, object> parrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"seasonID", seasonID}
        };
        AnalyticsManager.CustomLogEvent(eventName, parrams);
    }    
    private void FirebaseEvent_RefreshMissionClicked(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        int seasonID = (int)parameters[0];
        string missionType = (string)parameters[1];
        string missionName = (string)parameters[2];

        string eventName = "refreshMissionClicked";
        Dictionary<string, object> parrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"seasonID", seasonID},
           {"missionType", missionType},
           {"missionName", missionName}
        };
        AnalyticsManager.CustomLogEvent(eventName, parrams);
    }    
    private void FirebaseEvent_LeagueStarted(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        int trophyPoints = (int)parameters[0];
        string eventName = "leagueStarted";
        Dictionary<string, object> parrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"trophyPoints", trophyPoints}
        };
        AnalyticsManager.CustomLogEvent(eventName, parrams);
    }    
    private void FirebaseEvent_LeagueMenuReached(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        int dayCurrentLeague = (int)parameters[0];
        string division = (string)parameters[1];
        int position = (int)parameters[2];
        int numberOfPlayers = (int)parameters[3];
        string eventName = "leagueMenuReached";
        Dictionary<string, object> parrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"dayCurrentLeague", dayCurrentLeague},
           {"division", division},
           {"position", position},
           {"numberOfPlayers", numberOfPlayers},
        };
        AnalyticsManager.CustomLogEvent(eventName, parrams);
    }
    private void FirebaseEvent_InfoLeagueButtonClicked(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        int dayCurrentLeague = (int)parameters[0];
        string division = (string)parameters[1];
        int position = (int)parameters[2];
        int numberOfPlayers = (int)parameters[3];
        string eventName = "infoLeagueButtonClicked";

        Dictionary<string, object> parrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"dayCurrentLeague", dayCurrentLeague},
           {"division", division},
           {"position", position},
           {"numberOfPlayers", numberOfPlayers},
        };
        AnalyticsManager.CustomLogEvent(eventName, parrams);
    }
    private void FirebaseEvent_StartOfDivisionPopUp(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        string division = (string)parameters[0];
        int numberOfPlayers = (int)parameters[1];

        string eventName = "startOfDivisionPopUp";
        Dictionary<string, object> parrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"division", division},
           {"numberOfPlayers", numberOfPlayers},
        };
        AnalyticsManager.CustomLogEvent(eventName, parrams);
    }
    private void FirebaseEvent_EndOfDivisionPopUp(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        string currentdivisionEvent = (string)parameters[0];
        string nextDivisionEvent = (string)parameters[1];
        int position = (int)parameters[2];
        int numberOfPlayers = (int)parameters[3];

        string eventName = "endOfDivisionPopUp";
        Dictionary<string, object> parrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"division", currentdivisionEvent},
           {"nextDivision", nextDivisionEvent},
           {"position", position},
           {"numberOfPlayers", numberOfPlayers},
        };
        AnalyticsManager.CustomLogEvent(eventName, parrams);
    }
    private void FirebaseEvent_EndOfLeaguePopUp(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        string division = (string)parameters[0];

        string eventName = "endOfLeaguePopUp";
        Dictionary<string, object> parrams = new Dictionary<string, object>
        {
           {"missionID", GetMissionIDCurrent()},
           {"currentArena", $"{GetCurrentArenaIndex()}"},
           {"division", division}
        };
        AnalyticsManager.CustomLogEvent(eventName, parrams);
    }

    private void HandleNewArenaUnlocked(object[] objs)
    {
        if (objs[0] is not PvPArenaSO arenaSO) return;

        string currentArenaName = m_CurrentHighestArenaVariable.value.GetModule<NameItemModule>().displayName;
        string previousArenaName = m_CurrentHighestArenaVariable.value.index == 0
            ? "null"
            : m_PBPvPTournamentSO.arenas
                .Select((arena, index) => new { arena, index })
                .Where(a => a.arena == m_CurrentHighestArenaVariable.value && a.index > 0)
                .Select(a => m_PBPvPTournamentSO.arenas[a.index - 1].GetModule<NameItemModule>().displayName)
                .FirstOrDefault() ?? "null";

        Dictionary<string, object> arenaParameters = new Dictionary<string, object>
            {
                {"missionID", GetMissionIDCurrent()},
                {"currentArena", currentArenaName},
                {"previousArena", previousArenaName},
                {"trophyPoints", (int)m_TrophyRoadPoint.value}
            };
        AnalyticsManager.LogArenaStarted(arenaParameters);
    }

    private string GetRarityType(PBPartSO partSO)
    {
        string rarity = partSO.GetModule<RarityItemModule>().rarityType switch
        {
            RarityType.Uncommon => "uncommon",
            RarityType.Common => "common",
            RarityType.Epic => "epic",
            RarityType.Legendary => "legendary",
            RarityType.Mythic => "mythic",

        };
        return rarity;
    }

    private int GetMissionIDCurrent()
    {
        if (!PlayerPrefs.HasKey(FireBaseEvent.GetLastMissionStartKey()))
            PlayerPrefs.SetInt(FireBaseEvent.GetLastMissionStartKey(), 1);
        return PlayerPrefs.GetInt(FireBaseEvent.GetLastMissionStartKey(), 1);
    }

    private int GetCurrentMilestone()
    {
        List<TrophyRoadSO.Milestone> milestones = m_TrophyRoadSO.ArenaSections
        .SelectMany(section => section.milestones)
        .ToList();

        if (!milestones.First().Unlocked)
            return 0;

        if (milestones.Last().Unlocked)
            return milestones.IndexOf(milestones.Last()) + 1;

        var previousUnlockedMilestone = milestones
            .TakeWhile(milestone => milestone.Unlocked)
            .LastOrDefault();

        return previousUnlockedMilestone != null ? milestones.IndexOf(previousUnlockedMilestone) + 1 : 0;
    }
    private int GetIDCurrentMilestone()
    {
        List<TrophyRoadSO.Milestone> milestones = m_TrophyRoadSO.ArenaSections
        .SelectMany(section => section.milestones)
        .ToList();

        if (!milestones.First().Unlocked)
            return 0;

        if (milestones.Last().Unlocked)
            return milestones.IndexOf(milestones.Last()) + 1;

        var previousUnlockedMilestone = milestones
            .TakeWhile(milestone => milestone.Unlocked)
            .LastOrDefault();

        return previousUnlockedMilestone != null ? milestones.IndexOf(previousUnlockedMilestone) + 1 : 0;
    }


    private int GetCurrentArenaIndex()
    {
        if(m_CurrentHighestArenaVariable == null)
            Debug.LogWarning($"CurrentHighestArenaVariabl Null");

        return m_CurrentHighestArenaVariable.value.index + 1;
    }

    private string GetMatchType()
    {
        string matchType = m_CurrentChosenModeVariable.value switch
        {
            Mode.Normal => "Duel Match",
            Mode.Boss => "Boss Fight",
            Mode.Battle => "Battle Royale",
            _ => "null"
        };

        return matchType;
    }
}
