using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class PlayerKDA
{
    public Action<PlayerKDA> OnChangedKDA;

    public string PlayerName;
    public int Kills;
    public int Deaths;
    public int Assists;

    public PlayerKDA(string name)
    {
        PlayerName = name;
        Kills = 0;
        Deaths = 0;
        Assists = 0;
    }

    public void AddKill()
    {
        Kills++;
        OnChangedKDA?.Invoke(this);
    }

    public void AddDeath()
    {
        Deaths++;
        OnChangedKDA?.Invoke(this);
    }
    public void AddAssist()
    {
        Assists++;
        OnChangedKDA?.Invoke(this);
    }

    public float GetKDA()
    {
        return Deaths == 0 ? Kills + Assists : (float)(Kills + Assists) / Deaths;
    }

    public override string ToString()
    {
        return $"{PlayerName} - K: {Kills} D: {Deaths} A: {Assists} (KDA: {GetKDA():0.00})";
    }
}

