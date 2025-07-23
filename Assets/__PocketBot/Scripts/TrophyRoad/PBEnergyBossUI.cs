using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyrphusQ.Events;
using HightLightDebug;
using Sirenix.OdinInspector;

public class PBEnergyBossUI : Singleton<PBEnergyBossUI>
{
    [SerializeField, BoxGroup("Property")] private int consumeEnergy = 1;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
    }

    private void OnMatchStarted(params object[] parrams)
    {
        PBChassisSO pbChassisSOCurrent = GearSaver.Instance.chassisSO.value.Cast<PBChassisSO>();
        if (pbChassisSOCurrent != null)
        {
            if (!pbChassisSOCurrent.IsSpecial) return;
            if (pbChassisSOCurrent.TryGetModule<EnergyItemModule>(out var energyItemModule))
            {
                if (!energyItemModule.IsOutOfEnergy())
                {
                    energyItemModule.ConsumeEnergy(consumeEnergy);
                    DebugPro.CyanBold($"({pbChassisSOCurrent.GetDisplayName()}) - Energy Consume: {consumeEnergy}");
                }
            }
            else
                DebugPro.ErrorRed($"({pbChassisSOCurrent.GetDisplayName()}) - Not Energy Module");
        }
    }
}
