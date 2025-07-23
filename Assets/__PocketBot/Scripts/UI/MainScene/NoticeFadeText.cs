using TMPro;
using UnityEngine;
using DG.Tweening;
using LatteGames;

public class NoticeFadeText : MonoBehaviour
{
    [SerializeField] private CanvasGroupVisibility canvasGroupVisibility;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TMP_Text noticeText;

    public RectTransform RectTransform => rectTransform;

    private void Start()
    {
        canvasGroupVisibility.Hide();
        rectTransform
            .DOAnchorPosY(rectTransform.anchoredPosition.y + 100, 1)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
    }

    private void OnDestroy()
    {
        rectTransform.DOKill();
    }

    public void SetText(string message)
    {
        noticeText.SetText(message);
    }
}
