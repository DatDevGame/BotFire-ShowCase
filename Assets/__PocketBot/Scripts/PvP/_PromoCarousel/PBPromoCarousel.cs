using System;
using System.Collections;
using System.Collections.Generic;
using DanielLochner.Assets.SimpleScrollSnap;
using HyrphusQ.Events;
using UnityEngine;
using UnityEngine.UI;

public class PBPromoCarousel : MonoBehaviour
{
    #region Fields
    [SerializeField] private CarouselPanel panelPrefab_NoAds;
    [SerializeField] private CarouselPanel panelPrefab_Starter;
    [SerializeField] private CarouselPanel panelPrefab_Arena;
    [SerializeField] private CarouselPanel panelPrefab_FullSkin;
    [SerializeField] private CarouselPanel panelPrefab_ProSkillSet;
    [SerializeField] private Toggle togglePrefab;
    [SerializeField] private ToggleGroup toggleGroup;
    [SerializeField] private SimpleScrollSnap scrollSnap;
    [SerializeField] private float timeToNextPanel = 3;

    private float toggleWidth;
    #endregion

    #region Methods
    private void Awake()
    {
        toggleWidth = (togglePrefab.transform as RectTransform).sizeDelta.x * (Screen.width / 2048f);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnShowGameOverUI, OnShowGameOverUI);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnShowGameOverUI, OnShowGameOverUI);
    }

    private void OnShowGameOverUI(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        if (!matchOfPlayer.isAbleToComplete)
            return;
        var isVictory = matchOfPlayer.isVictory;
        if (isVictory)
        {
            AddToBack(panelPrefab_NoAds);
            AddToBack(panelPrefab_FullSkin);
            AddToBack(panelPrefab_ProSkillSet);
        }
        else
        {
            AddToBack(panelPrefab_Starter);
            AddToBack(panelPrefab_Arena);
            AddToBack(panelPrefab_ProSkillSet);
        }
        if (scrollSnap.NumberOfPanels > 1)
        {
            StartAutoScroll();
        }
    }

    void StartAutoScroll()
    {
        if (autoScrollCoroutine != null)
        {
            StopCoroutine(autoScrollCoroutine);
        }
        autoScrollCoroutine = StartCoroutine(CR_AutoScroll());
    }

    Coroutine autoScrollCoroutine;
    IEnumerator CR_AutoScroll()
    {
        while (true)
        {
            var t = 0f;

            while (t < timeToNextPanel && !scrollSnap.IsDragging)
            {
                t += Time.deltaTime;
                yield return null;
            }

            if (t >= timeToNextPanel && !scrollSnap.IsDragging)
            {
                if (scrollSnap.GetNearestPanel() == scrollSnap.NumberOfPanels - 1)
                {
                    scrollSnap.GoToPanel(0);
                }
                else
                {
                    scrollSnap.GoToNextPanel();
                }
            }
            yield return null;
        }
    }

    public void Add(int index, CarouselPanel panelPrefab)
    {
        if (panelPrefab.isAvailable)
        {
            // Pagination
            Toggle toggle = Instantiate(togglePrefab, scrollSnap.Pagination.transform.position + new Vector3(toggleWidth * (scrollSnap.NumberOfPanels + 1), 0, 0), Quaternion.identity, scrollSnap.Pagination.transform);
            toggle.group = toggleGroup;
            scrollSnap.Pagination.transform.position -= new Vector3(toggleWidth / 2f, 0, 0);

            // Panel
            scrollSnap.Add(panelPrefab.gameObject, index, out GameObject panelInstance);
            CarouselPanel carouselPanel = panelInstance.GetComponent<CarouselPanel>();
            carouselPanel.OnPurchased += OnPurchased;
            carouselPanel.index = index;
        }
    }

    void OnPurchased(CarouselPanel carouselPanel, int index)
    {
        carouselPanel.OnPurchased -= OnPurchased;
        Remove(index);
    }

    public void AddToFront(CarouselPanel panelPrefab)
    {
        Add(0, panelPrefab);
    }
    public void AddToBack(CarouselPanel panelPrefab)
    {
        Add(scrollSnap.NumberOfPanels, panelPrefab);
    }

    public void Remove(int index)
    {
        if (scrollSnap.NumberOfPanels > 0)
        {
            // Pagination
            DestroyImmediate(scrollSnap.Pagination.transform.GetChild(scrollSnap.NumberOfPanels - 1).gameObject);
            scrollSnap.Pagination.transform.position += new Vector3(toggleWidth / 2f, 0, 0);

            // Panel
            scrollSnap.Remove(index);
        }
    }
    public void RemoveFromFront()
    {
        Remove(0);
    }
    public void RemoveFromBack()
    {
        if (scrollSnap.NumberOfPanels > 0)
        {
            Remove(scrollSnap.NumberOfPanels - 1);
        }
        else
        {
            Remove(0);
        }
    }
    #endregion
}
