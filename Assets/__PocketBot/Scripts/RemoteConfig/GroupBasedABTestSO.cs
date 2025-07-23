using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[WindowMenuItem("General/AB_TestSO", assetFolderPath: "Assets/__PocketBot/ABTestRemoteConfig", mode: WindowMenuItemAttribute.Mode.Multiple, sortByName: true)]
public abstract class GroupBasedABTestSO : ScriptableObject
{
    public abstract void InjectData(int groupIndex);
}