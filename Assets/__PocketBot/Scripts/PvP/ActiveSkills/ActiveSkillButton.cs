using System;
using HyrphusQ.Events;
using LatteGames.GameManagement;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ActiveSkillCaster;

public class ActiveSkillButton : MonoBehaviour
{
    private static readonly Color s_WhiteDisabledButton = Color.white;
    private static readonly Color s_DarkDisabledButton = new Color(0.5f, 0.5f, 0.5f, 1f);

    public event Action onInitializeCompleted = delegate { };

    [SerializeField]
    private Image m_SkillIconImage;
    [SerializeField]
    private Image m_SkillReadyImage;
    [SerializeField]
    private Image m_SkillActiveImage;
    [SerializeField]
    private TextMeshProUGUI m_RemainingCooldownText;
    [SerializeField]
    private Button m_PerformSkillButton;
    [SerializeField]
    private GrayscaleUI m_GrayscaleUI;
    [SerializeField]
    private Color m_NormalCardQuantityTextColor = Color.white, m_ZeroCardQuantityTextColor = Color.red;
    [SerializeField]
    private TextMeshProUGUI m_CardQuantityText;

    private ActiveSkillCaster m_SkillCaster;

    public ActiveSkillCaster skillCaster => m_SkillCaster;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(RobotStatusEventCode.OnModelSpawned, OnRobotSpawned);
        GameEventHandler.AddActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStart);
        Hide();
    }

    private void Update()
    {
        UpdateSkillState();
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(RobotStatusEventCode.OnModelSpawned, OnRobotSpawned);
        GameEventHandler.RemoveActionEvent(PBLevelEventCode.OnLevelStart, OnLevelStart);
    }

    private void OnRobotSpawned(object[] parameters)
    {
        if (SceneManager.GetActiveScene().name == SceneName.PvP_BossFight.ToString())
            return;
        if (parameters == null || parameters.Length <= 0)
            return;
        if (parameters[0] is PBRobot robot && !robot.IsPreview && robot.PersonalInfo.isLocal && robot.ActiveSkillCaster != null)
        {
            Initialize(robot.ActiveSkillCaster);

            #region Design Event
            try
            {
                GameEventHandler.Invoke(DesignEvent.SkillUsage_Duel_Equiped, robot, robot.ActiveSkillCaster.GetActiveSkillSO());
                GameEventHandler.Invoke(DesignEvent.SkillUsage_Battle_Equiped, robot, robot.ActiveSkillCaster.GetActiveSkillSO());
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion
        }
    }

    private void OnLevelStart()
    {
        if (m_SkillCaster != null)
            Show();
    }

    private void OnPerformSkillButtonClicked()
    {
        if (!m_SkillCaster.IsAbleToPerformSkill())
            return;
        m_SkillCaster.PerformSkill();
    }

    private void Show()
    {
        gameObject.SetActive(true);
        UpdateSkillState();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void SetButtonInteractble(bool isInteractable, Color disabledColor, bool shouldBeGrayscale = false)
    {
        if (!m_PerformSkillButton.colors.disabledColor.IsEqualTo(disabledColor))
        {
            ColorBlock colorBlock = m_PerformSkillButton.colors;
            colorBlock.disabledColor = disabledColor;
            m_PerformSkillButton.colors = colorBlock;
        }
        if (m_PerformSkillButton.interactable != isInteractable)
        {
            m_PerformSkillButton.interactable = isInteractable;
        }
        m_GrayscaleUI.SetGrayscale(shouldBeGrayscale);
    }

    private void SetActiveGO(GameObject gameObject, bool isActive)
    {
        if (gameObject.activeSelf != isActive)
            gameObject.SetActive(isActive);
    }

    #region Design Event
    private bool m_ActiveSkillDesignEvent = false;
    private bool m_IsAReadySkillDesignEvent = false;
    #endregion

    #region Firebase Event
    private bool m_ActiveSkillFirebaseEvent = false;
    private bool m_IsAReadySkillFirebaseEvent = false;
    #endregion

    private void OnSkillStateChanged(ValueDataChanged<ActiveSkillCaster.SkillState> data)
    {
        UpdateSkillState();

        if (m_SkillCaster != null)
        {
            #region Design Event
            try
            {
                string status = "Ready";
                string skillName = m_SkillCaster.GetActiveSkillSO().GetDisplayName();

                if (skillName != "")
                {
                    if (!m_ActiveSkillDesignEvent && data.oldValue == SkillState.Ready && data.newValue == SkillState.Active)
                    {
                        m_ActiveSkillDesignEvent = true;
                        status = "Used";
                        GameEventHandler.Invoke(DesignEvent.SkillTrack, status, skillName);
                    }

                    if (data.oldValue == SkillState.Ready && data.newValue == SkillState.Active && m_IsAReadySkillDesignEvent && m_ActiveSkillDesignEvent)
                    {
                        status = "Used";
                        m_IsAReadySkillDesignEvent = false;
                        GameEventHandler.Invoke(DesignEvent.SkillTrack, status, skillName);
                    }
                    else if (data.oldValue == SkillState.Unavailable && data.newValue == SkillState.Ready && !m_IsAReadySkillDesignEvent && m_ActiveSkillDesignEvent)
                    {
                        status = "Ready";
                        m_IsAReadySkillDesignEvent = true;
                        GameEventHandler.Invoke(DesignEvent.SkillTrack, status, skillName);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion

            #region Firebase Event
            try
            {
                string skill = m_SkillCaster.GetActiveSkillSO().GetDisplayName().ToLower();

                if (skill != "")
                {
                    if (!m_ActiveSkillFirebaseEvent && data.oldValue == SkillState.Ready && data.newValue == SkillState.Active)
                    {
                        m_ActiveSkillFirebaseEvent = true;
                        GameEventHandler.Invoke(LogFirebaseEventCode.SkillUse, skill);
                    }

                    if (data.oldValue == SkillState.Ready && data.newValue == SkillState.Active && m_IsAReadySkillFirebaseEvent && m_ActiveSkillFirebaseEvent)
                    {
                        m_IsAReadySkillFirebaseEvent = false;
                        GameEventHandler.Invoke(LogFirebaseEventCode.SkillUse, skill);
                    }
                    else if (data.oldValue == SkillState.Unavailable && data.newValue == SkillState.Ready && !m_IsAReadySkillFirebaseEvent && m_ActiveSkillFirebaseEvent)
                    {
                        m_IsAReadySkillFirebaseEvent = true;
                        GameEventHandler.Invoke(LogFirebaseEventCode.SkillAvailable, skill);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
            }
            #endregion
        }
    }

    private void UpdateSkillState()
    {
        if (m_SkillCaster == null)
            return;

        switch (m_SkillCaster.skillState)
        {
            case SkillState.OnCooldown:
                SetButtonInteractble(false, s_DarkDisabledButton);
                SetActiveGO(m_SkillReadyImage.gameObject, false);
                SetActiveGO(m_SkillActiveImage.gameObject, false);
                SetActiveGO(m_RemainingCooldownText.gameObject, true);
                m_RemainingCooldownText.SetText($"{m_SkillCaster.remainingCooldown.ToString("0.#")}s");
                break;
            case SkillState.Unavailable:
                if (m_SkillCaster.GetActiveSkillSO().GetNumOfCards() > 0)
                {
                    SetButtonInteractble(false, s_DarkDisabledButton);
                }
                else
                {
                    SetButtonInteractble(false, s_WhiteDisabledButton, true);
                }
                SetActiveGO(m_SkillReadyImage.gameObject, false);
                SetActiveGO(m_SkillActiveImage.gameObject, false);
                SetActiveGO(m_RemainingCooldownText.gameObject, false);
                break;
            case SkillState.Ready:
                SetButtonInteractble(true, s_WhiteDisabledButton);
                SetActiveGO(m_SkillReadyImage.gameObject, true);
                SetActiveGO(m_SkillActiveImage.gameObject, false);
                SetActiveGO(m_RemainingCooldownText.gameObject, false);
                break;
            case SkillState.Active:
                SetButtonInteractble(true, s_WhiteDisabledButton);
                SetActiveGO(m_SkillReadyImage.gameObject, false);
                SetActiveGO(m_SkillActiveImage.gameObject, true);
                SetActiveGO(m_RemainingCooldownText.gameObject, false);
                m_SkillActiveImage.fillAmount = m_SkillCaster.remainingActiveTime / m_SkillCaster.GetActiveSkillSO().activeDuration;
                break;

        }
    }

    private void OnCardQuantityChanged(ValueDataChanged<int> eventData)
    {
        UpdateCardQuantity(eventData.newValue);
    }

    private void UpdateCardQuantity(int cardQuantity)
    {
        m_CardQuantityText.color = cardQuantity > 0 ? m_NormalCardQuantityTextColor : m_ZeroCardQuantityTextColor;
        m_CardQuantityText.SetText(Mathf.Min(cardQuantity, ActiveSkillCardUI.k_MaxCardQuantityDisplay).ToString());
    }

    public void Initialize(ActiveSkillCaster skillCaster)
    {
        m_SkillCaster = skillCaster;
        m_SkillIconImage.sprite = skillCaster.GetActiveSkillSO().GetThumbnailImage();
        m_PerformSkillButton.onClick.AddListener(OnPerformSkillButtonClicked);
        m_SkillCaster.onSkillStateChanged += OnSkillStateChanged;
        m_SkillCaster.onCardQuantityChanged += OnCardQuantityChanged;
        UpdateCardQuantity(m_SkillCaster.GetActiveSkillSO().GetNumOfCards());
        onInitializeCompleted.Invoke();

        #region Firebase Event
        try
        {
            string skill = $"{m_SkillCaster.GetActiveSkillSO().GetDisplayName().ToLower()}";
            GameEventHandler.Invoke(LogFirebaseEventCode.SkillAvailable, skill);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError($"{LogErrorKeyGA.Key}-{ex}");
        }
        #endregion
    }
}