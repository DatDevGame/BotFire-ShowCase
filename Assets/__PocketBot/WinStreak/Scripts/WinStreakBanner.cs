using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using HyrphusQ.SerializedDataStructure;
using HyrphusQ.GUI;
using HyrphusQ.Events;
using LatteGames;

public class WinStreakBanner : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private Image m_AvatarStreak;
    [SerializeField, BoxGroup("Ref")] private MultiImageButton m_InfoWinStreakPopupBtn;
    [SerializeField, BoxGroup("Ref")] private TextAdapter m_TimeText;
    [SerializeField, BoxGroup("Ref")] private TextAdapter m_OutlineTimeText;
    [SerializeField, BoxGroup("Ref")] private GameObject m_TimeRunningObject;
    [SerializeField, BoxGroup("Ref")] private GameObject m_TimeUpObject;
    [SerializeField, BoxGroup("Ref")] private WinStreakLevel m_WinStreakLevel;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_MainCanvasGroup;

    [SerializeField, BoxGroup("Data")] private WinStreakManagerSO m_WinStreakManagerSO;
    [SerializeField, BoxGroup("Data")] private HighestAchivedWinStreak m_HighestAchivedWinStreakPPref;
    [SerializeField, BoxGroup("Data")] private PPrefIntVariable m_CurrentWinStreakPPref;

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

        //m_CurrentWinStreakPPref.onValueChanged += CurrentWinStreakPPref_OnValueChanged;
        //m_InfoWinStreakPopupBtn.onClick.AddListener(OnClickInfoPopup);

        //GameEventHandler.AddActionEvent(WinStreakPopup.OnLoadWinStreak, OnLoadWinStreak);
    }

    private void OnDestroy()
    {
        //m_CurrentWinStreakPPref.onValueChanged -= CurrentWinStreakPPref_OnValueChanged;
        //m_InfoWinStreakPopupBtn.onClick.RemoveListener(OnClickInfoPopup);

        //GameEventHandler.RemoveActionEvent(WinStreakPopup.OnLoadWinStreak, OnLoadWinStreak);
    }

    private void Update()
    {
        UpdateTime();
    }

    private void Start()
    {
        LoadInfo();
    }

    public void OnClickInfoPopup()
    {
        GameEventHandler.Invoke(WinStreakPopup.Show, "Manually");
    }

    private void OnLoadWinStreak()
    {
        LoadInfo();
    }

    private void LoadInfo()
    {
        //TODO: Hide IAP & Popup
        m_MainCanvasGroup.Hide();
        //if (m_WinStreakManagerSO.ConditionDisplayedWinStreak && m_WinStreakManagerSO.PprefTheFirstTime.value)
        //    m_MainCanvasGroup.Show();
        //else
        //    m_MainCanvasGroup.Hide();

        OnLoadAvatar();
    }

    private void OnLoadAvatar()
    {
        if (m_WinStreakLevel != null)
            m_WinStreakLevel.LoadAvatar(m_CurrentWinStreakPPref.value);
    }

    private void UpdateTime()
    {
        if (m_WinStreakManagerSO == null) return;
        m_TimeUpObject.gameObject.SetActive(m_WinStreakManagerSO.IsResetReward);
        m_TimeRunningObject.SetActive(!m_WinStreakManagerSO.IsResetReward);

        if (m_WinStreakManagerSO.IsResetReward)
        {
            m_TimeText.SetText(m_TimeText.blueprintText.Replace(Const.StringValue.PlaceholderValue, "Time Up!"));
            m_OutlineTimeText.SetText(m_TimeText.blueprintText.Replace(Const.StringValue.PlaceholderValue, "Time Up!"));
        }
        else
        {
            m_TimeText.SetText(m_TimeText.blueprintText.Replace(Const.StringValue.PlaceholderValue, $"{m_WinStreakManagerSO.GetRemainingTimeHandle(20, 20)}"));
            m_OutlineTimeText.SetText(m_TimeText.blueprintText.Replace(Const.StringValue.PlaceholderValue, $"{m_WinStreakManagerSO.GetRemainingTimeHandle(20, 20)}"));
        }
    }

    private void CurrentWinStreakPPref_OnValueChanged(HyrphusQ.Events.ValueDataChanged<int> obj)
    {
        LoadInfo();
    }

    public void Show()
    {
        //TODO: Hide IAP & Popup
        m_MainCanvasGroup.Hide();
        //if (m_WinStreakManagerSO.ConditionDisplayedWinStreak)
        //    m_MainCanvasGroup.Show();
    }

    public void Hide()
    {
        if (m_WinStreakManagerSO.ConditionDisplayedWinStreak)
            m_MainCanvasGroup.Hide();
    }

    private void ConditionDisplayedWinStreak_OnValueChanged(ValueDataChanged<bool> data)
    {
        if (data.newValue)
            m_MainCanvasGroup.Show();
    }
}
