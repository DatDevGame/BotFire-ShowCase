using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

[CreateAssetMenu(fileName = "RobotStatsSO", menuName = "PocketBots/VariableSO/RobotStatsSO")]
public class PBRobotStatsSO : FloatVariableReference
{
    [SerializeField]
    protected ItemSOVariable m_ChassisInUseVariableSO;
    [SerializeField]
    protected ItemSOVariable m_SkillInUseVariableSO;
    [NonSerialized]
    protected IPartStats m_CombinationRobotStats;
    [NonSerialized]
    protected Dictionary<PBPartSlot, IPartStats> m_StatsOfRobot;

    public override float value
    {
        get
        {
            return stats.GetStatsScore().value;
        }
        set
        {
            // Do nothing
        }
    }

    /// <summary>
    /// Stats of both chassis + part items
    /// </summary>
    public virtual IPartStats stats
    {
        get
        {
            if (m_CombinationRobotStats == null)
            {
                m_CombinationRobotStats = new CompoundRobotStats(statsOfRobot);
            }
            return m_CombinationRobotStats;
        }
        set
        {
            m_CombinationRobotStats = value;
        }
    }

    public virtual Dictionary<PBPartSlot, IPartStats> statsOfRobot
    {
        get
        {
            if (m_StatsOfRobot == null)
            {
                m_StatsOfRobot = GetRobotPartStats();
            }
            return m_StatsOfRobot;
        }
        set
        {
            m_StatsOfRobot = value;
        }
    }

    public virtual ItemSOVariable chassisInUse { get => m_ChassisInUseVariableSO; set => m_ChassisInUseVariableSO = value; }
    public virtual ItemSOVariable skillInUse { get => m_SkillInUseVariableSO; set => m_SkillInUseVariableSO = value; }

    Dictionary<PBPartSlot, IPartStats> GetRobotPartStats()
    {
        Dictionary<PBPartSlot, IPartStats> stats = new();
        if (m_ChassisInUseVariableSO.value == null) goto Return;
        var chassisSO = m_ChassisInUseVariableSO.value.Cast<PBChassisSO>();
        stats.Add(PBPartSlot.Body, chassisSO);

        foreach (var slot in chassisSO.AllPartSlots)
        {
            var partVariableSO = slot.PartVariableSO;
            if (partVariableSO.value == null) continue;
            stats.Add(slot.PartSlotType, partVariableSO.value.Cast<PBPartSO>());
        }
    Return:
        return stats;
    }

    protected void OnEnable()
    {
        if (!name.Contains("Player"))
            return;
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnGearCardEquip, NotifyEventStatChanged);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnGearCardUnEquip, NotifyEventStatChanged);

        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnGearCardUpgrade, NotifyEventStatChanged);
        GameEventHandler.AddActionEvent(PBGeneralEventCode.OnAnyStatChanged, OnAnyStatChanged);
    }

    protected void OnDisable()
    {
        if (!name.Contains("Player"))
            return;
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearCardEquip, NotifyEventStatChanged);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearCardUnEquip, NotifyEventStatChanged);

        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearCardUpgrade, NotifyEventStatChanged);
        GameEventHandler.RemoveActionEvent(PBGeneralEventCode.OnAnyStatChanged, OnAnyStatChanged);
    }

    private void OnAnyStatChanged()
    {
        statsOfRobot = GetRobotPartStats();
        m_CombinationRobotStats = new CompoundRobotStats(statsOfRobot);
    }

    protected void NotifyEventStatChanged()
    {
        GameEventHandler.Invoke(PBGeneralEventCode.OnAnyStatChanged);
    }

    public void ForceUpdateStats()
    {
        OnAnyStatChanged();
    }
}
