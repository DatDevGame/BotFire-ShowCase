using System.Collections;
using System.Collections.Generic;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;

public class ChangeNamePopup : MonoBehaviour
{
    private static bool _isInited = false;
    private const float SHOW_MEDAL = 60f;
    private static bool _isCanShow = false;
    [SerializeField] private CanvasGroupVisibility _visibility;
    [SerializeField] private HighestAchievedPPrefFloatTracker _highestAchievedMedal;

    /**
    private void Start()
    {
        if (_isCanShow)
        {
            _visibility.Show();
            _isCanShow = false;
        }
        if (!_isInited)
        {
            _highestAchievedMedal.onValueChanged += OnHighestMedalChange;
            _isInited = true;
        }
        GameEventHandler.AddActionEvent(MainSceneEventCode.HideChangeNameUI, Hide);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(MainSceneEventCode.HideChangeNameUI, Hide);
    }

    private void Hide()
    {
        _visibility.Hide();
    }

    private static void OnHighestMedalChange(ValueDataChanged<float> dataChanged)
    {
        if (dataChanged.oldValue < SHOW_MEDAL && dataChanged.newValue >= SHOW_MEDAL)
        {
            _isCanShow = true;
        }
    }//**/
}
