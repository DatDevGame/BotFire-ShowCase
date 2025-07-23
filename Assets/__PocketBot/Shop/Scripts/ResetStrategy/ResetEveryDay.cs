using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetEveryDay : MonoBehaviour, IResetStrategy
{
    public event Action onReset;

    [SerializeField]
    private DayBasedRewardSO m_NextDayRewardSO;
    [SerializeField]
    private TextMeshProUGUIPair m_TimeLeftBeforeResetText;

    private Coroutine m_UpdateDataCoroutine;

    private void Update()
    {
        if(m_NextDayRewardSO.LastRewardTime > DateTime.Now && m_NextDayRewardSO.LastRewardTime.DayOfYear != DateTime.Now.DayOfYear)
            ResetData();
    }

    private void OnEnable()
    {
        m_UpdateDataCoroutine = StartCoroutine(UpdateData_CR());
    }

    private void OnDisable()
    {
        if (m_UpdateDataCoroutine != null)
        {
            StopCoroutine(m_UpdateDataCoroutine);
            m_UpdateDataCoroutine = null;
        }
    }

    private void ResetData()
    {
        onReset?.Invoke();
    }

    private IEnumerator UpdateData_CR()
    {
        var timeToDelay = new WaitForSecondsRealtime(1f);
        while (true)
        {
            UpdateData();
            yield return timeToDelay;
        }
    }

    private void UpdateView()
    {
        var remainingSeconds = m_NextDayRewardSO.CoolDownInterval - (DateTime.Now - m_NextDayRewardSO.LastRewardTime).TotalSeconds;
        var timeLeftBeforeReset = TimeSpan.FromSeconds(remainingSeconds);
        var totalHours = timeLeftBeforeReset.TotalHours;
        var hours = Math.Floor(totalHours);
        var minutes = Math.Ceiling((totalHours - Math.Floor(totalHours)) * 60);
        m_TimeLeftBeforeResetText.text.SetText($"{I2LHelper.TranslateTerm(I2LTerm.Text_LinkReward_ResetIn)} <color=#FFFFFF>{hours}H {minutes}M</color>");
        m_TimeLeftBeforeResetText.shadowText.SetText($"{I2LHelper.TranslateTerm(I2LTerm.Text_LinkReward_ResetIn)} {hours}H {minutes}M");
    }

    private void UpdateData()
    {
        if (!gameObject.activeInHierarchy)
            return;
        if (m_NextDayRewardSO.canGetReward)
        {
            ResetData();
        }
        UpdateView();
    }
}