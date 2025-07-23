using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnreadManager : Singleton<UnreadManager>
{
    public Action OnUnreadTypeUpdated;
    private Dictionary<UnreadType, Dictionary<UnreadLocation, HashSet<GameObject>>> unreadGOList = new Dictionary<UnreadType, Dictionary<UnreadLocation, HashSet<GameObject>>>();

    protected override void Awake()
    {
        foreach (UnreadType unreadType in Enum.GetValues(typeof(UnreadType)))
        {
            if (unreadType != UnreadType.None)
            {
                unreadGOList.Add(unreadType, new Dictionary<UnreadLocation, HashSet<GameObject>>());
                foreach (UnreadLocation unreadLocation in Enum.GetValues(typeof(UnreadLocation)))
                {
                    unreadGOList[unreadType].Add(unreadLocation, new HashSet<GameObject>());
                }
            }
        }
    }

    public virtual UnreadType GetUnreadType(UnreadLocation unreadLocation)
    {
        if (unreadGOList[UnreadType.New][unreadLocation].Count > 0)
        {
            return UnreadType.New;
        }
        else if (unreadGOList[UnreadType.Dot][unreadLocation].Count > 0)
        {
            return UnreadType.Dot;
        }
        else
        {
            return UnreadType.None;
        }
    }

    public void AddUnreadTag(UnreadType unreadType, UnreadLocation unreadLocation, GameObject gameObject)
    {
        var gameObjects = unreadGOList[unreadType][unreadLocation];
        gameObjects.Add(gameObject);
        OnUnreadTypeUpdated?.Invoke();
    }

    public void RemoveUnreadTag(UnreadType unreadType, UnreadLocation unreadLocation, GameObject gameObject)
    {
        var gameObjects = unreadGOList[unreadType][unreadLocation];
        if (gameObjects.Contains(gameObject))
        {
            gameObjects.Remove(gameObject);
        }
        OnUnreadTypeUpdated?.Invoke();
    }
}

public enum UnreadType
{
    None,
    New,
    Dot
}

public enum UnreadLocation
{
    ArenaOffers,
    LinkRewards,
    FullSkins,
    DailyDeals,
    GachaBoxes,
    StarterPack,
    HotOffers,
    CoinsPacks,
    GemPacks,
    RVTickets,
    SeasonCompletedMissions,
    SeasonUnlockedMilestones,
}
