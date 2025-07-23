using UnityEngine;
using LatteGames.PvP.TrophyRoad;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;
using HyrphusQ.Events;
using System.Linq;
using LatteGames.Template;
using static UnityEngine.Rendering.CoreUtils;
using System;
using System.Data;
using LatteGames;
using Sirenix.OdinInspector;

public class PBTrophyRoadUI : TrophyRoadUI
{
    public Action OnCompleteCurrentFilled = delegate { };

    [SerializeField] protected GameObject blockPanel;
    [SerializeField] protected float bottomPadding;
    [SerializeField] protected TrophyRoadArenaSectionUI prevSectionUI; // determined by current medals not highest medals
    [SerializeField] protected int offsetSection = -180;
    [SerializeField] UIOptimizer uIOptimizer;
    private bool isNotReOpen = false;
    public bool IsVisible => isVisible;

    protected override IEnumerator Start()
    {
        yield return null;
        Setup();
        //Assuming all the initalizing is completed, so i dont miss in caching any costly component
        uIOptimizer.Init();
        if (!isVisible)
            uIOptimizer.DisableUnnecessary();
    }
    protected override void Awake()
    {
        base.Awake();
        GameEventHandler.AddActionEvent(PBTrophyRoadEventCode.NewArenaUnlock, NewArenaUnlock);
        GameEventHandler.AddActionEvent(PBTrophyRoadEventCode.DisableBlockTrophyRoad, DisableBlockTrophyRoad);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameEventHandler.RemoveActionEvent(PBTrophyRoadEventCode.NewArenaUnlock, NewArenaUnlock);
        GameEventHandler.RemoveActionEvent(PBTrophyRoadEventCode.DisableBlockTrophyRoad, DisableBlockTrophyRoad);
    }
    protected override void Setup()
    {
        if (firstTimeSetup)
        {
            UpdateLastOpenMedals();
        }
        firstTimeSetup = false;

        // Generate sectionUIs
        PBTrophyRoadArenaSectionUI prevSectionUI = null;
        foreach (var section in trophyRoadSO.ArenaSections)
        {
            var newSectionUI = Instantiate(arenaSectionUIPrefab, scrollViewContent);
            newSectionUI.PosY = prevSectionUI != null ? (prevSectionUI.PosY + prevSectionUI.Height) : bottomPadding;
            newSectionUI.Setup(trophyRoadSO, section);
            prevSectionUI = newSectionUI as PBTrophyRoadArenaSectionUI;
            sectionUIs.Add(newSectionUI);

            newSectionUI.UpdateFillsImmediately(lastOpenHighestAchievedMedals, lastOpenCurrentMedals);
            if (currentSectionUI == null && !newSectionUI.IsCurrentFillFull)
            {
                currentSectionUI = newSectionUI;
            }
            newSectionUI.OnExpand += HandleSectionExpand;
            newSectionUI.OnShrink += HandleSectionShrink;
        }
        if (currentSectionUI == null)
            currentSectionUI = sectionUIs[^1];

        for (int i = 0; i < sectionUIs.Count; i++)
        {
            if (sectionUIs[i].IsCurrentFillFull)
            {
                this.prevSectionUI = sectionUIs[i <= 0 ? 0 : i - 1];
            }
        }

        // Setup scollView
        var newSize = scrollViewContent.sizeDelta;
        newSize.y = prevSectionUI.PosY + prevSectionUI.Height;
        scrollViewContent.sizeDelta = newSize;
        pointerUI.transform.SetParent(scrollViewContent);
    }
    protected override void HandleOpened()
    {
        uIOptimizer.EnableAll();
        SetVisible(true);
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, HandleUnpackStart); // This occurs when opening rewards

        StartCoroutine(CRPlayUpdatingUIAnimation());
        UpdateLastOpenMedals();
        if (!isBlockSnap)
            SnapScrollViewAt(currentSectionUI.PosY + currentSectionUI.GetFillHeightFromMedals(trophyRoadSO.CurrentMedals) + ContentParent.rect.height * 0.5f);
        scrollRect.inertia = true;
    }

    protected override void HandleClosed()
    {
        if (isVisible)
            uIOptimizer.DisableUnnecessary();
        base.HandleClosed();
    }

    protected override IEnumerator CRPlayUpdatingUIAnimation()
    {
        canvasGroup.interactable = false;
        // List out sections that need updating
        List<PBTrophyRoadArenaSectionUI> sectionUIsNeedUpdate = new();
        var newHighestAchievedMedals = trophyRoadSO.HighestAchievedMedals;
        var newCurrentMedals = trophyRoadSO.CurrentMedals;
        foreach (var sectionUI in sectionUIs)
        {
            if (sectionUI.NeedUpdate(newHighestAchievedMedals, newCurrentMedals))
            {
                sectionUIsNeedUpdate.Add(sectionUI as PBTrophyRoadArenaSectionUI);
            }
        }
        if (sectionUIsNeedUpdate.Count > 0)
        {
            // Reverse the updating list if player has been demoted to lower arena
            if (newCurrentMedals < lastOpenCurrentMedals)
            {
                sectionUIsNeedUpdate.Reverse();
            }
            currentSectionUI = sectionUIsNeedUpdate[^1];
            // Update highest fills
            foreach (var sectionUI in sectionUIsNeedUpdate)
            {
                yield return StartCoroutine(sectionUI.CRPlayUpdatingHighestAchievedFillAnimation(newHighestAchievedMedals));
            }
            // Update current fills
            foreach (var sectionUI in sectionUIsNeedUpdate)
            {
                yield return StartCoroutine(sectionUI.CRPlayUpdatingCurrentFillAnimation(newCurrentMedals));
            }
        }

        if (currentSectionUI != null)
        {
            pointerUI.Anchor = currentSectionUI.PointerAnchor;
            pointerUI.Show();
        }

        canvasGroup.interactable = true;
    }

    private void HandleUnpackStart()
    {
        if (!isNotReOpen)
            isNotReOpen = isVisible;

        HandleClosed(); // Don't invoke the OnTrophyRoadClose as it will reopen the main game canvas
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, HandleUnpackDone);
    }

    private void HandleUnpackDone()
    {
        if (isNotReOpen)
            GameEventHandler.Invoke(TrophyRoadEventCode.OnTrophyRoadOpened); // Invoke the OnTrophyRoadOpened to close the main game canvas
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, HandleUnpackDone);

        isNotReOpen = false;
    }

    private void SnapScrollViewAt(float yPos)
    {
        var newPos = scrollViewContent.anchoredPosition;
        newPos.y = ContentParent.rect.height - yPos;
        scrollViewContent.anchoredPosition = newPos;

        if (yPos == 0)
            scrollViewContent.anchoredPosition = Vector2.zero;
    }

    private void SnapScrollViewAtDG(float yPos, float timeDuration, TweenCallback tweenCallback)
    {
        var newPos = scrollViewContent.anchoredPosition;
        newPos.y = ContentParent.rect.height - yPos;
        scrollViewContent.DOAnchorPos(newPos, timeDuration).SetEase(Ease.OutQuart)
            .OnComplete(tweenCallback);
    }

    private void NewArenaUnlock()
    {
        blockPanel.SetActive(true);
        PBTrophyRoadArenaSectionUI currentArenaSectionUI = currentSectionUI as PBTrophyRoadArenaSectionUI;
        PBTrophyRoadArenaSectionUI previousArenaSectionUI = null;
        PBTrophyRoadArenaSectionUI nextSectionCurrent = null;

        if (currentArenaSectionUI != null)
        {
            previousArenaSectionUI = sectionUIs
                .TakeWhile(v => v != currentArenaSectionUI)
                .LastOrDefault() as PBTrophyRoadArenaSectionUI;

            nextSectionCurrent = sectionUIs.SkipWhile(v => v != currentArenaSectionUI)
                .Skip(1)
                .FirstOrDefault() as PBTrophyRoadArenaSectionUI;

            currentArenaSectionUI.BannerUI.SetLocked(false);
            currentArenaSectionUI.CardsTabUI.SetLocked(false);
        }

        StartCoroutine(DelaySnapScrollView(currentArenaSectionUI, previousArenaSectionUI, nextSectionCurrent));
    }

    private bool isBlockSnap = false;
    private IEnumerator DelaySnapScrollView(PBTrophyRoadArenaSectionUI currentArenaSectionUI, PBTrophyRoadArenaSectionUI previousArenaSectionUI, PBTrophyRoadArenaSectionUI nextSectionCurrent = null)
    {
        if (currentArenaSectionUI == null) yield break;
        isBlockSnap = true;

        float previousArenaAnchorPosY = 0;
        if (previousArenaSectionUI != null && previousArenaSectionUI.LastSection != null)
            previousArenaAnchorPosY = previousArenaSectionUI.PosY + previousArenaSectionUI.GetFillHeightFromMedals(previousArenaSectionUI.CurrentSection.GetRequiredMedals()) + ContentParent.rect.height * 0.5f - offsetSection;

        if (previousArenaAnchorPosY != 0)
            SnapScrollViewAt(previousArenaAnchorPosY);
        else
            scrollViewContent.anchoredPosition = Vector2.zero;

        currentArenaSectionUI.Darklayer.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        SoundManager.Instance.PlaySFX(PBSFX.UITrophyRoadScrolling);
        float currentArenaAnchorPosY = (currentArenaSectionUI.PosY + currentArenaSectionUI.GetFillHeightFromMedals(currentArenaSectionUI.CurrentSection.GetRequiredMedals()) + ContentParent.rect.height * 0.5f) - offsetSection;
        SnapScrollViewAtDG(currentArenaAnchorPosY, 1.5f,
            () =>
            {
                isBlockSnap = false;
                sectionUIs.ForEach(v => (v as PBTrophyRoadArenaSectionUI).IsCompleteSnapNewArena = true);
                SoundManager.Instance.PlaySFX(PBSFX.UIMilestoneGain);
            });
    }

    private void DisableBlockTrophyRoad()
    {
        blockPanel.SetActive(false);
    }
}
