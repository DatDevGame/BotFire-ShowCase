using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;
using LatteGames.PvP;
using Sirenix.OdinInspector;
using UnityEditor;

[CreateAssetMenu(fileName = "PvPMatchmakingDecodingNumber", menuName = "PocketBots/PvP/PvPMatchmakingDecodingNumber")]
public class MatchmakingDecodingNumber : PPrefIntVariable
{
    [SerializeField] Mode mode;
    [SerializeField] ModeVariable m_CurrentChosenModeVariable;
    [SerializeField] CurrentHighestArenaVariable currentHighestArenaVariable;

    private PBPvPArenaSO m_ArenaSO => (PBPvPArenaSO)currentHighestArenaVariable.value;
    public virtual int m_TotalNumOfLostMatchesInColumn
    {
        get
        {
            return PlayerPrefs.GetInt($"{m_ArenaSO.guid}_m_TotalNumOfLostMatchesInColumn_{mode}", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{m_ArenaSO.guid}_m_TotalNumOfLostMatchesInColumn_{mode}", value);
        }
    }
    public virtual int m_TotalNumOfWonMatchesInColumn
    {
        get
        {
            return PlayerPrefs.GetInt($"{m_ArenaSO.guid}_m_TotalNumOfWonMatchesInColumn_{mode}", 0);
        }
        set
        {
            PlayerPrefs.SetInt($"{m_ArenaSO.guid}_m_TotalNumOfWonMatchesInColumn_{mode}", value);
        }
    }

    private void OnEnable()
    {
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUpgraded, OnAnyItemUpgraded);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchPrepared, OnMatchPrepared);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchCompleted, OnMatchCompleted);
        currentHighestArenaVariable.onValueChanged += OnHighestArenaChanged;
    }

    private void OnDisable()
    {
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUpgraded, OnAnyItemUpgraded);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchPrepared, OnMatchPrepared);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchCompleted, OnMatchCompleted);
        currentHighestArenaVariable.onValueChanged -= OnHighestArenaChanged;
    }

    private void OnHighestArenaChanged(ValueDataChanged<PvPArenaSO> data)
    {
        ResetValue();
        m_TotalNumOfLostMatchesInColumn = 0;
        m_TotalNumOfWonMatchesInColumn = 0;
    }

    private bool IsInHardCodedMatch()
    {
        if (mode == Mode.Normal)
        {
            return m_ArenaSO.totalNumOfPlayedMatches_Normal < m_ArenaSO.NumOfHardCodedMatches;
        }
        else if (mode == Mode.Battle)
        {
            return m_ArenaSO.totalNumOfPlayedMatches_Battle < m_ArenaSO.NumOfHardCodedMatchesBattle;
        }
        return true;
    }

    private void OnAnyItemUpgraded()
    {
        if (IsInHardCodedMatch())
            return;
        value = 20;
    }

    private void OnMatchPrepared(object[] parameters)
    {
        if (m_CurrentChosenModeVariable.value != mode)
        {
            return;
        }
        if (parameters == null || parameters.Length <= 0)
            return;
        var match = parameters[0] as PvPMatch;
        if (match.arenaSO != m_ArenaSO) return;
        value++;
    }

    private void OnMatchCompleted(object[] parameters)
    {
        if (m_CurrentChosenModeVariable.value != mode)
        {
            return;
        }
        if (parameters == null || parameters.Length <= 0)
            return;
        if (IsInHardCodedMatch())
            return;
        var match = parameters[0] as PvPMatch;
        if (match.arenaSO != m_ArenaSO) return;
        m_TotalNumOfLostMatchesInColumn = match.isVictory ? 0 : m_TotalNumOfLostMatchesInColumn + 1;
        m_TotalNumOfWonMatchesInColumn = !match.isVictory ? 0 : m_TotalNumOfWonMatchesInColumn + 1;

        var thresholdNumOfLostMatchesInColumn = mode == Mode.Normal ? m_ArenaSO.ThresholdNumOfLostMatchesInColumn_Normal : m_ArenaSO.ThresholdNumOfLostMatchesInColumn_Battle;
        var thresholdNumOfWonMatchesInColumn = mode == Mode.Normal ? m_ArenaSO.ThresholdNumOfWonMatchesInColumn_Normal : m_ArenaSO.ThresholdNumOfWonMatchesInColumn_Battle;
        if (m_TotalNumOfLostMatchesInColumn >= thresholdNumOfLostMatchesInColumn)
        {
            value = 10;
        }
        else if (m_TotalNumOfWonMatchesInColumn >= thresholdNumOfWonMatchesInColumn)
        {
            value = 30;
        }
    }
#if UNITY_EDITOR
    [OnInspectorGUI, PropertyOrder(100)]
    protected override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var centerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.LabelField("=============== PlayerPref Variables ===============", centerStyle);
        EditorGUILayout.LabelField($"CurrentArenaIndex_{mode}: {m_ArenaSO.index}");
        m_TotalNumOfLostMatchesInColumn = EditorGUILayout.IntField(nameof(m_TotalNumOfLostMatchesInColumn), m_TotalNumOfLostMatchesInColumn);
        m_TotalNumOfWonMatchesInColumn = EditorGUILayout.IntField(nameof(m_TotalNumOfWonMatchesInColumn), m_TotalNumOfWonMatchesInColumn);
        EditorGUILayout.LabelField("===================================================", centerStyle);
    }
#endif
}