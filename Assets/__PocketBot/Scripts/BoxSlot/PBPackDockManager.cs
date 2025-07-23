using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class PBPackDockManager : PackDockManager
{
    public static Action OnBoxOpened;
    [SerializeField] private PPrefBoolVariable isActiveFullSlotBox;

    public bool IsSemiStandardOpenStatus(GachaPackDockSlot gachaPackDockSlot)
    {
        var slotIndex = gachaPackDockData.gachaPackDockSlots.IndexOf(gachaPackDockSlot);
        return GetBoxSlotSpeedUpRVAmount(slotIndex) > 0;
    }

    public int GetBoxSlotSpeedUpRVAmount(int slotIndex)
    {
        return PlayerPrefs.GetInt($"{slotIndex}_BoxSlotSpeedUpRVAmount", 0);
    }

    public void SetBoxSlotSpeedUpRVAmount(int slotIndex, int value)
    {
        PlayerPrefs.SetInt($"{slotIndex}_BoxSlotSpeedUpRVAmount", value);
    }

    public override void ReduceUnlockTime(GachaPackDockSlot gachaPackDockSlot)
    {
        var slotIndex = gachaPackDockData.gachaPackDockSlots.IndexOf(gachaPackDockSlot);
        SetBoxSlotSpeedUpRVAmount(slotIndex, GetBoxSlotSpeedUpRVAmount(slotIndex) + 1);
        base.ReduceUnlockTime(gachaPackDockSlot);
    }

    public override bool TryToAddPack(GachaPack gachaPack)
    {
        string slotState = IsFull ? "Full" : "NotFull";
        if (gachaPack == null || IsFull)
        {
            GameEventHandler.Invoke(DesignEvent.CollectBox, gachaPack, slotState);

            #region Firebase Event
            bool isSlotFilled = true;
            GameEventHandler.Invoke(LogFirebaseEventCode.BoxAvailable, gachaPack, isSlotFilled);
            #endregion

            return false;
        }
        for (var i = 0; i < gachaPackDockData.gachaPackDockSlots.Count; i++)
        {
            var slot = gachaPackDockData.gachaPackDockSlots[i];
            if (slot.State == GachaPackDockSlotState.Empty)
            {
                slot.GachaPack = gachaPack;
                slot.SetState(GachaPackDockSlotState.WaitToUnlock);
                slot.HasPlayedFillAnim = false;
                SetBoxSlotSpeedUpRVAmount(i, 0);
                GameEventHandler.Invoke(GachaPackDockEventCode.OnAddPackToDock, slot);
                GameEventHandler.Invoke(DesignEvent.CollectBox, gachaPack, slotState);
                UpdateSlotStates();

                #region Firebase Event
                bool isSlotFilled = false;
                GameEventHandler.Invoke(LogFirebaseEventCode.BoxAvailable, gachaPack, isSlotFilled);
                #endregion
                return true;
            }
        }
        return false;
    }

    public override void UnlockNow(GachaPackDockSlot gachaPackDockSlot)
    {
        OpenSlot(gachaPackDockSlot);
        UpdateSlotStates();
    }

    public override void OpenSlot(GachaPackDockSlot gachaPackDockSlot)
    {
        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, null, new List<GachaPack>
        {
            gachaPackDockSlot.GachaPack
        }, null, true);
        gachaPackDockSlot.SetState(GachaPackDockSlotState.Empty);
        GameEventHandler.Invoke(GachaPackDockEventCode.OnGachaPackDockUpdated);
        isActiveFullSlotBox.value = false;
        OnBoxOpened?.Invoke();
    }

    public virtual void SetUnlockedToAllUnlockingSlot()
    {
        foreach (var slot in gachaPackDockData.gachaPackDockSlots)
        {
            if (slot.State == GachaPackDockSlotState.Unlocking)
            {
                slot.SetState(GachaPackDockSlotState.Unlocked);
                UpdateSlotStates();
            }
        }
    }

#if UNITY_EDITOR
    [OnInspectorGUI, PropertyOrder(100)]
    private void OnInspectorGUI()
    {
        var centerStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.LabelField("=============== PlayerPref Variables ===============", centerStyle);
        for (var i = 0; i < 4; i++)
        {
            var amount = EditorGUILayout.IntField($"BoxSlotSpeedUpRVAmount_{i}: ", GetBoxSlotSpeedUpRVAmount(i));
            SetBoxSlotSpeedUpRVAmount(i, amount);
        }
    }
#endif
}
