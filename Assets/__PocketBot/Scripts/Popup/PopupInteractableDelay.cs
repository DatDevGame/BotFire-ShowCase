using System.Collections;
using System.Collections.Generic;
using LatteGames;
using UnityEngine;
using UnityEngine.UI;

public class PopupInteractableDelay : MonoBehaviour
{
    private const float k_DelayTime = 0.5f;

    [SerializeField]
    private Button m_CloseButton;
    [SerializeField]
    private CanvasGroupVisibility m_CanvasGroupVisibility;

    private void Awake()
    {
        m_CanvasGroupVisibility.GetOnStartShowEvent().Subscribe(() =>
        {
            m_CloseButton.interactable = false;
        });
        m_CanvasGroupVisibility.GetOnEndShowEvent().Subscribe(() =>
        {
            if (Mathf.Approximately(k_DelayTime, 0f))
            {
                m_CloseButton.interactable = true;
            }
            else
            {
                StartCoroutine(CommonCoroutine.Delay(k_DelayTime, false, () =>
                {
                    m_CloseButton.interactable = true;
                }));
            }
        });
    }

    private void OnValidate()
    {
        if (m_CanvasGroupVisibility == null)
        {
            m_CanvasGroupVisibility = GetComponent<CanvasGroupVisibility>();
        }
    }
}