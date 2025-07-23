using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames.PvP;
using HyrphusQ.Events;
using System;
using System.Linq;
using System.Diagnostics;
using System.Text;
using TMPro;

public class PBPvPAliveDeadStateUI : MonoBehaviour
{
    [SerializeField] TMP_Text aliveTxt, deadTxt;
    int totalBots;
    int deadAmount;
    private void Start()
    {
        var matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
        var match = matchManager.GetCurrentMatchOfPlayer() as PBPvPMatch;
        if (match.mode == Mode.Battle)
        {
            gameObject.SetActive(true);
            totalBots = match.Contestants.Count;
            deadAmount = 0;
            UpdateView();

            GameEventHandler.AddActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
    }

    void OnAnyPlayerDied()
    {
        deadAmount++;
        deadAmount = Mathf.Min(totalBots, deadAmount);
        UpdateView();
    }

    void UpdateView()
    {
        aliveTxt.text = (totalBots - deadAmount).ToString();
        deadTxt.text = deadAmount.ToString();
    }
}