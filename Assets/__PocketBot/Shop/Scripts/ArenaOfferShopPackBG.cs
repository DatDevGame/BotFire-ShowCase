using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.SerializedDataStructure;
using I2.Loc;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArenaOfferShopPackBG : MonoBehaviour
{
    [SerializeField] PromotedViewController promotedViewController;
    [SerializeField, BoxGroup("Ref")] protected LocalizationParamsManager timeLimitedTxt;
    [SerializeField, BoxGroup("Ref")] protected GameObject timeLimitedBox;
    [SerializeField, BoxGroup("Ref")] LocalizationParamsManager discountTxt;
    [SerializeField, BoxGroup("Ref")] LocalizationParamsManager titleTxt;
    [SerializeField, BoxGroup("Ref")] LG_IAPButton PurchaseBtn;
    [SerializeField, BoxGroup("Ref")] List<PartCell> partCells;
    [SerializeField, BoxGroup("Ref")] GameObject container;
    [SerializeField, BoxGroup("Ref")] SerializedDictionary<RarityType, Sprite> raritySprites;
    [SerializeField, BoxGroup("Config")] bool isInShopUI = true;

    ArenaOffersPopup arenaOffersPopup;
    Coroutine SetupIAPButtonCoroutine;

    private void Awake()
    {
        StartCoroutine(CommonCoroutine.WaitUntil(() => ArenaOffersPopup.Instance != null, () =>
        {
            arenaOffersPopup = ArenaOffersPopup.Instance;
            arenaOffersPopup.OnUpdateView += UpdateView;
            arenaOffersPopup.OnUpdateTime += OnUpdateTime;
            UpdateView();
        }
    ));
    }

    private void OnDestroy()
    {
        if (arenaOffersPopup != null)
        {
            arenaOffersPopup.OnUpdateView -= UpdateView;
            arenaOffersPopup.OnUpdateTime -= OnUpdateTime;
        }
    }

    void UpdateView()
    {
        if (arenaOffersPopup.state == ArenaOfferState.Hide ||
            (arenaOffersPopup.state == ArenaOfferState.ShowLastChance && isInShopUI) ||
            (arenaOffersPopup.state == ArenaOfferState.Lost && !isInShopUI) ||
            arenaOffersPopup.state == ArenaOfferState.HasPurchased)
        {
            container.SetActive(false);
            return;
        }
        else
        {
            container.SetActive(true);
        }
        if (SetupIAPButtonCoroutine != null)
        {
            StopCoroutine(SetupIAPButtonCoroutine);
        }
        SetupIAPButtonCoroutine = StartCoroutine(CommonCoroutine.WaitUntil(() => arenaOffersPopup.currentProductSO != null, () =>
        {
            discountTxt.SetParameterValue("Multiplier", (arenaOffersPopup.state == ArenaOfferState.ShowNormal || arenaOffersPopup.state == ArenaOfferState.Lost) ? "20" : "40");
            PurchaseBtn.OverrideSetup(arenaOffersPopup.currentProductSO);
            int i = 0;
            foreach (var item in arenaOffersPopup.currentProductSO.generalItems)
            {
                var partCell = partCells[i];
                partCell.thumbnail.sprite = item.Key.GetThumbnailImage();
                partCell.rarityOutline.sprite = raritySprites[item.Key.GetRarityType()];
                partCell.amountTxt.text = item.Value.value.ToString();
                i++;
            }
            titleTxt.SetParameterValue("Index", (arenaOffersPopup.currentArenaSO.index + 1).ToString());
            promotedViewController.EnablePromotedView(arenaOffersPopup.state == ArenaOfferState.ShowDiscount || arenaOffersPopup.state == ArenaOfferState.ShowLastChance);
            timeLimitedBox.SetActive(arenaOffersPopup.state != ArenaOfferState.Lost);
        }));
    }

    void OnUpdateTime(string time)
    {
        timeLimitedTxt.SetParameterValue("Time", $"<color=#ffffffff>{time}</color>");
    }

    [Serializable]
    public class PartCell
    {
        public Image thumbnail;
        public Image rarityOutline;
        public TMP_Text amountTxt;
    }
}

