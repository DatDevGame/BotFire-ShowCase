using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[WindowMenuItem("General/AB_TestSO", assetFolderPath: "Assets/__PocketBot/ABTestRemoteConfig", mode: WindowMenuItemAttribute.Mode.Multiple, sortByName: true)]
public abstract class FeatureBasedABTestSO : ScriptableObject
{
    [SerializeField]
    private string featureNumberId = "1";
    [SerializeField]
    private string featureParamaterKey = "feature_featureName";

    public virtual string FeatureNumberId => featureNumberId;
    public virtual string FeatureParameterKey => featureParamaterKey;

    public abstract void InjectData(bool isEnable);
}