using System.Collections;
using System.Collections.Generic;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArenaOffersMainUIButton : MonoBehaviour
{
    [SerializeField] protected Button btn;
    [SerializeField] protected TMP_Text timeLimitedTxt;
    [SerializeField] protected GameObject lastChanceTxt, discountedTag, timeBox;

    ArenaOffersPopup arenaOffersPopup;
    public Button button => btn;
    CanvasGroupVisibility canvasGroupVisibility;
    LayoutElement layoutElement;

    private void Start()
    {
        canvasGroupVisibility = GetComponent<CanvasGroupVisibility>();
        layoutElement = gameObject.AddComponent<LayoutElement>();
        arenaOffersPopup = ArenaOffersPopup.Instance;
        arenaOffersPopup.ConnectButton(this);
        arenaOffersPopup.OnUpdateView += UpdateView;
        arenaOffersPopup.OnUpdateTimeForButton += OnUpdateTime;
        UpdateView();
        // StartCoroutine(CommonCoroutine.WaitUntil(() => ArenaOffersPopup.Instance != null, () =>
        // {
        //     arenaOffersPopup = ArenaOffersPopup.Instance;
        //     arenaOffersPopup.ConnectButton(this);
        //     arenaOffersPopup.OnUpdateView += UpdateView;
        //     arenaOffersPopup.OnUpdateTimeForButton += OnUpdateTime;
        //     UpdateView();
        // }));
    }

    private void OnDestroy()
    {
        if (arenaOffersPopup != null)
        {
            arenaOffersPopup.OnUpdateView -= UpdateView;
            arenaOffersPopup.OnUpdateTimeForButton -= OnUpdateTime;
        }
    }

    void UpdateView()
    {
        if (arenaOffersPopup.state == ArenaOfferState.Hide || arenaOffersPopup.state == ArenaOfferState.Lost || arenaOffersPopup.state == ArenaOfferState.HasPurchased)
        {
            canvasGroupVisibility.HideImmediately();
            layoutElement.ignoreLayout = true;
        }
        else
        {
            //TODO: Hide IAP & Popup
            canvasGroupVisibility.HideImmediately();
            //canvasGroupVisibility.ShowImmediately();
            layoutElement.ignoreLayout = false;
        }
        lastChanceTxt.SetActive(arenaOffersPopup.state == ArenaOfferState.ShowLastChance);
        discountedTag.SetActive(arenaOffersPopup.state == ArenaOfferState.ShowLastChance || arenaOffersPopup.state == ArenaOfferState.ShowDiscount);
        timeBox.SetActive(arenaOffersPopup.state == ArenaOfferState.ShowNormal || arenaOffersPopup.state == ArenaOfferState.ShowLastChance || arenaOffersPopup.state == ArenaOfferState.ShowDiscount);
    }

    void OnUpdateTime(string time)
    {
        timeLimitedTxt.SetText(time);
    }
}
