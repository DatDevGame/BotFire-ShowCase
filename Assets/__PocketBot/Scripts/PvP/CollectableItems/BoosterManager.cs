using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LatteGames;
using static BoosterConfigSO;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

public class BoosterManager : Singleton<BoosterManager>
{
    [ShowInInspector] private List<Booster> m_BoostersLinks;

    protected override void Awake()
    {
        m_BoostersLinks = new List<Booster>();
    }

    public void ReceiveBoosterLink(Booster booster)
    {
        m_BoostersLinks.Add(booster);
    }

    public void CollectBoosterLink(BoosterGroup boosterGroup)
    {
        List<Booster> boosters = CollectBoosterLinksByGroup(boosterGroup);

        float timeRegenerates = 0;
        foreach (var booster in boosters)
        {
            booster.DisableBooster();
            timeRegenerates = booster.RegeneratesBoosterGroup;
        }
        StartCoroutine(CommonCoroutine.Delay(timeRegenerates, false, () =>
        {
            boosters.ForEach(v => v.EnableBooster());
            StartCoroutine(CommonCoroutine.WaitUntil(() => boosters.All(v => v.IsAbleToCollect()), () =>
            {
                boosters.ForEach(v => v.TriggerCollider.enabled = true);
            }));
        }));
    }

    public List<Booster> CollectBoosterLinksByGroup(BoosterGroup boosterGroup)
    {
        m_BoostersLinks = m_BoostersLinks
            .Where(v => v != null)
            .ToList();

        var boostersInGroup = m_BoostersLinks
            .Where(v => v.BoosterGroup == boosterGroup)
            .Select(v => v)
            .ToList();

        return boostersInGroup;
    }
}
