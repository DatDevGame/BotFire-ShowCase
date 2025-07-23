using System.Collections;
using System.Collections.Generic;
using LatteGames;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenStarterPackBtn : MonoBehaviour
{
    [SerializeField] protected Button btn;
    [SerializeField] protected TMP_Text timeLimitedTxt;
    [SerializeField] protected GameObject lastChanceTxt, discountedTag, timeBox;

    StarterPack starterPack;
    public Button button => btn;
    CanvasGroupVisibility canvasGroupVisibility;
    LayoutElement layoutElement;

    private void Start()
    {
        canvasGroupVisibility = GetComponent<CanvasGroupVisibility>();
        layoutElement = gameObject.AddComponent<LayoutElement>();
        starterPack = StarterPack.Instance;
        starterPack.ConnectButton(this);
        starterPack.OnUpdateView += UpdateView;
        starterPack.OnUpdateTimeForButton += OnUpdateTime;
        UpdateView();
        //     StartCoroutine(CommonCoroutine.WaitUntil(() => StarterPack.Instance != null, () =>
        //     {
        //         starterPack = StarterPack.Instance;
        //         starterPack.ConnectButton(this);
        //         starterPack.OnUpdateView += UpdateView;
        //         starterPack.OnUpdateTimeForButton += OnUpdateTime;
        //         UpdateView();
        //     }
        // ));
    }

    private void OnDestroy()
    {
        if (starterPack != null)
        {
            starterPack.OnUpdateView -= UpdateView;
            starterPack.OnUpdateTimeForButton -= OnUpdateTime;
        }
    }

    void UpdateView()
    {
        if (starterPack.starterPackState == StarterPackState.Hide || starterPack.starterPackState == StarterPackState.Lost || !starterPack.isEnoughTrophy)
        {
            canvasGroupVisibility.HideImmediately();
            layoutElement.ignoreLayout = true;
        }
        else
        {
            //TODO: Hide IAP & Popup
            //canvasGroupVisibility.ShowImmediately();
            canvasGroupVisibility.HideImmediately();
            layoutElement.ignoreLayout = false;
        }
        lastChanceTxt.SetActive(starterPack.starterPackState == StarterPackState.ShowLastChance);
        discountedTag.SetActive(starterPack.starterPackState == StarterPackState.ShowLastChance || starterPack.starterPackState == StarterPackState.ShowDiscount);
        timeBox.SetActive(starterPack.starterPackState == StarterPackState.ShowNormal || starterPack.starterPackState == StarterPackState.ShowLastChance || starterPack.starterPackState == StarterPackState.ShowDiscount);
    }

    void OnUpdateTime(string time)
    {
        timeLimitedTxt.SetText(time);
    }
}
