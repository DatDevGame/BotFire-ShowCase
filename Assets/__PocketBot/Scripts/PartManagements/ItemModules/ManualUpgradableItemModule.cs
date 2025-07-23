using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomInspectorName("ManualUpgradableItemModule")]
public class ManualUpgradableItemModule : UpgradableItemModule
{
    #region Constructors
    public ManualUpgradableItemModule(int upgradeLevel)
    {
        m_UpgradeLevel = upgradeLevel;
    }
    #endregion

    [SerializeField]
    protected int m_UpgradeLevel;

    public override int upgradeLevel
    {
        get => m_UpgradeLevel;
        protected set => m_UpgradeLevel = value;
    }

    public override List<Requirement> GetUpgradeRequirementsOfLevel(int upgradeLevel)
    {
        return new List<Requirement>() { new Requirement_Empty() };
    }
}