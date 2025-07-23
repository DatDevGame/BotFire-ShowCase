using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonTabButton : LatteGames.Tab.TabButton
{
    public static event Action<SeasonTabButton> OnSetStateActive;
    public override void SetState(bool isActive)
    {
        button.interactable = !isActive;
        if (isActive)
        {
            OnSetStateActive?.Invoke(this);
        }
    }
}
