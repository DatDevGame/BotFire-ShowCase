using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PSFTUEConfig", menuName = "PocketBots/FTUE/PSFTUEConfig")]
public class PSFTUESO : SerializableScriptableObject
{
    [BoxGroup("Enemy")] public float EnemyHealth;
    [BoxGroup("Data")] public PBPartSO WeaponUpgradeFTUEPPref;
    [BoxGroup("Data")] public PBPartSO WeaponNewFTUEPPref;
    [BoxGroup("Data")] public PPrefBoolVariable FTUEFightPPref;
    [BoxGroup("Data")] public PPrefBoolVariable FTUEStartFirstMatch;
    [BoxGroup("Data")] public PPrefBoolVariable FTUEInFirstMatch;
    [BoxGroup("Data")] public PPrefBoolVariable FTUEOpenBuildUI;
    [BoxGroup("Data")] public PPrefBoolVariable FTUEOpenInfoPopup;
    [BoxGroup("Data")] public PPrefBoolVariable FTUEStart2ndMatch;
    [BoxGroup("Data")] public PPrefBoolVariable FTUENewWeapon;

    [BoxGroup("Weapon FTUE")] public PPrefItemSOVariable CurrentUpper_1;
    [BoxGroup("Weapon FTUE")] public PPrefItemSOVariable CurrentUpper_2;
    [BoxGroup("Data")] public PBPartSO WeaponDefault_1;
    [BoxGroup("Data")] public PBPartSO WeaponDefault_2;

    public void ActiveDefautWeapon()
    {
        CurrentUpper_1.value = WeaponDefault_1;
        CurrentUpper_2.value = WeaponDefault_2;
    }
}