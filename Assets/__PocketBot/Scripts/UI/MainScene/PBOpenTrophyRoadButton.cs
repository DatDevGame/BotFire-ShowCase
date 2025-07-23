using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using HyrphusQ.GUI;
using I2.Loc;
using LatteGames;
using LatteGames.PvP.TrophyRoad;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static LatteGames.PvP.TrophyRoad.TrophyRoadSO;
using HyrphusQ.SerializedDataStructure;
using GachaSystem.Core;
using System;

public class PBOpenTrophyRoadButton : OpenTrophyRoadButton
{
    public static Action OnSetupCompleted;

    [SerializeField, BoxGroup("Ref")] protected PBTrophyRoadButtonNextRewardGroup trophyRoadButtonNextRewardGroup;
    [SerializeField, BoxGroup("Ref")] protected TMP_Text m_NextRewardValueText;
    [SerializeField] protected Button openButton;
    [SerializeField] protected RoundedProgressBar roundedProgressBar;
    [SerializeField] protected CurrentHighestArenaVariable currentHighestArenaVariable;
    [SerializeField] protected LocalizationParamsManager arenaTxtParamsManager;
    [SerializeField, BoxGroup("Object")] protected CanvasGroupVisibility canvasGroupVisibility;

    [SerializeField, BoxGroup("Resource")] public CurrencyDictionarySO m_CurrencyDictionarySO;

    [SerializeField, BoxGroup("Data")] protected PBTrophyRoadSO m_PBTrophyRoadSO;

    protected override void Awake()
    {
        base.Awake();
        openButton.onClick.AddListener(OnOpenTrophyRoad);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        openButton.onClick.RemoveListener(OnOpenTrophyRoad);
    }

    protected override IEnumerator Start()
    {
        UpdateView();
        yield return base.Start();
        OnSetupCompleted?.Invoke();
    }

    protected virtual void OnOpenTrophyRoad()
    {
        GameEventHandler.Invoke(TrophyRoadEventCode.OnTrophyRoadOpened);
    }

    protected virtual void UpdateNextRewardDisplay()
    {
        if (m_PBTrophyRoadSO == null) return;

        Milestone nextMilestone = m_PBTrophyRoadSO.GetNextMilestoneCurrent();

        if (nextMilestone == null || nextMilestone.reward == null)
        {
            trophyRoadButtonNextRewardGroup.SetReward();
            return;
        }
        ShopProductSO nextReward = nextMilestone.reward;
        if (nextReward == null) return;

        if (nextReward.generalItems.Count > 0)
        {
            if (nextReward.generalItems.Any(x => x.Key is PBPartSO))
            {
                var partSO = nextReward.generalItems.Keys.ToList().Find(x => x is PBPartSO) as PBPartSO;
                trophyRoadButtonNextRewardGroup.SetReward(partSO);
            }
            else if (nextReward.generalItems.Any(x => x.Key is GachaPack))
            {
                var gachaPack = nextReward.generalItems.Keys.ToList().Find(x => x is GachaPack) as GachaPack;
                trophyRoadButtonNextRewardGroup.SetReward(gachaPack);
            }
        }
        else if (nextReward.currencyItems.Count > 0)
        {
            if (nextReward.currencyItems.Any(x => x.Key == CurrencyType.Premium))
            {
                var currencyType = nextReward.currencyItems.Keys.ToList().Find(x => x == CurrencyType.Premium);
                trophyRoadButtonNextRewardGroup.SetReward(currencyType);
            }
            else if (nextReward.currencyItems.Any(x => x.Key == CurrencyType.Standard))
            {
                var currencyType = nextReward.currencyItems.Keys.ToList().Find(x => x == CurrencyType.Standard);
                trophyRoadButtonNextRewardGroup.SetReward(currencyType);
            }
        }
    }

    protected override void UpdateView()
    {
        UpdateNextRewardDisplay();
        if (trophyRoadSO.TryGetHighestArenaProgressValues(out var currentValue, out _))
        {
            roundedProgressBar.fillAmount = currentValue;
        }
        else
        {
            roundedProgressBar.fillAmount = 0f;
        }
        currentMedalsText.SetText(currentMedalsText.blueprintText.Replace(Const.StringValue.PlaceholderValue, trophyRoadSO.CurrentMedals.ToRoundedText()));
        arenaTxtParamsManager.SetParameterValue("Index", (currentHighestArenaVariable.value.index + 1).ToString());

        var claimableCount = trophyRoadSO.GetClaimableCount();
        if (claimableCount > 0)
        {
            claimableCountText.text = claimableCount.ToString();
        }
        else
        {
            claimableCountText.transform.parent.gameObject.SetActive(false);
        }

        int indexArena = currentHighestArenaVariable.value.index;
        string key = $"AutoOpenTrophyRoad-{currentHighestArenaVariable.name.GetHashCode()}";
        if (indexArena > 0 && PlayerPrefs.GetInt(key) != indexArena)
        {
            openButton.onClick?.Invoke();
            PlayerPrefs.SetInt(key, indexArena);
        }
    }
}