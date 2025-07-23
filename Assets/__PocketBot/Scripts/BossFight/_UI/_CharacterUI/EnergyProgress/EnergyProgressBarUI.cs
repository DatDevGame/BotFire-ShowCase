using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using HightLightDebug;

public class EnergyProgressBarUI : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private ParticleSystem electricEffect;
    [SerializeField, BoxGroup("Ref")] private List<NodeEnergy> nodeEnergies;
    [SerializeField, BoxGroup("Ref")] private GameObject energyDepletedIcon;
    [SerializeField, BoxGroup("Color")] private Color lowBatteryColor;
    [SerializeField, BoxGroup("Color")] private Color mediumBatteryColor;
    [SerializeField, BoxGroup("Color")] private Color highBatteryColor;
    [SerializeField, BoxGroup("Color")] private Color lowPowerNodeColor;
    [SerializeField, BoxGroup("Color")] private Color HighPowerNodeColor;

    public List<NodeEnergy> NodeEnergies => nodeEnergies;

    public void Setup(PBChassisSO pbChassisSO, bool isCharging)
    {
        if (!pbChassisSO.IsSpecial) return;
        
        if (pbChassisSO.TryGetModule<EnergyItemModule>(out var energyItemModule))
        {
            UpdateEnergy(energyItemModule.currentEnergy, isCharging);
        }
    }

    private void UpdateEnergy(int energyCount, bool isCharging)
    {
        ResetAllEnergy();
        nodeEnergies.Take(energyCount).ToList().ForEach(v => v.OnEnergy());
        HandleColorEnergy(energyCount, isCharging);
    }

    private void ResetAllEnergy()
    {
        nodeEnergies.ForEach(v => v.OffEnergy());
    }

    private void HandleColorEnergy(int energyCount, bool isCharging)
    {
        Color energyNodeColor = Color.white;
        //if (isCharging)
        //{
        //    energyNodeColor = energyCount switch
        //    {
        //        _ when energyCount <= 4 => lowBatteryColor,
        //        _ => highBatteryColor
        //    };
        //}
        //else
        //{
        //    energyNodeColor = energyCount switch
        //    {
        //        _ when energyCount <= 1 => lowBatteryColor,
        //        _ when energyCount > 1 && energyCount <= 3 => mediumBatteryColor,
        //        _ when energyCount > 3 => highBatteryColor,
        //        _ => highBatteryColor
        //    };
        //}
        energyNodeColor = energyCount switch
        {
            _ when energyCount <= 1 => lowBatteryColor,
            _ when energyCount > 1 && energyCount <= 3 => mediumBatteryColor,
            _ when energyCount > 3 => highBatteryColor,
            _ => highBatteryColor
        };


        energyDepletedIcon.gameObject.SetActive(energyCount <= 0);
        nodeEnergies.ForEach((v) =>
        {
            v.SetColor(energyNodeColor);
            if (energyCount <= 0)
                v.GetComponent<Image>().color = lowPowerNodeColor;
            else
                v.GetComponent<Image>().color = HighPowerNodeColor;
        });

        if(energyCount > 0)
            nodeEnergies[energyCount == 0 ? 0 : energyCount - 1].OnAnimation();
    }

    public void OnAnimationFullEnergy() => electricEffect.Play();
}
