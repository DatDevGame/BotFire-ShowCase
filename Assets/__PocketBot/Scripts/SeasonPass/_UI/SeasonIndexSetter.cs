using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using TMPro;
using UnityEngine;

public class SeasonIndexSetter : MonoBehaviour
{

    // private void Awake()
    // {
    //     GameEventHandler.AddActionEvent(SeasonPassEventCode.OnSetNewSeasonFirstDay, UpdateIndexes);
    //     UpdateIndexes();
    // }

    // private void OnDestroy()
    // {
    //     GameEventHandler.RemoveActionEvent(SeasonPassEventCode.OnSetNewSeasonFirstDay, UpdateIndexes);
    // }

    void UpdateIndexes()
    {
        var seasonIndexes = GetComponentsInChildren<SeasonIndex>(true);
        var seasonIndexString = SeasonPassManager.Instance.seasonPassSO.GetSeasonIndex().ToString();
        foreach (var seasonIndex in seasonIndexes)
        {
            if (seasonIndex.TryGetComponent(out TMP_Text text))
            {
                text.text = seasonIndexString;
            }
        }
    }
}
