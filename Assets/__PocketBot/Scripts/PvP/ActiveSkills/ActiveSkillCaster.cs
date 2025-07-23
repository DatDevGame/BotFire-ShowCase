using System;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class ActiveSkillCaster : MonoBehaviour
{
    public enum SkillState : byte
    {
        Ready,
        OnCooldown,
        Unavailable,
        Active,
    }
    public event Action<ValueDataChanged<float>> onRemainingCooldownChanged = delegate { };
    public event Action<ValueDataChanged<SkillState>> onSkillStateChanged = delegate { };
    public event Action<ValueDataChanged<int>> onCardQuantityChanged = delegate { };

    protected SkillState m_SkillState;
    protected float m_RemainingCooldown;
    protected int m_TotalSkillCastCount;
    protected PBRobot m_Robot;

    [ShowInInspector]
    public virtual float remainingActiveTime => Const.FloatValue.OneF;
    [ShowInInspector]
    public virtual float remainingCooldown
    {
        get => m_RemainingCooldown;
        set
        {
            float oldValue = m_RemainingCooldown;
            float newValue = Mathf.Max(value, 0f);
            m_RemainingCooldown = newValue;
            if (oldValue != newValue)
                onRemainingCooldownChanged.Invoke(new ValueDataChanged<float>(oldValue, newValue));
            if (newValue == GetActiveSkillSO().cooldown)
                skillState = SkillState.OnCooldown;
            else if (newValue <= 0f && skillState != SkillState.Active)
                skillState = IsAbleToPerformSkill() ? SkillState.Ready : SkillState.Unavailable;
        }
    }
    [ShowInInspector]
    public virtual int totalSkillCastCount
    {
        get => m_TotalSkillCastCount;
        protected set => m_TotalSkillCastCount = value;
    }
    [ShowInInspector]
    public virtual SkillState skillState
    {
        get => m_SkillState;
        set
        {
            SkillState oldValue = m_SkillState;
            SkillState newValue = value;
            m_SkillState = value;
            if (oldValue != newValue)
                onSkillStateChanged.Invoke(new ValueDataChanged<SkillState>(oldValue, newValue));
        }
    }
    public virtual PBRobot mRobot => m_Robot;

    protected virtual void Awake()
    {
        ObjectFindCache<ActiveSkillCaster>.Add(this);
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, OnRobotSpawned);
    }

    protected virtual void OnDestroy()
    {
        if (GetActiveSkillSO().TryGetModule(out CardItemModule cardModule))
        {
            cardModule.onNumOfCardsChanged -= OnNumOfCardsChanged;
        }
        ObjectFindCache<ActiveSkillCaster>.Remove(this);
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, OnRobotSpawned);
    }

    protected virtual void Update()
    {
        remainingCooldown -= Time.deltaTime;
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1 + (31 - m_Robot.RobotLayer)) && IsAbleToPerformSkill())
        {
            PerformSkill();
        }
#endif
    }

    protected virtual void OnRobotSpawned(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        if (parameters[0] is PBRobot robot && robot == m_Robot)
            SetupRobot();
    }

    protected virtual void SetupRobot()
    {

    }

    protected virtual void OnNumOfCardsChanged(CardItemModule cardModule, int changedAmount)
    {
        int numOfCards = cardModule.numOfCards;
        onCardQuantityChanged.Invoke(new ValueDataChanged<int>(numOfCards - changedAmount, numOfCards));
    }

    public virtual bool IsOnCooldown()
    {
        return skillState == SkillState.OnCooldown;
    }

    public virtual bool IsCurrentlyActive()
    {
        return skillState == SkillState.Active;
    }

    [Button]
    public virtual bool IsAbleToPerformSkill()
    {
        if (mRobot.IsDead)
            return false;
        if (GetActiveSkillSO().GetNumOfCards() <= 0)
            return false;
        if (IsCurrentlyActive() || IsOnCooldown())
            return false;
        // Check if the robot is being stunned or immobilized
        if ((m_Robot.CombatEffectStatuses & GetActiveSkillSO().skillBlockingEffectStatuses) != 0)
            return false;
        return !IsCurrentlyActive() && !IsOnCooldown();
    }

    public virtual void PerformSkill()
    {
        skillState = SkillState.Active;
        totalSkillCastCount++;
        GetActiveSkillSO().UpdateNumOfCards(Mathf.Max(GetActiveSkillSO().GetNumOfCards() - 1, 0));

        #region Design Event
        try
        {
            GameEventHandler.Invoke(DesignEvent.SkillUsage_Duel_Used, m_Robot, GetActiveSkillSO());
            GameEventHandler.Invoke(DesignEvent.SkillUsage_Battle_Used, m_Robot, GetActiveSkillSO());
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    public abstract bool IsAbleToPerformSkillForAI();
    public abstract ActiveSkillSO GetActiveSkillSO();
}
public abstract class ActiveSkillCaster<T> : ActiveSkillCaster where T : ActiveSkillSO
{
    protected T m_ActiveSkillSO;

    public override ActiveSkillSO GetActiveSkillSO()
    {
        return m_ActiveSkillSO;
    }

    public virtual void Initialize(T activeSkillSO, PBRobot robot)
    {
        m_Robot = robot;
        m_ActiveSkillSO = activeSkillSO;
        m_SkillState = IsAbleToPerformSkill() ? SkillState.Ready : SkillState.Unavailable;
        if (GetActiveSkillSO().TryGetModule(out CardItemModule cardModule))
        {
            cardModule.onNumOfCardsChanged += OnNumOfCardsChanged;
        }
    }
}