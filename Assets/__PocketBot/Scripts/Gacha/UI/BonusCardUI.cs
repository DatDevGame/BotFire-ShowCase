using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames;
using LatteGames.Monetization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusCardUI : MonoBehaviour
{
    public event Action onSpendSucceeded = delegate { };

    [SerializeField]
    protected BonusCardDataSO dataSO;
    [SerializeField]
    protected RectTransform bonusCardContainer;
    [SerializeField]
    protected RVButtonBehavior claimRVButton;
    [SerializeField]
    protected Button rerollButton;
    [SerializeField]
    protected Button skipButton;
    [SerializeField]
    protected TextMeshProUGUIPair rerollGemTextPair;
    [SerializeField]
    protected TextMeshProUGUI tapToContinueText;
    [SerializeField]
    protected ResourceLocationProvider resourceLocationProvider;

    public Button RerollButton => rerollButton;
    public Button SkipButton => skipButton;
    public RVButtonBehavior ClaimRVButton => claimRVButton;
    public TextMeshProUGUI TapToContinueText => tapToContinueText;

    public bool IsAffordable()
    {
        return CurrencyManager.Instance.IsAffordable(CurrencyType.Premium, dataSO.config.rerollPrice);
    }

    public void Spend()
    {
        var isSpendSucceeded = CurrencyManager.Instance.Spend(CurrencyType.Premium, dataSO.config.rerollPrice, resourceLocationProvider.GetLocation(), resourceLocationProvider.GetItemId());
        if (isSpendSucceeded)
            onSpendSucceeded.Invoke();
    }

    public void Show(bool isShowReroll, float delayBeforeShowingButton)
    {
        gameObject.SetActive(true);
        rerollGemTextPair.text.SetText(dataSO.config.rerollPrice.ToRoundedText());
        rerollGemTextPair.shadowText.SetText(dataSO.config.rerollPrice.ToRoundedText());
        rerollButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        claimRVButton.gameObject.SetActive(false);
        tapToContinueText.gameObject.SetActive(false);

        // Delay 0.5s before showing button to avoid click accidentally
        StartCoroutine(CommonCoroutine.Delay(delayBeforeShowingButton, false, () =>
        {
            rerollButton.gameObject.SetActive(isShowReroll);
            skipButton.gameObject.SetActive(true);
            claimRVButton.gameObject.SetActive(true);
        }));
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetActiveButtonsGroup(bool rerollButtonActive, bool claimRVButtonActive)
    {
        rerollButton.gameObject.SetActive(rerollButtonActive);
        claimRVButton.gameObject.SetActive(claimRVButtonActive);
    }
}