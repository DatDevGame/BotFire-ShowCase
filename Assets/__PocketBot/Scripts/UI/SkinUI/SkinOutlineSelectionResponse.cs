using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinOutlineSelectionResponse : MonoBehaviour, ISelectionResponse
{
    [SerializeField]
    protected SkinCell m_SkinCell;
    [SerializeField]
    protected GameObject m_Outline, m_OutlineWithLabel;

    public void Select(bool isForceSelect = false)
    {
        var isUnlocked = m_SkinCell.item.IsUnlocked();
        m_Outline.SetActive(isUnlocked);
        m_OutlineWithLabel.SetActive(!isUnlocked);
    }

    public void Deselect()
    {
        if (m_Outline.activeSelf)
            m_Outline.SetActive(false);
        if (m_OutlineWithLabel.activeSelf)
            m_OutlineWithLabel.SetActive(false);
    }
}