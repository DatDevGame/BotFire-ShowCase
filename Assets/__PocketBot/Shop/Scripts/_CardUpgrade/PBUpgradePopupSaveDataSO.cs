using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PBUpgradePopupSaveDataSO : SavedDataSO<UpgradePopupSavedData>
{
    public DayBasedRewardSO dayBasedRewardSO;
}

[Serializable]
public class UpgradePopupSavedData : SavedData
{
    public List<string> gotCardPartGUIDs = new();
}
