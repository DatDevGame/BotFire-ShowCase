using System;
using System.Collections;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;

public class ProSkillSetPopup : ComposeCanvasElementVisibilityController
{
    [SerializeField]
    private int trophyIntervalForPeriodicShow = 150;
    [SerializeField]
    private ProSkillSet proSkillSet;
    [SerializeField]
    private PPrefIntVariable firstShowTrophyCountVar;
    [SerializeField]
    private PPrefBoolVariable hasCompletedActiveSkillTutorialVar;

    private void Awake()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        if (proSkillSet.CurrentStateVar.value == 3 || proSkillSet.ProSkillSetIAPProduct.IsPurchased())
        {
            gameObject.SetActive(false);
        }
        else
        {
            proSkillSet.OnItemPurchased += OnItemPurchased;
            proSkillSet.CurrentStateVar.onValueChanged += OnStateChanged;
            StartCoroutine(CheckToShowPopup_CR());
        }
    }

    private void OnDestroy()
    {
        proSkillSet.OnItemPurchased -= OnItemPurchased;
        proSkillSet.CurrentStateVar.onValueChanged -= OnStateChanged;
    }

    private void OnStateChanged(ValueDataChanged<int> eventData)
    {
        if (eventData.newValue == 3)
        {
            OnItemPurchased();
        }
    }

    private void OnItemPurchased()
    {
        StopAllCoroutines();
        HideImmediately();
        gameObject.SetActive(false);
    }

    private IEnumerator CheckToShowPopup_CR()
    {
        WaitUntil waitUntilActiveSkillUnlocked = new WaitUntil(() => proSkillSet.IsUnlocked());
        WaitUntil waitUntilOnMainScreen = new WaitUntil(() => LeagueDataSO.IsOnMainScreen());
        WaitUntil waitUntilActiveSkillTutorialCompleted = new WaitUntil(() => hasCompletedActiveSkillTutorialVar.value);
        while (true)
        {
            yield return waitUntilActiveSkillUnlocked;
            yield return waitUntilActiveSkillTutorialCompleted;
            yield return waitUntilOnMainScreen;
            if (!firstShowTrophyCountVar.hasKey) // First show
            {
                firstShowTrophyCountVar.value = Mathf.RoundToInt(proSkillSet.HighestAchievedTrophyVar.value);
                Show();
                LGDebug.Log($"First show ProSkillSet popup - {Time.time}", "ProSkillSet");
            }
            else if (proSkillSet.HighestAchievedTrophyVar.value - firstShowTrophyCountVar.value >= trophyIntervalForPeriodicShow) // Periodic show every 150 trophies
            {
                firstShowTrophyCountVar.value = Mathf.RoundToInt(proSkillSet.HighestAchievedTrophyVar.value);
                Show();
                LGDebug.Log($"Periodic show ProSkillSet popup - {Time.time}", "ProSkillSet");
            }
        }
    }
}