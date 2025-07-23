using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "TestMatchMakingSO", menuName = "PocketBots/Test/TestMatchMakingSO")]
public class TestMatchMakingSO : ScriptableObject
{
    [SerializeField]
    bool isTest;
    [SerializeField]
    public bool isFakeStats;
    [SerializeField, FoldoutGroup("Player References")]
    public PPrefChassisSOVariable playerChassisSO;
    [SerializeField, FoldoutGroup("Player References")]
    public PPrefItemSOVariable playerFrontSO;
    [SerializeField, FoldoutGroup("Player References")]
    public PPrefItemSOVariable playerUpperSO_1;
    [SerializeField, FoldoutGroup("Player References")]
    public PPrefItemSOVariable playerUpperSO_2;
    [SerializeField, FoldoutGroup("Player References")]
    public PPrefItemSOVariable playerWheelSO_1;
    [SerializeField, FoldoutGroup("Player References")]
    public PPrefItemSOVariable playerWheelSO_2;
    [SerializeField, OnValueChanged("OnChangePlayerScriptBot")]
    public ScriptedBot playerScriptedBot;
    void OnChangePlayerScriptBot()
    {
        playerChassisSO.value = playerScriptedBot.chassisSO;
        playerFrontSO.value = playerScriptedBot.front;
        playerUpperSO_1.value = playerScriptedBot.upper_1;
        playerUpperSO_2.value = playerScriptedBot.upper_2;
        playerWheelSO_1.value = playerScriptedBot.wheel_1;
        playerWheelSO_2.value = playerScriptedBot.wheel_2;
    }
    [SerializeField, FoldoutGroup("Enemy")]
    public ScriptedBot enemyScriptedBot;
    [SerializeField, FoldoutGroup("Enemy")]
    public PB_AIProfile _AIProfile;
    [SerializeField, FoldoutGroup("Enemy")]
    public float scoreMultiplier;

    public bool IsTest => GameDataSO.Instance.isDevMode && isTest;

    public float GetPlayerScore()
    {
        var playerChassis = (PBChassisSO)playerChassisSO.value;
        var parts = new List<PBPartSO>();
        var wheelSlots = playerChassis.AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Wheels));
        var upperSlots = playerChassis.AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Upper));
        var frontSlots = playerChassis.AllPartSlots.FindAll(slot => slot.PartType.Equals(PBPartType.Front));
        if (wheelSlots.Count >= 1)
        {
            if ((PBPartSO)playerWheelSO_1.value != null)
            {
                parts.Add((PBPartSO)playerWheelSO_1.value);
            }
        }
        if (wheelSlots.Count >= 2)
        {
            if ((PBPartSO)playerWheelSO_2.value != null)
            {
                parts.Add((PBPartSO)playerWheelSO_2.value);
            }
        }
        if (upperSlots.Count >= 1)
        {
            if ((PBPartSO)playerUpperSO_1.value != null)
            {
                parts.Add((PBPartSO)playerUpperSO_1.value);
            }
        }
        if (upperSlots.Count >= 2)
        {
            if ((PBPartSO)playerUpperSO_2.value != null)
            {
                parts.Add((PBPartSO)playerUpperSO_2.value);
            }
        }
        if (frontSlots.Count >= 1)
        {
            if ((PBPartSO)playerFrontSO.value != null)
            {
                parts.Add((PBPartSO)playerFrontSO.value);
            }
        }
        return RobotStatsCalculator.CalCombinationStatsScore(false, playerChassis, parts.ToArray());
    }

#if UNITY_EDITOR
    [OnInspectorInit("OnInspectorGUIBegin"), PropertyOrder(100)]
    void OnInspectorGUIBegin()
    {
        playerScriptedBot.chassisSO = (PBChassisSO)playerChassisSO.value;
        playerScriptedBot.front = (PBPartSO)playerFrontSO.value;
        playerScriptedBot.upper_1 = (PBPartSO)playerUpperSO_1.value;
        playerScriptedBot.upper_2 = (PBPartSO)playerUpperSO_2.value;
        playerScriptedBot.wheel_1 = (PBPartSO)playerWheelSO_1.value;
        playerScriptedBot.wheel_2 = (PBPartSO)playerWheelSO_2.value;
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}