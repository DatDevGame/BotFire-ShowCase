using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class TextMeshProUGUIPair
{
    [SerializeField]
    private TextMeshProUGUI m_Text;
    private TextMeshProUGUI m_ShadowText;

    public TextMeshProUGUI text => m_Text;
    public TextMeshProUGUI shadowText
    {
        get
        {
            if (m_ShadowText == null)
            {
                m_ShadowText = m_Text.transform.parent.GetComponent<TextMeshProUGUI>();
            }
            return m_ShadowText;
        }
    }

    public bool IsNull()
    {
        return text == null;
    }

    public void SetText(string text)
    {
        this.text.SetText(text);
        this.shadowText.SetText(text);
    }
}