using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using HyrphusQ.GUI;
using HyrphusQ.SerializedDataStructure;
using Sirenix.Utilities;
using DG.Tweening;
using UnityEngine.Events;
using LatteGames.Monetization;
using HyrphusQ.Events;
using LatteGames;
using HightLightDebug;
using System;
using UnityEditor;
using LatteGames.Template;

public enum PiggyBankState
{
    NotFull,
    Full,
    Timeout
}

public enum PiggyBankAction
{
    Show, 
    Hide,
    Load
}

public class PiggyBankUI : MonoBehaviour
{
    
    [SerializeField, BoxGroup("Config")] private float m_SliderAnimationDuration;
    [SerializeField, BoxGroup("Config")] private float m_AnimationLeveUpDuration = 0.5f; // Duration for the animation

    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_CanvasGroupMain;

    [SerializeField, BoxGroup("Ref Header")] private MultiImageButton m_CloseButton;

    [SerializeField, BoxGroup("Ref Info")] private Image m_Avatar;
    [SerializeField, BoxGroup("Ref Info")] private TMP_Text m_LevelText;
    [SerializeField, BoxGroup("Ref Info")] private TMP_Text m_OutlineLevelText;
    [SerializeField, BoxGroup("Ref Info")] private TMP_Text m_SavedGemText;
    [SerializeField, BoxGroup("Ref Info")] private Slider m_SliderCurrentGems;
    [SerializeField, BoxGroup("Ref Info")] private TextAdapter m_CurrentGemBubbleText;
    [SerializeField, BoxGroup("Ref Info")] private TextAdapter m_ClaimText;
    [SerializeField, BoxGroup("Ref Info")] private GameObject m_Sunburnt;
    [SerializeField, BoxGroup("Ref Info")] private AutoShinyEffect m_AvatarShinyEffect;

    [SerializeField, BoxGroup("Ref Button Group")] private MultiImageButton m_LoseItButton;
    [SerializeField, BoxGroup("Ref Button Group")] private MultiImageButton m_BuyButton;
    [SerializeField, BoxGroup("Ref Button Group")] private LG_IAPButton m_IAPBuyButton;
    [SerializeField, BoxGroup("Ref Button Group")] private GrayscaleUI m_BuyButtonGrayScale;
    [SerializeField, BoxGroup("Ref Button Group")] private SerializedDictionary<PiggyBankState, GameObject> m_BoxTextStatus;
    [SerializeField, BoxGroup("Ref Button Group")] private SerializedDictionary<PiggyBankState, TextAdapter> m_StatusTexts;
    [SerializeField, BoxGroup("Ref Button Group")] private SerializedDictionary<PiggyBankState, TextAdapter> m_OutlineStatusTexts;

    [SerializeField, BoxGroup("Ref Description Panel")] private TMP_Text m_DescriptionText;
    [SerializeField, BoxGroup("Ref Description Panel")] private TextAdapter m_DescriptionTextAdapter;

    [SerializeField, BoxGroup("Data")] private PiggyBankManagerSO m_PiggyBankManagerSO;

    [ShowInInspector]
    private PiggyBankState m_PiggyBankState;
    private PiggyBankState PiggyBankState
    {
        get => m_PiggyBankState;
        set
        {
            if (m_PiggyBankState != value)
            {
                m_PiggyBankState = value;
                OnLoad();
            }
        }
    }
    private Vector3 m_AvatarOriginalScale;
    private const string MANUALY_ACTION = "Manually";
    private const string AUTOMATICALLY_ACTION = "Automatically";
    private string m_ActionPopupName;
    private bool m_IsShow = false;
    private bool m_IsAnimationBuyComplete = false;

    private float m_ShinyWidth;
    private float m_ShinySoftness;
    private float m_ShinyBrightness;
    private float m_ShinyRotation;
    private float m_ShinyHighlight;

    private void Awake()
    {
        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        //m_CloseButton.onClick.AddListener(OnCloseClick);
        //m_LoseItButton.onClick.AddListener(OnLoseItClick);
        //m_SliderCurrentGems.onValueChanged.AddListener(SliderCurrentGemsOnChangeValue);
        //m_PiggyBankManagerSO.CurrentGem.onValueChanged += CurrentGem_OnValueChanged;

        //GameEventHandler.AddActionEvent(PiggyBankAction.Show, OnShow);
        //GameEventHandler.AddActionEvent(PiggyBankAction.Hide, OnHide);
        //GameEventHandler.AddActionEvent(IAPEventCode.OnProcessPurchase, OnProcessPurchase);
    }

    private void OnDestroy()
    {
        //m_CloseButton.onClick.RemoveListener(OnCloseClick);
        //m_LoseItButton.onClick.RemoveListener(OnLoseItClick);
        //m_SliderCurrentGems.onValueChanged.RemoveListener(SliderCurrentGemsOnChangeValue);
        //m_PiggyBankManagerSO.CurrentGem.onValueChanged -= CurrentGem_OnValueChanged;

        //GameEventHandler.RemoveActionEvent(PiggyBankAction.Show, OnShow);
        //GameEventHandler.RemoveActionEvent(PiggyBankAction.Hide, OnHide);
        //GameEventHandler.RemoveActionEvent(IAPEventCode.OnProcessPurchase, OnProcessPurchase);

        //if (m_IsAnimationBuyComplete)
        //{
        //    m_PiggyBankManagerSO.UpgradeLevel();
        //    m_PiggyBankManagerSO.ResetDeault();
        //}
    }

    private void Start()
    {
        m_AvatarOriginalScale = m_Avatar.transform.localScale;
        OnLoad();
    }

    private void Update()
    {
        UpdateState();
        UpdateButtonGroup();
        UpdateStatusText();
        PiggyHandle();
    }

    private void OnLoad()
    {
        m_IAPBuyButton.OverrideSetup(m_PiggyBankManagerSO.GetPiggyBankCurrent().IAPProductSO);
        UpdateView();
        GameEventHandler.Invoke(PiggyBankAction.Load);

        if (PiggyBankState == PiggyBankState.Timeout)
            OnShow(AUTOMATICALLY_ACTION);
    }

    private void UpdateView()
    {
        UpdateState();
        UpdateAvatarAndLevel();
        UpdateGemTexts();
        UpdateSlider();
        UpdateUIElements();
        UpdateStatusText();
        UpdateDescriptionText();

        #region Progression Event
        PiggyBankProgressionEvent("Start");
        #endregion
    }

    private void UpdateAvatarAndLevel()
    {
        PiggyBankLevelSO currentLevel = m_PiggyBankManagerSO.GetPiggyBankCurrent();
        m_Avatar.sprite = currentLevel.Avatar;
        string levelText = $"LV. {m_PiggyBankManagerSO.CurrentLevel.value}";
        m_LevelText.SetText(levelText);
        m_OutlineLevelText.SetText(levelText);
    }

    private void UpdateGemTexts()
    {
        PiggyBankLevelSO currentLevel = m_PiggyBankManagerSO.GetPiggyBankCurrent();
        m_SavedGemText.SetText($"{currentLevel.SavedGems}");
        m_CurrentGemBubbleText.SetText(
            m_CurrentGemBubbleText.blueprintText.Replace(Const.StringValue.PlaceholderValue,
            m_PiggyBankManagerSO.CurrentGem.value.ToString())
        );
    }

    private void UpdateSlider()
    {
        PiggyBankLevelSO currentLevel = m_PiggyBankManagerSO.GetPiggyBankCurrent();
        Image fillImage = m_SliderCurrentGems.fillRect.GetComponent<Image>();

        fillImage.sprite = m_PiggyBankManagerSO.IsEnoughReward ? null : m_PiggyBankManagerSO.SpriteNotFull;
        fillImage.color = m_PiggyBankManagerSO.IsEnoughReward ? m_PiggyBankManagerSO.SliderFullColor : m_PiggyBankManagerSO.SliderNotFullColor;

        m_SliderCurrentGems.maxValue = currentLevel.SavedGems;
        m_SliderCurrentGems.DOValue(m_PiggyBankManagerSO.CurrentGem.value, m_SliderAnimationDuration);
    }

    private void UpdateUIElements()
    {
        bool isNotFull = m_PiggyBankState == PiggyBankState.NotFull;

        m_Sunburnt.SetActive(!isNotFull);
        m_CurrentGemBubbleText.gameObject.SetActive(isNotFull);
        m_ClaimText.gameObject.SetActive(!isNotFull);

        // Hide all status texts and show the current one
        foreach (var statusText in m_StatusTexts.Values) statusText.gameObject.SetActive(false);
        foreach (var outlineText in m_OutlineStatusTexts.Values) outlineText.gameObject.SetActive(false);

        m_StatusTexts[m_PiggyBankState].gameObject.SetActive(true);
        m_OutlineStatusTexts[m_PiggyBankState].gameObject.SetActive(true);

        // Hide all box statuses and show the current one
        m_BoxTextStatus.Values.ToList().ForEach(v => v.SetActive(false));
        m_BoxTextStatus[m_PiggyBankState].SetActive(true);
    }

    private void UpdateButtonGroup()
    {
        m_LoseItButton.gameObject.SetActive(m_PiggyBankState == PiggyBankState.Timeout);
        m_BuyButton.gameObject.SetActive(true);
        m_BuyButton.interactable = m_PiggyBankState == PiggyBankState.Full || m_PiggyBankState == PiggyBankState.Timeout;
        m_BuyButtonGrayScale.SetGrayscale(m_PiggyBankState == PiggyBankState.NotFull);
        m_CloseButton.gameObject.SetActive(m_PiggyBankState != PiggyBankState.Timeout);
    }
    private void UpdateState()
    {
        if (!m_PiggyBankManagerSO.IsEnoughReward)
        {
            PiggyBankState = PiggyBankState.NotFull;
        }
        else if (m_PiggyBankManagerSO.IsEnoughReward && !m_PiggyBankManagerSO.IsTimeOutReward)
        {
            PiggyBankState = PiggyBankState.Full;
        }
        else if (m_PiggyBankManagerSO.IsEnoughReward && m_PiggyBankManagerSO.IsTimeOutReward)
        {
            PiggyBankState = PiggyBankState.Timeout;
        }
    }

    private void UpdateStatusText()
    {
        string statusText = m_PiggyBankState switch
        {
            PiggyBankState.NotFull => m_PiggyBankManagerSO.GetPiggyBankCurrent().PerKill.ToString(),
            PiggyBankState.Full => m_PiggyBankManagerSO.GetRemainingTimeHandle(44, 44),
            PiggyBankState.Timeout => m_PiggyBankManagerSO.GetRemainingTimeout(44, 44),
            _ => ""
        };
        m_StatusTexts[m_PiggyBankState].SetText(m_StatusTexts[m_PiggyBankState].blueprintText.Replace(Const.StringValue.PlaceholderValue, statusText));
        m_OutlineStatusTexts[m_PiggyBankState].SetText(m_OutlineStatusTexts[m_PiggyBankState].blueprintText.Replace(Const.StringValue.PlaceholderValue, statusText));
    }

    private void UpdateDescriptionText()
    {
        var blueprintText = m_DescriptionTextAdapter.blueprintText;
        string descriptionText = blueprintText
            .Replace("{savedgems}", m_PiggyBankManagerSO.GetPiggyBankCurrent().SavedGems.ToString())
            .Replace("{level}", (m_PiggyBankManagerSO.CurrentLevel.value + 1).ToString())
            .Replace("{nextsavedgems}", m_PiggyBankManagerSO.GetPiggyBankNextLevel().SavedGems.ToString())
            .Replace("{nextperkill}", m_PiggyBankManagerSO.GetPiggyBankNextLevel().PerKill.ToString());

        if (m_PiggyBankManagerSO.IsMaxLevel)
        {
            string[] parts = descriptionText.Split(new string[] { "<br>" }, System.StringSplitOptions.None);
            descriptionText = parts[0];
        }

        m_DescriptionTextAdapter.SetText(descriptionText);
    }

    private void PiggyHandle()
    {
        if (m_PiggyBankManagerSO.IsEnoughReward)
        {
            if (!m_PiggyBankManagerSO.IsHasTimeFilled)
                m_PiggyBankManagerSO.ActiveTime();

            if (!m_PiggyBankManagerSO.IsOpenFullPiggy)
            {
                m_PiggyBankManagerSO.ActiveShowOpenPiggy();
                OnShow(AUTOMATICALLY_ACTION);
            }
        }
    }

    private void AnimationLevelUpHanle(Action OnCompletedScaleUpAction = null, Action OnCompletedSceleDownAction = null)
    {
        // Create the sequence
        Sequence sequence = DOTween.Sequence();

        float time = m_AnimationLeveUpDuration / 2;
        // Scale up the piggy icon and level text
        sequence.Append(m_Avatar.transform.DOScale(m_AvatarOriginalScale * 1.2f, time).SetEase(Ease.OutQuad)
            .OnComplete(() => 
            {
                OnCompletedScaleUpAction?.Invoke();
            }));
        sequence.Join(m_LevelText.transform.DOScale(Vector3.one * 1.2f, time).SetEase(Ease.OutQuad));
        sequence.Join(m_OutlineLevelText.transform.DOScale(Vector3.one * 1.2f, time).SetEase(Ease.OutQuad));

        // Scale down back to original size
        sequence.Append(m_Avatar.transform.DOScale(m_AvatarOriginalScale, time).SetEase(Ease.InQuad).OnComplete(() => OnCompletedSceleDownAction?.Invoke()));
        sequence.Join(m_LevelText.transform.DOScale(Vector3.one, time).SetEase(Ease.InQuad));
        sequence.Join(m_OutlineLevelText.transform.DOScale(Vector3.one, time).SetEase(Ease.InQuad));

        // Optionally add a callback at the end
        sequence.OnComplete(() =>
        {
            SetDefaultAutoShiny();
        });

        // Play the sequence
        sequence.Play();
    }


    public void OnCloseClick()
    {
        if (PiggyBankState == PiggyBankState.Timeout) return;
        OnHide();
    }

    private void OnLoseItClick()
    {
        m_PiggyBankManagerSO.ResetDeault();
        OnLoad();
    }

    private void OnBuyCompleted()
    {
        #region Progression Event
        PiggyBankProgressionEvent("Complete");
        #endregion

        m_IsAnimationBuyComplete = true;
        HandleShinyUpgrade();
        Action OnCompletedScaleUpAction = () => 
        {
            m_IsAnimationBuyComplete = false;
            m_PiggyBankManagerSO.UpgradeLevel();
            m_PiggyBankManagerSO.ResetDeault();
            OnLoad();

            SoundManager.Instance.PlayLoopSFX(PBSFX.UIUpgrade);
        };
        AnimationLevelUpHanle(OnCompletedScaleUpAction);
    }

    private void SliderCurrentGemsOnChangeValue(float value)
    {
        m_CurrentGemBubbleText.SetText(m_CurrentGemBubbleText.blueprintText.Replace(Const.StringValue.PlaceholderValue, value.ToRoundedText()));

        if (value >= m_PiggyBankManagerSO.GetPiggyBankCurrent().SavedGems)
        {
            UpdateView();
        }
    }

    private void CurrentGem_OnValueChanged(HyrphusQ.Events.ValueDataChanged<int> data)
    {
        m_SliderCurrentGems.DOValue(data.newValue, m_SliderAnimationDuration);
        UpdateView();
    }

    private void OnShow(params object[] parameters)
    {
        if (!m_PiggyBankManagerSO.IsDisplayed)
            return;

        #region Design Events
        if (parameters.Length > 0 && parameters[0] != null && !m_IsShow)
        {
            m_ActionPopupName = (string)parameters[0];
            string popupName = "PiggyBank";
            string status = $"Start";
            string operation = m_ActionPopupName;
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        }
        #endregion

        m_IsShow = true;
        m_CanvasGroupMain.Show();
    }

    private void OnHide()
    {
        if (!m_PiggyBankManagerSO.IsDisplayed)
            return;

        #region Design Events
        if (m_ActionPopupName != "" && m_IsShow)
        {
            string popupName = "PiggyBank";
            string status = $"Complete";
            string operation = m_ActionPopupName;
            GameEventHandler.Invoke(DesignEvent.PopupGroup, popupName, operation, status);
        }
        #endregion

        m_IsShow = false;
        m_CanvasGroupMain.Hide();
    }

    private void OnProcessPurchase(params object[] parameters)
    {
        LG_IAPButton _IAPButton = (LG_IAPButton)parameters[1];
        if(_IAPButton == m_IAPBuyButton)
        {
            OnBuyCompleted();
        }
    }

    private void HandleShinyUpgrade()
    {
        m_AvatarShinyEffect.IsAutoShiny = false;
        m_ShinyWidth = m_AvatarShinyEffect.width;
        m_ShinySoftness = m_AvatarShinyEffect.softness;
        m_ShinyBrightness = m_AvatarShinyEffect.brightness;
        m_ShinyRotation = m_AvatarShinyEffect.rotation;
        m_ShinyHighlight = m_AvatarShinyEffect.highlight;

        m_AvatarShinyEffect.width = 1;
        m_AvatarShinyEffect.softness = 1;
        m_AvatarShinyEffect.highlight = 1;
        m_AvatarShinyEffect.rotation = 90;
        DOVirtual.Float(0, 0.5f, 0.3f, (v) => { m_AvatarShinyEffect.location = v; });
    }

    private void SetDefaultAutoShiny()
    {
        DOVirtual
            .Float(0.5f, 0, 1, (v) => { m_AvatarShinyEffect.location = v; })
            .OnComplete(() => 
            {
                m_AvatarShinyEffect.IsAutoShiny = true;
                m_AvatarShinyEffect.width = m_ShinyWidth;
                m_AvatarShinyEffect.softness = m_ShinySoftness;
                m_AvatarShinyEffect.brightness = m_ShinyBrightness;
                m_AvatarShinyEffect.rotation = m_ShinyRotation;
                m_AvatarShinyEffect.highlight = m_ShinyHighlight;
            });
    }

    #region Progression Event
    private void PiggyBankProgressionEvent(string status)
    {
        if (m_PiggyBankManagerSO.IsDisplayed)
        {
            PiggyBankLevelSO currentPiggyBankLevelSO = m_PiggyBankManagerSO.GetPiggyBankCurrent();
            string keyEvent = $"{status}-KeyPiggyBankProgressionEvent-{currentPiggyBankLevelSO.m_Key}";
            if (!PlayerPrefs.HasKey(keyEvent))
            {
                int level = m_PiggyBankManagerSO.GetLevelPiggyBank(currentPiggyBankLevelSO);
                GameEventHandler.Invoke(ProgressionEvent.PiggyBank, status, level);
                PlayerPrefs.SetInt(keyEvent, 1);
            }
        }
    }
    #endregion
}
