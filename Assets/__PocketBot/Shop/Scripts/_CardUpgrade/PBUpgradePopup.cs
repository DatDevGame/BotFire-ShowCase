using System;
using System.Collections;
using System.Collections.Generic;
using GachaSystem.Core;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using LatteGames;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PBUpgradePopup : Singleton<PBUpgradePopup>
{
    [SerializeField]
    private PBUpgradePopupSaveDataSO upgradePopupSaveDataSO;
    [SerializeField]
    private Button m_CloseButton;
    [SerializeField, BoxGroup("BoxShopGroup")]
    private GameObject m_BoxShopGroupContainer;
    [SerializeField, BoxGroup("BoxShopGroup")]
    private GearButton m_GearButton;
    [SerializeField, BoxGroup("MissingCardsGroup")]
    private GameObject m_MissingCardsGroupContainer;
    [SerializeField, BoxGroup("MissingCardsGroup")]
    private GameObject m_MissingCardRV;
    [SerializeField, BoxGroup("MissingCardsGroup")]
    private GearButton m_MissingCardsGearButton;
    [SerializeField, BoxGroup("MissingCardsGroup")]
    private Image m_MissingCardsPartIcon;
    [SerializeField, BoxGroup("MissingCardsGroup")]
    private Image m_MissingCardsRarityOutline;
    [SerializeField, BoxGroup("MissingCardsGroup")]
    private RVButtonBehavior claimBtn;
    [SerializeField, BoxGroup("MissingCardsGroup")]
    private int getPartMissingLimit = 3;
    [SerializeField, BoxGroup("MissingCardsGroup")]
    private int totalGetPartMissingLimit = 15;
    [SerializeField, BoxGroup("MissingCardsGroup")]
    private TMP_Text m_CardAmount;
    [SerializeField, BoxGroup("MissingCardsGroup")]
    private SerializedDictionary<RarityType, MissingCardData> missingCardDataMap;
    [SerializeField, BoxGroup("MissingCardsGroup/IAP")]
    private LG_IAPButton m_IAPButton;
    [SerializeField, BoxGroup("MissingCardsGroup/IAP")]
    private Image m_IAPMissingCardsPartIcon;
    [SerializeField, BoxGroup("MissingCardsGroup/IAP")]
    private Image m_IAPMissingCardsRarityOutline;
    [SerializeField, BoxGroup("MissingCardsGroup/IAP")]
    private TMP_Text m_IAPCardAmount;


    private PBPartSO currentPartSO;
    private int getMissingCardAmount;
    private int getMissingCardAmountIAP;
    private IUIVisibilityController m_VisibilityController;
    private IUIVisibilityController visibilityController
    {
        get
        {
            if (m_VisibilityController == null)
            {
                m_VisibilityController = GetComponentInChildren<IUIVisibilityController>();
            }
            return m_VisibilityController;
        }
    }

    private void Start()
    {
        m_CloseButton.onClick.AddListener(Hide);
        claimBtn.OnRewardGranted += OnRewardGranted;
        GameEventHandler.AddActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseItemCompleted);
    }

    private void OnDestroy()
    {
        m_CloseButton.onClick.RemoveListener(Hide);
        claimBtn.OnRewardGranted -= OnRewardGranted;
        GameEventHandler.RemoveActionEvent(IAPEventCode.OnPurchaseItemCompleted, OnPurchaseItemCompleted);
    }

    private void OnPurchaseItemCompleted(object[] parameters)
    {
        if (parameters == null || parameters.Length <= 0)
            return;
        var iapBuyButton = parameters[0] as LG_IAPButton;

        if (m_IAPButton == iapBuyButton)
        {
            var gachaItemCards = new List<GachaCard>();
            gachaItemCards.AddRange(PBGachaCardGenerator.Instance.GenerateRepeat<GachaCard_Part>(getMissingCardAmountIAP, currentPartSO));
            GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, gachaItemCards, null, null);
            HideWhenUpgradable();
        }
    }

    public void Hide()
    {
        visibilityController.Hide();

        #region Design Event
        try
        {
            string popupName = "GetMissingCards";
            string status = DesignEventStatus.Complete;
            string operation = "Manually";
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    public void Show(PBPartSO partSO)
    {
        visibilityController.Show();
        currentPartSO = partSO;
        UpdateView();
        UpdateMissingCardAmount();

        #region Design Event
        try
        {
            string popupName = "GetMissingCards";
            string status = DesignEventStatus.Start;
            string operation = "Manually";
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);

        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    void UpdateView()
    {
        if (upgradePopupSaveDataSO.dayBasedRewardSO.canGetReward)
        {
            upgradePopupSaveDataSO.dayBasedRewardSO.GetReward();
            upgradePopupSaveDataSO.data.gotCardPartGUIDs.Clear();
        }
        var getThisMissingCardsTimes = upgradePopupSaveDataSO.data.gotCardPartGUIDs.FindAll(x => x == currentPartSO.guid);

        var rarityBasedData = missingCardDataMap[currentPartSO.GetRarityType()];

        m_MissingCardsGroupContainer.SetActive(true);
        m_MissingCardRV.SetActive(!(upgradePopupSaveDataSO.data.gotCardPartGUIDs.Count >= totalGetPartMissingLimit || getThisMissingCardsTimes.Count >= getPartMissingLimit));
        m_MissingCardsGearButton.PartSO = currentPartSO;
        m_MissingCardsPartIcon.sprite = currentPartSO.GetThumbnailImage();
        m_IAPMissingCardsPartIcon.sprite = currentPartSO.GetThumbnailImage();
        m_MissingCardsRarityOutline.sprite = rarityBasedData.rarityOutlineSprite;
        m_IAPMissingCardsRarityOutline.sprite = rarityBasedData.rarityOutlineSprite;
    }

    void UpdateMissingCardAmount()
    {
        var rarityBasedData = missingCardDataMap[currentPartSO.GetRarityType()];

        if (currentPartSO.TryGetCurrentUpgradeRequirement(out Requirement_GachaCard requirement))
        {
            getMissingCardAmount = Mathf.CeilToInt(Mathf.Min(rarityBasedData.clamp, rarityBasedData.multiplier * requirement.requiredNumOfCards));
            getMissingCardAmountIAP = Mathf.CeilToInt(requirement.requiredNumOfCards - currentPartSO.GetNumOfCards());
            m_CardAmount.text = $"x{getMissingCardAmount}";
            m_IAPCardAmount.text = $"x{getMissingCardAmountIAP}";
        }
    }

    void OnRewardGranted(RVButtonBehavior.RewardGrantedEventData data)
    {
        #region MonetizationEventCode
        string partName = currentPartSO.GetDisplayName();
        string location = "UpgradePopup";
        GameEventHandler.Invoke(MonetizationEventCode.GetMissingCard, partName, location);
        #endregion

        upgradePopupSaveDataSO.data.gotCardPartGUIDs.Add(currentPartSO.guid);
        var gachaItemCards = new List<GachaCard>();
        gachaItemCards.AddRange(PBGachaCardGenerator.Instance.GenerateRepeat<GachaCard_Part>(getMissingCardAmount, currentPartSO));
        GameEventHandler.Invoke(UnpackEventCode.OnUnpackStart, gachaItemCards, null, null);
        HideWhenUpgradable();
    }

    void HideWhenUpgradable()
    {
        if (currentPartSO.IsEnoughCardToUpgrade())
        {
            Hide();
        }
        else
        {
            UpdateView();
        }
    }
}

[Serializable]
public struct MissingCardData
{
    public float multiplier;
    public int clamp;
    public Sprite rarityOutlineSprite;
}