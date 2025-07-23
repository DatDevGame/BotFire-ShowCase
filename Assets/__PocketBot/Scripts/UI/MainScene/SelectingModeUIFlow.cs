using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectingModeUIFlow : MonoBehaviour
{
    public static Action<SelectingModeUIFlow> OnCheckFlowCompleted;
    public static bool HasExitedSceneWithPlayModeUIShowing, HasExitedSceneWithBossUIShowing, HasPlayAnimBoxSlot, HasExitedSceneWithBattleBetUIShowing;
    [SerializeField] PlayModeGroup playModeUI;
    [SerializeField] BossMapUI bossModeUI;
    [SerializeField] BattleBetUI battleBetUI;

    public PlayModeGroup PlayModeUI { get => playModeUI; }
    public BossMapUI BossModeUI { get => bossModeUI; }
    public BattleBetUI BattleBetUI { get => battleBetUI; }

    private void Awake()
    {
        PBPackDockUI.OnCheckFillAnimCompleted += OnCheckFillAnimCompleted;
    }

    private void OnDestroy()
    {
        PBPackDockUI.OnCheckFillAnimCompleted -= OnCheckFillAnimCompleted;
        HasExitedSceneWithPlayModeUIShowing = playModeUI.isShowingModeUI;
        HasExitedSceneWithBossUIShowing = bossModeUI.isShowing;
        HasExitedSceneWithBattleBetUIShowing = battleBetUI.isShowing;
    }

    void OnCheckFillAnimCompleted(bool hasPlay)
    {
        HasPlayAnimBoxSlot = hasPlay;
        if (!hasPlay)
        {
            if (HasExitedSceneWithPlayModeUIShowing && playModeUI.CanAutoShow)
            {
                playModeUI.ShowModeUI();
            }
            if (HasExitedSceneWithBossUIShowing)
            {
                bossModeUI.HandleComeBackUIFromPVP(true);
            }
            if (HasExitedSceneWithBattleBetUIShowing)
            {
                battleBetUI.TryShowUI();
            }
        }
        else
        {
            HasExitedSceneWithPlayModeUIShowing = false;
            HasExitedSceneWithBossUIShowing = false;
            HasExitedSceneWithBattleBetUIShowing = false;
        }
        OnCheckFlowCompleted?.Invoke(this);
    }
}
