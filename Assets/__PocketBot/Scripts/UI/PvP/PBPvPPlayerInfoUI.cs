using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PBPvPPlayerInfoUI : MonoBehaviour
{
    protected const string Placeholder = "{value}";

    [SerializeField]
    protected PBRobotStatsSO m_RobotStatsOfMine;
    [SerializeField]
    protected Image m_ThumbnailImage;
    [SerializeField]
    protected TextMeshProUGUI m_NameText;
    [SerializeField]
    protected TextMeshProUGUI m_FormText;
    //[SerializeField]
    //protected List<SpriteSwitcher> m_FormStars;
    //[SerializeField]
    //protected List<PvPStatUI> m_StatUIs;

    protected string m_FormBlueprintText;
    protected string formBlueprintText
    {
        get
        {
            if (string.IsNullOrEmpty(m_FormBlueprintText))
                m_FormBlueprintText = m_FormText.text;
            return m_FormBlueprintText;
        }
    }

    protected void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchPrepared, UpdateView);
    }

    protected void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchPrepared, UpdateView);
    }

    protected void UpdateView()
    {
        m_ThumbnailImage = m_RobotStatsOfMine.chassisInUse.value.CreateIconImage(m_ThumbnailImage);
        m_NameText.SetText(m_RobotStatsOfMine.chassisInUse.value.GetDisplayName());
    }
}
