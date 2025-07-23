using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames;
using LatteGames.PvP;
using HyrphusQ.Events;
using LatteGames.GameManagement;
using HyrphusQ.Helpers;
public class PBPvPStateGameController : StateGameController
{
    protected bool m_IsSurrendered = false;
    protected PvPMatchManager m_MatchManager;
    protected override void Awake()
    {
        ObjectFindCache<PBPvPStateGameController>.Add(this);
        base.Awake();
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchCompleted, OnMatchCompleted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnLeaveInMiddleOfMatch, OnLeaveInMiddleOfMatch);
    }
    protected virtual void Start()
    {
        m_MatchManager = ObjectFindCache<PBPvPMatchManager>.Get();
    }
    protected virtual void OnDestroy()
    {
        ObjectFindCache<PBPvPStateGameController>.Remove(this);
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchCompleted, OnMatchCompleted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnLeaveInMiddleOfMatch, OnLeaveInMiddleOfMatch);
    }
    protected override IEnumerator GameLoopCR(GameSession session)
    {
        yield return base.GameLoopCR(session);
        var currentMatchOfPlayer = m_MatchManager.GetCurrentMatchOfPlayer();
        m_MatchManager.EndRound(currentMatchOfPlayer, currentSession.LevelController.IsVictory()
            ? currentMatchOfPlayer.GetLocalPlayerInfo()
            : currentMatchOfPlayer.GetOpponentInfo(),
            m_IsSurrendered);
        // Restart new game
        if (!currentMatchOfPlayer.isAbleToComplete)
        {
            StartNextLevel();
        }
    }
    protected void OnMatchCompleted(object[] parameters)
    {
        // if (parameters == null || parameters.Length <= 0)
        //     return;
        // var match = parameters[0] as PvPMatch;
        // var isVictory = match.endgameData.winner == match.GetLocalPlayerInfo();
        // if (!isVictory)
        //     LeaveGame();
    }
    protected void OnLeaveInMiddleOfMatch(object[] parameters)
    {
        m_IsSurrendered = true;
        //if (currentSession == null || parameters == null || parameters.Length <= 0)
        //    return;
        //var playerInfo = parameters[1] as PBPlayerInfo;
        //(currentSession.LevelController as PBLevelController).SetVictory(!playerInfo.isLocal);
    }
    protected void OnMatchStarted()
    {
        StartNextLevel();
    }
    protected void OnUnpackDone()
    {
        //LeaveGame();
    }
    public void LeaveGame()
    {
        if (m_MatchManager != null)
        {
            var matchOfPlayer = m_MatchManager.GetCurrentMatchOfPlayer();
            m_MatchManager.EndMatch(matchOfPlayer);
        }
#if UNITY_EDITOR
        var loadSceneReqStack = SceneManager.Instance.GetFieldValue<Stack<SceneManager.LoadSceneRequest>>("s_LoadSceneRequestStack");
        if (loadSceneReqStack.Count <= 0)
            SceneManager.LoadScene(SceneName.MainScene, isPushToStack: false);
        else
            SceneManager.BackToPreviousScene();
#else
        SceneManager.BackToPreviousScene();
#endif
    }
    public bool IsSurrender()
    {
        return m_IsSurrendered;
    }
}