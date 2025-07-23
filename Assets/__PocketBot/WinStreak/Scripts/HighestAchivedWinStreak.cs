using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "HighestAchivedWinStreak", menuName = "PocketBots/PackReward/HighestAchivedWinStreak")]
public class HighestAchivedWinStreak : PPrefIntVariable
{
    [SerializeField] private PPrefIntVariable toBeTrackedVariable;

    private void OnEnable()
    {
        TrackHigherAchieved();
        toBeTrackedVariable.onValueChanged += HandleTrackedVariableChanged;
    }

    private void OnDisable()
    {
        toBeTrackedVariable.onValueChanged -= HandleTrackedVariableChanged;
    }

    private void HandleTrackedVariableChanged(ValueDataChanged<int> valueDataChanged)
    {
        TrackHigherAchieved();
    }

    private void TrackHigherAchieved()
    {
        if (value < toBeTrackedVariable.value)
        {
            value = toBeTrackedVariable.value;
        }
    }
}
