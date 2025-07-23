using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HyrphusQ.Events;
using LatteGames.PvP;

public class CheatWinPvPButton : MonoBehaviour
{
    [SerializeField]
    private Button m_WinButton;
    private PBLevelController PBLevelController;
    private void Start()
    {
        m_WinButton.onClick.AddListener(OnWinButtonClicked);
    }

    private void OnDestroy()
    {
        m_WinButton.onClick.RemoveListener(OnWinButtonClicked);
    }

    private void OnWinButtonClicked()
    {
        if(PBLevelController == null)
            PBLevelController = ObjectFindCache<PBLevelController>.Get();
        if (PBLevelController != null)
            PBLevelController.SetVictory(true);

        var matchManager = ObjectFindCache<PBPvPMatchManager>.Get();
        if (matchManager != null)
        {
            GameEventHandler.Invoke(PBPvPEventCode.OnLeaveInMiddleOfMatch, matchManager.GetCurrentMatchOfPlayer(), matchManager.GetCurrentMatchOfPlayer().GetOpponentInfo());
        }
    }
}