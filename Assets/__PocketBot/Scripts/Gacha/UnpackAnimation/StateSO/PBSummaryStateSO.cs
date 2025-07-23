using System;
using System.Collections;
using System.Collections.Generic;
using LatteGames.UnpackAnimation;
using UnityEngine;

public class PBSummaryStateSO : SummaryStateSO
{
    public static Action OnStateDisabledWithPack;

    protected override void StateDisable()
    {
        base.StateDisable();
        if (controller.CurrentSubPackInfo.cardPlace == CardPlace.NormalPack)
        {
            OnStateDisabledWithPack?.Invoke();
        }
    }
}
