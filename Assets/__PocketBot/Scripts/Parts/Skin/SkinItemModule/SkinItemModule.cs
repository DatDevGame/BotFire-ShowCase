using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class AbstractSkinItemModule : ItemModule
{
    public abstract SkinSO currentSkin { get; set; }
}
[Serializable, CustomInspectorName("SkinModule")]
public class SkinItemModule : AbstractSkinItemModule
{
    public event Action<SkinItemModule> onSkinChanged;

    [SerializeField, ListDrawerSettings(CustomAddFunction = "OnAddNewSkin", CustomRemoveElementFunction = "CustomRemoveElementFunction")]
    private List<SkinSO> m_Skins = new List<SkinSO>();

    public List<SkinSO> skins => m_Skins;

    public SkinSO defaultSkin
    {
        get
        {
            return m_Skins[0];
        }
    }

    public override SkinSO currentSkin
    {
        get
        {
            var skinId = PlayerPrefs.GetString($"CurrentSkin_{m_ItemSO.guid}", defaultSkin.guid);
            foreach (var skin in m_Skins)
            {
                if (skinId == skin.guid)
                    return skin;
            }
            return defaultSkin;
        }
        set
        {
            var prevSkin = currentSkin;
            var currSkin = value;
            if (itemSO.Cast<PBPartSO>().PartType == PBPartType.Body)
            {
                var index = skins.IndexOf(value);
                foreach (var wheel in itemSO.Cast<PBChassisSO>().AttachedWheels)
                {
                    if (wheel.TryGetModule(out SkinItemModule skinItemModule))
                    {
                        skinItemModule.currentSkin = skinItemModule.skins[index];
                    }
                }
            }
            PlayerPrefs.SetString($"CurrentSkin_{m_ItemSO.guid}", currSkin.guid);
            if (currSkin != prevSkin)
                onSkinChanged?.Invoke(this);
        }
    }
}
public class ManualSkinItemModule : AbstractSkinItemModule
{
    public ManualSkinItemModule(SkinSO currentSkin)
    {
        m_CurrentSkin = currentSkin;
    }

    [SerializeField]
    private SkinSO m_CurrentSkin;
    private int m_CurrentSkinIndex;

    public override SkinSO currentSkin
    {
        get => m_CurrentSkin;
        set => m_CurrentSkin = value;
    }

    public int currentSkinIndex
    {
        get => m_CurrentSkinIndex;
        set => m_CurrentSkinIndex = value;
    }
}