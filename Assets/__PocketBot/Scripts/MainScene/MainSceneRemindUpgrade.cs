using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;

public class MainSceneRemindUpgrade : MonoBehaviour
{
    public static bool HasClickedUpgradeBtn = false;

    private void Start()
    {
        StartCoroutine(CommonCoroutine.Delay(0.001f, false, () =>
        {
            if (HasClickedUpgradeBtn)
            {
                HasClickedUpgradeBtn = false;
                GameEventHandler.Invoke(BossFightEventCode.OnBossMapClosed);
                GameEventHandler.Invoke(BattleBetEventCode.OnBattleBetClosed);
                GameEventHandler.Invoke(MainSceneEventCode.OnManuallyClickButton, ButtonType.Character);
            }
        }));
    }
}
