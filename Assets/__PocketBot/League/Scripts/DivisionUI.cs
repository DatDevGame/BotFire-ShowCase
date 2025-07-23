using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LeagueDataSO;

public class DivisionUI : MonoBehaviour
{
    [SerializeField]
    private GameObject m_FrameOutline;
    [SerializeField]
    private GameObject m_PassedPanelGO;
    [SerializeField]
    private Image m_IconImage;
    [SerializeField]
    private TextMeshProUGUI m_NameText;
    [SerializeField]
    private TextMeshProUGUI m_PlayerPoolCountText;
    [SerializeField]
    private LeagueRewardUI m_RewardUI;
    [SerializeField]
    private EZAnimBase m_VisibilityAnim;
    [SerializeField]
    private CanvasGroup m_CanvasGroup;

    public RectTransform rectTransform => transform as RectTransform;
    public EZAnimBase visibilityAnim => m_VisibilityAnim;
    public RectTransform selectionFrameRectTransform => m_FrameOutline.transform as RectTransform;
    public Image iconImage => m_IconImage;

    public void UpdateStatus(Status status)
    {
        m_FrameOutline.SetActive(status == Status.Present);
        m_PassedPanelGO.SetActive(status == Status.Passed);
    }

    public void Initialize(LeagueDivision division, Status status)
    {
        UpdateStatus(status);
        m_IconImage.sprite = division.icon;
        m_NameText.text = division.displayName;
        m_PlayerPoolCountText.text = division.playerPoolCount.ToString();
        if (division.bestRewardInfo.generalItems != null && division.bestRewardInfo.generalItems.Count > 0)
        {
            m_RewardUI.UpdateViewWithBestReward(division.bestRewardInfo);
        }
    }

    public void FadeOut(float duration)
    {
        m_CanvasGroup.FadeOut(duration);
    }

    public void FadeIn(float duration)
    {
        m_CanvasGroup.FadeIn(duration);
    }
}