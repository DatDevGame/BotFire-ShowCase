using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class CheatResetTimeStarterPack : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField TMP_InputField;
    [SerializeField]
    private PPrefBoolVariable m_PPrefStarterPackBuy;
    [SerializeField]
    private PPrefIntVariable m_PPrefWin;
    [SerializeField]
    private PPrefIntVariable m_PPrefMatch;
    [SerializeField]
    private TimeBasedRewardSO m_timeBasedRewardSO;
    [SerializeField]
    private TimeBasedRewardSO m_timeAprear;

    private void Awake()
    {
        TMP_InputField.onValueChanged.AddListener(second =>
        {
            m_PPrefWin.value = 10;
            m_PPrefMatch.value = 15;
            m_timeBasedRewardSO.CoolDownInterval = int.Parse(second);
            m_timeAprear.CoolDownInterval = int.Parse(second);
            m_PPrefStarterPackBuy.value = false;
            m_timeBasedRewardSO.GetReward();
        });
    }
}
