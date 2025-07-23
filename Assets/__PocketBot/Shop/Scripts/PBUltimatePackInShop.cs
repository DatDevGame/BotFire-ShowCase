using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

public class PBUltimatePackInShop : MonoBehaviour
{
    [SerializeField] PromotedViewController promotedViewController;
    [SerializeField, BoxGroup("Ref")] protected LocalizationParamsManager timeLimitedLastChance;
    [SerializeField, BoxGroup("Ref")] protected GameObject timeLimitedBox;
    [SerializeField, BoxGroup("Ref")] LG_IAPButton purchaseBtn;
    [SerializeField, BoxGroup("Ref")] GameObject noAdsIcon;
    [SerializeField, BoxGroup("Ref")] TMP_Text garageNameText;
    [SerializeField, BoxGroup("No Ads")] private PPrefBoolVariable removeAdsPPref;

    PBUltimatePackPopup ultimatePack;

    void Awake()
    {
        StartCoroutine(CommonCoroutine.WaitUntil(() => PBUltimatePackPopup.Instance != null, () =>
        {
            ultimatePack = PBUltimatePackPopup.Instance;
            // if (starterPack.starterPackState == StarterPackState.Lost)
            // {
            //     gameObject.SetActive(false);
            // }
            // else
            // {
            //     starterPack.OnUpdateView += UpdateView;
            //     starterPack.OnUpdateTime += OnUpdateTime;
            //     UpdateView();
            // }
            ultimatePack.OnUpdateView += UpdateView;
            ultimatePack.OnUpdateTime += OnUpdateTime;
            UpdateView();
        }));
        removeAdsPPref.onValueChanged += OnRemoveAdsPPrefChanged;
        EnableRemoveAdsView(!removeAdsPPref.value);
    }

    void OnRemoveAdsPPrefChanged(HyrphusQ.Events.ValueDataChanged<bool> data)
    {
        EnableRemoveAdsView(!removeAdsPPref.value);
    }

    void EnableRemoveAdsView(bool isEnable)
    {
        noAdsIcon.SetActive(isEnable);
    }

    private void OnDestroy()
    {
        if (ultimatePack != null)
        {
            ultimatePack.OnUpdateView -= UpdateView;
            ultimatePack.OnUpdateTime -= OnUpdateTime;
        }
        removeAdsPPref.onValueChanged -= OnRemoveAdsPPrefChanged;
    }

    void UpdateView()
    {
        if (ultimatePack.state == UltimatePackState.None)
        {
            gameObject.SetActive(false);
            return;
        }
        gameObject.SetActive(true);
        purchaseBtn.OverrideSetup(ultimatePack.currentIAPProductSO);
        promotedViewController.EnablePromotedView(ultimatePack.state == UltimatePackState.DiscountActive || ultimatePack.state == UltimatePackState.LastChanceDiscount);
        timeLimitedBox.SetActive(ultimatePack.state != UltimatePackState.PermanentNormalActive);
        ultimatePack.currentIAPProductSO.generalItems?.ForEach((v) =>
        {
            if (v.Key is GarageSO garageSO)
            {
                garageNameText.SetText(garageSO.NameGarage);
            }
        });
    }

    void OnUpdateTime(string time)
    {
        timeLimitedLastChance.SetParameterValue("Time", $"<color=#ffffffff>{time}</color>");
    }
}
