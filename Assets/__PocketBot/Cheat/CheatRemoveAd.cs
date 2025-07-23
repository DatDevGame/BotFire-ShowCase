using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class CheatRemoveAd : MonoBehaviour
{
    [SerializeField]
    private PPrefBoolVariable isRemoveAd;
    [SerializeField]
    private AdsConfigSO adsConfigSO;
    [SerializeField]
    private TextMeshProUGUI removeAdStatusText;

    StringBuilder stringBuilder = new StringBuilder();

    private void Update()
    {
        stringBuilder.Clear();
        stringBuilder.AppendLine($"Is Remove Ad: {isRemoveAd.value}");
        stringBuilder.AppendLine($"Is_Enable: {adsConfigSO.IsEnable}");
        stringBuilder.AppendLine($"IS_Firstshow_Trophy: {adsConfigSO.FirstShowTrophyThreshold}");
        removeAdStatusText.SetText(stringBuilder);
    }
}