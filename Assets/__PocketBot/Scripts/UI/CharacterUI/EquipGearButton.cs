using UnityEngine;
using UnityEngine.UI;
using HyrphusQ.Events;
using LatteGames.Template;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;

public class EquipGearButton : MonoBehaviour
{
    public Image ImageButton
    {
        get
        {
            return imageButton;
        }
    }

    [BoxGroup("Public Ref")] public GearButton gearButton;
    [BoxGroup("Public Ref")] public PBPartSO partSO;
    [BoxGroup("Public Ref")] public PBPartSlot partSlot;

    [SerializeField, BoxGroup("Ref")] private Image imageButton;
    [SerializeField, BoxGroup("Ref")] private Image gearImageBtn;
    [SerializeField, BoxGroup("Resource")] private Sprite equipSprite;
    [SerializeField, BoxGroup("Resource")] private Sprite unEquipSprite;

    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable Equip;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable FTUE_PVP;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable buildTabFTUE;
    [SerializeField, BoxGroup("FTUE")] private PPrefBoolVariable equipUpperFTUE;
    [SerializeField, BoxGroup("FTUE")] private GameObject ftueHand;

    public Button EquipButton => button;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(() => OnButtonClick());
    }

    private void OnEnable()
    {
        #region FTUE
        if (Equip != null)
        {
            if (!Equip.value && ftueHand != null)
            {
                ftueHand.SetActive(true);
            }
        }

        if (buildTabFTUE != null && equipUpperFTUE != null)
        {
            if (buildTabFTUE.value && !equipUpperFTUE.value)
            {
                ftueHand.SetActive(true);
            }
        }
        #endregion
    }

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

    private void OnButtonClick()
    {
        if (partSO == null) return;

        if (buildTabFTUE != null && equipUpperFTUE != null)
        {
            if (buildTabFTUE.value && !equipUpperFTUE.value)
            {
                ftueHand.SetActive(false);
                equipUpperFTUE.value = true;
                GameEventHandler.Invoke(FTUEEventCode.OnEquipUpperFTUE);
            }
        }

        if (!FTUE_PVP.value && Equip.value)
            return;

        gearButton?.EquipedIfSpecial();

        if (!partSO.IsEquipped)
        {
            if (!RobotPreviewSpawner.Instance.CheckEnoughPower(partSO, partSlot))
            {
                GameEventHandler.Invoke(CharacterUIEventCode.OnEquipNotEnoughPower);
                gearButton?.DeSelect();
                return;
            }
            GearSaver.Instance.EquipGear(partSO, partSlot);
            GameEventHandler.Invoke(CharacterUIEventCode.OnEquipEnoughPower);
            GameEventHandler.Invoke(CharacterUIEventCode.OnGearCardEquip, partSO, partSlot);
            GameEventHandler.Invoke(CharacterUIEventCode.SendWarning);

            partSO.IsEquipped = true;
        }
        else if (partSO.IsEquipped)
        {
            if (partSlot.GetPartTypeOfPartSlot() == PBPartType.Body) return;

            // bool isSwap = false;
            // PBChassisSO pbChassisSOCurrent = GearSaver.Instance.chassisSO.value.Cast<PBChassisSO>();

            // gearButton.PartSO.IsEquipped = false;
            // //pbChassisSOCurrent.AllPartSlots
            // //    .Where(slot => slot.PartType == partSlot.GetPartTypeOfPartSlot() && slot.PartVariableSO.value != null)
            // //    .ToList()
            // //    .ForEach(slot => slot.PartVariableSO.value.Cast<PBPartSO>().IsEquipped = false);

            // foreach (var slot in pbChassisSOCurrent.AllPartSlots)
            // {
            //     if (slot.PartType != partSlot.GetPartTypeOfPartSlot())
            //         continue;

            //     var partVariable = slot.PartVariableSO.value;
            //     if (partVariable == null || partVariable != partSO)
            //         continue;

            //     isSwap = slot.PartSlotType != partSlot;
            //     partSO.IsEquipped = false;
            //     GearSaver.Instance.EquipGear(null, slot.PartSlotType);
            //     GameEventHandler.Invoke(CharacterUIEventCode.OnGearCardUnEquip, partSO);
            // }

            // GearSaver.Instance.EquipGear(null, partSlot);
            // GameEventHandler.Invoke(CharacterUIEventCode.OnGearCardUnEquip, partSO);
            // GameEventHandler.Invoke(CharacterUIEventCode.SendWarning);
            // partSO.IsEquipped = false;

            // if (isSwap)
            // {
            //     GearSaver.Instance.EquipGear(partSO, partSlot);
            //     GameEventHandler.Invoke(CharacterUIEventCode.OnGearCardEquip, partSO, partSlot);
            //     GameEventHandler.Invoke(CharacterUIEventCode.SendWarning);
            //     partSO.IsEquipped = true;
            // }

            PBChassisSO currentChassisSO = GearSaver.Instance.chassisSO.value.Cast<PBChassisSO>();
            BotPartSlot currentSlotOfPartSO = currentChassisSO.AllPartSlots.Find(slot => slot.PartType == partSlot.GetPartTypeOfPartSlot() && slot.PartVariableSO.value == partSO);
            BotPartSlot desiredSlotOfPartSO = currentChassisSO.AllPartSlots.Find(slot => slot.PartSlotType == partSlot);
            bool isEquip = currentSlotOfPartSO.PartSlotType != partSlot;

            if (!isEquip)
            {
                Unequip(currentSlotOfPartSO.PartSlotType, partSO);
            }
            else
            {
                if (desiredSlotOfPartSO.PartVariableSO.value != null)
                {
                    var desiredSlotPartSO = desiredSlotOfPartSO.PartVariableSO.value.Cast<PBPartSO>();
                    GearSaver.Instance.EquipGear(null, currentSlotOfPartSO.PartSlotType);
                    GearSaver.Instance.EquipGear(null, desiredSlotOfPartSO.PartSlotType);
                    Equip(desiredSlotOfPartSO.PartSlotType, partSO);
                    Equip(currentSlotOfPartSO.PartSlotType, desiredSlotPartSO);
                }
                else
                {
                    Unequip(currentSlotOfPartSO.PartSlotType, partSO);
                    Equip(desiredSlotOfPartSO.PartSlotType, partSO);
                }
            }
            GameEventHandler.Invoke(CharacterUIEventCode.SendWarning);

            void Equip(PBPartSlot partSlot, PBPartSO partSO)
            {
                GearSaver.Instance.EquipGear(partSO, partSlot);
                GameEventHandler.Invoke(CharacterUIEventCode.OnGearCardEquip, partSO, partSlot);
                partSO.IsEquipped = true;
            }
            void Unequip(PBPartSlot partSlot, PBPartSO partSO)
            {
                GearSaver.Instance.EquipGear(null, partSlot);
                GameEventHandler.Invoke(CharacterUIEventCode.OnGearCardUnEquip, partSO);
                partSO.IsEquipped = false;
            }
        }

        if (gearButton != null)
        {
            gearButton.OnEquipEvent();
            gearButton.PartSlotEquip = partSlot;
        }

        GameEventHandler.Invoke(CharacterUIEventCode.OnClosePopUp, partSO);

        //FTUE
        if (Equip.value) return;
        GetComponent<Button>().interactable = false;
        ftueHand.SetActive(false);
        Equip.value = true;
        GameEventHandler.Invoke(FTUEEventCode.OnFinishEquipFTUE);
        GameEventHandler.Invoke(LogFTUEEventCode.EndEquip_1);
        GameEventHandler.Invoke(FTUEEventCode.OnShowEnergyFTUE, 0);
        SoundManager.Instance.PlaySFX(PBSFX.UIEquipAndUse);
    }

    public void LoadSpriteButton(bool isEquip)
    {
        if (gearImageBtn != null)
        {
            gearImageBtn.sprite = isEquip ? unEquipSprite : equipSprite;
        }
    }
}
