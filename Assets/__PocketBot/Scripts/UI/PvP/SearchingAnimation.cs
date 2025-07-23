using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SearchingAnimation : MonoBehaviour
{
    [SerializeField] private List<RectTransform> m_Points;
    [SerializeField] private float jumpHeight = 50f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float delayBetweenJumps = 0.2f;

    private void Start()
    {
        AnimateDots();
    }

    private void AnimateDots()
    {
        for (int i = 0; i < m_Points.Count; i++)
        {
            RectTransform point = m_Points[i];
            Vector2 originalPos = point.anchoredPosition;

            point.DOAnchorPosY(originalPos.y + jumpHeight, duration)
                .SetDelay(i * delayBetweenJumps) 
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
}
