using System;
using Sirenix.OdinInspector;
using UnityEngine;

[WindowMenuItem("ActiveSkillSO", assetFolderPath: "Assets/__PocketBot/PvP/ActiveSkills/ScriptableObjects", mode: WindowMenuItemAttribute.Mode.Multiple, sortByName: true)]
public abstract class ActiveSkillSO : GachaItemSO
{
    [SerializeField]
    protected float m_Cooldown = 2f;
    [SerializeField]
    protected CombatEffectStatuses m_SkillBlockingEffectStatuses = CombatEffectStatuses.Immobilized | CombatEffectStatuses.Stunned;

    public virtual float activeDuration => Const.FloatValue.OneF;
    public virtual float cooldown => m_Cooldown;
    public virtual CombatEffectStatuses skillBlockingEffectStatuses => m_SkillBlockingEffectStatuses;

    #region Editor Methods
#if UNITY_EDITOR
    [ButtonGroup, Button(SdfIconType.Plus, "Add 1 Card"), PropertyOrder(-1)]
    private void Add1Card()
    {
        this.UpdateNumOfCards(this.GetNumOfCards() + 1);
    }
    [ButtonGroup, Button(SdfIconType.Plus, "Add 10 Card"), PropertyOrder(-1)]
    private void Add10Card()
    {
        this.UpdateNumOfCards(this.GetNumOfCards() + 10);
    }
    [ButtonGroup, Button(SdfIconType.Plus, "Add 100 Card"), PropertyOrder(-1)]
    private void Add100Card()
    {
        this.UpdateNumOfCards(this.GetNumOfCards() + 100);
    }
    [ButtonGroup, Button(SdfIconType.Eraser, "Reset Card"), PropertyOrder(-1)]
    private void ResetCard()
    {
        this.UpdateNumOfCards(0);
    }
#endif
    #endregion

    public abstract ActiveSkillCaster SetupSkill(PBRobot robot);
}
public abstract class ActiveSkillSO<TData, TCaster> : ActiveSkillSO where TData : ActiveSkillSO where TCaster : ActiveSkillCaster<TData>
{
    public override ActiveSkillCaster SetupSkill(PBRobot robot)
    {
        TCaster activeSkillCaster = robot.gameObject.AddComponent<TCaster>();
        activeSkillCaster.Initialize(this as TData, robot);
        return activeSkillCaster;
    }
}