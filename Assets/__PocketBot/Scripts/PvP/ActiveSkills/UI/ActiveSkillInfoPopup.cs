using System;
using System.Linq;
using HyrphusQ.Events;
using HyrphusQ.GUI;
using LatteGames;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveSkillInfoPopup : ComposeCanvasElementVisibilityController
{
    [SerializeField]
    private TextMeshProUGUI m_SkillNameText;
    [SerializeField]
    private ActiveSkillCardUI m_ActiveSkillCardUI;
    [SerializeField]
    private TextMeshProUGUI m_SkillDescriptionText;
    [SerializeField]
    private ActiveSkillCardRVOffer m_ActiveSkillCardRVOffer;
    [SerializeField]
    private ActiveSkillCardFreeOffer m_ActiveSkillCardFreeOffer;
    [SerializeField]
    private Button m_EquipButton;
    [SerializeField]
    private Image m_EquipBGImage;
    [SerializeField]
    private GrayscaleUI m_EquipGrayscaleUI;
    [SerializeField]
    private TextMeshProUGUI m_EquipText;
    [SerializeField]
    private Color m_NormalCardQuantityTextColor = Color.white, m_ZeroCardQuantityTextColor = Color.red;
    [SerializeField]
    private TextAdapter m_CardQuantityText;
    [SerializeField]
    private ItemManagerSO m_ActiveSkillManagerSO;

    private void Awake()
    {
        m_EquipButton.onClick.AddListener(OnEquipButtonClicked);
        GetOnStartShowEvent().Subscribe(OnStartShow);
        GetOnStartHideEvent().Subscribe(OnStartHide);
        GetOnEndHideEvent().Subscribe(OnEndHide);
        GameEventHandler.AddActionEvent(ActiveSkillManagementEventCode.OnSkillUsed, UpdateView);
        GameEventHandler.AddActionEvent(ActiveSkillManagementEventCode.OnSkillCardChanged, UpdateView);
        ObjectFindCache<ActiveSkillInfoPopup>.Add(this);
    }

    private void Start()
    {
        OnEndHide();
    }

    private void OnDestroy()
    {
        m_EquipButton.onClick.RemoveListener(OnEquipButtonClicked);
        GetOnStartShowEvent().Unsubscribe(OnStartShow);
        GetOnStartHideEvent().Unsubscribe(OnStartHide);
        GetOnEndHideEvent().Unsubscribe(OnEndHide);
        GameEventHandler.RemoveActionEvent(ActiveSkillManagementEventCode.OnSkillUsed, UpdateView);
        GameEventHandler.RemoveActionEvent(ActiveSkillManagementEventCode.OnSkillCardChanged, UpdateView);
        ObjectFindCache<ActiveSkillInfoPopup>.Remove(this);
    }

    private void OnEquipButtonClicked()
    {
        if (m_ActiveSkillCardUI.activeSkillSO == null)
        {
            return;
        }
        // Not enough cards
        if (m_ActiveSkillCardUI.activeSkillSO.GetNumOfCards() <= 0)
        {
            ToastUI.Show("Need at least 1");
            return;
        }
        m_ActiveSkillManagerSO.Use(m_ActiveSkillCardUI.item);
    }

    private void OnStartShow()
    {
        gameObject.SetActive(true);
        ObjectFindCache<GearTabSelection>.Get().Hide();
    }

    private void OnStartHide()
    {
        ObjectFindCache<GearTabSelection>.Get().Show();
    }

    private void OnEndHide()
    {
        gameObject.SetActive(false);
    }

    private void SetEquipButtonInteractable(bool isInteractable, bool isGrayscale, string text)
    {
        m_EquipButton.interactable = isInteractable;
        m_EquipGrayscaleUI.SetGrayscale(isGrayscale);
        m_EquipText.SetText(text);
    }

    [Button]
    public void ShowDetails(ActiveSkillSO activeSkillSO)
    {
        m_ActiveSkillCardUI.Initialize(activeSkillSO, m_ActiveSkillManagerSO);
        m_SkillNameText.SetText(activeSkillSO.GetDisplayName());
        m_SkillDescriptionText.SetText(activeSkillSO.GetModule<DescriptionItemModule>().Description);
        m_ActiveSkillCardRVOffer.Initialize(activeSkillSO);
        UpdateView();
        Show();
    }

    public void UpdateView()
    {
        if (m_ActiveSkillCardUI.item == null)
            return;
        int cardQuantity = Mathf.Min(m_ActiveSkillCardUI.item.GetNumOfCards(), ActiveSkillCardUI.k_MaxCardQuantityDisplay);
        m_CardQuantityText.GetAdapteeText<TMP_Text>().color = cardQuantity > 0 ? m_NormalCardQuantityTextColor : m_ZeroCardQuantityTextColor;
        m_CardQuantityText.SetText(m_CardQuantityText.blueprintText.Replace(Const.StringValue.PlaceholderValue, cardQuantity.ToString()));
        m_ActiveSkillCardUI.UpdateView();
        SetEquipButtonInteractable(
            m_ActiveSkillManagerSO.currentItemInUse != m_ActiveSkillCardUI.item,
            m_ActiveSkillManagerSO.currentItemInUse == m_ActiveSkillCardUI.item || m_ActiveSkillCardUI.activeSkillSO.GetNumOfCards() <= 0,
            m_ActiveSkillManagerSO.currentItemInUse != m_ActiveSkillCardUI.item ? I2LHelper.TranslateTerm(I2LTerm.ButtonTitle_Equip) : I2LHelper.TranslateTerm(I2LTerm.Text_Equipped));
    }

    public void DisableAllButtons()
    {
        GetComponentsInChildren<Button>(true).Except(new Button[] { m_ActiveSkillCardFreeOffer.button, m_EquipButton }).ForEach(button => button.enabled = false);
    }

    public void EnableAllButtons()
    {
        GetComponentsInChildren<Button>(true).ForEach(button => button.enabled = true);
    }

    public void StartTutorialClaimFreeSkillCard(Action onClaimCompleted, Action onEquipCompleted)
    {
        m_ActiveSkillCardRVOffer.gameObject.SetActive(false);
        m_ActiveSkillCardFreeOffer.gameObject.SetActive(true);
        m_ActiveSkillCardFreeOffer.Initialize(m_ActiveSkillCardUI.activeSkillSO, OnClaimCompleted);

        void OnClaimCompleted()
        {
            m_ActiveSkillCardRVOffer.gameObject.SetActive(true);
            m_ActiveSkillCardFreeOffer.gameObject.SetActive(false);
            onClaimCompleted.Invoke();
            StartTutorialEquipSkillCard(onEquipCompleted);
        }
    }

    public void StartTutorialEquipSkillCard(Action onCompleted)
    {
        m_EquipButton.onClick.AddListener(OnEquipClicked);

        void OnEquipClicked()
        {
            m_EquipButton.onClick.RemoveListener(OnEquipClicked);
            onCompleted.Invoke();
        }
    }

    public ActiveSkillCardFreeOffer GetActiveSkillCardFreeOffer()
    {
        return m_ActiveSkillCardFreeOffer;
    }

    public Button GetEquipButton()
    {
        return m_EquipButton;
    }
}