using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class ActiveSkillPacks : MonoBehaviour
{
    [SerializeField]
    private IntVariable requiredTrophiesToUnlockActiveSkillVar;
    [SerializeField]
    private HighestAchievedPPrefFloatTracker highestAchievedTrophyVar;

    private void Start()
    {
        if (highestAchievedTrophyVar < requiredTrophiesToUnlockActiveSkillVar)
        {
            gameObject.SetActive(false);
            highestAchievedTrophyVar.onValueChanged += OnNumOfTrophiesChanged;
        }
    }

    private void OnDestroy()
    {
        highestAchievedTrophyVar.onValueChanged -= OnNumOfTrophiesChanged;
    }

    private void OnNumOfTrophiesChanged(ValueDataChanged<float> eventData)
    {
        if (eventData.newValue >= requiredTrophiesToUnlockActiveSkillVar && eventData.oldValue < requiredTrophiesToUnlockActiveSkillVar)
        {
            gameObject.SetActive(true);
        }
    }
}