using System.Collections;
using System.Collections.Generic;
using LatteGames;
using LatteGames.UnpackAnimation;
using UnityEngine;

public class OpenPackManager : Singleton<OpenPackManager>
{
    [SerializeField] OpenPackAnimationSM openPackAnimationSM;
    [SerializeField] int bonusCardTrophyThreshold;
    [SerializeField] CurrencySO trophyCurrencySO;

    public OpenPackAnimationSM OpenPackAnimationSM { get => openPackAnimationSM; set => openPackAnimationSM = value; }

    private void Start()
    {
        // if (trophyCurrencySO.value >= bonusCardTrophyThreshold)
        // {
        //     OpenPackAnimationSM.HasBonusCard = true;
        // }
        // else
        // {
        //     OpenPackAnimationSM.HasBonusCard = false;
        //     StartCoroutine(CommonCoroutine.WaitUntil(() => trophyCurrencySO.value >= bonusCardTrophyThreshold, () =>
        //     {
        //         OpenPackAnimationSM.HasBonusCard = true;
        //     }));
        // }
    }
}
