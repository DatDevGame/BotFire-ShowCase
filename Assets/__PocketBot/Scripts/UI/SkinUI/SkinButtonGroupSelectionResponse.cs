using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SkinButtonGroupSelectionResponse : MonoBehaviour, ISelectionResponse
{
    [SerializeField]
    protected SkinCell m_SkinCell;
    [SerializeField]
    protected RectTransform m_ButtonGroupRectTransform;

    protected void Awake()
    {
        m_ButtonGroupRectTransform.anchoredPosition = new Vector2(m_ButtonGroupRectTransform.anchoredPosition.x, m_ButtonGroupRectTransform.sizeDelta.y);
    }

    public void Select(bool isForceSelect = false)
    {
        m_ButtonGroupRectTransform.DOKill();
        m_ButtonGroupRectTransform.DOAnchorPos(new Vector2(m_ButtonGroupRectTransform.anchoredPosition.x, 10f), AnimationDuration.TINY);
    }

    public void Deselect()
    {
        m_ButtonGroupRectTransform.DOKill();
        m_ButtonGroupRectTransform.DOAnchorPos(new Vector2(m_ButtonGroupRectTransform.anchoredPosition.x, m_ButtonGroupRectTransform.sizeDelta.y), AnimationDuration.TINY).SetEase(Ease.InBack);
    }
}