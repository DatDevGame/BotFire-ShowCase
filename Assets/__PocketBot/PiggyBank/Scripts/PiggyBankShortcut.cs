using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using LatteGames;
using HyrphusQ.GUI;
using HyrphusQ.Events;
using TMPro;
using DG.Tweening;
using System;

public class PiggyBankShortcut : MonoBehaviour
{
    [SerializeField, BoxGroup("Config")] private bool m_IsMainScene;

    [SerializeField, BoxGroup("Ref")] private Image m_Avatar;
    [SerializeField, BoxGroup("Ref")] private Button m_Button;
    [SerializeField, BoxGroup("Ref")] private Slider m_CurrentGemSlider;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_TimeReward;
    [SerializeField, BoxGroup("Ref")] private GameObject m_GetFull;
    [SerializeField, BoxGroup("Ref")] private TextAdapter m_BubbleTextValue;
    [SerializeField, BoxGroup("Ref")] private GameObject m_Sunburnt;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_CanvasGroupVisibility;

    [SerializeField, BoxGroup("Data")] private PiggyBankManagerSO m_PiggyBankManagerSO;

    private float m_CurrentGems = 0;
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

        //m_PiggyBankManagerSO.CurrentGem.onValueChanged += CurrentGem_OnValueChanged;
        //m_PiggyBankManagerSO.CurrentLevel.onValueChanged += CurrentLevel_OnValueChanged;
        //m_CurrentGemSlider.onValueChanged.AddListener(SliderOnChangeValue);
        //m_Button.onClick.AddListener(OnClick);
        //SelectingModeUIFlow.OnCheckFlowCompleted += TrackSelectMode;
        //GameEventHandler.AddActionEvent(PlayModePopup.Enable, HandleOnEnablePlayModePopup);
        //GameEventHandler.AddActionEvent(PlayModePopup.Disable, HandleOnDisablePlayModePopup);
        //GameEventHandler.AddActionEvent(PiggyAction.CalcPerKill, PiggyActionCalcPerKill);
        //GameEventHandler.AddActionEvent(PiggyBankAction.Load, OnLoad);
    }

    private void OnDestroy()
    {
        //m_PiggyBankManagerSO.CurrentGem.onValueChanged -= CurrentGem_OnValueChanged;
        //m_PiggyBankManagerSO.CurrentLevel.onValueChanged -= CurrentLevel_OnValueChanged;
        //m_CurrentGemSlider.onValueChanged.RemoveListener(SliderOnChangeValue);
        //m_Button.onClick.RemoveListener(OnClick);
        //SelectingModeUIFlow.OnCheckFlowCompleted -= TrackSelectMode;
        //GameEventHandler.RemoveActionEvent(PlayModePopup.Enable, HandleOnEnablePlayModePopup);
        //GameEventHandler.RemoveActionEvent(PlayModePopup.Disable, HandleOnDisablePlayModePopup);
        //GameEventHandler.RemoveActionEvent(PiggyAction.CalcPerKill, PiggyActionCalcPerKill);
        //GameEventHandler.RemoveActionEvent(PiggyBankAction.Load, OnLoad);
    }

    private void Start()
    {
        if (m_PiggyBankManagerSO.IsDisplayed && !m_PiggyBankManagerSO.IsShowTheFirstTime && m_IsMainScene)
        {
            m_PiggyBankManagerSO.CurrentGem.value = 0;
            m_PiggyBankManagerSO.ActiveShowTheFirstTime();
            GameEventHandler.Invoke(PiggyBankAction.Show, "Automatically");
        }
    }

    private void TrackSelectMode(SelectingModeUIFlow selectingModeUIFlow)
    {
        if (!m_PiggyBankManagerSO.IsDisplayed)
        {
            m_CanvasGroupVisibility.Hide();
            return;
        }

        if (SelectingModeUIFlow.HasPlayAnimBoxSlot)
        {
            m_CanvasGroupVisibility.Show();
        }
        else if (!SelectingModeUIFlow.HasPlayAnimBoxSlot && !SelectingModeUIFlow.HasExitedSceneWithPlayModeUIShowing && !SelectingModeUIFlow.HasExitedSceneWithBossUIShowing && !SelectingModeUIFlow.HasExitedSceneWithBattleBetUIShowing)
        {
            m_CanvasGroupVisibility.Show();
        }
        else
        {
            m_CanvasGroupVisibility.Hide();
        }

        m_CurrentGems = m_PiggyBankManagerSO.CurrentGem.value;
        OnLoad();
    }

    private void Update()
    {
        m_TimeReward.SetText(m_PiggyBankManagerSO.GetRemainingTimeHandle(23, 23));
    }

    private void SliderOnChangeValue(float value)
    {
        m_GetFull.SetActive(m_PiggyBankManagerSO.IsEnoughReward && !m_IsMainScene);
        m_TimeReward.gameObject.SetActive(m_PiggyBankManagerSO.IsEnoughReward && m_IsMainScene);
    }

    private void OnLoad()
    {
        m_Sunburnt.SetActive(m_PiggyBankManagerSO.IsEnoughReward);

        m_Avatar.sprite = m_PiggyBankManagerSO.GetPiggyBankCurrent().SmallAvatar;

        Image imageFillSlider = m_CurrentGemSlider.fillRect.GetComponent<Image>();
        imageFillSlider.sprite = m_PiggyBankManagerSO.IsEnoughReward ? null : m_PiggyBankManagerSO.SpriteNotFull;
        if (m_IsMainScene)
        {
            imageFillSlider.sprite = m_PiggyBankManagerSO.IsEnoughReward ? null : m_PiggyBankManagerSO.SpriteNotFull;
            imageFillSlider.color = m_PiggyBankManagerSO.SliderNotFullColor;
            if (m_PiggyBankManagerSO.IsEnoughReward && !m_PiggyBankManagerSO.IsTimeOutReward)
                imageFillSlider.color = m_PiggyBankManagerSO.SliderFullColor;
            else if (m_PiggyBankManagerSO.IsEnoughReward && m_PiggyBankManagerSO.IsTimeOutReward)
                imageFillSlider.color = m_PiggyBankManagerSO.SliderTimeOutColor;
        }
        else
        {
            imageFillSlider.color = m_PiggyBankManagerSO.IsEnoughReward ? m_PiggyBankManagerSO.SliderFullColor : m_PiggyBankManagerSO.SliderNotFullColor;
        }

        m_CurrentGemSlider.maxValue = m_PiggyBankManagerSO.GetPiggyBankCurrent().SavedGems;
        m_BubbleTextValue.SetText(m_BubbleTextValue.blueprintText.Replace(Const.StringValue.PlaceholderValue, m_PiggyBankManagerSO.CurrentGem.value.ToString()));

        float timeDurationSlider = m_IsMainScene ? 0 : 1;
        TweenCallback<float> tweenCallback = (value) => 
        {
            m_CurrentGemSlider.value = value;
            m_BubbleTextValue.SetText(m_BubbleTextValue.blueprintText.Replace(Const.StringValue.PlaceholderValue, Mathf.RoundToInt(value).ToString()));
        };

        DOVirtual
            .Float(m_CurrentGems, m_PiggyBankManagerSO.CurrentGem.value, timeDurationSlider, tweenCallback)
            .OnComplete(() => 
            {
                if (!m_IsMainScene)
                {
                    transform
                    .DOShakeScale(0.5f, strength: 0.5f)
                    .OnComplete(() => 
                    {
                        StartCoroutine(CommonCoroutine.Delay(0.5f, false, () =>
                        {
                            m_CanvasGroupVisibility.Hide();
                        }));
                    });
                }
            });
    }

    private void OnClick()
    {
        GameEventHandler.Invoke(PiggyBankAction.Show, "Manually");
    }

    private void CurrentGem_OnValueChanged(ValueDataChanged<int> data)
    {
        m_BubbleTextValue.SetText(m_BubbleTextValue.blueprintText.Replace(Const.StringValue.PlaceholderValue, m_PiggyBankManagerSO.CurrentGem.value.ToString()));
        m_CurrentGemSlider.DOValue(data.newValue, 1);
        OnLoad();
    }

    private void CurrentLevel_OnValueChanged(ValueDataChanged<int> data)
    {
        OnLoad();
    }

    private void PiggyActionCalcPerKill(params object[] parameters)
    {
        if (!m_PiggyBankManagerSO.IsDisplayed)
        {
            m_CanvasGroupVisibility.Hide();
            return;
        }

        if (!m_PiggyBankManagerSO.IsShowTheFirstTime && !m_IsMainScene)
        {
            m_CanvasGroupVisibility.Hide();
            return;
        }

        if (parameters.Length <= 0 || parameters[0] == null) return;
        int killCount = (int)parameters[0];
        if (killCount > 0)
            m_CanvasGroupVisibility.Show();
        else
            m_CanvasGroupVisibility.Hide();
    }

    private void HandleOnEnablePlayModePopup()
    {
        if (!m_PiggyBankManagerSO.IsDisplayed)
            return;
        m_CanvasGroupVisibility.Hide();
    }
    private void HandleOnDisablePlayModePopup()
    {
        if (!m_PiggyBankManagerSO.IsDisplayed)
            return;
        m_CanvasGroupVisibility.Show();
    }
}
