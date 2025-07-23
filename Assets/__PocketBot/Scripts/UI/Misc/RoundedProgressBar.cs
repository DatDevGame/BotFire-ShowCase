using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class RoundedProgressBar : MonoBehaviour
{
    [SerializeField]
    protected Image m_FillImage;

    public float fillAmount
    {
        get
        {
            return m_FillImage.rectTransform.anchorMax.x;
        }
        set
        {
            var clampedValue = Mathf.Clamp01(value);
            m_FillImage.rectTransform.anchorMax = new Vector2(clampedValue, m_FillImage.rectTransform.anchorMax.y);
            m_FillImage.rectTransform.anchoredPosition = Vector2.zero;
        }
    }
    public Image fillImage => m_FillImage;

#if UNITY_EDITOR
    [ShowInInspector, PropertyRange(0f, 1f)]
    protected float fillAmountInspector
    {
        get
        {
            if (m_FillImage == null)
                return 0f;
            return fillAmount;
        }
        set
        {
            if (m_FillImage == null)
                return;
            fillAmount = value;
        }
    }
#endif
}