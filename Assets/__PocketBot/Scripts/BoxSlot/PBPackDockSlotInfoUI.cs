using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PBPackDockSlotInfoUI : PackDockSlotInfoUI
{
    [SerializeField] protected TMP_Text coinRangeAmountTxt, gemRangeAmountTxt, cardRangeAmountTxt;
    [SerializeField] protected SerializedDictionary<RarityType, RarityGuaranteedCardInfo> rarityGuaranteedCardInfoDictionary;
    [SerializeField] protected GameObject guaranteedGroup;
    [SerializeField] protected GameObject remainingTimeBox;
    [SerializeField] protected VerticalGroupResizer verticalGroupResizer;
    [SerializeField, BoxGroup("Footer Button Group")] protected Button openNowButton;

    protected bool isSpeedUpPack => gachaPackDockSlot.GachaPack is PBManualGachaPack && ((PBManualGachaPack)gachaPackDockSlot.GachaPack).IsSpeedUpPack;

    protected override void Awake()
    {
        base.Awake();
        packDockOpenByGemButton.OnEnoughGemClicked.AddListener(OnEnoughGemClicked);
        packDockOpenByGemButton.OnNotEnoughGemClicked.AddListener(OnNotEnoughGemClicked);
        openNowButton.onClick.AddListener(OnOpenNowButtonClicked);
        OpenNowOfferManager.Instance.OnLimitedStateChanged += OnOpenNowOfferStateChanged;
        OpenNowOfferManager.Instance.OnPermanentStateChanged += OnOpenNowOfferStateChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        packDockOpenByGemButton.OnEnoughGemClicked.RemoveListener(OnEnoughGemClicked);
        packDockOpenByGemButton.OnNotEnoughGemClicked.RemoveListener(OnNotEnoughGemClicked);
        openNowButton.onClick.RemoveListener(OnOpenNowButtonClicked);
        if (OpenNowOfferManager.Instance != null)
        {
            OpenNowOfferManager.Instance.OnLimitedStateChanged -= OnOpenNowOfferStateChanged;
            OpenNowOfferManager.Instance.OnPermanentStateChanged -= OnOpenNowOfferStateChanged;
        }
    }

    private void OnOpenNowOfferStateChanged()
    {
        if (gachaPackDockSlot == null || gachaPackDockSlot.GachaPack == null)
            return;
        UpdateView();
    }

    private void OnOpenNowButtonClicked()
    {
        #region Firebase Event
        if (gachaPackDockSlot.GachaPack != null)
        {
            string openType = "free";
            GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, gachaPackDockSlot.GachaPack, openType);
        }
        #endregion

        #region Design Event
        string openStatus = "Standard";
        string location = "BoxSlot";
        if (OpenNowOfferManager.Instance.IsApplyingOpenNow)
        {
            location = "OpenNowOffer";
            openStatus = "Standard";
        }
        GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
        #endregion

        PackDockManager.Instance.UnlockNow(gachaPackDockSlot);
    }

    private void OnEnoughGemClicked()
    {
        GameEventHandler.Invoke(GachaPackDockEventCode.OnOpenNowByGem);
    }

    private void OnNotEnoughGemClicked()
    {
        IAPGemPackPopup.Instance?.Show();
    }

    protected override void InitView()
    {
        var gachaPack = gachaPackDockSlot.GachaPack;
        if (gachaPack != null)
        {
            thumbnailImg.sprite = gachaPack.GetOriginalPackThumbnail();
            nameTxt.text = gachaPack.GetOriginalPackName();
        }

        packDockOpenByGemButton.Setup(gachaPackDockSlot);
        packDockOpenByAdsButton.Setup(gachaPackDockSlot);

        cardRangeAmountTxt.text = $"{gachaPack.GetOriginalPackCardCount()}";
        bool isShowGuaranteedGroup = false;
        foreach (var pair in rarityGuaranteedCardInfoDictionary)
        {
            var rarity = pair.Key;
            var guaranteedCardCount = gachaPack.GetOriginalPackGuaranteedCardsCount(rarity);
            if (guaranteedCardCount > 0)
            {
                var text = pair.Value.rarityCardAmountTxt;
                text.text = $"x{guaranteedCardCount}";
                pair.Value.groupGO.SetActive(true);
                isShowGuaranteedGroup = true;
            }
            else
            {
                pair.Value.groupGO.SetActive(false);
            }
        }

        coinRangeAmountTxt.text = $"{gachaPack.GetOriginalPackMoneyAmountRange().x.RoundToInt().ToRoundedText()} - {gachaPack.GetOriginalPackMoneyAmountRange().y.RoundToInt().ToRoundedText()}";
        gemRangeAmountTxt.text = $"{gachaPack.GetOriginalPackGemAmountRange().x.RoundToInt().ToRoundedText()} - {gachaPack.GetOriginalPackGemAmountRange().y.RoundToInt().ToRoundedText()}";

        guaranteedGroup.SetActive(isShowGuaranteedGroup);

        packDockSlotUI.OnUpdateRemainingTime.AddListener(OnUpdateRemainingTime);
        gachaPackDockSlot.OnStateChanged += OnSlotStateChanged;

        OnUpdateRemainingTimeEvent?.Invoke(gachaPackDockSlot);
        OnInit?.Invoke(gachaPackDockSlot);
    }

    protected override void UpdateView()
    {
        remainingTimeBox.SetActive(gachaPackDockSlot.State == GachaPackDockSlotState.Unlocking);
        verticalGroupResizer.UpdateSize();
        if (OpenNowOfferManager.Instance.IsApplyingOpenNow)
        {
            DisableAllFooterButtons();
            openNowButton.gameObject.SetActive(true);
            OnUpdateView?.Invoke(gachaPackDockSlot);
            return;
        }
        base.UpdateView();
        if (isSpeedUpPack)
        {
            packDockOpenNowByAdsButton.gameObject.SetActive(false);
        }

        #region Design Events
        if (gachaPackDockSlot.State == GachaPackDockSlotState.Unlocking ||
            gachaPackDockSlot.State == GachaPackDockSlotState.WaitToUnlock ||
            gachaPackDockSlot.State == GachaPackDockSlotState.WaitForAnotherUnlock)
        {
            string input = $"{gachaPackDockSlot.GachaPack.GetDisplayName()}";
            string typeBox = input.Split(' ')[0];

            string boxType = "";
            string rvName = "";
            string location = "";
            if (gachaPackDockSlot.CanOpenNowByAds && packDockOpenNowByAdsButton.gameObject.activeSelf)
            {
                boxType = typeBox;
                rvName = $"OpenNowBox_{boxType}";
                location = "BoxPopup";
                GameEventHandler.Invoke(DesignEvent.RVShow);
            }
            else if (!gachaPackDockSlot.CanOpenNowByAds && packDockOpenByAdsButton.gameObject.activeSelf)
            {
                boxType = typeBox;
                rvName = $"SpeedupBox_{boxType}";
                location = "BoxPopup";
                GameEventHandler.Invoke(DesignEvent.RVShow);
            }
        }
        #endregion

    }

    protected override void OnRewardGranted(RVButtonBehavior.RewardGrantedEventData data)
    {
        #region MonetizationEventCode
        string adsLocation = "BoxPopup";
        string input = $"{gachaPackDockSlot.GachaPack.GetDisplayName()}";
        string typeBox = input.Split(' ')[0];
        GameEventHandler.Invoke(MonetizationEventCode.OpenNowBox_BoxPopup, adsLocation, typeBox);
        #endregion

        #region Firebase Event
        if (gachaPackDockSlot.GachaPack != null)
        {
            string openType = "RV";
            GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, gachaPackDockSlot.GachaPack, openType);
        }
        #endregion

        base.OnRewardGranted(data);
        GameEventHandler.Invoke(GachaPackDockEventCode.OnOpenNowByRV);
    }

    protected override void UnlockingUpdateView()
    {
        if (OpenNowOfferManager.Instance.IsApplyingOpenNow)
        {
            DisableAllFooterButtons();
            openNowButton.gameObject.SetActive(true);
        }
        else
        {
            if (!isSpeedUpPack)
            {
                packDockOpenNowByAdsButton.gameObject.SetActive(gachaPackDockSlot.CanOpenNowByAds);
            }
            packDockOpenByAdsButton.gameObject.SetActive(!gachaPackDockSlot.CanOpenNowByAds);
            packDockOpenByGemButton.gameObject.SetActive(true);
            openNowButton.gameObject.SetActive(false);
        }
    }

    protected override void DisableAllFooterButtons()
    {
        openNowButton.gameObject.SetActive(false);
        base.DisableAllFooterButtons();
    }

    [Serializable]
    public struct RarityGuaranteedCardInfo
    {
        public TMP_Text rarityCardAmountTxt;
        public GameObject groupGO;
    }
}
