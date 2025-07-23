using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using UnityEngine;

public class MissionHelper : Singleton<MissionHelper>
{
    [SerializeField] PPrefItemSOVariable chassisInUse;
    [SerializeField] PPrefItemSOVariable upperInUse_1;
    [SerializeField] PPrefItemSOVariable upperInUse_2;
    [SerializeField] PPrefItemSOVariable frontInUse_1;
    [SerializeField] MissionGeneratorConfigSO missionGeneratorConfigSO;

    public string GetHelperBtnText(MissionData missionData)
    {
        var subCategories = GetMissionSubCategory(missionData);
        if (subCategories == MissionSubCategory.WeaponMastery)
        {
            return I2LHelper.TranslateTerm(I2LTerm.ButtonTitle_Equip);

        }
        else if (subCategories == MissionSubCategory.CardUpgrades)
        {
            return I2LHelper.TranslateTerm(I2LTerm.ButtonTitle_Upgrade);
        }
        return I2LHelper.TranslateTerm(I2LTerm.ButtonTitle_Equip);
    }

    public bool IsShowHelperBtn(MissionData missionData)
    {
        var subCategories = GetMissionSubCategory(missionData);
        if (subCategories == MissionSubCategory.WeaponMastery)
        {
            var partSO = TryGetPartSO(missionData);
            if (!partSO.IsUnlocked())
            {
                return false;
            }
            if (partSO.PartType == PBPartType.Body)
            {
                return chassisInUse.value != partSO;
            }
            else if (partSO.PartType == PBPartType.Upper)
            {
                return upperInUse_1.value != partSO && upperInUse_2.value != partSO;
            }
            else if (partSO.PartType == PBPartType.Front)
            {
                return frontInUse_1.value != partSO;
            }
            return false;
        }
        else if (subCategories == MissionSubCategory.CardUpgrades)
        {
            var partSO = TryGetPartSO(missionData);
            if (!partSO.IsUnlocked())
            {
                return false;
            }
            return true;
        }
        return false;
    }

    public MissionSubCategory GetMissionSubCategory(MissionData missionData)
    {
        return missionGeneratorConfigSO.TargetToSubCategory(missionData.targetType);
    }

    public PBPartSO TryGetPartSO(MissionData missionData)
    {
        if (missionData.progressTracker is RequirePart_MissionProgressTracker partTracker)
        {
            return partTracker.requiredPartSO;
        }
        return null;
    }
    Coroutine _goToPartInfoUICoroutine;
    public void GoToPartInfoUI(MissionData missionData)
    {
        if (_goToPartInfoUICoroutine != null)
            StopCoroutine(_goToPartInfoUICoroutine);
        var partSO = TryGetPartSO(missionData);
        var gearTabSelection = ObjectFindCache<GearTabSelection>.Get();
        if (partSO.PartType == PBPartType.Upper)
        {
            gearTabSelection.SetDefaultReloadButton(PBPartSlot.Upper_1);
        }
        else if (partSO.PartType == PBPartType.Front)
        {
            gearTabSelection.SetDefaultReloadButton(PBPartSlot.Front_1);
        }
        else if (partSO.PartType == PBPartType.Body)
        {
            gearTabSelection.SetDefaultReloadButton(PBPartSlot.Body);
        }
        else
        {
            gearTabSelection.SetDefaultReloadButton(PBPartSlot.PrebuiltBody);
        }
        GameEventHandler.Invoke(MainSceneEventCode.OnManuallyClickButton, ButtonType.Character);
        _goToPartInfoUICoroutine = StartCoroutine(CommonCoroutine.Delay(0, false, () =>
        {
            GameEventHandler.Invoke(CharacterUIEventCode.ClickGearButtonViaCode, partSO, true);
        }));
    }
}
