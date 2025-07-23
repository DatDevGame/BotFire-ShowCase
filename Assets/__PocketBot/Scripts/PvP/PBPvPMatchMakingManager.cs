using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;

public class PBPvPMatchMakingManager : MonoBehaviour
{
    [SerializeField]
    protected EventCode m_OpponentFoundedEventCode;
    [SerializeField]
    protected PvPArenaVariable m_ChosenArenaVariable;
    [SerializeField]
    protected BattleBetArenaVariable m_BattleBetArenaVariable;
    [SerializeField]
    protected ModeVariable m_ChosenModeVariable;
    [SerializeField]
    protected PBPvPMatchMakingSO m_MatchmakingSO;
    [SerializeField]
    protected TestMatchMakingSO testMatchMakingSO;

    protected virtual void Start()
    {
        var chosenArena = m_ChosenArenaVariable.value as PBPvPArenaSO;
        if (testMatchMakingSO != null && testMatchMakingSO.IsTest)
        {
            Time.timeScale = 5f;
            var opponentVariable = ScriptableObject.CreateInstance<PlayerInfoVariable>();
            opponentVariable.value = m_MatchmakingSO.FindOpponent(chosenArena, testMatchMakingSO);
            GameEventHandler.Invoke(m_OpponentFoundedEventCode, chosenArena, opponentVariable);
        }
        else
        {
            if (m_ChosenModeVariable.value == Mode.Normal)
                StartCoroutine(FindOpponentNormalModeSingleMatch_CR(chosenArena));
            else if (m_ChosenModeVariable.value == Mode.Battle)
            {
                //StartCoroutine(FindOpponentBattleModeSingleMatch_CR());
                StartCoroutine(FindOpponentBattleModeTeamMatch_CR());
            }

        }
    }

    protected virtual IEnumerator FindOpponentNormalModeSingleMatch_CR(PBPvPArenaSO arenaSO)
    {
        yield return m_MatchmakingSO.FindOpponent_CR(arenaSO, OnOpponentFounded);

        void OnOpponentFounded(PBPlayerInfo opponent)
        {
            var opponentVariable = ScriptableObject.CreateInstance<PlayerInfoVariable>();
            opponentVariable.value = opponent;
            GameEventHandler.Invoke(m_OpponentFoundedEventCode, arenaSO, opponentVariable);
        }
    }

    // protected virtual IEnumerator FindOpponentBattleModeSingleMatch_CR()
    // {
    //     var rewardArena = m_BattleBetArenaVariable.RewardArena;
    //     var opponentArena = m_BattleBetArenaVariable.OpponentProfileArena;
    //     var opponentPersonalInfo = new List<PersonalInfo>();
    //     m_MatchmakingSO.SetupForBattleMode(opponentArena, rewardArena.numOfContestant - 1);
    //     yield return new WaitForSeconds(0.1f);
    //     for (int i = 0; i < rewardArena.numOfContestant - 1; i++) //Find opponent exclude Player
    //     {
    //         m_MatchmakingSO.battleBotIndex = i;
    //         StartCoroutine(m_MatchmakingSO.FindOpponent_CR(opponentArena, OnOpponentFounded1, OnOpponentFounded, predicate: opponent => opponentPersonalInfo.Find(item => item.avatar == opponent.personalInfo.avatar) == null));
    //         yield return new WaitForSeconds(0.1f);

    //         void OnOpponentFounded1(PBPlayerInfo opponent)
    //         {
    //             opponentPersonalInfo.Add(opponent.personalInfo);
    //         }

    //         void OnOpponentFounded(PBPlayerInfo opponent)
    //         {
    //             var opponentVariable = ScriptableObject.CreateInstance<PlayerInfoVariable>();
    //             opponentVariable.value = opponent;
    //             GameEventHandler.Invoke(m_OpponentFoundedEventCode, rewardArena, opponentVariable);
    //         }
    //     }
    // }

    protected virtual IEnumerator FindOpponentBattleModeTeamMatch_CR()
    {
        yield return null;
        var rewardArena = m_BattleBetArenaVariable.RewardArena;
        var opponentArena = m_BattleBetArenaVariable.OpponentProfileArena;
        var opponentPersonalInfo = new List<PersonalInfo>();
        m_MatchmakingSO.SetupForBattleMode(opponentArena, rewardArena.numOfContestant - 1);
        //yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < rewardArena.numOfContestant - 1; i++)
        {
            m_MatchmakingSO.battleBotIndex = i;
            StartCoroutine(i < 3 ?
            m_MatchmakingSO.FindTeammate_CR(opponentArena, OnOpponentFounded1, OnOpponentFounded, predicate: opponent => opponentPersonalInfo.Find(item => item.avatar == opponent.personalInfo.avatar) == null) :
            m_MatchmakingSO.FindOpponent_CR(opponentArena, OnOpponentFounded1, OnOpponentFounded, predicate: opponent => opponentPersonalInfo.Find(item => item.avatar == opponent.personalInfo.avatar) == null));
            //yield return new WaitForSeconds(0.1f);

            void OnOpponentFounded1(PBPlayerInfo opponent)
            {
                opponentPersonalInfo.Add(opponent.personalInfo);
                var opponentVariable = ScriptableObject.CreateInstance<PlayerInfoVariable>();
                opponentVariable.value = opponent;
                GameEventHandler.Invoke(m_OpponentFoundedEventCode, rewardArena, opponentVariable);
            }

            void OnOpponentFounded(PBPlayerInfo opponent)
            {
                // var opponentVariable = ScriptableObject.CreateInstance<PlayerInfoVariable>();
                // opponentVariable.value = opponent;
                // GameEventHandler.Invoke(m_OpponentFoundedEventCode, rewardArena, opponentVariable);
            }
        }
    }
}
