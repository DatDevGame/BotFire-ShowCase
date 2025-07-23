using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using HyrphusQ.Events;
using System.Linq;
using HyrphusQ.Helpers;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "PartManagerSO", menuName = "PocketBots/PartManagement/PartManagerSO")]
public class PBPartManagerSO : GachaItemManagerSO<PBPartSO>
{
    #region Fields
    // Event Code Fields
    [SerializeField, BoxGroup("Event Code")]
    protected EventCode m_ItemSkinChangedEventCode;
    // Other Fields
    [SerializeField, BoxGroup("Info")]
    protected PBPartType m_PartType;
    #endregion

    #region Properties
    // Event Code Props
    /// <summary>
    /// Event is raised when skin of item is changed
    /// <para> <typeparamref name="PBPartManagerSO"/>: partManagerSO </para>
    /// <para> <typeparamref name="PBPartSO"/>: partSO </para>
    /// <para> <typeparamref name="Skin"/>: skin </para>
    /// </summary>
    public virtual EventCode itemSkinChangedEventCode => m_ItemSkinChangedEventCode;
    // Other Props
    public virtual PBPartType PartType => m_PartType;
    public virtual PBPartSO CurrentPartInUse => currentGenericItemInUse;
    public virtual List<PBPartSO> Parts => genericItems;
    #endregion

    #region Private & Protected Methods
    protected override void OnEnable()
    {
        base.OnEnable();
        foreach (var item in items)
        {
            if (item.TryGetModule(out SkinItemModule skinModule))
            {
                skinModule.onSkinChanged += OnItemSkinChanged;
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var item in items)
        {
            if (item.TryGetModule(out SkinItemModule skinModule))
            {
                skinModule.onSkinChanged -= OnItemSkinChanged;
            }
        }
    }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        foreach (var item in genericItems)
        {
            if (item.ManagerSO != this)
            {
                item.SetFieldValue("partManagerSO", this);
                EditorUtility.SetDirty(item);
            }
        }
    }
#endif

    // Notify Events
    protected virtual void NotifyEventItemSkinChanged(params object[] eventData)
    {
        if (m_ItemSkinChangedEventCode == null || m_ItemSkinChangedEventCode.eventCode == null)
            return;
        GameEventHandler.Invoke(m_ItemSkinChangedEventCode, eventData);
    }

    // Listen Events
    protected virtual void OnItemSkinChanged(SkinItemModule skinModule)
    {
        // Notify event
        NotifyEventItemSkinChanged(this, skinModule.itemSO, skinModule.currentSkin);
    }
    #endregion

    #region Public Methods
    public List<PBPartSO> FindPartsByRarity(RarityType rarity)
    {
        return Parts.Where(part => part.GetRarityType() == rarity).ToList();
    }

    public PBPartSO FindPartsByInternalName(string internalName)
    {
        return Parts.Find(part => part.GetInternalName() == internalName);
    }

    public PBPartSO GetRandomPart()
    {
        return items[Random.Range(0, itemCount)].Cast<PBPartSO>();
    }

    public PBPartSO FindHighestStatsScorePart()
    {
        var highestStatsScorePart = Parts[0];
        foreach (var part in Parts)
        {
            if (!part.IsOwned())
                continue;
            var statsScore_1 = part.CalCurrentStatsScore();
            var statsScore_2 = highestStatsScorePart.CalCurrentStatsScore();
            if (statsScore_1 > statsScore_2)
                highestStatsScorePart = part;
        }
        return highestStatsScorePart;
    }

    public PBPartSO FindHighestStatsScorePart(PBChassisSO chassisSO)
    {
        var highestStatsScorePart = Parts[0];
        foreach (var part in Parts)
        {
            if (!part.IsOwned())
                continue;
            var statsScore_1 = RobotStatsCalculator.CalCombinationStatsScore(false, chassisSO, part);
            var statsScore_2 = RobotStatsCalculator.CalCombinationStatsScore(false, chassisSO, highestStatsScorePart);
            if (statsScore_1 > statsScore_2)
                highestStatsScorePart = part;
        }
        return highestStatsScorePart;
    }

    public PBPartSO FindPotentialHighestStatsScorePart()
    {
        var highestStatsScorePart = Parts[0];
        foreach (var part in Parts)
        {
            if (!part.IsOwned())
                continue;
            var maxReachableUpgradeLevel_1 = part.CalMaxReachableUpgradeLevel();
            var statsScore_1 = part.CalStatsScoreByLevel(maxReachableUpgradeLevel_1);
            var maxReachableUpgradeLevel_2 = highestStatsScorePart.CalMaxReachableUpgradeLevel();
            var statsScore_2 = highestStatsScorePart.CalStatsScoreByLevel(maxReachableUpgradeLevel_2);
            if (statsScore_1 > statsScore_2)
                highestStatsScorePart = part;
        }
        return highestStatsScorePart;
    }

    public bool HasOnePartIsNew()
    {
        bool value = false;
        foreach (var part in Parts)
        {
            if (part.TryGetModule(out NewItemModule itemModule))
            {
                if (itemModule.isNew)
                {
                    value = true;
                    break;
                }
            }
        }
        return value;
    }

    public bool HasOnePartIsUpgradable()
    {
        bool value = false;
        foreach (var part in Parts)
        {
            if (part.IsEnoughCardToUpgrade())
            {
                value = true;
                break;
            }
        }
        return value;
    }
    #endregion
}