using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class BattleRoyaleLeaderboardUI : MonoBehaviour
{
    public event Action onContinue = delegate { };

    [SerializeField]
    private Button m_ContinueButton;
    [SerializeField]
    private BattleRoyaleLeaderboardRow m_LocalPlayerRow;
    [SerializeField]
    private List<BattleRoyaleLeaderboardRow> m_PlayerRows;

    private PBPvPMatch m_MatchOfPlayer;
    private IUIVisibilityController m_VisibilityController;

    private void Awake()
    {
        ObjectFindCache<BattleRoyaleLeaderboardUI>.Add(this);
        m_ContinueButton.onClick.AddListener(() =>
        {
            m_ContinueButton.interactable = false;
            Hide();
            onContinue.Invoke();
        });
        m_VisibilityController = GetComponentInChildren<IUIVisibilityController>();
        m_LocalPlayerRow.Hide();
        m_PlayerRows.ForEach(item => item.Hide());
    }

    private void Start()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnDestroy()
    {
        ObjectFindCache<BattleRoyaleLeaderboardUI>.Remove(this);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnAnyPlayerDied, OnAnyPlayerDied);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnAnyPlayerDied(object[] parameters)
    {
        if (m_MatchOfPlayer == null)
            return;
        InitData();
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        if (matchOfPlayer.mode != Mode.Battle)
            return;
        m_MatchOfPlayer = matchOfPlayer;
        //TODO: Hide IAP & Popup
        //Show(InitData);
        // Handle time up case
        PBLevelController levelController = ObjectFindCache<PBLevelController>.Get();
        levelController.OnTimeUp += InitData;
    }

    private void InitData()
    {
        int index = m_PlayerRows.Count - 1;
        for (int i = m_MatchOfPlayer.survivingContestantOrders.Count - 1; i >= 0; i--)
        {
            if (m_MatchOfPlayer.survivingContestantOrders[i].isLocal)
            {
                bool isNull = m_LocalPlayerRow.playerInfo == null;
                if (isNull)
                {
                    m_LocalPlayerRow.Init(m_MatchOfPlayer, m_MatchOfPlayer.survivingContestantOrders[i].Cast<PBPlayerInfo>());
                    m_LocalPlayerRow.Show();
                    m_LocalPlayerRow.transform.SetSiblingIndex(m_MatchOfPlayer.rankOfMine - 1);
                }
            }
            else
            {
                if (index >= 0 && m_PlayerRows[index].playerInfo == null)
                {
                    m_PlayerRows[index].Init(m_MatchOfPlayer, m_MatchOfPlayer.survivingContestantOrders[i].Cast<PBPlayerInfo>());
                    m_PlayerRows[index].Show();
                }
                index--;
            }
        }
    }

    private void Show(Action callback, float delayTime = AnimationDuration.SSHORT)
    {
        StartCoroutine(CommonCoroutine.Delay(delayTime, false, () =>
        {
            m_VisibilityController.Show();
            m_VisibilityController.GetOnEndShowEvent().Subscribe(() =>
            {
                callback.Invoke();
            });
        }));
    }

    private void Hide()
    {
        m_VisibilityController.Hide();
    }
}