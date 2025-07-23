using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
#if LatteGames_CLIK
using Tabtale.TTPlugins;
#endif

public class RateUsManager : MonoBehaviour
{
    [SerializeField] RateUsConfigSO rateUsConfigSO;
    [SerializeField] HighestAchievedPPrefFloatTracker highestTrophyAchieved;
    [SerializeField] PPrefFloatVariable timeCumulative;
    [SerializeField] PPrefBoolVariable firstTimeShow;

    bool isEnoughTime => timeCumulative.value >= rateUsConfigSO.FirstShowTimeThreshold;
    bool isEnoughTrophy => highestTrophyAchieved.value >= rateUsConfigSO.FirstShowTrophyThreshold;

    private void Awake()
    {
        if (!firstTimeShow.value)
        {
            GameEventHandler.AddActionEvent(PBPvPEventCode.OnShowGameOverUI, OnShowGameOverUI);
            StartCoroutine(CR_TrackTimeCumulative());
        }
    }

    private void OnDestroy()
    {
        if (!firstTimeShow.value)
        {
            GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnShowGameOverUI, OnShowGameOverUI);
        }
    }

    IEnumerator CR_TrackTimeCumulative()
    {
        while (!isEnoughTime)
        {
            timeCumulative.value += Time.deltaTime;
            yield return null;
        }
    }

    private void OnShowGameOverUI(object[] parameters)
    {
        if (!isEnoughTime || !isEnoughTrophy)
            return;
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        if (!matchOfPlayer.isAbleToComplete)
            return;
        var isVictory = matchOfPlayer.isVictory;
        if (isVictory)
        {
            StartCoroutine(CommonCoroutine.Delay(0.3f, false, () =>
            {
#if LatteGames_CLIK
                TTPRateUs.Popup();
#endif
#if UNITY_EDITOR
                Debug.Log("Show RateUs");
#endif
                firstTimeShow.value = true;
                GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnShowGameOverUI, OnShowGameOverUI);
            }));
        }
    }
}
