using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames.PvP;
using HyrphusQ.Events;

[CreateAssetMenu(fileName = "PvPTournamentSO", menuName = "PocketBots/PvP/TournamentSO")]
public class PBPvPTournamentSO : PvPTournamentSO
{
    [SerializeField]
    private CurrentHighestArenaVariable m_CurrentHighestArenaVar;

    protected void OnEnable()
    {
        m_CurrentHighestArenaVar.onValueChanged += OnArenaUnlocked;
    }

    protected void OnDisable()
    {
        m_CurrentHighestArenaVar.onValueChanged -= OnArenaUnlocked;
    }

    protected void OnArenaUnlocked(ValueDataChanged<PvPArenaSO> eventData)
    {
        GameEventHandler.Invoke(PBPvPEventCode.OnNewArenaUnlocked, eventData.newValue as PBPvPArenaSO);
    }
}
