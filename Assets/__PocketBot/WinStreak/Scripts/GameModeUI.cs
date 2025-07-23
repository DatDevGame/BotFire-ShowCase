using System.Collections;
using System.Collections.Generic;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class GameModeUI : MonoBehaviour
{
    [SerializeField] CanvasGroupVisibility winStreakVisibility;
    [SerializeField] VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] Vector2Int contentPaddingBottomRange;

    private void Awake()
    {
        winStreakVisibility.GetOnStartShowEvent().Subscribe(OnStartShow);
        winStreakVisibility.GetOnStartHideEvent().Subscribe(OnStartHide);
    }

    private void OnDestroy()
    {
        winStreakVisibility.GetOnStartShowEvent().Unsubscribe(OnStartShow);
        winStreakVisibility.GetOnStartHideEvent().Unsubscribe(OnStartHide);
    }

    void OnStartShow()
    {
        verticalLayoutGroup.padding.bottom = contentPaddingBottomRange.x;
    }

    void OnStartHide()
    {
        verticalLayoutGroup.padding.bottom = contentPaddingBottomRange.y;
    }
}
