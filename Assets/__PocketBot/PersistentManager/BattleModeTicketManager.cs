using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

public class BattleModeTicketManager : MonoBehaviour
{
    [SerializeField]
    private RangePPrefFloatProgressSO m_TicketCountProgressVariable;
    [SerializeField]
    private TimeBasedRewardSO m_TimeToRefillTicket;

    private float ticketCount => m_TicketCountProgressVariable.rangeProgress.value;
    private float maxTicketCount => m_TicketCountProgressVariable.rangeProgress.maxValue;
    private RangeProgress<float> ticketProgress => m_TicketCountProgressVariable.rangeProgress;

    private void Start()
    {
        ticketProgress.onValueChanged += OnTicketChanged;
        StartCoroutine(RefillTicket_CR());
    }

    private void OnDestroy()
    {
        ticketProgress.onValueChanged -= OnTicketChanged;
    }

    private void OnTicketChanged(ValueDataChanged<float> eventData)
    {
        if (eventData.oldValue == maxTicketCount && eventData.newValue == maxTicketCount - 1)
        {
            m_TimeToRefillTicket.GetReward();
        }
    }

    private IEnumerator RefillTicket_CR()
    {
        var waitToRefillTicket = new WaitUntil(() => m_TimeToRefillTicket.canGetReward && ticketCount < maxTicketCount);
        while (true)
        {
            yield return waitToRefillTicket;
            var numOfTickets = m_TimeToRefillTicket.GetRewardByAmount();
            //TODO: add resource event
            float aquireValue = 
                Mathf.Clamp(ticketProgress.value + numOfTickets, ticketProgress.minValue, ticketProgress.maxValue) -
                m_TicketCountProgressVariable.rangeProgress.value;
            CurrencyManager.Instance[CurrencyType.EventTicket].Acquire(aquireValue, ResourceLocation.AutoGenerate, $"AutoGenerate");
        }
    }
}