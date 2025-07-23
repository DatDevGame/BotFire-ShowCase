using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.PvP;
using TMPro;
using UnityEngine;

public class RPSBarUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_ScoreTextOfPlayer;
    [SerializeField]
    private TextMeshProUGUI m_ScoreTextOfOpponent;
    [SerializeField]
    private TextMeshProUGUI m_RPSStateText;
    [SerializeField]
    private RectTransform m_Arrow;
    [SerializeField]
    private AnimationCurve m_CurveX, m_CurveY;
    [SerializeField]
    private RPSCalculatorSO m_RPSCalculatorSO;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchPrepared, OnMatchPrepared);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchPrepared, OnMatchPrepared);
    }

    private void OnMatchPrepared(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch pvpMatch) return;
        if (pvpMatch.mode == Mode.Battle)
        {
            gameObject.SetActive(false);
            return;
        }
        var localPlayer = pvpMatch.GetLocalPlayerInfo().Cast<PBPlayerInfo>().robotStatsSO;
        var botPlayer = pvpMatch.GetOpponentInfo().Cast<PBPlayerInfo>().robotStatsSO;
        var rpsData = m_RPSCalculatorSO.CalcRPSValue(localPlayer, botPlayer);
        m_ScoreTextOfPlayer.SetText(rpsData.scoreOfPlayer.ToRoundedText());
        m_ScoreTextOfOpponent.SetText(rpsData.scoreOfOpponent.ToRoundedText());
        m_RPSStateText.SetText(rpsData.stateLabel);
        m_Arrow.anchoredPosition = new Vector2(m_CurveX.Evaluate(rpsData.rpsInverseLerp), m_CurveY.Evaluate(rpsData.rpsInverseLerp));
    }
}