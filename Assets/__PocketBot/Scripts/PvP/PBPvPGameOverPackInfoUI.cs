using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using LatteGames;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PBPackDockSlotInfoUI;

public class PBPvPGameOverPackInfoUI : MonoBehaviour
{
    [SerializeField] protected CanvasGroupVisibility canvasGroupVisibility;
    [SerializeField] protected Image thumbnailImg;
    [SerializeField] protected TMP_Text nameTxt;
    [SerializeField] protected Button closeBtn;
    [SerializeField] protected Button openByGemBtn, openByAdsBtn, openNowBtn;
    [SerializeField] protected TMP_Text coinRangeAmountTxt, gemRangeAmountTxt, cardRangeAmountTxt, gemAmountOfOpenNowBtnTxt, originalGemAmountOfOpenNowBtnTxt;
    [SerializeField] protected SerializedDictionary<RarityType, RarityGuaranteedCardInfo> rarityGuaranteedCardInfoDictionary;
    [SerializeField] protected GameObject guaranteedGroup;
    [SerializeField] protected VerticalGroupResizer verticalGroupResizer;
    [SerializeField] protected EZAnimBase showPanelEZAnim;

    protected Button originalOpenByGemBtn, originalOpenByAdsBtn, originalOpenByIAPBtn;
    GachaPack gachaPack;

    public bool IsPopup { get; private set; }

    private void Awake()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        closeBtn.onClick.AddListener(Hide);
        openByGemBtn.onClick.AddListener(OnOpenByGemBtnClicked);
        openByAdsBtn.onClick.AddListener(OnOpenByAdsBtnClicked);
        openNowBtn.onClick.AddListener(OnOpenByIAPBtnClicked);
        OpenNowOfferManager.Instance.OnLimitedStateChanged += OnOpenNowOfferStateChanged;
        OpenNowOfferManager.Instance.OnPermanentStateChanged += OnOpenNowOfferStateChanged;
    }

    private void OnDestroy()
    {
        closeBtn.onClick.RemoveListener(Hide);
        openByGemBtn.onClick.RemoveListener(OnOpenByGemBtnClicked);
        openByAdsBtn.onClick.RemoveListener(OnOpenByAdsBtnClicked);
        openNowBtn.onClick.RemoveListener(OnOpenByIAPBtnClicked);
        if (OpenNowOfferManager.Instance != null)
        {
            OpenNowOfferManager.Instance.OnLimitedStateChanged -= OnOpenNowOfferStateChanged;
            OpenNowOfferManager.Instance.OnPermanentStateChanged -= OnOpenNowOfferStateChanged;
        }
    }

    void OnOpenNowOfferStateChanged()
    {
        var isApplyingOpenNow = OpenNowOfferManager.Instance.IsApplyingOpenNow;
        openByGemBtn.gameObject.SetActive(originalOpenByGemBtn.gameObject.activeInHierarchy && !isApplyingOpenNow);
        openByAdsBtn.gameObject.SetActive(originalOpenByAdsBtn.gameObject.activeInHierarchy && !isApplyingOpenNow);
        closeBtn.gameObject.SetActive(!isApplyingOpenNow);
        openNowBtn.gameObject.SetActive(isApplyingOpenNow);
    }

    protected virtual void OnOpenByGemBtnClicked()
    {
        originalOpenByGemBtn.onClick.Invoke();
    }

    protected virtual void OnOpenByAdsBtnClicked()
    {
        originalOpenByAdsBtn.onClick.Invoke();
    }

    protected virtual void OnOpenByIAPBtnClicked()
    {
        #region Firebase Event
        if (gachaPack != null)
        {
            string openType = "free";
            GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, gachaPack, openType);
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

        originalOpenByIAPBtn.onClick.Invoke();
    }

    public virtual void Hide()
    {
        IsPopup = false;
        canvasGroupVisibility.Hide();
        showPanelEZAnim.InversePlay();
    }

    public void Show(GachaPack gachaPack, Button originalOpenByGemBtn, Button originalOpenByAdsBtn, Button originalOpenByIAPBtn)
    {
        var isApplyingOpenNow = OpenNowOfferManager.Instance.IsApplyingOpenNow;
        if (isApplyingOpenNow)
        {
            originalOpenByIAPBtn.onClick.Invoke();
            return;
        }

        IsPopup = true;
        this.gachaPack = gachaPack;
        this.originalOpenByGemBtn = originalOpenByGemBtn;
        this.originalOpenByAdsBtn = originalOpenByAdsBtn;
        this.originalOpenByIAPBtn = originalOpenByIAPBtn;

        openByGemBtn.gameObject.SetActive(originalOpenByGemBtn.gameObject.activeInHierarchy && !isApplyingOpenNow);
        openByAdsBtn.gameObject.SetActive(originalOpenByAdsBtn.gameObject.activeInHierarchy && !isApplyingOpenNow);
        closeBtn.gameObject.SetActive(!isApplyingOpenNow);
        openNowBtn.gameObject.SetActive(isApplyingOpenNow);

        InitView();
        canvasGroupVisibility.Show();
        showPanelEZAnim.SetToStart();
        showPanelEZAnim.Play();
    }
    protected virtual void InitView()
    {
        if (gachaPack != null)
        {
            thumbnailImg.sprite = gachaPack.GetOriginalPackThumbnail();
            nameTxt.text = gachaPack.GetOriginalPackName();
        }

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

        gemAmountOfOpenNowBtnTxt.text = originalGemAmountOfOpenNowBtnTxt.text;
        coinRangeAmountTxt.text = $"{gachaPack.GetOriginalPackMoneyAmountRange().x.RoundToInt().ToRoundedText()} - {gachaPack.GetOriginalPackMoneyAmountRange().y.RoundToInt().ToRoundedText()}";
        gemRangeAmountTxt.text = $"{gachaPack.GetOriginalPackGemAmountRange().x.RoundToInt().ToRoundedText()} - {gachaPack.GetOriginalPackGemAmountRange().y.RoundToInt().ToRoundedText()}";

        guaranteedGroup.SetActive(isShowGuaranteedGroup);
        verticalGroupResizer.UpdateSize();
    }
}
