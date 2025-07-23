using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using I2.Loc;
using Sirenix.OdinInspector;
using UnityEngine;

public class SkinLocker : MonoBehaviour
{
    public static int UNLOCK_TROPHY = 120;

    [SerializeField] GameObject lockLayer;
    [SerializeField] string trophyValueContain;
    [SerializeField] LocalizationParamsManager unlockTxt;
    [SerializeField] HighestAchievedPPrefFloatTracker highestAchievedPPrefFloatTracker;

    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable skinUI_FTUE;

    public bool isMetUnlockCondition => highestAchievedPPrefFloatTracker.value >= UNLOCK_TROPHY;

    protected void Awake()
    {
        if (!isMetUnlockCondition)
        {
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Medal).onValueChanged += OnMedalChange;
            unlockTxt.SetParameterValue("Value", trophyValueContain.Replace("{Value}", UNLOCK_TROPHY.ToString()));
        }
        else
        {
            lockLayer.SetActive(false);
        }

        if (!skinUI_FTUE)
        {
            GameEventHandler.AddActionEvent(FTUEEventCode.OnClickMoreButton, OnClickMoreButton);
        }

    }

    private void OnClickMoreButton()
    {
        if (!skinUI_FTUE && isMetUnlockCondition)
        {
            GameEventHandler.Invoke(FTUEEventCode.OnSkinUI_FTUE, gameObject);
            GameEventHandler.RemoveActionEvent(FTUEEventCode.OnClickMoreButton, OnClickMoreButton);
            skinUI_FTUE.value = true;
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null && CurrencyManager.Instance.GetCurrencySO(CurrencyType.Medal) != null)
        {
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Medal).onValueChanged -= OnMedalChange;
        }
    }

    void OnMedalChange(HyrphusQ.Events.ValueDataChanged<float> valueData)
    {
        if (isMetUnlockCondition)
        {
            lockLayer.SetActive(false);
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Medal).onValueChanged -= OnMedalChange;
        }
    }
}
