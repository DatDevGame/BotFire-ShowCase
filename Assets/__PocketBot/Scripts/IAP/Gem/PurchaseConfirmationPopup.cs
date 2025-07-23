using System;
using System.Collections;
using System.Collections.Generic;
using HyrphusQ.GUI;
using HyrphusQ.SerializedDataStructure;
using LatteGames;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DrawCardProcedure;
using static PBPackDockSlotInfoUI;

[Serializable]
public class NonBoxPanel
{
    [SerializeField]
    private RectTransform m_PanelRectTransform;
    [SerializeField]
    private TextAdapter m_TitleText;
    [SerializeField]
    private Button m_CloseButton;
    [SerializeField]
    private RectTransform m_ContentContainer;
    [SerializeField]
    private PurchaseConfirmationPopupContent m_ContentPrefab;
    [SerializeField]
    private TextAdapter m_ConfirmPriceText;
    [SerializeField]
    private Button m_ConfirmButton;

    private PurchaseConfirmationPopupContent m_ContentInstance;

    public PurchaseConfirmationPopupContent CreateContent(PartCard partCard)
    {
        m_ContentInstance.Init(partCard);
        return m_ContentInstance;
    }

    public PurchaseConfirmationPopupContent CreateContent(ShopProductSO shopProductSO)
    {
        m_ContentInstance.Init(shopProductSO);
        return m_ContentInstance;
    }

    public void Init(Action onCloseCallback, Action onConfirmCallback)
    {
        m_ContentInstance = GameObject.Instantiate(m_ContentPrefab, m_ContentContainer);
        m_CloseButton.onClick.AddListener(onCloseCallback.Invoke);
        m_ConfirmButton.onClick.AddListener(onConfirmCallback.Invoke);
    }

    public void Show(string title, float amountOfGem, PurchaseConfirmationPopupContent contentInstance)
    {
        m_PanelRectTransform.gameObject.SetActive(true);
        m_ContentInstance = contentInstance;
        m_ContentInstance.transform.SetParent(m_ContentContainer);
        m_TitleText.SetText(title);
        m_ConfirmPriceText.SetText(m_ConfirmPriceText.blueprintText.Replace(Const.StringValue.PlaceholderValue, amountOfGem.ToRoundedText()));
    }

    public void Hide()
    {
        m_PanelRectTransform.gameObject.SetActive(false);
    }
}
[Serializable]
public class BoxPanel
{
    [SerializeField]
    private RectTransform m_PanelRectTransform;
    [SerializeField]
    private Image m_BoxThumbnailImage;
    [SerializeField]
    private TMP_Text m_BoxNameText;
    [SerializeField]
    private Button m_CloseButton;
    [SerializeField]
    private TextMeshProUGUI m_CoinRangeAmountText, m_GemRangeAmountText, m_CardRangeAmountText;
    [SerializeField]
    private RectTransform m_GuaranteedGroupRectTransform;
    [SerializeField]
    private SerializedDictionary<RarityType, RarityGuaranteedCardInfo> m_RarityGuaranteedCardInfoDictionary;
    [SerializeField]
    private VerticalGroupResizer m_VerticalGroupResizer;
    [SerializeField]
    private TextAdapter m_ConfirmPriceText;
    [SerializeField]
    private Button m_ConfirmButton;

    public void Init(Action onCloseCallback, Action onConfirmCallback)
    {
        m_CloseButton.onClick.AddListener(onCloseCallback.Invoke);
        m_ConfirmButton.onClick.AddListener(onConfirmCallback.Invoke);
    }

    public void Show(PBGachaPack gachaPack, float amountOfGem)
    {
        m_PanelRectTransform.gameObject.SetActive(true);
        m_BoxThumbnailImage.sprite = gachaPack.GetThumbnailImage();
        m_BoxNameText.text = gachaPack.GetDisplayName();
        m_CoinRangeAmountText.text = $"{gachaPack.GetOriginalPackMoneyAmountRange().x.RoundToInt().ToRoundedText()} - {gachaPack.GetOriginalPackMoneyAmountRange().y.RoundToInt().ToRoundedText()}";
        m_GemRangeAmountText.text = $"{gachaPack.GetOriginalPackGemAmountRange().x.RoundToInt().ToRoundedText()} - {gachaPack.GetOriginalPackGemAmountRange().y.RoundToInt().ToRoundedText()}";
        m_CardRangeAmountText.text = $"{gachaPack.GetOriginalPackCardCount()}";
        bool isShowGuaranteedGroup = false;
        foreach (var pair in m_RarityGuaranteedCardInfoDictionary)
        {
            var rarity = pair.Key;
            var guaranteedCardCount = gachaPack.GetOriginalPackGuaranteedCardsCount(rarity);
            if (guaranteedCardCount > 0)
            {
                var text = pair.Value.rarityCardAmountTxt;
                text.text = $"x{guaranteedCardCount}";
                pair.Value.groupGO.SetActive(true);
                isShowGuaranteedGroup = true;
            }
            else
            {
                pair.Value.groupGO.SetActive(false);
            }
        }
        m_GuaranteedGroupRectTransform.gameObject.SetActive(isShowGuaranteedGroup);
        m_VerticalGroupResizer.UpdateSize();
        m_ConfirmPriceText.SetText(m_ConfirmPriceText.blueprintText.Replace(Const.StringValue.PlaceholderValue, amountOfGem.ToRoundedText()));
    }

    public void Hide()
    {
        m_PanelRectTransform.gameObject.SetActive(false);
    }
}
public class PurchaseConfirmationPopup : Singleton<PurchaseConfirmationPopup>
{
    [TitleGroup("Non Box Panel"), SerializeField]
    private NonBoxPanel m_NonBoxPanel;
    [TitleGroup("Box Panel"), SerializeField]
    private BoxPanel m_BoxPanel;

    private float m_AmountOfGem;
    private Action<bool> m_ConfirmCallback;

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

    protected override void Awake()
    {
        base.Awake();
        m_NonBoxPanel.Init(OnCloseButtonClicked, OnConfirmButtonClicked);
        m_BoxPanel.Init(OnCloseButtonClicked, OnConfirmButtonClicked);
    }

    private void OnCloseButtonClicked()
    {
        Hide(false);
    }

    private void OnConfirmButtonClicked()
    {
        if (CurrencyManager.Instance[CurrencyType.Premium].IsAffordable(m_AmountOfGem))
        {
            Hide(true);
        }
        else
        {
            //TODO: Hide IAP & Popup
            //IAPGemPackPopup.Instance?.Show();
        }
    }

    public void Hide(bool isConfirm)
    {
        m_ConfirmCallback.Invoke(isConfirm);
        m_ConfirmCallback = delegate { };
        visibilityController.Hide();
    }

    public void Show(string title, float amountOfGem, PurchaseConfirmationPopupContent contentInstance, Action<bool> callback)
    {
        m_AmountOfGem = amountOfGem;
        m_ConfirmCallback = callback;
        m_NonBoxPanel.Show(title, amountOfGem, contentInstance);
        m_BoxPanel.Hide();
        visibilityController.Show();
    }

    public void Show(PBGachaPack gachaPack, float amountOfGem, Action<bool> callback)
    {
        m_AmountOfGem = amountOfGem;
        m_ConfirmCallback = callback;
        m_BoxPanel.Show(gachaPack, amountOfGem);
        m_NonBoxPanel.Hide();
        visibilityController.Show();
    }

    public PurchaseConfirmationPopupContent CreateContent(PartCard partCard)
    {
        return m_NonBoxPanel.CreateContent(partCard);
    }

    public PurchaseConfirmationPopupContent CreateContent(ShopProductSO shopProductSO)
    {
        return m_NonBoxPanel.CreateContent(shopProductSO);
    }
}