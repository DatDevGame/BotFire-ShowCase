using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using static MultiplyRewardArc;

[CreateAssetMenu(fileName = "MultiplierRewardsDataSO", menuName = "PocketBots/PvP/MultiplierRewardsDataSO")]
public class MultiplierRewardsDataSO : ScriptableObject
{
    [Serializable]
    public class Config
    {
        [SerializeField] private int m_ShowingTrophyThreshold = 100;
        [SerializeField] private int m_NormalSetFrequency = 1;
        [SerializeField] private int m_PromotedSetFrequency = 3;
        [SerializeField] private SegmentConfigMode m_SingleModeSegment;
        [SerializeField] private SegmentConfigMode m_BattleModeSegment;

        public int showingTrophyThreshold => m_ShowingTrophyThreshold;
        public int normalSetFrequency => m_NormalSetFrequency;
        public int promotedSetFrequency => m_PromotedSetFrequency;
        public SegmentConfigMode SingleModeSegment => m_SingleModeSegment;
        public SegmentConfigMode BattleModeSegment => m_BattleModeSegment;

        public void DeepCopy(Config config)
        {
            m_ShowingTrophyThreshold = config.showingTrophyThreshold;
            m_NormalSetFrequency = config.normalSetFrequency;
            m_PromotedSetFrequency = config.promotedSetFrequency;
            m_SingleModeSegment = config.SingleModeSegment;
            m_BattleModeSegment = config.BattleModeSegment;
        }
    }

    [Serializable]
    public class SegmentConfigMode
    {
        public List<SegmentInfo> NomalSegments;
        public List<SegmentInfo> PromotedSegments;
    }

    [SerializeField, BoxGroup("Config")]
    private Config m_Config;
    [SerializeField]
    private PPrefIntVariable m_NormalSetCountVar;
    [SerializeField]
    private PPrefIntVariable m_PromotedSetCountVar;
    [SerializeField]
    private HighestAchievedPPrefFloatTracker m_HighestTrophiesVar;

    public Config config => m_Config;
    public PPrefIntVariable normalSetCountVar => m_NormalSetCountVar;
    public PPrefIntVariable promotedSetCountVar => m_PromotedSetCountVar;

    public bool IsEnoughTrophy()
    {
        return m_HighestTrophiesVar.value >= config.showingTrophyThreshold;
    }

    public bool IsAbleToShowMultiplierRewards()
    {
        return m_NormalSetCountVar.value == 0;
    }

    public bool IsAbleToShowPromotedRewards()
    {
        return m_PromotedSetCountVar.value >= config.promotedSetFrequency;
    }
}