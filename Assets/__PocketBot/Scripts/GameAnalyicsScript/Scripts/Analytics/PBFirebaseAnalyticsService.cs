using System.Collections.Generic;
using LatteGames.Analytics;
using UnityEngine;
using PBAnalyticsEvents;

#if LatteGames_CLIK
using Tabtale.TTPlugins;
#endif

public class PBFirebaseAnalyticsService : AnalyticsService,
    PBAnalyticsEvents.FireBaseEvent.ILogger
{

    private void Awake()
    {
#if LatteGames_CLIK
        //Init CLIK
        TTPCore.Setup();
#endif
    }

    public override void SendEventLog(string eventKey, Dictionary<string, object> additionData)
    {
        // Do nothing
    }

    public void SendEventMissionStarted(int missionID, Dictionary<string, object> parameters = null)
    {
#if LatteGames_CLIK
        TTPGameProgression.FirebaseEvents.MissionStarted(missionID, parameters ?? new Dictionary<string, object>());
#endif
    }

    public void SendEventMissionComplete(Dictionary<string, object> parameters = null)
    {
#if LatteGames_CLIK
        TTPGameProgression.FirebaseEvents.MissionComplete(parameters ?? new Dictionary<string, object>());
#endif
    }

    public void SendEventMissionFailed(Dictionary<string, object> parameters = null)
    {
#if LatteGames_CLIK
        TTPGameProgression.FirebaseEvents.MissionFailed(parameters ?? new Dictionary<string, object>());
#endif
    }

    public void LogMiniLevelStarted(Dictionary<string, object> parameters = null)
    {
#if LatteGames_CLIK        
        int miniMissionID = PlayerPrefs.GetInt(FireBaseEvent.GetMiniMissionKey(), 0);
        PlayerPrefs.SetInt(FireBaseEvent.GetMiniMissionKey(), miniMissionID + 1);
        miniMissionID = PlayerPrefs.GetInt(FireBaseEvent.GetMiniMissionKey(), 1);
        TTPGameProgression.MiniLevelStarted(miniMissionID, parameters ?? new Dictionary<string, object>());
#endif
    }

    public void LogMiniLevelCompleted()
    {
#if LatteGames_CLIK
        TTPGameProgression.MiniLevelCompleted();
#endif
    }

    public void LogMiniLevelFailed()
    {
#if LatteGames_CLIK
        TTPGameProgression.MiniLevelFailed();
#endif
    }
    public void LogArenaStarted(Dictionary<string, object> parameters = null)
    {
        //CustomLogEvent("arenaStarted", parameters);
    }

    public void LogArenaCompleted(Dictionary<string, object> parameters = null)
    {
        //CustomLogEvent("arenaCompleted", parameters);
    }

    public void LogItemEquip(Dictionary<string, object> parameters = null)
    {
        CustomLogEvent("itemEquip", parameters);
    }

    public void LogItemUpgrade(Dictionary<string, object> parameters = null)
    {
        CustomLogEvent("itemUpgrade", parameters);
    }

    public void LogTutorial(Dictionary<string, object> parameters = null)
    {
        CustomLogEvent("tutorialStep", parameters);
    }

    public void LogBossMenu(string eventName, Dictionary<string, object> parameters = null)
    {
        CustomLogEvent(eventName, parameters);
    }

    public void LogBossFight(string eventName, Dictionary<string, object> parameters = null)
    {
        CustomLogEvent(eventName, parameters);
    }

    public void LogBoxAvailable(Dictionary<string, object> parameters = null)
    {
        CustomLogEvent("boxAvailable", parameters);
    }

    public void LogBoxOpen(Dictionary<string, object> parameters = null)
    {
        CustomLogEvent("boxOpen", parameters);
    }

    public void LogBattleRoyale(string eventName, Dictionary<string, object> parameters = null)
    {
        CustomLogEvent(eventName, parameters);
    }    
    
    public void LogCurrencyTransaction(Dictionary<string, object> parameters = null)
    {
        CustomLogEvent("currencyTransaction", parameters);
    }

    public void LogFlip(string eventName, Dictionary<string, object> parameters = null)
    {
        CustomLogEvent(eventName, parameters);
    }

    public void LogUpgradeNowShown(Dictionary<string, object> parameters = null)
    {
        CustomLogEvent("upgradeNowShown", parameters);
    }

    public void LogUpgradeNowClicked(Dictionary<string, object> parameters = null)
    {
        CustomLogEvent("upgradeNowClicked", parameters);
    }

    public void LogPopupAction(Dictionary<string, object> parameters = null)
    {
        CustomLogEvent("popUpAction", parameters);
    }

    public void LogIAPLocationPurchased(Dictionary<string, object> parameters = null)
    {
        CustomLogEvent("iapLocationPurchased", parameters);
    }

    public void LogTrophyChange(Dictionary<string, object> parameters = null)
    {
        CustomLogEvent("trophyChange", parameters);
    }

    public void CustomLogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
#if LatteGames_CLIK
        TTPAnalytics.LogEvent(eventName, parameters, false);
#endif
    }
}