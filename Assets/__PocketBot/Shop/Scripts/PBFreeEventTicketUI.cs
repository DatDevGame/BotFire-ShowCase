using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class PBFreeEventTicketUI : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] protected ResourceLocationProvider m_PopupSourceResourceLocationProvider;
    [SerializeField, BoxGroup("Config")] protected ResourceLocationProvider m_ShopSourceResourceLocationProvider;
    [SerializeField, BoxGroup("Config")] protected bool m_IsShop;
    [SerializeField, BoxGroup("Ref")] protected RVButtonBehavior _claimRVBtn;
    [SerializeField, BoxGroup("Ref")] protected List<GameObject> _watchAdGOs;
    [SerializeField, BoxGroup("Ref")] protected List<GameObject> _remainTimeGOs;
    [SerializeField, BoxGroup("Ref")] protected TextMeshProUGUI _remainTimeText;
    [SerializeField, BoxGroup("Ref")] protected TextMeshProUGUI _limitBuyCountText;
    [SerializeField, BoxGroup("Data")] private DayBasedRewardSO _dailyLimitBuyTimeVar;
    [SerializeField, BoxGroup("Data")] private RangePPrefFloatProgressSO _eventTicketDailyLimitBuyProgressVar;

    private Coroutine _updateDataCoroutine;

    private void Awake()
    {
        _claimRVBtn.OnRewardGranted += ClaimRVBtn_OnRewardGranted;
        UpdateClaimable();
    }

    private void OnDestroy()
    {
        _claimRVBtn.OnRewardGranted -= ClaimRVBtn_OnRewardGranted;
    }

    private void OnEnable()
    {
        _updateDataCoroutine = StartCoroutine(UpdateData_CR());
    }

    private void OnDisable()
    {
        if (_updateDataCoroutine != null)
        {
            StopCoroutine(_updateDataCoroutine);
            _updateDataCoroutine = null;
        }
    }

    private IEnumerator UpdateData_CR()
    {
        var timeToDelay = new WaitForSecondsRealtime(1f);
        while (true)
        {
            if (_dailyLimitBuyTimeVar.LastRewardTime > DateTime.Now && _dailyLimitBuyTimeVar.LastRewardTime.DayOfYear != DateTime.Now.DayOfYear)
                ResetDailyMaxBuyByRV();

            if (gameObject.activeInHierarchy)
            {
                if (_dailyLimitBuyTimeVar.canGetReward)
                    ResetDailyMaxBuyByRV();
                else if (_eventTicketDailyLimitBuyProgressVar.rangeProgress.value <= 0)
                    UpdateDailyLimitCD();
                else
                    UpdateDailyLimitCount();
            }
            yield return timeToDelay;
        }
    }

    private void ResetDailyMaxBuyByRV()
    {
        _eventTicketDailyLimitBuyProgressVar.rangeProgress.value = _eventTicketDailyLimitBuyProgressVar.rangeProgress.maxValue;
        _dailyLimitBuyTimeVar.GetReward();
        UpdateClaimable();
    }

    private void UpdateDailyLimitCount()
    {
        _limitBuyCountText.SetText($"{_eventTicketDailyLimitBuyProgressVar.rangeProgress.value}/{_eventTicketDailyLimitBuyProgressVar.rangeProgress.maxValue}");
    }

    private void UpdateDailyLimitCD()
    {
        _remainTimeText.SetText("<size=50><voffset=-15><sprite name=Clock><voffset=-7><size=35> " + GetRemainTime());
    }

    private string GetRemainTime()
    {
        if (_dailyLimitBuyTimeVar.canGetReward)
            return string.Empty;
        TimeSpan interval = DateTime.Now - _dailyLimitBuyTimeVar.LastRewardTime;
        var remainingSeconds = Math.Max(0, _dailyLimitBuyTimeVar.CoolDownInterval - interval.TotalSeconds);
        interval = TimeSpan.FromSeconds(remainingSeconds);
        if (interval.TotalMinutes < 1)
            return string.Format("{0}S", interval.Seconds);
        else if (interval.TotalHours < 1)
            return string.Format("{0}M {1}S", interval.Minutes, interval.Seconds);
        else
            return string.Format("{0}H {1}M", interval.Hours + (interval.Days * 24f), interval.Minutes);
    }

    private void ClaimRVBtn_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData data)
    {
        --_eventTicketDailyLimitBuyProgressVar.rangeProgress.value;
        ResourceLocationProvider resourceProvider = m_IsShop ? m_ShopSourceResourceLocationProvider : m_PopupSourceResourceLocationProvider;
        CurrencyManager.Instance[CurrencyType.EventTicket].Acquire(1, resourceProvider.GetLocation(), resourceProvider.GetItemId());
        CurrencyManager.Instance.PlayAcquireAnimation(CurrencyType.EventTicket , 1, transform.position, null);
        UpdateClaimable();

        #region MonetizationEventCode
        try
        {
            string location = m_IsShop ? "Shop" : "Resource Popup";
            GameEventHandler.Invoke(MonetizationEventCode.FreeEventTicket, location);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    private void UpdateClaimable()
    {
        bool claimable = _eventTicketDailyLimitBuyProgressVar.rangeProgress.value > 0;
        _watchAdGOs.ForEach(v => v.SetActive(claimable));
        _remainTimeGOs.ForEach(v => v.SetActive(!claimable));
        if (claimable)
            UpdateDailyLimitCount();
        else
            UpdateDailyLimitCD();
    }
}
