using HyrphusQ.Events;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUIButton : MonoBehaviour
{
    [SerializeField, BoxGroup("Ref")] private Button m_Button;
    [SerializeField, BoxGroup("Data")] private PPrefBoolVariable m_SingleFTUE;

    private void Awake()
    {
        m_Button.onClick.AddListener(OnClick);

        //TODO: Hide IAP & Popup
        CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void OnDestroy()
    {
        m_Button.onClick.RemoveListener(OnClick);
    }

    private void OnClick()
    {
        if (!m_SingleFTUE.value) return;
        GameEventHandler.Invoke(CharacterUIEvent.Show);
    }
}
