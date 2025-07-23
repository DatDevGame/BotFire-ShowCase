using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using UnityEngine;

[CreateAssetMenu(menuName = "PocketBots/Gacha/Card/ActiveSkill")]
public class GachaCard_ActiveSkill : GachaCard_GachaItem
{
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (TryGetModule(out RarityItemModule rarityItemModule))
        {
            RemoveModule(rarityItemModule);
        }
    }
#endif

    public override GachaCard Clone(params object[] _params)
    {
        var instance = Instantiate(this);
        if (_params[0] is ActiveSkillSO activeSkillSO)
        {
            instance.GachaItemSO = activeSkillSO;
        }
        return instance;
    }

    public override string ToString()
    {
        return $"ActiveSkillCard_{GachaItemSO?.GetInternalName() ?? "Null"}";
    }
}