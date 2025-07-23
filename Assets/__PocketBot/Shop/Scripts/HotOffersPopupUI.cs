using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;
using HyrphusQ.GUI;
using PackReward;
using System.Linq;
using HightLightDebug;
using LatteGames;
using UnityEngine.UI;
using HyrphusQ.Events;
using System;

public class HotOffersPopupUI : MonoBehaviour
{
    public List<PackRewardUI> PackRewardUIs => m_PackRewards;

    [SerializeField, BoxGroup("Config")] private int m_TrophyRoadDisplayed;
    [SerializeField, BoxGroup("Ref")] private TextAdapter m_TimeResetText;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_MainCanvasGroupVisibility;
    [SerializeField, BoxGroup("Ref")] private List<PackRewardUI> m_PackRewards;
    [SerializeField, BoxGroup("Data")] private HotOffersManagerSO m_HotOffersManagerSO;
    [SerializeField, BoxGroup("Data")] private HighestAchievedPPrefFloatTracker m_TrophyHighestAchieved;

    private bool m_IsWatchingAds = false;

    private void Awake()
    {
        LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            m_MainCanvasGroupVisibility.GetOnStartShowEvent().Subscribe(() => { layoutElement.ignoreLayout = false; });
            m_MainCanvasGroupVisibility.GetOnStartHideEvent().Subscribe(() => { layoutElement.ignoreLayout = true; });
        }
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnNewArenaUnlocked, ResetHotOffers);
    }

    private void Start()
    {
        OnInit();
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnNewArenaUnlocked, ResetHotOffers);
    }

    private void Update()
    {
        UpdateRemainingTime();
        if (m_HotOffersManagerSO.LastRewardTime > DateTime.Now)
            ResetHotOffers();
    }

    private void ResetHotOffers()
    {
        m_HotOffersManagerSO.ResetNow();
    }

    private void OnInit()
    {
        if (m_TrophyHighestAchieved.value >= m_TrophyRoadDisplayed)
            m_HotOffersManagerSO.ActivePack();
        else
            m_MainCanvasGroupVisibility.Hide();

        if (!m_HotOffersManagerSO.IsActivePack)
        {
            return;
        }

        m_MainCanvasGroupVisibility.Show();
        ActivateInactivePack();
        SubscribeToRewardEvents();
    }

    private void ActivateInactivePack()
    {
        m_PackRewards.ForEach((v) =>
        {
            if (v is HotOffersItemUI offersItemUI)
            {
                offersItemUI.Active();
            }

            if (v is HotOffersCurrencyUI offersCurrencyUI)
            {
                offersCurrencyUI.Active();
            }
        });
    }

    private void SubscribeToRewardEvents()
    {
        m_PackRewards.ForEach((v) =>
        {
            v.OnStartReward += OnStartReward_Cell;
            v.OnClaimAction += OnRewardGranted_Cell;
            v.OnFailedReward += OnFailedReward_Cell;
        });
    }

    private void ResetCell()
    {
        m_PackRewards.ForEach((v) =>
        {
            if (v is HotOffersItemUI offersItemUI)
            {
                offersItemUI.Reset();
            }

            if (v is HotOffersCurrencyUI offersCurrencyUI)
            {
                offersCurrencyUI.Reset();
            }
        });
    }

    private void UpdateRemainingTime()
    {
        if (m_IsWatchingAds) return;
        if (m_HotOffersManagerSO == null) return;

        string remainingTime = m_HotOffersManagerSO.GetRemainingTimeHandle(36, 36);
        m_TimeResetText.SetText(m_TimeResetText.blueprintText.Replace(Const.StringValue.PlaceholderValue, remainingTime));

        if (m_HotOffersManagerSO.IsResetReward)
        {
            m_HotOffersManagerSO.ResetReward();
            ResetCell();
        }
    }

    private void OnStartReward_Cell()
    {
        m_IsWatchingAds = true;
    }

    private void OnRewardGranted_Cell()
    {
        m_IsWatchingAds = false;
    }

    private void OnFailedReward_Cell()
    {
        m_IsWatchingAds = false;
    }
}
