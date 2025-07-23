using HyrphusQ.Events;
using LatteGames.Monetization;
using UnityEngine;
using TMPro;

public class PBPackDockGAHandle : MonoBehaviour
{
    [SerializeField] private RVButtonBehavior speedUpByAds;
    [SerializeField] private TMP_Text titleBoxText;

    private void Awake()
    {
        speedUpByAds.OnRewardGranted += SpeedUpByAds_OnRewardGranted;
        GameEventHandler.AddActionEvent(GachaPackDockEventCode.OnOpenNowByGem, OnOpenNowByGem);
        GameEventHandler.AddActionEvent(GachaPackDockEventCode.OnOpenNowByRV, OnOpenNowByRV);
        GameEventHandler.AddActionEvent(GachaPackDockEventCode.OnOpenByProgression, OnOpenByProgression);
    }

    private void OnDestroy()
    {
        speedUpByAds.OnRewardGranted -= SpeedUpByAds_OnRewardGranted;
        GameEventHandler.RemoveActionEvent(GachaPackDockEventCode.OnOpenNowByGem, OnOpenNowByGem);
        GameEventHandler.RemoveActionEvent(GachaPackDockEventCode.OnOpenNowByRV, OnOpenNowByRV);
        GameEventHandler.RemoveActionEvent(GachaPackDockEventCode.OnOpenByProgression, OnOpenByProgression);
    }

    private void SpeedUpByAds_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData obj)
    {
        RaiseEventSpeedUp();
    }

    private void RaiseEventSpeedUp()
    {
        string adsLocation = "BoxPopup";
        string input = $"{titleBoxText.text}";
        string typeBox = input.Split(' ')[0];
        GameEventHandler.Invoke(MonetizationEventCode.SpeedUpBox, adsLocation, typeBox);
    }

    private void OnOpenNowByGem(params object[] parrams)
    {
        string openStatus = "OpenNow_Gems";
        string location = "BoxSlot";

        if (parrams.Length > 0)
        {
            if (parrams[0] != null)
                location = (string)parrams[0];
        }

        GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
    }

    private void OnOpenNowByRV(params object[] parrams)
    {
        string openStatus = "OpenNow_RV";
        string location = "BoxSlot";

        if (parrams.Length > 0)
        {
            if(parrams[0] != null)
                location = (string)parrams[0];
        }

        GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
    }

    private void OnOpenByProgression(params object[] parrams)
    {
        if (parrams.Length <= 0 || parrams[0] == null) return;
        bool IsSemiStandardOpenStatus = (bool)parrams[0];
        string openStatus = IsSemiStandardOpenStatus ? "SemiStandard" : "Standard";
        string location = "BoxSlot";
        GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
    }
}
