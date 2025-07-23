using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Events;
using UnityEngine;

public class PBPackDockOpenByGemButton : PackDockOpenByGemButton
{
    protected override void OnButtonClicked()
    {
        if (CurrencyManager.Instance.IsAffordable(GachaPackDockConfigs.SPEED_UP_CONVERT_CURRENCY_TYPE, price))
        {
            #region Firebase Events
            if (gachaPackDockSlot != null)
            {
                GachaPack pBGachaPack = gachaPackDockSlot.GachaPack;
                if (pBGachaPack != null)
                {
                    string openType = "gems";
                    GameEventHandler.Invoke(LogFirebaseEventCode.BoxOpen, pBGachaPack, openType);
                }
            }
            #endregion

            CurrencyManager.Instance.Spend(GachaPackDockConfigs.SPEED_UP_CONVERT_CURRENCY_TYPE, price, resourceLocation, itemID);
            OpenNow();
            OnEnoughGemClicked?.Invoke();
        }
        else
        {
            OnNotEnoughGemClicked?.Invoke();
        }
    }

    public override void OpenNow()
    {
        if (gachaPackDockSlot != null)
        {
            PackDockManager.Instance.UnlockNow(gachaPackDockSlot);
        }
        else if (gachaPack != null)
        {
            GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, null, new List<GachaPack>
                {
                    gachaPack
                }, null, true);
        }
        OnOpenNow?.Invoke();
    }
}