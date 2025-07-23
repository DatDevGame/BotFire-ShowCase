using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GlassPane : MonoBehaviour
{
    [SerializeField]
    private float m_Length = 1f;
    [SerializeField]
    private GameObject m_GlassPane;
    [SerializeField]
    private GameObject m_VerticalBar;

    private void OnValidate()
    {
        UpdateSize();
    }

    [Button]
    private void UpdateSize()
    {
        if (m_GlassPane == null || m_VerticalBar == null)
            return;
        m_GlassPane.transform.localScale = new Vector3(m_Length / 2f, m_GlassPane.transform.localScale.y, m_GlassPane.transform.localScale.z);
        m_VerticalBar.transform.localPosition = new Vector3(m_VerticalBar.transform.localPosition.x, m_VerticalBar.transform.localPosition.y, m_Length);
    }
}