using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnchoredElementScrollRect : MonoBehaviour
{
    private static readonly Vector2 s_DesiredPivot = Vector2.one * 0.5f;

    [SerializeField]
    private RectTransform m_ElementAtTheTopOrLeft;
    [SerializeField]
    private RectTransform m_ElementAtTheBottomOrRight;
    [SerializeField]
    private RectTransform m_ElementInsideScrollView;
    [SerializeField]
    private ScrollRect m_ScrollRect;

    public RectTransform elementAtTheTopOrLeft => m_ElementAtTheTopOrLeft;
    public RectTransform elementAtTheBottomOrRight => m_ElementAtTheBottomOrRight;
    public RectTransform elementInsideScrollView
    {
        get => m_ElementInsideScrollView;
        set => m_ElementInsideScrollView = value;
    }

    private void Start()
    {
        m_ScrollRect.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy()
    {
        m_ScrollRect.onValueChanged.RemoveListener(OnValueChanged);
    }

    private void OnValueChanged(Vector2 value)
    {
        UpdateView();
    }

    public void UpdateView()
    {
        if (m_ElementInsideScrollView == null)
            return;
        Vector2 topOrLeftPos = m_ElementAtTheTopOrLeft.CalcWorldPositionAtPivot(s_DesiredPivot);
        Vector2 bottomOrRightPos = m_ElementAtTheBottomOrRight.CalcWorldPositionAtPivot(s_DesiredPivot);
        Vector2 currentPos = m_ElementInsideScrollView.CalcWorldPositionAtPivot(s_DesiredPivot);
        if (m_ScrollRect.vertical)
        {
            m_ElementAtTheTopOrLeft.gameObject.SetActive(currentPos.y > topOrLeftPos.y);
            m_ElementAtTheBottomOrRight.gameObject.SetActive(currentPos.y < bottomOrRightPos.y);
        }
        else if (m_ScrollRect.horizontal)
        {
            m_ElementAtTheTopOrLeft.gameObject.SetActive(currentPos.x < topOrLeftPos.x);
            m_ElementAtTheBottomOrRight.gameObject.SetActive(currentPos.x > bottomOrRightPos.x);
        }
    }
}