using HyrphusQ.Events;
using LatteGames;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PBLinkRewardButton : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private Button m_LinkRewardBtn;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_AdsCount;
    [SerializeField, BoxGroup("Ref")] private CanvasGroupVisibility m_MainCanvasGroupVisibility;

    [SerializeField, BoxGroup("Data")] private HighestAchievedPPrefFloatTracker m_HighestAchievedPPrefFloatTracker;

    private PBLinkRewardManagerSO m_PBLinkRewardManagerSO => PBLinkRewardManager.Instance.PBLinkRewardManagerSO;
    private LayoutElement layoutElement;
    private bool m_IsEnoughTrophyDisplayed => m_HighestAchievedPPrefFloatTracker.value >= m_PBLinkRewardManagerSO.TrophyDisplayed;

    private void Awake()
    {
        //TODO: Hide IAP & Popup
        //PBLinkRewardManager.Instance.LoadPopup += Load;
        //layoutElement = gameObject.AddComponent<LayoutElement>();
        //GameEventHandler.AddActionEvent(LinkRewardAction.AnyCellOnRewardGranted, AnyCellOnRewardGranted);
        //GameEventHandler.AddActionEvent(PlayModePopup.Enable, PlayModeGroupEnable);
        //GameEventHandler.AddActionEvent(PlayModePopup.Disable, PlayModeGroupDisable);
        //m_LinkRewardBtn.onClick.AddListener(Click);
        //m_MainCanvasGroupVisibility.GetOnEndHideEvent().Subscribe(() => layoutElement.ignoreLayout = true);
        //m_MainCanvasGroupVisibility.GetOnStartShowEvent().Subscribe(() => layoutElement.ignoreLayout = false);
    }

    private void OnDestroy()
    {
        //if(PBLinkRewardManager.Instance != null)
        //    PBLinkRewardManager.Instance.LoadPopup -= Load;
        //GameEventHandler.RemoveActionEvent(LinkRewardAction.AnyCellOnRewardGranted, AnyCellOnRewardGranted);
        //GameEventHandler.RemoveActionEvent(PlayModePopup.Enable, PlayModeGroupEnable);
        //GameEventHandler.RemoveActionEvent(PlayModePopup.Disable, PlayModeGroupDisable);
        //m_LinkRewardBtn.onClick.RemoveListener(Click);
    }

    private void Start()
    {
        Load();
        UpdateDisplayCondition();
    }

    private void AnyCellOnRewardGranted()
    {
        Load();
    }

    private void Load()
    {
        List<PBLinkRewardCellSO> linkRewardCellSOs = PBLinkRewardManager.Instance.Queue;
        int countAdsClaimed = linkRewardCellSOs.Count(cell => cell.IsClaimed);
        m_AdsCount.SetText($"{countAdsClaimed}/{linkRewardCellSOs.Count}");
    }

    private void PlayModeGroupEnable() => m_MainCanvasGroupVisibility.Hide();

    private void PlayModeGroupDisable() => UpdateDisplayCondition();

    private void UpdateDisplayCondition()
    {
        //TODO: Hide IAP & Popup
        m_MainCanvasGroupVisibility.Hide();
        //    if (!m_IsEnoughTrophyDisplayed)
        //        m_MainCanvasGroupVisibility.Hide();
        //    else
        //        m_MainCanvasGroupVisibility.Show();
    }
        private void Click()
    {
        string operation = "Manually";
        GameEventHandler.Invoke(LinkRewardPopupState.Show, operation);
    }
}
