using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GachaSystem.Core;
using HyrphusQ.Events;
using UnityEngine;
using UnityEngine.UI;

public class CheatCompleteMission : MonoBehaviour
{
    [SerializeField] MissionSavedDataSO missionSavedDataSO;
    [SerializeField] bool isPrelude;
    [SerializeField] MissionScope missionScope;

    private void Awake()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            List<MissionData> missionDataList = null;
            if (isPrelude)
            {
                missionDataList = missionSavedDataSO.data.PreludeMissions;
            }
            else
            {
                switch (missionScope)
                {
                    case MissionScope.Daily:
                        missionDataList = missionSavedDataSO.data.DailyMissions;
                        break;
                    case MissionScope.Weekly:
                        missionDataList = missionSavedDataSO.data.WeeklyMissions;
                        break;
                    case MissionScope.Season:
                        missionDataList = missionSavedDataSO.data.SeasonMissions;
                        break;
                }
            }
            var uncompletedMission = missionDataList.FindAll(x => !x.isCompleted);
            var mission = uncompletedMission.First();
            mission.SetProgress(mission.targetValue);
            GameEventHandler.Invoke(SeasonPassEventCode.UpdateSeasonUI);
        });
    }
}
