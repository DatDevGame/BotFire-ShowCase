using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class SeasonProgressNode : MonoBehaviour
{
    [SerializeField] SlicedFilledImage core;

    public void Init(bool isActive)
    {
        core.fillAmount = isActive ? 1 : 0;
    }

    public void IncreaseFill()
    {
        if (core.fillAmount != 1)
        {
            StopAllCoroutines();
            StartCoroutine(CommonCoroutine.LerpFactor(AnimationDuration.TINY, t => core.fillAmount = t * 1));
        }
    }
}
