using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[EventCode]
public enum ActiveSkillManagementEventCode
{
    /// <summary>
    /// This event is raised when ActiveSkill in-use is changed
    /// <para> <typeparamref name="ActiveSkillManagerSO"/>: ActiveSkillManagerSO </para>
    /// <para> <typeparamref name="ActiveSkillSO"/>: ActiveSkillInUse </para>
    /// </summary>
    OnSkillUsed,
    /// <summary>
    /// This event is raised when ActiveSkill is selected
    /// <para> <typeparamref name="ActiveSkillManagerSO"/>: ActiveSkillManagerSO </para>
    /// <para> <typeparamref name="ActiveSkillSO"/>: currentSelectedActiveSkill </para>
    /// <para> <typeparamref name="ActiveSkillSO"/>: previousSelectedActiveSkill </para>
    /// </summary>
    OnSkillSelected,
    /// <summary>
    /// This event is raised when ActiveSkill is unlocked
    /// <para> <typeparamref name="ActiveSkillManagerSO"/>: ActiveSkillManagerSO </para>
    /// <para> <typeparamref name="ActiveSkillSO"/>: unlockedActiveSkill </para>
    /// </summary>
    OnSkillUnlocked,
    /// <summary>
    /// This event is raised when ActiveSkill card is changed
    /// <para> <typeparamref name="ActiveSkillManagerSO"/>: ActiveSkillManagerSO </para>
    /// <para> <typeparamref name="ActiveSkillSO"/>: ActiveSkillSO </para>
    /// <para> <typeparamref name="int"/>: numOfCards </para>
    /// <para> <typeparamref name="int"/>: changedAmount </para>
    /// </summary>
    OnSkillCardChanged,
}
[CreateAssetMenu(fileName = "ActiveSkillManagerSO", menuName = "PocketBots/ActiveSkillManagerSO")]
public class ActiveSkillManagerSO : GachaItemManagerSO<ActiveSkillSO>
{
    protected override void OnItemCardChanged(CardItemModule cardModule, int changedAmount)
    {
        base.OnItemCardChanged(cardModule, changedAmount);
        if (m_CurrentItemInUse.value == cardModule.itemSO && cardModule.numOfCards <= 0)
        {
            m_CurrentItemInUse.Clear();
        }
    }

    public virtual ActiveSkillSO GetSkill(string name)
    {
        var skill = initialValue.Find(x => x?.GetDisplayName() == name);
        return skill as ActiveSkillSO;
    }

}