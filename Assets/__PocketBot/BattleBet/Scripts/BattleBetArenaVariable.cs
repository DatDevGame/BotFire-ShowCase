using LatteGames.PvP;
using UnityEngine;

[CreateAssetMenu(fileName = "BattleBetArenaSelectVariable", menuName = "LatteGames/PvP/BattleBetArenaSelectVariable")]
public class BattleBetArenaVariable: Variable<PBPvPArenaSO>
{
    [SerializeField] protected CurrentHighestArenaVariable _highestArenaVar;
    [SerializeField] protected PvPTournamentSO _pvpTournamentSO;
    public PBPvPArenaSO OpponentProfileArena
    {
        get 
        {
            if (value == _highestArenaVar.value)
            {
                return value;
            }
            return _pvpTournamentSO.arenas[Mathf.Max(1, _highestArenaVar.value.index -1)] as PBPvPArenaSO;
        }
    }

    public PBPvPArenaSO StageArena => value;

    public PBPvPArenaSO RewardArena => value;
}