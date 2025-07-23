using System.Collections;
using System.Collections.Generic;
using I2.Loc;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using UnityEngine;

public class StarterPackInShop : MonoBehaviour
{
    [SerializeField] PromotedViewController promotedViewController;
    [SerializeField, BoxGroup("Ref")] protected LocalizationParamsManager timeLimitedLastChance;
    [SerializeField, BoxGroup("Ref")] GameObject timeLimitedBox;
    [SerializeField, BoxGroup("Ref")] LocalizationParamsManager discountTxt;
    [SerializeField, BoxGroup("Ref")] LG_IAPButton purchaseBtn;

    StarterPack starterPack;

    void Awake()
    {
        StartCoroutine(CommonCoroutine.WaitUntil(() => StarterPack.Instance != null, () =>
        {
            starterPack = StarterPack.Instance;
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
            starterPack.OnUpdateView += UpdateView;
            starterPack.OnUpdateTime += OnUpdateTime;
            UpdateView();
        }));
    }

    private void OnDestroy()
    {
        if (starterPack != null)
        {
            starterPack.OnUpdateView -= UpdateView;
            starterPack.OnUpdateTime -= OnUpdateTime;
        }
    }

    void UpdateView()
    {
        if (starterPack.starterPackState == StarterPackState.Hide)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
        discountTxt.SetParameterValue("Multiplier", (starterPack.starterPackState == StarterPackState.ShowNormal || starterPack.starterPackState == StarterPackState.Lost) ? "20" : "40");
        purchaseBtn.OverrideSetup(starterPack.currentProductSO);
        promotedViewController.EnablePromotedView(starterPack.starterPackState == StarterPackState.ShowDiscount || starterPack.starterPackState == StarterPackState.ShowLastChance);
        timeLimitedBox.SetActive(starterPack.starterPackState != StarterPackState.Lost);
    }

    void OnUpdateTime(string time)
    {
        timeLimitedLastChance.SetParameterValue("Time", $"<color=#ffffffff>{time}</color>");
    }
}
