using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[EventCode]
public enum AvatarFrameEventCode
{
    /// <summary>
    /// This event is raised when avatar in-use is changed
    /// <para> <typeparamref name="AvatarManagerSO"/>: avatarManagerSO </para>
    /// <para> <typeparamref name="AvatarItemSO"/>: itemInUse </para>
    /// </summary>
    OnAvatarUsed,
    /// <summary>
    /// This event is raised when avatar is selected
    /// <para> <typeparamref name="AvatarManagerSO"/>: avatarManagerSO </para>
    /// <para> <typeparamref name="AvatarItemSO"/>: currentSelectedItem </para>
    /// <para> <typeparamref name="AvatarItemSO"/>: previousSelectedItem </para>
    /// </summary>
    OnAvatarSelected,
    /// <summary>
    /// This event is raised when avatar is unlocked
    /// <para> <typeparamref name="AvatarManagerSO"/>: avatarManagerSO </para>
    /// <para> <typeparamref name="AvatarItemSO"/>: unlockedItem </para>
    /// </summary>
    OnAvatarUnlocked,

    /// <summary>
    /// This event is raised when frame in-use is changed
    /// <para> <typeparamref name="AvatarManagerSO"/>: frameManagerSO </para>
    /// <para> <typeparamref name="AvatarItemSO"/>: itemInUse </para>
    /// </summary>
    OnFrameUsed,
    /// <summary>
    /// This event is raised when frame is selected
    /// <para> <typeparamref name="AvatarManagerSO"/>: frameManagerSO </para>
    /// <para> <typeparamref name="AvatarItemSO"/>: currentSelectedItem </para>
    /// <para> <typeparamref name="AvatarItemSO"/>: previousSelectedItem </para>
    /// </summary>
    OnFrameSelected,
    /// <summary>
    /// This event is raised when frame is unlocked
    /// <para> <typeparamref name="AvatarManagerSO"/>: frameManagerSO </para>
    /// <para> <typeparamref name="AvatarItemSO"/>: unlockedItem </para>
    /// </summary>
    OnFrameUnlocked
}