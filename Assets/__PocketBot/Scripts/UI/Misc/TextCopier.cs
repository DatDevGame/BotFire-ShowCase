using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextCopier : MonoBehaviour
{
    [SerializeField] TMP_Text sourceTMP;
    TMP_Text m_TMP;
    private void Awake()
    {
        m_TMP = GetComponent<TMP_Text>();
        m_TMP.SetText(sourceTMP.text);
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
    }

    private void OnDestroy()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    }

    void OnTextChanged(object obj)
    {
        // This method will be called whenever text changes in a TextMeshPro component
        TMP_Text textComponent = obj as TMP_Text;
        if (textComponent == sourceTMP)
        {
            // Access the text component that triggered the event
            m_TMP.SetText(textComponent.text);
            m_TMP.ForceMeshUpdate(true);
        }
    }
}
