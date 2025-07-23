using System;
using System.Collections;
using I2.Loc;
using UnityEngine;

public class PBEventTicketCurrencyUI : CurrencyUI
{
    [SerializeField] private string _timeFormatHours;
    [SerializeField] private string _timeFormatLessThan1Hour;
    [SerializeField] private string _timeFormatLessThan1Min;
    [SerializeField] private GameObject _timerTextGO;
    [SerializeField] private LocalizationParamsManager _timeParamManager;
    [SerializeField] private string _timeParamName;
    [SerializeField] private TimeBasedRewardSO _timeBasedRewardSO;
    [SerializeField] private float _updateTimeStep = 0.25f;
    [SerializeField] private RangePPrefFloatProgressSO _eventTicketCountProgressVariable;

    private Coroutine _updateCoroutine;

    public override void UpdateText(float amount)
    {
        currencyText.text = amount.RoundToInt().ToRoundedText() + "/" + _eventTicketCountProgressVariable.rangeProgress.maxValue;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (_updateCoroutine == null)
            _updateCoroutine = StartCoroutine(CR_Update());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_updateCoroutine != null)
        {
            StopCoroutine(_updateCoroutine);
            _updateCoroutine = null;
        }
    }

    IEnumerator CR_Update()
    {
        while (true)
        {
            if (_timeBasedRewardSO.canGetReward || _eventTicketCountProgressVariable.rangeProgress.value >= _eventTicketCountProgressVariable.rangeProgress.maxValue)
            {
                if (_timerTextGO.activeSelf)
                    _timerTextGO.SetActive(false);
            }
            else
            {
                if (!_timerTextGO.activeSelf)
                    _timerTextGO.SetActive(true);
                string remainTimeStr = GetRemainTime();
                _timeParamManager.SetParameterValue(_timeParamName, remainTimeStr);
            }
            yield return Yielders.Get(_updateTimeStep);
        }
    }

    private string GetRemainTime()
    {
        if (_timeBasedRewardSO.canGetReward)
            return string.Empty;
        TimeSpan interval = DateTime.Now - _timeBasedRewardSO.LastRewardTime;
        var remainingSeconds = Math.Max(0, _timeBasedRewardSO.CoolDownInterval - interval.TotalSeconds);
        interval = TimeSpan.FromSeconds(remainingSeconds);
        if (interval.TotalMinutes < 1)
            return string.Format(_timeFormatLessThan1Min, interval.Seconds);
        else if (interval.TotalHours < 1)
            return string.Format(_timeFormatLessThan1Hour, interval.Minutes, interval.Seconds);
        else
            return string.Format(_timeFormatHours, interval.Hours + (interval.Days * 24f), interval.Minutes);
    }
}
