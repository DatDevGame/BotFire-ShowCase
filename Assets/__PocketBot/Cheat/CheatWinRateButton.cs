using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HyrphusQ.Events;
using HyrphusQ.Helpers;
using LatteGames.PvP;
using LatteGames.PvP.TrophyRoad;
using Sirenix.OdinInspector;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class CheatWinRateButton : MonoBehaviour
{
    [Serializable]
    public class PartData
    {
        public int upgradeLevel = 1;
        public PBPartSO partSO;
    }
    [Serializable]
    public class ArenaData
    {
        public PBPvPArenaSO arenaSO;
        public List<PartData> dataOfParts = new List<PartData>();
    }

    [SerializeField]
    private CurrentHighestArenaVariable currentHighestArenaVariable;
    [SerializeField]
    private PPrefFloatVariable currentMedals;
    [SerializeField]
    private PPrefFloatVariable highestAchievedMedals;
    [SerializeField]
    private TrophyRoadSO trophyRoadSO;
    // [SerializeField]
    // private TMP_Dropdown arenaDropdown;
    // [SerializeField]
    // private Button restartButton;
    [SerializeField]
    private TMP_Text winRatePercentageText;
    [SerializeField]
    private PvPTournamentSO tournamentSO;
    [SerializeField]
    private List<PBPartManagerSO> partManagerSOs;
    [SerializeField]
    private List<PPrefItemSOVariable> currentPartsInUse;
    [SerializeField, TableList]
    private List<ArenaData> dataOfArenas = new List<ArenaData>();

    private void Awake()
    {
        GameEventHandler.AddActionEvent(SceneManagementEventCode.OnLoadSceneCompleted, OnLoadSceneCompleted);
        // restartButton.onClick.AddListener(() => UnlockArena(arenaDropdown.value));
    }

    private void OnLoadSceneCompleted(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        if ((parameters[0] as string) == SceneName.MainScene.ToString())
        {
            UpdateWinRateText();
        }
    }

    private void UpdateWinRateText()
    {
        var stringBuilder = new StringBuilder();
        for (int i = 0; i < tournamentSO.arenas.Count; i++)
        {
            var arenaSO = tournamentSO.arenas[i].Cast<PBPvPArenaSO>();
            var winRateDuel = arenaSO.totalNumOfWonMatches_Normal / (float)Mathf.Max(1, arenaSO.totalNumOfPlayedMatches_Normal) * 100f;
            var winRateBattle = arenaSO.totalNumOfWonMatches_Battle / (float)Mathf.Max(1, arenaSO.totalNumOfPlayedMatches_Battle) * 100f;
            stringBuilder.AppendLine($"Arena {arenaSO.index + 1}: {winRateDuel.Ceil(2)}% - {winRateBattle.Ceil(2)}%");
        }
        winRatePercentageText.text = stringBuilder.ToString();
    }

//     private IEnumerator Start()
//     {
//         yield return new WaitUntil(() => arenaDropdown.TryGetComponentInChildren(out Canvas canvas));
//         var canvases = arenaDropdown.GetComponentsInChildren<Canvas>(true);
//         foreach (var canvas in canvases)
//         {
//             canvas.sortingOrder = short.MaxValue;
//         }
//     }

//     private void UnlockArena(int index)
//     {
//         // Skip all FTUE
//         GetComponent<CheatSkipAllFTUEButton>().InvokeMethod("OnCheatButtonClicked");
//         // Reset data
//         trophyRoadSO.Delete();
//         foreach (var currentPartInUse in currentPartsInUse)
//         {
//             currentPartInUse.ResetValue();
//             currentPartInUse.value = null;
//         }
//         foreach (var partManagerSO in partManagerSOs)
//         {
//             foreach (var partSO in partManagerSO.Parts)
//             {
//                 partSO.IsEquipped = false;
//                 if (partSO.TryGetModule(out UnlockableItemModule unlockModule))
//                     unlockModule.ResetUnlockToDefault();
//                 if (partSO.TryGetModule(out UpgradableItemModule upgradeModule))
//                     upgradeModule.ResetUpgradeLevelToDefault();
//                 if (partSO.TryGetModule(out NewItemModule newModule))
//                     newModule.isNew = false;
//             }
//         }
//         foreach (var dataOfArena in dataOfArenas)
//         {
//             dataOfArena.arenaSO.ResetUnlockToDefault();
//         }
//         currentHighestArenaVariable.ResetValue();
//         // Set new data
//         var arenaData = dataOfArenas[index];
//         var numOfTrophies = arenaData.arenaSO.RequiredNumOfTrophiesToUnlock;
//         currentMedals.value = numOfTrophies;
//         highestAchievedMedals.value = numOfTrophies;
//         foreach (var partData in arenaData.dataOfParts)
//         {
//             var partSO = partData.partSO;
//             if (partSO is PBChassisSO chassisSO)
//             {
//                 if (currentPartsInUse[0].value == null)
//                     currentPartsInUse[0].value = chassisSO;
//             }
//             partSO.TryUnlockIgnoreRequirement(isSetNew: false);
//             if (partSO.TryGetModule(out UpgradableItemModule upgradeModule))
//             {
//                 var propertyInfo = upgradeModule.GetType().GetProperty("upgradeLevel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
//                 propertyInfo.SetValue(upgradeModule, partData.upgradeLevel);
//             }
//         }
// #if UNITY_EDITOR
//         if (EditorApplication.isPlaying)
//             EditorApplication.ExitPlaymode();
// #else
//             Application.Quit();
// #endif
//     }
}