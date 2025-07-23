using GachaSystem.Core;
using HyrphusQ.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PBPackDockUI : PackDockUI
{
    public static Action<bool> OnCheckFillAnimCompleted;
    [SerializeField] float flashFXSpacingDelay = 0.1f;

    private void Awake()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    protected override void Start()
    {
        bool isPlayFillAnimation = false;
        float flashFXdelay = 0;
        List<GachaPack> gachaPacks = new List<GachaPack> ();
        for (var i = 0; i < PackDockManager.Instance.GachaPackDockSO.data.gachaPackDockSlots.Count; i++)
        {
            var slotData = PackDockManager.Instance.GachaPackDockSO.data.gachaPackDockSlots[i];
            if (!isPlayFillAnimation)
            {
                isPlayFillAnimation = !slotData.HasPlayedFillAnim;
            }
            var slotUI = Instantiate(slotPrefab, slotContainer);
            slotUI.Initialize(i);
            if (isPlayFillAnimation)
            {
                ((PBPackDockSlotUI)slotUI).PlayFlashFX(flashFXdelay);
                flashFXdelay += flashFXSpacingDelay;
            }

            //TODO: Hide IAP & Popup
            //if (slotData.GachaPack != null)
            //{
            //    gachaPacks.Add(slotData.GachaPack);
            //}

            //slotData.SetState(GachaPackDockSlotState.Empty);
            //GameEventHandler.Invoke(GachaPackDockEventCode.OnGachaPackDockUpdated);
        }
        //if (gachaPacks.Count > 0 && gachaPacks != null)
        //{
        //    GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, null, gachaPacks, null, false);
        //}

        OnCheckFillAnimCompleted?.Invoke(isPlayFillAnimation);
    }
}
