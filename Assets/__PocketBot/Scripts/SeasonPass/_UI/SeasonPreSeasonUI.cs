using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonPreSeasonUI : MonoBehaviour
{
    [SerializeField] TMP_Text cooldownTxt;

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
    }

    private void Refresh()
    {

    }

    public void Init()
    {
        isInitialized = true;
    }

    private void Update()
    {
        if (isInitialized)
        {
            cooldownTxt.text = SeasonPassManager.Instance.preSeasonRemainingTime;
        }
    }
}
