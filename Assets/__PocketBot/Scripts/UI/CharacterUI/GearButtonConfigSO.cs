using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GearButtonConfigSO", menuName = "PocketBots/GearButtonConfigSO")]
public class GearButtonConfigSO : ScriptableObject
{
    [BoxGroup("Tweaks")] public float amimationDuration;
    [BoxGroup("Tweaks")] public float maxUpgradeCardTextSize = 24, notMaxUpgradeCardTextSize = 32;
    [BoxGroup("Tweaks")] public Vector2 cardSelectedSizeDelta_1Slot;
    [BoxGroup("Tweaks")] public Vector2 cardSelectedSizeDelta_2Slot;
    [BoxGroup("Tweaks")] public Vector2 cardDeSelectedSizeDelta_Default;
    [BoxGroup("Tweaks")] public Vector2 buttonsSelectedPosition_1Slot;
    [BoxGroup("Tweaks")] public Vector2 buttonsSelectedPosition_2Slot;
    [BoxGroup("Tweaks")] public Vector2 buttonsDeSelectedPosition_Default;
    [BoxGroup("Tweaks")] public Vector2 sufficientCardQuantityToUpgradeTextPosition = Vector2.zero;
    [BoxGroup("Tweaks")] public Vector2 insufficientCardQuantityToUpgradeTextPosition = new Vector2(0f, -6f);

    [BoxGroup("Ref")] public Sprite commonSprite, epicSprite, legendarySprite;
    [BoxGroup("Ref")] public Sprite commonWithoutCurvedSprite, epicWithoutCurvedSprite, legendaryWithoutCurvedSprite;
    [BoxGroup("Ref")] public Sprite sufficientCardQuantityToUpgradeSprite, insufficientCardQuantityToUpgradeSprite;
    [BoxGroup("Ref")] public PBPartManagerSO specialBotManagerSO;
    [BoxGroup("Ref")] public ItemSOVariable currentChassisSelected;
    [BoxGroup("Ref")] public PPrefChassisSOVariable currentChassisInUse;
    [BoxGroup("Ref")] public Material grayScaleMat;

    [BoxGroup("FTUE")] public PPrefBoolVariable buildTabFTUE;
    [BoxGroup("FTUE")] public PPrefBoolVariable equipUpperFTUE;
    [BoxGroup("FTUE")] public PPrefBoolVariable upgradeFTUE;
    [BoxGroup("FTUE")] public PPrefBoolVariable equipFTUE;
}