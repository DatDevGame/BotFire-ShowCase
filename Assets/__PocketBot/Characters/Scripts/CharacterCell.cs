using DG.Tweening;
using HyrphusQ.Events;
using HyrphusQ.SerializedDataStructure;
using I2.Loc;
using LatteGames.Monetization;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public enum CharacterCellState
{
    Lock,
    Unlocked
}
public enum CharacterCellType
{
    None,
    Currency,
    RV,
    IAP
}
public enum CharacterCellEvent
{
    Select
}
public class CharacterCell : MonoBehaviour
{
    public CharacterCellState State
    {
        get => m_State;
        set
        {
            if (m_State != value)
            {
                m_State = value;
            }
        }
    }

    public CharacterCellType Type
    {
        get => m_Type;
        set
        {
            if (m_Type != value)
            {
                m_Type = value;
            }
        }
    }
    public bool IsPreview
    {
        get => m_IsPreview;
        set
        {
            m_IsPreview = value;
        }
    }
    public bool IsSelected
    {
        get => m_IsSelected;
        set 
        {
            m_IsSelected = value;
        }
    }
    public Action<CharacterCell> OnPreviewAction;
    public Action<CharacterCell> OnSelectAction;
    public CharacterSO CharacterSO => m_CharacterSO;

    [SerializeField, BoxGroup("Ref")] private Image m_Avatar;
    [SerializeField, BoxGroup("Ref")] private RectTransform m_ButtonGroupRect;
    [SerializeField, BoxGroup("Ref")] private Button m_PreviewButton;
    [SerializeField, BoxGroup("Ref")] private Button m_SelectButton;
    [SerializeField, BoxGroup("Ref")] private Button m_CurrentcyButton;
    [SerializeField, BoxGroup("Ref")] private GrayscaleUI m_SelectButtonGrayScale;
    [SerializeField, BoxGroup("Ref")] private GrayscaleUI m_UnlockButtonGrayScale;
    [SerializeField, BoxGroup("Ref")] private RVButtonBehavior m_RVBehavior;
    [SerializeField, BoxGroup("Ref")] private TextMeshProUGUI m_RequiredGemText, m_RequiredGemShadowText, m_SelectText, m_SelectedText;
    [SerializeField, BoxGroup("Ref")] private GameObject m_LockPanel;
    [SerializeField, BoxGroup("Ref")] private GameObject m_CurrencyLabel;
    [SerializeField, BoxGroup("Ref")] private GameObject m_RVLabel;
    [SerializeField, BoxGroup("Ref")] private GameObject m_OutlineNomal;
    [SerializeField, BoxGroup("Ref")] private GameObject m_OutlineButton;
    [SerializeField, BoxGroup("Ref")] private LocalizationParamsManager m_RequiredRVParamsManager;

    [SerializeField, BoxGroup("Resource")] private ResourceLocation m_SinkResourceLocation;

    [ShowInInspector] private CharacterSO m_CharacterSO;
    [ShowInInspector] private CharacterCellState m_State;
    [ShowInInspector] private CharacterCellType m_Type;
    [ShowInInspector] private bool m_IsPreview;
    [ShowInInspector] private bool m_IsSelected;

    private void Awake()
    {
        m_PreviewButton.onClick.AddListener(OnPreviewButton);
        m_SelectButton.onClick.AddListener(OnSelectButton);
        m_CurrentcyButton.onClick.AddListener(OnBuyButton);
        m_RVBehavior.OnRewardGranted += RVBehavior_OnRewardGranted;
    }

    private void OnDestroy()
    {
        m_PreviewButton.onClick.RemoveListener(OnPreviewButton);
        m_SelectButton.onClick.RemoveListener(OnSelectButton);
        m_CurrentcyButton.onClick.RemoveListener(OnBuyButton);
        m_RVBehavior.OnRewardGranted -= RVBehavior_OnRewardGranted;
    }

    private void OnBuyButton()
    {
        if (m_CharacterSO.TryGetUnlockRequirement(out Requirement_Currency currencyRequirement))
        {
            string itemID = $"{transform.GetSiblingIndex() + 1}";
            if (CurrencyManager.Instance.Spend(CurrencyType.Premium, currencyRequirement.requiredAmountOfCurrency, m_SinkResourceLocation, itemID))
            {
                m_CharacterSO.TryUnlockIgnoreRequirement();
                Load();
                Select();
                m_OutlineButton.SetActive(false);
            }
        }
    }
    private void RVBehavior_OnRewardGranted(RVButtonBehavior.RewardGrantedEventData obj)
    {
        m_CharacterSO.UpdateRVWatched();
        m_RequiredRVParamsManager.SetParameterValue("WatchedRV", m_CharacterSO.GetRVWatchedCount().ToString());
        m_RequiredRVParamsManager.SetParameterValue("RequiredRV", m_CharacterSO.GetRequiredRVCount().ToString());

        bool IsEnoughAds = m_CharacterSO.GetRVWatchedCount() == m_CharacterSO.GetRequiredRVCount();
        if (IsEnoughAds)
        {
            m_CharacterSO.TryUnlockItem();
            Load();
            Select();
            m_OutlineButton.SetActive(false);
        }

        #region MonetizationEventCode
        try
        {
            int characterID = transform.GetSiblingIndex() + 1;
            int timeRV = m_CharacterSO.GetRVWatchedCount();
            GameEventHandler.Invoke(MonetizationEventCode.BuyCharacter, characterID, timeRV);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }

    public void InitData(CharacterSO characterSO)
    {
        m_CharacterSO = characterSO;
        if (m_CharacterSO.IsFakeLock)
        {
            m_ButtonGroupRect.gameObject.SetActive(false);
            m_PreviewButton.gameObject.SetActive(false);
            m_LockPanel.gameObject.SetActive(true);
            return;
        }
        m_LockPanel.gameObject.SetActive(false);

        Load();
    }

    public void Select()
    {
        m_IsSelected = true;
        OnSelectAction?.Invoke(this);

        if (m_CharacterSO.IsUnlocked())
        {
            if (m_CharacterSO.TryGetModule<NewItemModule>(out var newItemModule))
            {
                if (newItemModule.isNew)
                {
                    newItemModule.isNew = false;

                    #region Firebase Event
                    GameEventHandler.Invoke(LogFirebaseEventCode.NewDriverSelected);
                    #endregion
                }
            }

            m_SelectButton.interactable = !m_IsSelected;
            m_OutlineNomal.SetActive(m_IsSelected);
            m_OutlineButton.SetActive(m_IsSelected);
            m_SelectButtonGrayScale.SetGrayscale(m_IsSelected);
            m_SelectText.gameObject.SetActive(!m_IsSelected);
            m_SelectedText.gameObject.SetActive(m_IsSelected);
        }
    }
    public void UnSelect()
    {
        m_IsSelected = false;
        m_OutlineNomal.SetActive(m_IsSelected);
        m_OutlineButton.SetActive(false);
    }
    private void OnSelectButton()
    {
        if (m_IsSelected)
            UnSelect();
        else
            Select();
    }
    private void OnPreviewButton()
    {
        //Close
        if (m_IsPreview)
        {
            UnPreview();
            return;
        }
        //Open
        OnPreview();
        OnPreviewAction?.Invoke(this);
    }
    public void OnPreview()
    {
        m_IsPreview = true;
        m_SelectText.gameObject.SetActive(!m_IsSelected);
        m_SelectedText.gameObject.SetActive(m_IsSelected);
        m_OutlineButton.SetActive(m_IsSelected);
        m_ButtonGroupRect.DOSizeDelta(new Vector2(m_ButtonGroupRect.sizeDelta.x, 125f), 0.2f);

        m_SelectButton.interactable = !m_IsSelected;
        m_SelectButtonGrayScale.SetGrayscale(m_IsSelected);
        if (m_CharacterSO.IsUnlocked())
        {
            m_SelectButton.transform.DOScale(Vector3.one, 0.2f);
            m_CurrentcyButton.transform.DOScale(Vector3.zero, 0);
            m_RVBehavior.transform.DOScale(Vector3.zero, 0);
        }
        else
        {
            if (Type == CharacterCellType.Currency)
                m_CurrentcyButton.transform.DOScale(Vector3.one, 0.2f);
            else if (Type == CharacterCellType.RV)
                m_RVBehavior.transform.DOScale(Vector3.one, 0.2f);
        }
    }
    public void UnPreview()
    {
        m_IsPreview = false;

        m_ButtonGroupRect.DOSizeDelta(new Vector2(m_ButtonGroupRect.sizeDelta.x, 0), 0.2f);

        m_CurrentcyButton.transform.DOScale(Vector3.zero, 0.2f);
        m_RVBehavior.transform.DOScale(Vector3.zero, 0.2f);
        m_SelectButton.transform.DOScale(Vector3.zero, 0.2f);
        m_OutlineButton.SetActive(false);
    }
    private void Load()
    {
        UpdateState();
        UpdateAvatar();
        UpdateButton();
    }

    private void UpdateState()
    {
        State = m_CharacterSO.IsUnlocked() ? CharacterCellState.Unlocked : CharacterCellState.Lock;
    }
    private void UpdateAvatar()
    {
        m_Avatar.sprite = m_CharacterSO.GetThumbnailImage();
    }
    private void UpdateButton()
    {
        m_RVBehavior.transform.DOScale(Vector3.zero, 0);
        m_CurrentcyButton.transform.DOScale(Vector3.zero, 0);
        m_SelectButton.transform.DOScale(Vector3.zero, 0);
        m_ButtonGroupRect.DOSizeDelta(new Vector2(m_ButtonGroupRect.sizeDelta.x, 0), 0.2f);

        if (m_CharacterSO.IsUnlocked())
        {
            m_CurrentcyButton.interactable = false;
            m_RVBehavior.interactable = false;
            m_CurrencyLabel.SetActive(false);
            m_RVLabel.SetActive(false);
            m_OutlineNomal.SetActive(false);
            m_OutlineButton.SetActive(false);
            m_SelectButton.gameObject.SetActive(true);
            return;
        }

        if (m_CharacterSO.TryGetModule<UnlockableItemModule>(out var module))
        {
            if (module.TryGetUnlockRequirement<Requirement_Currency>(out var requirement_Currency))
            {
                m_Type = CharacterCellType.Currency;
                m_CurrentcyButton.gameObject.SetActive(true);
                m_CurrencyLabel.SetActive(true);
                m_RequiredGemShadowText.SetText(m_CharacterSO.GetPrice().ToRoundedText());
                m_RequiredGemText.SetText(m_CharacterSO.GetPrice().ToRoundedText());

                m_UnlockButtonGrayScale.SetGrayscale(!requirement_Currency.IsMeetRequirement());
                m_CurrentcyButton.interactable = requirement_Currency.IsMeetRequirement();
            }
            else if (module.TryGetUnlockRequirement<Requirement_RewardedAd>(out var requirement_RewardedAd))
            {
                m_Type = CharacterCellType.RV;
                m_RVBehavior.gameObject.SetActive(true);
                m_RVLabel.SetActive(!m_CharacterSO.IsUnlocked());
                m_RequiredRVParamsManager.SetParameterValue("WatchedRV", m_CharacterSO.GetRVWatchedCount().ToString());
                m_RequiredRVParamsManager.SetParameterValue("RequiredRV", m_CharacterSO.GetRequiredRVCount().ToString());
            }
            else if (module.TryGetUnlockRequirement<Requirement_IAP>(out var requirement_IAP))
            {
                //Null
            }
        }
    }
}
