using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LatteGames.UnpackAnimation;
using GachaSystem.Core;
using LatteGames;
using LatteGames.Monetization;
using LatteGames.Template;
using HyrphusQ.Events;

public class PBCardController : AbstractCardController
{
    [SerializeField]
    protected ParticleSystem bonusCardShineFX;
    [SerializeField]
    protected ParticleSystem bonusCardFlashFX;
    [SerializeField]
    protected EZAnim<Vector3>[] scaleUpGearCardPhaseAnims;
    [SerializeField]
    protected List<GachaCardUI> m_GachaCardUIs;
    [SerializeField]
    protected List<GameObject> m_TitleGachaCard;

    DuplicateGachaCardsGroup cardInfo;
    bool isInSummary;

    public override void Setup(DuplicateGachaCardsGroup cardInfo, bool isInSummary = false)
    {
        this.cardInfo = cardInfo;
        this.isInSummary = isInSummary;
        IsAnimationEnded = false;

        if (cardInfo != null && cardInfo.isBonusCard)
        {
            if (isInSummary)
            {
                gameObject.SetActive(false);
                bonusCardShineFX.gameObject.SetActive(false);
            }
            else
            {
                scaleUpGearCardPhaseAnims[0].Play();
                bonusCardShineFX.gameObject.SetActive(true);
            }
        }
        else
        {
            bonusCardShineFX.gameObject.SetActive(false);
        }

        foreach (var gachaCardUI in m_GachaCardUIs)
        {
            gachaCardUI.TryInitialize(cardInfo, OnAnimationCompleted, (cardInfo != null && cardInfo.isBonusCard) || isInSummary);
        }

        void OnAnimationCompleted()
        {
            IsAnimationEnded = true;
        }
    }

    public void GrantBonusCardReward(RVButtonBehavior.RewardGrantedEventData data)
    {
        SoundManager.Instance.PlayLoopSFX(GeneralSFX.UICardFlip, 0.5f, false, false, gameObject);
        IsAnimationEnded = false;
        for (var i = 0; i < cardInfo.cardsAmount; i++)
        {
            cardInfo.representativeCard.GrantReward();
        }
        scaleUpGearCardPhaseAnims[1].Play(OnScaleUpGearCompleted);
        void OnScaleUpGearCompleted()
        {
            bonusCardFlashFX.Play();
            foreach (var gachaCardUI in m_GachaCardUIs)
            {
                gachaCardUI.TryInitialize(cardInfo, OnAnimationCompleted, false);
            }
        }
        void OnAnimationCompleted()
        {
            IsAnimationEnded = true;
        }

        // TODO: Require GD provide information about the event
        #region MonetizationEventCode
        string adsLocation = isInSummary ? "YouGotUI" : "OpenBoxUI";
        MonetizationEventCode monetizationEventCode = isInSummary ? MonetizationEventCode.BonusCard_YouGotUI : MonetizationEventCode.BonusCard_OpenBoxUI;
        GameEventHandler.Invoke(monetizationEventCode, adsLocation);
        #endregion
    }

    public override void ToggleTitleVisibility(bool isVisible)
    {
        if (m_TitleGachaCard == null) return;

        for (int i = 0; i < m_TitleGachaCard.Count; i++)
        {
            if (m_TitleGachaCard[i] != null)
                m_TitleGachaCard[i].SetActive(isVisible);
        }
    }
}