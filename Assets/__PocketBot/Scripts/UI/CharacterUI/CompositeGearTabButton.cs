using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using UnityEngine;

public class CompositeGearTabButton : GearTabButton
{
    [SerializeField, BoxGroup("Ref")] protected List<GearTabButton> subTabButtons;
    private GearTabButton selectedSubTabButton;

    public override PBPartType PartType => GetSelectedSubTabButton().PartType;
    public override PBPartSlot PartSlot => GetSelectedSubTabButton().PartSlot;
    public override ItemManagerSO ItemManagerSO => GetSelectedSubTabButton().ItemManagerSO;
    public List<GearTabButton> SubTabButtons => subTabButtons;

    protected override void Start()
    {
        base.Start();
        foreach (var subTabButton in subTabButtons)
        {
            subTabButton.Button.onClick.AddListener(() =>
            {
                if (selectedSubTabButton == subTabButton)
                    return;

                selectedSubTabButton?.Deselect();
                selectedSubTabButton = subTabButton;
                selectedSubTabButton.Select();
                GameEventHandler.Invoke(CharacterUIEventCode.OnTabButtonChange, selectedSubTabButton);
            });
        }
    }

    public override bool HasAnyNewItem()
    {
        return SubTabButtons.Any(tabButton => tabButton.HasAnyNewItem());
    }

    public override bool HasAnyUpgradableItem()
    {
        return SubTabButtons.Any(tabButton => tabButton.HasAnyUpgradableItem());
    }

    public override void Select()
    {
        base.Select();
        GetSelectedSubTabButton().Select();
    }

    public override void Deselect()
    {
        base.Deselect();
        GetSelectedSubTabButton().Deselect();
    }

    public virtual GearTabButton GetDefaultSelectedSubTabButton()
    {
        return subTabButtons[0];
    }

    public virtual GearTabButton GetSelectedSubTabButton()
    {
        if (selectedSubTabButton == null)
        {
            selectedSubTabButton = GetDefaultSelectedSubTabButton();
        }
        return selectedSubTabButton;
    }

    public virtual void SetSelectedSubTabButton(GearTabButton tabButton)
    {
        if (tabButton == this)
        {
            throw new StackOverflowException("Will cause stackoverflow");
        }
        selectedSubTabButton?.Deselect();
        selectedSubTabButton = tabButton;
    }
}