using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GearCardProgressBar : MonoBehaviour
{
    [SerializeField]
    private GearButton gearButton;

    public void UpdateView(int currentNumOfCards, int requiredNumOfCards)
    {
        gearButton.UpdateLevelUpgradeData(currentNumOfCards, requiredNumOfCards);
    }

    public void UpdateViewWithAnimation(GachaItemSO gachaItemSO, int quantityOfCard, Action animationCompleted, float duration = AnimationDuration.TINY, float delay = AnimationDuration.SSHORT)
    {
        Requirement_GachaCard gachaCardRequirement = gachaItemSO.GetCurrentUpgradeRequirements().Find(req => req is Requirement_GachaCard) as Requirement_GachaCard;
        int currentNumOfCards = gachaItemSO.GetNumOfCards() - quantityOfCard;
        int requiredNumOfCards = gachaCardRequirement?.requiredNumOfCards ?? 1;
        gearButton.PartSO = gachaItemSO.Cast<PBPartSO>();
        gearButton.UpdateLevelUpgradeData(currentNumOfCards, requiredNumOfCards);
        if (!gachaItemSO.IsMaxUpgradeLevel())
        {
            var destinationNumOfCards = gachaItemSO.GetNumOfCards();
            DOTween
                .To(GetMethod, SetMethod, destinationNumOfCards, duration)
                .SetDelay(delay)
                .OnComplete(OnAnimationCompleted);

            int GetMethod()
            {
                return currentNumOfCards;
            }
            void SetMethod(int value)
            {
                currentNumOfCards = value;
                UpdateView(currentNumOfCards, requiredNumOfCards);
            }
        }
        else
        {
            OnAnimationCompleted();
        }

        void OnAnimationCompleted()
        {
            animationCompleted?.Invoke();
        }
    }
}