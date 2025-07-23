using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using UnityEngine;

[EventCode]
public enum CompetitorStatusEventCode
{
    /// <summary>
    /// Raised when a competitor health changed
    /// <para><typeparamref name="Competitor"/>: Competitor</para>
    /// <para><typeparamref name="HealthChangedEventData"/>: HealthChangedEventData</para>
    /// </summary>
    OnHealthChanged,
    /// <summary>
    /// Raised when a competitor has join the match
    /// <para><typeparamref name="Competitor"/>: Competitor</para>
    /// </summary>
    OnCompetitorJoined,
    /// <summary>
    /// Raised when a competitor has left the match
    /// <para><typeparamref name="Competitor"/>: Competitor</para>
    /// </summary>
    OnCompetitorLeft,
    /// <summary>
    /// Raised when a competitor before died
    /// <para><typeparamref name="Competitor"/>: Competitor</para>
    /// </summary>
    OnCompetitorBeforeDied
}

public class Competitor : MonoBehaviour
{
    public event Action<HealthChangedEventData> OnHealthChanged = delegate { };

    protected float health;
    protected float maxHealth;

    public float Health
    {
        get => health;
        set
        {
            float oldHealth = health;
            health = Mathf.Max(0, value);
            var eventData = new HealthChangedEventData(oldHealth, health, MaxHealth);
            if (health <= 0)
                GameEventHandler.Invoke(CompetitorStatusEventCode.OnCompetitorBeforeDied, this);
            OnHealthChanged(eventData);
            GameEventHandler.Invoke(CompetitorStatusEventCode.OnHealthChanged, this, eventData);
        }
    }

    public float MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
            Health = value;
        }
    }

    public float HealthPercentage => Health / MaxHealth;

    public bool IsDead => Health <= 0;

    [SerializeField] protected PlayerInfoVariable playerInfoVariable;
    public PlayerInfoVariable PlayerInfoVariable => playerInfoVariable;
    public PersonalInfo PersonalInfo => playerInfoVariable.value.personalInfo;

    public class HealthChangedEventData
    {
        public float OldHealth;
        public float CurrentHealth;
        public float MaxHealth;

        public HealthChangedEventData(float oldHealth, float currentHealth, float maxHealth)
        {
            OldHealth = oldHealth;
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }

    protected void NotifyCompetitorJoined()
    {
        GameEventHandler.Invoke(CompetitorStatusEventCode.OnCompetitorJoined, this);
    }

    private void OnDestroy()
    {
        GameEventHandler.Invoke(CompetitorStatusEventCode.OnCompetitorLeft, this);
    }
}
