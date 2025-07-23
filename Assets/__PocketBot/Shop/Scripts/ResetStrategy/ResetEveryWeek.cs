using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ResetEveryWeek : MonoBehaviour, IResetStrategy
{
    public event Action onReset;

    [SerializeField]
    private TextMeshProUGUIPair m_TimeLeftBeforeResetText;
    [SerializeField]
    private PPrefDatetimeVariable m_NextMondayTimeVar;

    private Coroutine m_UpdateDataCoroutine;

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
        m_TimeLeftBeforeResetText.text.SetText($"{I2LHelper.TranslateTerm(I2LTerm.Text_LinkReward_ResetIn)} <color=#FFFFFF>{m_NextMondayTimeVar.ToRemainingTime()}</color>");
        m_TimeLeftBeforeResetText.shadowText.SetText($"{I2LHelper.TranslateTerm(I2LTerm.Text_LinkReward_ResetIn)} {m_NextMondayTimeVar.ToRemainingTime()}");
    }

    private void UpdateData()
    {
        if (!gameObject.activeInHierarchy)
            return;
        if (DateTime.Now >= m_NextMondayTimeVar.value)
        {
            ResetData();
        }
        UpdateView();
    }
}