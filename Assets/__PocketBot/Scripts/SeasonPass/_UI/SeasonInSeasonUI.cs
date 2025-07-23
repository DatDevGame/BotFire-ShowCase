using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SeasonInSeasonUI : MonoBehaviour
{
    public SeasonMissionUI SeasonMissionUI => seasonMissionUI;

    [SerializeField] SeasonRewardUI seasonRewardUI;
    [SerializeField] SeasonMissionUI seasonMissionUI;
    [SerializeField] SeasonInSeasonHeaderUI seasonInSeasonHeaderUI;

    bool isInitialized = false;

    public void InitOrRefresh()
    {
        if (!isInitialized)
        {
            Init();
        }
        else
        {
            Refresh();
        }
        seasonRewardUI.InitOrRefresh();
        seasonMissionUI.InitOrRefresh();
    }

    private void Refresh()
    {
        seasonInSeasonHeaderUI.SwitchToDefaultTab();
    }

    public void Init()
    {
        isInitialized = true;
        seasonInSeasonHeaderUI.Init();
        seasonMissionUI.header = seasonInSeasonHeaderUI;
    }

    private void Update()
    {
        if (isInitialized)
        {
            seasonInSeasonHeaderUI.UpdateView();
        }
    }
}
