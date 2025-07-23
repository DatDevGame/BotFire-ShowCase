using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames.PvP;

[CreateAssetMenu(fileName = "TotalNumOfMatchesVariable", menuName = "PocketBots/PvP/TotalNumOfMatches")]
public class TotalNumOfMatchesVariable : IntVariable
{
    [SerializeField]
    protected List<PvPTournamentSO> m_TournamentSOs;

    public override int value
    {
        get
        {
            var totalNumOfMatches = 0;
            foreach (var tournamentSO in m_TournamentSOs)
            {
                foreach (var arenaSO in tournamentSO.arenas)
                {
                    totalNumOfMatches += arenaSO.totalNumOfPlayedMatches;
                }
            }
            return totalNumOfMatches;
        }
        set
        {
            // Do nothing
        }
    }
}