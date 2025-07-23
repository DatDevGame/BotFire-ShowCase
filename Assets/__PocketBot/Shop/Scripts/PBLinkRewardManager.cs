using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;
using HyrphusQ.Events;

public enum LinkRewardAction
{
    AnyCellOnRewardGranted
}
public class PBLinkRewardManager : Singleton<PBLinkRewardManager>
{
    public Action LoadPopup = delegate { };
    public Action OnLoadQueue = delegate { };
    public List<PBLinkRewardCellSO> Queue
    {
        get
        {
            m_Queue = m_PBLinkRewardManagerSO.GetQueue();
            return m_Queue;
        }
    }
    public PBLinkRewardManagerSO PBLinkRewardManagerSO => m_PBLinkRewardManagerSO;

    [SerializeField, BoxGroup("Ref")] private List<PBLinkRewardCellSO> m_Queue;
    [SerializeField, BoxGroup("Data")] protected PPrefBoolVariable m_PPrefBoolVariable_ShowTheFirstTime;
    [SerializeField, BoxGroup("Data")] protected PPrefIntVariable m_PromotedPPref;
    [SerializeField, BoxGroup("Data")] private PBLinkRewardManagerSO m_PBLinkRewardManagerSO;
    [SerializeField, BoxGroup("Data")] private Variable<int> m_PBLinkRewardConditionShowPopup;

    private bool m_IsWatchingAdsCell = false;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.AddActionEvent(SceneManagementEventCode.OnLoadSceneCompleted, OnLoadSceneCompleted);
        GameEventHandler.AddActionEvent(MainSceneEventCode.OnClickButtonShop, OnClickButtonShop);
        m_PPrefBoolVariable_ShowTheFirstTime.onValueChanged += PPrefBoolVariable_ShowTheFirstTime_OnValueChanged;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
        GameEventHandler.RemoveActionEvent(SceneManagementEventCode.OnLoadSceneCompleted, OnLoadSceneCompleted);
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.OnClickButtonShop, OnClickButtonShop);
        m_PPrefBoolVariable_ShowTheFirstTime.onValueChanged -= PPrefBoolVariable_ShowTheFirstTime_OnValueChanged;
    }

    private void Load(bool isTheFirstTime)
    {
        if (!IsEnoughConditionDisplayed()) return;
        m_PBLinkRewardManagerSO.ActivePack();
    }

    private void OnFinalRoundCompleted(params object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete)
            return;

        if (matchOfPlayer.isVictory)
        {
            m_PBLinkRewardConditionShowPopup.value++;
        }
    }

    private void PPrefBoolVariable_ShowTheFirstTime_OnValueChanged(ValueDataChanged<bool> data)
    {
        if (!data.oldValue)
            Load(true);
        else
            Load(false);
    }

    private void Update()
    {
        if (m_IsWatchingAdsCell) return;

        if (m_PBLinkRewardManagerSO.IsResetReward)
        {
            if (m_PBLinkRewardManagerSO.RewardSequenceType == RewardSequenceType.Promoted)
                m_PromotedPPref.value = 0;
            else
                m_PromotedPPref.value++;

            m_Queue = m_PBLinkRewardManagerSO.GetQueue();

            LoadPopup?.Invoke();
        }

        if (m_PBLinkRewardManagerSO.LastRewardTime > DateTime.Now)
        {
            m_PBLinkRewardManagerSO.Reset();
            m_Queue = m_PBLinkRewardManagerSO.GetQueue();
            LoadPopup?.Invoke();
        }
    }

    private void OnLoadSceneCompleted(params object[] parameters)
    {
        if (parameters == null || parameters.Length < 2)
        {
            Debug.LogWarning("Insufficient parameters passed.");
            return;
        }

        string destinationSceneName = parameters[0] as string;
        string originSceneName = parameters[1] as string;

        if (destinationSceneName == SceneName.MainScene.ToString())
        {
            Load(false);
        }
    }

    private void OnClickButtonShop()
    {
        if(m_PBLinkRewardManagerSO.m_IsDisplayed)
            m_PBLinkRewardManagerSO.ActivePack();
    }

    public bool IsEnoughConditionDisplayed()
    {
        return m_PPrefBoolVariable_ShowTheFirstTime.value;
    }
    public void NoticeWatchingAdsCell(bool isWatching) => m_IsWatchingAdsCell = isWatching;
}
