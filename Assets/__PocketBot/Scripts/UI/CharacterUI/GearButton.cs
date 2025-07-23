using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using HyrphusQ.Events;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using LatteGames.Monetization;
using System;

public class GearButton : MonoBehaviour
{
    public UnityEvent OnEquipAction = new UnityEvent();

    [SerializeField, BoxGroup("Property")] private bool showFullBossName = true;
    [SerializeField, BoxGroup("Property")] private string prefixProgressionCardsColor;
    [SerializeField, BoxGroup("Property")] private string suffixProgressionCardsColor;
    // [SerializeField, BoxGroup("Property")] private float configSO.amimationDuration;
    // [SerializeField, BoxGroup("Property")] private Vector2 configSO.cardSelectedSizeDelta_1Slot;
    // [SerializeField, BoxGroup("Property")] private Vector2 cardSelectedPosition_2Slot;
    // [SerializeField, BoxGroup("Property")] private Vector2 cardDeSelectedPosition_Default;
    // [SerializeField, BoxGroup("Property")] private Vector2 buttonsSelectedPosition_1Slot;
    // [SerializeField, BoxGroup("Property")] private Vector2 buttonsSelectedPosition_2Slot;
    // [SerializeField, BoxGroup("Property")] private Vector2 buttonsDeSelectedPosition_Default;
    // [SerializeField, BoxGroup("Property")] private Vector2 sufficientCardQuantityToUpgradeTextPosition = Vector2.zero;
    // [SerializeField, BoxGroup("Property")] private Vector2 insufficientCardQuantityToUpgradeTextPosition = new Vector2(0f, -6f);

    [SerializeField, BoxGroup("Ref")] private GearButtonConfigSO configSO;
    [SerializeField, BoxGroup("Ref")] private EquipGearButton equipGearButtonSlot_1;
    [SerializeField, BoxGroup("Ref")] private EquipGearButton equipGearButtonSlot_2;
    [SerializeField, BoxGroup("Ref")] private Image lockIcon;
    [SerializeField, BoxGroup("Ref")] private Image rarityImage, rarityWithoutCurvedImage;
    [SerializeField, BoxGroup("Ref")] private Image newIcon;
    [SerializeField, BoxGroup("Ref")] private Image icon;
    [SerializeField, BoxGroup("Ref")] private Image upgradableIcon;
    [SerializeField, BoxGroup("Ref")] private Image missingCardIcon;
    [SerializeField, BoxGroup("Ref")] private GameObject energyBox;
    [SerializeField, BoxGroup("Ref")] private GameObject footer;
    [SerializeField, BoxGroup("Ref")] private GameObject footerNonSpecial;
    [SerializeField, BoxGroup("Ref")] private GameObject footerSpecial;
    [SerializeField, BoxGroup("Ref")] private GameObject foundInSection;
    [SerializeField, BoxGroup("Ref")] private GameObject foundInTransformBot;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI energyText;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI progressionText;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI levelText;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI foundInArenaText;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI bossNameText;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI specialBossText;
    [SerializeField, BoxGroup("Ref")] private RectTransform mainCardRect;
    [SerializeField, BoxGroup("Ref")] private RectTransform infoButtonRect;
    [SerializeField, BoxGroup("Ref")] private RectTransform energyBoxTransform;
    [SerializeField, BoxGroup("Ref")] private Image progressBar;
    [SerializeField, BoxGroup("Ref")] private LayoutElement layoutElement;
    [SerializeField, BoxGroup("Ref")] private PBPartSlot partSlot;
    [SerializeField, BoxGroup("Ref")] private DOTweenAnimation energyBoxBreakthingAnimation;
    [SerializeField, BoxGroup("FTUE")] private GameObject ftueHand;

    [SerializeField, BoxGroup("PSFTUE")] private GameObject m_FTUECardHand;
    [SerializeField, BoxGroup("PSFTUE")] private GameObject m_FTUEUpgradeHand;
    [SerializeField, BoxGroup("PSFTUE")] private GameObject m_FTUENewWeaponEquipHand;
    [SerializeField, BoxGroup("PSFTUE")] private PSFTUESO m_PSFTUESO;
    [BoxGroup("PSFTUE")] private bool m_IsInventory 
    {
        get
        {
            GearListSelection gearListSelection = gameObject.GetComponentInParent<GearListSelection>();
                return gearListSelection != null;
        }
    }

    [BoxGroup("Public Ref")] public Image equippedOutline;
    [BoxGroup("Public Ref")] public Button upgradeBtn;
    [BoxGroup("Public Ref")] public Button equipBtn;
    [BoxGroup("Public Ref")] public Button equipBtn_2;
    [BoxGroup("Public Ref")] public Button swapBtn;
    [BoxGroup("Public Ref")] public Button claimRVButton;
    [BoxGroup("Public Ref")] public TextMeshProUGUI equipText;
    [BoxGroup("Public Ref")] public TextMeshProUGUI equipText_2;
    [BoxGroup("Public Ref")] public TextMeshProUGUI gearNameText;

    [NonSerialized]
    public bool isDisable;
    [NonSerialized]
    public bool isSameSlot;

    public PBPartSO PartEquip1_FTUE => HandleFTUECharacterUI.partEquip1_FTUE;
    public PBPartSO PartEquip2_FTUE => HandleFTUECharacterUI.partEquip2_FTUE;
    public EquipGearButton EquipGearButtonSlot_1 => equipGearButtonSlot_1;
    public EquipGearButton EquipGearButtonSlot_2 => equipGearButtonSlot_2;
    public PBPartSlot PartSlotEquip { get; set; }
    public PBPartManagerSO SpecialSO => configSO.specialBotManagerSO;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
        }
    }
    public PBPartSO PartSO
    {
        get => partSO;
        set
        {
            partSO = value;
            UpdateBasicData(partSO?.ManagerSO, partSO);
            UpdateLevelUpgradeData(partSO?.ManagerSO, partSO);
        }
    }
    public PBPartSlot PartSlot
    {
        get => partSlot;
        set
        {
            partSlot = value;
            IsSameSlot();
            if (equipBtn != null)
            {
                if (partSlot == PBPartSlot.Body)
                {
                    EquipGearButton equipGearButton = equipBtn.GetComponent<EquipGearButton>();
                    equipGearButton.partSO = PartSO;
                    equipGearButton.partSlot = PartSlot;
                    equipGearButton.gearButton = this;
                }
                else
                {
                    SetUpSlotPartEquipButton();
                }

                equippedOutline.gameObject.SetActive(partSO.IsEquipped);
            }
        }
    }
    public CanvasGroup CanvasGroup
    {
        get
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            return canvasGroup;
        }
    }
    public RectTransform RectTransform => transform as RectTransform;
    public Button Button
    {
        get
        {
            if (button == null)
                button = GetComponent<MultiImageButton>();
            return button;
        }
    }
    public Material GrayScaleMat => configSO.grayScaleMat;

    private PBPartSO partSO;
    private Button button;
    private CanvasGroup canvasGroup;
    private GearListSelection m_GearListSelection;
    private int slotIndex;
    private bool isMoreThanOneSlot;
    private bool isSelected;
    private bool isNotSlotPart = false;
    private bool isEquipBossBody
    {
        get
        {
            var chassisSO = configSO.currentChassisInUse.value.Cast<PBChassisSO>();
            return chassisSO.IsSpecial;
        }
    }

    private string equipSlot_Text = "EQUIP";
    private string equipSlot1_Text = "EQUIP 1";
    private string equipSlot2_Text = "EQUIP 2";
    private string equippedSlot_Text = "EQUIPPED";
    private string unEquippedSlot_Text = "UNEQUIP";
    private string noSlot_Text = "NO SLOT";
    private string useBoss_Text = "USE";
    private string usedBoss_Text = "USED";

    private void Awake()
    {
        claimRVButton.onClick.AddListener(OnClickClaimBossBtn);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnGearCardEquip, OnEquipEvent);
        // GameEventHandler.AddActionEvent(CharacterUIEventCode.OnClosePopUp, OnClosePopupEvent);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartCardChanged, UpdateLevelUpgradeData);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUpgraded, UpdateLevelUpgradeData);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUnlocked, UpdateBasicData);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnEquipNotEnoughPower, OnNotEnoughPower);
        GameEventHandler.AddActionEvent(BossFightEventCode.OnClaimBossComplete, OnClaimCompleteRV);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnAutoOpenGearinfo, OnAutoOpenGearinfo);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnShowEnergyFTUE, OnShowEnergyFTUE);
        GameEventHandler.AddActionEvent(FTUEEventCode.OnFinishShowEnergyFTUE, OnFinishShowEnergyFTUE);
    }

    private void OnDestroy()
    {
        claimRVButton.onClick.RemoveListener(OnClickClaimBossBtn);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearCardEquip, OnEquipEvent);
        // GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnClosePopUp, OnClosePopupEvent);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartCardChanged, UpdateLevelUpgradeData);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUpgraded, UpdateLevelUpgradeData);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUnlocked, UpdateBasicData);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnEquipNotEnoughPower, OnNotEnoughPower);
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnClaimBossComplete, OnClaimCompleteRV);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnAutoOpenGearinfo, OnAutoOpenGearinfo);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnShowEnergyFTUE, OnShowEnergyFTUE);
        GameEventHandler.RemoveActionEvent(FTUEEventCode.OnFinishShowEnergyFTUE, OnFinishShowEnergyFTUE);

        DOTween.Kill(this);
    }

    private void OnEnable()
    {
        #region FTUE
        if (!configSO.equipFTUE.value)
        {
            if (partSlot.GetPartTypeOfPartSlot() == HandleFTUECharacterUI.partType_Equip_1 && partSO.IsUnlocked() && PartEquip1_FTUE == partSO)
            {
                ftueHand.SetActive(true);
            }
        }
        if (!configSO.upgradeFTUE.value && configSO.equipFTUE.value)
        {
            if (partSlot.GetPartTypeOfPartSlot() == HandleFTUECharacterUI.partType_Equip_1 && partSO.IsUnlocked() && PartEquip1_FTUE == partSO)
            {
                ftueHand.SetActive(true);
                FTUEMainScene.Instance.ShowUpgradeMakeYouStronger();
            }
        }
        #endregion

        OnCheckPSFTUE();
    }

    private void OnDisable()
    {
        energyText.DOKill();
        energyText.color = Color.white;
        energyBoxTransform.DOKill();

        m_FTUECardHand.SetActive(false);
        m_FTUENewWeaponEquipHand.SetActive(false);
        m_FTUEUpgradeHand.SetActive(false);
        GameEventHandler.Invoke(PSFTUEBlockAction.Unblock, this.gameObject);
        OnScroll(true);
    }

    private void Start()
    {
        m_GearListSelection = gameObject.GetComponentInParent<GearListSelection>();
        upgradeBtn.onClick.AddListener(
            () =>
            {
                GameEventHandler.Invoke(CharacterUIEventCode.OnGearCardInfoPopUpShow, partSO, partSlot, this, isDisable, isSameSlot, isMoreThanOneSlot, slotIndex, equipGearButtonSlot_1, equipGearButtonSlot_2);

                if (m_PSFTUESO.FTUEOpenBuildUI.value && !m_PSFTUESO.FTUEOpenInfoPopup.value && m_PSFTUESO.WeaponUpgradeFTUEPPref == partSO && m_IsInventory)
                {
                    m_FTUECardHand.SetActive(false);
                    m_FTUEUpgradeHand.SetActive(false);
                    GameEventHandler.Invoke(PSFTUEBlockAction.Unblock, this.gameObject);
                    OnScroll(true);

                    GameEventHandler.Invoke(PSLogFTUEEventCode.EndOpenInfoPopup);
                    GameEventHandler.Invoke(PSLogFTUEEventCode.StartClickUpgrade);
                }
            });

        if (isEquipBossBody)
        {
            if (!(partSO as PBChassisSO))
            {
                equippedOutline.gameObject.SetActive(false);
            }
        }
    }

    private void OnCheckPSFTUE()
    {
        if (m_PSFTUESO != null)
        {
            if (m_PSFTUESO.FTUEOpenBuildUI.value && !m_PSFTUESO.FTUEOpenInfoPopup.value && m_PSFTUESO.WeaponUpgradeFTUEPPref == partSO && m_IsInventory)
            {
                OnScroll(false);
                m_FTUECardHand.SetActive(true);
                GameEventHandler.Invoke(PSFTUEBlockAction.Block, this.gameObject, PSFTUEBubbleText.None);
            }

            if (m_PSFTUESO.FTUEStart2ndMatch.value && !m_PSFTUESO.FTUENewWeapon.value && m_PSFTUESO.WeaponNewFTUEPPref == partSO && m_IsInventory)
            {
                OnScroll(false);
                m_FTUECardHand.SetActive(true);
                GameEventHandler.Invoke(PSFTUEBlockAction.Block, this.gameObject, PSFTUEBubbleText.None);
                equipGearButtonSlot_1.GetComponent<Button>().onClick.AddListener(EquipFTUE);

                void EquipFTUE()
                {
                    upgradeBtn.enabled = true;
                    EquipGearButtonSlot_1.enabled = true;
                    EquipGearButtonSlot_2.enabled = true;

                    OnScroll(true);
                    m_PSFTUESO.FTUENewWeapon.value = true;
                    m_FTUECardHand.SetActive(false);
                    m_FTUENewWeaponEquipHand.SetActive(false);
                    equipGearButtonSlot_1.GetComponent<Button>().onClick.RemoveListener(EquipFTUE);
                    GameEventHandler.Invoke(PSFTUEBlockAction.Unblock, this.gameObject);

                    GameEventHandler.Invoke(PSLogFTUEEventCode.EndNewWeapon_Equip);
                }
            }
        }
    }

    private void OnScroll(bool isActive)
    {
        ScrollRect scrollRect = gameObject.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
            scrollRect.enabled = isActive;
    }

    public void SetUpSlotPartEquipButton()
    {
        PBPartType pBPartType = PartSO.PartType;
        if (equipGearButtonSlot_1 != null)
        {
            equipGearButtonSlot_1.partSO = PartSO;
            equipGearButtonSlot_1.partSlot = GearSlotHelper.GetSlotPartType(pBPartType, 0);
            equipGearButtonSlot_1.gearButton = this;
        }

        if (equipGearButtonSlot_2 != null)
        {
            equipGearButtonSlot_2.partSO = PartSO;
            equipGearButtonSlot_2.partSlot = GearSlotHelper.GetSlotPartType(pBPartType, 1);
            equipGearButtonSlot_2.gearButton = this;
        }
    }

    public void EquipedIfSpecial()
    {
        if (partSO == null) return;
        if (partSO as PBChassisSO)
        {
            if (!(partSO as PBChassisSO).IsSpecial)
            {
                GameEventHandler.Invoke(CharacterUIEventCode.OnEquipedNotSpecial);
            }
            else
            {
                PBChassisSO pbChassisSO = partSO as PBChassisSO;
                pbChassisSO.CurrentInUse.value = partSO as PBChassisSO;
                GameEventHandler.Invoke(CharacterUIEventCode.OnEquipedNotSpecial);
            }
        }
    }
    private void ConvertTextEquiped(string text)
    {
        if (partSO as PBChassisSO)
        {
            if (partSO.Cast<PBChassisSO>().IsSpecial)
                equipText.SetText(text);
        }
    }
    private void HandleButtonWhenEquipedSpecial()
    {
        if (isEquipBossBody)
        {
            if (!(partSO as PBChassisSO))
            {
                equipBtn.gameObject.SetActive(true);
                swapBtn.gameObject.SetActive(false);

                isDisable = true;
                equipText.SetText(noSlot_Text);
                equipBtn.image.material = GrayScaleMat;
                equipBtn.interactable = false;

                equipText_2.SetText(noSlot_Text);
                equipBtn_2.image.material = GrayScaleMat;
                equipBtn_2.interactable = false;

                equippedOutline.gameObject.SetActive(false);
            }
        }
    }
    public void Select()
    {
        if (!configSO.equipFTUE.value && partSO.PartType == PBPartType.Body)
        {
            return;
        }

        if (!configSO.upgradeFTUE.value && partSO.PartType != HandleFTUECharacterUI.partType_Equip_1)
        {
            return;
        }

        if (!partSO.IsUnlocked())
        {
            if (partSO is PBChassisSO transformChassisSO && transformChassisSO.IsTransformBot)
            {
                PBUltimatePackPopup.Instance.TryShowIfCan(transformChassisSO.TransformBotID);
            }
            return;
        }

        if (isSelected)
        {
            isSelected = false;
            DeSelect();
            return;
        }

        //FTUE
        if (m_PSFTUESO.FTUEOpenBuildUI.value && !m_PSFTUESO.FTUEOpenInfoPopup.value && m_PSFTUESO.WeaponUpgradeFTUEPPref == partSO && m_IsInventory)
        {
            m_FTUECardHand.gameObject.SetActive(false);
            m_FTUEUpgradeHand.gameObject.SetActive(true);

            EquipGearButtonSlot_1.enabled = false;
            EquipGearButtonSlot_2.enabled = false;

            if (!partSO.IsUpgradable())
            {
                partSO.UpdateNumOfCards(2);
                CurrencyManager.Instance.AcquireWithoutLogEvent(CurrencyType.Standard, 200);
            }
        }

        if (m_PSFTUESO.FTUEStart2ndMatch.value && !m_PSFTUESO.FTUENewWeapon.value && m_PSFTUESO.WeaponNewFTUEPPref == partSO && m_IsInventory)
        {
            m_FTUECardHand.gameObject.SetActive(false);
            m_FTUENewWeaponEquipHand.gameObject.SetActive(true);

            upgradeBtn.enabled = false;
            EquipGearButtonSlot_1.enabled = true;
            EquipGearButtonSlot_2.enabled = false;

            if (!m_PSFTUESO.WeaponNewFTUEPPref.IsUnlocked())
                m_PSFTUESO.WeaponNewFTUEPPref.TryUnlockItem();
        }

        isSelected = true;
        if (partSO is PBChassisSO pBChassis)
        {
            configSO.currentChassisSelected.value = partSO.Cast<PBChassisSO>();
            // infoButtonRect.anchoredPosition = configSO.buttonsDeSelectedPosition_Default;
            infoButtonRect.DOAnchorPos(new Vector2(0, configSO.buttonsSelectedPosition_1Slot.y), configSO.amimationDuration);
            mainCardRect.DOSizeDelta(configSO.cardSelectedSizeDelta_1Slot, configSO.amimationDuration);
            upgradeBtn.gameObject.SetActive(true);
            equipBtn.gameObject.SetActive(true);
            equipBtn.image.material = null;
            equipText.SetText(equipSlot_Text);

            if (pBChassis.IsSpecial)
                equipText.SetText(useBoss_Text);
        }
        else
        {
            int slotCount = 0;
            PBChassisSO pbChassisSOCurrent = configSO.currentChassisInUse.value.Cast<PBChassisSO>();
            for (int i = 0; i < pbChassisSOCurrent.AllPartSlots.Count; i++)
            {
                if (pbChassisSOCurrent.AllPartSlots[i].PartType == partSO.PartType)
                {
                    slotCount++;
                }
            }

            isNotSlotPart = slotCount == 0;
            isMoreThanOneSlot = slotCount >= 2;
            if (isMoreThanOneSlot)
            {
                // infoButtonRect.anchoredPosition = configSO.buttonsDeSelectedPosition_Default;
                infoButtonRect.DOAnchorPos(configSO.buttonsSelectedPosition_2Slot, configSO.amimationDuration);
                mainCardRect.DOSizeDelta(configSO.cardSelectedSizeDelta_2Slot, configSO.amimationDuration);
                upgradeBtn.gameObject.SetActive(true);
                equipBtn.gameObject.SetActive(true);
                equipBtn_2.gameObject.SetActive(true);
                swapBtn.gameObject.SetActive(false);
                equipBtn.interactable = true;
                equipBtn_2.interactable = true;
                equipBtn.image.material = null;
                equipBtn_2.image.material = null;
                equipText.SetText(equipSlot1_Text);
                equipText_2.SetText(equipSlot2_Text);
            }
            else
            {
                // infoButtonRect.anchoredPosition = configSO.buttonsDeSelectedPosition_Default;
                infoButtonRect.DOAnchorPos(configSO.buttonsSelectedPosition_1Slot, configSO.amimationDuration);
                mainCardRect.DOSizeDelta(configSO.cardSelectedSizeDelta_1Slot, configSO.amimationDuration);
                upgradeBtn.gameObject.SetActive(true);
                equipBtn.gameObject.SetActive(true);
                equipBtn_2.gameObject.SetActive(false);
                swapBtn.gameObject.SetActive(false);
                equipBtn.interactable = true;
                equipBtn_2.interactable = false;
                equipBtn.image.material = null;
                equipBtn_2.image.material = null;
                equipText.SetText(equipSlot_Text);
            }
        }

        if (partSO.IsNew())
        {
            NewItemModule newItemModule = partSO.GetModule<NewItemModule>();
            newItemModule.isNew = false;
            newIcon.gameObject.SetActive(newItemModule.isNew);
        }

        equipBtn.gameObject.SetActive(true);
        if (partSO is PBChassisSO chassis)
        {
            if (chassis.IsSpecial)
            {
                GameEventHandler.Invoke(CharacterUIEventCode.OnSelectSpecial, partSO);
                GameEventHandler.Invoke(CharacterUIEventCode.OnShowPopup);
                GameEventHandler.Invoke(CharacterUIEventCode.OnHidePowerInfo);
            }
            else
            {
                GameEventHandler.Invoke(CharacterUIEventCode.PureOnClosePopup);
                GameEventHandler.Invoke(CharacterUIEventCode.OnShowPowerInfo);
            }
        }

        if (!configSO.equipFTUE.value || !configSO.upgradeFTUE.value)
        {
            ftueHand.SetActive(false);
        }


        //FTUE configSO.equipFTUE Upper
        if (configSO.buildTabFTUE.value && !configSO.equipUpperFTUE.value && PartEquip2_FTUE == partSO)
        {
            ftueHand.SetActive(false);
        }

        upgradeBtn.gameObject.SetActive(true);
        //equipBtn.gameObject.SetActive(true);
        //swapBtn.gameObject.SetActive(true);

        upgradeBtn.image.DOFade(1, configSO.amimationDuration).OnComplete(() => CheckFTUE());

        //Swap Button Condition
        CurrentGearHolder.Instance.swapSO = partSO;
        SwapCondition();
        if (partSO.PartType == PBPartType.Body)
        {
            if (partSO.IsEquipped == true)
            {
                equipBtn.image.DOFade(1, configSO.amimationDuration);
                equipText.SetText(equippedSlot_Text);
                equipBtn.image.material = GrayScaleMat;
                equipBtn.interactable = false;
                HandleButtonWhenEquipedSpecial();
                ConvertTextEquiped("USED");
                equipBtn.GetComponent<EquipGearButton>().LoadSpriteButton(false);

                return;
            }
            equipBtn.GetComponent<EquipGearButton>().LoadSpriteButton(false);
        }
        else
        {
            if (partSO.IsEquipped == true)
            {
                infoButtonRect.DOKill();
                // infoButtonRect.anchoredPosition = configSO.buttonsDeSelectedPosition_Default;
                infoButtonRect.DOAnchorPos(isMoreThanOneSlot ? configSO.buttonsSelectedPosition_2Slot : configSO.buttonsSelectedPosition_1Slot, configSO.amimationDuration);
                mainCardRect.DOKill();
                mainCardRect.DOSizeDelta(isMoreThanOneSlot ? configSO.cardSelectedSizeDelta_2Slot : configSO.cardSelectedSizeDelta_1Slot, configSO.amimationDuration);

                equipBtn.image.DOFade(1, configSO.amimationDuration).OnComplete(() => equipBtn.interactable = true);

                int indexSlot = 0;
                PBChassisSO pbChassisSOCurrent = configSO.currentChassisInUse.value.Cast<PBChassisSO>();
                for (int i = 0; i < pbChassisSOCurrent.AllPartSlots.Count; i++)
                {
                    if (partSO == pbChassisSOCurrent.AllPartSlots[i].PartVariableSO.value)
                    {
                        indexSlot = GearSlotHelper.GetSlotIndex(pbChassisSOCurrent.AllPartSlots[i].PartSlotType);
                        break;
                    }
                }
                slotIndex = indexSlot;

                if (isMoreThanOneSlot)
                {
                    equipText.SetText(indexSlot == 0 ? unEquippedSlot_Text : equipSlot1_Text);
                    equipText_2.SetText(indexSlot == 1 ? unEquippedSlot_Text : equipSlot2_Text);
                    equipBtn.gameObject.SetActive(true);
                    equipBtn_2.gameObject.SetActive(true);

                    equipBtn.GetComponent<EquipGearButton>().LoadSpriteButton(indexSlot == 0);
                    equipBtn_2.GetComponent<EquipGearButton>().LoadSpriteButton(indexSlot == 1);
                }
                else
                {
                    equipText.SetText(unEquippedSlot_Text);
                    equipText_2.SetText(unEquippedSlot_Text);
                    equipBtn.gameObject.SetActive(indexSlot == 0);
                    equipBtn_2.gameObject.SetActive(indexSlot == 1);

                    equipBtn.GetComponent<EquipGearButton>().LoadSpriteButton(true);
                }
                return;
            }
            else
            {
                equipBtn.GetComponent<EquipGearButton>().LoadSpriteButton(false);
                equipBtn_2.GetComponent<EquipGearButton>().LoadSpriteButton(false);
            }
        }


        if (!(partSO is PBChassisSO))
            isDisable = isNotSlotPart;

        if (isEquipBossBody)
        {
            if (!(partSO as PBChassisSO))
                isDisable = true;
        }

        if (isDisable)
        {
            equipText.SetText(noSlot_Text);
            equipBtn.image.material = GrayScaleMat;
            equipBtn.interactable = false;
        }

        HandleButtonWhenEquipedSpecial();
        equipBtn.image.DOFade(1, configSO.amimationDuration).OnComplete(() => equipBtn.interactable = !isDisable);

        claimRVButton.gameObject.SetActive(false);
        if (partSO is PBChassisSO)
        {
            PBChassisSO pBChassisSO = partSO.Cast<PBChassisSO>();
            if (pBChassisSO != null)
            {
                if (pBChassisSO.IsSpecial && !pBChassisSO.IsClaimedRV)
                {
                    GameEventHandler.Invoke(BossFightEventCode.OnSelectBossLockInventory, pBChassisSO);
                    equipBtn.gameObject.SetActive(false);
                    claimRVButton.gameObject.SetActive(true);
                }
            }
        }
    }

    public void DeSelect()
    {
        //FTUE
        if (m_PSFTUESO.FTUEOpenBuildUI.value && !m_PSFTUESO.FTUEOpenInfoPopup.value && m_PSFTUESO.WeaponUpgradeFTUEPPref == partSO && m_IsInventory)
        {
            m_FTUECardHand.gameObject.SetActive(true);
            m_FTUEUpgradeHand.gameObject.SetActive(false);
        }
        if (m_PSFTUESO.FTUEStart2ndMatch.value && !m_PSFTUESO.FTUENewWeapon.value && m_PSFTUESO.WeaponNewFTUEPPref == partSO && m_IsInventory)
        {
            m_FTUECardHand.gameObject.SetActive(true);
            m_FTUENewWeaponEquipHand.gameObject.SetActive(false);
        }


        OnEquipAction.RemoveAllListeners();
        isSelected = false;
        if (partSO == null || !partSO.IsUnlocked())
            return;

        infoButtonRect.DOAnchorPos(configSO.buttonsDeSelectedPosition_Default, configSO.amimationDuration);
        mainCardRect.DOSizeDelta(configSO.cardDeSelectedSizeDelta_Default, configSO.amimationDuration);
        upgradeBtn.gameObject.SetActive(false);
        equipBtn.gameObject.SetActive(false);
        equipBtn_2.gameObject.SetActive(false);
        swapBtn.gameObject.SetActive(false);
        claimRVButton.gameObject.SetActive(false);

        if (!configSO.equipFTUE.value || !configSO.upgradeFTUE.value)
        {
            ftueHand.SetActive(true);
        }

        //FTUE configSO.equipFTUE Upper
        if (configSO.buildTabFTUE.value && !configSO.equipUpperFTUE.value && PartEquip2_FTUE == partSO)
        {
            ftueHand.SetActive(true);
        }

        upgradeBtn.image.DOFade(0, configSO.amimationDuration).OnComplete(() => upgradeBtn.interactable = false);
        if (isDisable)
        {
            return;
        }

        equipBtn.image.material = null;

        equipBtn.image.DOFade(0, configSO.amimationDuration).OnComplete(() => equipBtn.interactable = false);
        swapBtn.image.DOFade(0, configSO.amimationDuration).OnComplete(() => swapBtn.interactable = false);
    }

    private void CheckFTUE()
    {
        upgradeBtn.interactable = true;
        if (!configSO.equipFTUE.value)
        {
            upgradeBtn.interactable = false;
        }
        if (configSO.buildTabFTUE.value && !configSO.equipUpperFTUE.value)
        {
            upgradeBtn.interactable = false;
        }
    }

    public void UpdateBasicData(params object[] parameters)
    {
        if (partSO == null) return;
        if (parameters[1] as PBPartSO != partSO) return;

        MonoImageItemModule monoImageItemModule = partSO.GetModule<MonoImageItemModule>();
        icon.sprite = monoImageItemModule.thumbnailImage;
        energyBox.SetActive(partSO.GetPower().value > 0 && partSO.IsUnlocked());
        lockIcon.gameObject.SetActive(!partSO.IsUnlocked());
        energyText.SetText(partSO.GetPower().value.ToString());
        RarityItemModule rarityItemModule = partSO.GetModule<RarityItemModule>();
        if (rarityItemModule.rarityType == RarityType.Legendary)
        {
            rarityImage.sprite = configSO.legendarySprite;
            rarityWithoutCurvedImage.sprite = configSO.legendaryWithoutCurvedSprite;
        }
        else if (rarityItemModule.rarityType == RarityType.Epic)
        {
            rarityImage.sprite = configSO.epicSprite;
            rarityWithoutCurvedImage.sprite = configSO.epicWithoutCurvedSprite;
        }
        else if (rarityItemModule.rarityType == RarityType.Common)
        {
            rarityImage.sprite = configSO.commonSprite;
            rarityWithoutCurvedImage.sprite = configSO.commonWithoutCurvedSprite;
        }

        bool isSpecialBoss = false;
        bool isUnlocked = partSO.IsUnlocked();

        if (partSO is PBChassisSO chassisSO && chassisSO.IsSpecial)
        {
            isSpecialBoss = true;
            foundInTransformBot.SetActive(chassisSO.IsTransformBot);
            foundInArenaText.gameObject.SetActive(!chassisSO.IsTransformBot);
            foundInArenaText.SetText($"Boss Fight");
            gearNameText.gameObject.SetActive(false);
            bossNameText.gameObject.SetActive(showFullBossName && isUnlocked);
            specialBossText.gameObject.SetActive(!showFullBossName && isUnlocked);
            bossNameText.SetText(chassisSO.GetDisplayName());
            rarityImage.gameObject.SetActive(false);
            rarityWithoutCurvedImage.gameObject.SetActive(true);
        }
        else
        {
            foundInTransformBot.SetActive(false);
            foundInArenaText.gameObject.SetActive(true);
            foundInArenaText.SetText($"Arena {partSO.foundInArena}");
            gearNameText.gameObject.SetActive(true);
            gearNameText.SetText(partSO.GetDisplayName());
            bossNameText.gameObject.SetActive(false);
            specialBossText.gameObject.SetActive(false);
            rarityImage.gameObject.SetActive(isUnlocked);
            rarityWithoutCurvedImage.gameObject.SetActive(!isUnlocked);
        }

        equipBtn.interactable = !isDisable;

        if (isUnlocked)
        {
            footer.SetActive(true);
            footerSpecial.gameObject.SetActive(isSpecialBoss);
            footerNonSpecial.gameObject.SetActive(!isSpecialBoss);
            foundInSection.SetActive(false);
        }
        else
        {
            footer.SetActive(false);
            foundInSection.SetActive(true);
        }

        if (partSO.IsEquipped)
        {
            equippedOutline.gameObject.SetActive(true);
            if (IsSameSlot())
            {
                CurrentGearHolder.Instance.currentSO = partSO;
            }
        }
        else
        {
            equippedOutline.gameObject.SetActive(false);
        }
        OnEquipEvent();
    }

    public void UpdateOutlineSelected()
    {
        if (partSO == null) return;
        equippedOutline.gameObject.SetActive(partSO.IsEquipped);
    }

    public void UpdateLevelUpgradeData(params object[] parameters)
    {
        if (partSO == null) return;
        if (parameters[1] as PBPartSO != partSO) return;

        int currentLevel = partSO.GetCurrentUpgradeLevel();
        levelText.SetText($"{currentLevel}");

        NewItemModule newItemModule = partSO.GetModule<NewItemModule>();
        newIcon.gameObject.SetActive(newItemModule.isNew);

        if (!partSO.IsMaxUpgradeLevel() && partSO.TryGetCurrentUpgradeRequirement(out Requirement_GachaCard requirement_GachaCard))
        {
            UpdateLevelUpgradeData(requirement_GachaCard.currentNumOfCards, requirement_GachaCard.requiredNumOfCards);
        }
        else
        {
            UpdateLevelUpgradeData(0, 0);
        }
    }

    public void UpdateLevelUpgradeData(int currentNumOfCards, int requiredNumOfCards)
    {
        if (!partSO.IsMaxUpgradeLevel())
        {
            string text = $"{currentNumOfCards}/{requiredNumOfCards}";
            if (currentNumOfCards < requiredNumOfCards)
                text = $"{prefixProgressionCardsColor}{currentNumOfCards}{suffixProgressionCardsColor}/{requiredNumOfCards}";
            progressionText.SetText(text);
            progressBar.fillAmount = (float)currentNumOfCards / requiredNumOfCards;
            if (currentNumOfCards >= requiredNumOfCards)
            {
                missingCardIcon.gameObject.SetActive(false);
                upgradableIcon.gameObject.SetActive(true);
                progressionText.rectTransform.anchoredPosition = configSO.sufficientCardQuantityToUpgradeTextPosition;
                progressBar.sprite = configSO.sufficientCardQuantityToUpgradeSprite;
            }
            else
            {
                missingCardIcon.gameObject.SetActive(true);
                upgradableIcon.gameObject.SetActive(false);
                progressionText.rectTransform.anchoredPosition = configSO.insufficientCardQuantityToUpgradeTextPosition;
                progressBar.sprite = configSO.insufficientCardQuantityToUpgradeSprite;
            }
            progressionText.GetComponent<ContentSizeFitter>().enabled = true;
            progressionText.fontSize = configSO.notMaxUpgradeCardTextSize;
        }
        else
        {
            missingCardIcon.gameObject.SetActive(false);
            upgradableIcon.gameObject.SetActive(false);
            progressionText.GetComponent<ContentSizeFitter>().enabled = false;
            progressionText.rectTransform.sizeDelta = new Vector2(115f, progressionText.rectTransform.sizeDelta.y);
            progressionText.fontSize = configSO.maxUpgradeCardTextSize;
            progressionText.SetText(I2LHelper.TranslateTerm(I2LTerm.ButtonTitle_MaxUpgraded));
            progressionText.rectTransform.anchoredPosition = configSO.sufficientCardQuantityToUpgradeTextPosition;
            progressBar.fillAmount = 1;
            progressBar.sprite = configSO.sufficientCardQuantityToUpgradeSprite;
        }
    }

    public void OnEquipEvent()
    {
        if (!isSelected) return;
        if (partSO == null) return;

        PBChassisSO pbSkinSO;
        if (partSO as PBChassisSO)
        {
            pbSkinSO = partSO as PBChassisSO;
            if (pbSkinSO.IsSpecial)
            {
                GameEventHandler.Invoke(CharacterUIEventCode.OnTabSpecial, true);
            }
            else
                GameEventHandler.Invoke(CharacterUIEventCode.OnTabSpecial, false);
        }

        IsSameSlot();
        if (partSO.IsEquipped)
        {
            if (isSameSlot)
            {
                equippedOutline.gameObject.SetActive(true);
            }

            if (partSO.PartType == PBPartType.Body)
            {
                equipBtn.interactable = false;
                equipText.SetText(equippedSlot_Text);
                if (partSO.PartType == PBPartType.Body)
                {
                    equippedOutline.gameObject.SetActive(true);
                }

                ConvertTextEquiped("USED");
            }
            else
            {
                equipText.SetText(unEquippedSlot_Text);
            }
        }
        else
        {
            equippedOutline.gameObject.SetActive(false);
            equipText.SetText(equipSlot_Text);
            ConvertTextEquiped("USE");

            equipBtn.image.material = null;
        }

        equippedOutline.gameObject.SetActive(partSO.IsEquipped);

        isSelected = false;
        Select();
        GameEventHandler.Invoke(CharacterUIEventCode.OnSwapPart);
        OnEquipAction.Invoke();
    }

    public void IgnoreLayout(bool isIgnore = true)
    {
        layoutElement.ignoreLayout = isIgnore;
    }

    public bool IsSameSlot()
    {
        isSameSlot = false;
        switch (PartSlotEquip)
        {
            case PBPartSlot.Body:
                break;
            case PBPartSlot.Wheels_1:
                if (partSO == GearSaver.Instance.wheels_1SO.value)
                {
                    isSameSlot = true;
                    CurrentGearHolder.Instance.currentSO = partSO;
                }
                break;
            case PBPartSlot.Wheels_2:
                if (partSO == GearSaver.Instance.wheels_2SO.value)
                {
                    isSameSlot = true;
                    CurrentGearHolder.Instance.currentSO = partSO;
                }
                break;
            case PBPartSlot.Wheels_3:
                if (partSO == GearSaver.Instance.wheels_3SO.value)
                {
                    isSameSlot = true;
                    CurrentGearHolder.Instance.currentSO = partSO;
                }
                break;
            case PBPartSlot.Front_1:
                if (partSO == GearSaver.Instance.frontSO.value)
                {
                    isSameSlot = true;
                    CurrentGearHolder.Instance.currentSO = partSO;
                }
                break;
            case PBPartSlot.Upper_1:
                if (partSO == GearSaver.Instance.upper_1.value)
                {
                    isSameSlot = true;
                    CurrentGearHolder.Instance.currentSO = partSO;
                }
                break;
            case PBPartSlot.Upper_2:
                if (partSO == GearSaver.Instance.upper_2.value)
                {
                    isSameSlot = true;
                    CurrentGearHolder.Instance.currentSO = partSO;
                }
                break;
            default:
                CurrentGearHolder.Instance.currentSO = null;
                break;
        }
        return isSameSlot;
    }

    public void CheckEquipButton()
    {
        if (isDisable) equipText.SetText(noSlot_Text);
        if (partSO.PartType == PBPartType.Body || partSO.PartType == PBPartType.Wheels)
        {
            if (partSO.IsEquipped == true)
            {
                return;
            }
        }

        else
        {
            if (partSO.IsEquipped == true)
            {
                if (isSameSlot)
                {
                    equipBtn.interactable = true;
                }
                else
                {
                    equipBtn.image.DOFade(1, configSO.amimationDuration);
                }
                return;
            }
        }
        equipBtn.interactable = true;
    }

    public void CheckUpgradeButton()
    {
        if (partSO == null) return;

    }
    public void OnNotEnoughPower()
    {
        if (!isSelected) return;
        StartCoroutine(CR_OnNotEnoughPower());
    }

    IEnumerator CR_OnNotEnoughPower()
    {
        energyText.DOColor(Color.red, 1.5f);
        energyBoxTransform.transform.DOPunchPosition(Vector2.right * 8f, 1.5f, 15);
        yield return new WaitForSeconds(3);
        energyText.DOColor(Color.white, 1.5f);
    }

    public void OnSwapEvent()
    {
        IsSameSlot();
        //if (partSO.IsEquipped && !isSameSlot)
        //{
        //    swapBtn.gameObject.SetActive(true);
        //    equipBtn.gameObject.SetActive(false);
        //}
        //else
        //{
        //    swapBtn.gameObject.SetActive(false);
        //    equipBtn.gameObject.SetActive(true);
        //}

        //if (partSO.PartType == PBPartType.Upper)
        //{
        //    equipBtn.enabled = true;
        //    equipBtn.interactable = true;
        //    equipText.SetText("UNEQUIP");
        //    equipBtn.image.material = null;
        //}

        //if (partSO.PartType == PBPartType.Wheels)
        //{
        //    swapBtn.gameObject.SetActive(false);
        //    equipBtn.gameObject.SetActive(true);
        //    equipBtn.interactable = false;
        //    equippedOutline.enabled = true;
        //    equippedOutline.gameObject.SetActive(true);
        //    equipText.SetText("EQUIPPED");
        //    equipBtn.image.material = grayScale;

        //    ConvertTextEquiped("USED");
        //}
    }

    public void SwapCondition()
    {
        //if (partSO.PartType == PBPartType.Upper || partSO.PartType == PBPartType.Wheels)
        //{
        //    IsSameSlot();
        //    CheckSwapCondition();
        //    if (partSO.IsEquipped && !isSameSlot)
        //    {
        //        swapBtn.gameObject.SetActive(true);
        //        equipBtn.gameObject.SetActive(false);

        //        swapBtn.image.DOFade(1, configSO.amimationDuration).OnComplete(() => swapBtn.interactable = true);
        //        SwapGearButton swapGearButton = swapBtn.GetComponent<SwapGearButton>();
        //        if (swapGearButton != null)
        //        {
        //            CurrentGearHolder.Instance.swapSO = partSO;
        //        }

        //        HandleButtonWhenEquipedSpecial();
        //        return;
        //    }
        //}
    }

    void CheckSwapCondition()
    {
        if (partSlot == PBPartSlot.Wheels_1)
        {
            CurrentGearHolder.Instance.currentSlot = partSlot;
            CurrentGearHolder.Instance.swapSlot = PBPartSlot.Wheels_2;
        }
        if (partSlot == PBPartSlot.Wheels_2)
        {
            CurrentGearHolder.Instance.currentSlot = partSlot;
            CurrentGearHolder.Instance.swapSlot = PBPartSlot.Wheels_1;
        }
        if (partSlot == PBPartSlot.Upper_1)
        {
            CurrentGearHolder.Instance.currentSlot = partSlot;
            CurrentGearHolder.Instance.swapSlot = PBPartSlot.Upper_2;
        }
        if (partSlot == PBPartSlot.Upper_2)
        {
            CurrentGearHolder.Instance.currentSlot = partSlot;
            CurrentGearHolder.Instance.swapSlot = PBPartSlot.Upper_1;
        }
    }

    private void OnClickClaimBossBtn()
    {
        GameEventHandler.Invoke(BossFightEventCode.OnUnlockBoss, true);
        GameEventHandler.AddActionEvent(BossFightEventCode.OnClaimBossComplete, OnClaimBossComplete);
    }

    private void OnClaimBossComplete()
    {
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnClaimBossComplete, OnClaimCompleteRV);
        OnClaimCompleteRV();
    }

    private void OnClaimCompleteRV()
    {
        if (isSelected)
        {
            equipBtn.gameObject.SetActive(true);
            claimRVButton.gameObject.SetActive(false);
        }
    }

    private void OnAutoOpenGearinfo(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;

        PBPartSO pbpartAuto = parameters[0] as PBPartSO;
        if (partSO == pbpartAuto)
        {
            Select();
            upgradeBtn.onClick.Invoke();
        }
    }

    private void OnClosePopupEvent(params object[] parameters)
    {
        if (parameters.Length <= 0 || parameters[0] == null) return;

        PBPartSO pbPartSO = (PBPartSO)parameters[0];
        if (pbPartSO == partSO)
            OnEquipEvent();
    }

    // public void LoadEnergyUI(bool isCharging)
    // {
    //     if (energyProgressBarUI != null)
    //         energyProgressBarUI.gameObject.SetActive(false);

    //     if (energyProgressBarUI == null) return;
    //     if (partSO is PBChassisSO pBChassisSO)
    //     {
    //         if (!pBChassisSO.IsSpecial) return;

    //         if (pBChassisSO.TryGetModule<EnergyItemModule>(out var v))
    //         {
    //             if (v.IsFullEnergy())
    //                 v.currentEnergy = energyProgressBarUI.NodeEnergies.Count;
    //         }

    //         footer.SetActive(!pBChassisSO.IsSpecial);
    //         energyProgressBarUI.gameObject.SetActive(pBChassisSO.IsSpecial);
    //         energyProgressBarUI.Setup(pBChassisSO, isCharging);

    //         if (isSelected)
    //         {
    //             if (pBChassisSO.TryGetModule<EnergyItemModule>(out var energyItemModule))
    //             {
    //                 equipBtn.gameObject.SetActive(!energyItemModule.IsOutOfEnergy());
    //                 refillEnergyRV.gameObject.SetActive(energyItemModule.IsOutOfEnergy());
    //             }
    //         }
    //     }
    // }

    // It should be show power limit FTUE, it might cause a bit confuse with other feature energy
    private void OnShowEnergyFTUE()
    {
        bool isCanShow = false;
        if (m_GearListSelection != null)
        {
            for (int i = 0; i < m_GearListSelection.UnlockearButtons.Count; i++)
            {
                if (m_GearListSelection.UnlockearButtons[i] != null)
                {
                    if (m_GearListSelection.UnlockearButtons[i] == this)
                        isCanShow = true;
                }
            }
        }

        if (isCanShow)
        {
            if (m_GearListSelection != null)
            {
                if (gameObject.GetComponent<RectTransform>().position.y < m_GearListSelection.OverUpPanel.position.y
                    && gameObject.GetComponent<RectTransform>().position.y > m_GearListSelection.OverDownPanel.position.y)
                {
                    energyBoxBreakthingAnimation.DOPlay();
                    Canvas energyCanvas = energyBox.AddComponent<Canvas>();
                    energyCanvas.overrideSorting = true;
                    energyCanvas.sortingOrder = 9000;
                }
            }
        }
    }

    // It should be show power limit FTUE, it might cause a bit confuse with other feature energy
    private void OnFinishShowEnergyFTUE()
    {
        energyBoxBreakthingAnimation.DOKill();
        Canvas energyCanvas = energyBox.GetComponent<Canvas>();
        if (energyCanvas != null)
            Destroy(energyCanvas);
    }

    #region FTUE
    public void StartFTUEEquipUpper()
    {
        if (partSlot.GetPartTypeOfPartSlot() == HandleFTUECharacterUI.partType_Equip_2 && partSO.IsUnlocked())
        {
            ftueHand.SetActive(true);
        }
    }
    #endregion
}

public enum GearButtonEvent
{
    EquipedSpecial,
    OpenPopupInfo,
}

