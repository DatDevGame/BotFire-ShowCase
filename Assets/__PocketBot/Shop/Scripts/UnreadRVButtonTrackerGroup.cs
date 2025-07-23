using System.Collections;
using System.Collections.Generic;
using LatteGames.Monetization;
using UnityEngine;

public class UnreadRVButtonTrackerGroup : MonoBehaviour
{
    [SerializeField] UnreadLocation unreadLocation;

    private void Start()
    {
        InitTrackers();
    }

    public void InitTrackers()
    {
        var _RVButtonBehaviorList = GetComponentsInChildren<RVButtonBehavior>(true);
        foreach (var _RVButtonBehavior in _RVButtonBehaviorList)
        {
            var unreadRVButtonTracker = _RVButtonBehavior.gameObject.GetOrAddComponent<UnreadRVButtonTracker>();
            unreadRVButtonTracker.Init(unreadLocation);
        }
    }
}
