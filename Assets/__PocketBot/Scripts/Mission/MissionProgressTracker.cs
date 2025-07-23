using System;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Events;
using LatteGames.Monetization;
using LatteGames.PvP;
using UnityEngine;

[Serializable]
public abstract class MissionProgressTracker
{
    public event Action<float> onMissionProgressChanged;
    public event Action onMissionCompleted;
    public virtual string condition1 => string.Empty;
    public virtual string condition2 => string.Empty;

    public abstract void InitTracker();
    public abstract void ClearTracker();

    protected void NotifyMissionProgressChanged(float changedAmount)
    {
        onMissionProgressChanged?.Invoke(changedAmount);
    }

    protected void NotifyMissionCompleted()
    {
        onMissionCompleted?.Invoke();
    }

    public virtual object[] Serialize()
    {
        return null;
    }
}

[Serializable]
public abstract class RequirePart_MissionProgressTracker : MissionProgressTracker
{
    [SerializeField] protected PBPartSO _requiredPartSO;
    public PBPartSO requiredPartSO => _requiredPartSO;
}

//MissionTargetType.MatchWins_Count_Any
[Serializable]
public sealed class MatchWins_Count_Any_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete)
            return;
        if (matchOfPlayer.isVictory)
            NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.MatchWins_Count_Duel
[Serializable]
public sealed class MatchWins_Count_Duel_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete)
            return;
        if (matchOfPlayer.isVictory && matchOfPlayer.mode == Mode.Normal)
            NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.Battle_Top_1
[Serializable]
public sealed class Battle_Top_1_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete)
            return;
        if (matchOfPlayer.mode == Mode.Battle && matchOfPlayer.rankOfMine == 1)
            NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.WeaponMastery_Count_Equip
[Serializable]
public sealed class WeaponMastery_Count_Equip_ProgressTracker : RequirePart_MissionProgressTracker
{
    private bool _isValidMatch;
    public override string condition1 => _requiredPartSO.GetDisplayName();

    public WeaponMastery_Count_Equip_ProgressTracker() { }

    public WeaponMastery_Count_Equip_ProgressTracker(PBPartSO requiredPartSO)
    {
        _requiredPartSO = requiredPartSO;
    }

    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override object[] Serialize()
    {
        return new object[] { _requiredPartSO.guid };
    }

    private void OnMatchStarted(object[] parameters)
    {
        _isValidMatch = false;
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        if (matchOfPlayer.GetLocalPlayerInfo() is not PBPlayerInfo player)
            return;
        foreach (var part in player.robotStatsSO.chassisInUse.value.Cast<PBChassisSO>().AllPartSlots)
        {
            if (part.PartVariableSO.value is PBPartSO equipedPart && equipedPart.guid == _requiredPartSO.guid)
            {
                _isValidMatch = true;
                return;
            }
        }
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete || !_isValidMatch)
            return;
        NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.WeaponMastery_Count_Win
[Serializable]
public sealed class WeaponMastery_Count_Win_ProgressTracker : RequirePart_MissionProgressTracker
{
    private bool _isValidMatch;
    public override string condition1 => _requiredPartSO.GetDisplayName();

    public WeaponMastery_Count_Win_ProgressTracker() { }

    public WeaponMastery_Count_Win_ProgressTracker(PBPartSO requiredPartSO)
    {
        _requiredPartSO = requiredPartSO;
    }

    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override object[] Serialize()
    {
        return new object[] { _requiredPartSO.guid };
    }

    private void OnMatchStarted(object[] parameters)
    {
        _isValidMatch = false;
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        if (matchOfPlayer.GetLocalPlayerInfo() is not PBPlayerInfo player)
            return;
        foreach (var part in player.robotStatsSO.chassisInUse.value.Cast<PBChassisSO>().AllPartSlots)
        {
            if (part.PartVariableSO.value is PBPartSO equipedPart && equipedPart.guid == _requiredPartSO.guid)
            {
                _isValidMatch = true;
                return;
            }
        }
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete || !matchOfPlayer.isVictory || !_isValidMatch)
            return;
        NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.WeaponMastery_Count_Kill
[Serializable]
public sealed class WeaponMastery_Count_Kill_ProgressTracker : RequirePart_MissionProgressTracker
{
    private bool _isValidMatch;
    private bool _isKillingWorkOnBoss;//HACK: boss map don' have killing system. work around by using winning
    private Mode _mode;
    public override string condition1 => _requiredPartSO.GetDisplayName();

    public WeaponMastery_Count_Kill_ProgressTracker() { }

    public WeaponMastery_Count_Kill_ProgressTracker(PBPartSO requiredPartSO)
    {
        _requiredPartSO = requiredPartSO;
    }

    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.RemoveActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.AddActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override object[] Serialize()
    {
        return new object[] { _requiredPartSO.guid };
    }

    private void OnMatchStarted(object[] parameters)
    {
        _isValidMatch = false;
        _isKillingWorkOnBoss = false;
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        _mode = matchOfPlayer.mode;
        if (matchOfPlayer.GetLocalPlayerInfo() is not PBPlayerInfo player)
            return;
        foreach (var part in player.robotStatsSO.chassisInUse.value.Cast<PBChassisSO>().AllPartSlots)
        {
            if (part.PartVariableSO.value is PBPartSO equipedPart && equipedPart.guid == _requiredPartSO.guid)
            {
                _isValidMatch = true;
                return;
            }
        }
    }

    private void OnKillDeathInfosUpdated(object[] parameters)
    {
        if (parameters.Length < 3 || parameters[0] == null || parameters[1] == null || parameters[2] == null || !_isValidMatch)
            return;
        var killer = parameters[1] as PBRobot;
        if (killer.PersonalInfo.isLocal)
            NotifyMissionProgressChanged(1);
        if (_mode == Mode.Boss)
            _isKillingWorkOnBoss = true;
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete || !_isValidMatch)
            return;
        if (matchOfPlayer.isVictory && matchOfPlayer.mode == Mode.Boss && !_isKillingWorkOnBoss)
            NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.MatchPlay_Count_Any
[Serializable]
public sealed class MatchPlay_Count_Any_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
    }

    private void OnMatchStarted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.MatchPlay_Count_Duel
[Serializable]
public sealed class MatchPlay_Count_Duel_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
    }

    private void OnMatchStarted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || matchOfPlayer.mode != Mode.Normal)
            return;
        NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.MatchPlay_Count_BattleRoyale
[Serializable]
public sealed class MatchPlay_Count_BattleRoyale_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
    }

    private void OnMatchStarted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || matchOfPlayer.mode != Mode.Battle)
            return;
        NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.KillMaster_Count_Any
[Serializable]
public sealed class KillMaster_Count_Any_ProgressTracker : MissionProgressTracker
{
    private bool _isKillingWorkOnBoss;//HACK: boss map don' have killing system. work around by using winning
    private Mode _mode;

    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.RemoveActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.AddActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnMatchStarted(object[] parameters)
    {
        _isKillingWorkOnBoss = false;
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        _mode = matchOfPlayer.mode;
    }

    private void OnKillDeathInfosUpdated(object[] parameters)
    {
        if (parameters.Length < 3 || parameters[0] == null || parameters[1] == null || parameters[2] == null)
            return;
        var killer = parameters[1] as PBRobot;
        if (killer.PersonalInfo.isLocal)
            NotifyMissionProgressChanged(1);
        if (_mode == Mode.Boss)
            _isKillingWorkOnBoss = true;
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete)
            return;
        if (matchOfPlayer.isVictory && matchOfPlayer.mode == Mode.Boss && !_isKillingWorkOnBoss)
            NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.KillMaster_Count_DoubleKill
[Serializable]
public sealed class KillMaster_Count_DoubleKill_ProgressTracker : MissionProgressTracker
{
    private int _killCount;
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        _killCount = 0;
        GameEventHandler.AddActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnKillDeathInfosUpdated(object[] parameters)
    {
        if (parameters.Length < 3 || parameters[0] == null || parameters[1] == null || parameters[2] == null)
            return;
        var killer = parameters[1] as PBRobot;
        if (killer.PersonalInfo.isLocal)
            _killCount++;
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete || !matchOfPlayer.isVictory || _killCount < 2)
        {
            _killCount = 0;
            return;
        }
        NotifyMissionProgressChanged(1);
        _killCount = 0;
    }
}

//MissionTargetType.KillMaster_Count_TripleKill
[Serializable]
public sealed class KillMaster_Count_TripleKill_ProgressTracker : MissionProgressTracker
{
    private int _killCount;
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        _killCount = 0;
        GameEventHandler.AddActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnKillDeathInfosUpdated(object[] parameters)
    {
        if (parameters.Length < 3 || parameters[0] == null || parameters[1] == null || parameters[2] == null)
            return;
        var killer = parameters[1] as PBRobot;
        if (killer.PersonalInfo.isLocal)
            _killCount++;
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete || !matchOfPlayer.isVictory || _killCount < 3)
        {
            _killCount = 0;
            return;
        }
        NotifyMissionProgressChanged(1);
        _killCount = 0;
    }
}

//MissionTargetType.KillMaster_Count_UltraKill
[Serializable]
public sealed class KillMaster_Count_UltraKill_ProgressTracker : MissionProgressTracker
{
    private int _killCount;
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        _killCount = 0;
        GameEventHandler.AddActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnKillDeathInfosUpdated(object[] parameters)
    {
        if (parameters.Length < 3 || parameters[0] == null || parameters[1] == null || parameters[2] == null)
            return;
        var killer = parameters[1] as PBRobot;
        if (killer.PersonalInfo.isLocal)
            _killCount++;
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete || !matchOfPlayer.isVictory || _killCount < 4)
        {
            _killCount = 0;
            return;
        }
        NotifyMissionProgressChanged(1);
        _killCount = 0;
    }
}

//MissionTargetType.KillMaster_Count_RampageKill
[Serializable]
public sealed class KillMaster_Count_RampageKill_ProgressTracker : MissionProgressTracker
{
    private int _killCount;
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        _killCount = 0;
        GameEventHandler.AddActionEvent(KillingSystemEvent.OnKillDeathInfosUpdated, OnKillDeathInfosUpdated);
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnKillDeathInfosUpdated(object[] parameters)
    {
        if (parameters.Length < 3 || parameters[0] == null || parameters[1] == null || parameters[2] == null)
            return;
        var killer = parameters[1] as PBRobot;
        if (killer.PersonalInfo.isLocal)
            _killCount++;
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete || !matchOfPlayer.isVictory || _killCount < 5)
        {
            _killCount = 0;
            return;
        }
        NotifyMissionProgressChanged(1);
        _killCount = 0;
    }
}

//MissionTargetType.EarnCurrency_Count_Trophies
[Serializable]
public sealed class EarnCurrency_Count_Trophies_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        if (CurrencyManager.Instance)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Medal).onValueChanged -= OnCurrencyChanged;
    }

    public override void InitTracker()
    {
        CurrencyManager.Instance.GetCurrencySO(CurrencyType.Medal).onValueChanged += OnCurrencyChanged;
    }

    private void OnCurrencyChanged(ValueDataChanged<float> valueDataChanged)
    {
        if (valueDataChanged.newValue > valueDataChanged.oldValue)
            NotifyMissionProgressChanged(valueDataChanged.newValue - valueDataChanged.oldValue);
    }
}

//MissionTargetType.EarnCurrency_Count_Coins
[Serializable]
public sealed class EarnCurrency_Count_Coins_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        if (CurrencyManager.Instance)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Standard).onValueChanged -= OnCurrencyChanged;
    }

    public override void InitTracker()
    {
        CurrencyManager.Instance.GetCurrencySO(CurrencyType.Standard).onValueChanged += OnCurrencyChanged;
    }

    private void OnCurrencyChanged(ValueDataChanged<float> valueDataChanged)
    {
        if (valueDataChanged.newValue > valueDataChanged.oldValue)
            NotifyMissionProgressChanged(valueDataChanged.newValue - valueDataChanged.oldValue);
    }
}

//MissionTargetType.EarnCurrency_Count_Gems
[Serializable]
public sealed class EarnCurrency_Count_Gems_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        if (CurrencyManager.Instance)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Premium).onValueChanged -= OnCurrencyChanged;
    }

    public override void InitTracker()
    {
        CurrencyManager.Instance.GetCurrencySO(CurrencyType.Premium).onValueChanged += OnCurrencyChanged;
    }

    private void OnCurrencyChanged(ValueDataChanged<float> valueDataChanged)
    {
        if (valueDataChanged.newValue > valueDataChanged.oldValue)
            NotifyMissionProgressChanged(valueDataChanged.newValue - valueDataChanged.oldValue);
    }
}

//MissionTargetType.StreakMastery_Reach_Streak
[Serializable]
public sealed class StreakMastery_Reach_Streak_ProgressTracker : MissionProgressTracker
{
    [SerializeField] private PPrefIntVariable _currentWinStreak;

    public StreakMastery_Reach_Streak_ProgressTracker() { }

    public StreakMastery_Reach_Streak_ProgressTracker(PPrefIntVariable currentWinStreak)
    {
        _currentWinStreak = currentWinStreak;
    }

    public override void ClearTracker()
    {
        _currentWinStreak.onValueChanged -= OnWinstreakValueChange;
    }

    public override void InitTracker()
    {
        NotifyMissionProgressChanged(_currentWinStreak.value);
        _currentWinStreak.onValueChanged += OnWinstreakValueChange;
    }

    private void OnWinstreakValueChange(ValueDataChanged<int> valueDataChanged)
    {
        NotifyMissionProgressChanged(valueDataChanged.newValue);
    }
}

//MissionTargetType.BossDefeats_Count
[Serializable]
public sealed class BossDefeats_Count_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete)
            return;
        if (matchOfPlayer.isVictory && matchOfPlayer.mode == Mode.Boss)
            NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.BossDefeats_Specific
[Serializable]
public sealed class BossDefeats_Specific_ProgressTracker : MissionProgressTracker
{
    [SerializeField] private BossSO _requiredBoss;
    private bool _isValidMatch;
    public override string condition1 => _requiredBoss.botInfo.name;

    public BossDefeats_Specific_ProgressTracker() { }

    public BossDefeats_Specific_ProgressTracker(BossSO requiredBoss)
    {
        _requiredBoss = requiredBoss;
    }

    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
    }

    public override void InitTracker()
    {
        if (_requiredBoss.IsClaimed)
            NotifyMissionCompleted();
        else
        {
            GameEventHandler.AddActionEvent(PBPvPEventCode.OnFinalRoundCompleted, OnFinalRoundCompleted);
            GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
        }
    }

    private void OnMatchStarted(object[] parameters)
    {
        _isValidMatch = false;
        if (parameters[0] is not PBPvPMatch matchOfPlayer)
            return;
        if (BossFightManager.Instance.bossMapSO.currentBossSO == _requiredBoss)
            _isValidMatch = true;
    }

    private void OnFinalRoundCompleted(object[] parameters)
    {
        if (parameters[0] is not PBPvPMatch matchOfPlayer || !matchOfPlayer.isAbleToComplete)
            return;
        if (matchOfPlayer.isVictory && matchOfPlayer.mode == Mode.Boss && _isValidMatch)
            NotifyMissionCompleted();
    }

    public override object[] Serialize()
    {
        return new object[] { _requiredBoss.Key };
    }
}

//MissionTargetType.OpenBoxes_CountAny
[Serializable]
public sealed class OpenBoxes_CountAny_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(UnpackEventCode.OnUnpackStart, OnUnpackStart);
    }

    private void OnUnpackStart(object[] parameters)
    {
        if (parameters.Length < 2 || parameters[1] == null || parameters[1] is not List<GachaPack> packs || packs.Count <= 0)
            return;
        NotifyMissionProgressChanged(packs.Count);
    }
}

//MissionTargetType.CardCollection_Any
[Serializable]
public sealed class CardCollection_Any_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartCardChanged, OnPartCardChanged);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartCardChanged, OnPartCardChanged);
    }

    private void OnPartCardChanged(object[] parameters)
    {
        if (parameters.Length < 4 || parameters[1] == null || parameters[1] is not PBPartSO partSO ||
            !(partSO.PartType == PBPartType.Body || partSO.PartType == PBPartType.Front || partSO.PartType == PBPartType.Upper) ||
            parameters[3] == null || parameters[3] is not int changedAmount || changedAmount <= 0)
            return;
        NotifyMissionProgressChanged(changedAmount);
    }
}

//MissionTargetType.CardCollection_Specific
[Serializable]
public sealed class CardCollection_Specific_ProgressTracker : RequirePart_MissionProgressTracker
{
    public override string condition1 => _requiredPartSO.GetDisplayName();

    public CardCollection_Specific_ProgressTracker() { }

    public CardCollection_Specific_ProgressTracker(PBPartSO requiredPartSO)
    {
        _requiredPartSO = requiredPartSO;
    }

    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUnlocked, OnPartUnlocked);
    }

    public override void InitTracker()
    {
        if (_requiredPartSO.IsUnlocked())
            NotifyMissionCompleted();
        else
            GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUnlocked, OnPartUnlocked);
    }

    public override object[] Serialize()
    {
        return new object[] { _requiredPartSO.guid };
    }

    private void OnPartUnlocked(object[] parameters)
    {
        if (_requiredPartSO.IsUnlocked())
            NotifyMissionCompleted();
    }
}

//MissionTargetType.ArenaUnlocks_Specific
[Serializable]
public sealed class ArenaUnlocks_Specific_ProgressTracker : MissionProgressTracker
{
    [SerializeField] private PvPArenaSO _arenaSO;
    public override string condition1 => _arenaSO.GetDisplayName();
    public ArenaUnlocks_Specific_ProgressTracker() { }

    public ArenaUnlocks_Specific_ProgressTracker(PvPArenaSO arenaSO)
    {
        _arenaSO = arenaSO;
    }

    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnNewArenaUnlocked, OnNewArenaUnlocked);
    }

    public override void InitTracker()
    {
        if (_arenaSO.IsUnlocked())
            NotifyMissionCompleted();
        else
            GameEventHandler.AddActionEvent(PBPvPEventCode.OnNewArenaUnlocked, OnNewArenaUnlocked);
    }

    public override object[] Serialize()
    {
        return new object[] { _arenaSO.index };
    }

    private void OnNewArenaUnlocked(object[] parameters)
    {
        if (_arenaSO.IsUnlocked())
            NotifyMissionCompleted();
    }
}

//MissionTargetType.BossClaim_Count
[Serializable]
public sealed class BossClaim_Count_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnClaimBossComplete, OnClaimBossComplete);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(BossFightEventCode.OnClaimBossComplete, OnClaimBossComplete);
    }

    private void OnClaimBossComplete()
    {
        NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.BossClaim_Specific
[Serializable]
public sealed class BossClaim_Specific_ProgressTracker : MissionProgressTracker
{
    [SerializeField] private BossSO _requiredBoss;
    public override string condition1 => _requiredBoss.botInfo.name;

    public BossClaim_Specific_ProgressTracker() { }

    public BossClaim_Specific_ProgressTracker(BossSO requiredBoss)
    {
        _requiredBoss = requiredBoss;
    }

    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnClaimBossComplete, OnClaimBossComplete);
    }

    public override void InitTracker()
    {
        if (_requiredBoss.IsClaimed)
            NotifyMissionCompleted();
        else
            GameEventHandler.AddActionEvent(BossFightEventCode.OnClaimBossComplete, OnClaimBossComplete);
    }

    private void OnClaimBossComplete()
    {
        if (_requiredBoss.IsClaimed)
            NotifyMissionCompleted();
    }

    public override object[] Serialize()
    {
        return new object[] { _requiredBoss.Key };
    }
}

//MissionTargetType.SpendCurrency_Count_Coins
[Serializable]
public sealed class SpendCurrency_Count_Coins_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        if (CurrencyManager.Instance)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Standard).onValueChanged -= OnCurrencyChanged;
    }

    public override void InitTracker()
    {
        CurrencyManager.Instance.GetCurrencySO(CurrencyType.Standard).onValueChanged += OnCurrencyChanged;
    }

    private void OnCurrencyChanged(ValueDataChanged<float> valueDataChanged)
    {
        if (valueDataChanged.newValue < valueDataChanged.oldValue)
            NotifyMissionProgressChanged(valueDataChanged.oldValue - valueDataChanged.newValue);
    }
}

//MissionTargetType.SpendCurrency_Count_Gems
[Serializable]
public sealed class SpendCurrency_Count_Gems_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        if (CurrencyManager.Instance)
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.Premium).onValueChanged -= OnCurrencyChanged;
    }

    public override void InitTracker()
    {
        CurrencyManager.Instance.GetCurrencySO(CurrencyType.Premium).onValueChanged += OnCurrencyChanged;
    }

    private void OnCurrencyChanged(ValueDataChanged<float> valueDataChanged)
    {
        if (valueDataChanged.newValue < valueDataChanged.oldValue)
            NotifyMissionProgressChanged(valueDataChanged.oldValue - valueDataChanged.newValue);
    }
}

//MissionTargetType.CardUpgrades_UptoLevel_Specific
[Serializable]
public sealed class CardUpgrades_UptoLevel_Specific_ProgressTracker : RequirePart_MissionProgressTracker
{
    [SerializeField] private int _requiredLevel;
    public override string condition1 => _requiredPartSO.GetDisplayName();
    public override string condition2 => _requiredLevel.ToString();

    public CardUpgrades_UptoLevel_Specific_ProgressTracker() { }

    public CardUpgrades_UptoLevel_Specific_ProgressTracker(PBPartSO requiredPartSO, int requiredLevel)
    {
        _requiredPartSO = requiredPartSO;
        _requiredLevel = requiredLevel;
    }

    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUpgraded, OnPartCardChanged);
    }

    public override void InitTracker()
    {
        if (_requiredPartSO.IsUnlocked() && _requiredPartSO.GetCurrentUpgradeLevel() >= _requiredLevel)
            NotifyMissionCompleted();
        else
            GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUpgraded, OnPartCardChanged);
    }

    public override object[] Serialize()
    {
        return new object[] { _requiredPartSO.guid, _requiredLevel };
    }

    private void OnPartCardChanged()
    {
        if (_requiredPartSO.IsUnlocked() && _requiredPartSO.GetCurrentUpgradeLevel() >= _requiredLevel)
            NotifyMissionCompleted();
    }
}

//MissionTargetType.PowerupsCollection_Count_Any
[Serializable]
public sealed class PowerupsCollection_Count_Any_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(MissionEventCode.OnPlayerPickedUpCollectable, OnPlayerPickedUpCollectable);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(MissionEventCode.OnPlayerPickedUpCollectable, OnPlayerPickedUpCollectable);
    }

    private void OnPlayerPickedUpCollectable()
    {
        NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.OfferClaims_Count_HotOffers
[Serializable]
public sealed class OfferClaims_Count_HotOffers_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(MissionEventCode.OnHotOfferClaimed, OnHotOfferClaimed);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(MissionEventCode.OnHotOfferClaimed, OnHotOfferClaimed);
    }

    private void OnHotOfferClaimed()
    {
        NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.Purchases_Count_Any
[Serializable]
public sealed class Purchases_Count_Any_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseIAPItemCompleted);
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseIAPItemCompleted);
    }

    private void OnPurchaseIAPItemCompleted()
    {
        NotifyMissionProgressChanged(1);
    }
}

//MissionTargetType.WatchRVs_Count_Any
[Serializable]
public sealed class WatchRVs_Count_Any_ProgressTracker : MissionProgressTracker
{
    public override void ClearTracker()
    {
        GameEventHandler.RemoveActionEvent(AdvertisingEventCode.OnCloseAd, OnCloseAd);
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.GetCurrencySO(CurrencyType.RVTicket).onValueChanged -= OnSpendRVTicket;
        }
    }

    public override void InitTracker()
    {
        GameEventHandler.AddActionEvent(AdvertisingEventCode.OnCloseAd, OnCloseAd);
        CurrencyManager.Instance.GetCurrencySO(CurrencyType.RVTicket).onValueChanged += OnSpendRVTicket;
    }

    private void OnSpendRVTicket(ValueDataChanged<float> data)
    {
        if (data.newValue < data.oldValue)
        {
            NotifyMissionProgressChanged(data.oldValue - data.newValue);
        }
    }

    private void OnCloseAd(object[] _params)
    {
        var adsType = (AdsType)_params[0];
        var location = (AdsLocation)_params[1];
        var isSuccess = (bool)_params[2];
        if (adsType == AdsType.Rewarded && isSuccess)
        {
            NotifyMissionProgressChanged(1);
        }
    }
}
