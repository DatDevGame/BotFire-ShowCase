using HightLightDebug;
using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


public enum PiggyAction
{
    CalcPerKill
}

public class PiggyBankManager : Singleton<PiggyBankManager>
{
    [SerializeField, BoxGroup("Data")] private PiggyBankManagerSO m_PiggyBankManagerSO;
    [SerializeField, BoxGroup("Data")] private Variable<Mode> m_CurrentMode;
    private int m_KilledCount = 0;

    protected override void Awake()
    {
        base.Awake();
        GameEventHandler.AddActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
    }

    private void OnKillDeathInfosUpdated(object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null || parameters[1] == null || parameters[2] == null) return;
        var killAndDeathTracker = parameters[0] as KillAndDeathTracker;
        var killer = parameters[1] as PBRobot;
        var victim = parameters[2] as PBRobot;
        if (killer.PersonalInfo.isLocal)
        {
            m_KilledCount++;
        }
    }

    public void CalcPerKill(params object[] parameters)
    {
        if (!m_PiggyBankManagerSO.IsDisplayed) return;

        //Handle Boss
        if (parameters.Length > 0 && parameters[0] != null)
            m_KilledCount++;
        StartCoroutine(WaitingCalcShortCut());
    }

    private IEnumerator WaitingCalcShortCut()
    {
        yield return new WaitForSeconds(1);

        if (m_CurrentMode.value != Mode.Boss)
        {
            WinStreakPopupUI winStreakPopupUI = WinStreakPopupUI.Instance;
            PBPvPGameOverPackInfoUI pBPvPGameOverPackInfoUI = FindObjectOfType<PBPvPGameOverPackInfoUI>();
            yield return new WaitUntil(() => pBPvPGameOverPackInfoUI != null);
            yield return new WaitUntil(() => !winStreakPopupUI.IsWaitingShow && !pBPvPGameOverPackInfoUI.IsPopup);
        }

        GameEventHandler.Invoke(PiggyAction.CalcPerKill, m_KilledCount);
        m_PiggyBankManagerSO.PerKill(m_KilledCount);
        m_KilledCount = 0;
    }
}
