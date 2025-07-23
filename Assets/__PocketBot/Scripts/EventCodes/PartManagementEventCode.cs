using HyrphusQ.Events;

[EventCode]
public enum PartManagementEventCode
{
    /// <summary>
    /// This event is raised when Part in-use is changed
    /// <para> <typeparamref name="PartManagerSO"/>: PartManagerSO </para>
    /// <para> <typeparamref name="PartSO"/>: PartInUse </para>
    /// </summary>
    OnPartUsed,
    /// <summary>
    /// This event is raised when Part is selected
    /// <para> <typeparamref name="PartManagerSO"/>: PartManagerSO </para>
    /// <para> <typeparamref name="PartSO"/>: currentSelectedPart </para>
    /// <para> <typeparamref name="PartSO"/>: previousSelectedPart </para>
    /// </summary>
    OnPartSelected,
    /// <summary>
    /// This event is raised when Part is unlocked
    /// <para> <typeparamref name="PartManagerSO"/>: PartManagerSO </para>
    /// <para> <typeparamref name="PartSO"/>: unlockedPart </para>
    /// </summary>
    OnPartUnlocked,
    /// <summary>
    /// This event is raised when Part card is changed
    /// <para> <typeparamref name="PartManagerSO"/>: PartManagerSO </para>
    /// <para> <typeparamref name="PartSO"/>: PartSO </para>
    /// <para> <typeparamref name="int"/>: numOfCards </para>
    /// <para> <typeparamref name="int"/>: changedAmount </para>
    /// </summary>
    OnPartCardChanged,
    /// <summary>
    /// This event is raised when Part is upgraded
    /// <para> <typeparamref name="PartManagerSO"/>: PartManagerSO </para>
    /// <para> <typeparamref name="PartSO"/>: PartSO </para>
    /// <para> <typeparamref name="int"/>: upgradeLevel </para>
    /// </summary>
    OnPartUpgraded,
    /// <summary>
    /// This event is raised when new state of Part is changed
    /// <para> <typeparamref name="PartManagerSO"/>: PartManagerSO </para>
    /// <para> <typeparamref name="PartSO"/>: PartSO </para>
    /// <para> <typeparamref name="bool"/>: isNew </para>
    /// </summary>
    OnPartNewStateChanged,
    /// <summary>
    /// This event is raised when you change Color
    /// <para> <typeparamref name="Color"/>: Color </para>
    /// </summary>
    OnColorChanged,
    /// <summary>
    /// This event is raised when any skin of a part changed
    /// <para> <typeparamref name="PartManagerSO"/>: PartManagerSO </para>
    /// <para> <typeparamref name="PartSO"/>: partSO </para>
    /// <para> <typeparamref name="Skin"/>: SkinSO </para>
    /// </summary>
    OnSkinChanged,
    /// <summary>
    /// This event is raised when any skin of a part previewed
    /// <para> <typeparamref name="PartManagerSO"/>: PartManagerSO </para>
    /// <para> <typeparamref name="PartSO"/>: partSO </para>
    /// <para> <typeparamref name="Skin"/>: SkinSO </para>
    /// </summary>
    OnSkinPreviewed
}
