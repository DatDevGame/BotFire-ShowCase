using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ReviveDataSO", menuName = "PocketBots/BossFight/ReviveDataSO")]
[WindowMenuItem(menuItemPath: "General/BossFight/", assetFolderPath: "Assets/__PocketBot/BossFight/ScriptableObject")]
public class ReviveDataSO : ScriptableObject
{
    [SerializeField]
    private int m_MaxReviveTimes = 1;
    [SerializeField]
    private int m_CountdownTime = 10;
    [SerializeField]
    private float m_TimeToShowNoThanksButton = 3;
    [SerializeField]
    private int m_RequiredNumOfGemsToRevive = 30;
    [SerializeField, Range(0f, 1f)]
    private float m_HealthPercentageAfterRevive = 0.5f;
    [NonSerialized]
    private int m_RemainedReviveTimes;
    public bool isAbleToRevive
    {
        get
        {
            if (remainedReviveTimes <= 0)
                return false;
            if (ObjectFindCache<PBPvPStateGameController>.Get()?.IsSurrender() ?? false)
                return false;
            if (ObjectFindCache<PBLevelController>.Get()?.RemainingTime <= 0)
                return false;
            return true;
        }
    }
    public int maxReviveTimes { get => m_MaxReviveTimes; set => m_MaxReviveTimes = value; }
    public int remainedReviveTimes => m_RemainedReviveTimes;
    public int countdownTime => m_CountdownTime;
    public float timeToShowNoThanksButton => m_TimeToShowNoThanksButton;
    public int requiredNumOfGemsToRevive => m_RequiredNumOfGemsToRevive;
    public float healthPercentageAfterRevive => m_HealthPercentageAfterRevive;
    public void RefillReviveTimes()
    {
        m_RemainedReviveTimes = m_MaxReviveTimes;
    }
    public void SubtractReviveTimes(int numOfReviveTimes)
    {
        m_RemainedReviveTimes -= numOfReviveTimes;
    }
}