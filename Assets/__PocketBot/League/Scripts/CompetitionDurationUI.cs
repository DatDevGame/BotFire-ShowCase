using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using static LeagueDataSO;

public class CompetitionDurationUI : MonoBehaviour
{
    private const int k_DaysInAWeek = 7;

    [SerializeField]
    private CompetitionDurationNodeUI m_DurationNodeUIPrefab;
    [SerializeField]
    private RectTransform m_DurationNodeContent;

    private CompetitionDurationNodeUI[] m_NodeUIs = new CompetitionDurationNodeUI[k_DaysInAWeek];

    public CompetitionDurationNodeUI[] nodeUIs => m_NodeUIs;

    private void Awake()
    {
        GenerateDurationNodeUI(GetCurrentTime());
    }

    public void GenerateDurationNodeUI(DateTime now)
    {
        for (int i = 0; i < k_DaysInAWeek; i++)
        {
            CompetitionDurationNodeUI durationNodeUI = Instantiate(m_DurationNodeUIPrefab, m_DurationNodeContent);
            int currentDayOfWeek = now.DayOfWeek == DayOfWeek.Sunday ? k_DaysInAWeek : (int)now.DayOfWeek;
            int dayOfWeek = i + 1;
            if (dayOfWeek == k_DaysInAWeek)
                durationNodeUI.DisableLink();
            Status status = GetStatus(dayOfWeek, currentDayOfWeek);
            durationNodeUI.SetStatus(status, currentDayOfWeek);
            m_NodeUIs[i] = durationNodeUI;
        }
    }

    public void UpdateStatus(DateTime now, bool showTickIcon = false)
    {
        if (m_DurationNodeContent.transform.childCount <= 0)
            return;

        for (int i = 0; i < m_NodeUIs.Length; i++)
        {
            int currentDayOfWeek = now.DayOfWeek == DayOfWeek.Sunday ? k_DaysInAWeek : (int)now.DayOfWeek;
            int dayOfWeek = i + 1;
            Status status = GetStatus(dayOfWeek, currentDayOfWeek);
            m_NodeUIs[i].SetStatus(status, currentDayOfWeek, showTickIcon);
        }
    }

    [Button]
    public void PlayPassedAnimation(float delayTime = AnimationDuration.TINY)
    {
        if (m_DurationNodeContent.transform.childCount <= 0)
            return;

        DateTime now = GetCurrentTime();
        int currentDayOfWeek = now.DayOfWeek == DayOfWeek.Sunday ? k_DaysInAWeek : (int)now.DayOfWeek;
        CompetitionDurationNodeUI passedNode = m_NodeUIs[currentDayOfWeek - 2];
        CompetitionDurationNodeUI currentNode = m_NodeUIs[currentDayOfWeek - 1];
        passedNode.SetStatus(Status.Present, currentDayOfWeek - 1, true);
        passedNode.PlayPassedAnimation(Status.Passed, currentDayOfWeek - 1).Append(currentNode.PlayPassedAnimation(Status.Present, currentDayOfWeek)).PrependInterval(delayTime).OnComplete(() => UpdateStatus(now));
    }
}