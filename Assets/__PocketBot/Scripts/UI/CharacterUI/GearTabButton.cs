using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GearTabButton : MonoBehaviour
{
    [SerializeField, BoxGroup("Events")] public UnityEvent onSelected;
    [SerializeField, BoxGroup("Events")] public UnityEvent onDeselected;

    [SerializeField, BoxGroup("Ref")] protected Button button;
    [SerializeField, BoxGroup("Ref")] protected Image bgImage;
    [SerializeField, BoxGroup("Ref")] protected Sprite selectedSprite, deselectedSprite;
    [SerializeField, BoxGroup("Ref")] protected GameObject newTag;
    [SerializeField, BoxGroup("Ref")] protected GameObject upgradableTag;
    [SerializeField, BoxGroup("Ref"), ShowIf("@GetType() == typeof(GearTabButton)")] protected ItemManagerSO itemManagerSO;

    [SerializeField, BoxGroup("FTUE")] protected PPrefBoolVariable equipFrontFTUE;
    [SerializeField, BoxGroup("FTUE")] protected PPrefBoolVariable upgradeFTUE;
    [SerializeField, BoxGroup("FTUE")] protected GameObject FtueHand;

    [SerializeField, BoxGroup("Data")] private PSFTUESO m_PSFTUESO;

    protected bool hasNewTag;
    protected bool hasUpgradeTag;

    public virtual PBPartType PartType => PartSlot.GetPartTypeOfPartSlot();
    public virtual PBPartSlot PartSlot
    {
        get
        {
            if (itemManagerSO is PBPartManagerSO partManagerSO)
            {
                switch (partManagerSO.PartType)
                {
                    case PBPartType.Body:
                        if (partManagerSO.value[0].Cast<PBChassisSO>().IsSpecial)
                            return PBPartSlot.PrebuiltBody;
                        else
                            return PBPartSlot.Body;
                    case PBPartType.Front:
                        return PBPartSlot.Front_1;
                    case PBPartType.Upper:
                        return PBPartSlot.Upper_1;
                    case PBPartType.Wheels:
                        return PBPartSlot.Wheels_1;
                    default:
                        return (PBPartSlot)(-1);
                }
            }
            return (PBPartSlot)(-1);
        }
        set
        {
            // DO nothing
        }
    }
    public virtual ItemManagerSO ItemManagerSO => itemManagerSO;
    public virtual Button Button => button;

    protected virtual void Awake()
    {
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartNewStateChanged, UpdateView);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartCardChanged, UpdateView);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUpgraded, UpdateView);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnTabButtonChange, HandleOnCharacterOpen);
    }

    protected virtual void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartNewStateChanged, UpdateView);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartCardChanged, UpdateView);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUpgraded, UpdateView);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnTabButtonChange, HandleOnCharacterOpen);
    }

    protected virtual void Start()
    {
        UpdateView();
    }

    protected virtual void HandleOnCharacterOpen()
    {
        if (!equipFrontFTUE.value)
        {
            if (PartType != HandleFTUECharacterUI.partType_Equip_1)
            {
                button.interactable = false;
                FtueHand.SetActive(false);
            }
        }
        if (!upgradeFTUE.value)
        {
            if (PartType != HandleFTUECharacterUI.partType_Equip_1)
            {
                button.interactable = false;
                FtueHand.SetActive(false);
            }
        }

        if (m_PSFTUESO != null)
        {
            if (!m_PSFTUESO.FTUEOpenInfoPopup.value && PartType == PBPartType.Upper)
            {
                Button.onClick?.Invoke();
            }

            if (m_PSFTUESO.FTUEStart2ndMatch.value && !m_PSFTUESO.FTUENewWeapon.value && PartType == PBPartType.Upper)
            {
                Button.onClick?.Invoke();
            }
        }
    }

    public virtual bool HasAnyNewItem()
    {
        return ItemManagerSO.value.Any(itemSO => itemSO.IsNew());
    }

    public virtual bool HasAnyUpgradableItem()
    {
        return ItemManagerSO.value.Any(itemSO => itemSO.IsEnoughCardToUpgrade());
    }

    public virtual void UpdateView()
    {
        if (HasAnyNewItem())
        {
            newTag.SetActive(true);
            upgradableTag.SetActive(false);
        }
        else
        {
            newTag.SetActive(false);
            upgradableTag.SetActive(HasAnyUpgradableItem());
        }
    }

    [Button]
    public virtual void Select()
    {
        bgImage.sprite = selectedSprite;
        onSelected.Invoke();
        if (equipFrontFTUE.value)
            return;
        FtueHand.SetActive(false);
    }

    [Button]
    public virtual void Deselect()
    {
        bgImage.sprite = deselectedSprite;
        onDeselected.Invoke();
    }
}