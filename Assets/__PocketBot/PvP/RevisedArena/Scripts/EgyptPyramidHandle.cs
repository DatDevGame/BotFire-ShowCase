using HyrphusQ.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EgyptPyramidHandle : MonoBehaviour
{
    [SerializeField] private Renderer m_PyramidRenderer;

    private void Awake()
    {
        GameEventHandler.AddActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
    }

    private void OnDestroy()
    {
        GameEventHandler.RemoveActionEvent(PBPvPEventCode.OnMatchStarted, OnMatchStarted);
    }

    private void Start()
    {
        if (m_PyramidRenderer != null)
            m_PyramidRenderer.enabled = false;
    }

    private void OnMatchStarted()
    {
        if(m_PyramidRenderer != null)
            m_PyramidRenderer.enabled = true;
    }
}
