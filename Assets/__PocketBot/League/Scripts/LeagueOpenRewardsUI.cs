using GachaSystem.Core;
using HyrphusQ.Events;
using LatteGames;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeagueOpenRewardsUI : IUIVisibilityController
{
    private SubscriptionEvent m_OnStartShow = new SubscriptionEvent();
    private SubscriptionEvent m_OnEndShow = new SubscriptionEvent();
    private SubscriptionEvent m_OnStartHide = new SubscriptionEvent();
    private SubscriptionEvent m_OnEndHide = new SubscriptionEvent();
    private RewardGroupInfo m_RewardInfo;

    private LeagueDataSO leagueDataSO => LeagueManager.leagueDataSO;

    public void Show()
    {
        ShowImmediately();
    }

    public void Hide()
    {
        HideImmediately();
    }

    public void ShowImmediately()
    {
        GetOnStartShowEvent().Invoke();
        PBGachaPackManagerSO gachaPackManagerSO = leagueDataSO.gachaPackManagerSO;
        PBGachaCardGenerator cardGenerator = PBGachaCardGenerator.Instance as PBGachaCardGenerator;
        List<GachaCard> gachaCards = cardGenerator.Generate(m_RewardInfo);
        List<GachaPack> gachaBoxes = new List<GachaPack>();
        if (m_RewardInfo.generalItems != null)
        {
            foreach (var keyValuePair in m_RewardInfo.generalItems)
            {
                if(keyValuePair.Key is PBGachaPack gachaBox)
                {
                    var boxRarity = gachaPackManagerSO.GetGachaPackRarity(gachaBox);
                    for (int i = 0; i < keyValuePair.Value.value; i++)
                    {
                        gachaBoxes.Add(gachaPackManagerSO.GetGachaPackCurrentArena(boxRarity));
                    }
                }
            }
        }

        #region Adjust Sink Souce
        if (gachaCards != null)
        {
            gachaCards.ForEach(card => 
            {
                if (card == null) return;
                if (card is GachaCard_Currency gachaCard_Currency)
                {
                    string itemID = "League_Rewards";
                    gachaCard_Currency.ResourceLocationProvider = new ResourceLocationProvider(ResourceLocation.League, itemID);
                }
            });
        }
        #endregion

        #region Log GA
        try
        {
            gachaBoxes.ForEach(pack =>
            {
                if (pack != null)
                {
                    #region DesignEvent
                    string openStatus = "NoTimer";
                    string location = "League";
                    GameEventHandler.Invoke(DesignEvent.OpenBox, openStatus, location);
                    #endregion

                    #region Firebase Event
                    GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, pack, "free");
                    #endregion
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion

        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, gachaCards, gachaBoxes, null, leagueDataSO.configLeague.hasBonusCard);
        GetOnEndShowEvent().Invoke();
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);

        void OnUnpackDone()
        {
            GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackDone, OnUnpackDone);
            HideImmediately();
        }
    }

    public void HideImmediately()
    {
        GetOnStartHideEvent().Invoke();
        GetOnEndHideEvent().Invoke();
    }

    public SubscriptionEvent GetOnStartShowEvent()
    {
        return m_OnStartShow;
    }

    public SubscriptionEvent GetOnEndShowEvent()
    {
        return m_OnEndShow;
    }

    public SubscriptionEvent GetOnStartHideEvent()
    {
        return m_OnStartHide;
    }

    public SubscriptionEvent GetOnEndHideEvent()
    {
        return m_OnEndHide;
    }

    public void Initialize(RewardGroupInfo rewardInfo)
    {
        m_RewardInfo = rewardInfo;
    }
}