using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public enum PermanentOpenNowOfferState
{
    None = 0,
    Discount = 1,
    Original = 2,
    Purchased = 3
}
public class OpenNowOfferManager : Singleton<OpenNowOfferManager>
{
    public Action<string> OnPermanentSaleTimerUpdated;
    public Action OnPermanentStateChanged;
    public Action<string> OnLimitedSaleTimerUpdated;
    public Action OnLimitedStateChanged;
    public Action OnMeetShowingCondition;

    [SerializeField, BoxGroup("General")] PBPvPArenaSO arenaSO_1;
    [SerializeField, BoxGroup("General")] CurrentHighestArenaVariable currentHighestArenaVariable;
    [SerializeField, BoxGroup("Limited")] TimeBasedRewardSO applyOpenNowTimer;
    [SerializeField, BoxGroup("Limited")] PPrefBoolVariable isApplyingLimitedOpenNow;
    [SerializeField, BoxGroup("Permanent")] TimeBasedRewardSO saleTimeBasedRewardSO;
    [SerializeField, BoxGroup("Permanent")] PPrefIntVariable permanentState;
    [SerializeField, BoxGroup("Permanent")] DiscountableIAPProduct permanentOpenNowOfferProduct;

    public bool IsApplyingLimitedOpenNow => isApplyingLimitedOpenNow;
    public bool IsApplyingPermanentOpenNow => permanentOpenNowOfferProduct.IsPurchased();
    public PermanentOpenNowOfferState permanentOpenNowOfferState => (PermanentOpenNowOfferState)permanentState.value;
    public bool IsMeetShowingCondition => isMeetShowingCondition;
    public bool IsApplyingOpenNow => isMeetShowingCondition && (IsApplyingLimitedOpenNow || IsApplyingPermanentOpenNow);

    bool isMeetShowingCondition;
    Coroutine TrackingLimitedOpenNowCoroutine;
    Coroutine DiscountCoroutine;

    private void Awake()
    {
        PBPackDockManager.OnBoxOpened += CheckShowingCondition;
        arenaSO_1.ScriptedGachaBoxes.ScriptedGachaPacksIndex.onValueChanged += OnScriptedBoxesIndexChanged;
    }

    void OnScriptedBoxesIndexChanged(HyrphusQ.Events.ValueDataChanged<int> data)
    {
        CheckShowingCondition();
    }

    void CheckShowingCondition()
    {
        if (!isMeetShowingCondition && (!arenaSO_1.ScriptedGachaBoxes.isRemainPack || !arenaSO_1.ScriptedGachaBoxes.currentManualGachaPack.IsSpeedUpPack || currentHighestArenaVariable.value.index > arenaSO_1.index))
        {
            foreach (var slot in PBPackDockManager.Instance.gachaPackDockData.gachaPackDockSlots)
            {
                if (slot.GachaPack is PBManualGachaPack manualGachaPack && manualGachaPack.IsSpeedUpPack)
                {
                    isMeetShowingCondition = false;
                    return;
                }
            }
            isMeetShowingCondition = true;

            if (IsApplyingPermanentOpenNow)
            {
                ((PBPackDockManager)PBPackDockManager.Instance).SetUnlockedToAllUnlockingSlot();
                if (DiscountCoroutine != null)
                {
                    StopCoroutine(DiscountCoroutine);
                };
                if (TrackingLimitedOpenNowCoroutine != null)
                {
                    StopCoroutine(TrackingLimitedOpenNowCoroutine);
                };
                permanentState.value = (int)PermanentOpenNowOfferState.Purchased;
                PBPackDockManager.OnBoxOpened -= CheckShowingCondition;
                arenaSO_1.ScriptedGachaBoxes.ScriptedGachaPacksIndex.onValueChanged -= OnScriptedBoxesIndexChanged;
            }

            if (permanentState.value == (int)PermanentOpenNowOfferState.None)
            {
                permanentState.value = (int)PermanentOpenNowOfferState.Discount;
                saleTimeBasedRewardSO.GetReward();
                OnPermanentStateChanged?.Invoke();
            }

            if (permanentState.value == (int)PermanentOpenNowOfferState.Discount)
            {
                if (DiscountCoroutine != null)
                {
                    StopCoroutine(DiscountCoroutine);
                };
                DiscountCoroutine = StartCoroutine(CR_Discount());
            }

            OnMeetShowingCondition?.Invoke();
        }
    }

    private void Start()
    {
        if (isApplyingLimitedOpenNow)
        {
            if (TrackingLimitedOpenNowCoroutine != null)
            {
                StopCoroutine(TrackingLimitedOpenNowCoroutine);
            };
            TrackingLimitedOpenNowCoroutine = StartCoroutine(CR_TrackingLimitedOpenNowTimer());
        }
        CheckShowingCondition();
    }

    IEnumerator CR_TrackingLimitedOpenNowTimer()
    {
        while (!applyOpenNowTimer.canGetReward)
        {
            yield return null;
            OnLimitedSaleTimerUpdated?.Invoke(applyOpenNowTimer.GetRemainingTimeInShort());
        }
        isApplyingLimitedOpenNow.value = false;
        OnLimitedStateChanged?.Invoke();
    }

    IEnumerator CR_Discount()
    {
        while (!saleTimeBasedRewardSO.canGetReward)
        {
            yield return null;
            OnPermanentSaleTimerUpdated?.Invoke(saleTimeBasedRewardSO.GetRemainingTime());
        }
        permanentState.value = (int)PermanentOpenNowOfferState.Original;
        OnPermanentStateChanged?.Invoke();
    }

    public void PurchaseLimitedOpenNow()
    {
        ((PBPackDockManager)PBPackDockManager.Instance).SetUnlockedToAllUnlockingSlot();
        applyOpenNowTimer.GetReward();
        if (TrackingLimitedOpenNowCoroutine != null)
        {
            StopCoroutine(TrackingLimitedOpenNowCoroutine);
        };
        TrackingLimitedOpenNowCoroutine = StartCoroutine(CR_TrackingLimitedOpenNowTimer());
        isApplyingLimitedOpenNow.value = true;
        OnLimitedStateChanged?.Invoke();
    }

    public void PurchasePermanentOpenNow()
    {
        ((PBPackDockManager)PBPackDockManager.Instance).SetUnlockedToAllUnlockingSlot();
        if (DiscountCoroutine != null)
        {
            StopCoroutine(DiscountCoroutine);
        };
        if (TrackingLimitedOpenNowCoroutine != null)
        {
            StopCoroutine(TrackingLimitedOpenNowCoroutine);
        };
        permanentState.value = (int)PermanentOpenNowOfferState.Purchased;
        OnPermanentStateChanged?.Invoke();
    }
}
