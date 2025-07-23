using HyrphusQ.Events;
using LatteGames;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Linq;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using HyrphusQ.SerializedDataStructure;
using System.Globalization;

public class GearInfoPopup : Singleton<GearInfoPopup>
{
    [BoxGroup("Public Ref")] public bool isShowing;
    [BoxGroup("Public Ref")] public bool hasSwap;

    private const string moneyIcon = "<size=60><voffset=-5><sprite name=Standard></voffset></size>";

    [SerializeField, BoxGroup("Property")] private float equipSlotAndchorPosYOn;
    [SerializeField, BoxGroup("Property")] private float equipSlotAndchorPosYOff;
    [SerializeField, BoxGroup("Property")] private float equipSlotSizeDeltaYOn;
    [SerializeField, BoxGroup("Property")] private float equipSlotSizeDeltaYOff;

    [SerializeField, BoxGroup("Ref")] private GameObject btnHolderEquip;
    [SerializeField, BoxGroup("Ref")] private GameObject btnHolderUpgrade;
    [SerializeField, BoxGroup("Ref")] private GameObject equipSlotHolder;
    [SerializeField, BoxGroup("Ref")] private EquipGearButton EquipSlot_1_ButtonDefault;
    [SerializeField, BoxGroup("Ref")] private EquipGearButton EquipSlot_2_ButtonDefault;
    [SerializeField, BoxGroup("Ref")] private EquipGearButton EquipSlot_1_Button;
    [SerializeField, BoxGroup("Ref")] private EquipGearButton EquipSlot_2_Button;
    [SerializeField, BoxGroup("Ref")] private RectTransform equipPanel;
    [SerializeField, BoxGroup("Ref")] private Button swapButton;
    [SerializeField, BoxGroup("Ref")] private Button openEquipSlotBtn;
    [SerializeField, BoxGroup("Ref")] private Button closeBtn;
    [SerializeField, BoxGroup("Ref")] private TMP_Text equipListSlotText;
    [SerializeField, BoxGroup("Ref")] private TMP_Text equipDefault_1_Text;
    [SerializeField, BoxGroup("Ref")] private TMP_Text equipDefault_2_Text;
    [SerializeField, BoxGroup("Ref")] private Material grayScale;
    [SerializeField, BoxGroup("Ref")] private SkinListView skinListView;
    [Header("Properties")]
    [SerializeField] List<TextMeshProUGUI> gearNames;
    [SerializeField] TextMeshProUGUI upgradeCost;
    [SerializeField] TextMeshProUGUI maxLevel;
    [SerializeField] GameObject requirementText;
    [SerializeField] Image gearRarity;
    [SerializeField] GearButton gearButton;
    [SerializeField] EquipGearButton equipGearButton;
    [SerializeField] UpgradeGearButton upgradeGearButton;
    [SerializeField] Button getCardsButton;
    [SerializeField] Button claimRVButton;
    [SerializeField] GearStatUI hpStatUI;
    [SerializeField] GearStatUI atkStatUI;
    [SerializeField] GearStatUI energyStatUI;
    [SerializeField] GearStatUI resistanceStatUI;
    [SerializeField] GearStatUI agilityStatUI;
    [SerializeField] GearStatUI speedStatUI;
    [SerializeField] GearStatUI rangeStatUI;
    [SerializeField] GearStatUI reloadStatUI;
    [SerializeField] TextMeshProUGUI gearDescription;

    [SerializeField, BoxGroup("PSFTUE")] private GameObject m_FTUEHandle_1;
    [SerializeField, BoxGroup("PSFTUE")] private PSFTUESO m_PSFTUESO;

    [Space]
    [Header("Sub-Properties")]
    [SerializeField] SerializedDictionary<RarityType, GameObject> rarityLabels;

    private CanvasGroupVisibility canvasGroupVisibility;
    private RectTransform rectTransform;
    private PBPartSO partSO;
    private GearButton mainGearButton;

    private int slotIndex;
    private bool isCallFromGearButton;
    private bool isMoreThanOneSlot;
    private bool isDisableEquip;
    private bool hasNoSlot;
    private bool isSelectEquipSlot = false;
    private float originalEquipPanelX;
    private string equipSlot_Text = "EQUIP";
    private string equippedSlot_Text = "EQUIPPED";
    private string unEquippedSlot_Text = "UNEQUIP";
    private string noSlot_Text = "NO SLOT";
    private string used_Text = "USED";
    private string use_Text = "USE";

    protected override void Awake()
    {
        base.Awake();
        canvasGroupVisibility = GetComponent<CanvasGroupVisibility>();
        rectTransform = GetComponent<RectTransform>();
        openEquipSlotBtn.onClick.AddListener(OpenEquipSlot);
        closeBtn.onClick.AddListener(CloseButtonClick);
        originalEquipPanelX = equipPanel.sizeDelta.x;
        getCardsButton.onClick.AddListener(OnGetCardsButtonClicked);
    }

    private void Start()
    {
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnSendBotInfo, GetInfoSpecial);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartCardChanged, UpdateCardsData);
        GameEventHandler.AddActionEvent(PartManagementEventCode.OnPartUpgraded, HandlePartUpgraded);
        GameEventHandler.AddActionEvent(CharacterUIEventCode.OnGearCardInfoPopUpShow, GetPartSO);
        GameEventHandler.AddActionEvent(BossFightEventCode.OnClaimBossComplete, OnClaimCompleteRV);
        claimRVButton.onClick.AddListener(OnClickClaimBossBtn);
        EquipSlot_1_ButtonDefault.EquipButton.onClick.AddListener(HandleEquipButtonClickedGearSlot);
        EquipSlot_2_ButtonDefault.EquipButton.onClick.AddListener(HandleEquipButtonClickedGearSlot);
        EquipSlot_1_Button.EquipButton.onClick.AddListener(() => { slotIndex = 0; HandleEquipButtonClickedGearSlot(); });
        EquipSlot_2_Button.EquipButton.onClick.AddListener(() => { slotIndex = 1; HandleEquipButtonClickedGearSlot(); });
        gearButton.OnEquipAction.AddListener(OnGearCardEquip);

        // if (refillRV != null)
        //     refillRV.OnRewardGranted += RefillRV_OnRewardGranted;
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnSendBotInfo, GetInfoSpecial);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartCardChanged, UpdateCardsData);
        GameEventHandler.RemoveActionEvent(PartManagementEventCode.OnPartUpgraded, HandlePartUpgraded);
        GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearCardInfoPopUpShow, GetPartSO);
        GameEventHandler.RemoveActionEvent(BossFightEventCode.OnClaimBossComplete, OnClaimCompleteRV);
        openEquipSlotBtn.onClick.RemoveListener(OpenEquipSlot);
        closeBtn.onClick.RemoveListener(CloseButtonClick);
        claimRVButton.onClick.RemoveListener(OnClickClaimBossBtn);
        EquipSlot_1_ButtonDefault.EquipButton.onClick.RemoveListener(HandleEquipButtonClickedGearSlot);
        EquipSlot_2_ButtonDefault.EquipButton.onClick.RemoveListener(HandleEquipButtonClickedGearSlot);
        EquipSlot_1_Button.EquipButton.onClick.RemoveAllListeners();
        EquipSlot_2_Button.EquipButton.onClick.RemoveAllListeners();
        gearButton.OnEquipAction.RemoveListener(OnGearCardEquip);

        // if (refillRV != null)
        //     refillRV.OnRewardGranted -= RefillRV_OnRewardGranted;
    }

    private void OnGetCardsButtonClicked()
    {
        PBUpgradePopup.Instance.Show(partSO);
    }

    private void HandlePartUpgraded()
    {
        UpdateStatsData();
        UpdateUpgradeData();
        // DefaultToStatPage();
    }

    private void GetPartSO(params object[] parameters)
    {
        PBPartSO c = parameters[0] as PBPartSO;
        if (c == null) return;
        // _footerLayoutGroup.spacing = -130;

        partSO = (PBPartSO)parameters[0];

        if (parameters[2] != null)
            mainGearButton = (GearButton)parameters[2];

        isDisableEquip = (bool)parameters[3];

        isMoreThanOneSlot = (bool)parameters[5];
        slotIndex = (int)parameters[6];

        EquipGearButton equipGearCardSlot1 = null;
        EquipGearButton equipGearCardSlot2 = null;
        if (parameters[7] != null)
            equipGearCardSlot1 = (EquipGearButton)parameters[7];

        if (parameters[8] != null)
            equipGearCardSlot2 = (EquipGearButton)parameters[8];

        isCallFromGearButton = parameters[7] != null && parameters[8] != null;

        // Helper method to set up equip slot buttons
        void SetupEquipSlotButton(EquipGearButton source, EquipGearButton buttonEquipDefault, EquipGearButton buttonEquipSlot, int indexSlot)
        {
            if (source != null)
            {
                buttonEquipDefault.partSlot = source.partSlot;
                buttonEquipDefault.partSO = source.partSO;
                buttonEquipDefault.gearButton = source.gearButton;

                buttonEquipSlot.partSlot = source.partSlot;
                buttonEquipSlot.partSO = source.partSO;
                buttonEquipSlot.gearButton = source.gearButton;
            }
            else
            {
                gearButton.PartSlot = GearSlotHelper.GetSlotPartType(gearButton.PartSO.PartType, indexSlot);
                buttonEquipDefault.partSlot = gearButton.PartSlot;
                buttonEquipDefault.partSO = gearButton.PartSO;
                buttonEquipDefault.gearButton = gearButton;

                buttonEquipSlot.partSlot = gearButton.PartSlot;
                buttonEquipSlot.partSO = gearButton.PartSO;
                buttonEquipSlot.gearButton = gearButton;
            }
        }


        if (mainGearButton != null)
            mainGearButton.OnEquipAction.AddListener(OnGearCardEquip);

        ObjectFindCache<GearTabSelection>.Get().Hide();
        canvasGroupVisibility.Show();
        isShowing = true;
        GameEventHandler.Invoke(CharacterUIEventCode.OnShowPopup);
        rectTransform.DOLocalMoveY(0, AnimationDuration.TINY);
        //EnableSkillPage(c is PBChassisSO chassisSO && chassisSO.ActiveSkillSO != null);
        UpdateData();

        // Setup equip buttons with data from equipGearCardSlot1 and equipGearCardSlot2
        gearButton.SetUpSlotPartEquipButton();
        SetupEquipSlotButton(equipGearCardSlot1, EquipSlot_1_ButtonDefault, EquipSlot_1_Button, 0);
        SetupEquipSlotButton(equipGearCardSlot2, EquipSlot_2_ButtonDefault, EquipSlot_2_Button, 1);

        StartCoroutine(CR_CheckEquip(c.PartType));
        if (c.PartType != PBPartType.Body)
        {
            if ((bool)parameters[3])
            {
                hasNoSlot = true;

                gearButton.equipText.SetText(noSlot_Text);
            }
            else
            {
                hasNoSlot = false;
            }
        }
        else
        {
            if (partSO.IsEquipped)
            {
                gearButton.equipBtn.interactable = false;
            }
        }
        gearButton.SwapCondition();
        gearButton.isSameSlot = (bool)parameters[4];
        if (!FTUEMainScene.Instance.FTUEUpgrade.value)
            GameEventHandler.RemoveActionEvent(CharacterUIEventCode.OnGearCardInfoPopUpShow, GetPartSO);

        if (c as PBChassisSO)
        {
            if (c.Cast<PBChassisSO>().IsSpecial)
            {
                // _footerLayoutGroup.spacing = -100;

                if (_hpTotalSpecial > 0)
                {
                    hpStatUI.SetStat(_hpTotalSpecial);
                    hpStatUI.gameObject.SetActive(true);
                }
                else
                {
                    hpStatUI.gameObject.SetActive(false);
                }

                if (_atkTotalSpecial > 0)
                {
                    atkStatUI.SetStat(_atkTotalSpecial);
                    atkStatUI.gameObject.SetActive(true);
                }
                else
                {
                    atkStatUI.gameObject.SetActive(false);
                }
            }
        }
        // gearButton.LoadEnergyUI(false);

        if (m_PSFTUESO.FTUEOpenBuildUI.value && !m_PSFTUESO.FTUEOpenInfoPopup.value && m_PSFTUESO.WeaponUpgradeFTUEPPref == partSO)
        {
            EquipSlot_2_ButtonDefault.EquipButton.enabled = false;
            m_FTUEHandle_1.SetActive(true);
            GameEventHandler.Invoke(PSFTUEBlockAction.Block, this.gameObject, PSFTUEBubbleText.None);
            upgradeGearButton.OnCompletedUpgrade += OnFTUEInfoUpgradePopup;

            closeBtn.enabled = false;
            EquipSlot_1_Button.EquipButton.enabled = false;
            EquipSlot_2_Button.EquipButton.enabled = false;
            equipSlotHolder.GetComponent<Button>().enabled = false;
        }

        void OnFTUEInfoUpgradePopup()
        {
            if (!m_PSFTUESO.FTUEOpenInfoPopup.value)
            {
                EquipSlot_2_ButtonDefault.EquipButton.enabled = true;
                m_FTUEHandle_1.SetActive(false);
                m_PSFTUESO.FTUEOpenInfoPopup.value = true;
                GameEventHandler.Invoke(PSFTUEBlockAction.Unblock, this.gameObject);
                closeBtn.onClick?.Invoke();

                closeBtn.enabled = true;
                EquipSlot_1_Button.EquipButton.enabled = true;
                EquipSlot_2_Button.EquipButton.enabled = true;
                equipSlotHolder.GetComponent<Button>().enabled = true;

                FTUEMainButton mainButton = FindObjectOfType<FTUEMainButton>();
                if (mainButton != null)
                    mainButton.FTUE2ndMath();

                GameEventHandler.Invoke(PSLogFTUEEventCode.EndClickUpgrade);
                GameEventHandler.Invoke(PSLogFTUEEventCode.Start2ndMatch);
            }
            upgradeGearButton.GetComponent<Button>().onClick.RemoveListener(OnFTUEInfoUpgradePopup);
        }
    }

    private float _hpTotalSpecial;
    private float _atkTotalSpecial;
    private void GetInfoSpecial(params object[] parameters)
    {
        if (parameters.Length <= 0) return;
        _hpTotalSpecial = (float)parameters[0];
        _atkTotalSpecial = (float)parameters[1];
    }
    private void ConvertEquipText(string text)
    {
        if (partSO as PBChassisSO)
        {
            if (partSO.Cast<PBChassisSO>().IsSpecial)
                gearButton.equipText.SetText(text);
        }
        else
        {
            bool hasSpecial = gearButton.SpecialSO.initialValue.Any(v => (v as PBChassisSO).IsEquipped && partSO.PartType == PBPartType.Wheels);
            if (hasSpecial)
                gearButton.equipText.SetText(noSlot_Text);
        }
    }
    private IEnumerator CR_CheckEquip(PBPartType partType)
    {
        yield return new WaitForSeconds(0.251f);
        if (partSO.IsEquipped == true)
        {
            if (partType == PBPartType.Wheels || partType == PBPartType.Body)
            {
                gearButton.equipText.SetText(equippedSlot_Text);
                ConvertEquipText(used_Text);

                gearButton.equipBtn.image.material = gearButton.GrayScaleMat;
                gearButton.equipBtn.interactable = false;
            }
            if (!gearButton.isSameSlot)
            {
                gearButton.equipText.SetText(equippedSlot_Text);
                ConvertEquipText(used_Text);

                gearButton.equipBtn.image.material = gearButton.GrayScaleMat;
                gearButton.equipBtn.interactable = false;
            }
        }
    }

    private void UpdateData()
    {
        if (partSO == null) return;
        Button equipButton;
        equipButton = equipGearButton.GetComponent<Button>();
        equipButton.image.material = null;

        NameItemModule nameItemModule = partSO.GetModule<NameItemModule>();
        RarityItemModule rarityItemModule = partSO.GetModule<RarityItemModule>();
        DescriptionItemModule descriptionItemModule = partSO.GetModule<DescriptionItemModule>();

        UpdateStatsData();
        // SetSkillPageData();
        // DefaultToStatPage();

        gearDescription.SetText(descriptionItemModule.Description);
        var currentPower = partSO.GetPower().value;
        if (currentPower > 0)
        {
            energyStatUI.SetStat(currentPower);
            energyStatUI.SetUpgradable(false);
        }
        else
        {
            energyStatUI.gameObject.SetActive(false);
        }

        foreach (var gearName in gearNames)
        {
            gearName.SetText(nameItemModule.displayName);
        }

        var resistance = partSO.GetStat(PBStatID.Resistance);
        if (resistance > 0)
        {
            resistanceStatUI.SetStat($"{resistance * 100} %");
            resistanceStatUI.gameObject.SetActive(true);
        }
        else
        {
            resistanceStatUI.gameObject.SetActive(false);
        }

        var turning = partSO.GetStat(PBStatID.Turning);
        if (turning > 0)
        {
            agilityStatUI.SetStat(turning.ToString());
            agilityStatUI.gameObject.SetActive(true);
        }
        else
        {
            agilityStatUI.gameObject.SetActive(false);
        }

        var speed = (partSO is PBWheelSO wheelSO) ? wheelSO.Speed : ((partSO is PBChassisSO chassisSO) ? chassisSO.Speed : 0);
        if (speed > 0)
        {
            speedStatUI.gameObject.SetActive(true);
            speedStatUI.SetStat(speed.ToString("F2", CultureInfo.InvariantCulture));
        }
        else
        {
            speedStatUI.gameObject.SetActive(false);
        }

        var part = partSO.GetModelPrefab<PBPart>();
        var gunBase = part.GetComponent<GunBase>();
        if (gunBase != null)
        {
            rangeStatUI.gameObject.SetActive(true);
            rangeStatUI.SetStat(gunBase.aimRange.ToString());
            reloadStatUI.gameObject.SetActive(true);
            reloadStatUI.SetStat(gunBase.reload.ToString("F2", CultureInfo.InvariantCulture));
        }
        else
        {
            rangeStatUI.gameObject.SetActive(false);
            reloadStatUI.gameObject.SetActive(false);
        }

        foreach (var label in rarityLabels)
        {
            label.Value.SetActive(rarityItemModule.rarityType == label.Key);
        }

        gearButton.PartSO = partSO;
        upgradeGearButton.partSO = partSO;

        if (!hasNoSlot)
        {
            gearButton.OnEquipEvent();
            gearButton.CheckEquipButton();
        }
        else
        {
            gearButton.equipText.SetText(noSlot_Text);
        }

        upgradeGearButton.AddEvent();

        UpdateUpgradeData();

        // if (refillRV != null)
        //     refillRV.gameObject.SetActive(false);
        if (partSO is PBChassisSO pBChassisSO && pBChassisSO.IsSpecial) // Special
        {
            if (!pBChassisSO.IsClaimedRV)
            {
                claimRVButton.gameObject.SetActive(true);
                gearButton.swapBtn.gameObject.SetActive(false);
                gearButton.equipBtn.gameObject.SetActive(false);
                btnHolderEquip.gameObject.SetActive(false);
                btnHolderUpgrade.gameObject.SetActive(false);
            }
            else
            {
                claimRVButton.gameObject.SetActive(false);
                gearButton.swapBtn.gameObject.SetActive(true);
                gearButton.equipBtn.gameObject.SetActive(true);
                btnHolderEquip.gameObject.SetActive(true);
                btnHolderUpgrade.gameObject.SetActive(true);
                var equipBtnRectTransform = gearButton.equipBtn.transform.parent as RectTransform;
                equipBtnRectTransform.sizeDelta = new Vector3(645f, equipBtnRectTransform.sizeDelta.y);

                // if (refillRV != null)
                // {
                //     if (pBChassisSO.TryGetModule<EnergyItemModule>(out var energyItemModule))
                //     {
                //         refillRV.gameObject.SetActive(energyItemModule.IsOutOfEnergy());
                //         btnHolderEquip.gameObject.SetActive(!energyItemModule.IsOutOfEnergy());
                //         btnHolderUpgrade.gameObject.SetActive(!energyItemModule.IsOutOfEnergy());
                //         gearButton.swapBtn.gameObject.SetActive(!energyItemModule.IsOutOfEnergy());
                //         gearButton.equipBtn.gameObject.SetActive(!energyItemModule.IsOutOfEnergy());
                //     }
                // }
            }
            gearButton.upgradeBtn.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            claimRVButton.gameObject.SetActive(false);
            gearButton.equipBtn.gameObject.SetActive(true);
            gearButton.swapBtn.gameObject.SetActive(true);
            btnHolderEquip.gameObject.SetActive(true);
            btnHolderUpgrade.gameObject.SetActive(true);
        }

        HandleEquipButton();
        // gearButton.LoadEnergyUI(false);
    }

    private void UpdateStatsData()
    {
        var isEnoughCardToUpgrade = partSO.IsEnoughCardToUpgrade();
        var currentHp = partSO.CalCurrentHp();
        var nextHp = partSO.CalHpByLevel(partSO.GetCurrentUpgradeLevel() + 1);
        if (currentHp > 0 || nextHp > 0)
        {
            hpStatUI.gameObject.SetActive(true);
            hpStatUI.SetUpgradable(isEnoughCardToUpgrade);
            if (isEnoughCardToUpgrade)
                hpStatUI.SetStat(currentHp, nextHp);
            else
                hpStatUI.SetStat(currentHp);
        }
        else
        {
            hpStatUI.gameObject.SetActive(false);
        }

        var currentAtk = partSO.CalCurrentAttack();
        var nextAtk = partSO.CalAttackByLevel(partSO.GetCurrentUpgradeLevel() + 1);

        // var currentAtk = partSO.CalCurrentAttack() * partSO.Stats.DamagePerHitRatio;
        // var nextAtk = partSO.CalAttackByLevel(partSO.GetCurrentUpgradeLevel() + 1) * partSO.Stats.DamagePerHitRatio;

        // var part = partSO.GetModelPrefab<PBPart>();
        // var gunBase = part.GetComponent<ContinuousGunBase>();
        // if (gunBase != null)
        // {
        //     currentAtk = currentAtk * (gunBase.reload + gunBase.playDuration);
        //     nextAtk = nextAtk * (gunBase.reload + gunBase.playDuration);
        // }

        if (currentAtk > 0 || nextAtk > 0)
        {
            atkStatUI.gameObject.SetActive(true);
            atkStatUI.SetUpgradable(isEnoughCardToUpgrade);
            if (isEnoughCardToUpgrade)
                atkStatUI.SetStat(currentAtk, nextAtk);
            else
                atkStatUI.SetStat(currentAtk);
        }
        else
        {
            atkStatUI.gameObject.SetActive(false);
        }
    }

    private void UpdateCardsData()
    {
        if (partSO == null)
        {
            return;
        }
        // if (partSO.IsEnoughCardToUpgrade())
        // {
        //     gearButton.upgradeBtn.image.material = null;
        //     gearButton.upgradeBtn.interactable = true;
        // }
        // else
        // {
        //     gearButton.upgradeBtn.image.material = gearButton.grayScale;
        //     gearButton.upgradeBtn.interactable = false;
        // }
        if (partSO.IsEnoughCardToUpgrade())
        {
            getCardsButton.gameObject.SetActive(false);
            gearButton.upgradeBtn.gameObject.SetActive(true);
        }
        else
        {
            getCardsButton.gameObject.SetActive(true);
            gearButton.upgradeBtn.gameObject.SetActive(false);
        }
        UpdateStatsData();
    }

    private void UpdateUpgradeData()
    {
        UpdateCardsData();
        if (partSO.IsMaxUpgradeLevel())
        {
            getCardsButton.gameObject.SetActive(false);
            maxLevel.gameObject.SetActive(true);
            requirementText.SetActive(false);

            gearButton.upgradeBtn.gameObject.SetActive(true);
            gearButton.upgradeBtn.interactable = false;
            gearButton.upgradeBtn.image.material = gearButton.GrayScaleMat;
        }
        else
        {
            if (partSO.TryGetCurrentUpgradeRequirement(out Requirement_Currency requirement_Currency))
                upgradeCost.SetText($"{moneyIcon}{requirement_Currency.requiredAmountOfCurrency}");
            maxLevel.gameObject.SetActive(false);
            requirementText.SetActive(true);

            gearButton.upgradeBtn.interactable = true;
            gearButton.upgradeBtn.image.material = null;
        }
    }

    public void OnClose()
    {
        if (!FTUEMainScene.Instance.FTUEUpgrade.value) return;
        GameEventHandler.Invoke(CharacterUIEventCode.PureOnClosePopup);
        if (!hasNoSlot)
        {
            GameEventHandler.Invoke(CharacterUIEventCode.OnClosePopUp, partSO);
        }
        if (hasSwap)
        {
            if (mainGearButton != null)
                mainGearButton.OnSwapEvent();
        }
        hasNoSlot = false;
        hasSwap = false;
        rectTransform.DOLocalMoveY(-2000f, 0.5f).OnComplete(() => isShowing = false);
        ObjectFindCache<GearTabSelection>.Get().Show();
    }

    private void OnClickClaimBossBtn()
    {
        GameEventHandler.Invoke(BossFightEventCode.OnUnlockBoss, true);
    }

    private void OnClaimCompleteRV()
    {
        gearButton.equipBtn.gameObject.SetActive(true);
        claimRVButton.gameObject.SetActive(false);
        var equipBtnRectTransform = gearButton.equipBtn.transform.parent as RectTransform;
        equipBtnRectTransform.sizeDelta = new Vector3(645f, equipBtnRectTransform.sizeDelta.y);
        UpdateData();
    }

    private void SetEquipButtonText(string content)
    {
        equipDefault_1_Text.SetText(content);
        equipDefault_2_Text.SetText(content);
        EquipSlot_1_ButtonDefault.LoadSpriteButton(equipDefault_1_Text.text == unEquippedSlot_Text);
        EquipSlot_2_ButtonDefault.LoadSpriteButton(equipDefault_2_Text.text == unEquippedSlot_Text);
    }

    private void HandleEquipButton()
    {
        gearButton.equippedOutline.gameObject.SetActive(partSO.IsEquipped);
        SetDefaultButton();

        if (partSO is PBChassisSO pBChassisSO)
        {
            string equipStatusText = partSO.IsEquipped ? equippedSlot_Text : equipSlot_Text;
            SetEquipButtonText(equipStatusText);

            if (pBChassisSO.IsSpecial)
            {
                string equipText = pBChassisSO.IsEquipped ? used_Text : use_Text;
                SetEquipButtonText(equipText);
            }
            EquipSlot_1_ButtonDefault.gameObject.SetActive(true);
            EquipSlot_1_ButtonDefault.EquipButton.interactable = !partSO.IsEquipped;
            EquipSlot_1_ButtonDefault.ImageButton.material = partSO.IsEquipped ? grayScale : null;
        }
        else
        {
            if (isDisableEquip)
            {
                EquipSlot_1_ButtonDefault.gameObject.SetActive(true);
                EquipSlot_1_ButtonDefault.EquipButton.interactable = false;
                EquipSlot_1_ButtonDefault.ImageButton.material = grayScale;
                equipDefault_1_Text.SetText(noSlot_Text);
                return;
            }

            equipSlotHolder.gameObject.SetActive(!partSO.IsEquipped && isMoreThanOneSlot);
            if (partSO.IsEquipped)
            {
                EquipSlot_1_ButtonDefault.gameObject.SetActive(slotIndex == 0);
                EquipSlot_2_ButtonDefault.gameObject.SetActive(slotIndex == 1);

                SetEquipButtonText(unEquippedSlot_Text);
            }
            else
            {
                if (!isMoreThanOneSlot)
                {
                    EquipSlot_1_ButtonDefault.gameObject.SetActive(slotIndex == 0);
                    EquipSlot_2_ButtonDefault.gameObject.SetActive(slotIndex == 1);
                }
                else
                {
                    EquipSlot_1_Button.gameObject.SetActive(true);
                    EquipSlot_2_Button.gameObject.SetActive(true);
                }
                equipListSlotText.SetText(equipSlot_Text);
                SetEquipButtonText(equipSlot_Text);
            }
        }
    }

    private void OpenEquipSlot()
    {
        if (!isSelectEquipSlot)
        {
            equipPanel.DOAnchorPosY(equipSlotAndchorPosYOn, AnimationDuration.TINY);
            equipPanel.DOSizeDelta(new Vector2(originalEquipPanelX, equipSlotSizeDeltaYOn), AnimationDuration.TINY);
            equipPanel.gameObject.SetActive(true);
            EquipSlot_1_Button.gameObject.SetActive(true);
            EquipSlot_2_Button.gameObject.SetActive(true);
            isSelectEquipSlot = true;
            return;
        }

        equipPanel.DOAnchorPosY(equipSlotAndchorPosYOff, AnimationDuration.TINY);
        equipPanel.DOSizeDelta(new Vector2(0, equipSlotSizeDeltaYOff), AnimationDuration.TINY).OnComplete(() =>
        {
            equipPanel.gameObject.SetActive(false);
        });
        isSelectEquipSlot = false;
    }

    private void OnGearCardEquip()
    {
        isSelectEquipSlot = false;
        gearButton.equippedOutline.gameObject.SetActive(partSO.IsEquipped);

        if (partSO is PBChassisSO pBChassisSO)
        {
            string textToDisplay = pBChassisSO.IsSpecial && pBChassisSO.IsEquipped ? used_Text : equippedSlot_Text;
            SetEquipButtonText(textToDisplay);

            EquipSlot_1_ButtonDefault.EquipButton.interactable = false;
            EquipSlot_1_ButtonDefault.ImageButton.material = grayScale;
            return;
        }

        if (partSO.IsEquipped)
        {
            equipPanel.DOAnchorPosY(equipSlotAndchorPosYOff, 0f);
            equipPanel.DOSizeDelta(new Vector2(0, equipSlotSizeDeltaYOff), 0f);

            equipSlotHolder.gameObject.SetActive(false);

            EquipSlot_1_ButtonDefault.gameObject.SetActive(slotIndex == 0);
            EquipSlot_2_ButtonDefault.gameObject.SetActive(slotIndex == 1);

            SetEquipButtonText(unEquippedSlot_Text);
        }
        else
        {
            equipSlotHolder.gameObject.SetActive(isMoreThanOneSlot);

            if (isMoreThanOneSlot)
            {
                openEquipSlotBtn.interactable = true;
                equipPanel.gameObject.SetActive(false);
            }

            SetEquipButtonText(equipSlot_Text);
            equipListSlotText.SetText(equipSlot_Text);
            EquipSlot_1_ButtonDefault.gameObject.SetActive(!isMoreThanOneSlot && slotIndex == 0);
            EquipSlot_2_ButtonDefault.gameObject.SetActive(!isMoreThanOneSlot && slotIndex == 1);
        }
    }

    private void SetDefaultButton()
    {
        EquipSlot_1_ButtonDefault.EquipButton.interactable = true;
        EquipSlot_1_ButtonDefault.ImageButton.material = null;
        EquipSlot_2_ButtonDefault.EquipButton.interactable = true;
        EquipSlot_2_ButtonDefault.ImageButton.material = null;
        EquipSlot_1_ButtonDefault.gameObject.SetActive(false);
        EquipSlot_2_ButtonDefault.gameObject.SetActive(false);
        EquipSlot_1_Button.gameObject.SetActive(false);
        EquipSlot_2_Button.gameObject.SetActive(false);
        swapButton.gameObject.SetActive(false);
        equipSlotHolder.gameObject.SetActive(false);
        equipPanel.gameObject.SetActive(false);
    }

    private void CloseButtonClick()
    {
        skinListView.ResetToLastOwnedSelectedItem();
        isSelectEquipSlot = false;
        equipPanel.DOAnchorPosY(equipSlotAndchorPosYOff, 0);
        equipPanel.DOSizeDelta(new Vector2(0, equipSlotSizeDeltaYOff), 0).OnComplete(() =>
        {
            equipPanel.gameObject.SetActive(false);
        });
        ObjectFindCache<GearTabSelection>.Get().Show();
    }

    private void HandleEquipButtonClickedGearSlot()
    {
        if (isCallFromGearButton) return;
        StartCoroutine(HandleEquipButtonClickedGearSlotCR());
    }

    private IEnumerator HandleEquipButtonClickedGearSlotCR()
    {
        yield return new WaitForSeconds(0.01f);
        OnGearCardEquip();
    }

    // private void RefillRV_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData obj)
    // {
    //     if (gearButton.PartSO is PBChassisSO pBChassisSO)
    //     {
    //         refillRV.gameObject.SetActive(false);
    //         btnHolderEquip.gameObject.SetActive(true);
    //         btnHolderUpgrade.gameObject.SetActive(true);
    //         gearButton.swapBtn.gameObject.SetActive(true);
    //         gearButton.equipBtn.gameObject.SetActive(true);

    //         List<GearButton> gearButtons = new List<GearButton>();
    //         gearButtons.Add(mainGearButton);
    //         gearButtons.Add(gearButton);
    //         GameEventHandler.Invoke(WarningOutOfEnergyState.OpenAutoCharge, pBChassisSO, gearButtons, AdsLocation.RV_Recharge_Boss_Popup);
    //     }
    // }

    // private void DefaultToStatPage()
    // {
    //     if (isShowSkillPage)
    //     {
    //         skillScrollSnap.GoToPanel(0);
    //     }
    //     else
    //     {
    //         skillScrollRect.ScrollToTop();
    //         statPageToggle.isOn = true;
    //     }

    // }

    // private void EnableSkillPage(bool isEnable)
    // {
    //     isShowSkillPage = isEnable;
    //     skillScrollSnap.enabled = isEnable;
    //     pageToggleGroup.gameObject.SetActive(isEnable);
    //     skillScrollRect.horizontal = isEnable;

    // }

    // private void SetSkillPageData()
    // {
    //     if (!isShowSkillPage)
    //     {
    //         return;
    //     }
    //     if (partSO is PBChassisSO chassisSO)
    //     {
    //         skillIcon.sprite = chassisSO.ActiveSkillSO.GetThumbnailImage();
    //         skillDescription.SetText(chassisSO.ActiveSkillSO.GetModule<DescriptionItemModule>().Description);
    //     }
    // }
}