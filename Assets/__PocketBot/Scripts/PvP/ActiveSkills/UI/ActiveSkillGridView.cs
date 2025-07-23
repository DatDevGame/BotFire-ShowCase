using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using HyrphusQ.GUI;
using UnityEngine;

public class ActiveSkillGridView : ItemListView
{
    [SerializeField]
    private TextAdapter m_UnlockAtText;
    [SerializeField]
    private IntVariable m_RequiredTrophiesToUnlockActiveSkillVar;
    [SerializeField]
    private HighestAchievedPPrefFloatTracker m_HighestAchievedTrophyVar;
    [SerializeField]
    private GameObject m_LockPanelGO;

    private ActiveSkillInfoPopup m_SkillInfoPopup;

    private ActiveSkillInfoPopup skillInfoPopup
    {
        get
        {
            if (m_SkillInfoPopup == null)
            {
                m_SkillInfoPopup = FindObjectOfType<ActiveSkillInfoPopup>(true);
            }
            return m_SkillInfoPopup;
        }
    }

    private void Awake()
    {
        GenerateView(false);
        ObjectFindCache<ActiveSkillGridView>.Add(this);
    }

    protected override void Start()
    {
        base.Start();
        GameEventHandler.AddActionEvent(ActiveSkillManagementEventCode.OnSkillUsed, UpdateView);
        GameEventHandler.AddActionEvent(ActiveSkillManagementEventCode.OnSkillCardChanged, UpdateView);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnTabButtonChange, HandleOnTabButtonChange);
        m_UnlockAtText.SetText(m_UnlockAtText.blueprintText.Replace(Const.StringValue.PlaceholderValue, m_RequiredTrophiesToUnlockActiveSkillVar.value.ToString()));
        m_LockPanelGO.SetActive(m_HighestAchievedTrophyVar < m_RequiredTrophiesToUnlockActiveSkillVar);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameEventHandler.RemoveActionEvent(ActiveSkillManagementEventCode.OnSkillUsed, UpdateView);
        GameEventHandler.RemoveActionEvent(ActiveSkillManagementEventCode.OnSkillCardChanged, UpdateView);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnTabButtonChange, HandleOnTabButtonChange);
        ObjectFindCache<ActiveSkillGridView>.Remove(this);
    }

    private void HandleOnTabButtonChange(object[] parameters)
    {
        if (parameters[0] is GearTabButton gearTabButton && gearTabButton.ItemManagerSO is ActiveSkillManagerSO)
        {
            ShowImmediately();
        }
        else
        {
            HideImmediately();
        }
    }

    protected override void SelectItem(ItemCell itemCell, bool isForceSelect = false)
    {
        if (!isForceSelect)
            skillInfoPopup.ShowDetails(itemCell.item.Cast<ActiveSkillSO>());
        base.SelectItem(itemCell, isForceSelect);
        m_CurrentSelectedCell = null;
    }

    public void DisableAllButtons(params ItemCell[] excludedCells)
    {
        List<ItemCell> includedCells = itemCells.Except(excludedCells).ToList();
        foreach (ActiveSkillCardUI itemCell in includedCells)
        {
            itemCell.button.enabled = false;
        }
    }

    public void EnableAllButtons()
    {
        foreach (ActiveSkillCardUI itemCell in itemCells)
        {
            itemCell.button.enabled = true;
        }
    }
}