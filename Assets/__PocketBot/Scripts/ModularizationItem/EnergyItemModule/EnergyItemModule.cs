using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyItemModule : ItemModule
{
    public event Action<EnergyItemModule> onEnergyChanged;

    [SerializeField]
    protected int m_DefaultEnergy = 5;
    [SerializeField]
    protected int m_MaxEnergy = 5;
    [SerializeField]
    protected int m_AmountOfEnergyRefillPerTime = 5;
    [SerializeField]
    protected int m_AmountOfEnergyConsumePerTime = 1;

    public virtual int maxEnergy => m_MaxEnergy;
    public virtual int currentEnergy
    {
        get
        {
            return PlayerPrefs.GetInt($"{nameof(EnergyItemModule)}_CurrentEnergy_{itemSO.guid}", m_DefaultEnergy);
        }
        set
        {
            var prevEnergy = currentEnergy;
            var curEnergy = Mathf.Clamp(value, 0, m_MaxEnergy);
            PlayerPrefs.SetInt($"{nameof(EnergyItemModule)}_CurrentEnergy_{itemSO.guid}", curEnergy);
            if (curEnergy != prevEnergy)
            {
                onEnergyChanged?.Invoke(this);
            }
        }
    }

    public void ConsumeEnergy(int amountOfEnergy)
    {
        currentEnergy -= amountOfEnergy;
    }

    public void ConsumeEnergy()
    {
        ConsumeEnergy(m_AmountOfEnergyConsumePerTime);
    }

    public void RefillEnergy(int amountOfEnergy)
    {
        currentEnergy += amountOfEnergy;
    }

    public void RefillEnergy()
    {
        RefillEnergy(m_AmountOfEnergyRefillPerTime);
    }

    public bool IsOutOfEnergy()
    {
        return currentEnergy <= 0;
    }

    public bool IsFullEnergy()
    {
        return currentEnergy >= m_MaxEnergy;
    }
}