using UnityEngine;
using LatteGames.PvP.TrophyRoad;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;
using HyrphusQ.Events;
using Sirenix.OdinInspector;

public class PBTrophyRoadArenaSectionUI : TrophyRoadArenaSectionUI
{
    [SerializeField] protected Image BGImg, patternImg;
    public TrophyRoadSO.ArenaSection LastSection => lastSection;
    public TrophyRoadSO.ArenaSection CurrentSection => section;
    public TrophyRoadSO.ArenaSection NextSection => nextSection;
    public TrophyRoadArenaBannerUI BannerUI => bannerUI;
    public TrophyRoadFoundInCardsTabUI CardsTabUI => cardsTabUI;

    public bool IsCompleteSnapNewArena { get; set; }
    public Image Darklayer => darkLayer;

    public override void Setup(TrophyRoadSO trophyRoadSO, TrophyRoadSO.ArenaSection section)
    {
        base.Setup(trophyRoadSO, section);

        Material bgMat = Instantiate(BGImg.material);
        BGImg.material = bgMat;
        if (section.arenaSO is PBPvPArenaSO)
        {
            var arenaSO = (PBPvPArenaSO)section.arenaSO;
            bgMat.SetColor("_InsideColor", arenaSO.InsideColor);
            bgMat.SetColor("_OutsideColor", arenaSO.OutsideColor);
            patternImg.sprite = arenaSO.PatternSprite;
            patternImg.material = Instantiate(patternImg.material);
            patternImg.material.SetColor("_Color", arenaSO.PatternTintColor);
        }
        else
        {
            var emptyArenaSO = (PBEmptyPvPArenaSO)section.arenaSO;
            bgMat.SetColor("_InsideColor", emptyArenaSO.InsideColor);
            bgMat.SetColor("_OutsideColor", emptyArenaSO.OutsideColor);
            patternImg.sprite = emptyArenaSO.PatternSprite;
        }
    }

    public override IEnumerator CRPlayUpdatingHighestAchievedFillAnimation(float highestAchievedMedals)
    {
        var oldFillHeight = highestAchievedFill.Height;
        var newFillHeight = GetFillHeightFromMedals(highestAchievedMedals);

        bool isActiveWaitingHighestAchievedFill = false;
        if (nextSection != null)
        {
            if (highestAchievedMedals >= nextSection.GetRequiredMedals())
            {
                string keyCallOneTimeUnlockArena = $"ArenaUnlock{transform.GetSiblingIndex()}-Animation";
                if (!PlayerPrefs.HasKey(keyCallOneTimeUnlockArena))
                {
                    IsCompleteSnapNewArena = false;
                    isActiveWaitingHighestAchievedFill = true;
                    PlayerPrefs.SetInt(keyCallOneTimeUnlockArena, 1);
                    GameEventHandler.Invoke(PBTrophyRoadEventCode.NewArenaUnlock);
                }
            }
        }

        bool isBlockUnlockNewArenaUI = false;
        MilestoneUI passedMilestoneUI = null;
        if (newFillHeight > oldFillHeight || newFillHeight == spacing && oldFillHeight == spacing)
        {
            if (darkLayer.enabled && section.IsUnlocked)
            {
                UpdateLockedViewsImmediately();
                darkLayer.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.5f);
                GameEventHandler.Invoke(ArenaUnlockEvent.StartUnlockNewArena, patternImg.sprite, section);
                PBNewArenaUnlockUI.Instance.IsRunningAnimator = true;

                yield return new WaitUntil(() => !PBNewArenaUnlockUI.Instance.IsRunningAnimator);
                isBlockUnlockNewArenaUI = true;
            }
            passedMilestoneUI = milestoneUIs.Find(milestoneUI => milestoneUI.PosY <= newFillHeight && milestoneUI.PosY > oldFillHeight);
        }

        if (isActiveWaitingHighestAchievedFill)
            yield return new WaitUntil(() => IsCompleteSnapNewArena);

        if (passedMilestoneUI != null && passedMilestoneUI.RewardUI != null)
        {
            yield return WaitForFillTween(highestAchievedFill, passedMilestoneUI.PosY);
            yield return passedMilestoneUI.RewardUI.CRUpdateUIAnimation();
        }
        yield return WaitForFillTween(highestAchievedFill, newFillHeight);

        if(isBlockUnlockNewArenaUI)
            GameEventHandler.Invoke(PBTrophyRoadEventCode.DisableBlockTrophyRoad);
    }

    public override bool NeedUpdate(float highestAchievedMedals, float currentMedals)
    {
        return section.IsUnlocked;
    }
}
