using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PBGachaUpgradeRequirementData : GachaUpgradeRequirementData
{
    [SerializeField]
    protected List<float> m_RequiredAmountOfCurrencyLevels = new List<float>();

    public List<int> requiredNumOfCardsLevels
    {
        get
        {
            return m_RequiredNumOfCardsLevels;
        }
        set
        {
            m_RequiredNumOfCardsLevels = value;
        }
    }

    public List<float> requiredAmountOfCurrencyLevels
    {
        get
        {
            return m_RequiredAmountOfCurrencyLevels;
        }
        set
        {
            m_RequiredAmountOfCurrencyLevels = value;
        }
    }

    public virtual CurrencyType GetCurrencyType() => CurrencyType.Standard;

    public virtual float GetRequiredAmountOfCurrencyCount()
    {
        return m_RequiredAmountOfCurrencyLevels.Count;
    }

    public virtual float GetRequiredAmountOfCurrency(int upgradeLevel)
    {
        return m_RequiredAmountOfCurrencyLevels[Mathf.Min(upgradeLevel - 1, m_RequiredNumOfCardsLevels.Count - 1)];
    }
}
[CustomInspectorName("PBUpgradableItemModule")]
public class PBUpgradableItemModule : GachaUpgradableItemModule<PBGachaUpgradeRequirementData>, IResourceLocationProvider
{
    public string GetItemId()
    {
        return $"{itemSO.GetModule<NameItemModule>().displayName}";
    }

    public ResourceLocation GetLocation()
    {
        return ResourceLocation.UpgradePart;
    }

    public PBGachaUpgradeRequirementData GetUpgradeRequirementData()
    {
        return m_UpgradeRequirementData;
    }

    public override List<Requirement> GetUpgradeRequirementsOfLevel(int upgradeLevel)
    {
        var requirements = base.GetUpgradeRequirementsOfLevel(upgradeLevel);
        requirements.Add(new Requirement_Currency()
        {
            requiredAmountOfCurrency = m_UpgradeRequirementData.GetRequiredAmountOfCurrency(upgradeLevel),
            currencyType = m_UpgradeRequirementData.GetCurrencyType(),
            resourceLocationProvider = this,
        });
        return requirements;
    }
}