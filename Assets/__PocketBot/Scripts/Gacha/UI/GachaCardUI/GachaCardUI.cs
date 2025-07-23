using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using UnityEngine;

public abstract class GachaCardUI : MonoBehaviour
{
    public abstract bool TryInitialize(DuplicateGachaCardsGroup cardInfo, Action callback, bool isInSummary = false);
}
public abstract class GachaCardUI<T> : GachaCardUI where T : GachaCard
{
    public override bool TryInitialize(DuplicateGachaCardsGroup cardInfo, Action callback, bool isInSummary = false)
    {
        if (cardInfo.representativeCard is not T genericGachaCard)
        {
            HideCard();
            return false;
        }
        ShowCard();
        UpdateView(cardInfo, genericGachaCard, callback, isInSummary);
        return true;
    }

    public virtual void ShowCard() => gameObject.SetActive(true);

    public virtual void HideCard() => gameObject.SetActive(false);

    public abstract void UpdateView(DuplicateGachaCardsGroup cardInfo, T gachaCard, Action callback, bool isInSummary = false);
}