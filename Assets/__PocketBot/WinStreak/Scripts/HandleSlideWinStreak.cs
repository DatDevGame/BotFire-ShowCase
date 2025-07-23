using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class HandleSlideWinStreak : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private RectTransform m_BubbleRect;
    [SerializeField, BoxGroup("Ref")] private RectTransform m_ValueTextRect;
    [SerializeField, BoxGroup("Ref")] private List<RectTransform> m_PointThePath;
    [SerializeField, BoxGroup("Ref")] private TMP_Text m_ValueText;
    [SerializeField, BoxGroup("Ref")] private Image m_BubbleImage;
    [SerializeField, BoxGroup("Data")] private Sprite m_BubbleDown;
    [SerializeField, BoxGroup("Data")] private Sprite m_BubbleLeft;

    public void Load(float value)
    {
        int roundedValue = Mathf.RoundToInt(value);
        UpdateBubbleImage(roundedValue);
        UpdateBubblePosition(roundedValue);
        UpdateValueText(roundedValue);
    }

    private void UpdateBubbleImage(int value)
    {
        m_BubbleImage.sprite = value > 1 ? m_BubbleLeft : m_BubbleDown;
    }

    private void UpdateBubblePosition(int value)
    {
        Vector2 bubblePosition = value > 1
            ? new Vector2(70, -2)
            : new Vector2(0, 30);
        m_BubbleRect.DOAnchorPos(bubblePosition, 0.7f);

        Vector2 textPosition = value > 1
            ? new Vector2(7.5f, -4.4f)
            : new Vector2(0, 2.1f);
        m_ValueTextRect.anchoredPosition = textPosition;
    }

    private void UpdateValueText(int value)
    {
        m_ValueText.SetText(value.ToString());
    }

}
