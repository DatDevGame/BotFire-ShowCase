using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;
using LatteGames.Template;

public class LaserControllerGroup : MonoBehaviour
{
    [SerializeField] private float enableDuration = 3f;
    [SerializeField] private List<LazerGroup> lazerGroups;

    private void DisableAllLazer()
    {
        for (int x = 0; x < lazerGroups.Count; x++)
        {
            for (int y = 0; y < lazerGroups[x].LaserFences.Count; y++)
            {
                lazerGroups[x].LaserFences[y].OffController();
            }
        }
    }
    private IEnumerator Start()
    {
        if (lazerGroups.Count <= 0) yield break;
        DisableAllLazer();

        while (true)
        {
            for (int x = 0; x < lazerGroups.Count; x++)
            {
                for (int y = 0; y < lazerGroups[x].LaserFences.Count; y++)
                {
                    if (x > 0)
                        lazerGroups[x - 1].LaserFences[y].OffController();

                    lazerGroups[x].LaserFences[y].OnController();
                }
                yield return new WaitForSeconds(enableDuration);
            }

            DisableAllLazer();
        }
    }
}

[Serializable]
public class LazerGroup
{
    public List<LaserFence> LaserFences;
}
